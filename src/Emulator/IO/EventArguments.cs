namespace Emulator.IO;

public class InterruptEventArgs : EventArgs
{
    public byte Vector { get; set; }
    public byte Data { get; set; }
    
    public InterruptEventArgs(byte vector = 0)
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
