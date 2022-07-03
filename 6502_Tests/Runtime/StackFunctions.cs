using Abstractions;
using Dependency;
using Shadow.Quack;

namespace Runtime;

public static class StackFunctions
{
    private static ILogger Logger => Shelf.RetrieveInstance<ILogger>();

    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();
    public static (I6502_Sate, byte) PullFromStack(this I6502_Sate processorState)
    {
        processorState = processorState.MergeWith(new { S = (byte)(processorState.S + 1) });
        var pulledValue = Address.Read(processorState.GetCurrentStackAddress(), 1)[0];
        Logger?.LogStackPull(pulledValue);
        return (processorState, pulledValue);
    }


    public static I6502_Sate PushToStack(this I6502_Sate processorState, ushort value) =>
        BitConverter.GetBytes(value).Reverse()
            .Aggregate(processorState, (current, b) => current.PushToStack(b));

    public static I6502_Sate PushToStack(this I6502_Sate processorState, byte value)
    {
        Address.WriteAt(processorState.GetCurrentStackAddress(), value);
        Logger?.LogStackPush(value);
        return processorState.MergeWith(new { S = (byte)(processorState.S - 1) });
    }

    private static ushort GetCurrentStackAddress(this I6502_Sate processorState)
    {
        return BitConverter.ToUInt16(new byte []{ processorState.S, 0x01 });
    }
}