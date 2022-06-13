﻿namespace Abstractions
{
    // addressing
    // _z   zeropage
    // _a   absolute
    // _i   indirect
    // X / Y Register

    public enum OpCode
    {
        KIL = 0x02,
        BRK = 0x00, // IRQ vector @ $FFFE,$FFFF
        RTI = 0x40,
        NOP = 0xEA,
        

        //LDA
        LDA_immediate = 0xA9,
        LDA_zeropage = 0xA5,
        LDA_zeropage_X = 0xB5,
        LDA_absolute = 0xAD,
        LDA_absolute_X = 0xBD,
        LDA_absolute_Y = 0xB9,
        LDA_indirect_X = 0xA1,
        LDA_indirect_Y = 0xB1,

        //LDX
        LDX_immediate = 0xA2,
        LDX_zeropage = 0xA6,
        LDX_zeropage_Y = 0xB6,
        LDX_absolute = 0xAE,
        LDX_absolute_Y = 0xBE,

        //LDY
        LDY_immediate = 0xA0,
        LDY_zeropage = 0xA4,
        LDY_zeropage_X = 0xB4,
        LDY_absolute = 0xAC,
        LDY_absolute_X = 0xBC,

        //STA
        STA_absolute = 0x8D,
        STA_zeropage = 0x85,
        STA_zeropage_X = 0x95,
        STA_absolute_X = 0x9D,
        STA_absolute_Y = 0x99,
        STA_indirect_X = 0x81,
        STA_indirect_Y = 0x91,

        //STX
        STX_absolute = 0x8E,
        STX_zeropage = 0x86,
        STX_zeropage_Y = 0x96,

        //STY
        STY_absolute = 0x8C,
        STY_zeropage = 0x84,
        STY_zeropage_X = 0x94,


        // ADC Add Memory to Accumulator with Carry
        ADC_absolute = 0x6D,
        ADC_zeropage = 0x65,
        ADC_zeropage_X = 0x75,
        ADC_absolute_X = 0x7D,
        ADC_absolute_Y = 0x79,
        ADC_indirect_X = 0x61,
        ADC_indirect_Y = 0x71,
        ADC_immediate = 0x69,
        // AND
        AND_imediate = 0x29,
        AND_zeropage = 0x25,
        AND_zeropage_X = 0x35,
        AND_absolute = 0x2D,
        AND_absolute_X = 0x3D,
        AND_absolute_Y = 0x39,
        AND_indirect_X = 0x21,
        AND_indirect_Y = 0x31,
        // ASL
        ASL = 0x0A,
        ASL_zeropage    = 0x06,
        ASL_zeropage_X  = 0x16,
        ASL_absolute    = 0x0E,
        ASL_absolute_X  = 0x1E,
        // BCC
        BCC_relative = 0x90,
        // BCS
        BCS_relative = 0xB0,
        // BEQ
        BEQ_relative = 0xF0,
        // BNE
        BNE_relative = 0xD0,
        // BMI
        BMI_relative = 0x30,
        // BPL
        BPL_relative = 0x10,
        //BVC
        BVC_relative = 0x50,
        //BVS
        BVS_relative = 0x70,

       
        // Stack
        PHA = 0x48,
        PLA = 0x68,
        PHP = 0x08,
        PLP = 0x28,
        //Transfer
        TAX = 0xAA,
        TAY = 0xA8,
        TSX = 0xBA,
        TXA = 0x8A,
        TXS = 0x9A,
        TYA = 0x98,

        //BIT
        BIT_zeropage = 0x24,
        BIT_absolute = 0x2C,

        //FLAGS
        SEC = 0x38,
        SED = 0xF8,
        SEI = 0x78,
        CLC = 0x18,
        CLD = 0xD8,
        CLI = 0x58,
        CLV = 0xB8
    }
}
