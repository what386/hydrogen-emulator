using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Emulator.IO.Devices;

/// <summary>
/// Floppy Disk Controller - supports 3.5" 1.44MB diskettes (80 tracks, 2 heads, 18 sectors/track, 512 bytes/sector)
/// 
/// COMMUNICATION PROTOCOL:
/// 
/// Commands are sent via OnPortWrite(). Multi-byte commands require sequential writes.
/// Data is read via WriteToPort event after OnPortRead() is called.
/// Interrupts signal command completion.
/// 
/// STATUS REGISTER (read via OnPortRead):
/// Bit 7: RQM (Request for Master) - 1=ready for command/data, 0=busy
/// Bit 6: DIO (Data Input/Output) - 1=read from FDC, 0=write to FDC
/// Bit 5: EXM (Execution Mode) - 1=executing command
/// Bit 4: CB (Command Busy) - 1=command in progress
/// Bits 3-0: Reserved
/// 
/// COMMANDS:
/// 
/// 0x03 - SPECIFY (3 bytes total)
///   Byte 1: 0x03
///   Byte 2: SRT (bits 7-4), HUT (bits 3-0) - Step Rate Time, Head Unload Time
///   Byte 3: HLT (bits 7-1), DMA (bit 0) - Head Load Time, DMA enable
/// 
/// 0x04 - SENSE DRIVE STATUS (2 bytes command, 1 byte result)
///   Byte 1: 0x04
///   Byte 2: Drive | Head (bits 2-0: drive, bit 2: head select)
///   Result: ST3 status byte
/// 
/// 0x07 - RECALIBRATE (2 bytes total)
///   Byte 1: 0x07
///   Byte 2: Drive (bits 1-0)
///   Generates interrupt when complete
/// 
/// 0x08 - SENSE INTERRUPT STATUS (1 byte command, 2 bytes result)
///   Byte 1: 0x08
///   Result 1: ST0 (status register 0)
///   Result 2: PCN (present cylinder number)
/// 
/// 0x0F - SEEK (3 bytes total)
///   Byte 1: 0x0F
///   Byte 2: (Head << 2) | Drive
///   Byte 3: Cylinder number (0-79)
///   Generates interrupt when complete
/// 
/// 0x46 - READ DATA (9 bytes command, then sector data + 7 bytes result)
///   Byte 1: 0x46 (MT=0, MF=1, SK=0)
///   Byte 2: (Head << 2) | Drive
///   Byte 3: Cylinder (C)
///   Byte 4: Head (H)
///   Byte 5: Sector (R) - 1-based
///   Byte 6: Bytes per sector (N) - 2 = 512 bytes
///   Byte 7: End of track (EOT)
///   Byte 8: Gap length (GPL)
///   Byte 9: Data length (DTL) - 0xFF for 512 bytes
///   Data: 512 bytes per sector
///   Result: 7 status bytes (ST0, ST1, ST2, C, H, R, N)
/// 
/// 0x45 - WRITE DATA (9 bytes command + sector data, then 7 bytes result)
///   Same format as READ DATA
///   Data: 512 bytes to write
///   Result: 7 status bytes
/// 
/// 0x4A - READ ID (2 bytes command, 7 bytes result)
///   Byte 1: 0x4A
///   Byte 2: (Head << 2) | Drive
///   Result: 7 bytes (ST0, ST1, ST2, C, H, R, N)
/// 
/// </summary>
public class FloppyController : IDevice
{
    public event EventHandler<InterruptRequestedEventArgs>? RequestInterrupt;
    public event EventHandler<DeviceWriteEventArgs>? WriteToPort;

    private const int BYTES_PER_SECTOR = 512;
    private const int SECTORS_PER_TRACK = 18;
    private const int HEADS = 2;
    private const int CYLINDERS = 80;
    private const int DISK_SIZE = BYTES_PER_SECTOR * SECTORS_PER_TRACK * HEADS * CYLINDERS; // 1.44MB

    private readonly byte _interruptVector;
    private byte[] _diskImage;
    private string? _diskImagePath;
    private bool _diskInserted;

    // Controller state
    private readonly Queue<byte> _commandBuffer = new();
    private readonly Queue<byte> _resultBuffer = new();
    private CommandState _state = CommandState.AwaitingCommand;
    private byte _currentCommand;
    private int _expectedCommandBytes;
    private int _dataTransferRemaining;
    
