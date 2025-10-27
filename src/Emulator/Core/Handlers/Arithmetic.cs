namespace Emulator.Core.Handlers;

using Emulator.Models;
using Emulator.Registers;

public static class Arithmetic
{
    public static void Ldi(MachineState state, Instruction instruction)
    {
        state.Registers.WriteDirect(instruction.ValueX, (byte)instruction.ValueY);
    }

    public static void Mov(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, state.Registers.Read(instruction.ValueY));
                state.Registers.Write(instruction.ValueY, 0);
                break;
            case 1:
                state.Registers.Write(instruction.ValueX, state.Registers.Read(instruction.ValueY));
                break;
            case 2:
                break;
            case 3:
                break;
        }
    }

    public static void Adi(MachineState state, Instruction instruction)
    {
        state.Registers.Write(instruction.ValueX, 
            state.ALU.Add(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY));
    }

    public static void Ani(MachineState state, Instruction instruction)
    {
        state.Registers.Write(instruction.ValueX, 
            state.ALU.And(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY));
    }

    public static void Ori(MachineState state, Instruction instruction)
    {
        state.Registers.Write(instruction.ValueX, 
            state.ALU.Or(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY));
    }

    public static void Xri(MachineState state, Instruction instruction)
    {
        state.Registers.Write(instruction.ValueX, 
            state.ALU.Xor(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY));
    }

    public static void Cpi(MachineState state, Instruction instruction)
    {
        state.ALU.Sub(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY);
    }

    public static void Tsi(MachineState state, Instruction instruction)
    {
        state.ALU.And(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY);
    }

    public static void Add(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.Add(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 1:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.AddCarry(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 2:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.AddVector(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 3:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.AddVectorCarry(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
        }
    }

    public static void Sub(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.Sub(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 1:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.SubBorrow(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 2:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.SubVector(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 3:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.SubVectorBorrow(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
        }
    }

    public static void Bit(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.Or(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 1:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.And(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 2:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.Xor(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 3:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.Implies(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
        }
    }

    public static void Bnt(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.Nor(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 1:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.Nand(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 2:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.Xnor(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
            case 3:
                state.Registers.Write(instruction.ValueX, 
                    state.ALU.Nimplies(
                        state.Registers.Read(instruction.ValueY), 
                        state.Registers.Read(instruction.ValueZ)));
                break;
        }
    }
}
