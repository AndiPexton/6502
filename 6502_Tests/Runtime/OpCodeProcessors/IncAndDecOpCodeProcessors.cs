using Abstractions;
using Dependency;
using Runtime.Internal;
using Shadow.Quack;

namespace Runtime.OpCodeProcessors;

internal static class IncAndDecOpCodeProcessors
{
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();

    public static I6502_Sate Process_DEX(this I6502_Sate processorState)
    {
        var value = (byte)(processorState.X - 1);
        return processorState
            .SetX(value)
            .WithFlags(value);
    }

    public static I6502_Sate Process_DEY(this I6502_Sate processorState)
    {
        var value = (byte)(processorState.Y - 1);
        return processorState
            .SetY(value)
            .WithFlags(value);
    }

    public static I6502_Sate Process_INX(this I6502_Sate processorState)
    {
        var value = (byte)(processorState.X + 1);
        return processorState
            .SetX(value)
            .WithFlags(value);
    }

    public static I6502_Sate Process_INY(this I6502_Sate processorState)
    {
        var value = (byte)(processorState.Y + 1);
        return processorState
            .SetY(value)
            .WithFlags(value);
    }

    public static I6502_Sate Process_DEC(this I6502_Sate processorState, ushort address)
    {
        var value = Address.Read(address, 1)[0];
        value = (byte)(value - 1);
        Address.WriteAt(address, value);
        return processorState
            .WithFlags(value);
    }

    public static I6502_Sate Process_INC(this I6502_Sate processorState, ushort address)
    {
        var value = Address.Read(address, 1)[0];
        var result = (byte)(value + 1);
        Address.WriteAt(address, result);
        return processorState
            .WithFlags(result);
    }

    private static I6502_Sate WithFlags(this I6502_Sate state, byte value) =>
        state.MergeWith(new
        {
            Z = value == 0,
            N = value.IsNegative()
        });

    private static I6502_Sate SetX(this I6502_Sate processorState, byte value) =>
        processorState.MergeWith(new
        {
            X = value
        });

    private static I6502_Sate SetY(this I6502_Sate processorState, byte value) =>
        processorState.MergeWith(new
        {
            Y = value
        });
}