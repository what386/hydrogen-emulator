namespace Machine.Memory.Data;

public class MemoryPool
{
    public readonly int poolSize = 65536; // 64 KiB
    public readonly int blockSize = 256;
    
    private byte[] memory;

    public MemoryPool(int poolSize, int blockSize)
    {
        this.poolSize = poolSize;
        this.blockSize = blockSize;
        this.memory = new byte[poolSize];

        Array.Clear(memory, 0, poolSize);
    }
    
    public byte[] ReadBlock(int blockNumber)
    {
        byte[] block = new byte[blockSize];
        int startAddress = blockNumber * blockSize;
        Array.Copy(memory, startAddress, block, 0, blockSize);
        return block;
    }
    
    public void WriteBlock(int blockNumber, byte[] data)
    {
        if (data.Length != blockSize)
            throw new ArgumentException($"Data must be exactly {blockSize} bytes", nameof(data));
        
        int startAddress = blockNumber * blockSize;
        Array.Copy(data, 0, memory, startAddress, blockSize);
    }
    
    public byte ReadDirect(ushort address) => memory[address];
    
    public void WriteDirect(ushort address, byte value) => memory[address] = value;
    
    public void LoadData(byte[] data, ushort startAddress = 0)
    {
        int length = Math.Min(data.Length, poolSize - startAddress);
        Array.Copy(data, 0, memory, startAddress, length);
    }
    
    public void Clear() => Array.Clear(memory, 0, poolSize);
    
    public byte[] Dump() => memory;
}
