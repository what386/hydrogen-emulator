namespace Emulator.Application.Commands;

using Emulator.Models;

public static class BreakpointCommands
{
    private static HashSet<int> breakpoints = new();
    
    public static bool IsBreakpoint(int address) => breakpoints.Contains(address);
    
    public static void SetBreakpoint(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing address");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: break <address>");
            Console.WriteLine("  Example: break 0x1000 or break 4096");
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
        
        if (breakpoints.Contains(address))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ Breakpoint already exists at 0x{address:X4}");
            Console.ResetColor();
            return;
        }
        
        breakpoints.Add(address);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Breakpoint set at 0x{address:X4}");
        Console.ResetColor();
    }
    
    public static void DeleteBreakpoint(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing address");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: delete <address>");
            Console.WriteLine("  Usage: delete all");
            Console.ResetColor();
            return;
        }
        
        if (arg.ToLowerInvariant() == "all")
        {
            int count = breakpoints.Count;
            breakpoints.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ Deleted {count} breakpoint{(count != 1 ? "s" : "")}");
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
        
        if (breakpoints.Remove(address))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ Deleted breakpoint at 0x{address:X4}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ No breakpoint at 0x{address:X4}");
            Console.ResetColor();
        }
    }
    
    public static void ListBreakpoints(MachineState state)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("          BREAKPOINTS              ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        if (breakpoints.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  No breakpoints set");
            Console.ResetColor();
            return;
        }
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  Total: {breakpoints.Count}");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Address");
        Console.WriteLine("  ───────");
        Console.ResetColor();
        
        foreach (var address in breakpoints.OrderBy(a => a))
        {
            Console.WriteLine($"  0x{address:X4}");
        }
    }
}
