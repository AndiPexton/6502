using System;
using Abstractions;
using Dependency;
using FluentAssertions;
using Runtime;
using Shadow.Quack;
using Xunit;

namespace _6502_Tests
{
    public class _6802OpCodeTests
    {
        private const ushort ResetStartAddress = 0x1111;
        private static IAddressSpace AddressSpace => Shelf.RetrieveInstance<IAddressSpace>();

        // https://www.masswerk.at/6502/6502_instruction_set.html

        // LIFO Stack addresses $0100 - $01FD
        // The stack grows down as new values are pushed onto it
        // with the current insertion point maintained in the stack
        // pointer register.
        //
        // $0100
        // ...     ^
        // ...     |
        // $01FF <== 
        //
        // 7 cycles NOP then the processor transfers 
        // control by performing a JMP to the provided address in the Reset vector


        [Fact]
        public void DeadTest()
        {
            Shelf.Clear();

            var newState = RunToEnd();
            newState.ProgramCounter.Should().Be(CpuFunctions.EndOfAddress);
        }

        [Fact]
        public void TestResetVector()
        {
            SetupAddressSpaceAndResetVector();
            byte[] code =
            {
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);
            var newState = RunToEnd();
            newState.ProgramCounter.Should().Be(ResetStartAddress);
        }

        [Fact]
        public void Test_LDX_immediate()
        {
            SetupAddressSpaceAndResetVector();
            byte[] code =
            {
                (byte)OpCode.LDX_immediate, (byte)0x01,
                (byte)OpCode.KIL

            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();
            newState.X.Should().Be(0x01);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeFalse();
        }

        [Fact]
        public void Test_LDY_immediate()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDY_immediate, (byte)0x01,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();
            newState.Y.Should().Be(0x01);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeFalse();
        }

        [Fact]
        public void Test_LDA_Immediate()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, (byte)0x01,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();
            newState.A.Should().Be(0x01);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeFalse();
        }

