namespace Emulator.Memory.Data;

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
    
    public byte ReadDirect(int address) => memory[address];
    
    public void WriteDirect(int address, byte value) => memory[address] = value;
    
    public void LoadData(byte[] data, int startAddress = 0)
    {
        int length = Math.Min(data.Length, poolSize - startAddress);
        Array.Copy(data, 0, memory, startAddress, length);
    }
    
    public void Clear() => Array.Clear(memory, 0, poolSize);
    
    public byte[] Dump() => memory;

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Memory Pool ===");
        sb.AppendLine($"Pool Size: {poolSize} bytes");
        sb.AppendLine($"Block Size: {blockSize} bytes");
        sb.AppendLine($"Blocks: {poolSize / blockSize}");
        sb.AppendLine();

        int displayLength = Math.Clamp(128, 0, poolSize);

        for (int i = 0; i < displayLength; i += 16)
        {
            sb.Append($"{i:X4}: ");
            for (int j = 0; j < 16 && i + j < displayLength; j++)
                sb.Append($"{memory[i + j]:X2} ");
            sb.AppendLine();
        }

        if (displayLength < poolSize)
            sb.AppendLine($"... ({poolSize - displayLength} bytes hidden)");

        return sb.ToString();
    }
}
