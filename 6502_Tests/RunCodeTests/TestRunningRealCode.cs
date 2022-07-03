using System.Text;
using Abstractions;
using Dependency;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Runtime;
using Xunit;
using Xunit.Abstractions;

namespace RunCodeTests
{
    public class TestRunningRealCode
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private static ILogger Logger => Shelf.RetrieveInstance<ILogger>();


        public TestRunningRealCode(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private const ushort OSRom = 0xC000;
        private const ushort AppleRom = 0xFF00;
        private const ushort BasicRom = 0xE000;
        private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();

     
        public void RunCode()
        {
            Shelf.Clear();
            // 16-bit reset vector at $FFFC (LB-HB)
            // Reset all registers program counter set to address in vector

            IAddressSpace AddressSpace = new AddressSpace();
            AddressSpace.SetResetVector(0xd9cd);
            Shelf.ShelveInstance<IAddressSpace>(AddressSpace);
          //  Shelf.ShelveInstance<ILogger>(new TestLogger());
            var osRom = File.ReadAllBytes("D:\\Examples\\Emulator\\6502_Tests\\RunCodeTests\\Os12.rom");

            Address.WriteAt(OSRom, osRom);

            Address.RegisterOverlay(new Mode7Screen(_testOutputHelper));
            Address.RegisterOverlay(new _6522_VIA_SYSTEM_VIA23());
            var state = RunToEndOr(100000);

        }

        [Fact]
        public void Apple1()
        {
            Shelf.Clear();
            // 16-bit reset vector at $FFFC (LB-HB)
            // Reset all registers program counter set to address in vector

            IAddressSpace AddressSpace = new AddressSpace();
          
            Shelf.ShelveInstance<IAddressSpace>(AddressSpace);
            var logger = new TestLogger();
            Shelf.ShelveInstance<ILogger>(logger);
            var appleRom = File.ReadAllBytes("D:\\Examples\\Emulator\\6502_Tests\\RunCodeTests\\apple1.rom");
            var basicRom = File.ReadAllBytes("D:\\Examples\\Emulator\\6502_Tests\\RunCodeTests\\basic.rom");

            Address.WriteAt(AppleRom, appleRom);
            Address.WriteAt(BasicRom, basicRom);

            var display = new FakeAppleDisplay(_testOutputHelper);
            var keyboard = new FakeAppleAsciiKeyboard();
            Address.RegisterOverlay(display);
            Address.RegisterOverlay(keyboard);

            keyboard.Type("E000R");
            keyboard.Type("PRINT \"HELLO\"");

            var state = RunToEndOr(10000);
            display.Flush();

            var output = display.GetOutput();

            File.WriteAllText("D:\\Examples\\appleOne.log", logger.GetLog());
            File.WriteAllBytes("D:\\Examples\\appleOne.dump", Address.Read(0,0xFFFF));
            File.WriteAllText("D:\\Examples\\appleOne.txt", output);
            
            Assert.Equal("\\\r\nE000R\r\n\r\nE000: 4C\r\n>PRINT \"HELLO\"\r\nHELLO\r\n>", output );

        }

        [Fact]
        public void _6502_Func_Tests()
        {
            Shelf.Clear();
            // 16-bit reset vector at $FFFC (LB-HB)
            // Reset all registers program counter set to address in vector

            IAddressSpace AddressSpace = new AddressSpace();

            Shelf.ShelveInstance<IAddressSpace>(AddressSpace);
            var logger = new TestLogger();
            
            var testRom = File.ReadAllBytes("D:\\Examples\\Emulator\\6502_Tests\\RunCodeTests\\6502_functional_test.rom");
            
            Address.WriteAt(0, testRom);

            Shelf.ShelveInstance<ILogger>(logger);
            AddressSpace.SetResetVector(0x0400);
            
            var state = RunToEndOr(100000);
            
            File.WriteAllText("D:\\Examples\\test.log", logger.GetLog());
            File.WriteAllBytes("D:\\Examples\\test.dump", Address.Read(0, 0xFFFF));
            
        }



        private static I6502_Sate RunToEndOr(int limit)
        {
            var newState = CpuFunctions.Empty6502ProcessorState();
            var run = true;
            var instructions = 0;
            while (run)
            {
                try
                {
                    newState = newState.RunCycle();
                    instructions++;
                }
                catch (ProcessorKillException)
                {
                    run = false;
                }

                if (instructions > limit) run = false;
                Logger?.LogState(newState);
            }

           
            return newState;
        }

    }
}