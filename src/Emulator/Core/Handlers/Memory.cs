namespace Emulator.Core.Handlers;

using Emulator.Models;
using Emulator.Registers;

public static class Memory
{
    public static void Sld(MachineState state, Instruction instruction)
    {
        switch (instruction.ValueY)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, (byte)state.RAM.AddressPointer);
                break;
                
            case 1:
                state.Registers.Write(instruction.ValueX, (byte)state.CallStack.StackPointer);
                break;
                
            case 2:
                state.Registers.Write(instruction.ValueX, state.StatusWord.Flags);
                break;
            
            case 3:
                state.Registers.Write(instruction.ValueX, state.ControlWord.Flags);
                break;
            
            case 4:
                state.Registers.Write(instruction.ValueX, (byte)state.LoopRegister.LoopPointer);
                break;
            
            case 5:
                state.Registers.Write(instruction.ValueX, (byte)state.PC.BranchOffset);
                break;
            
            case 6:
                state.Registers.Write(instruction.ValueX, state.PC.PCLow);
                break;
            
            case 7:
                state.Registers.Write(instruction.ValueX, state.PC.PCHigh);
                break;
        }
    }

    public static void Sst(MachineState state, Instruction instruction)
    {
        switch (instruction.ValueX)
        {
            case 0:
                state.RAM.AddressPointer = state.Registers.Read(instruction.ValueY);
                break;
                
            case 1:
                state.CallStack.SetStackPointer(state.Registers.Read(instruction.ValueY));
                break;
                
            case 2:
                if (!state.ControlWord.GetFlag(ControlWord.KERNEL_MODE))
                {
                    state.StatusWord.SetError(true);
                    break;
                }
                state.StatusWord.Flags = state.Registers.Read(instruction.ValueY);
                break;
            
            case 3:
                if (!state.ControlWord.GetFlag(ControlWord.KERNEL_MODE))
                {
                    state.StatusWord.SetError(true);
                    break;
                }
                state.ControlWord.Flags = state.Registers.Read(instruction.ValueY);
                break;
            
            case 4:
                state.LoopRegister.LoopPointer = state.Registers.Read(instruction.ValueY);
                break;
            
            case 5:
                if (!state.ControlWord.GetFlag(ControlWord.KERNEL_MODE))
                {
                    state.StatusWord.SetError(true);
                    break;
                }
                state.PC.SetBranchOffset(state.Registers.Read(instruction.ValueY));
                break;
            
            case 6:
                if (!state.ControlWord.GetFlag(ControlWord.KERNEL_MODE))
                {
                    state.StatusWord.SetError(true);
                    break;
                }
                state.PC.SetLow(state.Registers.Read(instruction.ValueY));
                break;
            
            case 7:
                if (!state.ControlWord.GetFlag(ControlWord.KERNEL_MODE))
                {
                    state.StatusWord.SetError(true);
                    break;
                }
                state.PC.SetHigh(state.Registers.Read(instruction.ValueY));
                break;
        }
    }

    public static void Pop(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.Registers.Write(instruction.ValueX, state.RAM.Pop(instruction.ValueY));
                break;
            case 1:
                state.Registers.Write(instruction.ValueX, state.RAM.Peek(instruction.ValueY));
                break;
            case 2:
                state.StatusWord.Flags = state.RAM.Pop(instruction.ValueY);
                break;
            case 3:
                // discard
                _ = state.RAM.Pop(instruction.ValueY);
                break;
        }
    }

    public static void Psh(MachineState state, Instruction instruction)
    {
        switch (instruction.Type)
        {
            case 0:
                state.RAM.Push(state.Registers.Read(instruction.ValueX), instruction.ValueY);
                break;
            case 1:
                state.RAM.Poke(state.Registers.Read(instruction.ValueX), instruction.ValueY);
                break;
            case 2:
                state.RAM.Poke(state.StatusWord.Flags, instruction.ValueY);
                break;
            case 3:
                state.RAM.Push(0, instruction.ValueY);
                break;
        }
    }

    public static void Mld(MachineState state, Instruction instruction)
    {
        state.Registers.Write(instruction.ValueX, state.RAM.Read(instruction.ValueY));
    }

    public static void Mst(MachineState state, Instruction instruction)
    {
        state.RAM.Write(instruction.ValueY, state.Registers.Read(instruction.ValueX));
    }
}
