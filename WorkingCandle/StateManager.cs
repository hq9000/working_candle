namespace WorkingCandle;

/// <summary>
/// Manages the application state and validates state transitions.
/// </summary>
public class StateManager
{
    /// <summary>
    /// Enum representing possible timer states.
    /// </summary>
    public enum State
    {
        Stopped,
        Running,
        Paused
    }

    private State _currentState = State.Stopped;

    /// <summary>
    /// Event raised when the state changes.
    /// </summary>
    public event EventHandler<State>? StateChanged;

    /// <summary>
    /// Gets the current state of the timer.
    /// </summary>
    public State CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState != value)
            {
                _currentState = value;
                StateChanged?.Invoke(this, _currentState);
            }
        }
    }

    /// <summary>
    /// Checks if a transition to the specified state is valid.
    /// </summary>
    /// <param name="newState">The state to transition to.</param>
    /// <returns>True if the transition is valid; otherwise, false.</returns>
    public bool CanTransitionTo(State newState)
    {
        return (CurrentState, newState) switch
        {
            // From Stopped, can only go to Running
            (State.Stopped, State.Running) => true,
            
            // From Running, can go to Paused or Stopped
            (State.Running, State.Paused) => true,
            (State.Running, State.Stopped) => true,
            
            // From Paused, can go to Running or Stopped
            (State.Paused, State.Running) => true,
            (State.Paused, State.Stopped) => true,
            
            // All other transitions are invalid
            _ => false
        };
    }

    /// <summary>
    /// Transitions to the specified state if the transition is valid.
    /// </summary>
    /// <param name="newState">The state to transition to.</param>
    /// <returns>True if the transition was successful; otherwise, false.</returns>
    public bool TransitionTo(State newState)
    {
        if (!CanTransitionTo(newState))
        {
            return false;
        }

        CurrentState = newState;
        return true;
    }
}
