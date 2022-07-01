using System.Text;
using Abstractions;
using Xunit.Abstractions;

namespace RunCodeTests;

public class FakeAppleDisplay : IOverLay
{
    private ITestOutputHelper _console;
    private StringBuilder _lineBuffer;
    public FakeAppleDisplay(ITestOutputHelper testOutputHelper)
    {
        Start = 0xD012;
        End = 0xD012;
        _console = testOutputHelper;
        _lineBuffer = new StringBuilder();
    }

    public int Start { get;  }
    public int End { get;  }

    public void Flush()
    {
        _console.WriteLine(_lineBuffer.ToString());
        _lineBuffer.Clear();
    }

    public void Write(ushort address, byte b)
    {
        if (b == 155) return;
        b = (byte)(b & 0b01111111);

        if (b == 13)
        {
            _console.WriteLine(_lineBuffer.ToString());
            _lineBuffer.Clear();
        }
        else
        {
            var s = Encoding.ASCII.GetString(new[] { b });
            _lineBuffer.Append(s);
        }
    }

    public byte Read(ushort address)
    {
        return 0;
    }
}