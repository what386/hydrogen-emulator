namespace Emulator.Memory.Data;

public class MainMemory
{
    public const int MEMORY_SIZE = 65536; // 64 KiB
    public const int BANK_SIZE = 256; // 256 bytes

    private readonly MemoryPool pool;
    private readonly MemoryBank bank;

    private int activePage = 0;

    // Stack grows downward
    private ushort virtualStackPointer = 0;
    private ushort physicalStackPointer => (ushort)(ushort.MaxValue - virtualStackPointer);
    
    public int ActivePage => activePage;
    public int StackPointer => virtualStackPointer;
    
    public MainMemory()
    {
        pool = new MemoryPool(MEMORY_SIZE, BANK_SIZE);
        bank = new MemoryBank(BANK_SIZE);
    }
    
    public byte Read(byte address)
    {
        return bank.Read(address);
    }
    
    public void Write(byte address, byte data)
    {
        bank.Write(address, data);
        bank.isDirty = true; // Mark bank as modified
    }

    public void Push(byte data)
    {
        SetPage(physicalStackPointer / BANK_SIZE);
        bank.Write((byte)(physicalStackPointer % BANK_SIZE), data);
        bank.isDirty = true;

        virtualStackPointer = (ushort)(virtualStackPointer + 1);
    }

    public byte Pop()
    {
        virtualStackPointer = (ushort)(virtualStackPointer - 1);

        SetPage(physicalStackPointer / BANK_SIZE);
        return bank.Read((byte)(physicalStackPointer % BANK_SIZE));
    }

    public void SetStackPointer(byte data)
    {
        virtualStackPointer = (ushort)(MEMORY_SIZE - data);
    }
    
    
    public void SetPage(int page)
    {
        if (page == activePage)
            return;
        
        if (bank.isDirty)
            WritebackCurrentBank();
        
        LoadPageIntoBank(page);
        
        activePage = page;
    }

    private void WritebackCurrentBank()
    {
        byte[] bankData = bank.DumpBlock();
        pool.WriteBlock(activePage, bankData);
        bank.isDirty = false;
    }

    private void LoadPageIntoBank(int page)
    {
        byte[] pageData = pool.ReadBlock(page);
        bank.LoadBlock(pageData);
        bank.isDirty = false;
    }
    
    public void Clear()
    {
        pool.Clear();
        bank.Clear();
        bank.isDirty = false;
        bank.isInvalid = false;
        activePage = 0;
    }

    public byte[] DumpMemory()
    {
        if (bank.isDirty)
            WritebackCurrentBank();
        
        return pool.Dump();
    }
    
    public void LoadData(byte[] data, byte startAddress = 0)
    {
        pool.LoadData(data, startAddress);
        LoadPageIntoBank(0);
    }
}
