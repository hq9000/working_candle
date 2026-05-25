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
        // UI updates will be implemented in Phase 4
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
        // Timer tick event handling - will update UI in Phase 4
    }

    private void OnTimerCompleted(object? sender, EventArgs e)
    {
        // Timer completed event handling
        // Will play sound in Phase 5
    }
}
