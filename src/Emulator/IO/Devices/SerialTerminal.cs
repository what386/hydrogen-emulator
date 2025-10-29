namespace Emulator.IO.Devices;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Serial Terminal device with 7-bit ASCII and even parity.
/// Emulates a UART-style serial interface similar to 8250/16550.
/// 
/// PORT MAP:
/// Offset 0: DATA - Read/Write data register
/// Offset 1: STATUS - Read status flags
/// 
/// DATA FORMAT:
/// Bits 0-6: 7-bit ASCII
/// Bit 7: Even parity
/// </summary>
public class SerialTerminal : IDevice
{
    public int PortCount => 2;
    public event EventHandler<InterruptRequestedEventArgs>? RequestInterrupt;
    public event EventHandler<DeviceWriteEventArgs>? WriteToPort;

    private readonly byte _interruptVector;
    private CancellationTokenSource? _cts;
    private bool _isRunning;
    private readonly object _lock = new();

    private byte _inputBuffer;
    private bool _inputReady;
    private bool _parityError;

    private const int PORT_DATA = 0;
    private const int PORT_STATUS = 1;
    private const byte STATUS_RX_READY = 0x01;
    private const byte STATUS_TX_READY = 0x02;
    private const byte STATUS_PARITY_ERROR = 0x80;

    // Input FSM for escape sequences
    private enum EscapeState { Ground, Escape, CSI }
    private EscapeState _escState = EscapeState.Ground;
    private readonly List<byte> _escBuffer = new();

    // Output FSM for escape sequences
    private EscapeState _outState = EscapeState.Ground;
    private readonly List<byte> _outBuffer = new();

    public SerialTerminal(byte interruptVector = 0x08) => _interruptVector = interruptVector;

    public void OnPortWrite(int offset, byte data)
    {
        if (offset == PORT_DATA)
            TransmitCharacter(data);
    }

    public void OnPortRead(int offset)
    {
        if (offset == PORT_DATA)
        {
            lock (_lock) _inputReady = false;
        }
    }

    private void TransmitCharacter(byte data)
    {
        byte asciiChar = (byte)(data & 0x7F);
        bool parityBit = (data & 0x80) != 0;
        bool calculatedParity = CalculateEvenParity(asciiChar);

        if (parityBit != calculatedParity)
        {
            Console.Write($"[PARITY ERROR: 0x{data:X2}] ");
            return;
        }

        // Output FSM
        switch (_outState)
        {
            case EscapeState.Ground:
                if (asciiChar == 0x1B) // ESC
                {
                    _outState = EscapeState.Escape;
                    _outBuffer.Clear();
                    _outBuffer.Add(asciiChar);
                }
                else
                {
                    PrintChar(asciiChar);
                }
                break;

            case EscapeState.Escape:
                _outBuffer.Add(asciiChar);
                if (asciiChar == (byte)'[')
                    _outState = EscapeState.CSI;
                else
                {
                    // Standalone ESC
                    foreach (var b in _outBuffer) PrintChar(b);
                    _outState = EscapeState.Ground;
                }
                break;

            case EscapeState.CSI:
                _outBuffer.Add(asciiChar);
                if (asciiChar >= 0x40 && asciiChar <= 0x7E)
                {
                    ExecuteCSI(_outBuffer);
                    _outState = EscapeState.Ground;
                }
                break;
        }
    }

    private void PrintChar(byte asciiChar)
    {
        Console.Write((char)asciiChar);
    }

    private void ExecuteCSI(List<byte> seq)
    {
        foreach (var b in seq) PrintChar(b);
    }

    public Task StartAsync()
    {
        lock (_lock)
        {
            if (_isRunning) return Task.CompletedTask;
            _isRunning = true;
            _inputReady = false;
            _parityError = false;
            _inputBuffer = 0;
            _cts = new CancellationTokenSource();
        }

        _ = Task.Run(() => MonitorKeyboardInputAsync(_cts.Token));
        Console.WriteLine("[Serial Terminal Started - 7-bit ASCII, Even Parity]");
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        lock (_lock)
        {
            if (!_isRunning) return Task.CompletedTask;
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
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    byte asciiValue = (byte)key.KeyChar;

                    // Handle Enter / LF
                    if (key.Key == ConsoleKey.Enter) asciiValue = 0x0D;
                    else if (asciiValue == 0x0A) asciiValue = 0x0A; // Ctrl+J

                    if (asciiValue <= 0x7F)
                    {
                        lock (_lock)
                        {
                            if (!_inputReady)
                                ProcessAsciiByte(asciiValue);
                        }
                    }
                }

                await Task.Delay(10, ct);
            }
        }
        catch (OperationCanceledException) { }
    }

    private void ProcessAsciiByte(byte b)
    {
        switch (_escState)
        {
            case EscapeState.Ground:
                if (b == 0x1B) { _escState = EscapeState.Escape; _escBuffer.Clear(); _escBuffer.Add(b); }
                else SendByteToPort(b);
                break;

            case EscapeState.Escape:
                _escBuffer.Add(b);
                if (b == (byte)'[') _escState = EscapeState.CSI;
                else { foreach (var by in _escBuffer) SendByteToPort(by); _escState = EscapeState.Ground; }
                break;

            case EscapeState.CSI:
                _escBuffer.Add(b);
                if (b >= 0x40 && b <= 0x7E) { foreach (var by in _escBuffer) SendByteToPort(by); _escState = EscapeState.Ground; }
                break;
        }
    }

    private void SendByteToPort(byte asciiValue)
    {
        bool parity = CalculateEvenParity(asciiValue);
        _inputBuffer = (byte)(asciiValue | (parity ? 0x80 : 0x00));
        _inputReady = true;
        _parityError = false;

        WriteToPort?.Invoke(this, new DeviceWriteEventArgs(PORT_DATA, _inputBuffer));
        byte status = STATUS_TX_READY | STATUS_RX_READY;
        WriteToPort?.Invoke(this, new DeviceWriteEventArgs(PORT_STATUS, status));
        RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(_interruptVector));
    }

    private bool CalculateEvenParity(byte value)
    {
        int count = 0;
        byte temp = (byte)(value & 0x7F);
        while (temp != 0) { count += temp & 1; temp >>= 1; }
        return (count & 1) == 1;
    }
}
