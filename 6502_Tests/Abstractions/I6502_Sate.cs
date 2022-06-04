namespace Abstractions;

public interface I6502_Sate
{
    ushort ProgramCounter { get; }
    byte Y { get; }
    byte A { get; }
    byte X { get; }
    bool C { get; }
    bool Z { get; }
    bool I { get; }
    bool D { get; }
    bool B { get; }
    bool V { get; }
    bool N { get; }
    byte S { get; }
}