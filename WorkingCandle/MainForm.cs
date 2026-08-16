namespace WorkingCandle;

public partial class MainForm : Form
{
    private readonly StateManager _stateManager;
    private readonly TimerController _timerController;
    private readonly NotificationService _notificationService;
    private readonly GlobalHotkeyService _globalHotkeyService;
    
    private const string INITIAL_TIME_DISPLAY = "60:00";
    private const int TOTAL_TIMER_MINUTES = 60;
    private const int SECONDS_PER_MINUTE = 60;
    private const int PERCENTAGE_DIVISOR = 100;

    public MainForm()
    {
        InitializeComponent();

        // Instantiate core services
        _stateManager = new StateManager();
        _timerController = new TimerController();
        _notificationService = new NotificationService();
        _globalHotkeyService = new GlobalHotkeyService();
        
        // Initialize tray notification with the form's icon
        _notificationService.InitializeTrayNotification(this.Icon);
        
        // Setup context menu for tray icon with handlers
        _notificationService.SetupContextMenu(
            onStart: StartButton_Click,
            onStop: StopButton_Click,
            onPause: PauseButton_Click,
            onResume: ResumeButton_Click,
            onAddFiveMinutes: AddFiveMinutesButton_Click,
            onSubtractFiveMinutes: SubtractFiveMinutesButton_Click,
            onExit: ExitApplication
        );

        // Subscribe to state change events
        _stateManager.StateChanged += OnStateChanged;

        // Subscribe to global hotkey events (Right Ctrl pauses/resumes, Right Shift stops)
        _globalHotkeyService.PauseResumeHotkeyPressed += GlobalHotkeyService_PauseResumeHotkeyPressed;
        _globalHotkeyService.StopHotkeyPressed += GlobalHotkeyService_StopHotkeyPressed;
        _globalHotkeyService.Register();

        // Subscribe to timer events
        _timerController.TimerStarted += OnTimerStarted;
        _timerController.TimerPaused += OnTimerPaused;
        _timerController.TimerResumed += OnTimerResumed;
        _timerController.TimerStopped += OnTimerStopped;
        _timerController.TimerTick += OnTimerTick;
        _timerController.TimerCompleted += OnTimerCompleted;
        
        // Initialize context menu state
        _notificationService.UpdateContextMenuState(_stateManager.CurrentState);
    }

    /// <summary>
    /// Handles the global pause/resume hotkey (Right Ctrl) press.
    /// Pauses the timer when running, or resumes it when paused.
    /// </summary>
    private void GlobalHotkeyService_PauseResumeHotkeyPressed(object? sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(GlobalHotkeyService_PauseResumeHotkeyPressed, sender, e);
            return;
        }

