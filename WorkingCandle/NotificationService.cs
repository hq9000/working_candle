using System.Diagnostics;
using System.Media;
using System.Reflection;

namespace WorkingCandle;

/// <summary>
/// Service responsible for playing notification sounds.
/// </summary>
public class NotificationService : IDisposable
{
    private SoundPlayer? _soundPlayer;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the NotificationService class.
    /// </summary>
    public NotificationService()
    {
        try
        {
            // Load embedded sound resource
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WorkingCandle.Resources.completion.wav";
            var stream = assembly.GetManifestResourceStream(resourceName);
            
            if (stream != null)
            {
                _soundPlayer = new SoundPlayer(stream);
                _soundPlayer.Load(); // Pre-load the sound
            }
            else
            {
                Debug.WriteLine("Warning: Completion sound resource not found.");
            }
        }
        catch (Exception ex)
        {
            // Silent failure - graceful degradation if sound resource is missing
            Debug.WriteLine($"Warning: Could not load completion sound: {ex.Message}");
        }
    }

    /// <summary>
    /// Plays the completion sound when the timer finishes.
    /// </summary>
    public void PlayCompletionSound()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(NotificationService));
        }

        try
        {
            _soundPlayer?.Play();
        }
        catch (Exception ex)
        {
            // Silent failure - graceful degradation
            Debug.WriteLine($"Warning: Could not play completion sound: {ex.Message}");
        }
    }

    /// <summary>
    /// Releases all resources used by the NotificationService.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the NotificationService and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _soundPlayer?.Dispose();
                _soundPlayer = null;
            }
            _disposed = true;
        }
    }
}
