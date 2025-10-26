namespace Emulator.Arithmetic;

using Emulator.Registers;

public class BitShiftUnit
{
    private StatusWord flagsRegister;

    public BitShiftUnit(StatusWord flagRegister)
    {
        this.flagsRegister = flagRegister;
    }

    // Left shift operations

    public byte ShiftLeftLogical(byte value, byte positions)
    {
        if (positions < 0 || positions > 7)
            throw new ArgumentOutOfRangeException(nameof(positions), "Shift positions must be between 0 and 7");

        if (positions == 0)
            return value;

        bool carry = false;
        byte result = value;

        for (int i = 0; i < positions; i++)
        {
            carry = (result & 0x80) != 0;  // Capture bit 7 before shifting
            result = (byte)(result << 1);
        }

        flagsRegister.UpdateFlags(result, carry, false, false);
        return result;
    }

    // Right shift operations

    public byte ShiftRightLogical(byte value, byte positions)
    {
        if (positions < 0 || positions > 7)
            throw new ArgumentOutOfRangeException(nameof(positions), "Shift positions must be between 0 and 7");

        if (positions == 0)
            return value;

        bool carry = false;
        byte result = value;

        for (int i = 0; i < positions; i++)
        {
            carry = (result & 0x01) != 0;  // Capture bit 0 before shifting
            result = (byte)(result >> 1);
        }

        flagsRegister.UpdateFlags(result, carry, false, false);
        return result;
    }

    public byte ShiftRightArithmetic(byte value, byte positions)
    {
        if (positions < 0 || positions > 7)
            throw new ArgumentOutOfRangeException(nameof(positions), "Shift positions must be between 0 and 7");

        if (positions == 0)
            return value;

        bool carry = false;
        byte result = value;
        bool signBit = (value & 0x80) != 0;  // Preserve sign bit

        for (int i = 0; i < positions; i++)
        {
            carry = (result & 0x01) != 0;  // Capture bit 0 before shifting
            result = (byte)(result >> 1);
            if (signBit)
                result |= 0x80;  // Fill with sign bit
        }

        flagsRegister.UpdateFlags(result, carry, false, false);
        return result;
    }

    // Rotate operations

    public byte RotateLeft(byte value, byte positions)
    {
        if (positions < 0 || positions > 7)
            throw new ArgumentOutOfRangeException(nameof(positions), "Rotate positions must be between 0 and 7");

        if (positions == 0)
            return value;
        
        byte result = (byte)((value << positions) | (value >> (8 - positions)));
        
        // Carry flag gets the last bit that was rotated out (bit 7 of original after positions-1 rotations)
        bool carry = ((value >> (8 - positions)) & 0x01) != 0; // Same bit, before rotation
        
        flagsRegister.UpdateFlags(result, carry, false, false);
        return result;
    }

    public byte RotateRight(byte value, byte positions)
    {
        if (positions < 0 || positions > 7)
            throw new ArgumentOutOfRangeException(nameof(positions), "Rotate positions must be between 0 and 7");

        if (positions == 0)
            return value;

        byte result = (byte)((value >> positions) | (value << (8 - positions)));
        
        // Carry flag gets the last bit that was rotated out (bit 0 of original after positions-1 rotations)
        bool carry = positions > 0 ? ((value >> (positions - 1)) & 0x01) != 0 : false;
        
        flagsRegister.UpdateFlags(result, carry, false, false);
        return result;
    }

    // Rotate through carry operations (9-bit rotations including carry flag)

    public byte RotateLeftThroughCarry(byte value, byte positions)
    {
        if (positions < 0 || positions > 7)
            throw new ArgumentOutOfRangeException(nameof(positions), "Rotate positions must be between 0 and 7");

        if (positions == 0)
            return value;

        bool carry = flagsRegister.GetFlag(StatusWord.CARRY_FLAG);
        byte result = value;

        for (int i = 0; i < positions; i++)
        {
            bool newCarry = (result & 0x80) != 0;
            result = (byte)((result << 1) | (carry ? 1 : 0));
            carry = newCarry;
        }

        flagsRegister.UpdateFlags(result, carry, false, false);
        return result;
    }

    public byte RotateRightThroughCarry(byte value, byte positions)
    {
        if (positions < 0 || positions > 7)
            throw new ArgumentOutOfRangeException(nameof(positions), "Rotate positions must be between 0 and 7");

        if (positions == 0)
            return value;

        bool carry = flagsRegister.GetFlag(StatusWord.CARRY_FLAG);
        byte result = value;

        for (int i = 0; i < positions; i++)
        {
            bool newCarry = (result & 0x01) != 0;
            result = (byte)((result >> 1) | (carry ? 0x80 : 0));
            carry = newCarry;
        }

        flagsRegister.UpdateFlags(result, carry, false, false);
        return result;
    }
}
