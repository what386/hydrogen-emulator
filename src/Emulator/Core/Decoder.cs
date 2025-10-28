namespace Emulator.Core;
using Emulator.Models;

public static class Decoder 
{
    public static int Extract(ushort value, int startBit, int endBit)
    {
        int numBits = endBit - startBit + 1;
        int mask = (1 << numBits) - 1;
        return (value >> startBit) & mask;
    }

    public static Instruction Decode(ushort binary)
    {
        Instruction instruction = new Instruction();

        instruction.RawInstruction = binary;
        
        int opcode = Extract(binary, 11, 15);
        
        instruction.Opcode = opcode;

        // Cascading cases compile to a jump table
        switch (opcode)
        {
            case 0:
                return instruction;
                
            case 1:
                instruction.Type = Extract(binary, 8, 8);
                return instruction;

            case 7:
                instruction.Type = Extract(binary, 8, 9);
                return instruction;
                
            case 2:
            case 8:
            case 9:
            case 14:
            case 15:
            case 16:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
                instruction.ValueX = Extract(binary, 8, 10);
                instruction.ValueY = Extract(binary, 0, 7);
                return instruction;
                
            case 3:
                instruction.ValueX = Extract(binary, 8, 10);
                instruction.ValueY = Extract(binary, 5, 7);
                instruction.ValueZ = Extract(binary, 0, 4);
                return instruction;
                
            case 4:
            case 6:
                instruction.ValueX = Extract(binary, 0, 10);
                return instruction;
                
            case 5:
                instruction.ValueX = Extract(binary, 8, 10);
                instruction.ValueY = Extract(binary, 0, 5);
                return instruction;
                
            case 10:
            case 11:
                instruction.ValueX = Extract(binary, 8, 10);
                instruction.ValueY = Extract(binary, 5, 7);
                return instruction;
                
            case 12:
            case 13:
                instruction.ValueX = Extract(binary, 8, 10);
                instruction.Type = Extract(binary, 6, 7);
                instruction.ValueY = Extract(binary, 0, 5);
                return instruction;
                
            case 17:
                instruction.ValueX = Extract(binary, 8, 10);
                instruction.ValueY = Extract(binary, 5, 7);
                instruction.Type = Extract(binary, 3, 4);
                return instruction;

            case 24:
            case 25:
            case 26:
            case 27:
            case 28:
            case 29:
            case 30:
            case 31:
                instruction.ValueX = Extract(binary, 8, 10);
                instruction.ValueY = Extract(binary, 5, 7);
                instruction.Type = Extract(binary, 3, 4);
                instruction.ValueZ = Extract(binary, 0, 2);
                return instruction;
        }
       
        return instruction;
    }
}
