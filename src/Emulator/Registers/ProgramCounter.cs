namespace Emulator.Registers;

public class ProgramCounter 
{
    private ushort programCounter = 0;

    private byte pcLow => (byte)programCounter;
    private byte pcHigh => (byte)(programCounter >> 8);

    private int offset;

    public ProgramCounter()
    {
    }

    public ushort Get() => programCounter;
    public byte Low() => pcLow;
    public byte High() => pcHigh;

    public void Jump(ushort value) => programCounter = value;

    public void Add(short offset) => programCounter = (ushort)(programCounter + offset);

    public void Increment() => programCounter++;
    public void Decrement() => programCounter--;

    public void Reset() => programCounter = 0;
}

