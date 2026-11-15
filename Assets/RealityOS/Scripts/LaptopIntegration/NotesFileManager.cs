using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RealityOS.Core;
using RealityOS.Panels;
using Debug = UnityEngine.Debug;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// Manages real physical .txt documents on the user's laptop and provides two-way
    /// synchronization between multiple RealityOS 3D Notes panels and Windows Notepad.
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
        
        // Multi-note mapping: normalized file path -> NotesPanel
        private readonly Dictionary<string, NotesPanel> _activeMonitoredNotes = new Dictionary<string, NotesPanel>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, float> _lastEventTimes = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        private const float DEBOUNCE_INTERVAL = 0.35f;

        public string DocumentsFolder => _documentsFolder;
        public IReadOnlyDictionary<string, NotesPanel> MonitoredNotes => _activeMonitoredNotes;

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

                _watcher.Changed += OnFileSystemEvent;
                _watcher.Created += OnFileSystemEvent;
                Debug.Log($"[RealityOS.NotesFileManager] FileSystemWatcher active on {_documentsFolder}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RealityOS.NotesFileManager] Could not initialize FileSystemWatcher: {ex.Message}");
            }
        }

        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            // Debounce file events in background thread queue
            _changedFilePaths.Enqueue(e.FullPath);
        }

        private void Update()
        {
            // Drain queue on Unity main thread without blocking
            while (_changedFilePaths.TryDequeue(out string filePath))
            {
                float now = Time.realtimeSinceStartup;
                if (_lastEventTimes.TryGetValue(filePath, out float lastTime))
                {
                    if (now - lastTime < DEBOUNCE_INTERVAL)
                    {
                        continue; // Skip duplicate rapid event
                    }
                }
                _lastEventTimes[filePath] = now;
                ProcessFileChange(filePath);
            }
        }

        private void ProcessFileChange(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;

                // Non-blocking file read with shared read/write lock
                string content = ReadTextNonBlocking(filePath);
                if (content == null) return;

                Debug.Log($"[RealityOS.NotesFileManager] Detected external edit in Notepad: {Path.GetFileName(filePath)} ({content.Length} chars)");

                // Multi-note update: route to the matching NotesPanel!
                string norm = Path.GetFullPath(filePath);
                if (_activeMonitoredNotes.TryGetValue(norm, out NotesPanel panel) && panel != null)
                {
                    panel.SetContent(content);
                }

                OnFileContentUpdated?.Invoke(filePath, content);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RealityOS.NotesFileManager] Error reading updated file: {ex.Message}");
            }
        }

        private string ReadTextNonBlocking(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                // File locked by external editor; will be read on next tick without hanging Unity thread
                return null;
            }
        }

        /// <summary>
        /// Exports text from a NotesPanel into a physical .txt file and launches Windows Notepad.
        /// Changes saved in Notepad will auto-sync back to the specific panel in real time.
        /// </summary>
        public bool OpenNotesPanelInNotepad(NotesPanel panel, string optionalFileName = null)
        {
            if (panel == null) return false;

            InitializeFolder();

            // Safe, unique filename incorporating clean title and panel GUID
            string cleanTitle = CleanFileName(panel.Title);
            string guidSnippet = panel.PanelId.Length >= 8 ? panel.PanelId.Substring(0, 8) : "doc";

            string fileName = string.IsNullOrWhiteSpace(optionalFileName)
                ? $"Notes_{cleanTitle}_{guidSnippet}.txt"
                : CleanFileName(optionalFileName);

            if (!fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".txt";
            }

            string fullPath = Path.Combine(_documentsFolder, fileName);
            string normPath = Path.GetFullPath(fullPath);

            // Path Traversal Security check
            if (!normPath.StartsWith(Path.GetFullPath(_documentsFolder), StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError($"[RealityOS.NotesFileManager] Rejected illegal path traversal attempt: {fullPath}");
                return false;
            }

            try
            {
                // Export current content from 3D panel to text file
                string currentContent = panel.ExportData().notesContent ?? "";
                File.WriteAllText(normPath, currentContent);

                // Register panel in multi-note mapping
                _activeMonitoredNotes[normPath] = panel;

                // Launch Notepad with this file
                var proc = WindowsApplicationLauncher.Launch(ExternalAppType.Notepad, $"\"{normPath}\"");
                return proc != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.NotesFileManager] Failed to export and open note in Notepad: {ex.Message}");
                return false;
            }
        }

        private string CleanFileName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Note";
            char[] invalid = Path.GetInvalidFileNameChars();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in raw)
            {
                if (Array.IndexOf(invalid, c) < 0 && c != ' ')
                {
                    sb.Append(c);
                }
                else if (c == ' ')
                {
                    sb.Append('_');
                }
            }
            return sb.Length > 0 ? sb.ToString() : "Note";
        }

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
