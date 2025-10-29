namespace Emulator.Application.Commands;

using Emulator.Models;
using Emulator.Core;

public static class ExecutionCommands
{
    public static void Step(MachineState state, string? arg)
    {
        int steps = 1;
        if (!string.IsNullOrWhiteSpace(arg))
        {
            if (!int.TryParse(arg, out steps) || steps < 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid step count: '{arg}'");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  Usage: step [count]");
                Console.ResetColor();
                return;
            }
        }
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine($"      STEPPING {steps} INSTRUCTION{(steps > 1 ? "S" : "")}");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        for (int i = 0; i < steps; i++)
        {
            var pcBefore = state.PC.Get();
            var binary = state.ROM.Read((ushort)pcBefore);
            var instruction = Decoder.Decode(binary);
            
            if (steps == 1 || i == 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("  Before: ");
                Console.ResetColor();
                Console.WriteLine($"PC = 0x{pcBefore:X4}");
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("  Instruction: ");
                Console.ResetColor();
                Console.WriteLine($"{instruction.Opcode}");
            }
            
            Executor.Execute(state, instruction);
            
            if (steps == 1 || i == steps - 1)
            {
                var pcAfter = state.PC.Get();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("  After: ");
                Console.ResetColor();
                Console.WriteLine($"PC = 0x{pcAfter:X4}");
            }
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n  ✓ Executed {steps} instruction{(steps > 1 ? "s" : "")}");
        Console.ResetColor();
    }
    
    public static void Until(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing address");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: until <address>");
            Console.WriteLine("  Example: until 0x1000 or until 4096");
            Console.ResetColor();
            return;
        }
        
        int targetAddress;
        if (arg.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(arg.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out targetAddress))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid hex address: '{arg}'");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            if (!int.TryParse(arg, out targetAddress))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid address: '{arg}'");
                Console.ResetColor();
                return;
            }
        }
        
        if (targetAddress < 0 || targetAddress > 0xFFFF)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Address out of range: 0x{targetAddress:X}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Valid range: 0x0000 - 0xFFFF");
            Console.ResetColor();
            return;
        }
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine($"    RUNNING UNTIL 0x{targetAddress:X4}");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        int instructionCount = 0;
        const int MAX_INSTRUCTIONS = 1000000;
        
        while (state.PC.Get() != targetAddress && instructionCount < MAX_INSTRUCTIONS)
        {
            var binary = state.ROM.Read((ushort)state.PC.Get());
            var instruction = Decoder.Decode(binary);
            Executor.Execute(state, instruction);
            instructionCount++;
        }
        
        if (state.PC.Get() == targetAddress)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ Reached target address 0x{targetAddress:X4}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  Instructions executed: ");
            Console.ResetColor();
            Console.WriteLine($"{instructionCount}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ Safety limit reached ({MAX_INSTRUCTIONS} instructions)");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("  Current PC: ");
            Console.ResetColor();
            Console.WriteLine($"0x{state.PC.Get():X4}");
        }
    }
    
    public static void SetSpeed(MachineState state, string? arg)
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
}
