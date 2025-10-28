namespace Emulator;

using Emulator.Application;

public class Program
{
    public static int Main(string[] args)
    {
        Console.WriteLine("Hydrogen Emulator v1.0.0");

        var cli = new EmulatorCLI();
        var options = cli.PromptUserForConfig();

        var emulator = new EmulatorApp(options);
        emulator.Run();

        return 0;
    }
}

