using System.Reflection.Metadata.Ecma335;
using Abstractions;
using Dependency;
using Runtime.Internal;
using Runtime.OpCodeProcessors;
using Shadow.Quack;
using static Shadow.Quack.Duck;

namespace Runtime;

public static class _6502cpu
{
    public const int EndOfAddress = 0xFFFF;
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();
    private static ILogger Logger => Shelf.RetrieveInstance<ILogger>();

    public static I6502_Sate Empty6502ProcessorState() =>
        Implement<I6502_Sate>(new { S = 0xFF })
            .WriteStateRegister(0x0);

    public static I6502_Sate RunCycle(this I6502_Sate processorState)
    {
        if (processorState.ProgramCounter == 0) return processorState.ResetProgramCounter();

        var opCode = ReadOpCodeFromCurrentProgramPosition(processorState);

        Logger?.LogInstruction(opCode, processorState.ProgramCounter);

        return processorState
            .IncrementProgramCounter()
            .ProcessOpCode(opCode);
    }

    private static I6502_Sate ResetProgramCounter(this I6502_Sate processorState) =>
        processorState.MergeWith(new
        {
            ProgramCounter = Address?.GetResetVector() ?? EndOfAddress
        });

    private static OpCode ReadOpCodeFromCurrentProgramPosition(I6502_Sate processorState)
    {
        var rawCode = Address
                        ?.Read(processorState.ProgramCounter, 1)[0]
                    ?? (int)OpCode.KIL;
        return CastToEnum<OpCode>(rawCode);
    }

    private static I6502_Sate ProcessOpCode(this I6502_Sate processorState, OpCode opCode)
    {
        var opCodeAndAddressMode = ToOpCodeAndAddressModeElement(opCode);
        (var address, processorState) = processorState.ResolveAddress(opCodeAndAddressMode);

        Logger?.LogAddress(address);

        return processorState.ProcessOperation(opCodeAndAddressMode.GetOpCodeName(), address);
    }

    private static (ushort?, I6502_Sate) ResolveAddress(this I6502_Sate processorState, IReadOnlyCollection<string> opCodeAndAddressMode) =>
        opCodeAndAddressMode.HasAddressMode() 
            ? processorState.ReadAddressAndIncrementProgramCounter(opCodeAndAddressMode.ToAddressMode())
            : (null, processorState);

    private static I6502_Sate ProcessOperation(this I6502_Sate processorState, string opCodeName, ushort? address) =>
        opCodeName switch
        {
            "ADC" => processorState.Process_ADC(address ?? 0),
            "AND" => processorState.Process_AND(address ?? 0),
            "ASL" => processorState.Process_ASL(address),
            "BCC" => processorState.Process_BCC(address ?? 0),
            "BCS" => processorState.Process_BCS(address ?? 0),
            "BEQ" => processorState.Process_BEQ(address ?? 0),
            "BIT" => processorState.Process_BIT(address ?? 0),
            "BMI" => processorState.Process_BMI(address ?? 0),
            "BNE" => processorState.Process_BNE(address ?? 0),
            "BPL" => processorState.Process_BPL(address ?? 0),
            "BRK" => processorState.Process_BRK(),
            "BVC" => processorState.Process_BVC(address ?? 0),
            "BVS" => processorState.Process_BVS(address ?? 0),
            "CLC" => processorState.Process_CLC(),
            "CLD" => processorState.Process_CLD(),
            "CLI" => processorState.Process_CLI(),
            "CLV" => processorState.Process_CLV(),
            "CMP" => processorState.Process_CMP(address ?? 0),
            "CPX" => processorState.Process_CPX(address ?? 0),
            "CPY" => processorState.Process_CPY(address ?? 0),
            "DEC" => processorState.Process_DEC(address ?? 0),
            "DEX" => processorState.Process_DEX(),
            "DEY" => processorState.Process_DEY(),
            "EOR" => processorState.Process_EOR(address ?? 0),
            "INC" => processorState.Process_INC(address ?? 0),
            "INX" => processorState.Process_INX(),
            "INY" => processorState.Process_INY(),
            "JMP" => processorState.Process_JMP(address ?? 0),
            "JSR" => processorState.Process_JSR(address ?? 0),
            "KIL" => throw new ProcessorKillException(),
            "LDA" => processorState.Process_LDA(address ?? 0),
            "LDX" => processorState.Process_LDX(address ?? 0),
            "LDY" => processorState.Process_LDY(address ?? 0),
            "LSR" => processorState.Process_LSR(address),
            "NOP" => processorState,
            "ORA" => processorState.Process_ORA(address ?? 0),
            "PHA" => processorState.Process_PHA(),
            "PHP" => processorState.Process_PHP(),
            "PLA" => processorState.Process_PLA(),
            "PLP" => processorState.Process_PLP(),
            "ROL" => processorState.Process_ROL(address),
            "ROR" => processorState.Process_ROR(address),
            "RTI" => processorState.Process_RTI(),
            "RTS" => processorState.Process_RTS(),
            "SBC" => processorState.Process_SBC(address ?? 0),
            "SEC" => processorState.Process_SEC(),
            "SED" => processorState.Process_SED(),
            "SEI" => processorState.Process_SEI(),
            "STA" => processorState.Process_STA(address ?? 0),
            "STX" => processorState.Process_STX(address ?? 0),
            "STY" => processorState.Process_STY(address ?? 0),
            "TAX" => processorState.Process_TAX(),
            "TAY" => processorState.Process_TAY(),
            "TSX" => processorState.Process_TSX(),
            "TXA" => processorState.Process_TXA(),
            "TXS" => processorState.Process_TXS(),
            "TYA" => processorState.Process_TYA(),
            _ => processorState
        };

    private static string GetOpCodeName(this string[] opCodeAndAddressMode) => 
        opCodeAndAddressMode[0].ToUpper();

    private static bool HasAddressMode(this IReadOnlyCollection<string> opCodeAndAddressMode) =>
        opCodeAndAddressMode.Count > 1;

    private static string[] ToOpCodeAndAddressModeElement(this OpCode opCode) =>
        opCode
            .ToString()
            .Split('_');

    private static string[] ToAddressMode(this IEnumerable<string> opCodeAndAddressMode) =>
        opCodeAndAddressMode
            .Skip(1)
            .ToArray();
}