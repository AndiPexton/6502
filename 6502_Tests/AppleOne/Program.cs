// See https://aka.ms/new-console-template for more information

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


            IAddressSpace AddressSpace = new AddressSpace();

            Shelf.ShelveInstance<IAddressSpace>(AddressSpace);

            var appleRom = File.ReadAllBytes("D:\\Examples\\Emulator\\6502_Tests\\RunCodeTests\\apple1.rom");
            var basicRom = File.ReadAllBytes("D:\\Examples\\Emulator\\6502_Tests\\RunCodeTests\\basic.rom");

            Address.WriteAt(AppleRom, appleRom);
            Address.WriteAt(BasicRom, basicRom);

            Address.RegisterOverlay(new AppleScreen());
            Address.RegisterOverlay(new Keyboard());
            var state = RunToEnd();
        }

        private static I6502_Sate RunToEnd()
        {
            var newState = CpuFunctions.Empty6502ProcessorState();
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
                //hread.Sleep(TimeSpan.FromMilliseconds(1));
            }

            return newState;
        }

    }
}