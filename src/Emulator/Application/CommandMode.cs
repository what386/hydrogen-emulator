namespace Emulator.Application;

using Emulator.Registers;
using Emulator.Core;
using Emulator.Models;

public class CommandMode
{
    private MachineState state;
    
    public CommandMode(MachineState state)
    {
        this.state = state;
    }
    
    public void Run()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> ");
            Console.ResetColor();
            
            string? input = Console.ReadLine();
            if (input == null) continue;
            
            var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;
            
            string cmd = parts[0].ToLowerInvariant();
            string? arg = parts.Length > 1 ? parts[1] : null;
            
            Console.WriteLine(); // Spacing before output
            
            switch (cmd)
            {
                case "help":
                case "?":
                    ShowHelp();
                    break;
                    
                case "regs":
                case "r":
                    ShowRegisters();
                    break;
                
                case "bank":
                    ShowBank();
                    break;
 
                case "mem":
                    ShowMemory();
                    break;
                    
                case "statw":
                case "status":
                    ShowStatusWord();
                    break;
                    
                case "ctrlw":
                case "control":
                    ShowControlWord();
                    break;
                    
                case "pc":
                    ShowProgramCounter();
                    break;
                    
                case "intv":
                case "int":
                    ShowInterruptVector();
                    break;
                    
                case "step":
                case "s":
                    Step();
                    break;
                    
                case "speed":
                    SetSpeed(arg);
                    break;
                    
                case "resume":
                case "continue":
                case "c":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Resuming execution...");
                    Console.ResetColor();
                    return;
                    
                case "clear":
                case "cls":
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("╔════════════════════════════════════╗");
                    Console.WriteLine("║        COMMAND MODE ACTIVE         ║");
                    Console.WriteLine("╚════════════════════════════════════╝");
                    Console.ResetColor();
                    break;
                    
                case "quit":
                case "exit":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exiting emulator...");
                    Console.ResetColor();
                    Environment.Exit(0);
                    break;
                    
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ Unknown command: '{cmd}'");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("  Type 'help' for a list of commands");
                    Console.ResetColor();
                    break;
            }
            
