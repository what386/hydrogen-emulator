namespace Emulator.IO;

/// <summary>
/// I/O Devices should be located in the "Emulator.IO.Devices" namespace.
/// Both custom EventArgs are defined in the "Emulator.IO" namespace.
/// </summary>
public interface IDevice
{
    /// <summary>
    /// Number of consecutive I/O ports this device requires.
    /// The device will be allocated ports [basePort, basePort + PortCount).
    /// Must be at least 1.
    /// </summary>
    int PortCount { get; }
    
    /// <summary>
    /// Raised by the device when it wants to request a processor interrupt.
    /// InterruptRequestedEventArgs is constructed with one byte to be used as the interrupt vector.
    /// Example: RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(vector));
    /// </summary>
    event EventHandler<InterruptRequestedEventArgs>? RequestInterrupt;
    
    /// <summary>
    /// Raised by the device when it wants to write data out.
    /// DeviceWriteEventArgs is constructed with a port offset and data byte.
    /// The port offset is relative to the device's base port (0 to PortCount-1).
    /// Example: WriteToPort?.Invoke(this, new DeviceWriteEventArgs(offset, data));
    /// </summary>
    event EventHandler<DeviceWriteEventArgs>? WriteToPort;
    
    /// <summary>
    /// Called by the processor when it writes data to one of the device's I/O ports.
    /// </summary>
    /// <param name="offset">Port offset from device base (0 to PortCount-1)</param>
    /// <param name="data">Data byte written</param>
    void OnPortWrite(int offset, byte data);
    
    /// <summary>
    /// Called by the processor when it reads data from one of the device's I/O ports.
    /// </summary>
    /// <param name="offset">Port offset from device base (0 to PortCount-1)</param>
    void OnPortRead(int offset);
    
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
