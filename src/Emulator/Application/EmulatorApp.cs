namespace Emulator.Application;

using Emulator.Core;
using Emulator.IO.Devices;
using Emulator.Models;

public class EmulatorApp
{
    private readonly EmulatorConfig config;
    private readonly MachineState state = new();
    private bool isRunning = false;
    private bool inCommandMode = false;

    public EmulatorApp(EmulatorConfig config)
    {
        this.config = config;
    }

    public void Run()
    {
        Setup();

        Console.WriteLine($"Clock started at {state.Clock.ClockSpeedHz}Hz");
        Console.WriteLine("Press Ctrl+C to enter command mode.\n");

        state.Clock.OnTick += OnClockTick;

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // prevent app exit
            EnterCommandMode();
        };

        state.Clock.Start();
        isRunning = true;

        while (isRunning)
            Thread.Sleep(100);

        Shutdown();
    }

    private void Setup()
    {
        Console.WriteLine("Flashing ROM...");
        state.ROM.Flash(config.RomData);
        state.Clock.SetSpeed(config.ClockSpeedHz);

        Console.WriteLine("Connecting devices...");
        state.PortController.ConnectDevice(0, new SerialTerminal());
        _ = state.PortController.StartAllDevicesAsync();
    }

    private void Shutdown()
    {
        Console.WriteLine("Shutting down...");
        state.Clock.Stop();
        _ = state.PortController.StopAllDevicesAsync();
    }

    private void OnClockTick()
    {
        var binary = state.ROM.Read((ushort)state.PC.Get());
        var instruction = Decoder.Decode(binary);
        Executor.Execute(state, instruction);
    }

    private void EnterCommandMode()
    {
        if (inCommandMode)
            return;

        Console.WriteLine("\n\n--- Command Mode ---");
        inCommandMode = true;
        state.Clock.Stop();

        var cmdMode = new CommandMode(state);
        cmdMode.Run();

        Console.WriteLine("--- Resuming Execution ---\n");
        state.Clock.Start();
        inCommandMode = false;
    }
}
