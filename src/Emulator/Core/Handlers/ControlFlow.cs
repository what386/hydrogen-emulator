namespace Emulator.Core.Handlers;

using Emulator.Models;
using Emulator.Registers;

public static class ControlFlow
{
    public static void Cli(MachineState state, Instruction instruction)
    {
        bool condition = state.StatusWord.CheckCondition(
            instruction.ValueY, 
            state.ControlWord.GetFlag(ControlWord.ALT_CONDITIONS));
        
        if (!condition)
            return;
        
        state.Registers.WriteDirect(instruction.ValueX, (byte)instruction.ValueZ);
    }

    public static void Jmp(MachineState state, Instruction instruction)
    {
        state.PC.Jump(instruction.ValueX, state.ControlWord.GetFlag(ControlWord.PAGE_JUMP_MODE));
    }

    public static void Bra(MachineState state, Instruction instruction)
    {
        bool condition = state.StatusWord.CheckCondition(
            instruction.ValueY, 
            state.ControlWord.GetFlag(ControlWord.ALT_CONDITIONS));
        
        if (!condition)
            return;
        
        state.PC.Jump(instruction.ValueY, state.ControlWord.GetFlag(ControlWord.PAGE_JUMP_MODE));
    }

    public static void Cal(MachineState state, Instruction instruction)
    {
        state.CallStack.Push(state.PC.Get());
        state.PC.Jump(instruction.ValueX, state.ControlWord.GetFlag(ControlWord.PAGE_JUMP_MODE));
    }

    public static void Ret(MachineState state, Instruction instruction)
    {
        if (instruction.Type == 0)
        {
            state.PC.Jump(state.CallStack.Pop(), 
                state.ControlWord.GetFlag(ControlWord.PAGE_JUMP_MODE));
        }
        else
        {
            state.PC.Jump(state.CallStack.GetOldest(), 
                state.ControlWord.GetFlag(ControlWord.PAGE_JUMP_MODE));
            state.CallStack.Clear();
        }
    }
}
