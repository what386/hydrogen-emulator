namespace Emulator.IO.Devices;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Floppy disk drive controller that supports reading/writing to floppy disk images.
/// Standard 1.44MB floppy format: 80 tracks, 2 heads, 18 sectors per track, 512 bytes per sector.
/// 
/// Port Layout (4 ports):
/// - Port 0 (Command/Status): Write commands, read status
/// - Port 1 (Data): Read/write data bytes
/// - Port 2 (Track): Current track number
/// - Port 3 (Sector): Current sector number
/// 
/// Commands (written to Port 0):
/// - 0x01: Read sector
/// - 0x02: Write sector
/// - 0x03: Seek to track
/// - 0x04: Reset drive
/// 
/// Status bits (read from Port 0):
/// - Bit 0: Ready (1 = ready, 0 = busy)
/// - Bit 1: Error (1 = error occurred)
/// - Bit 2: Write protected
/// - Bit 6: Disk present
/// - Bit 7: Data ready (for read operations)
/// </summary>
public class FloppyDrive : IDevice
{
    // Floppy disk geometry (1.44MB)
    private const int TRACKS = 80;
    private const int HEADS = 2;
    private const int SECTORS_PER_TRACK = 18;
    private const int BYTES_PER_SECTOR = 512;
    private const int TOTAL_SECTORS = TRACKS * HEADS * SECTORS_PER_TRACK; // 2880 sectors
    private const int DISK_SIZE = TOTAL_SECTORS * BYTES_PER_SECTOR; // 1,474,560 bytes

    // Status register bits
    private const byte STATUS_READY = 0x01;
    private const byte STATUS_ERROR = 0x02;
    private const byte STATUS_WRITE_PROTECTED = 0x04;
    private const byte STATUS_DISK_PRESENT = 0x40;
    private const byte STATUS_DATA_READY = 0x80;

    // Commands
    private const byte CMD_READ_SECTOR = 0x01;
    private const byte CMD_WRITE_SECTOR = 0x02;
    private const byte CMD_SEEK = 0x03;
    private const byte CMD_RESET = 0x04;

    private byte[] _diskImage;
    private string? _imagePath;
    private byte _statusRegister;
    private byte _currentTrack;
    private byte _currentSector;
    private byte _currentHead;
    private byte[] _sectorBuffer;
    private int _bufferPosition;
    private bool _isRunning;
    private readonly SemaphoreSlim _operationLock = new(1, 1);

    public int PortCount => 4;

    public event EventHandler<InterruptRequestedEventArgs>? RequestInterrupt;
    public event EventHandler<DeviceWriteEventArgs>? WriteToPort;

    public FloppyDrive(string? imagePath = null)
    {
        _imagePath = imagePath;
        _diskImage = new byte[DISK_SIZE];
        _sectorBuffer = new byte[BYTES_PER_SECTOR];
        _statusRegister = STATUS_READY;
        _currentTrack = 0;
        _currentSector = 1; // Sectors are 1-indexed
        _currentHead = 0;
        _bufferPosition = 0;
    }

    public async Task StartAsync()
    {
        _isRunning = true;
        
        // Load disk image if path provided
        if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
        {
            try
            {
                var fileData = await File.ReadAllBytesAsync(_imagePath);
                Array.Copy(fileData, _diskImage, Math.Min(fileData.Length, DISK_SIZE));
                _statusRegister |= STATUS_DISK_PRESENT;
            }
            catch (Exception)
            {
                _statusRegister &= unchecked((byte)~STATUS_DISK_PRESENT);
            }
        }
        else if (!string.IsNullOrEmpty(_imagePath))
        {
            // Create new empty disk image
            _statusRegister |= STATUS_DISK_PRESENT;
        }

        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _isRunning = false;

        // Save disk image if path provided and disk is present
        if (!string.IsNullOrEmpty(_imagePath) && (_statusRegister & STATUS_DISK_PRESENT) != 0)
        {
            try
            {
                await File.WriteAllBytesAsync(_imagePath, _diskImage);
            }
            catch (Exception)
            {
                // Failed to save - could log this
            }
        }

        await Task.CompletedTask;
    }

    public void OnPortWrite(int offset, byte data)
    {
        switch (offset)
        {
            case 0: // Command register
                ExecuteCommand(data);
                break;

            case 1: // Data register (for write operations)
                if (_bufferPosition < BYTES_PER_SECTOR)
                {
                    _sectorBuffer[_bufferPosition++] = data;
                    
                    // If buffer is full, complete the write operation
                    if (_bufferPosition >= BYTES_PER_SECTOR)
                    {
                        CompleteWriteSector();
                    }
                }
                break;

            case 2: // Track register
                _currentTrack = data;
                break;

            case 3: // Sector register
                _currentSector = data;
                break;
        }
    }

    public void OnPortRead(int offset)
    {
        // Port read acknowledgment - could be used for timing in more complex implementations
    }

    private void ExecuteCommand(byte command)
    {
        // Check if ready
        if ((_statusRegister & STATUS_READY) == 0)
        {
            return;
        }

        // Clear error and data ready flags
        _statusRegister &= unchecked((byte)~(STATUS_ERROR | STATUS_DATA_READY));

        switch (command)
        {
            case CMD_READ_SECTOR:
                Task.Run(() => ReadSectorAsync());
                break;

            case CMD_WRITE_SECTOR:
                // Prepare for write - clear buffer
                _bufferPosition = 0;
                Array.Clear(_sectorBuffer, 0, BYTES_PER_SECTOR);
                break;

            case CMD_SEEK:
                // Seek is instant in this implementation
                break;

            case CMD_RESET:
                Reset();
                break;
        }
    }

