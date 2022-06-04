namespace Abstractions;

public interface IOverLay
{
    int Start { get; }
    int End { get; }
    void Write(ushort address, byte b);
    byte Read(ushort address);
}