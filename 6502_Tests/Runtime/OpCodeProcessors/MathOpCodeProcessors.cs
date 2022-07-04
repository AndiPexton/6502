using Abstractions;
using Dependency;
using Runtime.Internal;
using Shadow.Quack;

namespace Runtime.OpCodeProcessors;

internal static class MathOpCodeProcessors
{
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();

    public static I6502_Sate Process_ADC(this I6502_Sate processorState, ushort address) =>
        processorState
            .ProcessAdc(
                address.ReadByte());

    public static I6502_Sate Process_SBC(this I6502_Sate processorState, ushort address) =>
        processorState
            .ProcessAdc(
                address.ReadAndInvertByte());

    private static I6502_Sate ProcessAdc(this I6502_Sate processorState, byte value) =>
        processorState
            .SetAccAndFlags(
                value,
                processorState.GetAddToAccWithCarryResult(value));

    private static I6502_Sate SetAccAndFlags(this I6502_Sate processorState, byte value, int result) =>
        processorState.MergeWith(new
        {
            A = (byte)result,
            C = result > byte.MaxValue,
            V = RegisterFunctions.IsOverflow(value, processorState.A, (byte)result),
            Z = (byte)result == 0,
            N = ((byte)result).IsNegative()
        });

    private static int GetAddToAccWithCarryResult(this I6502_Sate processorState, byte value) => 
        processorState.A + value + processorState.ReadCarryFlag();

    private static byte ReadAndInvertByte(this ushort address) => 
        (byte)~address.ReadByte();

    private static byte ReadByte(this ushort address) => 
        Address.Read(address, 1)[0];
}