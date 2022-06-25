namespace Abstractions;

public interface ILogger
{
    void LogInstruction(OpCode opCode, ushort processorStateProgramCounter);
    string GetLog();
    void LogAddress(ushort? address);
    void LogStackPull(byte pulledValue);
    void LogStackPush(byte value);
    void LogStackPointer(byte S);
}