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
                return Process_LDY(processorState, address?? 0);
            case "LDA":
                return Process_LDA(processorState, address?? 0);
            case "LDX":
                return Process_LDX(processorState, address??0);
            case "STA":
                return Process_STA(processorState, address??0);
            case "ADC":
                return Process_ADC(processorState, address??0);
            case "CLC":
                return processorState.MergeWith(new { C = false });
            case "KIL":
                throw new ProcessorKillException();
            case "PHA":
                return Process_PHA(processorState);
            case "PLA":
                return Process_PLA(processorState);
            case "PHP":
                return Process_PHP(processorState);
            case "PLP":
                return Process_PLP(processorState);
            case "AND":
                return Process_AND(processorState, address??0);
            case "ASL":
                return Process_ASL(processorState, address);
            case "BCC":
                return Process_BCC(processorState, address??0);
            case "BCS":
                return Process_BCS(processorState, address ?? 0);
            case "BEQ":
                return Process_BEQ(processorState, address ?? 0);
            case "BNE":
                return Process_BNE(processorState, address ?? 0);
            case "BMI":
                return Process_BMI(processorState, address ?? 0);
            case "BPL":
                return Process_BPL(processorState, address ?? 0);
            case "BRK":
                return Process_BRK(processorState);
            case "RTI":
                return Process_RTI(processorState);
            case "TAX":
                return Process_TAX(processorState);
            case "TAY":
                return Process_TAY(processorState);
            case "TSX":
                return Process_TSX(processorState);
            case "TXA":
                return Process_TXA(processorState);
            case "TXS":
                return Process_TXS(processorState);
            case "TYA":
                return Process_TYA(processorState);
            case "NOP":
            default:
                return processorState;
        }
    }

    private static I6502_Sate Process_TYA(I6502_Sate processorState)
    {
        var value = processorState.Y;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = RegisterFunctions.IsNegative(value),
            A = value
        });
    }

    private static I6502_Sate Process_TXS(I6502_Sate processorState)
    {
        var value = processorState.X;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = RegisterFunctions.IsNegative(value),
            S = value
        });
    }

    private static I6502_Sate Process_TXA(I6502_Sate processorState)
    {
        var value = processorState.X;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = RegisterFunctions.IsNegative(value),
            A = value
        });
    }

    private static I6502_Sate Process_TSX(I6502_Sate processorState)
    {
        var value = processorState.S;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = RegisterFunctions.IsNegative(value),
            X = value
        });
    }

    private static I6502_Sate Process_TAX(I6502_Sate processorState)
    {
        var value = processorState.A;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = RegisterFunctions.IsNegative(value),
            X = value
           });
    }

    private static I6502_Sate Process_TAY(I6502_Sate processorState)
    {
        var value = processorState.A;
        return processorState.MergeWith(new
        {
            Z = value == 0,
            N = RegisterFunctions.IsNegative(value),
            Y = value
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

    private static I6502_Sate Process_BPL(I6502_Sate processorState, ushort address) =>
        !processorState.N
            ? JumpToAddress(processorState, address)
            : processorState;

    private static I6502_Sate Process_BMI(I6502_Sate processorState, ushort address) =>
        processorState.N
        ? JumpToAddress(processorState, address)
        : processorState;

    private static I6502_Sate Process_BCS(I6502_Sate processorState, ushort address) =>
        processorState.C
        ? JumpToAddress(processorState, address)
        : processorState;

    private static I6502_Sate Process_BNE(I6502_Sate processorState, ushort address) =>
        !processorState.Z
            ? JumpToAddress(processorState, address)
            : processorState;

    private static I6502_Sate Process_BCC(I6502_Sate processorState, ushort address) =>
        !processorState.C 
            ? JumpToAddress(processorState, address) 
            : processorState;

    private static I6502_Sate Process_BEQ(I6502_Sate processorState, ushort address) =>
        processorState.Z 
            ? JumpToAddress(processorState, address) 
            : processorState;

    private static I6502_Sate JumpToAddress(I6502_Sate processorState, ushort address) =>
        processorState.MergeWith(new
        {
            ProgramCounter = address
        });

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
            N = RegisterFunctions.IsNegative(value),
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
            N = RegisterFunctions.IsNegative(value),
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
            N = RegisterFunctions.IsNegative(value)
        });
    }

    private static I6502_Sate Process_PHP(I6502_Sate processorState)
    {
        return processorState.PushToStack(processorState.ReadStateRegister(true));
    }
    private static I6502_Sate Process_PLP(I6502_Sate processorState)
    {
        (processorState, var sr) = processorState.PullFromStack();
        return processorState.WriteStateRegister(sr);
    }


    private static I6502_Sate Process_PLA(I6502_Sate processorState)
    {
        (processorState, var a) = processorState.PullFromStack();
        return processorState.MergeWith(new
        {
            A = a,
            Z = a == 0,
            N = RegisterFunctions.IsNegative(a)
        });
    }

    private static I6502_Sate Process_PHA(I6502_Sate processorState)
    {
        return processorState.PushToStack(processorState.A);
    }

    private static I6502_Sate Process_LDY(I6502_Sate processorState, ushort address)
    {
        var b = Address.Read(address, 1)[0];
        return processorState.MergeWith(new 
            { 
                Y = b, 
                Z = b == 0,
                N = RegisterFunctions.IsNegative(b)
            });
    }

    private static I6502_Sate Process_LDX(I6502_Sate processorState, ushort address)
    {
        var b = Address.Read(address, 1)[0];
        return processorState.MergeWith(new
        {
            X = b,
            Z = b == 0,
            N = RegisterFunctions.IsNegative(b)
        });
    }

    private static I6502_Sate Process_LDA(I6502_Sate processorState, ushort address)
    {
        var b = Address.Read(address, 1)[0];
        return processorState.MergeWith(new
        {
            A = b,
            Z = b == 0,
            N = RegisterFunctions.IsNegative(b)
        });
    }

    private static I6502_Sate Process_STA(I6502_Sate processorState, ushort address)
    {
        Address.WriteAt(address, new[] { processorState.A });
        return processorState;
    }

    private static I6502_Sate Process_ADC(I6502_Sate processorState, ushort address)
    {
        var value1 = Address.Read(address, 1)[0];
        var result = processorState.A + value1 + processorState.ReadCarryFlag();

        return processorState.MergeWith(new
        {
            A = (byte)result,
            C = result > byte.MaxValue,
            V = RegisterFunctions.IsOverflow(value1, processorState.A, (byte)result),
            Z = result == 0,
            N = RegisterFunctions.IsNegative((byte)result)
        });
    }

    public static I6502_Sate Empty6502ProcessorState() =>
        Duck.Implement<I6502_Sate>(new { S = 0xFF });
}