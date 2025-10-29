namespace Emulator;

using Emulator.Application;

public class Program
{
    public static int Main(string[] args)
    {
        Console.WriteLine("Hydrogen Emulator v1.0.0\n");

        string? filePathArg = args.Length > 0 ? args[0] : null;

        var cli = new ConfigPrompt();
        var config = cli.PromptUserForConfig(filePathArg);

        var emulator = new EmulatorRuntime(config);
        emulator.Run();

        return 0;
    }
}

