namespace Emulator.Registers;

using System;

public class InterruptVector 
{
    private const int SIZE = 16;
    private int[] interrupts = new int[SIZE];

    public byte InterruptMask = 0b11111111;

    public Stack<(byte priority, byte vector)> pendingInterrupts = new();
    public Stack<(byte priority, byte vector)> activeInterrupts = new();

    public InterruptVector()
    {
        Array.Clear(interrupts, 0, SIZE);
    }

    public int GetAddress(int index) => interrupts[index];

    public void SetAddress(int index, int address) => interrupts[index] = address;

    public void RequestInterrupt(byte vector, byte priority){

        pendingInterrupts.Push((vector, priority));
    }

    public void Clear() => Array.Clear(interrupts, 0, SIZE);
}
