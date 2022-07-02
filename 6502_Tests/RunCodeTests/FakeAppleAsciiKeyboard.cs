using System.Text;
using Abstractions;
using Dependency;

namespace RunCodeTests;

public class FakeAppleAsciiKeyboard : IOverLay
{
    private ILogger Logger => Shelf.RetrieveInstance<ILogger>();

    private Queue<byte> _keyBuffer;

    public FakeAppleAsciiKeyboard()
    {
        Start = 0xD010;
        End = 0xD011;
        _keyBuffer = new Queue<byte>();
      
    }

    public int Start { get; }
    public int End { get; }
    public void Write(ushort address, byte b)
    {
    }

    public void Type(string s)
    {
        foreach (var c in s) _keyBuffer.Enqueue(Encoding.ASCII.GetBytes(new[] { c })[0]);
        Enter();
    }
    private void Enter()
    {
        _keyBuffer.Enqueue(13);
    }

    public byte Read(ushort address)
    {
        if (address == 0xD010 && _keyBuffer.Any())
        {
            var b = _keyBuffer.Dequeue();
            var dequeue = (byte)(b | 0b10000000);
            Logger?.LogAsciiInput(Encoding.ASCII.GetString(new [] {b}));
            return dequeue;
        }

        if (address == 0xD011 && _keyBuffer.Any() )
        {
            return 0xFF;
        }


        return 0x00;
    }
}