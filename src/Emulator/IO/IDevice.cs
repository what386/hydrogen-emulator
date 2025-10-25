namespace Emulator.IO;

public interface IDevice
{
    event EventHandler<InterruptRequestedEventArgs> RequestInterrupt;
    event EventHandler<DeviceWriteEventArgs> WriteToPort;
    
    // Device receives notification when port is written to
    void OnPortWrite(byte data);
    
    // Device can provide data when port is read from
    byte OnPortRead();

    Task StartAsync();
    Task StopAsync();
}
