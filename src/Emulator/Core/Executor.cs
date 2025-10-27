namespace Emulator.Core;

using Emulator.Models;
using Emulator.Registers;

public static class Executor
{
    public static void Execute(MachineState state, Instruction instruction)
    {
        switch((Opcode)instruction.Opcode)
        {
            case Opcode.NOP:
                break;
                
            case Opcode.HLT:
            {
                if (instruction.Type == 0)
                    state.Clock.Stop();
                else
                    state.Clock.Pause();
                break;
            }
            
            case Opcode.SYS:
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
                break;
            }
            
            case Opcode.CLI:
            {
                bool condition = state.StatusWord.CheckCondition(
                    instruction.ValueY, 
                    state.ControlWord.GetFlag(ControlWord.ALT_CONDITIONS)); 

                if (!condition)
                    break;
                
                state.Registers.WriteDirect(instruction.ValueX, (byte)instruction.ValueZ);
                break;
            }
            
            case Opcode.JMP:
            {
                state.PC.Jump(instruction.ValueX, state.ControlWord.GetFlag(ControlWord.PAGE_JUMP_MODE));
                break;
            }
            
            case Opcode.BRA:
            {
                bool condition = state.StatusWord.CheckCondition(
                    instruction.ValueY, 
                    state.ControlWord.GetFlag(ControlWord.ALT_CONDITIONS));
                
                if (!condition)
                    break;

                state.PC.Jump(instruction.ValueY, state.ControlWord.GetFlag(ControlWord.PAGE_JUMP_MODE));

                break;
            }
            
            case Opcode.CAL:
            {
                state.CallStack.Push(state.PC.Get());
                state.PC.Jump(instruction.ValueX, state.ControlWord.GetFlag(ControlWord.PAGE_JUMP_MODE));
                break;
            }

            case Opcode.RET:
            {
                if (instruction.Type == 0)
                {

                    state.PC.Jump(state.CallStack.Pop(), state.ControlWord.GetFlag(ControlWord.PAGE_JUMP_MODE));
                }
                else
                {
                    state.PC.Jump(state.CallStack.GetOldest(), state.ControlWord.GetFlag(ControlWord.PAGE_JUMP_MODE));
                    state.CallStack.Clear();
                }

                break;
            }

            case Opcode.INP:
            {
                state.Registers.Write(instruction.ValueX, state.PortController.Read(instruction.ValueY));
                break;
            }

            case Opcode.OUT:
            {
                state.PortController.Write(instruction.ValueY, state.Registers.Read(instruction.ValueX));
                break;
            }

            case Opcode.SLD:
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
                break;
            }

