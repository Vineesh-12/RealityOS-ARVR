using System;
using UnityEngine;
using RealityOS.Core;
using RealityOS.Panels;
using RealityOS.Persistence;
using RealityOS.Workspace;
using RealityOS.UI;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// 3D Spatial Panel inside RealityOS that provides an interactive control hub for the user's
    /// Windows laptop: launching Chrome, Notepad, File Explorer, Calculator, VS Code, and tiling windows.
    /// </summary>
    public class LaptopConnectPanel : WorkspacePanelBase
    {
        private string _customUrl = "https://www.google.com";
        private string _statusFeedback = "Ready. Select an action below.";
        private float _statusFeedbackTimer = 0f;

        private GUIStyle _panelTitleStyle;
        private GUIStyle _subTitleStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _accentBtnStyle;
        private GUIStyle _boxStyle;
        private GUIStyle _statusStyle;
        private bool _guiStylesInit = false;

        protected override void Awake()
        {
            _panelType = PanelType.LaptopConnect;
            base.Awake();
            UpdateHubWorldText();
        }

        protected override void Start()
        {
            base.Start();
            UpdateHubWorldText();
        }

        private void UpdateHubWorldText()
        {
            bool chromeAvail = WindowsApplicationLauncher.IsAppAvailable(ExternalAppType.Chrome);
            bool vsCodeAvail = WindowsApplicationLauncher.IsAppAvailable(ExternalAppType.VSCode);

            string text = $"Windows Laptop Hub\n" +
                          $"* Chrome: {(chromeAvail ? "Installed" : "Not Found")}\n" +
                          $"* Notepad: Ready\n" +
                          $"* VS Code: {(vsCodeAvail ? "Installed" : "Not Found")}\n" +
                          $"* Click to open laptop controls";
            SetWorldBodyText(text);
        }

        protected override void Update()
        {
            base.Update();

            if (_statusFeedbackTimer > 0f)
            {
                _statusFeedbackTimer -= Time.deltaTime;
                if (_statusFeedbackTimer <= 0f)
                {
                    _statusFeedback = "Ready. Select an action below.";
                }
            }
        }

        private void InitGuiStyles()
        {
            if (_guiStylesInit) return;

            _panelTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _panelTitleStyle.normal.textColor = UITheme.PrimaryTeal;

            _subTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            _subTitleStyle.normal.textColor = UITheme.PrimaryText;

            _btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            _btnStyle.normal.textColor = Color.white;

            _accentBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            _accentBtnStyle.normal.textColor = UITheme.PrimaryTeal;

            _boxStyle = new GUIStyle(GUI.skin.box);

            _statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft
            };
            _statusStyle.normal.textColor = new Color(0.75f, 0.95f, 0.85f);

            _guiStylesInit = true;
        }

        private void OnGUI()
        {
            // Only draw the interactive controls when this panel is selected by the user
            if (!IsSelected) return;

            InitGuiStyles();

            // Right-side Floating Inspector Card
            float w = 340;
            float h = 420;
            float x = Screen.width - w - 20;
            float y = 70;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none, _boxStyle);

            GUI.Label(new Rect(x + 10, y + 10, w - 20, 22), "💻 Windows Laptop Integration", _panelTitleStyle);

            float curY = y + 36;

            // SECTION 1: Launch Windows Applications
            GUI.Label(new Rect(x + 12, curY, w - 24, 18), "Native Applications:", _subTitleStyle);
            curY += 20;

            float btnHalfW = (w - 30) / 2.0f;
            if (GUI.Button(new Rect(x + 12, curY, btnHalfW, 28), "Launch Chrome", _btnStyle))
            {
                var p = WindowsApplicationLauncher.Launch(ExternalAppType.Chrome);
                SetFeedback(p != null ? $"Launched Chrome (PID: {p.Id})" : "Failed: Chrome executable not found.");
            }
            if (GUI.Button(new Rect(x + 18 + btnHalfW, curY, btnHalfW, 28), "Launch Notepad", _btnStyle))
            {
                var p = WindowsApplicationLauncher.Launch(ExternalAppType.Notepad);
                SetFeedback(p != null ? $"Launched Notepad (PID: {p.Id})" : "Failed: Notepad not found.");
            }
            curY += 32;

            if (GUI.Button(new Rect(x + 12, curY, btnHalfW, 28), "Launch VS Code", _btnStyle))
            {
                var p = WindowsApplicationLauncher.Launch(ExternalAppType.VSCode);
                SetFeedback(p != null ? "Launched VS Code successfully." : "Failed: VS Code not found in standard paths.");
            }
            if (GUI.Button(new Rect(x + 18 + btnHalfW, curY, btnHalfW, 28), "Calculator", _btnStyle))
            {
                var p = WindowsApplicationLauncher.Launch(ExternalAppType.Calculator);
                SetFeedback(p != null ? "Launched Windows Calculator." : "Failed: Calculator not found.");
            }
            curY += 34;

            // SECTION 2: Live Notepad Synchronization
            GUI.Label(new Rect(x + 12, curY, w - 24, 18), "Real-time Note Synchronization:", _subTitleStyle);
            curY += 20;

            if (GUI.Button(new Rect(x + 12, curY, w - 24, 28), "📝 Sync Active Note to Notepad", _accentBtnStyle))
            {
                NotesPanel note = FindAnyObjectByType<NotesPanel>();
                if (note != null)
                {
                    bool ok = NotesFileManager.Instance.OpenNotesPanelInNotepad(note);
                    SetFeedback(ok ? "Note exported to .txt & opened in Notepad!" : "Failed to open in Notepad.");
                }
                else
                {
                    SetFeedback("No NotesPanel found in workspace.");
                }
            }
            curY += 34;

            // SECTION 3: Web Browsing Quick Actions
            GUI.Label(new Rect(x + 12, curY, w - 24, 18), "Spatial Web Browser:", _subTitleStyle);
            curY += 20;

            _customUrl = GUI.TextField(new Rect(x + 12, curY, w - 100, 24), _customUrl);
            if (GUI.Button(new Rect(x + w - 82, curY, 70, 24), "Go", _btnStyle))
            {
                BrowserLauncher.OpenUrl(_customUrl);
                SetFeedback($"Navigating: {_customUrl}");
            }
            curY += 28;

            float bmW = (w - 36) / 3.0f;
            if (GUI.Button(new Rect(x + 12, curY, bmW, 24), "GitHub", _btnStyle))
            {
                BrowserLauncher.OpenUrl("https://github.com");
                SetFeedback("Opening GitHub in Chrome...");
            }
            if (GUI.Button(new Rect(x + 18 + bmW, curY, bmW, 24), "Wikipedia", _btnStyle))
            {
                BrowserLauncher.OpenUrl("https://en.wikipedia.org");
                SetFeedback("Opening Wikipedia...");
            }
            if (GUI.Button(new Rect(x + 24 + bmW * 2, curY, bmW, 24), "Unity 6", _btnStyle))
            {
                BrowserLauncher.OpenUrl("https://docs.unity3d.com/6000.0/Documentation/Manual/index.html");
                SetFeedback("Opening Unity 6 Manual...");
            }
            curY += 32;

            // SECTION 4: Window Layout & File Explorer
            GUI.Label(new Rect(x + 12, curY, w - 24, 18), "Window Arrangement & Storage:", _subTitleStyle);
            curY += 20;

            if (GUI.Button(new Rect(x + 12, curY, btnHalfW, 26), "Tile Side-by-Side", _btnStyle))
            {
                ExternalWindowManager.TileSideBySide(ExternalAppType.Chrome);
                SetFeedback("Tiled Unity (Left) & Chrome (Right)!");
            }
            if (GUI.Button(new Rect(x + 18 + btnHalfW, curY, btnHalfW, 26), "Tile Dock Right", _btnStyle))
            {
                ExternalWindowManager.TileDockRight(ExternalAppType.Notepad);
                SetFeedback("Docked Notepad to right 40%!");
            }
            curY += 30;

            if (GUI.Button(new Rect(x + 12, curY, w - 24, 26), "📁 Open Workspace Data Folder", _btnStyle))
            {
                FileExplorerLauncher.OpenPersistenceFolder();
                SetFeedback("Opened Workspace folder in Explorer.");
            }
            curY += 30;

            // Status Bar at bottom of card
            GUI.Box(new Rect(x + 10, y + h - 34, w - 20, 26), GUIContent.none, _boxStyle);
            GUI.Label(new Rect(x + 16, y + h - 30, w - 32, 20), _statusFeedback, _statusStyle);
        }

        private void SetFeedback(string message)
        {
            _statusFeedback = message;
            _statusFeedbackTimer = 5.0f;
            Debug.Log($"[RealityOS.LaptopConnectPanel] {message}");
        }

        public override PanelData ExportData()
        {
            PanelData data = base.ExportData();
            data.notesContent = _customUrl; // Store current URL
            return data;
        }

        public override void ImportData(PanelData data)
        {
            base.ImportData(data);
            if (data != null && !string.IsNullOrEmpty(data.notesContent))
            {
                _customUrl = data.notesContent;
            }
            UpdateHubWorldText();
        }
    }
}
