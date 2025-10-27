namespace Emulator.IO.Devices;

using System;
using System.Threading.Tasks;

/// <summary>
/// Floating Point Unit (FPU) co-processor peripheral
/// Supports 32-bit IEEE-754 single-precision floating point operations.
/// 
/// PORT MAP (requires 8 consecutive ports):
/// Offset 0: COMMAND - Write operation code to execute
/// Offset 1: STATUS - Read operation status and flags
/// Offset 2: REG_SELECT - Write/read which FP register (0-3) to access
/// Offset 3: DATA_0 - LSB of 32-bit float
/// Offset 4: DATA_1 - Byte 1 of 32-bit float
/// Offset 5: DATA_2 - Byte 2 of 32-bit float
/// Offset 6: DATA_3 - MSB of 32-bit float
/// Offset 7: CTRL - Control register (bit 0: interrupt enable)
/// 
/// COMMANDS:
/// 0x00: NOP
/// 0x10: ADD - R2 = R0 + R1
/// 0x11: SUB - R2 = R0 - R1
/// 0x12: MUL - R2 = R0 * R1
/// 0x13: DIV - R2 = R0 / R1
/// 0x20: SQRT - R1 = sqrt(R0)
/// 0x21: ABS - R1 = abs(R0)
/// 0x22: NEG - R1 = -R0
/// 0x30: CMP - Compare R0 with R1, sets flags
/// 
/// STATUS FLAGS:
/// Bit 0: READY - Set when operation complete
/// Bit 1: ZERO - Set when result equals zero
/// Bit 2: NEGATIVE - Set when result is negative
/// Bit 7: ERROR - Set on error (divide by zero, sqrt of negative)
/// </summary>
public class FloatingPointUnit : IDevice
{
    public int PortCount => 8;
    public event EventHandler<InterruptRequestedEventArgs>? RequestInterrupt;
    public event EventHandler<DeviceWriteEventArgs>? WriteToPort;

    // Internal FP registers (R0-R3)
    private readonly float[] registers = new float[4];
    private byte selectedRegister = 0;
    
    // Data buffer for 32-bit float assembly/disassembly
    private readonly byte[] dataBuffer = new byte[4];
    
    // Control/status
    private byte status = 0x01; // Start with READY flag set
    private byte control = 0x00;

    // Port offsets
    private const int PORT_COMMAND = 0;
    private const int PORT_STATUS = 1;
    private const int PORT_REG_SELECT = 2;
    private const int PORT_DATA_0 = 3;
    private const int PORT_DATA_1 = 4;
    private const int PORT_DATA_2 = 5;
    private const int PORT_DATA_3 = 6;
    private const int PORT_CTRL = 7;

    // Commands
    private const byte CMD_NOP = 0x00;
    private const byte CMD_ADD = 0x10;
    private const byte CMD_SUB = 0x11;
    private const byte CMD_MUL = 0x12;
    private const byte CMD_DIV = 0x13;
    private const byte CMD_SQRT = 0x20;
    private const byte CMD_ABS = 0x21;
    private const byte CMD_NEG = 0x22;
    private const byte CMD_CMP = 0x30;

    // Status flags
    private const byte STATUS_READY = 0x01;
    private const byte STATUS_ZERO = 0x02;
    private const byte STATUS_NEGATIVE = 0x04;
    private const byte STATUS_ERROR = 0x80;

    public void OnPortWrite(int offset, byte data)
    {
        switch (offset)
        {
            case PORT_COMMAND:
                ExecuteCommand(data);
                break;

            case PORT_REG_SELECT:
                selectedRegister = (byte)(data & 0x03); // Only 4 registers
                break;

            case PORT_DATA_0:
            case PORT_DATA_1:
            case PORT_DATA_2:
            case PORT_DATA_3:
                dataBuffer[offset - PORT_DATA_0] = data;
                
                // Auto-load when all 4 bytes written (on DATA_3 write)
                if (offset == PORT_DATA_3)
                {
                    registers[selectedRegister] = BitConverter.ToSingle(dataBuffer, 0);
                }
                break;

            case PORT_CTRL:
                control = data;
                break;
        }
    }

