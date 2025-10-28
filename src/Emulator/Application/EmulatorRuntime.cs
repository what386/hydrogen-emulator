namespace Emulator.Application;

using Emulator.Core;
using Emulator.IO.Devices;
using Emulator.Models;

public class EmulatorRuntime
{
    private readonly EmulatorConfig config;
    private readonly MachineState state = new();
    private bool isRunning = false;
    private bool inCommandMode = false;

    public EmulatorRuntime(EmulatorConfig config)
    {
        this.config = config;
    }

    public void Run()
    {
        Setup();

        state.Clock.OnTick += OnClockTick;

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // prevent app exit
            EnterCommandMode();
        };

        state.Clock.Start();

        Console.WriteLine($"Clock started at {state.Clock.ClockSpeedHz}Hz");
        Console.WriteLine("Press Ctrl+C to enter command mode.\n");

        isRunning = true;

        while (isRunning)
            Thread.Sleep(100);

        Shutdown();
    }

    private void Setup()
    {
        TerminalControl.DisableEcho();

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
        Interruptor.HandleInterrupts(state);
        var binary = state.ROM.Read((ushort)state.PC.Get());
        var instruction = Decoder.Decode(binary);
        Executor.Execute(state, instruction);
    }

    private void EnterCommandMode()
    {
        if (inCommandMode)
            return;

        TerminalControl.EnableEcho();

        Console.WriteLine("\n\n--- Command Mode ---");

        inCommandMode = true;
        state.Clock.Stop();

        var cmdMode = new CommandMode(state);
        cmdMode.Run();

        Console.WriteLine("--- Resuming Execution ---\n");

        TerminalControl.DisableEcho();

        state.Clock.Start();
        inCommandMode = false;
    }
}
