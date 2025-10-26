namespace Emulator.Registers;

public class StatusWord
{
    public const byte DEFAULTS = 0b00000000;
    private byte flags;

    public byte Flags => flags;

    // Bits 1 and 3 are reserved
    public const byte CARRY_FLAG = 0b00000001;
    public const byte ERROR_FLAG = 0b00000010;
    public const byte PARITY_FLAG = 0b00000100;
    public const byte RESERVED3 = 0b00001000;
    public const byte AUX_CARRY_FLAG = 0b00010000;
    public const byte OVERFLOW_FLAG = 0b00100000;
    public const byte ZERO_FLAG = 0b01000000;
    public const byte SIGN_FLAG = 0b10000000;
    
    public void ClearFlags() => flags = DEFAULTS;

    private void SetFlag(byte flagMask, bool value)
    {
        if (value)
            flags |= flagMask;
        else
            flags &= (byte)~flagMask;
    }

    public void SetError(bool value) => SetFlag(ERROR_FLAG, value);

    public bool CalculateParity(byte value)
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            if ((value & (1 << i)) != 0)
                count++;
        }
        return (count % 2) == 0;
    }

    public void UpdateFlags(byte result, bool carry, bool auxCarry, bool overflow)
    {
        SetFlag(ZERO_FLAG, result == 0);
        SetFlag(SIGN_FLAG, (result & 0x80) != 0);  // Check bit 7 for sign
        SetFlag(PARITY_FLAG, CalculateParity(result));
        SetFlag(CARRY_FLAG, carry);
        SetFlag(AUX_CARRY_FLAG, auxCarry);
        SetFlag(OVERFLOW_FLAG, overflow);
    }

    public bool GetFlag(byte flagMask)
    {
        return (flags & flagMask) != 0;
    }
}
