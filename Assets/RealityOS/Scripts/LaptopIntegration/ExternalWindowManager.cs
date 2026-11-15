using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using RealityOS.Core;
using Debug = UnityEngine.Debug;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// Uses safe Windows Win32 API interop (user32.dll) to coordinate and position external application
    /// windows (Chrome, Notepad, VS Code) side-by-side with RealityOS on the user's laptop screen.
    /// </summary>
    public static class ExternalWindowManager
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int SW_RESTORE = 9;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
#endif

        /// <summary>
        /// Arranges the RealityOS window and the external application window side-by-side (50% / 50%).
        /// </summary>
        public static bool TileSideBySide(ExternalAppType app)
        {
            return ApplyLayout(app, 0.5f);
        }

        /// <summary>
        /// Arranges RealityOS on the left (60%) and the external app docked on the right (40%).
        /// </summary>
        public static bool TileDockRight(ExternalAppType app)
        {
            return ApplyLayout(app, 0.6f);
        }

        private static bool ApplyLayout(ExternalAppType app, float unityRatio)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            try
            {
                int screenW = GetSystemMetrics(SM_CXSCREEN);
                int screenH = GetSystemMetrics(SM_CYSCREEN);

                if (screenW <= 0) screenW = Screen.currentResolution.width;
                if (screenH <= 0) screenH = Screen.currentResolution.height;

                // 1. Locate Unity Window Handle
                IntPtr unityHwnd = Process.GetCurrentProcess().MainWindowHandle;
                if (unityHwnd == IntPtr.Zero)
                {
                    unityHwnd = GetActiveWindow();
                }

                int unityW = Mathf.RoundToInt(screenW * unityRatio);
                int extX = unityW;
                int extW = screenW - unityW;

                // Position Unity Window
                if (unityHwnd != IntPtr.Zero)
                {
                    ShowWindow(unityHwnd, SW_RESTORE);
                    SetWindowPos(unityHwnd, IntPtr.Zero, 0, 0, unityW, screenH, SWP_NOZORDER | SWP_SHOWWINDOW);
                }

                // 2. Locate External Application Window Handle
                Process extProc = WindowsApplicationLauncher.GetActiveProcess(app);
                if (extProc == null)
                {
                    extProc = WindowsApplicationLauncher.Launch(app);
                }

                if (extProc != null)
                {
                    IntPtr extHwnd = GetProcessWindowHandle(extProc);
                    if (extHwnd != IntPtr.Zero)
                    {
                        ShowWindow(extHwnd, SW_RESTORE);
                        SetWindowPos(extHwnd, IntPtr.Zero, extX, 0, extW, screenH, SWP_NOZORDER | SWP_SHOWWINDOW);
                        SetForegroundWindow(extHwnd);
                        Debug.Log($"[RealityOS.ExternalWindowManager] Tiled {app} alongside RealityOS: Unity (0..{unityW}), {app} ({extX}..{screenW})");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RealityOS.ExternalWindowManager] Win32 SetWindowPos error: {ex.Message}");
                return false;
            }
#else
            Debug.LogWarning("[RealityOS.ExternalWindowManager] External window arrangement is supported on Windows.");
            return false;
#endif
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private static IntPtr GetProcessWindowHandle(Process proc)
        {
            if (proc == null) return IntPtr.Zero;

            try
            {
                proc.Refresh();
                if (proc.MainWindowHandle != IntPtr.Zero)
                {
                    return proc.MainWindowHandle;
                }

                // If MainWindowHandle is Zero (e.g. Chrome tabs / multiple processes), search with EnumWindows
                IntPtr found = IntPtr.Zero;
                uint targetPid = (uint)proc.Id;

                EnumWindows((hWnd, lParam) =>
                {
                    if (IsWindowVisible(hWnd))
                    {
                        GetWindowThreadProcessId(hWnd, out uint pid);
                        if (pid == targetPid)
                        {
                            found = hWnd;
                            return false; // Stop enumeration
                        }
                    }
                    return true;
                }, IntPtr.Zero);

                return found;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
#endif
    }
}
