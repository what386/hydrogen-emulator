namespace Emulator.Memory.Instruction;

public class InstructionROM
{
    public const int MEMORY_SIZE = 65536; // 128 KiB
    public const int CACHE_SIZE = 64; // 64 byte
    public const int NUM_PAGES = MEMORY_SIZE / CACHE_SIZE; // 1024 pages
    
    private readonly InstructionPool pool;
    private readonly InstructionCache cache;
    private ushort activePage = 0;
    
    public ushort ActivePage => activePage;
    
    public InstructionROM()
    {
        pool = new InstructionPool(MEMORY_SIZE, CACHE_SIZE);
        cache = new InstructionCache(CACHE_SIZE);
        
        LoadPageIntoCache(0);
    }
    
    public ushort Read(ushort address)
    {
        ushort pageNumber = (ushort)(address / CACHE_SIZE);
        
        byte offset = (byte)(address % CACHE_SIZE);
        
        if (pageNumber != activePage)
        {
            LoadPageIntoCache(pageNumber);
            activePage = pageNumber;
        }
        
        return cache.Read(offset);
    }
    
    public void SetPage(ushort page)
    {
        if (page == activePage)
            return;
        
        LoadPageIntoCache(page);
        activePage = page;
    }
    
    private void LoadPageIntoCache(ushort page)
    {
        ushort[] pageData = pool.ReadBlock(page);
        cache.LoadBlock(pageData);
    }

    public void Clear()
    {
        pool.Clear();
        cache.Clear();
        activePage = 0;
    }

    public ushort[] DumpMemory() => pool.Dump();
    
    public void Flash(ushort[] data)
    {
        pool.Flash(data);
        LoadPageIntoCache(0);
        activePage = 0;
    }
}
