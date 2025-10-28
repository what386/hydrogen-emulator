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

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Interrupt Vector:");
        sb.AppendLine($"Interrupt Mask: 0b{Convert.ToString(InterruptMask, 2).PadLeft(8, '0')}");
        sb.AppendLine("Entries:");
        
        for (int i = 0; i < interrupts.Length; i += 8)
        {
            sb.Append($"{i:D2}-{i + 7:D2}: ");
            for (int j = 0; j < 8 && i + j < interrupts.Length; j++)
                sb.Append($"{interrupts[i + j]:X2} ");
            sb.AppendLine();
        }

        sb.AppendLine("Pending interrupts:");
        for (int i = 0; i < pendingInterrupts.Count; i += 8)
        {
            var (vector, priority) = pendingInterrupts.ElementAt(i);
            sb.Append($"Position {i}: vector: {vector}, priority {priority}");
            sb.AppendLine();
        }

        sb.AppendLine("Active interrupts:");
        for (int i = 0; i < activeInterrupts.Count; i += 8)
        {
            var (vector, priority) = activeInterrupts.ElementAt(i);
            sb.Append($"Position {i}: vector: {vector}, priority {priority}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
