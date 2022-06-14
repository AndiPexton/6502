using System.Reflection.Metadata.Ecma335;
using Abstractions;
using Dependency;
using Shadow.Quack;

namespace Runtime;

public static class CpuFunctions
{
    public const int EndOfAddress = 0xFFFF;
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();

    public static I6502_Sate RunCycle(this I6502_Sate processorState)
    {
        if (processorState.ProgramCounter == 0)
            return processorState.MergeWith(new
            {
                ProgramCounter = Address?.GetResetVector() ?? EndOfAddress
            });
            
        var opCode = ReadOpCodeFromCurrentProgramPosition(processorState);

        return processorState.IncrementProgramCounter().ProcessOpCode(opCode);
    }

    private static OpCode ReadOpCodeFromCurrentProgramPosition(I6502_Sate processorState) =>
        Duck.CastToEnum<OpCode>(
            Address
                ?.Read(processorState.ProgramCounter, 1)[0] 
            ?? (int)OpCode.KIL
        );

    private static I6502_Sate ProcessOpCode(this I6502_Sate processorState, OpCode opCode)
    {
        var opCodeAndAddressMode = opCode.ToString().Split('_');
        ushort? address = null;
        if (opCodeAndAddressMode.Length > 1)
            (address, processorState) = AddressingModeFunctions.ReadAddressAndIncrementProgramCounter(processorState, opCodeAndAddressMode.Skip(1).ToArray());

        switch (opCodeAndAddressMode[0])
        {
            case "LDY":
                return processorState.Process_LDY(address ?? 0);
            case "LDA":
                return processorState.Process_LDA(address ?? 0);
            case "LDX":
                return processorState.Process_LDX(address ?? 0);
            case "STA":
                return processorState.Process_STA(address ?? 0);
            case "STX":
                return processorState.Process_STX(address ?? 0);
            case "STY":
                return processorState.Process_STY(address ?? 0);
            case "ADC":
                return Process_ADC(processorState, address ?? 0);
            case "SBC":
                return Process_SBC(processorState, address ?? 0);
            case "CLC":
                return processorState.Process_CLC();
            case "KIL":
                throw new ProcessorKillException();
            case "PHA":
                return processorState.Process_PHA();
            case "PLA":
                return processorState.Process_PLA();
            case "PHP":
                return processorState.Process_PHP();
            case "PLP":
                return processorState.Process_PLP();
            case "AND":
                return Process_AND(processorState, address ?? 0);
            case "ASL":
                return Process_ASL(processorState, address);
            case "BCC":
                return processorState.Process_BCC(address ?? 0);
            case "BCS":
                return processorState.Process_BCS(address ?? 0);
            case "BEQ":
                return processorState.Process_BEQ(address ?? 0);
            case "BNE":
                return processorState.Process_BNE(address ?? 0);
            case "BMI":
                return processorState.Process_BMI(address ?? 0);
            case "BPL":
                return processorState.Process_BPL(address ?? 0);
            case "BRK":
                return Process_BRK(processorState);
            case "RTI":
                return Process_RTI(processorState);
            case "TAX":
                return processorState.Process_TAX();
            case "TAY":
                return processorState.Process_TAY();
            case "TSX":
                return processorState.Process_TSX();
            case "TXA":
                return processorState.Process_TXA();
            case "TXS":
                return processorState.Process_TXS();
            case "TYA":
                return processorState.Process_TYA();
            case "BIT":
                return Process_BIT(processorState, address ?? 0);
            case "BVC":
                return processorState.Process_BVC(address ?? 0);
            case "BVS":
                return processorState.Process_BVS(address ?? 0);
            case "SEC":
                return processorState.Process_SEC();
            case "SED":
                return processorState.Process_SED();
            case "SEI":
                return processorState.Process_SEI();
            case "CLD":
                return processorState.Process_CLD();
            case "CLI":
                return processorState.Process_CLI();
            case "CLV":
                return processorState.Process_CLV();
            case "JMP":
                return processorState.Process_JMP(address ?? 0);
            case "JSR":
                return  Process_JSR(processorState, address ?? 0);
            case "RTS":
                return Process_RTS(processorState);
            case "ROL":
                return Process_ROL(processorState, address);
            case "ROR":
                return Process_ROR(processorState, address);
            case "LSR":
                return Process_LSR(processorState, address);
            case "CMP":
                return Process_CMP(processorState, address ?? 0);

            case "CPX":
                return Process_CPX(processorState, address ?? 0);
            case "CPY":
                return Process_CPY(processorState, address ?? 0);

            case "DEC":
                return Process_DEC(processorState, address ?? 0);
            case "DEX":
                return Process_DEX(processorState);
            case "DEY":
                return Process_DEY(processorState);
            case "INC":
                return Process_INC(processorState, address ?? 0);
            case "INY":
                return Process_INY(processorState);
            case "INX":
                return Process_INX(processorState);
            case "EOR":
                return Process_EOR(processorState, address ?? 0);
            case "ORA":
                return Process_ORA(processorState, address ?? 0);
            case "NOP":
            default:
                return processorState;
        }
    }

