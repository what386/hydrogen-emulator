namespace Emulator.Memory.Data;

public class MainMemory
{
    public const int MEMORY_SIZE = 65536;
    public const int BANK_SIZE = 256;
    
    private readonly MemoryPool pool;
    private readonly MemoryBank bank;

    private int activePage = 0;
    private ushort stackPointer = MEMORY_SIZE - 1;
    
    public int ActivePage => activePage;
    public int StackPointer => stackPointer;
    
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
        SetPage(stackPointer / BANK_SIZE);
        bank.Write((byte)(stackPointer % BANK_SIZE), data);
        bank.isDirty = true;

        // Stack grows downward
        stackPointer = (ushort)(stackPointer - 1);
    }

    public byte Pop()
    {
        // Stack grows downward
        stackPointer = (ushort)(stackPointer + 1);

        SetPage(stackPointer / BANK_SIZE);
        return bank.Read((byte)(stackPointer % BANK_SIZE));
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
