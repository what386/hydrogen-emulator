namespace Emulator.Application.Commands;

using Emulator.Models;

public static class SystemCommands
{
    public static void Reset(MachineState state)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("  ⚠ Are you sure you want to reset the emulator? (y/n): ");
        Console.ResetColor();
        
        var response = Console.ReadLine()?.ToLowerInvariant();
        if (response != "y" && response != "yes")
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Reset cancelled");
            Console.ResetColor();
            return;
        }
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("        RESETTING EMULATOR         ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        state.Registers.Clear();
        state.RAM.Clear();
        state.StatusWord.Clear();
        state.ControlWord.Clear();
        state.PC.Reset();
        state.IntVector.Clear();
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ Registers cleared");
        Console.WriteLine("  ✓ Memory cleared");
        Console.WriteLine("  ✓ Status word reset");
        Console.WriteLine("  ✓ Control word reset");
        Console.WriteLine("  ✓ Program counter reset");
        Console.WriteLine("  ✓ Interrupt vector cleared");
        Console.ResetColor();
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ Emulator reset complete");
        Console.ResetColor();
    }
    
    public static void ShowHelp()
    {
        PrintHelpSection("State Inspection:");
        PrintHelpCommand("regs, r", "Show all register values");
        PrintHelpCommand("bank", "Show current memory bank");
        PrintHelpCommand("mem", "Show memory pool");
        PrintHelpCommand("statw, status", "Show status word");
        PrintHelpCommand("ctrlw, control", "Show control word");
        PrintHelpCommand("pc", "Show program counter");
        PrintHelpCommand("intv, int", "Show interrupt vector");
        Console.WriteLine();
        
        PrintHelpSection("Execution Control:");
        PrintHelpCommand("step [n], s [n]", "Execute next N instructions (default: 1)");
        PrintHelpCommand("until <addr>, u <addr>", "Run until PC reaches address");
        PrintHelpCommand("speed <hz>", "Change clock speed");
        PrintHelpCommand("resume, continue, c", "Resume normal execution");
        Console.WriteLine();
        
        PrintHelpSection("Memory & Registers:");
        PrintHelpCommand("peek <addr>", "Read memory at address");
        PrintHelpCommand("poke <addr> <val>", "Write value to memory address");
        PrintHelpCommand("rpeek <reg>, regpeek", "Read register value (0-7 or r0-r7)");
        PrintHelpCommand("rpoke <reg> <val>", "Write value to register");
        Console.WriteLine();
        
        PrintHelpSection("Breakpoints & Watchpoints:");
        PrintHelpCommand("break <addr>", "Set breakpoint at address");
        PrintHelpCommand("delete <addr>", "Delete breakpoint (or 'delete all')");
        PrintHelpCommand("breaks", "List all breakpoints");
        PrintHelpCommand("watch <addr> [name]", "Add memory watchpoint");
        PrintHelpCommand("unwatch <addr>", "Remove watchpoint (or 'unwatch all')");
        PrintHelpCommand("watches", "List all watchpoints");
        Console.WriteLine();
        
        PrintHelpSection("Devices:");
        PrintHelpCommand("devices", "List all connected devices");
        PrintHelpCommand("ports", "Show port map");
        PrintHelpCommand("device <port>", "Show device information");
        PrintHelpCommand("inport <port>", "Read from I/O port");
        PrintHelpCommand("outport <port> <val>", "Write to I/O port");
        Console.WriteLine();
        
        PrintHelpSection("Other:");
        PrintHelpCommand("reset", "Reset emulator to initial state");
        PrintHelpCommand("clear, cls", "Clear screen");
        PrintHelpCommand("help, ?", "Show this help message");
        PrintHelpCommand("quit, exit", "Exit emulator");
    }
    
    private static void PrintHelpSection(string section)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {section}");
        Console.ResetColor();
    }
    
    private static void PrintHelpCommand(string command, string description)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"    {command.PadRight(25)}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {description}");
        Console.ResetColor();
    }}