    private static I6502_Sate Process_EOR(I6502_Sate processorState, ushort address)
    {
        var value = Address.Read(address, 1)[0];
        var result = (byte)(value ^ processorState.A);
        return processorState.MergeWith(new
        {
            A = result,
            Z = result == 0,
            N = result.IsNegative()
        });
    }

    private static I6502_Sate Process_ORA(I6502_Sate processorState, ushort address)
    {
        var value = Address.Read(address, 1)[0];
        var result = (byte)(value | processorState.A);
        return processorState.MergeWith(new
        {
            A = result,
            Z = result == 0,
            N = result.IsNegative()
        });
    }

    private static I6502_Sate Process_DEX(I6502_Sate processorState)
    {
        var value = (byte)(processorState.X - 1);
        return processorState.MergeWith(new
        {
            X = value,
            Z = value == 0,
            N = value.IsNegative()
        });
    }

    private static I6502_Sate Process_DEY(I6502_Sate processorState)
    {
        var value = (byte)(processorState.Y - 1);
        return processorState.MergeWith(new
        {
            Y = value,
            Z = value == 0,
            N = value.IsNegative()
        });
    }

    private static I6502_Sate Process_INX(I6502_Sate processorState)
    {
        var value = (byte)(processorState.X + 1);
        return processorState.MergeWith(new
        {
            X = value,
            Z = value == 0,
            N = value.IsNegative()
        });
    }

    private static I6502_Sate Process_INY(I6502_Sate processorState)
    {
        var value = (byte)(processorState.Y + 1);
        return processorState.MergeWith(new
        {
            Y = value,
            Z = value == 0,
            N = value.IsNegative()
        });
    }

