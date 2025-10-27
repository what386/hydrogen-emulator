namespace Emulator.Core.Handlers;

using Emulator.Models;
using Emulator.Registers;

public static class Compute
{
    public static void Bsh(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, 
                    state.BSU.ShiftLeftLogical(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 1:
                state.Registers.Write(instruction.ValueX, 
                    state.BSU.ShiftRightLogical(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 2:
                state.Registers.Write(instruction.ValueX, 
                    state.BSU.RotateLeft(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 3:
                state.Registers.Write(instruction.ValueX, 
                    state.BSU.ShiftRightArithmetic(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
        }
    }

    public static void Bsi(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, 
                    state.BSU.ShiftLeftLogical(
                        state.Registers.Read(instruction.ValueY), 
                        (byte)instruction.ValueZ));
                break;
            case 1:
                state.Registers.Write(instruction.ValueX, 
                    state.BSU.ShiftRightLogical(
                        state.Registers.Read(instruction.ValueY), 
                        (byte)instruction.ValueZ));
                break;
            case 2:
                state.Registers.Write(instruction.ValueX, 
                    state.BSU.RotateLeft(
                        state.Registers.Read(instruction.ValueY), 
                        (byte)instruction.ValueZ));
                break;
            case 3:
                state.Registers.Write(instruction.ValueX, 
                    state.BSU.ShiftRightArithmetic(
                        state.Registers.Read(instruction.ValueY), 
                        (byte)instruction.ValueZ));
                break;
        }
    }

    public static void Mul(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, 
                    state.CMU.MultiplyLow(
                        state.Registers.Read(instruction.ValueY), 
                        (byte)instruction.ValueZ));
                break;
            case 1:
                state.Registers.Write(instruction.ValueX, 
                    state.CMU.MultiplyHigh(
                        state.Registers.Read(instruction.ValueY), 
                        (byte)instruction.ValueZ));
                break;
            case 2:
                state.Registers.Write(instruction.ValueX, 
                    state.CMU.Divide(
                        state.Registers.Read(instruction.ValueY), 
                        (byte)instruction.ValueZ));
                break;
            case 3:
                state.Registers.Write(instruction.ValueX, 
                    state.CMU.Modulo(
                        state.Registers.Read(instruction.ValueY), 
                        (byte)instruction.ValueZ));
                break;
        }
    }

    public static void Btc(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, 
                    state.CMU.SquareRoot(state.Registers.Read(instruction.ValueY)));
                break;
            case 1:
                state.Registers.Write(instruction.ValueX, 
                    state.CMU.CountLeadingZeros(state.Registers.Read(instruction.ValueY)));
                break;
            case 2:
                state.Registers.Write(instruction.ValueX, 
                    state.CMU.CountTrailingZeros(state.Registers.Read(instruction.ValueY)));
                break;
            case 3:
                state.Registers.Write(instruction.ValueX, 
                    state.CMU.CountOnes(state.Registers.Read(instruction.ValueY)));
                break;
        }
    }
}
