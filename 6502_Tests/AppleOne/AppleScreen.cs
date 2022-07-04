using System.Text;
using Abstractions;
using Shadow.Quack;

namespace AppleOne;

public class AppleScreen : IOverLay
{
    private ushort screen = 0xD012;
    private ushort screenRegister = 0xD013;
    private ushort fgAddress = 0xD014;
    private ushort bgAddress = 0xD015;
    private ushort cursorX = 0xD016;
    private ushort cursorY = 0xD017;

    public AppleScreen()
    {
        Start = 0xD012;
        End = 0xD017;
        Console.CursorVisible = true;
    }

    public int Start { get; }
    public int End { get; }

    public void Write(ushort address, byte b)
    {
        if (address == screenRegister)
        {
            if (b == 0)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.BackgroundColor = ConsoleColor.Black;
            }
            if (b == 1)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }


        if (address == screen)
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

        if (address == fgAddress)
            Console.ForegroundColor = Duck.CastToEnum<ConsoleColor>(b);
        if (address == bgAddress)
            Console.BackgroundColor = Duck.CastToEnum<ConsoleColor>(b);
        if (address == cursorX)
            Console.CursorLeft = b;
        if (address == cursorY)
            Console.CursorTop = b;
    }

    public byte Read(ushort address)
    {
        if (address == bgAddress)
            return (byte)Console.BackgroundColor;
        if (address == fgAddress)
            return (byte)Console.ForegroundColor;
        if (address == cursorX)
            return (byte)Console.CursorLeft;
        if (address == cursorY)
            return (byte)Console.CursorTop;

        return 0;
    }
}