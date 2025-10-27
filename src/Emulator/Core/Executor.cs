namespace Emulator.Core;

using Emulator.Core.Handlers;
using Emulator.Models;

public static class Executor
{
    public static void Execute(MachineState state, Instruction instruction)
    {
        switch((Opcode)instruction.Opcode)
        {
            case Opcode.NOP:
                System.Nop(state, instruction);
                break;
                
            case Opcode.HLT:
                System.Hlt(state, instruction);
                break;
            
            case Opcode.SYS:
                System.Sys(state, instruction);
                break;
            
            case Opcode.CLI:
                ControlFlow.Cli(state, instruction);
                break;
            
            case Opcode.JMP:
                ControlFlow.Jmp(state, instruction);
                break;
            
            case Opcode.BRA:
                ControlFlow.Bra(state, instruction);
                break;
            
            case Opcode.CAL:
                ControlFlow.Cal(state, instruction);
                break;
                
            case Opcode.RET:
                ControlFlow.Ret(state, instruction);
                break;
                
            case Opcode.INP:
                InputOutput.Inp(state, instruction);
                break;
                
            case Opcode.OUT:
                InputOutput.Out(state, instruction);
                break;
            
            case Opcode.SLD:
                Memory.Sld(state, instruction);
                break;
                
            case Opcode.SST:
                Memory.Sst(state, instruction);
                break;
            
            case Opcode.POP:
                Memory.Pop(state, instruction);
                break;
                
            case Opcode.PSH:
                Memory.Psh(state, instruction);
                break;
            
            case Opcode.MLD:
                Memory.Mld(state, instruction);
                break;
                
            case Opcode.MST:
                Memory.Mst(state, instruction);
                break;
            
            case Opcode.LDI:
                Arithmetic.Ldi(state, instruction);
                break;
                
            case Opcode.MOV:
                Arithmetic.Mov(state, instruction);
                break;
            
            case Opcode.ADI:
                Arithmetic.Adi(state, instruction);
                break;
                
            case Opcode.ANI:
                Arithmetic.Ani(state, instruction);
                break;
                
            case Opcode.ORI:
                Arithmetic.Ori(state, instruction);
                break;
                
            case Opcode.XRI:
                Arithmetic.Xri(state, instruction);
                break;
                
            case Opcode.CPI:
                Arithmetic.Cpi(state, instruction);
                break;
                
            case Opcode.TSI:
                Arithmetic.Tsi(state, instruction);
                break;
            
            case Opcode.ADD:
                Arithmetic.Add(state, instruction);
                break;
                
            case Opcode.SUB:
                Arithmetic.Sub(state, instruction);
                break;
            
            case Opcode.BIT:
                Arithmetic.Bit(state, instruction);
                break;
                
            case Opcode.BNT:
                Arithmetic.Bnt(state, instruction);
                break;
            
            case Opcode.BSH:
                Compute.Bsh(state, instruction);
                break;
                
            case Opcode.BSI:
                Compute.Bsi(state, instruction);
                break;
            
            case Opcode.MUL:
                Compute.Mul(state, instruction);
                break;
                
            case Opcode.BTC:
                Compute.Btc(state, instruction);
                break;
        } 
    }
}
