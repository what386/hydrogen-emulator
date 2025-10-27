namespace Emulator.IO;

/// <summary>
/// I/O Devices should be located in the "Emulator.IO.Devices" namespace.
/// Both custom EventArgs are defined in the "Emulator.IO" namespace.
/// </summary>
public interface IDevice
{
    /// <summary>
    /// Raised by the device when it wants to request a processor interrupt.
    /// InterruptRequestedEventArgs is constructed with one byte to be used as the interrupt vector.
    /// Example: RequestInterrupt?.invoke(this, new InterruptRequestedEventArgs(byte));
    /// </summary>
    event EventHandler<InterruptRequestedEventArgs> RequestInterrupt;
    
    /// <summary>
    /// Raised by the device when it wants to write data out.
    /// DeviceWriteEventArgs is constructed with one byte to be used as data.
    /// Example: WriteToPort?.invoke(this, new DeviceWriteEventArgs(byte));
    /// </summary>
    event EventHandler<DeviceWriteEventArgs> WriteToPort;
    
    /// <summary>
    /// Called by the processor when it writes data to the I/O port the device is connected to.
    /// </summary>
    void OnPortWrite(byte data);
    
    /// <summary>
    /// Called by the processor when it reads data from the I/O port the device is connected to.
    /// </summary>
    void OnPortRead();
    
    /// <summary>
    /// Sent by the processor when it starts the connected device.
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// Sent by the processor when it stops the connected device.
    /// The device is forcibly disconnected after ten seconds if the Task is not completed.
    /// </summary>
    Task StopAsync();
}
