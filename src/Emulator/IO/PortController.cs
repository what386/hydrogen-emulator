namespace Emulator.IO;

public class PortController
{
    public const int PORT_AMOUNT = 8;
    private Port[] ports;
    
    public PortController()
    {
        ports = new Port[PORT_AMOUNT];
        
        for (int i = 0; i < PORT_AMOUNT; i++)
        {
            ports[i] = new Port();
            
            // Subscribe to each port's interrupt and inject port address
            byte portAddress = (byte)i;
            ports[i].InterruptRequested += (sender, e) => 
                OnPortInterrupt(sender, portAddress, e);
        }
    }
    
    // This event forwards to CPU with port address
    public event EventHandler<InterruptEventArgs>? InterruptRequested;
    
    public void ConnectDeviceToPort(byte address, IDevice device)
    {
        ports[address].ConnectDevice(device);
    }
    
    public void DisconnectDeviceFromPort(byte address)
    {
        ports[address].DisconnectDevice();
    }
    
    private void OnPortInterrupt(object sender, byte portAddress, InterruptRequestedEventArgs e)
    {
        InterruptRequested?.Invoke(sender, new InterruptEventArgs(portAddress, e.Vector));
    }
    
    public byte Read(byte address) => ports[address].Read();
    public void Write(byte address, byte data) => ports[address].Write(data);

    public byte ReadDirect(byte address) => ports[address].ReadDirect();
    public byte WriteDirect(byte address) => ports[address].WriteDirect();
}
