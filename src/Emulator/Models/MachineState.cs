namespace Emulator.Models;
using Emulator.Core;
using Emulator.IO;
using Emulator.Arithmetic;
using Emulator.Registers;
using Emulator.Memory;
using Emulator.Memory.Data;
using Emulator.Memory.Instruction;

public class MachineState
{
    public Clock Clock { get; set; } = new();
    public ProgramCounter PC { get; set; } = new();
    public PortController PortController { get; set; } = new();
    public StatusWord StatusWord { get; set; } = new();
    public ControlWord ControlWord { get; set; } = new();
    public InterruptVector IntVector { get; set; } = new();
    public RegisterFile Registers { get; set; } = new();
    public CallStack CallStack { get; set; } = new();
    public LoopRegister LoopRegister { get; set; } = new();

    private ArithmeticLogicUnit? _alu;
    public ArithmeticLogicUnit ALU => _alu ??= new(StatusWord);

    private BitShiftUnit? _bsu;
    public BitShiftUnit BSU => _bsu ??= new(StatusWord);

    private ComplexMathUnit? _cmu;
    public ComplexMathUnit CMU => _cmu ??= new(StatusWord);

    private InstructionROM? _rom;
    public InstructionROM ROM => _rom ??= new();

    private MainMemory? _ram;
    public MainMemory RAM => _ram ??= new(StatusWord);
}
