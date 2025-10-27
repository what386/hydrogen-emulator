namespace Emulator.IO.Devices;

using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Simple disk interface
/// 
/// PORT MAP (requires 6 consecutive ports):
/// Offset 0: COMMAND - Write operation code
/// Offset 1: STATUS - Read operation status
/// Offset 2: BLOCK_LO - Low byte of block number (0-255)
/// Offset 3: BLOCK_HI - High byte of block number (0-255) = 64K blocks = 16MB max
/// Offset 4: BUFFER - Data buffer (auto-increments through 256 bytes)
/// Offset 5: CTRL - Control register (bit 0: interrupt enable, bit 1: reset buffer index)
/// 
/// BLOCK SIZE: 256 bytes
/// 
/// COMMANDS:
/// 0x00: NOP - No operation
/// 0x01: READ - Read block into buffer
/// 0x02: WRITE - Write buffer to block
/// 0x03: FLUSH - Ensure all writes are saved to disk
/// 
/// STATUS FLAGS:
/// Bit 0: READY - Set when operation complete
/// Bit 1: BUSY - Set during operation
/// Bit 7: ERROR - Set on I/O error
/// 
/// CTRL FLAGS:
/// Bit 0: INTERRUPT_ENABLE - Raise interrupt on operation complete
/// Bit 1: RESET_BUFFER - Reset buffer index to 0
/// </summary>
public class BlockStorageDevice : IDevice
{
    // IDevice implementation
    public int PortCount => 6;
    public event EventHandler<InterruptRequestedEventArgs>? RequestInterrupt;
    public event EventHandler<DeviceWriteEventArgs>? WriteToPort;

    // Storage
    private const int BLOCK_SIZE = 256;
    private const int MAX_BLOCKS = 65536; // 64K blocks = 16MB
    private readonly string _diskImagePath;
    private FileStream? _diskFile;
    
    // Internal buffer
    private readonly byte[] buffer = new byte[BLOCK_SIZE];
    private int bufferIndex = 0;
    
    // Registers
    private ushort blockNumber = 0; // 16-bit block address
    private byte status = STATUS_READY;
    private byte control = 0x00;

    // Port offsets
    private const int PORT_COMMAND = 0;
    private const int PORT_STATUS = 1;
    private const int PORT_BLOCK_LO = 2;
    private const int PORT_BLOCK_HI = 3;
    private const int PORT_BUFFER = 4;
    private const int PORT_CTRL = 5;

    // Commands
    private const byte CMD_NOP = 0x00;
    private const byte CMD_READ = 0x01;
    private const byte CMD_WRITE = 0x02;
    private const byte CMD_FLUSH = 0x03;

    // Status flags
    private const byte STATUS_READY = 0x01;
    private const byte STATUS_BUSY = 0x02;
    private const byte STATUS_ERROR = 0x80;

    // Control flags
    private const byte CTRL_INTERRUPT_ENABLE = 0x01;
    private const byte CTRL_RESET_BUFFER = 0x02;

    public BlockStorageDevice(string diskImagePath = "disk.img")
    {
        _diskImagePath = diskImagePath;
    }

    public void OnPortWrite(int offset, byte data)
    {
        switch (offset)
        {
            case PORT_COMMAND:
                ExecuteCommand(data);
                break;

            case PORT_BLOCK_LO:
                blockNumber = (ushort)((blockNumber & 0xFF00) | data);
                break;

            case PORT_BLOCK_HI:
                blockNumber = (ushort)((blockNumber & 0x00FF) | (data << 8));
                break;

            case PORT_BUFFER:
                // Write to buffer and auto-increment
                buffer[bufferIndex] = data;
                bufferIndex = (bufferIndex + 1) % BLOCK_SIZE;
                break;

            case PORT_CTRL:
                control = data;
                
                // Check for buffer reset
                if ((control & CTRL_RESET_BUFFER) != 0)
                {
                    bufferIndex = 0;
                    // Clear the reset bit (it's a trigger, not a state)
                    control &= unchecked((byte)~CTRL_RESET_BUFFER);
                }
                break;
        }
    }

