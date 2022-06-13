using Abstractions;
using Shadow.Quack;

namespace Runtime;

public static class FlagOpCodeProcessors
{

    public static I6502_Sate Process_CLC(this I6502_Sate processorState) => 
        processorState.MergeWith(new
        {
            C = false
        });

    public static I6502_Sate Process_SEI(this I6502_Sate processorState) =>
        processorState.MergeWith(new
        {
            I = true
        });

    public static I6502_Sate Process_CLV(this I6502_Sate processorState) =>
        processorState.MergeWith(new
        {
            V = false
        });

    public static I6502_Sate Process_CLI(this I6502_Sate processorState) =>
        processorState.MergeWith(new
        {
            I = false
        });

    public static I6502_Sate Process_SEC(this I6502_Sate processorState) =>
        processorState.MergeWith(new
        {
            C = true
        });

    public static I6502_Sate Process_SED(this I6502_Sate processorState) =>
        processorState.MergeWith(new
        {
            D = true
        });

    public static I6502_Sate Process_CLD(this I6502_Sate processorState) =>
        processorState.MergeWith(new
        {
            D = false
        });
}