    // Drive state
    private int _currentCylinder;
    private int _currentHead;
    private int _currentSector;
    private int _currentDrive;
    private bool _interruptPending;
    private byte _st0; // Status register 0
    private byte _st1; // Status register 1
    private byte _st2; // Status register 2

    // Command execution
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    private enum CommandState
    {
        AwaitingCommand,
        ReceivingCommand,
        ExecutingCommand,
        SendingResult,
        TransferringData
    }

    public FloppyController(byte interruptVector = 0x0E, string? diskImagePath = null)
    {
        _interruptVector = interruptVector;
        _diskImagePath = diskImagePath;
        _diskImage = new byte[DISK_SIZE];
        _diskInserted = false;
        
        if (diskImagePath != null && File.Exists(diskImagePath))
        {
            LoadDiskImage(diskImagePath);
        }
    }

    /// <summary>
    /// Insert a disk image from a file path. Creates new blank image if file doesn't exist.
    /// </summary>
    public bool InsertDisk(string imagePath)
    {
        if (_diskInserted)
        {
            return false; // Disk already inserted
        }

        try
        {
            _diskImagePath = imagePath;
            
            if (File.Exists(imagePath))
            {
                LoadDiskImage(imagePath);
            }
            else
            {
                // Create blank disk
                _diskImage = new byte[DISK_SIZE];
                Array.Clear(_diskImage, 0, DISK_SIZE);
            }
            
            _diskInserted = true;
            _currentCylinder = 0;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Insert a disk image from a byte array (for in-memory disk images).
    /// </summary>
    public bool InsertDisk(byte[] imageData, string? imagePath = null)
    {
        if (_diskInserted)
        {
            return false;
        }

        try
        {
            _diskImagePath = imagePath;
            _diskImage = new byte[DISK_SIZE];
            Array.Copy(imageData, _diskImage, Math.Min(imageData.Length, DISK_SIZE));
            _diskInserted = true;
            _currentCylinder = 0;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Eject the current disk image. Saves to file if path is set.
    /// </summary>
    public bool EjectDisk()
    {
        if (!_diskInserted)
        {
            return false;
        }

        try
        {
            SaveDiskImage();
            _diskInserted = false;
            _diskImagePath = null;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if a disk is currently inserted.
    /// </summary>
    public bool IsDiskInserted => _diskInserted;

    /// <summary>
    /// Get the current disk image as a byte array (creates a copy).
    /// </summary>
    public byte[]? GetDiskImage()
    {
        if (!_diskInserted)
        {
            return null;
        }

        byte[] copy = new byte[DISK_SIZE];
        Array.Copy(_diskImage, copy, DISK_SIZE);
        return copy;
    }

    private void LoadDiskImage(string path)
    {
        var data = File.ReadAllBytes(path);
        _diskImage = new byte[DISK_SIZE];
        Array.Copy(data, _diskImage, Math.Min(data.Length, DISK_SIZE));
    }

    private void SaveDiskImage()
    {
        if (_diskImagePath != null && _diskInserted)
        {
            File.WriteAllBytes(_diskImagePath, _diskImage);
        }
    }

    public void OnPortWrite(byte data)
    {
        switch (_state)
        {
            case CommandState.AwaitingCommand:
                StartCommand(data);
                break;

            case CommandState.ReceivingCommand:
                _commandBuffer.Enqueue(data);
                if (_commandBuffer.Count >= _expectedCommandBytes)
                {
                    ExecuteCommand();
                }
                break;

            case CommandState.TransferringData:
                _commandBuffer.Enqueue(data);
                _dataTransferRemaining--;
                if (_dataTransferRemaining == 0)
                {
                    CompleteDataWrite();
                }
                break;
        }
    }

    public void OnPortRead()
    {
        if (_state == CommandState.SendingResult && _resultBuffer.Count > 0)
        {
            byte result = _resultBuffer.Dequeue();
            WriteToPort?.Invoke(this, new DeviceWriteEventArgs(result));

            if (_resultBuffer.Count == 0)
            {
                _state = CommandState.AwaitingCommand;
            }
        }
        else if (_state == CommandState.TransferringData)
        {
            if (_resultBuffer.Count > 0)
            {
                byte data = _resultBuffer.Dequeue();
                WriteToPort?.Invoke(this, new DeviceWriteEventArgs(data));
            }

            if (_resultBuffer.Count == 0 && _dataTransferRemaining == 0)
            {
                // Send result phase (7 status bytes)
                SendReadResult();
            }
        }
        else
        {
            // Return status register
            byte status = CalculateStatusRegister();
            WriteToPort?.Invoke(this, new DeviceWriteEventArgs(status));
        }
    }

    private byte CalculateStatusRegister()
    {
        byte status = 0x80; // RQM always set when ready

        if (_state == CommandState.SendingResult || _state == CommandState.TransferringData)
        {
            status |= 0x40; // DIO - read from FDC
        }

        if (_state == CommandState.ExecutingCommand)
        {
            status |= 0x20; // EXM - executing
        }

        if (_state != CommandState.AwaitingCommand)
        {
            status |= 0x10; // CB - command busy
        }

        return status;
    }

    private void StartCommand(byte command)
    {
        _currentCommand = command;
        _commandBuffer.Clear();
        _commandBuffer.Enqueue(command);

        _expectedCommandBytes = command switch
        {
            0x03 => 3, // SPECIFY
            0x04 => 2, // SENSE DRIVE STATUS
            0x07 => 2, // RECALIBRATE
            0x08 => 1, // SENSE INTERRUPT STATUS
            0x0F => 3, // SEEK
            0x46 => 9, // READ DATA
            0x45 => 9, // WRITE DATA
            0x4A => 2, // READ ID
            _ => 1
        };

        if (_expectedCommandBytes == 1)
        {
            ExecuteCommand();
        }
        else
        {
            _state = CommandState.ReceivingCommand;
        }
    }

    private void ExecuteCommand()
    {
        _state = CommandState.ExecutingCommand;
        byte[] cmd = _commandBuffer.ToArray();

        switch (_currentCommand)
        {
            case 0x03: // SPECIFY
                // Just acknowledge, no result
                _state = CommandState.AwaitingCommand;
                break;

            case 0x04: // SENSE DRIVE STATUS
                {
                    _currentDrive = cmd[1] & 0x03;
                    _currentHead = (cmd[1] >> 2) & 0x01;
                    byte st3 = (byte)((_currentHead << 2) | _currentDrive);
                    if (_diskInserted)
                    {
                        st3 |= 0x20; // Ready
                    }
                    if (_currentCylinder == 0)
                    {
                        st3 |= 0x10; // Track 0
                    }
                    _resultBuffer.Enqueue(st3);
                    _state = CommandState.SendingResult;
                }
                break;

            case 0x07: // RECALIBRATE
                _currentDrive = cmd[1] & 0x03;
                _currentCylinder = 0;
                _st0 = (byte)(0x20 | _currentDrive); // Seek end
                _interruptPending = true;
                RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(_interruptVector));
                _state = CommandState.AwaitingCommand;
                break;

            case 0x08: // SENSE INTERRUPT STATUS
                if (_interruptPending)
                {
                    _resultBuffer.Enqueue(_st0);
                    _resultBuffer.Enqueue((byte)_currentCylinder);
                    _interruptPending = false;
                }
                else
                {
                    _resultBuffer.Enqueue(0x80); // Invalid command
                    _resultBuffer.Enqueue(0x00);
                }
                _state = CommandState.SendingResult;
                break;

            case 0x0F: // SEEK
                _currentDrive = cmd[1] & 0x03;
                _currentHead = (cmd[1] >> 2) & 0x01;
                _currentCylinder = cmd[2];
                _st0 = (byte)(0x20 | (_currentHead << 2) | _currentDrive); // Seek end
                _interruptPending = true;
                RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(_interruptVector));
                _state = CommandState.AwaitingCommand;
                break;

            case 0x46: // READ DATA
                {
                    if (!_diskInserted)
                    {
                        // No disk - return error
                        _resultBuffer.Enqueue(0x40); // ST0: Abnormal termination
                        _resultBuffer.Enqueue(0x01); // ST1: Missing address mark
                        _resultBuffer.Enqueue(0x00); // ST2
                        _resultBuffer.Enqueue(cmd[2]); // C
                        _resultBuffer.Enqueue(cmd[3]); // H
                        _resultBuffer.Enqueue(cmd[4]); // R
                        _resultBuffer.Enqueue(cmd[5]); // N
                        _state = CommandState.SendingResult;
                        RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(_interruptVector));
                        break;
                    }

                    _currentDrive = cmd[1] & 0x03;
                    _currentHead = (cmd[1] >> 2) & 0x01;
                    int cylinder = cmd[2];
                    int head = cmd[3];
                    int sector = cmd[4]; // 1-based
                    int bytesPerSector = 128 << cmd[5]; // N parameter

                    // Calculate LBA
                    int lba = (cylinder * HEADS + head) * SECTORS_PER_TRACK + (sector - 1);
                    int offset = lba * BYTES_PER_SECTOR;

                    // Queue sector data
                    _resultBuffer.Clear();
                    for (int i = 0; i < BYTES_PER_SECTOR; i++)
                    {
                        _resultBuffer.Enqueue(offset + i < _diskImage.Length ? _diskImage[offset + i] : (byte)0);
                    }

                    _dataTransferRemaining = 0;
                    _state = CommandState.TransferringData;
                }
                break;

            case 0x45: // WRITE DATA
                {
                    if (!_diskInserted)
                    {
                        // No disk - return error
                        _resultBuffer.Enqueue(0x40); // ST0: Abnormal termination
                        _resultBuffer.Enqueue(0x02); // ST1: Not writable
                        _resultBuffer.Enqueue(0x00); // ST2
                        _resultBuffer.Enqueue(cmd[2]); // C
                        _resultBuffer.Enqueue(cmd[3]); // H
                        _resultBuffer.Enqueue(cmd[4]); // R
                        _resultBuffer.Enqueue(cmd[5]); // N
                        _state = CommandState.SendingResult;
                        RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(_interruptVector));
                        break;
                    }

                    _currentDrive = cmd[1] & 0x03;
                    _currentHead = (cmd[1] >> 2) & 0x01;
                    _currentCylinder = cmd[2];
                    _currentHead = cmd[3];
                    _currentSector = cmd[4];

                    _commandBuffer.Clear();
                    _dataTransferRemaining = BYTES_PER_SECTOR;
                    _state = CommandState.TransferringData;
                }
                break;

            case 0x4A: // READ ID
                _currentDrive = cmd[1] & 0x03;
                _currentHead = (cmd[1] >> 2) & 0x01;
                
                // Return current address mark
                _resultBuffer.Enqueue(0x00); // ST0
                _resultBuffer.Enqueue(0x00); // ST1
                _resultBuffer.Enqueue(0x00); // ST2
                _resultBuffer.Enqueue((byte)_currentCylinder);
                _resultBuffer.Enqueue((byte)_currentHead);
                _resultBuffer.Enqueue(0x01); // Sector 1
                _resultBuffer.Enqueue(0x02); // N = 512 bytes
                _state = CommandState.SendingResult;
                break;

            default:
                // Invalid command
                _resultBuffer.Enqueue(0x80);
                _state = CommandState.SendingResult;
                break;
        }
    }

    private void CompleteDataWrite()
    {
        byte[] data = _commandBuffer.ToArray();
        int cylinder = _currentCylinder;
        int head = _currentHead;
        int sector = _currentSector;

        int lba = (cylinder * HEADS + head) * SECTORS_PER_TRACK + (sector - 1);
        int offset = lba * BYTES_PER_SECTOR;

        // Write data to disk image
        Array.Copy(data, 0, _diskImage, offset, Math.Min(BYTES_PER_SECTOR, data.Length));

        // Send result
        _resultBuffer.Enqueue(0x00); // ST0
        _resultBuffer.Enqueue(0x00); // ST1
        _resultBuffer.Enqueue(0x00); // ST2
        _resultBuffer.Enqueue((byte)cylinder);
        _resultBuffer.Enqueue((byte)head);
        _resultBuffer.Enqueue((byte)sector);
        _resultBuffer.Enqueue(0x02); // N

        _state = CommandState.SendingResult;
        RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(_interruptVector));
    }

    private void SendReadResult()
    {
        byte[] cmd = _commandBuffer.ToArray();
        int cylinder = cmd[2];
        int head = cmd[3];
        int sector = cmd[4];

        _resultBuffer.Enqueue(0x00); // ST0
        _resultBuffer.Enqueue(0x00); // ST1
        _resultBuffer.Enqueue(0x00); // ST2
        _resultBuffer.Enqueue((byte)cylinder);
        _resultBuffer.Enqueue((byte)head);
        _resultBuffer.Enqueue((byte)sector);
        _resultBuffer.Enqueue(0x02); // N

        _state = CommandState.SendingResult;
        RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(_interruptVector));
    }

    public Task StartAsync()
    {
        _isRunning = true;
        _cts = new CancellationTokenSource();
        _state = CommandState.AwaitingCommand;
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _isRunning = false;
        _cts?.Cancel();
        _cts?.Dispose();
        
        // Save disk image if inserted
        SaveDiskImage();
        
        return Task.CompletedTask;
    }
}
