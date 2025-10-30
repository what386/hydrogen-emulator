namespace Emulator.Application;

using Emulator.Application.Commands;
using Emulator.Core;
using Emulator.IO.Devices;
using Emulator.Models;

public class EmulatorRuntime
{
    private readonly EmulatorConfig config;
    private readonly MachineState state = new();
    private bool isRunning = false;
    private bool inCommandMode = false;
    
    // Status bar tracking
    private Instruction lastInstruction = new();
    private int statusBarRow = 0;
    private readonly object statusLock = new();
    
    public EmulatorRuntime(EmulatorConfig config)
    {
        this.config = config;
    }
    
    public void Run()
    {
        Setup();
        state.Clock.OnTick += OnClockTick;
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // prevent app exit
            EnterCommandMode();
        };
        
        // Reserve space for status bar at bottom
        statusBarRow = Console.CursorTop;
        
        state.Clock.Start();
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"Clock started at {state.Clock.ClockSpeedHz}Hz");
        Console.ResetColor();
        Console.WriteLine(" - Press Ctrl+C to enter command mode.\n");
        
        isRunning = true;
        
        // Update status bar periodically
        _ = Task.Run(async () =>
        {
            while (isRunning)
            {
                UpdateStatusBar();
                await Task.Delay(100);
            }
        });
        
        while (isRunning)
            Thread.Sleep(100);
            
        Shutdown();
    }
    
    private void Setup()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Flashing ROM... ");
        Console.ResetColor();
        state.ROM.Flash(config.RomData);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"Setting clock speed to {config.ClockSpeedHz}Hz... ");
        Console.ResetColor();
        state.Clock.SetSpeed(config.ClockSpeedHz);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Connecting devices... ");
        Console.ResetColor();
        state.PortController.ConnectDevice(0, new SerialTerminal());
        state.PortController.ConnectDevice(2, new FloatingPointUnit());
        _ = state.PortController.StartAllDevicesAsync();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓");
        Console.ResetColor();
    }
    
    private void Shutdown()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nShutting down...");
        Console.ResetColor();
        state.Clock.Stop();
        _ = state.PortController.StopAllDevicesAsync();
    }
    
    private void OnClockTick()
    {
        if (BreakpointCommands.IsBreakpoint(state.PC.Get()))
        {
            Console.WriteLine($"\n⚠ Breakpoint hit at 0x{state.PC.Get():X4}");
            state.Clock.Stop();
        }

        if (WatchpointCommands.CheckWatches(state))
        {
            state.Clock.Stop();
        }

        Interruptor.HandleInterrupts(state);
        var binary = state.ROM.Read((ushort)state.PC.Get());
        var instruction = Decoder.Decode(binary);
        
        lock (statusLock)
        {
            lastInstruction = instruction;
        }
        
        Executor.Execute(state, instruction);
    }
    
    private void UpdateStatusBar()
    {
        if (inCommandMode)
            return;
            
        try
        {
            lock (statusLock)
            {
                // Save current cursor position
                int currentRow = Console.CursorTop;
                int currentCol = Console.CursorLeft;
                
                // Move to status bar location (bottom of console)
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                
                // Build status line
                var pc = state.PC.Get();
                var regs = state.Registers;
                
                // Get register values as hex
                string regValues = string.Join(" ", Enumerable.Range(0, 8)
                    .Select(i => $"R{i}:{regs.Read(i):X4}"));
                
                // Instruction info
                string instrInfo = $"[{lastInstruction.Opcode}]";
                
                // Build full status line
                string statusLine = $"PC:{pc:X4} {instrInfo} | {regValues}";
                
                // Truncate if too long
                if (statusLine.Length > Console.WindowWidth)
                {
                    statusLine = statusLine.Substring(0, Console.WindowWidth - 3) + "...";
                }
                
                // Pad to fill entire line
                statusLine = statusLine.PadRight(Console.WindowWidth);
                
                // Draw with colors
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(statusLine);
                Console.ResetColor();
                
                // Restore cursor position
                Console.SetCursorPosition(currentCol, currentRow);
            }
        }
        catch
        {
            // Ignore errors (can happen during resize or other console operations)
        }
    }
    
    private void EnterCommandMode()
    {
        if (inCommandMode)
            return;

        Console.Clear();
            
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╔════════════════════════════════════╗");
        Console.WriteLine("║        COMMAND MODE ACTIVE         ║");
        Console.WriteLine("╚════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine("Type 'help' for commands, 'resume' to continue\n");
        
        inCommandMode = true;
        state.Clock.Stop();
        
        var cmdMode = new CommandMode(state);
        cmdMode.Run();
        
        Console.Clear();

        state.Clock.Start();
        inCommandMode = false;
    }
}
