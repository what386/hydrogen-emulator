namespace Emulator.Memory.Data;

public class MemoryBank
{
    public readonly int bankSize = 256; // 256 bytes
    
    private byte[] memory;

    public bool isDirty;
    public bool isInvalid;
    
    public MemoryBank(int bankSize)
    {
        this.bankSize = bankSize;
        this.memory = new byte[bankSize];

        Array.Clear(memory, 0, bankSize);
        isDirty = false;
        isInvalid = false;
    }
    
    public void LoadBlock(byte[] block)
    {
        if (block.Length != bankSize)
            throw new ArgumentException($"Block must be exactly {bankSize} bytes", nameof(block));
        
        Array.Copy(block, 0, memory, 0, bankSize);
    }
    
    public byte[] DumpBlock() => memory;
    
    public byte Read(int address) => memory[address];
    
    public void Write(int address, byte data) => memory[address] = data;
       
    public void Fill(int startAddress, int length, byte value)
    {
        int endAddress = Math.Min(startAddress + length, bankSize);
        Array.Fill(memory, value, startAddress, endAddress - startAddress);
    }
    
    public void Clear() => Array.Clear(memory, 0, bankSize);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Memory Bank:");
        sb.AppendLine($"Size: {bankSize} bytes");
        sb.AppendLine($"Dirty: {isDirty}, Invalid: {isInvalid}");
        sb.AppendLine();

        // Show the first 64 bytes as a preview
        int previewLength = Math.Min(64, bankSize);
        for (int i = 0; i < previewLength; i += 16)
        {
            sb.Append($"{i:X4}: ");
            for (int j = 0; j < 16 && i + j < previewLength; j++)
            {
                sb.Append($"{memory[i + j]:X2} ");
            }
            sb.AppendLine();
        }

        if (bankSize > previewLength)
            sb.AppendLine($"... ({bankSize - previewLength} bytes hidden)");

        return sb.ToString();
    }
}
