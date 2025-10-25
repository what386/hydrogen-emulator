namespace Machine.Registers;

using System;

public class RegisterFile 
{
    private const int SIZE = 8;
    private byte[] registers = new byte[SIZE];

    public RegisterFile()
    {
        Array.Clear(registers, 0, SIZE);
    }

    public byte Read(int index)
    {
        return registers[index];
    }

    public void Write(int index, byte data)
    {
        if (index == 0) 
            return; // Ignore writes

        registers[index] = data;
    }

    public void SetR0(byte value)
    {
        registers[0] = value;
    }

    public void Clear() => Array.Clear(registers, 0, SIZE);
}
