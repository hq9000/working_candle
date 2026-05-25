namespace WorkingCandle;

public partial class MainForm : Form
{
    private readonly StateManager _stateManager;
    private readonly TimerController _timerController;
    private readonly NotificationService _notificationService;

    public MainForm()
    {
        InitializeComponent();

        // Instantiate core services
        _stateManager = new StateManager();
        _timerController = new TimerController();
        _notificationService = new NotificationService();

        // Subscribe to state change events
        _stateManager.StateChanged += OnStateChanged;

        // Subscribe to timer events
        _timerController.TimerStarted += OnTimerStarted;
        _timerController.TimerPaused += OnTimerPaused;
        _timerController.TimerResumed += OnTimerResumed;
        _timerController.TimerStopped += OnTimerStopped;
        _timerController.TimerTick += OnTimerTick;
        _timerController.TimerCompleted += OnTimerCompleted;
    }

    private void OnStateChanged(object? sender, StateManager.State newState)
    {
        UpdateUIForState(newState);
    }

    private void OnTimerStarted(object? sender, EventArgs e)
    {
        // Timer started event handling
    }

    private void OnTimerPaused(object? sender, EventArgs e)
    {
        // Timer paused event handling
    }

    private void OnTimerResumed(object? sender, EventArgs e)
    {
        // Timer resumed event handling
    }

    private void OnTimerStopped(object? sender, EventArgs e)
    {
        // Timer stopped event handling
    }

    private void OnTimerTick(object? sender, TimerTickEventArgs e)
    {
        // Update time display in MM:SS countdown format
        int minutes = e.SecondsRemaining / 60;
        int seconds = e.SecondsRemaining % 60;
        _timeLabel.Text = $"{minutes:D2}:{seconds:D2}";
        
        // Update progress bar value
        _progressBar.Value = e.ProgressPercent;
    }

    private void OnTimerCompleted(object? sender, EventArgs e)
    {
        // Timer completed event handling
        // Will play sound in Phase 5
        _stateManager.TransitionTo(StateManager.State.Stopped);
    }
    
    /// <summary>
    /// Updates the UI based on the current state.
    /// </summary>
    /// <param name="state">The current state.</param>
    private void UpdateUIForState(StateManager.State state)
    {
        switch (state)
        {
            case StateManager.State.Stopped:
                // Show Start button
                _startButton.Visible = true;
                
                // Hide all other controls
                _progressBar.Visible = false;
                _timeLabel.Visible = false;
                _pauseButton.Visible = false;
                _resumeButton.Visible = false;
                _stopButton.Visible = false;
                
                // Reset time display
                _timeLabel.Text = "60:00";
                _progressBar.Value = 0;
                break;
                
            case StateManager.State.Running:
                // Hide Start button
                _startButton.Visible = false;
                
                // Show Progress bar, Time display, and Pause button
                _progressBar.Visible = true;
                _timeLabel.Visible = true;
                _pauseButton.Visible = true;
                
                // Hide Resume and Stop buttons
                _resumeButton.Visible = false;
                _stopButton.Visible = false;
                break;
                
            case StateManager.State.Paused:
                // Hide Start and Pause buttons
                _startButton.Visible = false;
                _pauseButton.Visible = false;
                
                // Show Progress bar, Time display, Resume, and Stop buttons
                _progressBar.Visible = true;
                _timeLabel.Visible = true;
                _resumeButton.Visible = true;
                _stopButton.Visible = true;
                break;
        }
    }
    
    /// <summary>
    /// Handles the Start button click event.
    /// </summary>
    private void StartButton_Click(object? sender, EventArgs e)
    {
        if (_stateManager.CanTransitionTo(StateManager.State.Running))
        {
            _timerController.Start();
            _stateManager.TransitionTo(StateManager.State.Running);
        }
    }
    
    /// <summary>
    /// Handles the Pause button click event.
    /// </summary>
    private void PauseButton_Click(object? sender, EventArgs e)
    {
        if (_stateManager.CanTransitionTo(StateManager.State.Paused))
        {
            _timerController.Pause();
            _stateManager.TransitionTo(StateManager.State.Paused);
        }
    }
    
    /// <summary>
    /// Handles the Resume button click event.
    /// </summary>
    private void ResumeButton_Click(object? sender, EventArgs e)
    {
        if (_stateManager.CanTransitionTo(StateManager.State.Running))
        {
            _timerController.Resume();
            _stateManager.TransitionTo(StateManager.State.Running);
        }
    }
    
    /// <summary>
    /// Handles the Stop button click event.
    /// </summary>
    private void StopButton_Click(object? sender, EventArgs e)
    {
        if (_stateManager.CanTransitionTo(StateManager.State.Stopped))
        {
            _timerController.Stop();
            _stateManager.TransitionTo(StateManager.State.Stopped);
        }
    }
}
