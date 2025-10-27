namespace Emulator.Core.Handlers;

using Emulator.Models;

public static class InputOutput
{
    public static void Inp(MachineState state, Instruction instruction)
    {
        state.Registers.Write(instruction.ValueX, state.PortController.Read(instruction.ValueY));
    }

    public static void Out(MachineState state, Instruction instruction)
    {
        state.PortController.Write(instruction.ValueY, state.Registers.Read(instruction.ValueX));
    }
}
