namespace Emulator.IO;

public class InterruptEventArgs : EventArgs
{
    public byte Vector { get; set; }
    public byte Port { get; set; }
    
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
    public int Offset { get; set; }
    public byte Data { get; set; }
    
    public DeviceWriteEventArgs(int offset, byte data)
    {
        Offset = offset;
        Data = data;
    }
}
