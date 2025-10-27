using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Emulator.IO;

namespace Emulator.IO.Devices;

public class SerialTerminal : IDevice
{
    public event EventHandler<InterruptRequestedEventArgs>? RequestInterrupt;
    public event EventHandler<DeviceWriteEventArgs>? WriteToPort;
    
    private readonly byte _interruptVector;
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private readonly object _lock = new();
    
    public SerialTerminal(byte interruptVector = 0x08)
    {
        _interruptVector = interruptVector;
    }
    
    public void OnPortWrite(byte data)
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
                Console.Write('\b');
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
    
    public void OnPortRead()
    {
        // Acknowledge that output data was read by the processor
        // Could be used to implement flow control or buffering
    }
    
    public Task StartAsync()
    {
        lock (_lock)
        {
            if (_isRunning)
                return Task.CompletedTask;
            
            _isRunning = true;
            _cts = new CancellationTokenSource();
        }
        
        // Start keyboard input monitoring in background
        _ = Task.Run(() => MonitorKeyboardInputAsync(_cts.Token));
        
        Console.WriteLine("[Serial Terminal Started]");
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
                        // Calculate even parity
                        bool parity = CalculateEvenParity(asciiValue);
                        
                        // Construct byte with parity bit in MSB
                        byte dataWithParity = (byte)(asciiValue | (parity ? 0x80 : 0x00));
                        
                        // Send data to processor via WriteToPort event
                        WriteToPort?.Invoke(this, new DeviceWriteEventArgs(dataWithParity));
                        
                        // Request interrupt to notify processor of new data
                        RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(_interruptVector));
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
