namespace Emulator.Registers;

using System;

public class InterruptVector 
{
    private const int SIZE = 32;
    private byte[] interrupts = new byte[SIZE];

    public byte InterruptMask = 0b11111111;

    public InterruptVector()
    {
        Array.Clear(interrupts, 0, SIZE);
    }

    public byte GetAddress(int index)
    {
        return interrupts[index];
    }

    public void SetAddress(int index, byte data)
    {
        interrupts[index] = data;
    }

    public void Clear() => Array.Clear(interrupts, 0, SIZE);
}
