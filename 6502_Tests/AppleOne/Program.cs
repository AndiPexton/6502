using System.ComponentModel.Design;
using Abstractions;
using Dependency;
using Runtime;

namespace AppleOne
{
    public static class AppleOne
    {
        private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();
        private const ushort AppleRom = 0xFF00;
        private const ushort BasicRom = 0xE000;
        public static void Main(string[] args)
        {
            Shelf.Clear();

            IAddressSpace addressSpace = new AddressSpace();

            Shelf.ShelveInstance<IAddressSpace>(addressSpace);

            var appleRom = File.ReadAllBytes("D:\\Examples\\Emulator\\6502_Tests\\RunCodeTests\\apple1.rom");
            var basicRom = File.ReadAllBytes("D:\\Examples\\Emulator\\6502_Tests\\RunCodeTests\\basic.rom");
            var apple30 = File.ReadAllBytes("D:\\Examples\\30th.rom");
            Address.WriteAt(AppleRom, appleRom);
            Address.WriteAt(BasicRom, basicRom);
            Address.WriteAt(0x0280, apple30);

            Address.RegisterOverlay(new AppleScreen());
            Address.RegisterOverlay(new Keyboard());
            RunToEnd();
        }

        private static I6502_Sate RunToEnd()
        {
            var newState = _6502cpu.Empty6502ProcessorState();
            var run = true;

            while (run)
            {
                try
                {
                    newState = newState.RunCycle();
                }
                catch (ProcessorKillException)
                {
                    run = false;
                }
            }

            return newState;
        }
    }
}