    private async Task ReadSectorAsync()
    {
        await _operationLock.WaitAsync();
        
        try
        {
            // Mark as busy
            _statusRegister &= unchecked((byte)~STATUS_READY);

            // Check if disk is present
            if ((_statusRegister & STATUS_DISK_PRESENT) == 0)
            {
                _statusRegister |= STATUS_ERROR;
                _statusRegister |= STATUS_READY;
                return;
            }

            // Validate parameters
            if (!ValidatePosition())
            {
                _statusRegister |= STATUS_ERROR;
                _statusRegister |= STATUS_READY;
                return;
            }

            // Simulate seek time
            await Task.Delay(10);

            // Calculate disk offset
            int sectorOffset = GetSectorOffset();
            
            // Read sector into buffer
            Array.Copy(_diskImage, sectorOffset, _sectorBuffer, 0, BYTES_PER_SECTOR);
            _bufferPosition = 0;

            // Mark data as ready
            _statusRegister |= STATUS_DATA_READY;
            _statusRegister |= STATUS_READY;

            // Raise interrupt to notify CPU
            RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(0x0E)); // Floppy IRQ
        }
        finally
        {
            _operationLock.Release();
        }
    }

    private void CompleteWriteSector()
    {
        Task.Run(async () =>
        {
            await _operationLock.WaitAsync();
            
            try
            {
                // Mark as busy
                _statusRegister &= unchecked((byte)~STATUS_READY);

                // Check if disk is present and not write protected
                if ((_statusRegister & STATUS_DISK_PRESENT) == 0 || 
                    (_statusRegister & STATUS_WRITE_PROTECTED) != 0)
                {
                    _statusRegister |= STATUS_ERROR;
                    _statusRegister |= STATUS_READY;
                    return;
                }

                // Validate parameters
                if (!ValidatePosition())
                {
                    _statusRegister |= STATUS_ERROR;
                    _statusRegister |= STATUS_READY;
                    return;
                }

                // Simulate write time
                await Task.Delay(10);

                // Calculate disk offset and write
                int sectorOffset = GetSectorOffset();
                Array.Copy(_sectorBuffer, 0, _diskImage, sectorOffset, BYTES_PER_SECTOR);

                // Mark as ready
                _statusRegister |= STATUS_READY;

                // Raise interrupt to notify CPU
                RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(0x0E)); // Floppy IRQ
            }
            finally
            {
                _operationLock.Release();
            }
        });
    }

    private bool ValidatePosition()
    {
        // Note: _currentHead is derived from track (track / 2 determines head)
        _currentHead = (byte)(_currentTrack % HEADS);
        int logicalTrack = _currentTrack / HEADS;

        return logicalTrack < TRACKS &&
               _currentSector >= 1 &&
               _currentSector <= SECTORS_PER_TRACK;
    }

    private int GetSectorOffset()
    {
        _currentHead = (byte)(_currentTrack % HEADS);
        int logicalTrack = _currentTrack / HEADS;
        
        // CHS to LBA conversion
        int lba = (logicalTrack * HEADS + _currentHead) * SECTORS_PER_TRACK + (_currentSector - 1);
        return lba * BYTES_PER_SECTOR;
    }

    private void Reset()
    {
        _currentTrack = 0;
        _currentSector = 1;
        _currentHead = 0;
        _bufferPosition = 0;
        _statusRegister = STATUS_READY;
        
        if ((_statusRegister & STATUS_DISK_PRESENT) != 0)
        {
            _statusRegister |= STATUS_DISK_PRESENT;
        }
        
        Array.Clear(_sectorBuffer, 0, BYTES_PER_SECTOR);
    }

    /// <summary>
    /// Read the current status register value.
    /// Call this to get the return value for reads from Port 0.
    /// </summary>
    public byte ReadStatus() => _statusRegister;

    /// <summary>
    /// Read the next data byte from the sector buffer.
    /// Call this to get the return value for reads from Port 1.
    /// </summary>
    public byte ReadData()
    {
        if (_bufferPosition < BYTES_PER_SECTOR)
        {
            return _sectorBuffer[_bufferPosition++];
        }
        
        // If all data has been read, clear data ready flag
        if (_bufferPosition >= BYTES_PER_SECTOR)
        {
            _statusRegister &= unchecked((byte)~STATUS_DATA_READY);
        }
        
        return 0xFF; // Return default value if buffer exhausted
    }

    /// <summary>
    /// Read the current track number.
    /// Call this to get the return value for reads from Port 2.
    /// </summary>
    public byte ReadTrack() => _currentTrack;

    /// <summary>
    /// Read the current sector number.
    /// Call this to get the return value for reads from Port 3.
    /// </summary>
    public byte ReadSector() => _currentSector;

    /// <summary>
    /// Insert a disk image into the drive.
    /// </summary>
    public void InsertDisk(byte[] diskData)
    {
        Array.Copy(diskData, _diskImage, Math.Min(diskData.Length, DISK_SIZE));
        _statusRegister |= STATUS_DISK_PRESENT;
    }

    /// <summary>
    /// Eject the disk from the drive.
    /// </summary>
    public void EjectDisk()
    {
        _statusRegister &= unchecked((byte)~STATUS_DISK_PRESENT);
        Array.Clear(_diskImage, 0, DISK_SIZE);
    }

    /// <summary>
    /// Set or clear write protection on the disk.
    /// </summary>
    public void SetWriteProtect(bool writeProtected)
    {
        if (writeProtected)
        {
            _statusRegister |= STATUS_WRITE_PROTECTED;
        }
        else
        {
            _statusRegister &= unchecked((byte)~STATUS_WRITE_PROTECTED);
        }
    }
}
