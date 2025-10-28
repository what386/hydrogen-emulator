namespace Emulator.Registers;

using Emulator.Memory.Instruction;

public class ProgramCounter 
{
    private int programCounter = 0;

    public byte PCLow => (byte)programCounter;
    public byte PCHigh => (byte)(programCounter >> 8);

    private int offset;

    public ProgramCounter()
    {
    }

    public int Get() => programCounter;

    public int BranchOffset => programCounter % InstructionROM.CACHE_SIZE;

    // Replace only the low byte
    public void SetLow(byte value)
    {
        // Clear the low byte and insert the new value
        programCounter = (programCounter & 0xFF00) | value;
    }

    // Replace only the high byte
    public void SetHigh(byte value)
    {
        // Clear the high byte and insert the new value
        programCounter = (programCounter & 0x00FF) | (value << 8);
    }

    public void SetBranchOffset(int offset)
    {
        if (offset < 0 || offset >= InstructionROM.CACHE_SIZE)
            throw new ArgumentOutOfRangeException(nameof(offset));

        programCounter = (programCounter - BranchOffset) + offset;
    }

    public void Jump(int value, bool pageMode = false)
    {
        if (pageMode)
            programCounter = (int)(value * InstructionROM.CACHE_SIZE);
        else
            programCounter = value;
    }

    public void Add(int offset) => programCounter = programCounter + offset;

    public void Increment() => programCounter++;
    public void Decrement() => programCounter--;

    public void Reset() => programCounter = 0;


    public override string ToString()
    {
        return $"Program Counter:\n" +
               $"PC: {programCounter} (0x{programCounter:X4})\n" +
               $"PCH: {PCHigh:X2}\n" +
               $"PCL: {PCLow:X2}\n" +
               $"Branch Offset: {BranchOffset}\n";
    }
}

