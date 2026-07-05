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
    private IntPtr _hookHandle = IntPtr.Zero;
    private bool _isRightCtrlDown;

    /// <summary>
    /// Event raised when the Right Ctrl key is pressed (key-down transition).
    /// </summary>
    public event EventHandler? PauseResumeHotkeyPressed;

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
    /// Low-level keyboard hook callback. Detects the key-down transition of Right Ctrl
    /// (ignoring auto-repeat while the key is held) and raises <see cref="PauseResumeHotkeyPressed"/>.
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
                    if (!_isRightCtrlDown)
                    {
                        _isRightCtrlDown = true;
                        PauseResumeHotkeyPressed?.Invoke(this, EventArgs.Empty);
                    }
                }
                else if (message is WM_KEYUP or WM_SYSKEYUP)
                {
                    _isRightCtrlDown = false;
                }
            }
        }

        return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    /// <summary>
    /// Uninstalls the keyboard hook and releases associated resources.
    /// </summary>
    public void Dispose()
    {
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
