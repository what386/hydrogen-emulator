namespace Emulator.IO;

public class Port
{
    private byte input;
    private byte output;
    private IDevice? connectedDevice = null;
    
    // Event to notify CPU of interrupts
    public event EventHandler? InterruptRequested;
    
    public void ConnectDevice(IDevice device)
    {
        connectedDevice = device;
        
        device.RequestInterrupt += OnDeviceInterrupt;
        device.WriteToPort += OnDeviceWrite;

        connectedDevice.OnPortWrite(output);
    }
    
    private void OnDeviceInterrupt(object sender, InterruptEventArgs e) => InterruptRequested?.Invoke(sender, e);
    
    private void OnDeviceWrite(object sender, DeviceWriteEventArgs e) => input = e.Data;
    
    public byte Read()
    {
        if (connectedDevice != null)
            input = connectedDevice.OnPortRead();

        return input;
    }
    
    public void Write(byte data)
    {
        output = data;
        
        connectedDevice?.OnPortWrite(output);
    }

    public byte ReadDirect() => input;
}
