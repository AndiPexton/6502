using Abstractions;
using Dependency;
using Runtime.Internal;
using Shadow.Quack;

namespace Runtime.OpCodeProcessors;

internal static class ShiftAndRollOpCodeProcessors
{
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();

    public static I6502_Sate Process_ROL(this I6502_Sate processorState, ushort? address)
    {
        var value = address == null ? processorState.A : Address.Read(address.Value, 1)[0];

        value = (byte)((byte)(value << 1) | (byte)(value >> 7));
        var carry = (value & 0b0000001) == 1;
        value = (byte)(processorState.C ? value | 1 : value & 0b11111110);
        if(address!=null) Address.WriteAt(address.Value, value);

        processorState = processorState.MergeWith(new
        {
            Z = value == 0,
            C = carry,
            N = value.IsNegative()
        });

        return address == null
            ? processorState.MergeWith(new
            {
                A = value
            })
            : processorState;
    }

    public static I6502_Sate Process_ROR(this I6502_Sate processorState, ushort? address)
    {
        var value = address == null ? processorState.A : Address.Read(address.Value, 1)[0];

        value = (byte)((byte)(value >> 1) | (byte)(value << 7));

        var carry = (value & 0b10000000) == 0b10000000;
        value = (byte)(processorState.C ? value | 0b10000000 : value & 0b01111111);

        if (address != null) Address.WriteAt(address.Value, value);

        processorState = processorState.MergeWith(new
        {
            Z = value == 0,
            C = carry,
            N = value.IsNegative()
        });

        return address == null
            ? processorState.MergeWith(new
            {
                A = value
            })
            : processorState;
    }

    public static I6502_Sate Process_LSR(this I6502_Sate processorState, ushort? address)
    {
        var value = address == null ? processorState.A : Address.Read(address.Value, 1)[0];

        var carry = (value & 1) == 1;

        value = (byte)(value >> 1);

        if (address != null) Address.WriteAt(address.Value, value);

        processorState = processorState.MergeWith(new
        {
            Z = value == 0,
            C = carry,
            N = value.IsNegative()
        });

        return address == null
            ? processorState.SetAccToNewValue(value)
            : processorState;
    }

    public static I6502_Sate Process_ASL(this I6502_Sate processorState, ushort? address) =>
        address == null
            ? processorState.Process_ASL_A() 
            : processorState.Process_ASL_M(address.Value);

    private static I6502_Sate Process_ASL_M(this I6502_Sate processorState, ushort address)
    {
        var currentValue = Address.Read(address, 1)[0];
        var newValue = (byte)(currentValue << 1);
        Address.WriteAt(address,newValue);
        return processorState
            .WithFlags(newValue, currentValue);
    }

    private static I6502_Sate Process_ASL_A(this I6502_Sate processorState)
    {
        var currentValue = processorState.A;
        var newValue = (byte)(currentValue << 1);
        return processorState
            .SetAccToNewValue(newValue)
            .WithFlags(newValue, currentValue);
    }

    private static I6502_Sate SetAccToNewValue(this I6502_Sate processorState, byte newValue) =>
        processorState
            .MergeWith(new { A = newValue });

    private static I6502_Sate WithFlags(this I6502_Sate processorState, byte newValue, byte currentValue) =>
        processorState.MergeWith(new
        {
            Z = newValue == 0,
            N = newValue.IsNegative(),
            C = (currentValue & 0x80) == 0x80
        });
}