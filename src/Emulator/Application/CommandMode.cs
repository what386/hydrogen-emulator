namespace Emulator.Application;

using Emulator.Core;
using Emulator.Models;

public class CommandMode
{
    private MachineState state;

    public CommandMode(MachineState state)
    {
        this.state = state;
    }

    public void Run()
    {
        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();
            if (input == null) continue;

            var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            string cmd = parts[0].ToLowerInvariant();
            string? arg = parts.Length > 1 ? parts[1] : null;

            switch (cmd)
            {
                case "help":
                    ShowHelp();
                    break;

                case "regs":
                    Console.WriteLine(state.Registers.ToString());
                    break;
                
                case "bank":
                    Console.WriteLine(state.RAM.GetBankString());
                    break;
 
                case "mem":
                    Console.WriteLine(state.RAM.GetPoolString());
                    break;

                case "statw":
                    Console.WriteLine(state.StatusWord.ToString());
                    break;

                case "ctrlw":
                    Console.WriteLine(state.ControlWord.ToString());
                    break;

                case "pc":
                    Console.WriteLine(state.PC.ToString());
                    break;

                case "intv":
                    Console.WriteLine(state.IntVector.ToString());
                    break;

                case "step":
                    Step();
                    break;

                case "speed":
                    SetSpeed(arg);
                    break;

                case "resume":
                    return;

                case "clear":
                    Console.Clear();
                    Console.WriteLine("--- Command Mode ---");
                    break;

                case "quit":
                    Environment.Exit(0);
                    break;

                default:
                    Console.WriteLine($"Unknown command: {cmd}\nUse 'help' for a list of commands");
                    break;
            }
        }
    }

    private void Step()
    {
        var binary = state.ROM.Read((ushort)state.PC.Get());
        var instruction = Decoder.Decode(binary);
        Executor.Execute(state, instruction);
        Console.WriteLine($"Executed 1 instruction at PC={state.PC.Get():X4}");
    }

    private void SetSpeed(string? arg)
    {
        if (int.TryParse(arg, out int hz) && hz > 0)
        {
            state.Clock.SetSpeed(hz);
            Console.WriteLine($"Clock speed set to {hz}Hz");
        }
        else
        {
            Console.WriteLine("Usage: speed <hz>");
        }    
    }

    private void ShowHelp()
    {
        Console.WriteLine(
            """
            Available commands:
              help          - Show help message
              regs          - Dump register values
              bank          - Dump contents of bank
              mem           - Dump contents of memory
              statw         - Dump status word 
              ctrlw         - Dump control word
              intv          - Dump interrupt vector
              pc            - Dump program counter
              step          - Execute next instruction
              speed <hz>    - Change clock speed
              resume        - Continue execution
              clear         - Clear screen
              quit          - Quit emulator
            """
        );
    }
}
