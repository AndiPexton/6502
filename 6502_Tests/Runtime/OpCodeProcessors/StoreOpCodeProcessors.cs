using Abstractions;
using Dependency;

namespace Runtime.OpCodeProcessors;

internal static class StoreOpCodeProcessors
{
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();

    public static I6502_Sate Process_STY(this I6502_Sate processorState, ushort address)
    {
        Address.WriteAt(address, new[] { processorState.Y });
        return processorState;
    }

    public static I6502_Sate Process_STX(this I6502_Sate processorState, ushort address)
    {
        Address.WriteAt(address, new[] { processorState.X });
        return processorState;
    }

    public static I6502_Sate Process_STA(this I6502_Sate processorState, ushort address)
    {
        Address.WriteAt(address, new[] { processorState.A });
        return processorState;
    }
}