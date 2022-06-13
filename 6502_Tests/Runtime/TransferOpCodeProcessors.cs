using Abstractions;
using Shadow.Quack;

namespace Runtime;

public static class TransferOpCodeProcessors
{
    public static I6502_Sate Process_TYA(this I6502_Sate processorState)
    {
        var value = processorState.Y;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = value.IsNegative(),
            A = value
        });
    }

    public static I6502_Sate Process_TXS(this I6502_Sate processorState)
    {
        var value = processorState.X;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = value.IsNegative(),
            S = value
        });
    }

    public static I6502_Sate Process_TXA(this I6502_Sate processorState)
    {
        var value = processorState.X;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = value.IsNegative(),
            A = value
        });
    }

    public static I6502_Sate Process_TSX(this I6502_Sate processorState)
    {
        var value = processorState.S;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = value.IsNegative(),
            X = value
        });
    }

    public static I6502_Sate Process_TAX(this I6502_Sate processorState)
    {
        var value = processorState.A;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = value.IsNegative(),
            X = value
        });
    }

    public static I6502_Sate Process_TAY(this I6502_Sate processorState)
    {
        var value = processorState.A;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = value.IsNegative(),
            Y = value
        });
    }
}