namespace Machine.Memory.Instruction;

public class InstructionPool
{
    public readonly int poolSize;
    public readonly int blockSize;
    
    private ushort[] instructions;

    public InstructionPool(int poolSize, int blockSize)
    {
        this.poolSize = poolSize;
        this.blockSize = blockSize;
        this.instructions = new ushort[poolSize];

        Array.Clear(instructions, 0, poolSize);
    }
    
    public ushort[] ReadBlock(ushort blockNumber)
    {
        ushort[] block = new ushort[blockSize];
        int startAddress = blockNumber * blockSize;
        Array.Copy(instructions, startAddress, block, 0, blockSize);
        return block;
    }
       
    public ushort ReadDirect(ushort address) => instructions[address];
    
    public void Flash(ushort[] data)
    {
        if (data.Length != instructions.Length)
            throw new ArgumentException();

        Array.Copy(data, 0, instructions, 0, poolSize);
    }
    
    public void Clear() => Array.Clear(instructions, 0, poolSize);
    
    public ushort[] Dump() => instructions;
}


