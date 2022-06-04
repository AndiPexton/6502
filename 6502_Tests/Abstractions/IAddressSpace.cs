namespace Abstractions;

public interface IAddressSpace
{
    void SetResetVector(ushort wordAddress);
    void SetIRQVector(ushort wordAddress);
    ushort GetResetVector();
    ushort GetIRQVector();
    void WriteAt(ushort i, byte[] code);
    void WriteAt(ushort i, byte value);
    byte[] Read(ushort address, int bytes);
    void RegisterOverlay(IOverLay overlay);
}