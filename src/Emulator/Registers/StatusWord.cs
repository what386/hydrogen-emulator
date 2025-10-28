namespace Emulator.Registers;

public class StatusWord
{
    public const byte DEFAULTS = 0b00000000;
    public byte Flags = DEFAULTS;

    public bool AlternateConditions = false;

    // Bits 1 and 3 are reserved
    public const byte CARRY_FLAG = 0b00000001;
    public const byte ERROR_FLAG = 0b00000010;
    public const byte PARITY_FLAG = 0b00000100;
    public const byte RESERVED3 = 0b00001000;
    public const byte AUX_CARRY_FLAG = 0b00010000;
    public const byte OVERFLOW_FLAG = 0b00100000;
    public const byte ZERO_FLAG = 0b01000000;
    public const byte SIGN_FLAG = 0b10000000;
    
    public void Clear() => Flags = DEFAULTS;

    private void SetFlag(byte flagMask, bool value)
    {
        if (value)
            Flags |= flagMask;
        else
            Flags &= (byte)~flagMask;
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
        return (Flags & flagMask) != 0;
    }

    public bool CheckCondition(int index, bool alt)
    {
        return alt ? CheckAltCondition(index) : CheckNormalCondition(index);
    }

    private bool CheckNormalCondition(int index)
    {
        bool Z = GetFlag(ZERO_FLAG);
        bool C = GetFlag(CARRY_FLAG);
        bool E = GetFlag(PARITY_FLAG);  // Even/Parity flag
        bool V = GetFlag(OVERFLOW_FLAG);
        bool N = GetFlag(SIGN_FLAG);    // Negative/Sign flag
        
        return index switch
        {
            0 => V,                    // 000: Overflow (V)
            1 => !V,                   // 001: No Overflow (!V)
            2 => N != V,               // 010: Less (N!=V)
            3 => (N == V) && !Z,       // 011: Greater (N=V AND !Z)
            4 => (N != V) || Z,        // 100: Less Equal (N!=V OR Z)
            5 => N == V,               // 101: Greater Equal (N=V)
            6 => !E,                   // 110: Odd (!E)
            7 => true,                 // 111: Always
            _ => throw new ArgumentOutOfRangeException(nameof(index), "Condition index must be 0-7")
        };
    }
    
    private bool CheckAltCondition(int index)
    {
        bool Z = GetFlag(ZERO_FLAG);
        bool C = GetFlag(CARRY_FLAG);
        bool E = GetFlag(PARITY_FLAG);  // Even/Parity flag
        bool V = GetFlag(OVERFLOW_FLAG);
        bool N = GetFlag(SIGN_FLAG);    // Negative/Sign flag

        return index switch
        {
            0 => Z,                    // 000: Equal / Zero (Z)
            1 => !Z,                   // 001: Not Equal / Not Zero (!Z)
            2 => !C,                   // 010: Lower / No Carry (!C)
            3 => C && !Z,              // 011: Higher (C AND !Z)
            4 => !C || Z,              // 100: Lower Same (!C OR Z)
            5 => C,                    // 101: Higher Same / Carry (C)
            6 => E,                    // 110: Even (E)
            7 => true,                 // 111: Always
            _ => throw new ArgumentOutOfRangeException(nameof(index), "Condition index must be 0-7")
        };
    }
}
