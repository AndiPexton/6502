using System.Text;
using Abstractions;
using Xunit.Abstractions;

namespace RunCodeTests;

public class FakeAppleDisplay : IOverLay
{
    private readonly ITestOutputHelper _console;
    private readonly StringBuilder _lineBuffer;
    private readonly StringBuilder _output;
    public FakeAppleDisplay(ITestOutputHelper testOutputHelper)
    {
        Start = 0xD012;
        End = 0xD012;
        _console = testOutputHelper;
        _lineBuffer = new StringBuilder();
        _output = new StringBuilder();
    }

    public int Start { get;  }
    public int End { get;  }

    public void Flush()
    {
        OutputLine(_lineBuffer.ToString());
        _lineBuffer.Clear();
    }

    private void OutputLine(string line)
    {
        _console.WriteLine(line);
        _output.AppendLine(line);
    }

    public void Write(ushort address, byte b)
    {
        b = StripBit7(b);

        switch (b)
        {
            case 27:
                return;
            case 13:
                Flush();
                break;
            default:
                _lineBuffer.Append(AsciiByteToString(b));
                break;
        }
    }

    private static byte StripBit7(byte b) => 
        (byte)(b & 0b01111111);

    private static string AsciiByteToString(byte b) => 
        Encoding.ASCII.GetString(new[] { b });

    public byte Read(ushort address) 
        => 0;

    public string GetOutput() => 
        _output.ToString();
}