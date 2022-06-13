using Abstractions;
using Shadow.Quack;

namespace Runtime;

public static class RegisterFunctions
{
    public static I6502_Sate IncrementProgramCounter(this I6502_Sate processorState, int bytes = 1) =>
        processorState
            .MergeWith(
                new
                {
                    ProgramCounter = (ushort)(processorState.ProgramCounter + bytes)
                });

    public static bool IsNegative(this byte value1)
    {
        return (value1 & 128) == 128;
    }

    public static bool OverflowSet(byte value1)
    {
        return (value1 & 64) == 64;
    }

    public static byte ReadCarryFlag(this I6502_Sate processorState) => 
        processorState.C ? (byte)0x01 : (byte)0x00;

    public static byte ReadStateRegister(this I6502_Sate processorState, bool br = false)
    {
        byte sr = 0;
        sr = (byte)(processorState.C ? sr + (byte)0x01 : sr);
        sr = (byte)(processorState.Z ? sr + (byte)0x02 : sr);
        sr = (byte)(processorState.I ? sr + (byte)0x04 : sr);
        sr = (byte)(processorState.D ? sr + (byte)0x08 : sr);
        sr = (byte)(br ? sr + (byte)0x10 : sr);
        // bit 5 - 32 0x20 ignore
        sr = (byte)(processorState.V ? sr + (byte)0x40 : sr);
        sr = (byte)(processorState.N ? sr + (byte)0x80 : sr);
        return sr;
    }

    public static I6502_Sate WriteStateRegister(this I6502_Sate processorState, byte sr)
    {
        return processorState.MergeWith(new
        {
            C = (sr & 0x01) == 0x01,
            Z = (sr & 0x02) == 0x02,
            I = (sr & 0x04) == 0x04,
            D = (sr & 0x08) == 0x08,
            B = (sr & 0x10) == 0x10,
            V = (sr & 0x40) == 0x40,
            N = (sr & 0x80) == 0x80
        });
    }

    public static bool IsOverflow(byte value1, byte value2, byte result)
    {
        return (RegisterFunctions.IsNegative(value1) == RegisterFunctions.IsNegative(value2))
               & (RegisterFunctions.IsNegative(value1) != RegisterFunctions.IsNegative(result));
    }
}