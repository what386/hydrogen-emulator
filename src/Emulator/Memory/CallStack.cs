namespace Emulator.Memory;

public class CallStack
{
    const int STACK_SIZE = 64;

    public int StackPointer = 0;

    int[] stack = new int[STACK_SIZE];

    public void Push(int data)
    {
        stack[StackPointer] = data;

        StackPointer++;

        if (StackPointer is >= STACK_SIZE)
            StackPointer = 0;
    }

    public int Pop()
    {
        if (StackPointer is 0)
            StackPointer = STACK_SIZE;

        StackPointer--;

        return stack[StackPointer];
    }

    public void SetStackPointer(int address)
    {
        StackPointer = address;
    }

    public int GetOldest()
    {
        return stack[0];
    }

    public void Clear()
    {
        StackPointer = 0;
        Array.Clear(stack, 0, STACK_SIZE);
    }
}
