using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WorkingCandle;

/// <summary>
/// Service for displaying progress on the Windows taskbar.
/// Uses the ITaskbarList3 COM interface to show progress and state.
/// </summary>
public class TaskbarProgressService : IDisposable
{
    [ComImport]
    [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
    private class TaskbarList { }

    [ComImport]
    [Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFDC")]
    private interface ITaskbarList3
    {
        void HrInit();
        void AddTab(IntPtr hwnd);
        void DeleteTab(IntPtr hwnd);
        void ActivateTab(IntPtr hwnd);
        void SetActiveAlt(IntPtr hwnd);
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
        void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
        void SetProgressState(IntPtr hwnd, TaskbarProgressState state);
        void RegisterTab(IntPtr hwndTab, IntPtr hwndMDI);
        void UnregisterTab(IntPtr hwndTab);
        void SetTabOrder(IntPtr hwndTab, IntPtr hwndInsertBefore);
        void SetTabActive(IntPtr hwndTab, IntPtr hwndMDI, uint dwReserved);
        void ThumbBarAddButtons(IntPtr hwnd, uint cButtons, IntPtr pButton);
        void ThumbBarUpdateButtons(IntPtr hwnd, uint cButtons, IntPtr pButton);
        void ThumbBarSetImageList(IntPtr hwnd, IntPtr himl);
        void SetOverlayIcon(IntPtr hwnd, IntPtr hIcon, [MarshalAs(UnmanagedType.LPWStr)] string pszDescription);
        void SetThumbnailTooltip(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszTip);
        void SetThumbnailClip(IntPtr hwnd, IntPtr prcClip);
    }

    public enum TaskbarProgressState
    {
        NoProgress = 0,
        Indeterminate = 1,
        Normal = 2,
        Error = 4,
        Paused = 8
    }

    private ITaskbarList3? _taskbarList;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the TaskbarProgressService class.
    /// </summary>
    public TaskbarProgressService()
    {
        try
        {
            _taskbarList = (ITaskbarList3)new TaskbarList();
            _taskbarList.HrInit();
        }
        catch (Exception ex)
        {
            // Silently fail if taskbar API is not available (e.g., on non-Windows systems)
            Debug.WriteLine($"Warning: Taskbar progress API not available: {ex.Message}");
            _taskbarList = null;
        }
    }

    /// <summary>
    /// Sets the progress value on the taskbar.
    /// </summary>
    /// <param name="windowHandle">The window handle to set progress for.</param>
    /// <param name="progressPercent">The progress percentage (0-100).</param>
    /// <param name="isRunning">Whether the timer is running (true) or paused (false).</param>
    public void SetProgress(IntPtr windowHandle, int progressPercent, bool isRunning)
    {
        if (_disposed || _taskbarList == null)
        {
            return;
        }

        try
        {
            // Set progress value as percentage out of 100
            _taskbarList.SetProgressValue(windowHandle, (ulong)progressPercent, 100);

            // Set the progress state based on running state
            TaskbarProgressState state = isRunning ? TaskbarProgressState.Normal : TaskbarProgressState.Paused;
            _taskbarList.SetProgressState(windowHandle, state);
        }
        catch (Exception ex)
        {
            // Silently fail if taskbar API fails
            Debug.WriteLine($"Warning: Could not set taskbar progress: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears the progress display on the taskbar.
    /// </summary>
    /// <param name="windowHandle">The window handle to clear progress for.</param>
    public void ClearProgress(IntPtr windowHandle)
    {
        if (_disposed || _taskbarList == null)
        {
            return;
        }

        try
        {
            _taskbarList.SetProgressState(windowHandle, TaskbarProgressState.NoProgress);
        }
        catch (Exception ex)
        {
            // Silently fail if taskbar API fails
            Debug.WriteLine($"Warning: Could not clear taskbar progress: {ex.Message}");
        }
    }

    /// <summary>
    /// Releases all resources used by the TaskbarProgressService.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the TaskbarProgressService.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _taskbarList != null)
            {
                try
                {
                    Marshal.ReleaseComObject(_taskbarList);
                }
                catch { }
                _taskbarList = null;
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer to ensure resources are released.
    /// </summary>
    ~TaskbarProgressService()
    {
        Dispose(false);
    }
}
