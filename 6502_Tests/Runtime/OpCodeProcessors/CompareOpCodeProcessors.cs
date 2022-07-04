using Abstractions;
using Dependency;
using Runtime.Internal;
using Shadow.Quack;

namespace Runtime.OpCodeProcessors;

internal static class CompareOpCodeProcessors
{
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();

    public static I6502_Sate Process_CPX(this I6502_Sate processorState, ushort address) => processorState.Compare(Address.Read(address, 1)[0], processorState.X);
    public static I6502_Sate Process_CPY(this I6502_Sate processorState, ushort address) => processorState.Compare(Address.Read(address, 1)[0], processorState.Y);
    public static I6502_Sate Process_CMP(this I6502_Sate processorState, ushort address) => processorState.Compare(Address.Read(address, 1)[0], processorState.A);

    public static I6502_Sate Process_BIT(this I6502_Sate processorState, ushort address)
    {
        var b = Address.Read(address, 1)[0];
        var result = (byte)(b & processorState.A);
       
        return processorState.MergeWith(new
        {
            Z = result == 0,
            N = b.IsNegative(),
            V = b.OverflowSet()
        });
    }

    private static I6502_Sate Compare(this I6502_Sate processorState, byte value, byte register)
    {
        value = (byte)(value ^ 0xff);
        var carry = register + value + 1;

        var b = (byte)carry;
        return processorState.MergeWith(new
        {
            C = carry > 0xff,
            Z = b == 0,
            N = ((byte)(b & 0xff)).IsNegative()
        });
    }
}