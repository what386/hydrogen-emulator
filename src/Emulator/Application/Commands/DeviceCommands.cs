namespace Emulator.Application.Commands;

using Emulator.Models;
using IDevice = Emulator.IO.IDevice;

public static class DeviceCommands
{
    public static void ListDevices(MachineState state)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("        CONNECTED DEVICES          ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        // Get all unique devices by checking ports
        var deviceMap = new Dictionary<int, (string deviceType, int portCount)>();
        
        for (int port = 0; port < 256; port++)
        {
            var device = GetDeviceAtPort(state, port);
            if (device != null)
            {
                int basePort = GetBasePort(state, device);
                if (!deviceMap.ContainsKey(basePort))
                {
                    string deviceType = device.GetType().Name;
                    int portCount = GetDevicePortCount(device);
                    deviceMap[basePort] = (deviceType, portCount);
                }
            }
        }
        
        if (deviceMap.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  No devices connected");
            Console.ResetColor();
            return;
        }
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  Total: {deviceMap.Count} device{(deviceMap.Count != 1 ? "s" : "")}");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Base Port  Ports  Device Type");
        Console.WriteLine("  ─────────  ─────  ──────────────────");
        Console.ResetColor();
        
        foreach (var kvp in deviceMap.OrderBy(x => x.Key))
        {
            int basePort = kvp.Key;
            var (deviceType, portCount) = kvp.Value;
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  0x{basePort:X2}       ");
            Console.ResetColor();
            
            Console.Write($"{portCount}      ");
            Console.WriteLine(deviceType);
        }
    }
    
    public static void ShowPorts(MachineState state)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("            PORT MAP               ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Showing ports 0x00 - 0xFF");
        Console.WriteLine();
        Console.WriteLine("  Port  Status      Device");
        Console.WriteLine("  ────  ──────      ──────────────────");
        Console.ResetColor();
        
        int connectedCount = 0;
        
        for (int port = 0; port < 256; port++)
        {
            var device = GetDeviceAtPort(state, port);
            
            if (device != null)
            {
                connectedCount++;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"  0x{port:X2}  ");
                Console.ResetColor();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Connected");
                Console.ResetColor();
                
                Console.Write($"   {device.GetType().Name}");
                
                // Show if this is an offset port
                int basePort = GetBasePort(state, device);
                if (basePort != port)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write($" (offset {port - basePort} from 0x{basePort:X2})");
                    Console.ResetColor();
                }
                
                Console.WriteLine();
            }
        }
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  Total connected: {connectedCount}/256");
        Console.ResetColor();
    }
    
    public static void DeviceInfo(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing port number");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: device <port>");
            Console.WriteLine("  Example: device 0x00 or device 0");
            Console.ResetColor();
            return;
        }
        
        int port;
        if (arg.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(arg.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out port))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid hex port: '{arg}'");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            if (!int.TryParse(arg, out port))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid port: '{arg}'");
                Console.ResetColor();
                return;
            }
        }
        
        if (port < 0 || port > 255)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Port out of range: {port}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Valid range: 0x00 - 0xFF (0 - 255)");
            Console.ResetColor();
            return;
        }
        
        var device = GetDeviceAtPort(state, port);
        
        if (device == null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ No device connected to port 0x{port:X2}");
            Console.ResetColor();
            return;
        }
        
        int basePort = GetBasePort(state, device);
        int portCount = GetDevicePortCount(device);
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════════════════════");
        Console.WriteLine("         DEVICE INFORMATION        ");
        Console.WriteLine("═══════════════════════════════════");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Device Type:  ");
        Console.ResetColor();
        Console.WriteLine(device.GetType().Name);
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Base Port:    ");
        Console.ResetColor();
        Console.WriteLine($"0x{basePort:X2}");
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Port Count:   ");
        Console.ResetColor();
        Console.WriteLine(portCount);
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("  Port Range:   ");
        Console.ResetColor();
        Console.WriteLine($"0x{basePort:X2} - 0x{(basePort + portCount - 1):X2}");
        
        if (port != basePort)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Note: Port 0x{port:X2} is offset {port - basePort} of this device");
            Console.ResetColor();
        }
    }
    
    public static void ReadPort(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing port number");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: inport <port>");
            Console.WriteLine("  Example: inport 0x00 or inport 0");
            Console.ResetColor();
            return;
        }
        
        int port;
        if (arg.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(arg.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out port))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid hex port: '{arg}'");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            if (!int.TryParse(arg, out port))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid port: '{arg}'");
                Console.ResetColor();
                return;
            }
        }
        
        if (port < 0 || port > 255)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Port out of range: {port}");
            Console.ResetColor();
            return;
        }
        
        byte value = state.PortController.Read(port);
        
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  Port 0x{port:X2} = ");
        Console.ResetColor();
        Console.Write($"0x{value:X2}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  ({value})");
        Console.ResetColor();
    }
    
    public static void WritePort(MachineState state, string? arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Missing arguments");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Usage: outport <port> <value>");
            Console.WriteLine("  Example: outport 0x00 0xFF or outport 0 255");
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
            Console.WriteLine("  Usage: outport <port> <value>");
            Console.ResetColor();
            return;
        }
        
        int port;
        if (parts[0].StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(parts[0].Substring(2), System.Globalization.NumberStyles.HexNumber, null, out port))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid hex port: '{parts[0]}'");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            if (!int.TryParse(parts[0], out port))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Invalid port: '{parts[0]}'");
                Console.ResetColor();
                return;
            }
        }
        
        if (port < 0 || port > 255)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Port out of range: {port}");
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
            return;
        }
        
        state.PortController.Write(port, (byte)value);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Wrote 0x{value:X2} to port 0x{port:X2}");
        Console.ResetColor();
    }
    
    private static Emulator.IO.IDevice? GetDeviceAtPort(MachineState state, int port)
    {
        return state.PortController.GetDeviceFromPort(port);
    }
    
    private static int GetBasePort(MachineState state, IDevice device)
    {
        return state.PortController.GetDeviceBasePort(device);
    }
    
    private static int GetDevicePortCount(IDevice device)
    {
        return device.PortCount;
    }
}
