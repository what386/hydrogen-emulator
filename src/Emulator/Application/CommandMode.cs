namespace Emulator.Application;

using Emulator.Models;
using Emulator.Application.Commands;

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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("> ");
            Console.ResetColor();
            
            string? input = Console.ReadLine();
            if (input == null) continue;
            
            var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;
            
            string cmd = parts[0].ToLowerInvariant();
            string? arg = parts.Length > 1 ? parts[1] : null;
            
            switch (cmd)
            {
                // Help
                case "help":
                case "?":
                    SystemCommands.ShowHelp();
                    break;
                
                // InspectionCommands
                case "regs":
                case "r":
                    InspectionCommands.ShowRegisters(state);
                    break;
                
                case "bank":
                    InspectionCommands.ShowBank(state);
                    break;
 
                case "mem":
                    InspectionCommands.ShowMemory(state);
                    break;
                    
                case "statw":
                case "status":
                    InspectionCommands.ShowStatusWord(state);
                    break;
                    
                case "ctrlw":
                case "control":
                    InspectionCommands.ShowControlWord(state);
                    break;
                    
                case "pc":
                    InspectionCommands.ShowProgramCounter(state);
                    break;
                    
                case "intv":
                case "int":
                    InspectionCommands.ShowInterruptVector(state);
                    break;
                
                // ExecutionCommands
                case "step":
                case "s":
                    ExecutionCommands.Step(state, arg);
                    break;
                    
                case "until":
                case "u":
                    ExecutionCommands.Until(state, arg);
                    break;
                    
                case "speed":
                    ExecutionCommands.SetSpeed(state, arg);
                    break;
                
                // MemoryCommands
                case "peek":
                    MemoryCommands.Peek(state, arg);
                    break;
                    
                case "poke":
                    MemoryCommands.Poke(state, arg);
                    break;
                    
                case "rpeek":
                case "regpeek":
                    MemoryCommands.RegisterPeek(state, arg);
                    break;
                    
                case "rpoke":
                case "regpoke":
                    MemoryCommands.RegisterPoke(state, arg);
                    break;

                // BreakpointCommands
                case "break":
                case "b":
                    BreakpointCommands.SetBreakpoint(state, arg);
                    break;
                case "delete":
                case "del":
                    BreakpointCommands.DeleteBreakpoint(state, arg);
                    break;
                case "breaks":
                    BreakpointCommands.ListBreakpoints(state);
                    break;

                // WatchpointCommands
                case "watch":
                case "w":
                    WatchpointCommands.AddWatch(state, arg);
                    break;
                case "unwatch":
                    WatchpointCommands.RemoveWatch(state, arg);
                    break;
                case "watches":
                    WatchpointCommands.ListWatches(state);
                    break;

                // DeviceCommands
                case "devices":
                    DeviceCommands.ListDevices(state);
                    break;
                case "ports":
                    DeviceCommands.ShowPorts(state);
                    break;
                case "device":
                    DeviceCommands.DeviceInfo(state, arg);
                    break;
                case "inport":
                    DeviceCommands.ReadPort(state, arg);
                    break;
                case "outport":
                    DeviceCommands.WritePort(state, arg);
                    break;

                // System
                case "reset":
                    SystemCommands.Reset(state);
                    break;
                    
                case "resume":
                case "continue":
                case "c":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ Resuming execution...");
                    Console.ResetColor();
                    return;
                    
                case "clear":
                case "cls":
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("╔════════════════════════════════════╗");
                    Console.WriteLine("║        COMMAND MODE ACTIVE         ║");
                    Console.WriteLine("╚════════════════════════════════════╝");
                    Console.ResetColor();
                    break;
                    
                case "quit":
                case "exit":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exiting emulator...");
                    Console.ResetColor();
                    Environment.Exit(0);
                    break;
                    
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ Unknown command: '{cmd}'");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("  Type 'help' for a list of commands");
                    Console.ResetColor();
                    break;
            }
            
            Console.WriteLine();
        }
    }
}
