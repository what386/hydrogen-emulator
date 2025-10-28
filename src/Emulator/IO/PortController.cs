namespace Emulator.IO;
using Emulator.Registers;

public class PortController
{
    public const int PORT_AMOUNT = 256; // Standard 8-bit I/O space
    private readonly Port[] ports;
    private readonly Dictionary<IDevice, int> deviceBasePorts; // Track device base addresses
    private InterruptVector interruptVector;
    
    public PortController(InterruptVector interruptVector)
    {
        ports = new Port[PORT_AMOUNT];
        deviceBasePorts = new Dictionary<IDevice, int>();
        this.interruptVector = interruptVector;
        
        for (int i = 0; i < PORT_AMOUNT; i++)
        {
            ports[i] = new Port(i);
        }
    }
    
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
        
        // Subscribe to device interrupts at the controller level (only once per device)
        device.RequestInterrupt += OnDeviceInterrupt;
        
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
        
        // Unsubscribe from device interrupts
        device.RequestInterrupt -= OnDeviceInterrupt;
        
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
    
    public async Task StartAllDevicesAsync()
    {
        var tasks = deviceBasePorts.Keys.Select(d => d.StartAsync()).ToArray();
        await Task.WhenAll(tasks);
    }
    
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
    public int GetDeviceBasePort(IDevice device) =>
        deviceBasePorts.TryGetValue(device, out int basePort) ? basePort : -1;
    
    /// <summary>
    /// Handles interrupt requests from devices.
    /// Uses the device's base port address as the interrupt source.
    /// </summary>
    private void OnDeviceInterrupt(object? sender, InterruptRequestedEventArgs e)
    {
        if (sender is IDevice device && deviceBasePorts.TryGetValue(device, out int basePort))
        {
            interruptVector.RequestInterrupt((byte)basePort, e.Vector);
        }
    }
    
    public byte Read(int address) => ports[address].Read();
    public void Write(int address, byte data) => ports[address].Write(data);
    public byte ReadDirect(int address) => ports[address].ReadDirect();
}
