namespace Emulator.IO.Devices;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Serial Terminal device with 7-bit ASCII and even parity.
/// Emulates a UART-style serial interface similar to 8250/16550.
/// 
/// PORT MAP (requires 2 consecutive ports):
/// Offset 0: DATA - Read/Write data register
///   - Write: Send character to terminal (with parity in bit 7)
///   - Read: Receive character from keyboard (with parity in bit 7)
/// Offset 1: STATUS - Read status flags
///   - Bit 0: RX_READY - Set when character available to read
///   - Bit 1: TX_READY - Set when ready to transmit (always 1)
///   - Bit 7: PARITY_ERROR - Set when last received char had parity error
/// 
/// DATA FORMAT:
/// Bits 0-6: 7-bit ASCII character
/// Bit 7: Even parity bit
/// </summary>
public class SerialTerminal : IDevice
{
    // IDevice implementation
    public int PortCount => 2;
    public event EventHandler<InterruptRequestedEventArgs>? RequestInterrupt;
    public event EventHandler<DeviceWriteEventArgs>? WriteToPort;
    
    private readonly byte _interruptVector;
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private readonly object _lock = new();
    
    // Input buffer (single character for now)
    private byte _inputBuffer;
    private bool _inputReady;
    private bool _parityError;
    
    // Port offsets
    private const int PORT_DATA = 0;
    private const int PORT_STATUS = 1;
    
    // Status flags
    private const byte STATUS_RX_READY = 0x01;
    private const byte STATUS_TX_READY = 0x02;
    private const byte STATUS_PARITY_ERROR = 0x80;
    
    public SerialTerminal(byte interruptVector = 0x08)
    {
        _interruptVector = interruptVector;
    }
    
    public void OnPortWrite(int offset, byte data)
    {
        switch (offset)
        {
            case PORT_DATA:
                // Transmit character to terminal
                TransmitCharacter(data);
                break;
                
            case PORT_STATUS:
                // STATUS is read-only, ignore writes
                break;
        }
    }
    
    public void OnPortRead(int offset)
    {
        byte value = 0x00;
        
        switch (offset)
        {
            case PORT_DATA:
                // Read received character
                lock (_lock)
                {
                    value = _inputBuffer;
                    _inputReady = false;  // Clear RX_READY flag
                    _inputBuffer = 0;
                }
                break;
                
            case PORT_STATUS:
                // Read status flags
                lock (_lock)
                {
                    if (_inputReady)
                        value |= STATUS_RX_READY;
                    
                    // TX always ready (no buffering)
                    value |= STATUS_TX_READY;
                    
                    if (_parityError)
                        value |= STATUS_PARITY_ERROR;
                }
                break;
        }
        
        WriteToPort?.Invoke(this, new DeviceWriteEventArgs(offset, value));
    }
    
    private void TransmitCharacter(byte data)
    {
        // Extract 7-bit ASCII (bits 0-6)
        byte asciiChar = (byte)(data & 0x7F);
        
        // Extract parity bit (bit 7)
        bool parityBit = (data & 0x80) != 0;
        
        // Calculate even parity for the 7 data bits
        bool calculatedParity = CalculateEvenParity(asciiChar);
        
        // Verify parity
        if (parityBit != calculatedParity)
        {
            Console.Write($"[PARITY ERROR: 0x{data:X2}] ");
            return;
        }
        
        // Display the character
        switch (asciiChar)
        {
            case 0x08: // Backspace
                Console.Write("\b \b"); // Backspace, space, backspace
                break;
            case 0x09: // Tab
                Console.Write('\t');
                break;
            case 0x0A: // Line feed
                Console.WriteLine();
                break;
            case 0x0D: // Carriage return
                Console.Write('\r');
                break;
            case >= 0x20 and <= 0x7E: // Printable ASCII
                Console.Write((char)asciiChar);
                break;
            default: // Non-printable character
                Console.Write($"[0x{asciiChar:X2}]");
                break;
        }
    }
    
    public Task StartAsync()
    {
        lock (_lock)
        {
            if (_isRunning)
                return Task.CompletedTask;
            
            _isRunning = true;
            _inputReady = false;
            _parityError = false;
            _inputBuffer = 0;
            _cts = new CancellationTokenSource();
        }
        
        // Start keyboard input monitoring in background
        _ = Task.Run(() => MonitorKeyboardInputAsync(_cts.Token));
        
        Console.WriteLine("[Serial Terminal Started - 7-bit ASCII, Even Parity]");
        return Task.CompletedTask;
    }
    
    public Task StopAsync()
    {
        lock (_lock)
        {
            if (!_isRunning)
                return Task.CompletedTask;
            
            _isRunning = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
        
        Console.WriteLine("\n[Serial Terminal Stopped]");
        return Task.CompletedTask;
    }
    
    private async Task MonitorKeyboardInputAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // Check if key is available without blocking
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    byte asciiValue = (byte)key.KeyChar;
                    
                    // Only process 7-bit ASCII
                    if (asciiValue <= 0x7F)
                    {
                        lock (_lock)
                        {
                            // Don't overwrite unread input
                            if (!_inputReady)
                            {
                                // Calculate even parity
                                bool parity = CalculateEvenParity(asciiValue);
                                
                                // Construct byte with parity bit in MSB
                                _inputBuffer = (byte)(asciiValue | (parity ? 0x80 : 0x00));
                                _inputReady = true;
                                _parityError = false;
                                
                                // Request interrupt to notify processor of new data
                                RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(_interruptVector));
                            }
                        }
                    }
                }
                
                // Small delay to prevent tight loop
                await Task.Delay(10, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
    }
    
    private bool CalculateEvenParity(byte value)
    {
        // Count number of 1 bits in the 7-bit value
        int count = 0;
        byte temp = (byte)(value & 0x7F);
        
        while (temp != 0)
        {
            count += temp & 1;
            temp >>= 1;
        }
        
        // Even parity: return true if odd number of bits (to make total even)
        return (count & 1) == 1;
    }
}
