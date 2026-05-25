namespace WorkingCandle;

/// <summary>
/// Controls the timer functionality including start, pause, resume, and stop operations.
/// </summary>
public class TimerController
{
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
    /// Starts the timer.
    /// </summary>
    public void Start()
    {
        // Placeholder implementation
        TimerStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Pauses the timer.
    /// </summary>
    public void Pause()
    {
        // Placeholder implementation
        TimerPaused?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Resumes the timer from a paused state.
    /// </summary>
    public void Resume()
    {
        // Placeholder implementation
        TimerResumed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Stops the timer and resets it.
    /// </summary>
    public void Stop()
    {
        // Placeholder implementation
        TimerStopped?.Invoke(this, EventArgs.Empty);
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
