namespace Machine.Memory;

public class CallStack
{
    const int STACK_SIZE = 64;

    int stackPointer = 0;

    ushort[] stack = new ushort[STACK_SIZE];

    public void Push(ushort data)
    {
        stack[stackPointer] = data;

        stackPointer++;

        if (stackPointer is >= STACK_SIZE)
            stackPointer = 0;
    }

    public ushort Pop()
    {
        if (stackPointer is 0)
            stackPointer = STACK_SIZE;

        stackPointer--;

        return stack[stackPointer];
    }
}
