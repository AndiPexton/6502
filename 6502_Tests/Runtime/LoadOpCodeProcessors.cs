using Abstractions;
using Dependency;
using Shadow.Quack;

namespace Runtime;

public static class LoadOpCodeProcessors
{
    private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();


    public static I6502_Sate Process_LDY(this I6502_Sate processorState, ushort address)
    {
        var b = Address.Read(address, 1)[0];
        return processorState.MergeWith(new 
        { 
            Y = b, 
            Z = b == 0,
            N = b.IsNegative()
        });
    }

    public static I6502_Sate Process_LDX(this I6502_Sate processorState, ushort address)
    {
        var b = Address.Read(address, 1)[0];
        return processorState.MergeWith(new
        {
            X = b,
            Z = b == 0,
            N = b.IsNegative()
        });
    }

    public static I6502_Sate Process_LDA(this I6502_Sate processorState, ushort address)
    {
        var b = Address.Read(address, 1)[0];
        return processorState.MergeWith(new
        {
            A = b,
            Z = b == 0,
            N = b.IsNegative()
        });
    }
}