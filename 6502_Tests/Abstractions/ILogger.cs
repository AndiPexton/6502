namespace Abstractions;

public interface ILogger
{
    void LogInstruction(OpCode opCode, ushort processorStateProgramCounter);
    string GetLog();
}