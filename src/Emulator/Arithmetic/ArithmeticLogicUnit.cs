namespace Emulator.Arithmetic;

using Emulator.Registers;

public class ArithmeticLogicUnit
{
    private StatusWord flagsRegister;

    public ArithmeticLogicUnit(StatusWord flagsRegister)
    {
        this.flagsRegister = flagsRegister;
    }

    // ---Arithmetic---
    public byte Add(byte a, byte b)
    {
        int result = a + b;
        byte byteResult = (byte)result;
        
        bool carry = result > 255;
        bool auxCarry = ((a & 0x0F) + (b & 0x0F)) > 0x0F;
        
        // Check for signed overflow (two positive numbers yielding negative, or two negative yielding positive)
        bool overflow = ((a & 0x80) == (b & 0x80)) && ((a & 0x80) != (byteResult & 0x80));
        
        flagsRegister.UpdateFlags(byteResult, carry, auxCarry, overflow);
        return byteResult;
    }

    public byte AddCarry(byte a, byte b)
    {
        byte carryIn = Convert.ToByte(flagsRegister.GetFlag(StatusWord.CARRY_FLAG));
        int result = a + b;
        byte byteResult = (byte)(result + carryIn);
        
        bool carry = result > 255;
        bool auxCarry = ((a & 0x0F) + (b & 0x0F)) > 0x0F;
        
        // Check for signed overflow (two positive numbers yielding negative, or two negative yielding positive)
        bool overflow = ((a & 0x80) == (b & 0x80)) && ((a & 0x80) != (byteResult & 0x80));
        
        flagsRegister.UpdateFlags(byteResult, carry, auxCarry, overflow);
        return byteResult;
    }

    public byte AddVector(byte a, byte b)
    {
        // Add low nibbles (bits 0-3) separately
        byte lowNibble = (byte)((a & 0x0F) + (b & 0x0F));
        
        // Add high nibbles (bits 4-7) separately
        byte highNibble = (byte)(((a & 0xF0) + (b & 0xF0)) & 0xF0);
        
        // Combine nibbles (carry from low nibble is discarded)
        byte byteResult = (byte)((lowNibble & 0x0F) | highNibble);
        
        bool carry = ((a & 0xF0) + (b & 0xF0)) > 0xF0;
        bool auxCarry = ((a & 0x0F) + (b & 0x0F)) > 0x0F;
        
        // Check for signed overflow in the high nibble only
        bool overflow = ((a & 0x80) == (b & 0x80)) && ((a & 0x80) != (byteResult & 0x80));
        
        flagsRegister.UpdateFlags(byteResult, carry, auxCarry, overflow);
        return byteResult;
    }

    public byte AddVectorCarry(byte a, byte b)
    {
        byte carryIn = Convert.ToByte(flagsRegister.GetFlag(StatusWord.CARRY_FLAG));
        byte auxCarryIn = Convert.ToByte(flagsRegister.GetFlag(StatusWord.AUX_CARRY_FLAG));
        
        // Add low nibbles (bits 0-3) with aux carry
        int lowSum = (a & 0x0F) + (b & 0x0F) + auxCarryIn;
        byte lowNibble = (byte)(lowSum & 0x0F);
        
        // Add high nibbles (bits 4-7) with carry
        int highSum = (a & 0xF0) + (b & 0xF0) + (carryIn << 4);
        byte highNibble = (byte)(highSum & 0xF0);
        
        // Combine nibbles (carry from low nibble is discarded)
        byte byteResult = (byte)(lowNibble | highNibble);
        
        bool carry = highSum > 0xF0;
        bool auxCarry = lowSum > 0x0F;
        
        // Check for signed overflow in the high nibble only
        bool overflow = ((a & 0x80) == (b & 0x80)) && ((a & 0x80) != (byteResult & 0x80));
        
        flagsRegister.UpdateFlags(byteResult, carry, auxCarry, overflow);
        return byteResult;
    }

    public byte Sub(byte a, byte b)
    {
        int result = a - b;
        byte byteResult = (byte)result;
        
        bool carry = result < 0;  // Borrow occurred
        bool auxCarry = (a & 0x0F) < (b & 0x0F);
        
        // Check for signed overflow
        bool overflow = ((a & 0x80) != (b & 0x80)) && ((a & 0x80) != (byteResult & 0x80));
        
        flagsRegister.UpdateFlags(byteResult, carry, auxCarry, overflow);
        return byteResult;
    }

