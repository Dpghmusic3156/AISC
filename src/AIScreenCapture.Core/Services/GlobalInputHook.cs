using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AIScreenCapture.Core.Services;

/// <summary>
/// Monitors global keyboard + mouse input using low-level Win32 hooks.
/// Fires CaptureTriggered when Ctrl + Middle Mouse Button is detected.
/// </summary>
public class GlobalInputHook : IDisposable
{
    // Win32 hook type constants
    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;

    // Keyboard messages
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    // Mouse messages
    private const int WM_MBUTTONDOWN = 0x0207;

    // Virtual key codes
    private const int VK_LCONTROL = 0xA2;
    private const int VK_RCONTROL = 0xA3;

    private IntPtr _keyboardHookId = IntPtr.Zero;
    private IntPtr _mouseHookId = IntPtr.Zero;
    private bool _isCtrlPressed;
    private bool _disposed;

    // Must store delegates as fields to prevent GC collection
    private readonly LowLevelKeyboardProc _keyboardProc;
    private readonly LowLevelMouseProc _mouseProc;

    /// <summary>
    /// Raised when the capture shortcut (Ctrl + Middle Mouse Button) is triggered.
    /// </summary>
    public event EventHandler? CaptureTriggered;

    /// <summary>
    /// Raised when Middle Mouse Button is clicked alone (used to toggle visibility).
    /// </summary>
    public event EventHandler? ToggleResultVisibility;

    public GlobalInputHook()
    {
        _keyboardProc = KeyboardHookCallback;
        _mouseProc = MouseHookCallback;
    }

    /// <summary>
    /// Installs the global keyboard and mouse hooks.
    /// </summary>
    public void Install()
    {
        if (_keyboardHookId != IntPtr.Zero || _mouseHookId != IntPtr.Zero)
            return; // Already installed

        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        var moduleHandle = GetModuleHandle(curModule.ModuleName);

        _keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, moduleHandle, 0);
        _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, moduleHandle, 0);

        if (_keyboardHookId == IntPtr.Zero || _mouseHookId == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                $"Failed to install global hooks. Keyboard: {_keyboardHookId}, Mouse: {_mouseHookId}. " +
                $"Error: {Marshal.GetLastWin32Error()}");
        }
    }

    /// <summary>
    /// Removes the global hooks.
    /// </summary>
    public void Uninstall()
    {
        if (_keyboardHookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_keyboardHookId);
            _keyboardHookId = IntPtr.Zero;
        }

        if (_mouseHookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_mouseHookId);
            _mouseHookId = IntPtr.Zero;
        }

        _isCtrlPressed = false;
    }

    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            if (vkCode == VK_LCONTROL || vkCode == VK_RCONTROL)
            {
                int msg = wParam.ToInt32();
                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    _isCtrlPressed = true;
                }
                else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
                {
                    _isCtrlPressed = false;
                }
            }
        }

        return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam.ToInt32() == WM_MBUTTONDOWN)
        {
            if (_isCtrlPressed)
            {
                CaptureTriggered?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ToggleResultVisibility?.Invoke(this, EventArgs.Empty);
            }
        }

        return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Uninstall();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~GlobalInputHook()
    {
        Dispose();
    }

    #region P/Invoke Declarations

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    #endregion
}
