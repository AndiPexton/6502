using Abstractions;
using Runtime.Internal;
using Shadow.Quack;

namespace Runtime.OpCodeProcessors;

internal static class StackOpCodeProcessors
{
    public static I6502_Sate Process_PHP(this I6502_Sate processorState)
    {
        return processorState.PushToStack(processorState.ReadStateRegister(true));
    }

    public static I6502_Sate Process_PLP(this I6502_Sate processorState)
    {
        (processorState, var sr) = processorState.PullFromStack();
        return processorState.WriteStateRegister(sr);
    }

    public static I6502_Sate Process_PLA(this I6502_Sate processorState)
    {
        (processorState, var a) = processorState.PullFromStack();
        return processorState.MergeWith(new
        {
            A = a,
            Z = a == 0,
            N = a.IsNegative()
        });
    }

    public static I6502_Sate Process_PHA(this I6502_Sate processorState)
    {
        return processorState.PushToStack(processorState.A);
    }
}