            Console.WriteLine(); // Spacing after output
        }
    }
    
    private void ShowRegisters()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("         REGISTER VALUES           ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        for (int i = 0; i < 8; i++)
        {
            ushort value = state.Registers.Read(i);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"  R{i}: ");
            Console.ResetColor();
            Console.Write($"0/home/bmorin/source/hydrogen-assembler/examples/echo.binx{value:X4}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  ({value})");
            Console.ResetColor();
        }
    }
    
    private void ShowBank()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("          MEMORY BANK              ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Active Page:  ");
        Console.ResetColor();
        Console.WriteLine($"{state.RAM.ActivePage}");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Bank Size:    ");
        Console.ResetColor();
        Console.WriteLine($"256 bytes");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Stack Ptr:    ");
        Console.ResetColor();
        Console.WriteLine($"0x{state.RAM.StackPointer:X4}");
        
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Offset    00 01 02 03 04 05 06 07  08 09 0A 0B 0C 0D 0E 0F");
        Console.WriteLine("  ────────  ───────────────────────  ───────────────────────");
        Console.ResetColor();
        
        for (int i = 0; i < 256; i += 16)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  {i:X2}:       ");
            Console.ResetColor();
            
            // First 8 bytes
            for (int j = 0; j < 8; j++)
            {
                byte value = state.RAM.ReadBank(i + j);
                if (value == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"{value:X2} ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write($"{value:X2} ");
                }
            }
            
            Console.Write(" ");
            
            // Second 8 bytes
            for (int j = 8; j < 16; j++)
            {
                byte value = state.RAM.ReadBank(i + j);
                if (value == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"{value:X2} ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write($"{value:X2} ");
                }
            }
            
            Console.WriteLine();
        }
    }
    
    private void ShowMemory()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("          MEMORY POOL              ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Total Size:   ");
        Console.ResetColor();
        Console.Write($"65536 bytes");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  (64 KB)");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Page Size:    ");
        Console.ResetColor();
        Console.WriteLine($"256 bytes");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Total Pages:  ");
        Console.ResetColor();
        Console.WriteLine($"256");
        
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Showing first 128 bytes:");
        Console.WriteLine();
        Console.WriteLine("  Address   00 01 02 03 04 05 06 07  08 09 0A 0B 0C 0D 0E 0F");
        Console.WriteLine("  ────────  ───────────────────────  ───────────────────────");
        Console.ResetColor();
        
        int displayLength = 128;
        
        for (int i = 0; i < displayLength; i += 16)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  {i:X4}:     ");
            Console.ResetColor();
            
            // First 8 bytes
            for (int j = 0; j < 8; j++)
            {
                byte value = state.RAM.ReadPool(i + j);
                if (value == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"{value:X2} ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write($"{value:X2} ");
                }
            }
            
            Console.Write(" ");
            
            // Second 8 bytes
            for (int j = 8; j < 16; j++)
            {
                byte value = state.RAM.ReadPool(i + j);
                if (value == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($"{value:X2} ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write($"{value:X2} ");
                }
            }
            
            Console.WriteLine();
        }
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"\n  ... (65408 bytes hidden)");
        Console.ResetColor();
    }
    
    private void ShowStatusWord()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("          STATUS WORD              ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        var statusWord = state.StatusWord;
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Flags:     ");
        Console.ResetColor();
        Console.Write($"0b{Convert.ToString(statusWord.Flags, 2).PadLeft(8, '0')}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  (0x{statusWord.Flags:X2})");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Alt Cond:  ");
        Console.ResetColor();
        Console.WriteLine(statusWord.AlternateConditions ? "Enabled" : "Disabled");
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Active Flags:");
        Console.ResetColor();
        
        bool hasFlags = false;
        if (statusWord.GetFlag(StatusWord.CARRY_FLAG))
        {
            Console.WriteLine("    ✓ CARRY");
            hasFlags = true;
        }
        if (statusWord.GetFlag(StatusWord.ERROR_FLAG))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("    ✓ ERROR");
            Console.ResetColor();
            hasFlags = true;
        }
        if (statusWord.GetFlag(StatusWord.PARITY_FLAG))
        {
            Console.WriteLine("    ✓ PARITY");
            hasFlags = true;
        }
        if (statusWord.GetFlag(StatusWord.AUX_CARRY_FLAG))
        {
            Console.WriteLine("    ✓ AUX_CARRY");
            hasFlags = true;
        }
        if (statusWord.GetFlag(StatusWord.OVERFLOW_FLAG))
        {
            Console.WriteLine("    ✓ OVERFLOW");
            hasFlags = true;
        }
        if (statusWord.GetFlag(StatusWord.ZERO_FLAG))
        {
            Console.WriteLine("    ✓ ZERO");
            hasFlags = true;
        }
        if (statusWord.GetFlag(StatusWord.SIGN_FLAG))
        {
            Console.WriteLine("    ✓ SIGN");
            hasFlags = true;
        }
        
        if (!hasFlags)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    (none)");
            Console.ResetColor();
        }
    }
    
    private void ShowControlWord()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("         CONTROL WORD              ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        var controlWord = state.ControlWord;
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Flags:  ");
        Console.ResetColor();
        Console.Write($"0b{Convert.ToString(controlWord.Flags, 2).PadLeft(8, '0')}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  (0x{controlWord.Flags:X2})");
        Console.ResetColor();
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Active Flags:");
        Console.ResetColor();
        
        bool hasFlags = false;
        if (controlWord.GetFlag(ControlWord.ALT_CONDITIONS))
        {
            Console.WriteLine("    ✓ ALT_CONDITIONS");
            hasFlags = true;
        }
        if (controlWord.GetFlag(ControlWord.PAGE_JUMP_MODE))
        {
            Console.WriteLine("    ✓ PAGE_JUMP_MODE");
            hasFlags = true;
        }
        if (controlWord.GetFlag(ControlWord.AUTO_INCREMENT))
        {
            Console.WriteLine("    ✓ AUTO_INCREMENT");
            hasFlags = true;
        }
        if (controlWord.GetFlag(ControlWord.DIRECTION_FLAG))
        {
            Console.WriteLine("    ✓ DIRECTION_FLAG (backward)");
            hasFlags = true;
        }
        if (controlWord.GetFlag(ControlWord.INTERRUPT_ENABLE))
        {
            Console.WriteLine("    ✓ INTERRUPT_ENABLE");
            hasFlags = true;
        }
        if (controlWord.GetFlag(ControlWord.HALT_FLAG))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("    ✓ HALT_FLAG");
            Console.ResetColor();
            hasFlags = true;
        }
        if (controlWord.GetFlag(ControlWord.DEBUG_MODE))
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("    ✓ DEBUG_MODE");
            Console.ResetColor();
            hasFlags = true;
        }
        if (controlWord.GetFlag(ControlWord.KERNEL_MODE))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("    ✓ KERNEL_MODE");
            Console.ResetColor();
            hasFlags = true;
        }
        
        if (!hasFlags)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    (none)");
            Console.ResetColor();
        }
    }
    
    private void ShowProgramCounter()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("        PROGRAM COUNTER            ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        int pc = state.PC.Get();
        byte pcHigh = state.PC.PCHigh;
        byte pcLow = state.PC.PCLow;
        int branchOffset = state.PC.BranchOffset;
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  PC:            ");
        Console.ResetColor();
        Console.Write($"0x{pc:X4}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  ({pc})");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  PC High:       ");
        Console.ResetColor();
        Console.WriteLine($"0x{pcHigh:X2}");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  PC Low:        ");
        Console.ResetColor();
        Console.WriteLine($"0x{pcLow:X2}");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Branch Offset: ");
        Console.ResetColor();
        Console.WriteLine($"{branchOffset}");
    }
    
    private void ShowInterruptVector()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("        INTERRUPT VECTOR           ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        var intVector = state.IntVector;
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Interrupt Mask: ");
        Console.ResetColor();
        Console.Write($"0b{Convert.ToString(intVector.InterruptMask, 2).PadLeft(8, '0')}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  (0x{intVector.InterruptMask:X2})");
        Console.ResetColor();
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Vector Table:");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Index  Address");
        Console.WriteLine("  ─────  ───────");
        Console.ResetColor();
        
        for (int i = 0; i < 16; i++)
        {
            int address = intVector.GetAddress(i);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"    {i:D2}   ");
            Console.ResetColor();
            
            if (address == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"0x{address:X4}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"0x{address:X4}");
            }
        }
        
        // Show pending interrupts
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Pending Interrupts:");
        Console.ResetColor();
        
        if (intVector.pendingInterrupts.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    (none)");
            Console.ResetColor();
        }
        else
        {
            int position = 0;
            foreach (var (priority, vector) in intVector.pendingInterrupts)
            {
                Console.WriteLine($"    [{position}] Priority: {priority}, Vector: 0x{vector:X2}");
                position++;
            }
        }
        
        // Show active interrupts
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  Active Interrupts:");
        Console.ResetColor();
        
        if (intVector.activeInterrupts.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    (none)");
            Console.ResetColor();
        }
        else
        {
            int position = 0;
            foreach (var (priority, vector) in intVector.activeInterrupts)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"    [{position}] Priority: {priority}, Vector: 0x{vector:X2}");
                Console.ResetColor();
                position++;
            }
        }
    }
    
    private void Step()
    {
        var pcBefore = state.PC.Get();
        var binary = state.ROM.Read((ushort)pcBefore);
        var instruction = Decoder.Decode(binary);
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("         SINGLE STEP               ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Before: ");
        Console.ResetColor();
        Console.WriteLine($"PC = 0x{pcBefore:X4}");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Instruction: ");
        Console.ResetColor();
        Console.WriteLine($"{instruction.Opcode}");
        
        Executor.Execute(state, instruction);
        
        var pcAfter = state.PC.Get();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  After: ");
        Console.ResetColor();
        Console.WriteLine($"PC = 0x{pcAfter:X4}");
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n  ✓ Executed 1 instruction");
        Console.ResetColor();
    }
    
    private void SetSpeed(string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing argument");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: speed <hz>");
            Console.ResetColor();
            return;
        }
        
        if (int.TryParse(arg, out int hz) && hz > 0)
        {
            var oldSpeed = state.Clock.ClockSpeedHz;
            state.Clock.SetSpeed(hz);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Clock speed changed");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  Old: ");
            Console.ResetColor();
            Console.WriteLine($"{oldSpeed}Hz");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  New: ");
            Console.ResetColor();
            Console.WriteLine($"{hz}Hz");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Invalid speed: '{arg}'");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Speed must be a positive integer");
            Console.ResetColor();
        }
    }
    
    private void ShowHelp()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    AVAILABLE COMMANDS                     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        
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
        PrintHelpCommand("step, s", "Execute next instruction");
        PrintHelpCommand("speed <hz>", "Change clock speed");
        PrintHelpCommand("resume, continue, c", "Resume normal execution");
        Console.WriteLine();
        
        PrintHelpSection("Other:");
        PrintHelpCommand("clear, cls", "Clear screen");
        PrintHelpCommand("help, ?", "Show this help message");
        PrintHelpCommand("quit, exit", "Exit emulator");
    }
    
    private void PrintHelpSection(string section)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {section}");
        Console.ResetColor();
    }
    
    private void PrintHelpCommand(string command, string description)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"    {command.PadRight(20)}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {description}");
        Console.ResetColor();
    }
}
