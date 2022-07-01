using System.Text;
using Abstractions;
using Xunit.Abstractions;

namespace RunCodeTests;

public class Mode7Screen : IOverLay
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IDictionary<ushort, byte> map;

    public Mode7Screen(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        Start = 0x7C00;
        End = 0x7FFF;
        this.map = new Dictionary<ushort, byte>();
    }

    public int Start { get; }
    public int End { get; }
    public void Write(ushort address, byte b)
    {
        if (b>0 && b!=32) _testOutputHelper.WriteLine(Encoding.ASCII.GetString(new [] {b}));
        if (map.ContainsKey(address))
            map[address] = b;
        else
            map.Add(address, b);
    }

    public byte Read(ushort address) => 
        map.ContainsKey(address) ? map[address] : (byte)0;
}