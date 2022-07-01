using System.Text;
using Abstractions;

namespace AppleOne;

public class Keyboard : IOverLay
{
     
    public Keyboard()
    {

        Start = 0xD010;
        End = 0xD011;
          
    }

    public int Start { get; }
    public int End { get; }
    public void Write(ushort address, byte b)
    {
    }

    public byte Read(ushort address)
    {
        if (address == 0xD010 && Console.KeyAvailable)
        {
            var consoleKeyInfo = Console.ReadKey(true);

            if (consoleKeyInfo.Key == ConsoleKey.Escape) 
                return (byte)(0x9B | 0b10000000);
            if (consoleKeyInfo.Key == ConsoleKey.Enter)
                return (byte)(0x8D | 0b10000000);
            if (consoleKeyInfo.Key == ConsoleKey.Backspace) 
                return (byte)(0xDF | 0b10000000);

            var ascii = Encoding.ASCII.GetBytes( new[] { consoleKeyInfo.KeyChar.ToString().ToUpper()[0]})[0];
                
            return (byte)(ascii | 0b10000000);
        }

        if (address == 0xD011 && Console.KeyAvailable)
            return 0xFF;

        return 0x00;
    }
}