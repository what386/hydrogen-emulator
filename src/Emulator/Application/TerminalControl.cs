namespace Emulator.Application;

using System;
using System.Runtime.InteropServices;

public static class TerminalControl
{
    // Windows API
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    
    private const int STD_INPUT_HANDLE = -10;
    private const uint ENABLE_ECHO_INPUT = 0x0004;
    
    // Unix API
    [DllImport("libc", SetLastError = true)]
    private static extern int tcgetattr(int fd, ref Termios termios);
    
    [DllImport("libc", SetLastError = true)]
    private static extern int tcsetattr(int fd, int optional_actions, ref Termios termios);
    
    private const int STDIN_FILENO = 0;
    private const int TCSANOW = 0;
    private const uint ECHO = 0x00000008;
    
    [StructLayout(LayoutKind.Sequential)]
    private struct Termios
    {
        public uint c_iflag;
        public uint c_oflag;
        public uint c_cflag;
        public uint c_lflag;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] c_cc;
        public uint c_ispeed;
        public uint c_ospeed;
    }
    
    private static Termios? savedTermios = null;
    private static uint savedConsoleMode = 0;
    private static bool isEchoDisabled = false;
    
    /// <summary>
    /// Disables terminal echo so typed characters are not automatically displayed.
    /// </summary>
    public static void DisableEcho()
    {
        if (isEchoDisabled)
            return;
            
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                DisableEchoWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                DisableEchoUnix();
            }
            
            isEchoDisabled = true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: Could not disable terminal echo: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Re-enables terminal echo, restoring the original terminal state.
    /// </summary>
    public static void EnableEcho()
    {
        if (!isEchoDisabled)
            return;
            
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                EnableEchoWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                EnableEchoUnix();
            }
            
            isEchoDisabled = false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: Could not restore terminal echo: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets whether echo is currently disabled.
    /// </summary>
    public static bool IsEchoDisabled => isEchoDisabled;
    
    private static void DisableEchoWindows()
    {
        IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
        if (handle == IntPtr.Zero || handle == new IntPtr(-1))
            return;
            
        if (GetConsoleMode(handle, out savedConsoleMode))
        {
            uint newMode = savedConsoleMode & ~ENABLE_ECHO_INPUT;
            SetConsoleMode(handle, newMode);
        }
    }
    
    private static void EnableEchoWindows()
    {
        IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);
        if (handle == IntPtr.Zero || handle == new IntPtr(-1))
            return;
            
        SetConsoleMode(handle, savedConsoleMode);
    }
    
    private static void DisableEchoUnix()
    {
        Termios term = new Termios();
        term.c_cc = new byte[32];
        
        if (tcgetattr(STDIN_FILENO, ref term) == 0)
        {
            savedTermios = term;
            term.c_lflag &= ~ECHO;
            tcsetattr(STDIN_FILENO, TCSANOW, ref term);
        }
    }
    
    private static void EnableEchoUnix()
    {
        if (savedTermios.HasValue)
        {
            Termios term = savedTermios.Value;
            tcsetattr(STDIN_FILENO, TCSANOW, ref term);
        }
    }
}