        [Fact]
        public void Test_LDA_zeropage()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, (byte)0xFE,
                (byte)OpCode.ADC_immediate, (byte)0x01,
                (byte)OpCode.STA_zeropage, (byte)0x10,
                (byte)OpCode.LDA_zeropage, (byte)0x10,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();
            newState.A.Should().Be(0xFF);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeTrue();
        }

        [Fact]
        public void TestLoadAll()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, (byte)0x01, 
                (byte)OpCode.LDY_immediate, (byte)0x02, 
                (byte)OpCode.LDX_immediate, (byte)0x03,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x01);
            newState.X.Should().Be(0x03);
            newState.Y.Should().Be(0x02);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeFalse();
        }

        [Fact]
        public void Test_STA()
        {
            SetupAddressSpaceAndResetVector();

            const byte expected = (byte)0xF0;

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, expected, 
                (byte)OpCode.STA_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            var result = AddressSpace.Read(To16BitAddress((byte)0x80, (byte)0x00), 1)[0];

            result.Should().Be(expected);
        }

        [Fact]
        public void Test_STX()
        {
            SetupAddressSpaceAndResetVector();

            const byte expected = (byte)0xF0;

            byte[] code =
            {
                (byte)OpCode.LDX_immediate, expected,
                (byte)OpCode.STX_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            var result = AddressSpace.Read(To16BitAddress((byte)0x80, (byte)0x00), 1)[0];

            result.Should().Be(expected);
        }

        [Fact]
        public void Test_STY()
        {
            SetupAddressSpaceAndResetVector();

            const byte expected = (byte)0xF0;

            byte[] code =
            {
                (byte)OpCode.LDY_immediate, expected,
                (byte)OpCode.STY_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            var result = AddressSpace.Read(To16BitAddress((byte)0x80, (byte)0x00), 1)[0];

            result.Should().Be(expected);
        }

        [Fact]
        public void Test_ADC_AddWithCarry_Simple()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.STA_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.ADC_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(2);
            newState.ReadCarryFlag().Should().Be(0);
        }

        [Fact]
        public void Test_ADC_AddWithCarry()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.STA_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.ADC_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0);
            newState.ReadCarryFlag().Should().Be(1);
        }




        [Fact]
        public void Test_ADC_AddWithCarry_Complex()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.STA_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.LDA_immediate, 0x02,
                (byte)OpCode.ADC_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.STA_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.ADC_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(3);
            newState.ReadCarryFlag().Should().Be(0);
        }

        [Fact]
        public void Test_ADC_AddWithCarry_Complex_Clear()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.STA_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.LDA_immediate, 0x02,
                (byte)OpCode.ADC_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.STA_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.CLC,
                (byte)OpCode.ADC_absolute, (byte)0x80, (byte)0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(2);
            newState.ReadCarryFlag().Should().Be(0);
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


        private static ushort To16BitAddress(byte low, byte high) => 
            BitConverter.ToUInt16(new[] { low, high });


        private static void SetupAddressSpaceAndResetVector()
        {
            Shelf.Clear();
            // 16-bit reset vector at $FFFC (LB-HB)
            // Reset all registers program counter set to address in vector

            IAddressSpace AddressSpace = new AddressSpace();
            AddressSpace.SetResetVector(ResetStartAddress);
            Shelf.ShelveInstance<IAddressSpace>(AddressSpace);
        }

        [Fact]
        public void Test_Stack_Push_PHA()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x02,
                (byte)OpCode.PHA,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(2);
            newState.S.Should().Be(0xFE);
        }

        [Fact]
        public void Test_Stack_Push_PHA_Multi()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x02,
                (byte)OpCode.PHA,
                (byte)OpCode.PHA,
                (byte)OpCode.PHA,
                (byte)OpCode.PHA,
                (byte)OpCode.PHA,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(2);
            newState.S.Should().Be(0xFA);
        }

        [Fact]
        public void Test_Stack_Pull_PLA()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x02,
                (byte)OpCode.PHA,
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.PLA,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(2);
            newState.S.Should().Be(0xFF);
        }

        [Fact]
        public void Test_Stack_Pull_PLA_Multi()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x02,
                (byte)OpCode.PHA,
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.PHA,
                (byte)OpCode.PLA,
                (byte)OpCode.PLA,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(2);
            newState.S.Should().Be(0xFF);
        }

        [Fact]
        public void Test_Stack_Pull_PHP()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.PHP,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0xFF);
            newState.S.Should().Be(0xFE);
            newState.N.Should().BeTrue();
        }

        [Fact]
        public void Test_Stack_Pull_PLP()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.PHP,
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.PLP,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x01);
            newState.S.Should().Be(0xFF);
            newState.N.Should().BeTrue();
        }

        [Fact]
        public void Test_AND()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x10,
                (byte)OpCode.STA_zeropage, 0x0A, 
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.AND_zeropage, 0x0A,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x00);
            newState.Z.Should().BeTrue();
        }

        [Fact]
        public void Test_ASL_A()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.ASL,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x02);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeFalse();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_ASL_A_Carry()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x80,
                (byte)OpCode.ASL,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x00);
            newState.Z.Should().BeTrue();
            newState.N.Should().BeFalse();
            newState.C.Should().BeTrue();
        }

        [Fact]
        public void Test_ASL_X()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x80,
                (byte)OpCode.LDX_immediate, 0x01,
                (byte)OpCode.STA_zeropage_X, 0xA0,
                (byte)OpCode.ASL_zeropage_X, 0xA0,
                (byte)OpCode.LDA_zeropage_X, 0xA0,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x00);
            newState.Z.Should().BeTrue();
            newState.N.Should().BeFalse();
            newState.C.Should().BeTrue();
        }

        [Fact]
        public void Test_BCC()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xA0,
                (byte)OpCode.CLC,
                (byte)OpCode.BCC_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x01);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeFalse();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_BCC_Carry()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x80,
                (byte)OpCode.ASL,
                (byte)OpCode.BCC_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x00);
            newState.Z.Should().BeTrue();
            newState.N.Should().BeFalse();
            newState.C.Should().BeTrue();
        }

        [Fact]
        public void Test_BCS()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x80,
                (byte)OpCode.ASL,
                (byte)OpCode.BCS_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x01);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeFalse();
            newState.C.Should().BeTrue();
        }

        [Fact]
        public void Test_BEQ()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.BEQ_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x01);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeFalse();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_BEQ_False()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.BEQ_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x01);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeFalse();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_BNE()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.BNE_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x00);
            newState.Z.Should().BeTrue();
            newState.N.Should().BeFalse();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_BMI()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.BMI_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x00);
            newState.Z.Should().BeTrue();
            newState.N.Should().BeFalse();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_BMI_Positive()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x0F,
                (byte)OpCode.BMI_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x0F);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeFalse();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_BPL()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0x0F,
                (byte)OpCode.BPL_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x00);
            newState.Z.Should().BeTrue();
            newState.N.Should().BeFalse();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_BPL_Negative()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.BPL_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0xFF);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeTrue();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_BRK()
        {
            SetupAddressSpaceAndResetVector();
            const ushort irqAddress = 0x2222;

            AddressSpace.SetIRQVector(irqAddress);

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xAF,
                (byte)OpCode.BRK, 0x22,
                (byte)OpCode.KIL
            };

            byte[] IRQCode =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);
            AddressSpace.WriteAt(irqAddress, IRQCode);

            var newState = RunToEnd();

            newState.A.Should().Be(0xFF);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeTrue();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_RTI()
        {
            SetupAddressSpaceAndResetVector();
            const ushort irqAddress = 0x2222;

            AddressSpace.SetIRQVector(irqAddress);

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xAF,
                (byte)OpCode.BRK, 0x22,
                (byte)OpCode.LDY_immediate, 0xAD,
                (byte)OpCode.KIL
            };

            byte[] IRQCode =
            {
                (byte)OpCode.LDX_immediate, 0xFF,
                (byte)OpCode.RTI
            };

            AddressSpace.WriteAt(ResetStartAddress, code);
            AddressSpace.WriteAt(irqAddress, IRQCode);

            var newState = RunToEnd();

            newState.A.Should().Be(0xAF);
            newState.X.Should().Be(0xFF);
            newState.Y.Should().Be(0xAD);

        }

        [Fact]
        public void Test_TAX()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.TAX,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.X.Should().Be(0xFF);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeTrue();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_TAY()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.TAY,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();


            newState.Y.Should().Be(0xFF);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeTrue();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_TSX()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.TSX,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            
            newState.X.Should().Be(0xFF);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeTrue();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_TXA()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDX_immediate, 0xFF,
                (byte)OpCode.TXA,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();


            newState.A.Should().Be(0xFF);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeTrue();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_TXS()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDX_immediate, 0xAF,
                (byte)OpCode.TXS,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();


            newState.S.Should().Be(0xAF);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeTrue();
            newState.C.Should().BeFalse();
        }

        [Fact]
        public void Test_TYA()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDY_immediate, 0xAF,
                (byte)OpCode.TYA,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();


            newState.A.Should().Be(0xAF);
            newState.Z.Should().BeFalse();
            newState.N.Should().BeTrue();
            newState.C.Should().BeFalse();
        }


        [Fact]
        public void Test_BIT()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xAF,
                (byte)OpCode.STA_zeropage_X, 0x01,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.BIT_zeropage, 0x01,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.Z.Should().BeTrue();
            newState.N.Should().BeTrue();
            newState.V.Should().BeFalse();
        }

        [Fact]
        public void Test_BIT_2()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xAF,
                (byte)OpCode.STA_zeropage_X, 0x01,
                (byte)OpCode.LDA_immediate, 0x01,
                (byte)OpCode.BIT_absolute, 0x01, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.Z.Should().BeFalse();
            newState.N.Should().BeTrue();
            newState.V.Should().BeFalse();
        }


        [Fact]
        public void Test_BVC()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.BVC_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x00);
        }


        [Fact]
        public void Test_BVS()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.STA_zeropage, 0x01,
                (byte)OpCode.BIT_zeropage, 0x01,
                (byte)OpCode.BVS_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0x00);
        }

        [Fact]
        public void Test_BVC_Negative()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.STA_zeropage, 0x01,
                (byte)OpCode.BIT_zeropage, 0x01,
                (byte)OpCode.BVC_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0xFF);
        }

        [Fact]
        public void Test_BVS_Negative()
        {
            SetupAddressSpaceAndResetVector();

            byte[] code =
            {
                (byte)OpCode.LDA_immediate, 0xFF,
                (byte)OpCode.BVS_relative, 0x01,
                (byte)OpCode.KIL,
                (byte)OpCode.LDA_immediate, 0x00,
                (byte)OpCode.KIL
            };

            AddressSpace.WriteAt(ResetStartAddress, code);

            var newState = RunToEnd();

            newState.A.Should().Be(0xFF);
        }

    }
}