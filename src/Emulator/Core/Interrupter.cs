namespace Emulator.Core;

using Emulator.Models;

public static class Interruptor 
{
    public static void HandleInterrupts(MachineState state)
    {
        if(state.IntVector.pendingInterrupts.Count > 0)
        {
            var (priority, vector) = state.IntVector.pendingInterrupts.Peek();
            
            // Check if there's an active interrupt with higher or equal priority
            if(state.IntVector.activeInterrupts.Count > 0)
            {
                var (currentPriority, currentVector) = state.IntVector.activeInterrupts.Peek();
                if(currentPriority >= priority)
                    return;
            }
            
            state.IntVector.pendingInterrupts.Pop();
            state.CallStack.Push(state.PC.Get());
            state.PC.Jump(state.IntVector.GetAddress(vector), false);
            state.IntVector.activeInterrupts.Push((priority, vector));
        } 
    }
}
