using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using RealityOS.Core;
using Debug = UnityEngine.Debug;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// Launches Windows File Explorer to browse RealityOS workspaces, notes files, and project assets.
    /// </summary>
    public static class FileExplorerLauncher
    {
        public static event Action<string> OnFolderOpened;

        /// <summary>
        /// Opens Windows File Explorer to the RealityOS persistence root directory.
        /// </summary>
        public static bool OpenPersistenceFolder()
        {
            return OpenFolder(Application.persistentDataPath);
        }

        /// <summary>
        /// Opens Windows File Explorer to the Notes Documents directory.
        /// </summary>
        public static bool OpenNotesFolder()
        {
            string path = Path.Combine(Application.persistentDataPath, "NotesDocuments");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return OpenFolder(path);
        }

        /// <summary>
        /// Opens Windows File Explorer to any validated folder path on the laptop.
        /// </summary>
        public static bool OpenFolder(string folderPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                {
                    Debug.LogWarning($"[RealityOS.FileExplorerLauncher] Folder does not exist: {folderPath}");
                    return false;
                }

                // Normalizing path for Windows
                string winPath = Path.GetFullPath(folderPath).Replace("/", "\\");
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{winPath}\"",
                    UseShellExecute = true
                };

                Process.Start(psi);
                Debug.Log($"[RealityOS.FileExplorerLauncher] Opened explorer at: {winPath}");
                OnFolderOpened?.Invoke(winPath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.FileExplorerLauncher] Error launching Explorer: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens Windows File Explorer and highlights a specific file.
        /// </summary>
        public static bool OpenFolderAndSelectFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    Debug.LogWarning($"[RealityOS.FileExplorerLauncher] File does not exist: {filePath}");
                    return false;
                }

                string winPath = Path.GetFullPath(filePath).Replace("/", "\\");
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{winPath}\"",
                    UseShellExecute = true
                };

                Process.Start(psi);
                Debug.Log($"[RealityOS.FileExplorerLauncher] Selected file in Explorer: {winPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.FileExplorerLauncher] Error opening file in Explorer: {ex.Message}");
                return false;
            }
        }
    }
}
