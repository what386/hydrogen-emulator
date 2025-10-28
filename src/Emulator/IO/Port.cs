namespace Emulator.IO;

public class Port(int address)
{
    private readonly int address = address;
    private byte input;
    private byte output;
    private IDevice? connectedDevice = null;
    private int deviceOffset = 0; // This port's offset within the device's port range
    
    public event EventHandler<InterruptRequestedEventArgs>? InterruptRequested;
    
    public IDevice? ConnectedDevice => connectedDevice;
    public bool IsConnected => connectedDevice != null;
    
    internal void ConnectDevice(IDevice device, int offset)
    {
        if (connectedDevice != null)
            throw new InvalidOperationException($"Port {address} already has a device connected");
        
        connectedDevice = device;
        deviceOffset = offset;
        
        device.RequestInterrupt += OnDeviceInterrupt;
        device.WriteToPort += OnDeviceWrite;
        
        // Notify device of initial output state
        device.OnPortWrite(offset, output);
    }
    
    internal void DisconnectDevice()
    {
        if (connectedDevice == null)
            return;
        
        connectedDevice.RequestInterrupt -= OnDeviceInterrupt;
        connectedDevice.WriteToPort -= OnDeviceWrite;
        
        connectedDevice = null;
        deviceOffset = 0;
    }
    
    private void OnDeviceInterrupt(object? sender, InterruptRequestedEventArgs e)
        => InterruptRequested?.Invoke(sender, e);
    
    private void OnDeviceWrite(object? sender, DeviceWriteEventArgs e)
    {
        // Only respond to writes for this port's offset
        if (e.Offset == deviceOffset)
        {
            input = e.Data;
        }
    }
    
    public byte Read()
    {
        if (connectedDevice != null)
        {
            connectedDevice.OnPortRead(deviceOffset);
        }
        return input;
    }
    
    public void Write(byte data)
    {
        output = data;
        connectedDevice?.OnPortWrite(deviceOffset, data);
    }
    
    public byte ReadDirect() => input;
    public void WriteDirect(byte data) => output = data;
}
