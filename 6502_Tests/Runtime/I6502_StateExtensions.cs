using Abstractions;

namespace Runtime;

public static class I6502StateExtensions
{
    public static byte ReadStateRegister(this I6502_Sate processorState, bool br = true)
    {
        var sr = (byte)(processorState.C ? 0x01 : 00);
        sr = (byte)(processorState.Z ? sr + 0x02 : sr);
        sr = (byte)(processorState.I ? sr + 0x04 : sr);
        sr = (byte)(processorState.D ? sr + 0x08 : sr);
        sr = (byte)(br ? sr + (byte)0x10 : sr);
        // bit 5 - 32 0x20 ignore
        sr = (byte)(processorState.V ? sr + 0x40 : sr);
        sr = (byte)(processorState.N ? sr + 0x80 : sr);
        return (byte)(sr + 0x20);
    }
}