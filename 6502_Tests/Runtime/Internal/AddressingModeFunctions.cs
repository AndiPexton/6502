using Abstractions;
using Dependency;

namespace Runtime.Internal;

internal static class AddressingModeFunctions
{

    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();
    public static (ushort,I6502_Sate) ReadAddressAndIncrementProgramCounter(this I6502_Sate processorState, string[] addressMode) =>
        addressMode[0].ToLower() switch
        {
            "immediate" => ResolveImmediateAddressing(processorState),
            "absolute" => ResolveAbsoluteAddressing(processorState, addressMode),
            "zeropage" => ResolveZeroPageAddressing(processorState, addressMode),
            "indirect" => ResolveIndirectAddressing(processorState, addressMode),
            "relative" => ResolveRelativeAddressing(processorState),
            _ => (0, processorState)
        };

  
    private static (ushort, I6502_Sate) ResolveRelativeAddressing(I6502_Sate processorState)
    {
        var b = Address.Read(processorState.ProgramCounter, 1)[0];
        var offset = unchecked((sbyte)b);
        return (
            (ushort)(processorState.ProgramCounter + 1 + offset),
            processorState.IncrementProgramCounter());
    }

    private static (ushort, I6502_Sate) ResolveIndirectAddressing(I6502_Sate processorState, string[] addressMode)
    {
        return addressMode.Length switch
        {
            1 => (ReadIndirectAddress(processorState), processorState.IncrementProgramCounter(2)),
            _ => ResolveIndexedAddressing(processorState, addressMode)
        };
    }

    private static (ushort, I6502_Sate) ResolveIndexedAddressing(I6502_Sate processorState, string[] addressMode)
    {
        var index = Address.Read(processorState.ProgramCounter, 1)[0];
        ushort address;
        switch (addressMode[1])
        {
            case "Y":
            {
                var pointer = GetZeroPageAddress(index);
                address = (ushort)(ReadWord(pointer) + processorState.Y);
                break;
            }
            default: // "X"
            {
                var pointer = GetZeroPageAddress((byte)(index + processorState.X));
                address = ReadWord(pointer);
                break;
            }
        }

        return (address, processorState.IncrementProgramCounter());
    }

    private static ushort ReadWord(ushort pointer)
    {
        return BitConverter.ToUInt16(Address.Read(pointer, 2));
    }

    private static ushort GetZeroPageAddress(byte index)
    {
        return BitConverter.ToUInt16(new byte[]
            {  index, (byte)0x00});
    }

    private static (ushort, I6502_Sate) ResolveZeroPageAddressing(I6502_Sate processorState, string[] addressMode) =>
    (
        processorState
            .GetZeroPageAddressAndApplyOffset(addressMode)
            .ToFullAddress()
        , 
        processorState
            .IncrementProgramCounter());

    private static ushort ToFullAddress(this byte zeroPage) => 
        new byte[] { zeroPage, 0x00 }.ToUshort();

    private static byte GetZeroPageAddressAndApplyOffset(this I6502_Sate processorState, string[] addressMode) =>
        processorState
            .ReadZeroPageAddress()
            .ProcessIndex(processorState, addressMode);

    private static (ushort, I6502_Sate) ResolveImmediateAddressing(I6502_Sate processorState) => 
    (
        processorState.ProgramCounter
        , 
        processorState.IncrementProgramCounter());

    private static (ushort, I6502_Sate) ResolveAbsoluteAddressing(I6502_Sate processorState, string[] addressMode) =>
    (
        processorState
            .ReadAbsoluteAddress()
            .ProcessIndex(processorState, addressMode)
        , 
        processorState
            .IncrementProgramCounter(2));

    private static ushort ProcessIndex(this ushort address, I6502_Sate processorState, string[] addressMode)
    {
        if (addressMode.Length <= 1) return address;

        return addressMode[1] switch
        {
            "X" => (ushort)(address + processorState.X),
            "Y" => (ushort)(address + processorState.Y),
            _ => address
        };
    }

    private static byte ProcessIndex(this byte address, I6502_Sate processorState, string[] addressMode)
    {
        if (addressMode.Length <= 1) return address;

        return addressMode[1] switch
        {
            "X" => (byte)(address + processorState.X),
            "Y" => (byte)(address + processorState.Y),
            _ => address
        };
    }

    private static ushort ReadAbsoluteAddress(this I6502_Sate processorState) =>
        processorState.ProgramCounter
            .ReadBytesFromAddress(2)
            .ToUshort();

    private static byte[] ReadBytesFromAddress(this ushort address, int bytes) =>
        Address
            ?.Read(address, bytes) 
        ?? Array.Empty<byte>();

    private static ushort ReadIndirectAddress(I6502_Sate processorState) =>
        processorState.ProgramCounter
            .ReadBytesFromAddress(2)
            .ToUshort()
            .ReadBytesFromAddress(2)
            .ToUshort();

    private static ushort ToUshort(this byte[] addressBytes) => 
        BitConverter.ToUInt16(addressBytes);

    private static byte ReadZeroPageAddress(this I6502_Sate processorState) =>
        processorState.ProgramCounter
            .ReadBytesFromAddress(1)[0];
}