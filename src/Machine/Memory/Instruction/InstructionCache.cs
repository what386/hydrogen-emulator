namespace Machine.Memory.Instruction;

public class InstructionCache
{
    private readonly int cacheSize;
    private ushort[] cache;

    public InstructionCache(int cacheSize)
    {
        this.cacheSize = cacheSize;
        cache = new ushort[cacheSize];

        Array.Clear(cache, 0, cacheSize);
    }

    public ushort Read(int index) => cache[index];

    public void LoadBlock(ushort[] block) => cache = block;

    public void Clear() => Array.Clear(cache, 0, cacheSize);
}
