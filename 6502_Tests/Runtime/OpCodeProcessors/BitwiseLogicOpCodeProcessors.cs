using Abstractions;
using Dependency;
using Runtime.Internal;
using Shadow.Quack;

namespace Runtime.OpCodeProcessors;

internal static class BitwiseLogicOpCodeProcessors
{
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();

    public static I6502_Sate Process_EOR(this I6502_Sate processorState, ushort address)
    {
        var value = Address.Read(address, 1)[0];
        var result = (byte)(value ^ processorState.A);
        return processorState
            .WithFlags(result);
    }

    public static I6502_Sate Process_ORA(this I6502_Sate processorState, ushort address)
    {
        var value = Address.Read(address, 1)[0];
        var result = (byte)(value | processorState.A);
        return processorState
            .WithFlags(result);
    }

    public static I6502_Sate Process_AND(this I6502_Sate processorState, ushort address)
    {
        var b = Address.Read(address, 1)[0];
        var result = (byte)(b & processorState.A);
        return processorState
            .WithFlags(result); 
    }

    private static I6502_Sate WithFlags(this I6502_Sate processorState, byte result) =>
        processorState.MergeWith(new
        {
            A = result,
            Z = result == 0,
            N = result.IsNegative()
        });
}