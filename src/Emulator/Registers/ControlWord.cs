namespace Emulator.Registers;

public class ControlWord
{
    // Flag register (8 bits)
    public const byte DEFAULTS = 0b10010000;
    public byte Flags = DEFAULTS;

    public const byte ALT_CONDITIONS = 0b00000001;
    public const byte PAGE_JUMP_MODE = 0b00000010;
    public const byte AUTO_INCREMENT = 0b00000100;
    public const byte DIRECTION_FLAG = 0b00001000; // 0 forward, 1 backward
    public const byte INTERRUPT_ENABLE = 0b00010000;
    public const byte HALT_FLAG = 0b00100000;
    public const byte DEBUG_MODE = 0b01000000;
    public const byte KERNEL_MODE = 0b10000000;

    public void Clear() => Flags = DEFAULTS;

    public void SetFlag(byte flagMask, bool value)
    {
        if (value)
            Flags |= flagMask;
        else
            Flags &= (byte)~flagMask;
    }

    public bool GetFlag(byte flagMask)
    {
        return (Flags & flagMask) != 0;
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Control Word:");
        sb.AppendLine($"Flags (bin): 0b{Convert.ToString(Flags, 2).PadLeft(8, '0')}");
        sb.AppendLine("Set Flags:");

        if (GetFlag(ALT_CONDITIONS)) sb.AppendLine(" - ALT_CONDITIONS");
        if (GetFlag(PAGE_JUMP_MODE)) sb.AppendLine(" - PAGE_JUMP_MODE");
        if (GetFlag(AUTO_INCREMENT)) sb.AppendLine(" - AUTO_INCREMENT");
        if (GetFlag(DIRECTION_FLAG)) sb.AppendLine(" - DIRECTION_FLAG");
        if (GetFlag(INTERRUPT_ENABLE)) sb.AppendLine(" - INTERRUPT_ENABLE");
        if (GetFlag(HALT_FLAG)) sb.AppendLine(" - HALT_FLAG");
        if (GetFlag(DEBUG_MODE)) sb.AppendLine(" - DEBUG_MODE");
        if (GetFlag(KERNEL_MODE)) sb.AppendLine(" - KERNEL_MODE");

        if (sb.ToString().EndsWith("Set Flags:\n"))
            sb.AppendLine(" (none)");

        return sb.ToString();
    }
}
