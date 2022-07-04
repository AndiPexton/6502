using System.Text;
using Abstractions;

namespace AppleOne;

public class AppleScreen : IOverLay
{
       
    public AppleScreen()
    {
        Start = 0xD012;
        End = 0xD012;
        Console.CursorVisible = true;
        Console.CursorSize = 1;
    }

    public int Start { get; }
    public int End { get; }

    public void Write(ushort address, byte b)
    {
        if (b == 155) return;
        b = (byte)(b & 0b01111111);
           
        if (b == 13)
        {
            Console.WriteLine();
        }
        else
        {
            var format = Encoding.ASCII.GetString(new[] { b });
            Console.Write(format);
        }
    }
    
    public byte Read(ushort address)
    {
        return 0;
    }
}