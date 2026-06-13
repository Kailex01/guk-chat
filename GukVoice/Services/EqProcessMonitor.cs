using System.Diagnostics;

namespace GukVoice.Services;

// Polls for the eqgame.exe process every 30 seconds and fires events on state change.
public sealed class EqProcessMonitor : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private bool _wasRunning;

    public event Action? EqClosed;
    public event Action? EqStarted;

    public bool IsRunning => Process.GetProcessesByName("eqgame").Length > 0;

    public EqProcessMonitor()
    {
        _timer = new System.Timers.Timer(30_000) { AutoReset = true };
        _timer.Elapsed += (_, _) => Check();
    }

    public void Start()
    {
        _wasRunning = IsRunning;
        _timer.Start();
    }

    private void Check()
    {
        var running = IsRunning;
        if  (running && !_wasRunning) EqStarted?.Invoke();
        else if (!running && _wasRunning) EqClosed?.Invoke();
        _wasRunning = running;
    }

    public void Dispose() => _timer.Dispose();
}
