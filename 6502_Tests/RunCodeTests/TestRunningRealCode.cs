using System.Text;
using Abstractions;
using Dependency;
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

        public TestRunningRealCode(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private const ushort OSRom = 0xC000;
        private static IAddressSpace Address => Shelf.RetrieveInstance<IAddressSpace>();

        [Fact]
        public void RunCode()
        {
            Shelf.Clear();
            // 16-bit reset vector at $FFFC (LB-HB)
            // Reset all registers program counter set to address in vector

            IAddressSpace AddressSpace = new AddressSpace();
            AddressSpace.SetResetVector(0xd9cd);
            Shelf.ShelveInstance<IAddressSpace>(AddressSpace);
            //Shelf.ShelveInstance<ILogger>(new Logger());
            var osRom = File.ReadAllBytes("D:\\Examples\\Emulator\\6502_Tests\\RunCodeTests\\Os12.rom");

            Address.WriteAt(OSRom, osRom);

            Address.RegisterOverlay(new Mode7Screen(_testOutputHelper));

            RunToEnd();

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
            }

            return newState;
        }

    }

    public class Logger : ILogger
    {
        private readonly StringBuilder _log;

        public Logger()
        {
            this._log = new StringBuilder();
        }

        public void LogInstruction(OpCode opCode, ushort processorStateProgramCounter)
        {
            _log.AppendLine($"{processorStateProgramCounter:X} : {opCode}");
        }

        public string GetLog()
        {
            return _log.ToString();
        }
    }

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
            _testOutputHelper.WriteLine($"{b}");
            if (map.ContainsKey(address))
                map[address] = b;
            else
                map.Add(address, b);
        }

        public byte Read(ushort address) => 
            map.ContainsKey(address) ? map[address] : (byte)0;
    }
}