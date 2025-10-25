namespace Emulator.IO;

public class InterruptEventArgs : EventArgs
{
    public byte Vector;
    public byte Port;
    
    public InterruptEventArgs(byte port, byte vector = 0)
    {
        Port = port;
        Vector = vector;
    }
}

public class InterruptRequestedEventArgs : EventArgs
{
    public byte Vector { get; set; }
    
    public InterruptRequestedEventArgs(byte vector = 0)
    {
        Vector = vector;
    }
}

public class DeviceWriteEventArgs : EventArgs
{
    public byte Data { get; set; }
    
    public DeviceWriteEventArgs(byte data)
    {
        Data = data;
    }
}
