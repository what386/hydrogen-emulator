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

    public byte Not(byte a)
    {
        byte result = (byte)~a;
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }

    public byte Implies(byte a, byte b)
    {
        byte result = (byte)(~a | b);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }
}

