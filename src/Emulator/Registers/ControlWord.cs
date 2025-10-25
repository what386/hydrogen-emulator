namespace Emulator.Registers;

public class ControlWord
{
    // Flag register (8 bits)
    public const byte DEFAULTS = 0b00000000;
    public byte Flags { get; private set; }

    // Bit 4 is reserved
    public const byte ALT_CONDITIONS = 0b00000001;
    public const byte PAGE_JUMP_MODE = 0b00000010;
    public const byte AUTO_INCREMENT = 0b00000100;
    public const byte INTERRUPT_ENABLE = 0b00001000;
    public const byte RESERVED4 = 0b00010000;
    public const byte HALT_FLAG = 0b00100000;
    public const byte DEBUG_MODE = 0b01000000;
    public const byte KERNEL_MODE = 0b10000000;

    public void ClearFlags() => Flags = 0;

    private void SetFlag(byte flagMask, bool value)
    {
        if (value)
            Flags |= flagMask;
        else
            Flags &= (byte)~flagMask;
    }
}