    public byte SubBorrow(byte a, byte b)
    {
        byte carryIn = Convert.ToByte(flagsRegister.GetFlag(StatusWord.CARRY_FLAG));
        int result = a - b;
        byte byteResult = (byte)(result - carryIn);
        
        bool carry = result < 0;  // Borrow occurred
        bool auxCarry = (a & 0x0F) < (b & 0x0F);
        
        // Check for signed overflow
        bool overflow = ((a & 0x80) != (b & 0x80)) && ((a & 0x80) != (byteResult & 0x80));
        
        flagsRegister.UpdateFlags(byteResult, carry, auxCarry, overflow);
        return byteResult;
    }

    public byte SubVector(byte a, byte b)
    {
        // Subtract low nibbles (bits 0-3) separately
        int lowDiff = (a & 0x0F) - (b & 0x0F);
        byte lowNibble = (byte)(lowDiff & 0x0F);
        
        // Subtract high nibbles (bits 4-7) separately
        int highDiff = (a & 0xF0) - (b & 0xF0);
        byte highNibble = (byte)(highDiff & 0xF0);
        
        // Combine nibbles (borrow from low nibble is discarded)
        byte byteResult = (byte)(lowNibble | highNibble);
        
        bool carry = highDiff < 0;  // Borrow occurred in high nibble
        bool auxCarry = (a & 0x0F) < (b & 0x0F);  // Borrow occurred in low nibble
        
        // Check for signed overflow in the high nibble only
        bool overflow = ((a & 0x80) != (b & 0x80)) && ((a & 0x80) != (byteResult & 0x80));
        
        flagsRegister.UpdateFlags(byteResult, carry, auxCarry, overflow);
        return byteResult;
    }

    public byte SubVectorBorrow(byte a, byte b)
    {
        byte carryIn = Convert.ToByte(flagsRegister.GetFlag(StatusWord.CARRY_FLAG));
        byte auxCarryIn = Convert.ToByte(flagsRegister.GetFlag(StatusWord.AUX_CARRY_FLAG));
        
        // Subtract low nibbles (bits 0-3) with aux carry (borrow)
        int lowDiff = (a & 0x0F) - (b & 0x0F) - auxCarryIn;
        byte lowNibble = (byte)(lowDiff & 0x0F);
        
        // Subtract high nibbles (bits 4-7) with carry (borrow)
        int highDiff = (a & 0xF0) - (b & 0xF0) - (carryIn << 4);
        byte highNibble = (byte)(highDiff & 0xF0);
        
        // Combine nibbles (borrow from low nibble is discarded)
        byte byteResult = (byte)(lowNibble | highNibble);
        
        bool carry = highDiff < 0;  // Borrow occurred in high nibble
        bool auxCarry = lowDiff < 0;  // Borrow occurred in low nibble
        
        // Check for signed overflow in the high nibble only
        bool overflow = ((a & 0x80) != (b & 0x80)) && ((a & 0x80) != (byteResult & 0x80));
        
        flagsRegister.UpdateFlags(byteResult, carry, auxCarry, overflow);
        return byteResult;
    }

    public byte Increment(byte a)
    {
        int result = a + 1;
        byte byteResult = (byte)result;
        
        bool auxCarry = (a & 0x0F) == 0x0F;
        bool overflow = a == 0x7F;  // Incrementing max positive signed value
       
        // Increment does not affect the carry flag
        bool carry = flagsRegister.GetFlag(StatusWord.CARRY_FLAG);

        flagsRegister.UpdateFlags(byteResult, carry, auxCarry, overflow);
        return byteResult;
    }

    public byte Decrement(byte a)
    {
        int result = a - 1;
        byte byteResult = (byte)result;
        
        bool auxCarry = (a & 0x0F) == 0x00;
        bool overflow = a == 0x80;  // Decrementing max negative signed value
        
        // Decrement does not affect the carry flag
        bool carry = flagsRegister.GetFlag(StatusWord.CARRY_FLAG);

        flagsRegister.UpdateFlags(byteResult, carry, auxCarry, overflow);
        return byteResult;
    }

    // Bitwise operations
    // Logical operations clear carry, aux carry, and overflow


    public byte And(byte a, byte b)
    {
        byte result = (byte)(a & b);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }

    public byte Or(byte a, byte b)
    {
        byte result = (byte)(a | b);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }

    public byte Xor(byte a, byte b)
    {
        byte result = (byte)(a ^ b);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }


    public byte Implies(byte a, byte b)
    {
        byte result = (byte)(~a | b);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }

    public byte Nand(byte a, byte b)
    {
        byte result = (byte)~(a & b);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }

    public byte Nor(byte a, byte b)
    {
        byte result = (byte)~(a | b);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }

    public byte Xnor(byte a, byte b)
    {
        byte result = (byte)~(a ^ b);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }

    public byte Nimplies(byte a, byte b)
    {
        byte result = (byte)(a & ~b);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }
}
