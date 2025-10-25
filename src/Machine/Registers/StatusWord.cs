namespace Machine.Registers;

public class StatusWord
{
    // Flag register (8 bits)
    public const byte DEFAULTS = 0b00000000;
    public byte Flags { get; private set; }

    // Bits 1 and 3 are reserved
    public const byte CARRY_FLAG = 0b00000001;
    public const byte RESERVED1 = 0b00000010;
    public const byte PARITY_FLAG = 0b00000100;
    public const byte RESERVED3 = 0b00001000;
    public const byte AUX_CARRY_FLAG = 0b00010000;
    public const byte OVERFLOW_FLAG = 0b00100000;
    public const byte ZERO_FLAG = 0b01000000;
    public const byte SIGN_FLAG = 0b10000000;

    public void ClearFlags() => Flags = 0;

    private void SetFlag(byte flagMask, bool value)
    {
        if (value)
            Flags |= flagMask;
        else
            Flags &= (byte)~flagMask;
    }
}
