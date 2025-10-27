namespace Emulator.Registers;

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
        if (index == 0 && registers[0] != 0)
        {
            byte temp = registers[0];
            registers[0] = 0;
            return temp;
        }
        
        return registers[index];
    }

    public void Write(int index, byte data)
    {
        if (index == 0) 
            return; // Ignore writes

        registers[index] = data;
    }

    public void WriteDirect(int index, byte value) => registers[index] = value;

    public void Clear() => Array.Clear(registers, 0, SIZE);
}
