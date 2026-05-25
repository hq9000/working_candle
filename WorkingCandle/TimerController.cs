namespace WorkingCandle;

/// <summary>
/// Controls the timer functionality including start, pause, resume, and stop operations.
/// </summary>
public class TimerController
{
    private const int TIMER_DURATION_SECONDS = 3600;
    private const int PROGRESS_DIVISOR = TIMER_DURATION_SECONDS / 100; // 36 seconds per 1% progress
    
    private readonly System.Windows.Forms.Timer _uiTimer;
    private DateTime _startTime;
    private TimeSpan _pausedDuration;
    private DateTime? _pauseStartTime;

    /// <summary>
    /// Event raised when the timer starts.
    /// </summary>
    public event EventHandler? TimerStarted;

    /// <summary>
    /// Event raised when the timer is paused.
    /// </summary>
    public event EventHandler? TimerPaused;

    /// <summary>
    /// Event raised when the timer is resumed.
    /// </summary>
    public event EventHandler? TimerResumed;

    /// <summary>
    /// Event raised when the timer is stopped.
    /// </summary>
    public event EventHandler? TimerStopped;

    /// <summary>
    /// Event raised on each timer tick with remaining seconds and progress percentage.
    /// </summary>
    public event EventHandler<TimerTickEventArgs>? TimerTick;

    /// <summary>
    /// Event raised when the timer completes.
    /// </summary>
    public event EventHandler? TimerCompleted;

    /// <summary>
    /// Initializes a new instance of the TimerController class.
    /// </summary>
    public TimerController()
    {
        _uiTimer = new System.Windows.Forms.Timer();
        _uiTimer.Interval = 1000; // 1000ms = 1 second
        _uiTimer.Tick += OnTimerTick;
        _pausedDuration = TimeSpan.Zero;
    }

    /// <summary>
    /// Starts the timer.
    /// </summary>
    public void Start()
    {
        _startTime = DateTime.Now;
        _pausedDuration = TimeSpan.Zero;
        _pauseStartTime = null;
        _uiTimer.Start();
        TimerStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Pauses the timer.
    /// </summary>
    public void Pause()
    {
        _uiTimer.Stop();
        _pauseStartTime = DateTime.Now;
        TimerPaused?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Resumes the timer from a paused state.
    /// </summary>
    public void Resume()
    {
        if (_pauseStartTime.HasValue)
        {
            TimeSpan pauseDuration = DateTime.Now - _pauseStartTime.Value;
            _pausedDuration += pauseDuration;
            _pauseStartTime = null;
        }
        _uiTimer.Start();
        TimerResumed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Stops the timer and resets it.
    /// </summary>
    public void Stop()
    {
        _uiTimer.Stop();
        _startTime = DateTime.MinValue;
        _pausedDuration = TimeSpan.Zero;
        _pauseStartTime = null;
        TimerStopped?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Handles the timer tick event.
    /// </summary>
    private void OnTimerTick(object? sender, EventArgs e)
    {
        // Calculate elapsed time
        TimeSpan elapsed = DateTime.Now - _startTime - _pausedDuration;
        
        // Calculate remaining time with bounds checking
        int secondsRemaining = TIMER_DURATION_SECONDS - (int)elapsed.TotalSeconds;
        
        // Ensure timer never shows negative values
        if (secondsRemaining < 0)
        {
            secondsRemaining = 0;
        }
        
        // Calculate progress percentage (0-100)
        int progress = (int)(elapsed.TotalSeconds / PROGRESS_DIVISOR);
        
        // Ensure progress is within bounds
        progress = Math.Clamp(progress, 0, 100);
        
        // Fire TimerTick event with time and progress data
        TimerTick?.Invoke(this, new TimerTickEventArgs(secondsRemaining, progress));
        
        // Check for completion
        if (secondsRemaining <= 0)
        {
            OnTimerComplete();
        }
    }

    /// <summary>
    /// Handles timer completion.
    /// </summary>
    private void OnTimerComplete()
    {
        _uiTimer.Stop();
        TimerCompleted?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// Event arguments for timer tick events.
/// </summary>
public class TimerTickEventArgs : EventArgs
{
    /// <summary>
    /// Gets the number of seconds remaining.
    /// </summary>
    public int SecondsRemaining { get; }

    /// <summary>
    /// Gets the progress percentage (0-100).
    /// </summary>
    public int ProgressPercent { get; }

    /// <summary>
    /// Initializes a new instance of the TimerTickEventArgs class.
    /// </summary>
    /// <param name="secondsRemaining">The number of seconds remaining.</param>
    /// <param name="progressPercent">The progress percentage (0-100).</param>
    public TimerTickEventArgs(int secondsRemaining, int progressPercent)
    {
        SecondsRemaining = secondsRemaining;
        ProgressPercent = progressPercent;
    }
}