    private static I6502_Sate Process_DEC(I6502_Sate processorState, ushort address)
    {
        var value = Address.Read(address, 1)[0];
        value = (byte)(value - 1);
        Address.WriteAt(address, value);
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = value.IsNegative()
        });
    }

    private static I6502_Sate Process_INC(I6502_Sate processorState, ushort address)
    {
        var value = Address.Read(address, 1)[0];
        value = (byte)(value + 1);
        Address.WriteAt(address, value);
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = value.IsNegative()
        });
    }

    private static I6502_Sate Process_CPX(I6502_Sate processorState, ushort address) =>
        Compare(processorState, Address.Read(address, 1)[0], processorState.X);

    private static I6502_Sate Process_CPY(I6502_Sate processorState, ushort address) =>
        Compare(processorState, Address.Read(address, 1)[0], processorState.Y);

    private static I6502_Sate Process_CMP(I6502_Sate processorState, ushort address) => 
        Compare(processorState, Address.Read(address, 1)[0], processorState.A);

    private static I6502_Sate Compare(I6502_Sate processorState, byte value, byte register) =>
        processorState.MergeWith(new
        {
            C = register >= value,
            Z = value == register,
            N = ((byte)(register - value)).IsNegative()
        });

    private static I6502_Sate Process_ROL(I6502_Sate processorState, ushort? address)
    {
        var value = address == null ? processorState.A : Address.Read(address.Value, 1)[0];

        value = (byte)((byte)(value << 1) | (byte)(value >> 7));

        if(address!=null) Address.WriteAt(address.Value, value);

        return address == null
            ? processorState.MergeWith(new
            {
                A = value
            })
            : processorState;
    }

    private static I6502_Sate Process_ROR(I6502_Sate processorState, ushort? address)
    {
        var value = address == null ? processorState.A : Address.Read(address.Value, 1)[0];

        value = (byte)((byte)(value >> 1) | (byte)(value << 7));

        if (address != null) Address.WriteAt(address.Value, value);

        return address == null
            ? processorState.MergeWith(new
            {
                A = value
            })
            : processorState;
    }

    private static I6502_Sate Process_LSR(I6502_Sate processorState, ushort? address)
    {
        var value = address == null ? processorState.A : Address.Read(address.Value, 1)[0];

        value = (byte)(value >> 1);

        if (address != null) Address.WriteAt(address.Value, value);

        return address == null
            ? processorState.MergeWith(new
            {
                A = value
            })
            : processorState;
    }

    private static I6502_Sate Process_RTS(I6502_Sate processorState)
    {
        (processorState, var highByte) = processorState.PullFromStack();
        (processorState, var lowByte) = processorState.PullFromStack();
        var address = BitConverter.ToUInt16(new[] { lowByte, highByte });
        return processorState
            .Process_JMP(address);
    }

    private static I6502_Sate Process_JSR(I6502_Sate processorState, ushort address) =>
        processorState
            .PushToStack(processorState.ProgramCounter)
            .Process_JMP(address);


    private static I6502_Sate Process_BIT(I6502_Sate processorState, ushort address)
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

    private static I6502_Sate Process_RTI(I6502_Sate processorState)
    {
        (processorState, var sr) = processorState.PullFromStack();
        (processorState, var h) = processorState.PullFromStack();
        (processorState, var l) = processorState.PullFromStack();
        var returnAddress = BitConverter.ToUInt16(new byte[] { l, h });
        return processorState.MergeWith(new
        {
            ProgramCounter = returnAddress
        });
    }

    private static I6502_Sate Process_BRK(I6502_Sate processorState)
    {
        var returnAddress = BitConverter.GetBytes((ushort)(processorState.ProgramCounter + 1));
        return processorState
            .PushToStack(returnAddress[0])
            .PushToStack(returnAddress[1])
            .PushToStack(processorState.ReadStateRegister())
            .MergeWith(new
                {
                    ProgramCounter = Address.GetIRQVector()
                });
    }


    private static I6502_Sate Process_ASL(I6502_Sate processorState, ushort? address)
    {
        return address == null? Process_ASL_A(processorState) : Process_ASL_M(processorState, address.Value);
    }

    private static I6502_Sate Process_ASL_M(I6502_Sate processorState, ushort address)
    {
        var b = Address.Read(address, 1)[0];
        var value = (byte)(b << 1);
        Address.WriteAt(address,value);
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = value.IsNegative(),
            C = (b & 0x80) == 0x80
        });
    }

    private static I6502_Sate Process_ASL_A(I6502_Sate processorState)
    {
        var value = (byte)(processorState.A << 1);
        return processorState.MergeWith(new
        {
            A = value,
            Z = value == 0,
            N = value.IsNegative(),
            C = (processorState.A & 0x80) == 0x80
        });
    }

    private static I6502_Sate Process_AND(I6502_Sate processorState, ushort address)
    {
        var b = Address.Read(address, 1)[0];
        var value = (byte)(b & processorState.A);
        return processorState.MergeWith(new
        {
            A = value,
            Z = value == 0,
            N = value.IsNegative()
        });
    }


    private static I6502_Sate Process_ADC(I6502_Sate processorState, ushort address)
    {
        var value1 = Address.Read(address, 1)[0];
        return ProcessAdc(processorState, value1);
    }

    private static I6502_Sate ProcessAdc(I6502_Sate processorState, byte value1)
    {
        var result = processorState.A + value1 + processorState.ReadCarryFlag();

        return processorState.MergeWith(new
        {
            A = (byte)result,
            C = result > byte.MaxValue,
            V = RegisterFunctions.IsOverflow(value1, processorState.A, (byte)result),
            Z = result == 0,
            N = ((byte)result).IsNegative()
        });
    }

    private static I6502_Sate Process_SBC(I6502_Sate processorState, ushort address)
    {
        var value1 = (byte)~Address.Read(address, 1)[0];
        return ProcessAdc(processorState, value1);
    }

    public static I6502_Sate Empty6502ProcessorState() =>
        Duck.Implement<I6502_Sate>(new { S = 0xFF });
}