    public void OnPortRead(int offset)
    {
        byte value = 0x00;

        switch (offset)
        {
            case PORT_COMMAND:
                value = 0x00; // Commands are write-only, return 0
                break;

            case PORT_STATUS:
                value = status;
                break;

            case PORT_REG_SELECT:
                value = selectedRegister;
                break;

            case PORT_DATA_0:
            case PORT_DATA_1:
            case PORT_DATA_2:
            case PORT_DATA_3:
                // Prepare buffer on first read (DATA_0)
                if (offset == PORT_DATA_0)
                {
                    byte[] bytes = BitConverter.GetBytes(registers[selectedRegister]);
                    Array.Copy(bytes, dataBuffer, 4);
                }
                value = dataBuffer[offset - PORT_DATA_0];
                break;

            case PORT_CTRL:
                value = control;
                break;
        }

        WriteToPort?.Invoke(this, new DeviceWriteEventArgs(offset, value));
    }

    private void ExecuteCommand(byte command)
    {
        // Clear flags
        status &= unchecked((byte)~STATUS_READY);
        status &= unchecked((byte)~STATUS_ERROR);
        status &= unchecked((byte)~(STATUS_ZERO | STATUS_NEGATIVE));

        try
        {
            float result;
            
            switch (command)
            {
                case CMD_NOP:
                    break;

                case CMD_ADD:
                    result = registers[0] + registers[1];
                    registers[2] = result;
                    UpdateComparisonFlags(result);
                    break;

                case CMD_SUB:
                    result = registers[0] - registers[1];
                    registers[2] = result;
                    UpdateComparisonFlags(result);
                    break;

                case CMD_MUL:
                    result = registers[0] * registers[1];
                    registers[2] = result;
                    UpdateComparisonFlags(result);
                    break;

                case CMD_DIV:
                    if (registers[1] == 0.0f)
                    {
                        status |= STATUS_ERROR;
                        registers[2] = float.NaN;
                    }
                    else
                    {
                        result = registers[0] / registers[1];
                        registers[2] = result;
                        UpdateComparisonFlags(result);
                    }
                    break;

                case CMD_SQRT:
                    if (registers[0] < 0.0f)
                    {
                        status |= STATUS_ERROR;
                        registers[1] = float.NaN;
                    }
                    else
                    {
                        result = (float)Math.Sqrt(registers[0]);
                        registers[1] = result;
                        UpdateComparisonFlags(result);
                    }
                    break;

                case CMD_ABS:
                    result = Math.Abs(registers[0]);
                    registers[1] = result;
                    UpdateComparisonFlags(result);
                    break;

                case CMD_NEG:
                    result = -registers[0];
                    registers[1] = result;
                    UpdateComparisonFlags(result);
                    break;

                case CMD_CMP:
                    UpdateComparisonFlags(registers[0] - registers[1]);
                    break;
            }
        }
        catch (Exception)
        {
            status |= STATUS_ERROR;
        }

        // Set READY flag
        status |= STATUS_READY;

        // Trigger interrupt if enabled
        if ((control & 0x01) != 0)
        {
            RequestInterrupt?.Invoke(this, new InterruptRequestedEventArgs(0xF0));
        }
    }

    private void UpdateComparisonFlags(float value)
    {
        if (value == 0.0f)
        {
            status |= STATUS_ZERO;
        }
        
        if (value < 0.0f)
        {
            status |= STATUS_NEGATIVE;
        }
    }

    public Task StartAsync()
    {
        // Initialize to clean state
        Array.Clear(registers, 0, registers.Length);
        Array.Clear(dataBuffer, 0, dataBuffer.Length);
        
        status = STATUS_READY;
        selectedRegister = 0;
        control = 0x00;
        
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}