            case Opcode.SST:
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
                break;

            }
            case Opcode.POP:
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
                break;
            }
            case Opcode.PSH:
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
                break;
            }

            case Opcode.MLD:
            {
                state.Registers.Write(instruction.ValueX, state.RAM.Read(instruction.ValueY));
                break;
            }


            case Opcode.MST:
            {
                state.RAM.Write(instruction.ValueY, state.Registers.Read(instruction.ValueX));
                break;
            }


            case Opcode.LDI:
            {
                state.Registers.WriteDirect(instruction.ValueX, (byte)instruction.ValueY);
                break;
            }

            case Opcode.MOV:
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
                break;
            }


            case Opcode.ADI:
            {
                state.Registers.Write(instruction.ValueX, state.ALU.Add(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY));
                break;
            }


            case Opcode.ANI:
            {
                state.Registers.Write(instruction.ValueX, state.ALU.And(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY));
                break;
            }


            case Opcode.ORI:
            {
                state.Registers.Write(instruction.ValueX, state.ALU.Or(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY));
                break;
            }


            case Opcode.XRI:
            {
                state.Registers.Write(instruction.ValueX, state.ALU.Xor(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY));
                break;
            }


            case Opcode.CPI:
            {
                state.ALU.Sub(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY);
                break;
            }


            case Opcode.TSI:
            {
                state.ALU.And(state.Registers.Read(instruction.ValueX), (byte)instruction.ValueY);
                break;
            }


            case Opcode.ADD:
            {
                switch (instruction.Type)
                {
                    case 0:
                        state.Registers.Write(instruction.ValueX, state.ALU.Add(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 1:
                        state.Registers.Write(instruction.ValueX, state.ALU.AddCarry(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 2:
                        state.Registers.Write(instruction.ValueX, state.ALU.AddVector(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 3:
                        state.Registers.Write(instruction.ValueX, state.ALU.AddVectorCarry(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                }                
                break; 
            }


            case Opcode.SUB:
            {
                switch (instruction.Type)
                {
                    case 0:
                        state.Registers.Write(instruction.ValueX, state.ALU.Sub(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 1:
                        state.Registers.Write(instruction.ValueX, state.ALU.SubBorrow(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 2:
                        state.Registers.Write(instruction.ValueX, state.ALU.SubVector(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 3:
                        state.Registers.Write(instruction.ValueX, state.ALU.SubVectorBorrow(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                }                
                break;
            }


            case Opcode.BIT:
            {
                switch (instruction.Type)
                {
                    case 0:
                        state.Registers.Write(instruction.ValueX, state.ALU.Or(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 1:
                        state.Registers.Write(instruction.ValueX, state.ALU.And(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 2:
                        state.Registers.Write(instruction.ValueX, state.ALU.Xor(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 3:
                        state.Registers.Write(instruction.ValueX, state.ALU.Implies(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                }
                break;
            }


            case Opcode.BNT:
            {
                switch (instruction.Type)
                {
                    case 0:
                        state.Registers.Write(instruction.ValueX, state.ALU.Nor(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 1:
                        state.Registers.Write(instruction.ValueX, state.ALU.Nand(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 2:
                        state.Registers.Write(instruction.ValueX, state.ALU.Xnor(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 3:
                        state.Registers.Write(instruction.ValueX, state.ALU.Nimplies(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                }
                break;
            }


            case Opcode.BSH:
            {
                switch (instruction.Type)
                {
                    case 0:
                        state.Registers.Write(instruction.ValueX, state.BSU.ShiftLeftLogical(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 1:
                        state.Registers.Write(instruction.ValueX, state.BSU.ShiftRightLogical(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 2:
                        state.Registers.Write(instruction.ValueX, state.BSU.RotateLeft(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                    case 3:
                        state.Registers.Write(instruction.ValueX, state.BSU.ShiftRightArithmetic(state.Registers.Read(instruction.ValueY), state.Registers.Read(instruction.ValueZ)));
                        break;
                }
                break;
            }


            case Opcode.BSI:
            {
                switch (instruction.Type)
                {
                    case 0:
                        state.Registers.Write(instruction.ValueX, state.BSU.ShiftLeftLogical(state.Registers.Read(instruction.ValueY), (byte)instruction.ValueZ));
                        break;
                    case 1:
                        state.Registers.Write(instruction.ValueX, state.BSU.ShiftRightLogical(state.Registers.Read(instruction.ValueY), (byte)instruction.ValueZ));
                        break;
                    case 2:
                        state.Registers.Write(instruction.ValueX, state.BSU.RotateLeft(state.Registers.Read(instruction.ValueY), (byte)instruction.ValueZ));
                        break;
                    case 3:
                        state.Registers.Write(instruction.ValueX, state.BSU.ShiftRightArithmetic(state.Registers.Read(instruction.ValueY), (byte)instruction.ValueZ));
                        break;
                }
                break;
            }


            case Opcode.MUL:
            {
                switch (instruction.Type)
                {
                    case 0:
                        state.Registers.Write(instruction.ValueX, state.CMU.MultiplyLow(state.Registers.Read(instruction.ValueY), (byte)instruction.ValueZ));
                        break;
                    case 1:
                        state.Registers.Write(instruction.ValueX, state.CMU.MultiplyHigh(state.Registers.Read(instruction.ValueY), (byte)instruction.ValueZ));
                        break;
                    case 2:
                        state.Registers.Write(instruction.ValueX, state.CMU.Divide(state.Registers.Read(instruction.ValueY), (byte)instruction.ValueZ));
                        break;
                    case 3:
                        state.Registers.Write(instruction.ValueX, state.CMU.Modulo(state.Registers.Read(instruction.ValueY), (byte)instruction.ValueZ));
                        break;
                }
                break;
            }


            case Opcode.BTC:
            {
                switch (instruction.Type)
                {
                    case 0:
                        state.Registers.Write(instruction.ValueX, state.CMU.SquareRoot(state.Registers.Read(instruction.ValueY)));
                        break;
                    case 1:
                        state.Registers.Write(instruction.ValueX, state.CMU.CountLeadingZeros(state.Registers.Read(instruction.ValueY)));
                        break;
                    case 2:
                        state.Registers.Write(instruction.ValueX, state.CMU.CountTrailingZeros(state.Registers.Read(instruction.ValueY)));
                        break;
                    case 3:
                        state.Registers.Write(instruction.ValueX, state.CMU.CountOnes(state.Registers.Read(instruction.ValueY)));
                        break;
                }
                break;
            }
        } 
    }
}
