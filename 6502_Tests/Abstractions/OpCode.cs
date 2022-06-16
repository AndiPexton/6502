namespace Abstractions
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
        AND_immediate = 0x29,
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
        CLV = 0xB8,

        //JUMP
        JMP_absolute = 0x4C,
        JMP_indirect = 0x6C,
        JSR_absolute = 0x20,
        RTS = 0x60,

        // Subtract with Borrow
        SBC_immediate = 0xE9,
        SBC_zeropage = 0xE5,
        SBC_zeropage_X = 0xF5,
        SBC_absolute = 0xED,
        SBC_absolute_x = 0xFD,
        SBC_absolute_Y = 0xF9,
        SBC_indirect_X = 0xE1,
        SBC_indirect_Y = 0xF1,

        //ROL
        ROL = 0x2A,
        ROL_zeropage = 0x26,
        ROL_zeropage_X = 0x36,
        ROL_absolute = 0x2E,
        ROL_ABSOLUTE_x = 0X3E,

        //ROR
        ROR = 0x6A,
        ROR_zeropage = 0x66,
        ROR_zeropage_X = 0x76,
        ROR_absolute = 0x6E,
        ROR_ABSOLUTE_x = 0X7E,

        //LSR
        LSR = 0x4A,
        LSR_zeropage = 0x46,
        LSR_zeropage_X = 0x56,
        LSR_absolute = 0x4E,
        LSR_ABSOLUTE_x = 0X5E,

        // CMP
        CMP_immediate = 0xC9,
        CMP_zeropage = 0xC5,
        CMP_zeropage_X = 0xD5,
        CMP_absolute = 0xCD,
        CMP_absolute_x = 0xDD,
        CMP_absolute_Y = 0xD9,
        CMP_indirect_X = 0xC1,
        CMP_indirect_Y = 0xD1,

        // CPX
        CPX_immediate = 0xE0,
        CPX_zeropage = 0xE4,
        CPX_absolute = 0xEC,

        // CPY
        CPY_immediate = 0xC0,
        CPY_zeropage = 0xC4,
        CPY_absolute = 0xCC,

        // DEC
        DEC_zeropage = 0xC6,
        DEC_zeropage_X = 0xD6,
        DEC_absolute = 0xCE,
        DEC_absolute_X = 0xDE,

        // Registers Dec/Inc
        DEX = 0xCA,
        DEY = 0x88,
        INX = 0xE8,
        INY = 0xC8,

        // INC
        INC_zeropage = 0xE6,
        INC_zeropage_X = 0xF6,
        INC_absolute = 0xEE,
        INC_absolute_X = 0xFE,

        // EOR
        EOR_immediate = 0x49,
        EOR_zeropage = 0x45,
        EOR_zeropage_X = 0x55,
        EOR_absolute = 0x4D,
        EOR_absolute_x = 0x5D,
        EOR_absolute_Y = 0x59,
        EOR_indirect_X = 0x41,
        EOR_indirect_Y = 0x51,

        // ORA
        ORA_immediate = 0x09,
        ORA_zeropage = 0x05,
        ORA_zeropage_X = 0x15,
        ORA_absolute = 0x0D,
        ORA_absolute_x = 0x1D,
        ORA_absolute_Y = 0x19,
        ORA_indirect_X = 0x01,
        ORA_indirect_Y = 0x11
    }
}
