namespace Emulator.IO;

public class Port
{
    private byte input;
    private byte output;
    private IDevice? connectedDevice = null;
    
    public event EventHandler<InterruptRequestedEventArgs>? InterruptRequested;
    
    public void ConnectDevice(IDevice device)
    {
        connectedDevice = device;
        
        device.RequestInterrupt += OnDeviceInterrupt;
        device.WriteToPort += OnDeviceWrite;
        connectedDevice.OnPortWrite(output);
    }
    
    public void DisconnectDevice()
    {
        if (connectedDevice == null)
            return;
        
        connectedDevice.RequestInterrupt -= OnDeviceInterrupt;
        connectedDevice.WriteToPort -= OnDeviceWrite;
        
        connectedDevice = null;
    }

    public void StartDeviceAsync()
    {
        if (connectedDevice == null)
            return;

        connectedDevice.StartAsync();
    }

    public async Task StopDeviceAsync()
    {
        if (connectedDevice == null)
            return;
        
        try
        {
            // Wait max 10 seconds for device to stop
            await connectedDevice.StopAsync().WaitAsync(TimeSpan.FromSeconds(10));
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Warning: Device did not stop within 10 seconds, continuing anyway");
        }
    }
    
    // Forward the event unchanged
    private void OnDeviceInterrupt(object sender, InterruptRequestedEventArgs e) 
        => InterruptRequested?.Invoke(sender, e);
    
    private void OnDeviceWrite(object sender, DeviceWriteEventArgs e) 
        => input = e.Data;
    
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
    public byte WriteDirect(byte data) => output = data;
}
