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

        return sb.ToString();
    }
}
