namespace Emulator.Application;

public class ConfigPrompt
{
    public EmulatorConfig PromptUserForConfig()
    {
        Console.Write("Enter ROM file path: ");
        string? filePath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("ROM file not found.", filePath);

        Console.Write("Set clock speed (Hz): ");
        if (!int.TryParse(Console.ReadLine(), out int speed))
            speed = 1000; // default

        var fileBytes = File.ReadAllBytes(filePath);
        ushort[] romData = ConvertToUShorts(fileBytes);

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
