using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
    private Icon? _baseIcon;
    private Icon? _currentDynamicIcon;
    private bool _isBaseIconSystemIcon = false;
    
    private const int BalloonTipDurationMs = 5000;
    private const int TrayIconCleanupDelayMs = 6000; // 1 second longer than balloon tip duration
    private const int IconSize = 16; // Standard tray icon size
    private const int IconFontSize = 8; // Font size for percentage text
    private const int ShadowAlpha = 128; // Alpha value for text shadow
    private const int ShadowOffsetX = 1; // Horizontal shadow offset
    private const int ShadowOffsetY = 1; // Vertical shadow offset
    
    // Import Windows API function to destroy icon handle
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);

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
            // Store the icon but mark if it's a system icon (which should not be disposed)
            _baseIcon = icon ?? SystemIcons.Application;
            _isBaseIconSystemIcon = (icon == null || ReferenceEquals(icon, SystemIcons.Application));
            
            _notifyIcon = new NotifyIcon
            {
                Icon = _baseIcon,
                Visible = true, // Always visible to show status
                Text = "Working Candle - Stopped"
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Warning: Could not initialize tray notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the taskbar icon to show the current timer status and progress.
    /// </summary>
    /// <param name="state">The current timer state (Stopped, Running, or Paused).</param>
    /// <param name="progressPercent">The progress percentage (0-100).</param>
    public void UpdateTaskbarIcon(StateManager.State state, int progressPercent)
    {
        lock (_syncLock)
        {
            if (_disposed || _notifyIcon == null)
            {
                return;
            }

            try
            {
                // Dispose of the previous dynamic icon
                if (_currentDynamicIcon != null && _currentDynamicIcon != _baseIcon)
                {
                    _currentDynamicIcon.Dispose();
                    _currentDynamicIcon = null;
                }

                // Generate new icon with status overlay
                _currentDynamicIcon = GenerateStatusIcon(state, progressPercent);
                _notifyIcon.Icon = _currentDynamicIcon;

                // Update tooltip text
                string stateText = state switch
                {
                    StateManager.State.Stopped => "Stopped",
                    StateManager.State.Running => "Running",
                    StateManager.State.Paused => "Paused",
                    _ => "Unknown"
                };
                _notifyIcon.Text = $"Working Candle - {stateText} ({progressPercent}%)";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warning: Could not update taskbar icon: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Generates a dynamic icon with state indicator and progress percentage.
    /// </summary>
    /// <param name="state">The current timer state.</param>
    /// <param name="progressPercent">The progress percentage (0-100).</param>
    /// <returns>An icon with the status overlay.</returns>
    private Icon GenerateStatusIcon(StateManager.State state, int progressPercent)
    {
        // Create a bitmap for the icon
        using (Bitmap bitmap = new Bitmap(IconSize, IconSize, PixelFormat.Format32bppArgb))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Determine background color based on state
            Color backgroundColor = state switch
            {
                StateManager.State.Stopped => Color.FromArgb(128, 128, 128), // Gray
                StateManager.State.Running => Color.FromArgb(76, 175, 80), // Green
                StateManager.State.Paused => Color.FromArgb(255, 152, 0), // Orange
                _ => Color.Gray
            };

            // Fill background
            using (SolidBrush bgBrush = new SolidBrush(backgroundColor))
            {
                g.FillRectangle(bgBrush, 0, 0, IconSize, IconSize);
            }

            // Draw percentage text (white color for visibility)
            string percentText = progressPercent.ToString();
            // Use system font for better compatibility across different systems
            using (Font font = new Font(FontFamily.GenericSansSerif, IconFontSize, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                // Measure text to center it
                SizeF textSize = g.MeasureString(percentText, font);
                float x = (IconSize - textSize.Width) / 2;
                float y = (IconSize - textSize.Height) / 2;

                // Draw text with a slight shadow for better readability
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(ShadowAlpha, 0, 0, 0)))
                {
                    g.DrawString(percentText, font, shadowBrush, x + ShadowOffsetX, y + ShadowOffsetY);
                }
                g.DrawString(percentText, font, textBrush, x, y);
            }

            // Convert bitmap to icon
            IntPtr hIcon = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(hIcon);
            
            // Clone the icon to ensure it persists after bitmap disposal
            Icon clonedIcon = (Icon)icon.Clone();
            
            // Dispose the original icon and clean up the handle
            icon.Dispose();
            DestroyIcon(hIcon);
            
            return clonedIcon;
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
                    _notifyIcon.ShowBalloonTip(
                        BalloonTipDurationMs,
                        "Working Candle",
                        "Your 1-hour focus session is complete!",
                        ToolTipIcon.Info
                    );
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
                    
                    if (_currentDynamicIcon != null && _currentDynamicIcon != _baseIcon)
                    {
                        _currentDynamicIcon.Dispose();
                        _currentDynamicIcon = null;
                    }
                    
                    _notifyIcon?.Dispose();
                    _notifyIcon = null;
                    
                    // Only dispose _baseIcon if it's not a system icon
                    if (_baseIcon != null && !_isBaseIconSystemIcon)
                    {
                        _baseIcon.Dispose();
                    }
                    _baseIcon = null;
                }
                _disposed = true;
            }
        }
    }
}
