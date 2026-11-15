using System;
using System.Collections.Concurrent;
using System.IO;
using UnityEngine;
using RealityOS.Core;
using RealityOS.Panels;
using RealityOS.Workspace;
using Debug = UnityEngine.Debug;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// Manages real physical .txt document files on the user's laptop and synchronizes them
    /// two-way between RealityOS 3D Notes panels and Windows Notepad using a FileSystemWatcher.
    /// </summary>
    public class NotesFileManager : MonoBehaviour
    {
        private static NotesFileManager _instance;
        public static NotesFileManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<NotesFileManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("RealityOS_NotesFileManager");
                        _instance = go.AddComponent<NotesFileManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        public static event Action<string, string> OnFileContentUpdated; // (filePath, newText)

        private string _documentsFolder;
        private FileSystemWatcher _watcher;
        private readonly ConcurrentQueue<string> _changedFilePaths = new ConcurrentQueue<string>();
        private NotesPanel _monitoredNotesPanel;
        private string _activeMonitoredFile;

        public string DocumentsFolder => _documentsFolder;
        public string ActiveMonitoredFile => _activeMonitoredFile;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeFolder();
            SetupWatcher();
        }

        private void InitializeFolder()
        {
            _documentsFolder = Path.Combine(Application.persistentDataPath, "NotesDocuments");
            if (!Directory.Exists(_documentsFolder))
            {
                Directory.CreateDirectory(_documentsFolder);
            }
        }

        private void SetupWatcher()
        {
            try
            {
                if (_watcher != null)
                {
                    _watcher.Dispose();
                }

                _watcher = new FileSystemWatcher(_documentsFolder, "*.txt")
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };

                _watcher.Changed += OnFileChanged;
                _watcher.Created += OnFileChanged;
                Debug.Log($"[RealityOS.NotesFileManager] FileSystemWatcher active on {_documentsFolder}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RealityOS.NotesFileManager] Could not initialize FileSystemWatcher: {ex.Message}");
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Background thread: enqueue for main thread consumption
            _changedFilePaths.Enqueue(e.FullPath);
        }

        private void Update()
        {
            // Drain queue on Unity main thread
            while (_changedFilePaths.TryDequeue(out string filePath))
            {
                ProcessFileChange(filePath);
            }
        }

        private void ProcessFileChange(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;

                // Brief delay retry in case Notepad is holding write lock
                string content = ReadTextWithRetry(filePath, 3);
                if (content == null) return;

                Debug.Log($"[RealityOS.NotesFileManager] Detected external edit in Notepad: {Path.GetFileName(filePath)} ({content.Length} chars)");

                // If currently syncing with a specific NotesPanel, update it directly!
                if (_monitoredNotesPanel != null && string.Equals(filePath, _activeMonitoredFile, StringComparison.OrdinalIgnoreCase))
                {
                    _monitoredNotesPanel.SetContent(content);
                }

                OnFileContentUpdated?.Invoke(filePath, content);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RealityOS.NotesFileManager] Error reading updated file: {ex.Message}");
            }
        }

        private string ReadTextWithRetry(string path, int maxRetries)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(50);
                }
            }
            return null;
        }

        /// <summary>
        /// Exports text from a NotesPanel into a physical .txt file and launches Windows Notepad.
        /// Changes saved in Notepad will auto-sync back to the panel in real time.
        /// </summary>
        public bool OpenNotesPanelInNotepad(NotesPanel panel, string optionalFileName = null)
        {
            if (panel == null) return false;

            InitializeFolder();

            string fileName = string.IsNullOrWhiteSpace(optionalFileName)
                ? $"Notes_{panel.Title.Replace(" ", "_")}.txt"
                : optionalFileName;

            if (!fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".txt";
            }

            string fullPath = Path.Combine(_documentsFolder, fileName);

            try
            {
                // Export current content from 3D panel to text file
                string currentContent = panel.ExportData().notesContent ?? "";
                File.WriteAllText(fullPath, currentContent);

                _monitoredNotesPanel = panel;
                _activeMonitoredFile = fullPath;

                // Launch Notepad with this file
                var proc = WindowsApplicationLauncher.Launch(ExternalAppType.Notepad, $"\"{fullPath}\"");
                return proc != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.NotesFileManager] Failed to export and open note in Notepad: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads all .txt files available in the NotesDocuments folder.
        /// </summary>
        public string[] GetAvailableTextFiles()
        {
            InitializeFolder();
            if (Directory.Exists(_documentsFolder))
            {
                return Directory.GetFiles(_documentsFolder, "*.txt");
            }
            return new string[0];
        }

        private void OnDestroy()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }
    }
}
