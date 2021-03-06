using System.Text;
using Abstractions;
using Runtime;

namespace RunCodeTests;

public class TestLogger : ILogger
{
    private readonly StringBuilder _log;

    public TestLogger()
    {
        this._log = new StringBuilder();
    }

    public void LogInstruction(OpCode opCode, ushort processorStateProgramCounter)
    {
        _log.AppendLine();
        _log.Append($"${processorStateProgramCounter:X4} : {opCode}");
    }

    public string GetLog()
    {
        return _log.ToString();
    }

    public void LogAddress(ushort? address)
    {
        var addressOutput = address == null ? string.Empty : $"  A[${address.Value:X4}]";
        _log.Append($"{addressOutput}");
    }

    public void LogStackPull(byte pulledValue)
    {
        _log.AppendLine();
        _log.Append($"    << ${pulledValue:X2}");
    }

    public void LogStackPush(byte value)
    {
        _log.AppendLine();
        _log.Append($"    >> ${value:X2}");
    }

    public void LogStackPointer(byte s)
    {
        _log.AppendLine();
        _log.Append($"  S = ${s:X2}");
    }

    public void LogRead(ushort address, byte value)
    {
        _log.AppendLine();
        _log.Append($"\t\t R[${address:X4}] >> ${value:X2} ({value})");
    }

    public void LogWrite(ushort address, byte value)
    {
        _log.AppendLine();
        _log.Append($"\t\t W[${address:X4}] << ${value:X2} ({value})");
    }

    public void LogTextOutput(string asciiChar)
    {
        _log.Append($" \"{asciiChar}\">");
    }

    public void LogAsciiInput(string asciiChar)
    {
        _log.Append($" <\"{asciiChar}\"");
    }

    public void LogState(I6502_Sate newState)
    {
        _log.AppendLine();
        _log.Append($"PC:${newState.ProgramCounter:X4} A:${newState.A:X2} X:${newState.X:X2} Y:${newState.Y:X2} flags:[${newState.ReadStateRegister(newState.B):X2}] C:{newState.C} Z:{newState.Z} I:{newState.I} D:{newState.D} B:{newState.B} V:{newState.V} N:{newState.N}");
    }
}