        switch (_stateManager.CurrentState)
        {
            case StateManager.State.Running:
                PauseTimer();
                break;
            case StateManager.State.Paused:
                ResumeTimer();
                break;
        }
    }

    /// <summary>
    /// Handles the global stop hotkey (Right Shift) press.
    /// </summary>
    private void GlobalHotkeyService_StopHotkeyPressed(object? sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(GlobalHotkeyService_StopHotkeyPressed, sender, e);
            return;
        }

        StopTimer();
    }

    private void OnStateChanged(object? sender, StateManager.State newState)
    {
        UpdateUIForState(newState);
        
        // Update taskbar icon and title when state changes
        int minutesRemaining = CalculateMinutesRemaining();
        _notificationService.UpdateTaskbarIcon(newState, minutesRemaining);
        UpdateTaskbarTitle(newState, minutesRemaining);
        
        // Update context menu state
        _notificationService.UpdateContextMenuState(newState);
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
        int minutes = e.SecondsRemaining / SECONDS_PER_MINUTE;
        int seconds = e.SecondsRemaining % SECONDS_PER_MINUTE;
        _timeLabel.Text = $"{minutes:D2}:{seconds:D2}";
        
        // Update progress bar value
        _progressBar.Value = e.ProgressPercent;
        
        // Update taskbar icon and title with current progress
        // Round up to show accurate minutes remaining (e.g., 119 seconds = 2 minutes)
        int minutesRemaining = (int)Math.Ceiling((double)e.SecondsRemaining / SECONDS_PER_MINUTE);
        _notificationService.UpdateTaskbarIcon(_stateManager.CurrentState, minutesRemaining);
        UpdateTaskbarTitle(_stateManager.CurrentState, minutesRemaining);
    }

    private void OnTimerCompleted(object? sender, EventArgs e)
    {
        // Play completion sound
        _notificationService.PlayCompletionSound();
        
        // Show tray notification
        _notificationService.ShowCompletionNotification();
        
        // Transition to STOPPED state
        _stateManager.TransitionTo(StateManager.State.Stopped);
    }
    
    /// <summary>
    /// Calculates the minutes remaining based on the current progress bar value.
    /// </summary>
    /// <returns>The number of minutes remaining.</returns>
    private int CalculateMinutesRemaining()
    {
        // When stopped, return total timer duration
        if (_stateManager.CurrentState == StateManager.State.Stopped)
        {
            return TOTAL_TIMER_MINUTES;
        }
        
        // Calculate based on progress bar (0-100%)
        // Use ceiling to match the rounding behavior in OnTimerTick
        int progressPercent = _progressBar.Value;
        double elapsedMinutes = (double)(TOTAL_TIMER_MINUTES * progressPercent) / PERCENTAGE_DIVISOR;
        int minutesRemaining = TOTAL_TIMER_MINUTES - (int)Math.Ceiling(elapsedMinutes);
        
        return Math.Max(0, minutesRemaining);
    }
    
    /// <summary>
    /// Updates the taskbar title to show the current status and minutes remaining.
    /// </summary>
    /// <param name="state">The current timer state.</param>
    /// <param name="minutesRemaining">The minutes remaining in the timer.</param>
    private void UpdateTaskbarTitle(StateManager.State state, int minutesRemaining)
    {
        string stateText = state switch
        {
            StateManager.State.Stopped => "Stopped",
            StateManager.State.Running => "Running",
            StateManager.State.Paused => "Paused",
            _ => "Unknown"
        };
        
        this.Text = $"Working Candle - {stateText} ({minutesRemaining}m)";
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
                _addFiveMinutesButton.Visible = false;
                _subtractFiveMinutesButton.Visible = false;
                
                // Reset time display
                _timeLabel.Text = INITIAL_TIME_DISPLAY;
                _progressBar.Value = 0;
                break;
                
            case StateManager.State.Running:
                // Hide Start button
                _startButton.Visible = false;
                
                // Show Progress bar, Time display, Pause, and Stop buttons
                _progressBar.Visible = true;
                _timeLabel.Visible = true;
                _pauseButton.Visible = true;
                _stopButton.Visible = true;
                
                // Show +5m and -5m buttons when timer is running
                _addFiveMinutesButton.Visible = true;
                _subtractFiveMinutesButton.Visible = true;
                
                // Hide Resume button
                _resumeButton.Visible = false;
                break;
                
            case StateManager.State.Paused:
                // Hide Start and Pause buttons
                _startButton.Visible = false;
                _pauseButton.Visible = false;
                
                // Hide +5m and -5m buttons when paused
                _addFiveMinutesButton.Visible = false;
                _subtractFiveMinutesButton.Visible = false;
                
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
        PauseTimer();
    }
    
    /// <summary>
    /// Handles the Resume button click event.
    /// </summary>
    private void ResumeButton_Click(object? sender, EventArgs e)
    {
        ResumeTimer();
    }
    
    /// <summary>
    /// Pauses the timer if a transition to the Paused state is currently valid.
    /// Shared by the Pause button and the global pause/resume hotkey.
    /// </summary>
    private void PauseTimer()
    {
        if (_stateManager.CanTransitionTo(StateManager.State.Paused))
        {
            _timerController.Pause();
            _stateManager.TransitionTo(StateManager.State.Paused);
        }
    }
    
    /// <summary>
    /// Resumes the timer if a transition to the Running state is currently valid.
    /// Shared by the Resume button and the global pause/resume hotkey.
    /// </summary>
    private void ResumeTimer()
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
        StopTimer();
    }

    private void StopTimer()
    {
        if (_stateManager.CanTransitionTo(StateManager.State.Stopped))
        {
            _timerController.Stop();
            _stateManager.TransitionTo(StateManager.State.Stopped);
        }
    }
    
    /// <summary>
    /// Handles the +5m button click event.
    /// Jumps forward 5 minutes, reducing remaining time.
    /// </summary>
    private void AddFiveMinutesButton_Click(object? sender, EventArgs e)
    {
        // Jump forward 5 minutes (reduce remaining time by 300 seconds)
        _timerController.JumpForward(300);
    }
    
    /// <summary>
    /// Handles the -5m button click event.
    /// Jumps backward 5 minutes, increasing remaining time.
    /// </summary>
    private void SubtractFiveMinutesButton_Click(object? sender, EventArgs e)
    {
        // Jump backward 5 minutes (increase remaining time by 300 seconds)
        _timerController.JumpBackward(300);
    }
    
    /// <summary>
    /// Handles the Exit action from the tray icon context menu.
    /// Closes the application properly using the form's Close method for proper cleanup.
    /// </summary>
    private void ExitApplication(object? sender, EventArgs e)
    {
        this.Close();
    }
}
