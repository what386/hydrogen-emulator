namespace Emulator.Application;

public record EmulatorConfig(
    ushort[] RomData, 
    int ClockSpeedHz
);
