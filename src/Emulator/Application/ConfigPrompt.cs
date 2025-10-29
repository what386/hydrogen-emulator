namespace Emulator.Application;

public class ConfigPrompt
{
    public EmulatorConfig PromptUserForConfig(string? filePathArg = null)
    {
        Console.WriteLine("Configuration Setup");
        Console.WriteLine("═══════════════════\n");

        string? filePath = filePathArg;

        if (!string.IsNullOrWhiteSpace(filePath))
            goto validateFile;

promptFile:
        Console.Write("ROM file path: ");
        filePath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Console.WriteLine("  ⚠ Path cannot be empty");
            goto promptFile;
        }

validateFile:
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"  ⚠ File not found: {filePath}");
            if (filePathArg is null)
                goto promptFile; // only retry interactively
            else
                Environment.Exit(1);
        }

        Console.WriteLine($"✓ Using ROM: {filePath}\n");            
        
        // Clock speed with default
        Console.Write("Clock speed (Hz) [default: 1000]: ");
        string? speedInput = Console.ReadLine();
        int speed = 1000;
        
        if (!string.IsNullOrWhiteSpace(speedInput) && int.TryParse(speedInput, out int parsedSpeed))
        {
            speed = parsedSpeed;
        }
        
        Console.WriteLine($"✓ Clock speed: {speed}Hz\n");
        
        var fileBytes = File.ReadAllBytes(filePath);
        ushort[] romData = ConvertToUShorts(fileBytes);
        
        Console.WriteLine($"\n✓ Loaded {fileBytes.Length} bytes from ROM");
        
        return new EmulatorConfig(romData, speed);
    } 

    private static ushort[] ConvertToUShorts(byte[] bytes)
    {
        ushort[] data = new ushort[bytes.Length / 2];
        for (int i = 0; i < data.Length; i++)
            data[i] = (ushort)((bytes[i * 2] << 8) | bytes[i * 2 + 1]);
        return data;
    }
}
