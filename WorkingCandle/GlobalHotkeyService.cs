using System.Runtime.InteropServices;

namespace WorkingCandle;

/// <summary>
/// Registers and manages a system-wide (global) hotkey using the Win32 RegisterHotKey API,
/// so that the application can react to the hotkey being pressed regardless of whether
/// the application window is focused.
/// </summary>
public class GlobalHotkeyService : IDisposable
{
    /// <summary>
    /// Windows message identifier sent when a registered hotkey is pressed.
    /// </summary>
    private const int WM_HOTKEY = 0x0312;

    /// <summary>
    /// Identifier used to register/unregister the pause/resume hotkey.
    /// </summary>
    private const int HOTKEY_ID_PAUSE_RESUME = 1;

    /// <summary>
    /// Virtual key code for the Right Ctrl key.
    /// </summary>
    private const uint VK_RCONTROL = 0xA3;

    /// <summary>
    /// No modifier keys required for the hotkey.
    /// </summary>
    private const uint MOD_NONE = 0x0000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private IntPtr _windowHandle = IntPtr.Zero;
    private bool _isRegistered;

    /// <summary>
    /// Event raised when the pause/resume global hotkey (Right Ctrl) is pressed.
    /// </summary>
    public event EventHandler? PauseResumeHotkeyPressed;

    /// <summary>
    /// Registers the global pause/resume hotkey (Right Ctrl) against the specified window handle.
    /// </summary>
    /// <param name="windowHandle">The handle of the window that will receive the WM_HOTKEY message.</param>
    public void Register(IntPtr windowHandle)
    {
        if (_isRegistered)
        {
            return;
        }

        _windowHandle = windowHandle;
        _isRegistered = RegisterHotKey(_windowHandle, HOTKEY_ID_PAUSE_RESUME, MOD_NONE, VK_RCONTROL);

        if (!_isRegistered)
        {
            System.Diagnostics.Debug.WriteLine("Warning: Failed to register global hotkey (Right Ctrl). It may already be in use by another application.");
        }
    }

    /// <summary>
    /// Processes a Windows message and raises <see cref="PauseResumeHotkeyPressed"/> if it
    /// corresponds to the registered pause/resume hotkey.
    /// </summary>
    /// <param name="m">The Windows message to inspect.</param>
    public void ProcessWndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID_PAUSE_RESUME)
        {
            PauseResumeHotkeyPressed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Unregisters the global hotkey and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        if (_isRegistered)
        {
            if (!UnregisterHotKey(_windowHandle, HOTKEY_ID_PAUSE_RESUME))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Failed to unregister global hotkey (Right Ctrl).");
            }
            _isRegistered = false;
        }

        GC.SuppressFinalize(this);
    }
}
