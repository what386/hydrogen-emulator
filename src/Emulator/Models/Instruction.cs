namespace Emulator.Models;

public class Instruction
(
    int opcode,
    int type,
    int valueX,
    int valueY,
    int valueZ,
    ushort rawInstruction
){
    public int Opcode = opcode;
    public int Type = type;
    public int ValueX = valueX;
    public int ValueY = valueY;
    public int ValueZ = valueZ;
    public ushort RawInstruction = rawInstruction;
}
