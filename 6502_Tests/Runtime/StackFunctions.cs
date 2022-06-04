using Abstractions;
using Dependency;
using Shadow.Quack;

namespace Runtime;

public static class StackFunctions
{
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();
    public static (I6502_Sate, byte) PullFromStack(this I6502_Sate processorState)
    {
        if (processorState.S == 0xFF) return (processorState, (byte)0x00);

        processorState = processorState.MergeWith(new { S = processorState.S + 1 });
        return (processorState, Address.Read(processorState.GetCurrentStackAddress(), 1)[0]);
    }

    public static I6502_Sate PushToStack(this I6502_Sate processorState, byte value)
    {
        if (processorState.S == 0x00) throw new StackOverflowException();
        Address.WriteAt(processorState.GetCurrentStackAddress(), value);
        return processorState.MergeWith(new { S = processorState.S - 1 });
    }

    private static ushort GetCurrentStackAddress(this I6502_Sate processorState)
    {
        return BitConverter.ToUInt16(new byte []{ 0x01, processorState.S });
    }
}