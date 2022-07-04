namespace Abstractions;

public interface ILogger
{
    void LogInstruction(OpCode opCode, ushort processorStateProgramCounter);
    string GetLog();
    void LogAddress(ushort? address);
    void LogStackPull(byte pulledValue);
    void LogStackPush(byte value);
    void LogStackPointer(byte S);
    void LogRead(ushort address, byte value);
    void LogWrite(ushort address, byte value);
    void LogTextOutput(string asciiChar);
    void LogAsciiInput(string asciiChar);
    void LogState(I6502_Sate newState);
}