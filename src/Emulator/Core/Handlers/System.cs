namespace Emulator.Core.Handlers;

using Emulator.Models;
using Emulator.Registers;

public static class System
{
    public static void Nop(MachineState state, Instruction instruction)
    {
        // Do nothing
    }

    public static void Hlt(MachineState state, Instruction instruction)
    {
        if (instruction.Type == 0)
            state.Clock.Stop();
        else
            state.Clock.Pause();
    }

    public static void Sys(MachineState state, Instruction instruction)
    {
        switch (instruction.ValueX)
        {
            case 0:
                int currentAddress = state.PC.Get();
                state.LoopRegister.LoopStart = currentAddress + 1;
                state.LoopRegister.LoopEnd = currentAddress + instruction.ValueY;
                break;
                
            case 1:
                state.LoopRegister.LoopCount = instruction.ValueY;
                break;
                
            case 2:
            {
                if (!state.ControlWord.GetFlag(ControlWord.KERNEL_MODE))
                {
                    state.StatusWord.SetError(true);
                    break;
                }
                state.ControlWord.Flags = (byte)instruction.ValueY;
                break;
            }
            
            case 3:
                if (!state.ControlWord.GetFlag(ControlWord.KERNEL_MODE))
                {
                    state.StatusWord.SetError(true);
                    break;
                }
                state.StatusWord.Flags = (byte)instruction.ValueY;
                break;
            
            case 4:
                state.RAM.SetPage(instruction.ValueY);
                break;
            
            case 5:
                if (!state.ControlWord.GetFlag(ControlWord.KERNEL_MODE))
                {
                    state.StatusWord.SetError(true);
                    break;
                }
                int index = state.Registers.Read(0);
                state.IntVector.SetAddress(index, (byte)instruction.ValueY);
                break;
            
            case 6:
                if (!state.ControlWord.GetFlag(ControlWord.KERNEL_MODE))
                {
                    state.StatusWord.SetError(true);
                    break;
                }
                state.IntVector.InterruptMask = (byte)instruction.ValueY;
                break;
            
            case 7:
                break;
        }
    }
}
