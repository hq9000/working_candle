using System.Runtime.InteropServices;

namespace WorkingCandle;

/// <summary>
/// Detects presses of the Right Ctrl key system-wide using a low-level keyboard hook,
/// so that the application can react to the key being pressed regardless of whether
/// the application window is focused.
/// </summary>
/// <remarks>
/// A low-level keyboard hook is used instead of the Win32 RegisterHotKey API because
/// RegisterHotKey does not reliably trigger for a modifier key (like Right Ctrl) used
/// on its own, since modifier key presses are handled earlier in the input pipeline
/// and typically never reach the hotkey dispatch mechanism.
/// </remarks>
public class GlobalHotkeyService : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYUP = 0x0105;
    private const int LONG_PRESS_THRESHOLD_MS = 1000;

    /// <summary>
    /// Virtual key code for the Right Ctrl key.
    /// </summary>
    private const int VK_RCONTROL = 0xA3;

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    // Keep a reference to the delegate for the lifetime of the hook so it isn't
    // garbage-collected while native code still holds a pointer to it.
    private readonly LowLevelKeyboardProc _hookProc;
    private readonly object _keyStateLock = new();
    private IntPtr _hookHandle = IntPtr.Zero;
    private bool _isRightCtrlDown;
    private bool _longPressTriggered;
    private System.Threading.Timer? _longPressTimer;
    private bool _isDisposed;

    /// <summary>
    /// Event raised when the Right Ctrl key is released after a short press.
    /// </summary>
    public event EventHandler? PauseResumeHotkeyPressed;

    /// <summary>
    /// Event raised when the Right Ctrl key is held for at least one second.
    /// </summary>
    public event EventHandler? StopHotkeyPressed;

    public GlobalHotkeyService()
    {
        // Store the delegate in a field to prevent it from being collected by the GC
        // while the unmanaged hook still references it.
        _hookProc = HookCallback;
    }

    /// <summary>
    /// Installs the low-level keyboard hook used to detect Right Ctrl presses system-wide.
    /// </summary>
    public void Register()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            return;
        }

        using var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var currentModule = currentProcess.MainModule;
        IntPtr moduleHandle = currentModule != null
            ? GetModuleHandle(currentModule.ModuleName)
            : IntPtr.Zero;

        _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, moduleHandle, 0);

        if (_hookHandle == IntPtr.Zero)
        {
            System.Diagnostics.Debug.WriteLine("Warning: Failed to install global keyboard hook for the Right Ctrl hotkey.");
        }
    }

    /// <summary>
    /// Low-level keyboard hook callback. Detects Right Ctrl presses while ignoring
    /// auto-repeat and distinguishes short presses from long presses.
    /// </summary>
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            int message = (int)wParam;

            if (hookStruct.vkCode == VK_RCONTROL)
            {
                if (message is WM_KEYDOWN or WM_SYSKEYDOWN)
                {
                    System.Threading.Timer? timerToDispose = null;
                    lock (_keyStateLock)
                    {
                        if (!_isRightCtrlDown)
                        {
                            _isRightCtrlDown = true;
                            _longPressTriggered = false;
                            timerToDispose = _longPressTimer;
                            _longPressTimer = new System.Threading.Timer(
                                OnLongPressTimerElapsed,
                                null,
                                LONG_PRESS_THRESHOLD_MS,
                                Timeout.Infinite);
                        }
                        timerToDispose?.Dispose();
                    }
                }
                else if (message is WM_KEYUP or WM_SYSKEYUP)
                {
                    bool isShortPress;
                    System.Threading.Timer? timerToDispose;
                    lock (_keyStateLock)
                    {
                        isShortPress = _isRightCtrlDown && !_longPressTriggered;
                        _isRightCtrlDown = false;
                        timerToDispose = _longPressTimer;
                        _longPressTimer = null;
                    }

                    timerToDispose?.Dispose();

                    if (isShortPress)
                    {
                        PauseResumeHotkeyPressed?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private void OnLongPressTimerElapsed(object? state)
    {
        bool raiseStopEvent;
        lock (_keyStateLock)
        {
            raiseStopEvent = !_isDisposed && _isRightCtrlDown && !_longPressTriggered;
            _longPressTriggered = raiseStopEvent;
        }

        if (raiseStopEvent)
        {
            StopHotkeyPressed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Uninstalls the keyboard hook and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        System.Threading.Timer? timerToDispose;
        lock (_keyStateLock)
        {
            _isDisposed = true;
            _isRightCtrlDown = false;
            timerToDispose = _longPressTimer;
            _longPressTimer = null;
        }
        timerToDispose?.Dispose();

        if (_hookHandle != IntPtr.Zero)
        {
            if (!UnhookWindowsHookEx(_hookHandle))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Failed to uninstall global keyboard hook for the Right Ctrl hotkey.");
            }
            _hookHandle = IntPtr.Zero;
        }

        GC.SuppressFinalize(this);
    }
}