    public void OnPortRead(int offset)
    {
        byte value = 0x00;

        switch (offset)
        {
            case PORT_COMMAND:
                value = 0x00; // Write-only
                break;

            case PORT_STATUS:
                value = status;
                break;

            case PORT_BLOCK_LO:
                value = (byte)(blockNumber & 0xFF);
                break;

            case PORT_BLOCK_HI:
                value = (byte)((blockNumber >> 8) & 0xFF);
                break;

            case PORT_BUFFER:
                // Read from buffer and auto-increment
                value = buffer[bufferIndex];
                bufferIndex = (bufferIndex + 1) % BLOCK_SIZE;
                break;

            case PORT_CTRL:
                value = control;
                break;
        }

        WriteToPort?.Invoke(this, new DeviceWriteEventArgs(offset, value));
    }

    private void ExecuteCommand(byte command)
    {
        // Clear flags
        status = STATUS_BUSY;
        status &= unchecked((byte)~STATUS_ERROR);

        try
        {
            switch (command)
            {
                case CMD_NOP:
                    break;

                case CMD_READ:
                    ReadBlock();
                    break;

                case CMD_WRITE:
                    WriteBlock();
                    break;

                case CMD_FLUSH:
                    _diskFile?.Flush();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Storage Error: {ex.Message}]");
            status |= STATUS_ERROR;
        }

        // Operation complete
        status &= unchecked((byte)~STATUS_BUSY);
        status |= STATUS_READY;

        // Reset buffer index for convenience
        bufferIndex = 0;

        // Trigger interrupt if enabled
        if ((control & CTRL_INTERRUPT_ENABLE) != 0)
        {
            RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(0xE0));
        }
    }

    private void ReadBlock()
    {
        if (_diskFile == null)
        {
            status |= STATUS_ERROR;
            return;
        }

        long filePosition = (long)blockNumber * BLOCK_SIZE;

        // If reading beyond file size, return zeros
        if (filePosition >= _diskFile.Length)
        {
            Array.Clear(buffer, 0, BLOCK_SIZE);
            return;
        }

        _diskFile.Seek(filePosition, SeekOrigin.Begin);
        int bytesRead = _diskFile.Read(buffer, 0, BLOCK_SIZE);

        // If partial block at end of file, zero-fill the rest
        if (bytesRead < BLOCK_SIZE)
        {
            Array.Clear(buffer, bytesRead, BLOCK_SIZE - bytesRead);
        }
    }

    private void WriteBlock()
    {
        if (_diskFile == null)
        {
            status |= STATUS_ERROR;
            return;
        }

        long filePosition = (long)blockNumber * BLOCK_SIZE;

        // Extend file if necessary
        if (filePosition > _diskFile.Length)
        {
            _diskFile.SetLength(filePosition);
        }

        _diskFile.Seek(filePosition, SeekOrigin.Begin);
        _diskFile.Write(buffer, 0, BLOCK_SIZE);
    }

    public Task StartAsync()
    {
        try
        {
            // Create or open disk image file
            if (File.Exists(_diskImagePath))
            {
                _diskFile = new FileStream(_diskImagePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                Console.WriteLine($"[Block Storage: Opened '{_diskImagePath}' ({_diskFile.Length / 1024}KB)]");
            }
            else
            {
                _diskFile = new FileStream(_diskImagePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                // Create a 1MB disk by default
                _diskFile.SetLength(1024 * 1024);
                Console.WriteLine($"[Block Storage: Created '{_diskImagePath}' (1024KB)]");
            }

            // Initialize state
            Array.Clear(buffer, 0, BLOCK_SIZE);
            bufferIndex = 0;
            blockNumber = 0;
            status = STATUS_READY;
            control = 0x00;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Block Storage Error: {ex.Message}]");
            status = STATUS_ERROR;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _diskFile?.Flush();
        _diskFile?.Close();
        _diskFile?.Dispose();
        _diskFile = null;

        Console.WriteLine("[Block Storage: Stopped]");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Get the size of the disk image in blocks.
    /// </summary>
    public int GetBlockCount()
    {
        if (_diskFile == null)
            return 0;
        
        return (int)(_diskFile.Length / BLOCK_SIZE);
    }

    /// <summary>
    /// Get the total capacity in bytes.
    /// </summary>
    public long GetCapacity()
    {
        return _diskFile?.Length ?? 0;
    }
}
