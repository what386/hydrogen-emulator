namespace Emulator.Models;

public class Instruction
{
    public int Opcode;
    public int Type;
    public int ValueX;
    public int ValueY;
    public int ValueZ;
    public ushort RawInstruction;
}
