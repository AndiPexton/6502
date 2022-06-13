using Abstractions;
using Shadow.Quack;

namespace Runtime;

public static class BranchOpCodeProcessors
{
    public static I6502_Sate Process_BPL(this I6502_Sate processorState, ushort address) =>
        !processorState.N
            ? JumpToAddress(processorState, address)
            : processorState;

    public static I6502_Sate Process_BVC(this I6502_Sate processorState, ushort address) =>
        !processorState.V
            ? JumpToAddress(processorState, address)
            : processorState;

    public static I6502_Sate Process_BVS(this I6502_Sate processorState, ushort address) =>
        processorState.V
            ? JumpToAddress(processorState, address)
            : processorState;

    public static I6502_Sate Process_BMI(this I6502_Sate processorState, ushort address) =>
        processorState.N
            ? JumpToAddress(processorState, address)
            : processorState;

    public static I6502_Sate Process_BCS(this I6502_Sate processorState, ushort address) =>
        processorState.C
            ? JumpToAddress(processorState, address)
            : processorState;

    public static I6502_Sate Process_BNE(this I6502_Sate processorState, ushort address) =>
        !processorState.Z
            ? JumpToAddress(processorState, address)
            : processorState;

    public static I6502_Sate Process_BCC(this I6502_Sate processorState, ushort address) =>
        !processorState.C 
            ? JumpToAddress(processorState, address) 
            : processorState;

    public static I6502_Sate Process_BEQ(this I6502_Sate processorState, ushort address) =>
        processorState.Z 
            ? JumpToAddress(processorState, address) 
            : processorState;

    private static I6502_Sate JumpToAddress(this I6502_Sate processorState, ushort address) =>
        processorState.MergeWith(new
        {
            ProgramCounter = address
        });
}