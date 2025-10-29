namespace Emulator.Memory.Data;

using Emulator.Registers;

public class MainMemory
{
    public const int MEMORY_SIZE = 65536; // 64 KiB
    public const int BANK_SIZE = 256; // 256 bytes

    private readonly MemoryPool pool;
    private readonly MemoryBank bank;
    private StatusWord statusRegister;

    private int activePage = 0;
    public int AddressPointer = 0;

    // Stack grows downward
    private int virtualStackPointer = 0;
    private int physicalStackPointer => (ushort.MaxValue - virtualStackPointer);
    
    public int ActivePage => activePage;
    public int StackPointer => virtualStackPointer;
    
    public MainMemory(StatusWord statusRegister)
    {
        pool = new MemoryPool(MEMORY_SIZE, BANK_SIZE);
        bank = new MemoryBank(BANK_SIZE);
        this.statusRegister = statusRegister;
    }

    public byte ReadPool(int address)
    {
        return pool.ReadDirect(address);
    }

    public byte ReadBank(int address)
    {
        return pool.ReadDirect(address);
    }

    public void WritePool(int address, byte data)
    {
        pool.WriteDirect(address, data);
    }

    public void WriteBank(int address, byte data)
    {
        pool.WriteDirect(address, data);
    }

    
    public byte Read(int address)
    {
        return bank.Read(address + AddressPointer);
    }
    
    public void Write(int address, byte data)
    {
        bank.Write(address + AddressPointer, data);
        bank.isDirty = true; // Mark bank as modified
    }

    public void Push(byte data, int offset)
    {
        if (virtualStackPointer >= MEMORY_SIZE)
        {
            statusRegister.SetError(true);
            return;
        }
        
        SetPage(physicalStackPointer / BANK_SIZE);
        bank.Write((physicalStackPointer % BANK_SIZE) - offset, data);
        bank.isDirty = true;
        virtualStackPointer = virtualStackPointer + 1;
    }
    
    public byte Pop(int offset)
    {
        if (virtualStackPointer == 0)
        {
            statusRegister.SetError(true);
            return 0;
        }
        
        virtualStackPointer = virtualStackPointer - 1;
        SetPage(physicalStackPointer / BANK_SIZE);
        return bank.Read((physicalStackPointer % BANK_SIZE) - offset);
    }
    
    public void Poke(byte data, int offset)
    {
        if (virtualStackPointer == 0 || offset >= virtualStackPointer)
        {
            statusRegister.SetError(true);
            return;
        }
        
        int topElement = (physicalStackPointer + 1);
        SetPage(topElement / BANK_SIZE);
        bank.Write((topElement % BANK_SIZE) - offset, data);
        bank.isDirty = true;
    }
    
    public byte Peek(int offset)
    {
        if (virtualStackPointer == 0 || offset >= virtualStackPointer)
        {
            statusRegister.SetError(true);
            return 0;
        }
        
        int topElement = (physicalStackPointer + 1);
        SetPage(topElement / BANK_SIZE);
        return bank.Read((topElement % BANK_SIZE) - offset);
    } 

    public void SetStackPointer(byte data)
    {
        virtualStackPointer = data;
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
        byte [] bankData = bank.DumpBlock();
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

    public void LoadData(byte[] data, int startAddress = 0)
    {
        pool.LoadData(data, startAddress);
        LoadPageIntoBank(0);
    }
}
