using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using RealityOS.Core;
using Debug = UnityEngine.Debug;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// Safe, allowlisted application launcher connecting RealityOS with installed Windows applications.
    /// Strictly limits execution to authorized native tools (Chrome, Notepad, Calc, VS Code, Explorer).
    /// </summary>
    public static class WindowsApplicationLauncher
    {
        public static event Action<ExternalAppType, Process> OnApplicationLaunched;
        public static event Action<ExternalAppType, string> OnApplicationLaunchFailed;

        // Tracks recently launched external processes
        private static readonly Dictionary<ExternalAppType, Process> _activeProcesses = new Dictionary<ExternalAppType, Process>();

        public static IReadOnlyDictionary<ExternalAppType, Process> ActiveProcesses => _activeProcesses;

        /// <summary>
        /// Locates the real executable path for an allowlisted application on Windows.
        /// </summary>
        public static string ResolveExecutablePath(ExternalAppType app)
        {
            switch (app)
            {
                case ExternalAppType.Chrome:
                    return FindFirstExistingPath(
                        @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                        @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe")
                    );

                case ExternalAppType.Notepad:
                    return FindFirstExistingPath(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "notepad.exe"),
                        "notepad.exe"
                    );

                case ExternalAppType.Calculator:
                    return FindFirstExistingPath(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "calc.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "calc.exe"),
                        "calc.exe"
                    );

                case ExternalAppType.VSCode:
                    return FindFirstExistingPath(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\Code.exe"),
                        @"C:\Program Files\Microsoft VS Code\Code.exe",
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Microsoft VS Code\bin\code.cmd")
                    );

                case ExternalAppType.FileExplorer:
                    return FindFirstExistingPath(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"),
                        "explorer.exe"
                    );

                default:
                    return null;
            }
        }

        public static bool IsAppAvailable(ExternalAppType app)
        {
            string path = ResolveExecutablePath(app);
            if (string.IsNullOrEmpty(path)) return false;
            if (File.Exists(path)) return true;

            // Verify if command is directly resolvable on system PATH
            if (!path.Contains("\\") && !path.Contains("/"))
            {
                string pathEnv = Environment.GetEnvironmentVariable("PATH");
                if (!string.IsNullOrEmpty(pathEnv))
                {
                    foreach (string dir in pathEnv.Split(';'))
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(dir) && File.Exists(Path.Combine(dir.Trim(), path)))
                            {
                                return true;
                            }
                        }
                        catch { }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Launches an allowlisted external Windows application with optional safe arguments.
        /// </summary>
        public static Process Launch(ExternalAppType app, string arguments = "")
        {
            try
            {
                string exePath = ResolveExecutablePath(app);
                if (string.IsNullOrEmpty(exePath))
                {
                    string err = $"Application {app} was not found in standard installation paths.";
                    Debug.LogWarning($"[RealityOS.LaptopIntegration] {err}");
                    OnApplicationLaunchFailed?.Invoke(app, err);
                    return null;
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments ?? string.Empty,
                    UseShellExecute = true, // Normal desktop launch
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                };

                Process process = Process.Start(psi);
                if (process != null)
                {
                    _activeProcesses[app] = process;
                    Debug.Log($"[RealityOS.LaptopIntegration] Successfully launched {app} (PID: {process.Id}) with arguments: '{arguments}'");
                    OnApplicationLaunched?.Invoke(app, process);
                    return process;
                }
                else
                {
                    string err = $"Process.Start returned null for {app}.";
                    Debug.LogWarning($"[RealityOS.LaptopIntegration] {err}");
                    OnApplicationLaunchFailed?.Invoke(app, err);
                    return null;
                }
            }
            catch (Exception ex)
            {
                string err = $"Failed to launch {app}: {ex.Message}";
                Debug.LogError($"[RealityOS.LaptopIntegration] {err}");
                OnApplicationLaunchFailed?.Invoke(app, err);
                return null;
            }
        }

        public static Process GetActiveProcess(ExternalAppType app)
        {
            if (_activeProcesses.TryGetValue(app, out Process proc))
            {
                if (proc != null && !proc.HasExited)
                {
                    return proc;
                }
                _activeProcesses.Remove(app);
            }
            return null;
        }

        private static string FindFirstExistingPath(params string[] candidatePaths)
        {
            foreach (var path in candidatePaths)
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    return path;
                }
            }
            // Return the first non-empty fallback if none physically found (for system commands like notepad.exe)
            return candidatePaths.Length > 0 ? candidatePaths[candidatePaths.Length - 1] : null;
        }
    }
}
