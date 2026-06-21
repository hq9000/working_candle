using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;

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
    private IntPtr _currentIconHandle = IntPtr.Zero;
    private bool _isBaseIconSystemIcon = false;
    private ContextMenuStrip? _contextMenu;
    private ToolStripMenuItem? _startMenuItem;
    private ToolStripMenuItem? _stopMenuItem;
    private ToolStripMenuItem? _pauseMenuItem;
    private ToolStripMenuItem? _resumeMenuItem;
    private ToolStripMenuItem? _addFiveMinutesMenuItem;
    private ToolStripMenuItem? _subtractFiveMinutesMenuItem;
    private ToolStripMenuItem? _exitMenuItem;
    
    private const int BalloonTipDurationMs = 5000;
    private const int TrayIconCleanupDelayMs = 6000; // 1 second longer than balloon tip duration
    private const int IconSize = 16; // Standard tray icon size
    private const int IconFontSize = 8; // Font size for minutes text
    private const int ShadowAlpha = 128; // Alpha value for text shadow
    private const int ShadowOffsetX = 1; // Horizontal shadow offset
    private const int ShadowOffsetY = 1; // Vertical shadow offset
    
    // Import Windows API function to destroy icon handle
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);
    
    // Import Windows Shell API to control notification area icon behavior
    [DllImport("shell32.dll", SetLastError = true)]
    private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA pnid);
    
    // Constants for Shell_NotifyIcon
    private const uint NIM_SETVERSION = 0x00000004;
    private const uint NIM_MODIFY = 0x00000001;
    private const uint NOTIFYICON_VERSION_4 = 4;
    
    // NOTIFYICONDATA structure for Shell_NotifyIcon
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct NOTIFYICONDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }
    
    // State flags for notification icon
    private const uint NIS_HIDDEN = 0x00000001;
    private const uint NIF_STATE = 0x00000008;

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
            
            // Ensure the icon is shown in the notification area (not hidden in overflow)
            EnsureIconAlwaysVisible();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Warning: Could not initialize tray notification: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Sets up the context menu for the tray icon with action callbacks.
    /// </summary>
    /// <param name="onStart">Callback for Start action.</param>
    /// <param name="onStop">Callback for Stop action.</param>
    /// <param name="onPause">Callback for Pause action.</param>
    /// <param name="onResume">Callback for Resume action.</param>
    /// <param name="onAddFiveMinutes">Callback for +5m action.</param>
    /// <param name="onSubtractFiveMinutes">Callback for -5m action.</param>
    /// <param name="onExit">Callback for Exit action.</param>
    public void SetupContextMenu(
        EventHandler onStart,
        EventHandler onStop,
        EventHandler onPause,
        EventHandler onResume,
        EventHandler onAddFiveMinutes,
        EventHandler onSubtractFiveMinutes,
        EventHandler onExit)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(NotificationService));
        }

        if (_notifyIcon == null)
        {
            throw new InvalidOperationException("Tray notification must be initialized before setting up context menu.");
        }

        try
        {
            // Create context menu
            _contextMenu = new ContextMenuStrip();
            
            // Create menu items
            _startMenuItem = new ToolStripMenuItem("Start", null, onStart);
            _stopMenuItem = new ToolStripMenuItem("Stop", null, onStop);
            _pauseMenuItem = new ToolStripMenuItem("Pause", null, onPause);
            _resumeMenuItem = new ToolStripMenuItem("Resume", null, onResume);
            _addFiveMinutesMenuItem = new ToolStripMenuItem("+5m", null, onAddFiveMinutes);
            _subtractFiveMinutesMenuItem = new ToolStripMenuItem("-5m", null, onSubtractFiveMinutes);
            _exitMenuItem = new ToolStripMenuItem("Exit", null, onExit);
            
            // Add items to context menu
            _contextMenu.Items.Add(_startMenuItem);
            _contextMenu.Items.Add(_stopMenuItem);
            _contextMenu.Items.Add(_pauseMenuItem);
            _contextMenu.Items.Add(_resumeMenuItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(_addFiveMinutesMenuItem);
            _contextMenu.Items.Add(_subtractFiveMinutesMenuItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(_exitMenuItem);
            
            // Assign context menu to notify icon
            _notifyIcon.ContextMenuStrip = _contextMenu;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Warning: Could not setup context menu: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Updates the context menu items' enabled state based on the current timer state.
    /// </summary>
    /// <param name="state">The current timer state.</param>
    public void UpdateContextMenuState(StateManager.State state)
    {
        if (_disposed || _contextMenu == null)
        {
            return;
        }

        try
        {
            // Use the same state transition logic as MainForm
            switch (state)
            {
                case StateManager.State.Stopped:
                    _startMenuItem!.Enabled = true;
                    _stopMenuItem!.Enabled = false;
                    _pauseMenuItem!.Enabled = false;
                    _resumeMenuItem!.Enabled = false;
                    _addFiveMinutesMenuItem!.Enabled = false;
                    _subtractFiveMinutesMenuItem!.Enabled = false;
                    break;
                    
                case StateManager.State.Running:
                    _startMenuItem!.Enabled = false;
                    _stopMenuItem!.Enabled = true;
                    _pauseMenuItem!.Enabled = true;
                    _resumeMenuItem!.Enabled = false;
                    _addFiveMinutesMenuItem!.Enabled = true;
                    _subtractFiveMinutesMenuItem!.Enabled = true;
                    break;
                    
                case StateManager.State.Paused:
                    _startMenuItem!.Enabled = false;
                    _stopMenuItem!.Enabled = true;
                    _pauseMenuItem!.Enabled = false;
                    _resumeMenuItem!.Enabled = true;
                    _addFiveMinutesMenuItem!.Enabled = false;
                    _subtractFiveMinutesMenuItem!.Enabled = false;
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Warning: Could not update context menu state: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Ensures the notification icon is always shown in the notification area (not hidden).
    /// Uses Windows Shell API to set the icon state to always visible.
    /// NOTE: This method uses reflection to access internal NotifyIcon fields, which is fragile
    /// and may break in future .NET versions if the internal implementation changes.
    /// </summary>
    private void EnsureIconAlwaysVisible()
    {
        if (_notifyIcon == null)
        {
            return;
        }
        
        try
        {
            // Get the window handle and ID of the NotifyIcon using reflection
            // WARNING: This accesses internal implementation details and may break in future versions
            var type = typeof(NotifyIcon);
            var windowField = type.GetField("window", BindingFlags.NonPublic | BindingFlags.Instance);
            var idField = type.GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (windowField != null && idField != null)
            {
                var window = windowField.GetValue(_notifyIcon);
                var id = idField.GetValue(_notifyIcon);
                
                if (window != null && id != null)
                {
                    var handleProperty = window.GetType().GetProperty("Handle");
                    if (handleProperty != null)
                    {
                        var handleValue = handleProperty.GetValue(window);
                        if (handleValue != null)
                        {
                            var handle = (IntPtr)handleValue;
                            var iconId = (uint)(int)id;
                            
                            // Create NOTIFYICONDATA structure for modifying icon state
                            NOTIFYICONDATA nidModify = new NOTIFYICONDATA
                            {
                                cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                                hWnd = handle,
                                uID = iconId,
                                uFlags = NIF_STATE,
                                dwState = 0, // 0 means not hidden
                                dwStateMask = NIS_HIDDEN // We're modifying the hidden state
                            };
                            
                            // Call Shell_NotifyIcon to modify the icon state
                            bool result = Shell_NotifyIcon(NIM_MODIFY, ref nidModify);
                            if (!result)
                            {
                                int modifyError = Marshal.GetLastWin32Error();
                                Debug.WriteLine($"Warning: Shell_NotifyIcon(NIM_MODIFY) failed with error code: {modifyError}");
                            }
                            
                            // Create NOTIFYICONDATA structure for setting icon version
                            NOTIFYICONDATA nidSetVersion = new NOTIFYICONDATA
                            {
                                cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                                hWnd = handle,
                                uID = iconId,
                                uVersion = NOTIFYICON_VERSION_4 // Windows 7 and later
                            };
                            
                            result = Shell_NotifyIcon(NIM_SETVERSION, ref nidSetVersion);
                            if (!result)
                            {
                                int setVersionError = Marshal.GetLastWin32Error();
                                Debug.WriteLine($"Warning: Shell_NotifyIcon(NIM_SETVERSION) failed with error code: {setVersionError}");
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Silent failure - this is a best-effort enhancement
            Debug.WriteLine($"Warning: Could not set notification icon to always visible: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the taskbar icon to show the current timer status and minutes remaining.
    /// </summary>
    /// <param name="state">The current timer state (Stopped, Running, or Paused).</param>
    /// <param name="minutesRemaining">The minutes remaining in the timer.</param>
    public void UpdateTaskbarIcon(StateManager.State state, int minutesRemaining)
    {
        lock (_syncLock)
        {
            if (_disposed || _notifyIcon == null)
            {
                return;
            }

            try
            {
                // Clean up previous dynamic icon and its handle
                if (_currentDynamicIcon != null && _currentDynamicIcon != _baseIcon)
                {
                    _currentDynamicIcon.Dispose();
                    _currentDynamicIcon = null;
                }
                
                if (_currentIconHandle != IntPtr.Zero)
                {
                    DestroyIcon(_currentIconHandle);
                    _currentIconHandle = IntPtr.Zero;
                }

                // Generate new icon with status overlay
                _currentDynamicIcon = GenerateStatusIcon(state, minutesRemaining, out _currentIconHandle);
                _notifyIcon.Icon = _currentDynamicIcon;

                // Update tooltip text
                string stateText = state switch
                {
                    StateManager.State.Stopped => "Stopped",
                    StateManager.State.Running => "Running",
                    StateManager.State.Paused => "Paused",
                    _ => "Unknown"
                };
                _notifyIcon.Text = $"Working Candle - {stateText} ({minutesRemaining}m)";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warning: Could not update taskbar icon: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Generates a dynamic icon with state indicator and minutes remaining.
    /// </summary>
    /// <param name="state">The current timer state.</param>
    /// <param name="minutesRemaining">The minutes remaining in the timer.</param>
    /// <param name="iconHandle">Output parameter containing the icon handle that must be destroyed later.</param>
    /// <returns>An icon with the status overlay.</returns>
    private Icon GenerateStatusIcon(StateManager.State state, int minutesRemaining, out IntPtr iconHandle)
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

            // Draw minutes text (white color for visibility)
            string minutesText = minutesRemaining.ToString();
            // Use system font for better compatibility across different systems
            using (Font font = new Font(FontFamily.GenericSansSerif, IconFontSize, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                // Measure text to center it
                SizeF textSize = g.MeasureString(minutesText, font);
                float x = (IconSize - textSize.Width) / 2;
                float y = (IconSize - textSize.Height) / 2;

                // Draw text with a slight shadow for better readability
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(ShadowAlpha, 0, 0, 0)))
                {
                    g.DrawString(minutesText, font, shadowBrush, x + ShadowOffsetX, y + ShadowOffsetY);
                }
                g.DrawString(minutesText, font, textBrush, x, y);
            }

            // Convert bitmap to icon and return the handle for later cleanup
            iconHandle = bitmap.GetHicon();
            return Icon.FromHandle(iconHandle);
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
                    
                    // Clean up the GDI handle for the dynamic icon
                    if (_currentIconHandle != IntPtr.Zero)
                    {
                        DestroyIcon(_currentIconHandle);
                        _currentIconHandle = IntPtr.Zero;
                    }
                    
                    // Clean up context menu
                    _contextMenu?.Dispose();
                    _contextMenu = null;
                    
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
