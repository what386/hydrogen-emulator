namespace Emulator.Application.Commands;

using Emulator.Models;

public static class WatchpointCommands
{
    private class Watchpoint
    {
        public int Address { get; set; }
        public string? Name { get; set; }
        public byte LastValue { get; set; }
        
        public Watchpoint(int address, string? name, byte initialValue)
        {
            Address = address;
            Name = name;
            LastValue = initialValue;
        }
    }
    
    private static Dictionary<int, Watchpoint> watchpoints = new();
    
    public static void AddWatch(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing address");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: watch <address> [name]");
            Console.WriteLine("  Example: watch 0x1000");
            Console.WriteLine("  Example: watch 0x1000 player_health");
            Console.ResetColor();
            return;
        }
        
        var parts = arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string addressStr = parts[0];
        string? name = parts.Length > 1 ? parts[1] : null;
        
        int address;
        if (addressStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(addressStr.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out address))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid hex address: '{addressStr}'");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            if (!int.TryParse(addressStr, out address))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid address: '{addressStr}'");
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
        
        if (watchpoints.ContainsKey(address))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ Watchpoint already exists at 0x{address:X4}");
            Console.ResetColor();
            return;
        }
        
        byte currentValue = state.RAM.ReadPool(address);
        watchpoints[address] = new Watchpoint(address, name, currentValue);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"  ✓ Watchpoint set at 0x{address:X4}");
        Console.ResetColor();
        
        if (!string.IsNullOrWhiteSpace(name))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" ({name})");
            Console.ResetColor();
        }
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"    Current value: 0x{currentValue:X2} ({currentValue})");
        Console.ResetColor();
    }
    
    public static void RemoveWatch(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing address");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: unwatch <address>");
            Console.WriteLine("  Usage: unwatch all");
            Console.ResetColor();
            return;
        }
        
        if (arg.ToLowerInvariant() == "all")
        {
            int count = watchpoints.Count;
            watchpoints.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ Removed {count} watchpoint{(count != 1 ? "s" : "")}");
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
        
        if (watchpoints.Remove(address))
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ Removed watchpoint at 0x{address:X4}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ No watchpoint at 0x{address:X4}");
            Console.ResetColor();
        }
    }
    
    public static void ListWatches(MachineState state)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("          WATCHPOINTS              ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        if (watchpoints.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  No watchpoints set");
            Console.ResetColor();
            return;
        }
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  Total: {watchpoints.Count}");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Address  Value  Name");
        Console.WriteLine("  ───────  ─────  ────────────────");
        Console.ResetColor();
        
        foreach (var watch in watchpoints.Values.OrderBy(w => w.Address))
        {
            byte currentValue = state.RAM.ReadPool(watch.Address);
            bool changed = currentValue != watch.LastValue;
            
            if (changed)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            
            Console.Write($"  0x{watch.Address:X4}  ");
            Console.Write($"0x{currentValue:X2}  ");
            
            if (!string.IsNullOrWhiteSpace(watch.Name))
            {
                Console.Write(watch.Name);
            }
            
            if (changed)
            {
                Console.Write($" (was 0x{watch.LastValue:X2})");
            }
            
            Console.WriteLine();
            Console.ResetColor();
        }
    }
    
    public static void UpdateWatches(MachineState state)
    {
        // Call this during execution to update watchpoint values
        foreach (var watch in watchpoints.Values)
        {
            watch.LastValue = state.RAM.ReadPool(watch.Address);
        }
    }
    
    public static bool CheckWatches(MachineState state)
    {
        // Returns true if any watchpoint changed (to trigger a break)
        bool anyChanged = false;
        
        foreach (var watch in watchpoints.Values)
        {
            byte currentValue = state.RAM.ReadPool(watch.Address);
            if (currentValue != watch.LastValue)
            {
                anyChanged = true;
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"\n⚠ Watchpoint 0x{watch.Address:X4}");
                Console.ResetColor();
                
                if (!string.IsNullOrWhiteSpace(watch.Name))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($" ({watch.Name})");
                    Console.ResetColor();
                }
                
                Console.Write($" changed: 0x{watch.LastValue:X2} → 0x{currentValue:X2}\n");
                
                watch.LastValue = currentValue;
            }
        }
        
        return anyChanged;
    }
}
