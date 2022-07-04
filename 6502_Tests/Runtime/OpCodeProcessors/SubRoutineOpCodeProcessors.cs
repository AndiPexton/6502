using Abstractions;
using Dependency;
using Runtime.Internal;
using Runtime.OpCodeProcessors;
using Shadow.Quack;

namespace Runtime.OpCodeProcessors;

internal static class SubRoutineOpCodeProcessors
{
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();
    private static ILogger Logger => Shelf.RetrieveInstance<ILogger>();

    public static I6502_Sate Process_RTS(this I6502_Sate processorState)
    {
        Logger?.LogStackPointer(processorState.S);

        (processorState, var lowByte) = processorState.PullFromStack();
        (processorState, var highByte) = processorState.PullFromStack();
        
        var address = BitConverter.ToUInt16(new[] { lowByte, highByte });
        return processorState
            .Process_JMP((ushort)(address+1));
    }

    public static I6502_Sate Process_JSR(this I6502_Sate processorState, ushort address) =>
        processorState
            .PushToStack((ushort)(processorState.ProgramCounter-1))
            .Process_JMP(address);

    public static I6502_Sate Process_RTI(this I6502_Sate processorState)
    {
        (processorState, var sr) = processorState.PullFromStack();
        (processorState, var h) = processorState.PullFromStack();
        (processorState, var l) = processorState.PullFromStack();
       
        var returnAddress = BitConverter.ToUInt16(new byte[] { h, l });
        var processRti = processorState.MergeWith(new
        {
            ProgramCounter = returnAddress
        }).WriteStateRegister(sr);
        return processRti;
    }

    public static I6502_Sate Process_BRK(this I6502_Sate processorState)
    {
        var returnAddress = BitConverter.GetBytes((ushort)(processorState.ProgramCounter + 1));
        return processorState
            .PushToStack(returnAddress[1])
            .PushToStack(returnAddress[0])
            .PushToStack(processorState.ReadStateRegister())
            .MergeWith(new
            {
                ProgramCounter = Address.GetIRQVector(),
                I = true
            });
    }
}