namespace Emulator.Core;

using System.Diagnostics;

public class Clock
{
    private const int CLOCK_SPEED = 500000; // 500 KHz
    
    private Thread? clockThread;
    private CancellationTokenSource? cancellationTokenSource;
    private bool isRunning = false;
    private readonly object lockObject = new object();
    
    public event Action? OnTick;

    public void Start()
    {
        lock (lockObject)
        {
            if (isRunning) return;
            
            isRunning = true;
            cancellationTokenSource = new CancellationTokenSource();
            
            clockThread = new Thread(() => ClockLoop(cancellationTokenSource.Token))
            {
                IsBackground = true,
                Name = "EmulatorClock",
                Priority = ThreadPriority.Highest
            };
            
            clockThread.Start();
        }
    }

    public void Stop()
    {
        lock (lockObject)
        {
            if (!isRunning) return;
            
            isRunning = false;
            cancellationTokenSource?.Cancel();
            clockThread?.Join(1000);
            
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            clockThread = null;
        }
    }

    public void Pause()
    {
        lock (lockObject)
        {
            if (!isRunning) return;
            
            isRunning = false;
            cancellationTokenSource?.Cancel();
            clockThread?.Join(1000);
            
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            clockThread = null;
        }
    }

    public void Step()
    {
        Tick();
    }

    public void Tick()
    {
        OnTick?.Invoke();
    }

    private void ClockLoop(CancellationToken token)
    {
        var stopwatch = Stopwatch.StartNew();
        long tickCount = 0;
        
        const int batchSize = 1000; // Check timing every 1000 ticks (every 2ms at 500 KHz)
        int ticksUntilCheck = batchSize;
        
        while (!token.IsCancellationRequested)
        {
            // Execute ticks in batches
            Tick();
            tickCount++;
            ticksUntilCheck--;
            
            // Only check timing periodically
            if (ticksUntilCheck <= 0)
            {
                ticksUntilCheck = batchSize;
                double targetTimeMs = (tickCount * 1000.0) / CLOCK_SPEED;
                double currentTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                double deltaMs = targetTimeMs - currentTimeMs;
                
                if (deltaMs > 1.0) // We're ahead by more than 1ms
                {
                    // Sleep for part of the difference
                    int sleepMs = (int)(deltaMs * 0.8);
                    if (sleepMs > 0)
                    {
                        Thread.Sleep(sleepMs);
                    }
                }
                else if (deltaMs < -100.0) // We're more than 100ms behind
                {
                    // We can't keep up - reset timing to avoid accumulating lag
                    stopwatch.Restart();
                    tickCount = 0;
                }
            }
        }
    }
}
