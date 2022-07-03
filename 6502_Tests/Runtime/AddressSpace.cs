using Abstractions;
using Dependency;

namespace Runtime;

public class AddressSpace : IAddressSpace
{
    private static ILogger Logger => Shelf.RetrieveInstance<ILogger>();

    public const ushort _ResetVector = 65532;
    public const ushort _IRQVector = 0xFFFE;
    private const byte ZeroByte = (byte)0;
    private readonly IDictionary<ushort, byte> map;
    private readonly IList<IOverLay> overlays;

    public AddressSpace()
    {
        this.map = new Dictionary<ushort, byte>();
        this.overlays = new List<IOverLay>();
    }

    public void SetResetVector(ushort wordAddress)
    {
        var bytes = BitConverter.GetBytes(wordAddress);
        map[_ResetVector] = bytes[0];
        map[_ResetVector+1] = bytes[1];
    }

    public void SetIRQVector(ushort wordAddress)
    {
        var bytes = BitConverter.GetBytes(wordAddress);
        map[_IRQVector] = bytes[0];
        map[_IRQVector + 1] = bytes[1];
    }

    public ushort GetResetVector()
    {
        var bytes = new byte[]
        {
            map.ContainsKey(_ResetVector) ? map[_ResetVector] : ZeroByte,
            map.ContainsKey(_ResetVector + 1) ? map[_ResetVector + 1] : ZeroByte
        };

        return BitConverter.ToUInt16(bytes);
    }

    public ushort GetIRQVector()
    {
        var bytes = new byte[]
        {
            map.ContainsKey(_IRQVector) ? map[_IRQVector] : ZeroByte,
            map.ContainsKey(_IRQVector + 1) ? map[_IRQVector + 1] : ZeroByte
        };

        return BitConverter.ToUInt16(bytes);
    }

    public void WriteAt(ushort i, byte[] code)
    {
        foreach (var b in code)
        {
            WriteAt(i, b);
            i++;
        }
    }

    public void WriteAt(ushort i, byte value)
    {
        WriteByte(i, value);
        Logger?.LogWrite(i, value);
    }

    private void WriteByte(ushort address, byte b)
    {
        var overLay = GetOverlay(address);
        if (overLay != null)
            overLay.Write(address, b);
        else
            map[address] = b;
    }

    private IOverLay? GetOverlay(ushort address) => 
        overlays.FirstOrDefault(x => x.Start <= address && x.End >= address);

    public byte[] Read(ushort address, int bytes)
    {
        return Enumerable.Range(0, bytes)
            .Select(offset =>
            {
                var i = ToUShort(address + offset);
                return ResolveValue(i);
            }).ToArray();
    }

    public void RegisterOverlay(IOverLay overlay)
    {
        overlays.Add(overlay);
    }

    private static ushort ToUShort(int offset)
    {
        var array = BitConverter.GetBytes(offset).Take(2).ToArray();
        return BitConverter.ToUInt16(array);
    }

    private byte ResolveValue(ushort address)
    {
        var overLay = GetOverlay(address);
        byte value;
        if (overLay != null)
            value = overLay.Read(address);
        else
            value = map.ContainsKey(address) ? map[address] : (byte)0;
        Logger?.LogRead(address, value);
        return value;
    }
}