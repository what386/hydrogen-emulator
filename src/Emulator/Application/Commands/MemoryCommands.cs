namespace Emulator.Application.Commands;

using Emulator.Models;

public static class MemoryCommands
{
    public static void Peek(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing address");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: peek <address>");
            Console.WriteLine("  Example: peek 0x1000 or peek 4096");
            Console.ResetColor();
            return;
        }
        
        int address;
        if (arg.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(arg.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out address))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid hex address: '{arg}'");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            if (!int.TryParse(arg, out address))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid address: '{arg}'");
                Console.ResetColor();
                return;
            }
        }
        
        if (address < 0 || address > 0xFFFF)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Address out of range: 0x{address:X}");
            Console.ResetColor();
            return;
        }
        
        byte value = state.RAM.ReadPool(address);
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  [0x{address:X4}] = ");
        Console.ResetColor();
        Console.Write($"0x{value:X2}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  ({value})");
        Console.ResetColor();
    }
    
    public static void Poke(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing arguments");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: poke <address> <value>");
            Console.WriteLine("  Example: poke 0x1000 0xFF or poke 4096 255");
            Console.ResetColor();
            return;
        }
        
        var parts = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Invalid arguments");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: poke <address> <value>");
            Console.ResetColor();
            return;
        }
        
        int address;
        if (parts[0].StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(parts[0].Substring(2), System.Globalization.NumberStyles.HexNumber, null, out address))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid hex address: '{parts[0]}'");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            if (!int.TryParse(parts[0], out address))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid address: '{parts[0]}'");
                Console.ResetColor();
                return;
            }
        }
        
        if (address < 0 || address > 0xFFFF)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Address out of range: 0x{address:X}");
            Console.ResetColor();
            return;
        }
        
        int value;
        if (parts[1].StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(parts[1].Substring(2), System.Globalization.NumberStyles.HexNumber, null, out value))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid hex value: '{parts[1]}'");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            if (!int.TryParse(parts[1], out value))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid value: '{parts[1]}'");
                Console.ResetColor();
                return;
            }
        }
        
        if (value < 0 || value > 0xFF)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Value out of range: {value}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Valid range: 0x00 - 0xFF (0 - 255)");
            Console.ResetColor();
            return;
        }
        
        byte oldValue = state.RAM.ReadPool(address);
        state.RAM.WritePool(address, (byte)value);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ MemoryCommands updated");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  [0x{address:X4}] ");
        Console.ResetColor();
        Console.Write($"0x{oldValue:X2}");
        Console.Write(" → ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"0x{value:X2}");
        Console.ResetColor();
    }
    
    public static void RegisterPeek(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing register number");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: rpeek <register>");
            Console.WriteLine("  Example: rpeek 0 or rpeek r3");
            Console.ResetColor();
            return;
        }
        
        string regStr = arg.ToLowerInvariant().TrimStart('r');
        if (!int.TryParse(regStr, out int regNum) || regNum < 0 || regNum > 7)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Invalid register: '{arg}'");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Valid registers: 0-7 or r0-r7");
            Console.ResetColor();
            return;
        }
        
        byte value = state.Registers.Read(regNum);
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  R{regNum} = ");
        Console.ResetColor();
        Console.Write($"0x{value:X4}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  ({value})");
        Console.ResetColor();
    }
    
    public static void RegisterPoke(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing arguments");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: rpoke <register> <value>");
            Console.WriteLine("  Example: rpoke 0 0x1234 or rpoke r3 4660");
            Console.ResetColor();
            return;
        }
        
        var parts = arg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Invalid arguments");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: rpoke <register> <value>");
            Console.ResetColor();
            return;
        }
        
        string regStr = parts[0].ToLowerInvariant().TrimStart('r');
        if (!int.TryParse(regStr, out int regNum) || regNum < 0 || regNum > 7)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Invalid register: '{parts[0]}'");
            Console.ResetColor();
            return;
        }
        
        int value;
        if (parts[1].StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(parts[1].Substring(2), System.Globalization.NumberStyles.HexNumber, null, out value))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid hex value: '{parts[1]}'");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            if (!int.TryParse(parts[1], out value))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid value: '{parts[1]}'");
                Console.ResetColor();
                return;
            }
        }
        
        if (value < 0 || value > 0xFFFF)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Value out of range: {value}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Valid range: 0x0000 - 0xFFFF (0 - 65535)");
            Console.ResetColor();
            return;
        }
        
        byte oldValue = state.Registers.Read(regNum);
        state.Registers.Write(regNum, (byte)value);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Register updated");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  R{regNum} ");
        Console.ResetColor();
        Console.Write($"0x{oldValue:X4}");
        Console.Write(" → ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"0x{value:X4}");
        Console.ResetColor();
    }
}
