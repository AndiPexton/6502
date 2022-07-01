using Abstractions;

namespace RunCodeTests;

public class _6522_VIA_SYSTEM_VIA23 : IOverLay
{
    private byte _register;
    private static readonly int _VIAInterruptEnableRegister = 0xFE4E;
    private static readonly int _KeyBoard = 0xFE4F;
    private static readonly int _Sound_Keyboard_Set = 0xFE43;
    private static readonly int _SoundCommand = 0xFE41;
    private byte _Row;
    private byte _Col;
    private byte _soundCommand;
    private byte _sound_Keyboard_Setting;

    public _6522_VIA_SYSTEM_VIA23()
    {
           
        Start = 0xFE40;
        End = 0xFE5F;
        _register = 0;
    }

    public int Start { get; }
    public int End { get; }
    public void Write(ushort address, byte b)
    {
        if (address == _VIAInterruptEnableRegister)
            if ((b & 0b10000000) == 0b10000000)
            {
                _register = (byte)(b | _register);
                return;
            }
            else
            {
                _register = (byte)((~b) & _register);
                return;
            }

        if (address == _KeyBoard)
        {
            _Row = (byte)((b & 0b01110000) >> 4);
            _Col = (byte)(b & 0b00001111);
            return;
        }
        if (address == _Sound_Keyboard_Set)
        {
            _sound_Keyboard_Setting = b;
            return;
        }
        if (address == _SoundCommand)
        {
            _soundCommand = b;
            return;
        }

        var i = 1;
    }

    public byte Read(ushort address)
    {
        if (address == _VIAInterruptEnableRegister)
            return (byte)(0b10000000 | _register);

        if (address == _KeyBoard)
            return 0;

        return 0;
    }
}