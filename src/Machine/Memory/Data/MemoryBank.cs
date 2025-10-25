namespace Machine.Memory.Data;

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
    
    public byte Read(byte address) => memory[address];
    
    public void Write(byte address, byte data) => memory[address] = data;
       
    public void Fill(byte startAddress, int length, byte value)
    {
        int endAddress = Math.Min(startAddress + length, bankSize);
        Array.Fill(memory, value, startAddress, endAddress - startAddress);
    }
    
    public void Clear() => Array.Clear(memory, 0, bankSize);
}



