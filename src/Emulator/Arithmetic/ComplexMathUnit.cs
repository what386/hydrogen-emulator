namespace Emulator.Arithmetic;

using Emulator.Registers;

// TODO:
// Make divide by zero errors trigger
// the "DIV_BY_ZERO" interrupt.

public class ComplexMathUnit
{
    private StatusWord flagsRegister;

    public ComplexMathUnit(StatusWord flagRegister)
    {
        this.flagsRegister = flagRegister;
    }

    // Multiplication operations

    public byte MultiplyLow(byte a, byte b)
    {
        int result = a * b;
        byte lowResult = (byte)(result & 0xFF);

        flagsRegister.UpdateFlags(lowResult, false, false, false);
        return lowResult;
    }

    public byte MultiplyHigh(byte a, byte b)
    {
        int result = a * b;
        byte highResult = (byte)((result >> 8) & 0xFF);

        flagsRegister.UpdateFlags(highResult, false, false, false);
        return highResult;
    }

    public (byte low, byte high) MultiplyFull(byte a, byte b)
    {
        int result = a * b;
        byte low = (byte)(result & 0xFF);
        byte high = (byte)((result >> 8) & 0xFF);

        flagsRegister.UpdateFlags(low, false, false, false);
        return (low, high);
    }

    // Division operations

    public byte Divide(byte dividend, byte divisor)
    {
        if (divisor == 0)
        {
            flagsRegister.SetError(true);
        }

        byte result = (byte)(dividend / divisor);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }

    public byte Modulo(byte dividend, byte divisor)
    {
        if (divisor == 0)
        {
            flagsRegister.SetError(true);
        }

        byte result = (byte)(dividend % divisor);
        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }

    public (byte quotient, byte remainder) DivideWithRemainder(byte dividend, byte divisor)
    {
        if (divisor == 0)
        {
            flagsRegister.SetError(true);
        }

        byte quotient = (byte)(dividend / divisor);
        byte remainder = (byte)(dividend % divisor);

        flagsRegister.UpdateFlags(quotient, false, false, false);
        return (quotient, remainder);
    }

    // Advanced mathematical operations

    public byte SquareRoot(byte value)
    {
        if (value == 0)
        {
            flagsRegister.UpdateFlags(0, false, false, false);
            return 0;
        }

        byte result = 0;
        byte bit = 0x80;

        while (bit > result)
        {
            byte temp = (byte)(result | bit);
            if (temp * temp <= value)
                result = temp;
            bit >>= 1;
        }

        flagsRegister.UpdateFlags(result, false, false, false);
        return result;
    }

    // Bit counting

    public byte CountLeadingZeros(byte value)
    {
        if (value == 0)
        {
            flagsRegister.UpdateFlags(8, false, false, false);
            return 8;
        }

        byte count = 0;
        for (int i = 7; i >= 0; i--)
        {
            if ((value & (1 << i)) != 0)
                break;
            count++;
        }

        flagsRegister.UpdateFlags(count, false, false, false);
        return count;
    }

    public byte CountTrailingZeros(byte value)
    {
        if (value == 0)
        {
            flagsRegister.UpdateFlags(8, false, false, false);
            return 8;
        }

        byte count = 0;
        for (int i = 0; i < 8; i++)
        {
            if ((value & (1 << i)) != 0)
                break;
            count++;
        }

        flagsRegister.UpdateFlags(count, false, false, false);
        return count;
    }

    public byte CountOnes(byte value)
    {
        byte count = 0;
        byte temp = value;

        while (temp != 0)
        {
            count++;
            temp &= (byte)(temp - 1);
        }

        flagsRegister.UpdateFlags(count, false, false, false);
        return count;
    }
}
