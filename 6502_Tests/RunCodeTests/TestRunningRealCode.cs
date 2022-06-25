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
          //  Shelf.ShelveInstance<ILogger>(new Logger());
            var osRom = File.ReadAllBytes("D:\\Examples\\Emulator\\6502_Tests\\RunCodeTests\\Os12.rom");

            Address.WriteAt(OSRom, osRom);

            Address.RegisterOverlay(new Mode7Screen(_testOutputHelper));
            Address.RegisterOverlay(new _6522_VIA_SYSTEM_VIA23());
            var state = RunToEnd();

        }

        private static I6502_Sate RunToEnd()
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

                if (instructions > 100000) run = false;
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
            _log.Append($"${processorStateProgramCounter:X4} : {opCode}");
        }

        public string GetLog()
        {
            return _log.ToString();
        }

        public void LogAddress(ushort? address)
        {
            var addressOutput = address == null ? string.Empty : $" ${address.Value:X4}";
            _log.AppendLine($"{addressOutput}");
        }

        public void LogStackPull(byte pulledValue)
        {
            _log.AppendLine($"    << ${pulledValue:X2}");
        }

        public void LogStackPush(byte value)
        {
            _log.AppendLine($"    >> ${value:X2}");
        }

        public void LogStackPointer(byte s)
        {
            _log.AppendLine($"  S = ${s:X2}");
        }
    }

    public class _6522_VIA_SYSTEM_VIA23 : IOverLay
    {
        private byte _register;
        private static readonly int _VIAInterruptEnableRegister = 0xFE4E;
        private static readonly int _KeyBoard = 0xFE4F;
        private static readonly int _Sound_Keyboard_Set = 0xFE43;
        private static readonly int _SoundCommand = 0xFE41;
        private byte _Row;
        private byte _Col;
        private byte _soundCommand;
        private byte _sound_Keyboard_Setting;

        public _6522_VIA_SYSTEM_VIA23()
        {
           
            Start = 0xFE40;
            End = 0xFE5F;
            _register = 0;
        }

        public int Start { get; }
        public int End { get; }
        public void Write(ushort address, byte b)
        {
            if (address == _VIAInterruptEnableRegister)
                if ((b & 0b10000000) == 0b10000000)
                {
                    _register = (byte)(b | _register);
                    return;
                }
                else
                {
                    _register = (byte)((~b) & _register);
                    return;
                }

            if (address == _KeyBoard)
            {
                _Row = (byte)((b & 0b01110000) >> 4);
                _Col = (byte)(b & 0b00001111);
                return;
            }
            if (address == _Sound_Keyboard_Set)
            {
                _sound_Keyboard_Setting = b;
                return;
            }
            if (address == _SoundCommand)
            {
                _soundCommand = b;
                return;
            }

            var i = 1;
        }

        public byte Read(ushort address)
        {
            if (address == _VIAInterruptEnableRegister)
                return (byte)(0b10000000 | _register);

            if (address == _KeyBoard)
                return 0;

            return 0;
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
            if (b>0) _testOutputHelper.WriteLine($"{b}");
            if (map.ContainsKey(address))
                map[address] = b;
            else
                map.Add(address, b);
        }

        public byte Read(ushort address) => 
            map.ContainsKey(address) ? map[address] : (byte)0;
    }
}