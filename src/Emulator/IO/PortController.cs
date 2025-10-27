namespace Emulator.IO;

public class PortController
{
    public const int PORT_AMOUNT = 256; // Standard 8-bit I/O space
    private readonly Port[] ports;
    private readonly Dictionary<IDevice, int> deviceBasePorts; // Track device base addresses
    
    public PortController()
    {
        ports = new Port[PORT_AMOUNT];
        deviceBasePorts = new Dictionary<IDevice, int>();
        
        for (int i = 0; i < PORT_AMOUNT; i++)
        {
            ports[i] = new Port(i);
            
            // Subscribe to each port's interrupt and inject port address
            byte portAddress = (byte)i;
            ports[i].InterruptRequested += (sender, e) => 
                OnPortInterrupt(sender, portAddress, e);
        }
    }
    
    // This event forwards to CPU with port address
    public event EventHandler<InterruptEventArgs>? InterruptRequested;
    
    /// <summary>
    /// Connects a device to a contiguous block of ports starting at baseAddress.
    /// The device will occupy ports [baseAddress, baseAddress + device.PortCount).
    /// </summary>
    /// <param name="baseAddress">Starting port address</param>
    /// <param name="device">Device to connect</param>
    /// <returns>True if successful, false if ports are already occupied</returns>
    public bool ConnectDevice(int baseAddress, IDevice device)
    {
        if (device.PortCount < 1)
            throw new ArgumentException("Device must require at least 1 port", nameof(device));
        
        if (baseAddress < 0 || baseAddress + device.PortCount > PORT_AMOUNT)
            throw new ArgumentException($"Device requires {device.PortCount} ports but address {baseAddress} would exceed port space", nameof(baseAddress));
        
        // Check if all required ports are available
        for (int i = 0; i < device.PortCount; i++)
        {
            if (ports[baseAddress + i].IsConnected)
                return false; // Port already occupied
        }
        
        // Connect device to all required ports
        for (int i = 0; i < device.PortCount; i++)
        {
            ports[baseAddress + i].ConnectDevice(device, i);
        }
        
        deviceBasePorts[device] = baseAddress;
        return true;
    }
    
    /// <summary>
    /// Disconnects a device from all its ports.
    /// </summary>
    public void DisconnectDevice(IDevice device)
    {
        if (!deviceBasePorts.TryGetValue(device, out int baseAddress))
            return; // Device not connected
        
        for (int i = 0; i < device.PortCount; i++)
        {
            ports[baseAddress + i].DisconnectDevice();
        }
        
        deviceBasePorts.Remove(device);
    }
    
    /// <summary>
    /// Disconnects device from a specific base port address.
    /// </summary>
    public void DisconnectDeviceFromPort(int baseAddress)
    {
        if (baseAddress < 0 || baseAddress >= PORT_AMOUNT)
            return;
        
        var device = ports[baseAddress].ConnectedDevice;
        if (device != null)
        {
            DisconnectDevice(device);
        }
    }
    
    /// <summary>
    /// Starts all connected devices.
    /// </summary>
    public async Task StartAllDevicesAsync()
    {
        var tasks = deviceBasePorts.Keys.Select(d => d.StartAsync()).ToArray();
        await Task.WhenAll(tasks);
    }
    
    /// <summary>
    /// Stops all connected devices.
    /// </summary>
    public async Task StopAllDevicesAsync()
    {
        var stopTasks = deviceBasePorts.Keys.Select(async device =>
        {
            try
            {
                await device.StopAsync().WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"Warning: Device at port {deviceBasePorts[device]} did not stop within 10 seconds");
            }
        });
        
        await Task.WhenAll(stopTasks);
    }
    
    /// <summary>
    /// Gets the base port address of a connected device, or -1 if not connected.
    /// </summary>
    public int GetDeviceBasePort(IDevice device)
    {
        return deviceBasePorts.TryGetValue(device, out int basePort) ? basePort : -1;
    }
    
    private void OnPortInterrupt(object? sender, byte portAddress, InterruptRequestedEventArgs e)
    {
        InterruptRequested?.Invoke(sender, new InterruptEventArgs(portAddress, e.Vector));
    }
    
    public byte Read(int address) => ports[address].Read();
    public void Write(int address, byte data) => ports[address].Write(data);
    public byte ReadDirect(int address) => ports[address].ReadDirect();
}
