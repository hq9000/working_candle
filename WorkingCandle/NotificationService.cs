using System.Diagnostics;
using System.Media;
using System.Reflection;

namespace WorkingCandle;

/// <summary>
/// Service responsible for playing notification sounds and displaying tray notifications.
/// </summary>
public class NotificationService : IDisposable
{
    private SoundPlayer? _soundPlayer;
    private NotifyIcon? _notifyIcon;
    private bool _disposed = false;
    private readonly object _syncLock = new object();
    
    private const int BALLOON_TIP_DURATION_MS = 5000;
    private const int TRAY_ICON_CLEANUP_DELAY_MS = 6000; // 1 second longer than balloon tip duration

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
    /// Initializes the tray notification with the specified icon.
    /// </summary>
    /// <param name="icon">The icon to use for tray notifications.</param>
    public void InitializeTrayNotification(Icon? icon)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(NotificationService));
        }

        try
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = icon ?? SystemIcons.Application,
                Visible = false, // Only show when needed
                Text = "Working Candle"
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Warning: Could not initialize tray notification: {ex.Message}");
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
    /// Shows a tray notification (balloon tip) when the timer completes.
    /// </summary>
    public void ShowCompletionNotification()
    {
        lock (_syncLock)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(NotificationService));
            }

            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = true;
                    _notifyIcon.ShowBalloonTip(
                        BALLOON_TIP_DURATION_MS,
                        "Working Candle",
                        "Your 1-hour focus session is complete!",
                        ToolTipIcon.Info
                    );
                    
                    // Hide after a short delay to clean up the tray
                    // Note: NotifyIcon.Visible is thread-safe and can be set from any thread
                    Task.Delay(TRAY_ICON_CLEANUP_DELAY_MS).ContinueWith(_ =>
                    {
                        lock (_syncLock)
                        {
                            if (!_disposed && _notifyIcon != null)
                            {
                                _notifyIcon.Visible = false;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // Silent failure - graceful degradation
                Debug.WriteLine($"Warning: Could not show tray notification: {ex.Message}");
            }
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
        lock (_syncLock)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _soundPlayer?.Dispose();
                    _soundPlayer = null;
                    
                    _notifyIcon?.Dispose();
                    _notifyIcon = null;
                }
                _disposed = true;
            }
        }
    }
}
