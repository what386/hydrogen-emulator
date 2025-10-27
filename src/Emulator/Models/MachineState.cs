namespace Emulator.Models;

using Emulator.Core;
using Emulator.IO;
using Emulator.Arithmetic;
using Emulator.Registers;
using Emulator.Memory;
using Emulator.Memory.Data;
using Emulator.Memory.Instruction;

public class MachineState(
    Clock clock,
    ProgramCounter pc,
    PortController portController,
    StatusWord statusWord,
    ControlWord controlWord,
    InterruptVector intVector,
    RegisterFile registers,
    ArithmeticLogicUnit alu,
    BitShiftUnit bsu,
    ComplexMathUnit cmu,
    InstructionROM rom,
    MainMemory ram,
    CallStack callStack,
    LoopRegister loopRegister
)
{
    public Clock Clock = clock;
    public ProgramCounter PC = pc;
    public PortController PortController = portController;
    public StatusWord StatusWord = statusWord;
    public ControlWord ControlWord = controlWord;
    public InterruptVector IntVector = intVector;
    public RegisterFile Registers = registers;
    public ArithmeticLogicUnit ALU = alu;
    public BitShiftUnit BSU = bsu;
    public ComplexMathUnit CMU = cmu;
    public InstructionROM ROM = rom;
    public MainMemory RAM = ram;
    public CallStack CallStack = callStack;
    public LoopRegister LoopRegister = loopRegister;
}

