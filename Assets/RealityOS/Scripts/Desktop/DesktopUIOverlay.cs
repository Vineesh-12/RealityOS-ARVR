using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Workspace;
using RealityOS.Panels;

namespace RealityOS.Desktop
{
    public class DesktopUIOverlay : MonoBehaviour
    {
        [Header("Header Info")]
        [SerializeField] private TextMeshProUGUI _workspaceTitleText;
        [SerializeField] private TextMeshProUGUI _saveStatusText;

        [Header("Camera Controller Reference")]
        [SerializeField] private DesktopCameraController _cameraController;

        private string _statusMessage = "Welcome to RealityOS Desktop Spatial Mode!";
        private float _statusTimer = 4.0f;
        private bool _showHelp = false;

        private GUIStyle _headerStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _accentBtnStyle;
        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _subLabelStyle;
        private bool _stylesInitialized = false;

        private void Start()
        {
            if (_cameraController == null)
            {
                _cameraController = FindAnyObjectByType<DesktopCameraController>();
            }

            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.OnWorkspaceLoaded += OnWorkspaceLoaded;
                WorkspaceManager.Instance.OnWorkspaceSaved += OnWorkspaceSaved;
            }
        }

        private void OnDestroy()
        {
            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.OnWorkspaceLoaded -= OnWorkspaceLoaded;
                WorkspaceManager.Instance.OnWorkspaceSaved -= OnWorkspaceSaved;
            }
        }

        private void Update()
        {
            if (_statusTimer > 0f)
            {
                _statusTimer -= Time.deltaTime;
                if (_statusTimer <= 0f) _statusMessage = "";
            }

            if (Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.F1))
            {
                _showHelp = !_showHelp;
            }
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _headerStyle.normal.textColor = new Color(0.0f, 0.9f, 0.85f); // Cyan

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            _buttonStyle.normal.textColor = Color.white;

            _accentBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            _accentBtnStyle.normal.textColor = new Color(0.0f, 0.95f, 0.9f);

            _boxStyle = new GUIStyle(GUI.skin.box);

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _labelStyle.normal.textColor = Color.white;

            _subLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal
            };
            _subLabelStyle.normal.textColor = new Color(0.8f, 0.85f, 0.9f);

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            // 1. Top HUD Bar
            float barWidth = Mathf.Min(Screen.width - 20, 1100);
            float startX = (Screen.width - barWidth) * 0.5f;

            GUI.Box(new Rect(startX, 10, barWidth, 48), GUIContent.none, _boxStyle);

            string wsName = WorkspaceManager.Instance?.ActiveWorkspace?.workspaceName ?? "Default Workspace";
            GUI.Label(new Rect(startX + 12, 16, 260, 32), $"RealityOS | {wsName}", _headerStyle);

            float btnX = startX + 270;
            float btnW = 75;
            float btnH = 30;
            float btnY = 19;
            float gap = 6;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "+ Notes", _buttonStyle))
            {
                SpawnPanel(PanelType.Notes);
            }
            btnX += btnW + gap;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "+ Tasks", _buttonStyle))
            {
                SpawnPanel(PanelType.TaskBoard);
            }
            btnX += btnW + gap;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "+ Info", _buttonStyle))
            {
                SpawnPanel(PanelType.Information);
            }
            btnX += btnW + gap;

            if (GUI.Button(new Rect(btnX, btnY, 82, btnH), "+ 3D Model", _buttonStyle))
            {
                SpawnPanel(PanelType.ObjectViewer);
            }
            btnX += 82 + gap;

            if (GUI.Button(new Rect(btnX, btnY, 80, btnH), "+ Shortcut", _buttonStyle))
            {
                SpawnPanel(PanelType.QuickAccess);
            }
            btnX += 80 + gap + 10;

            if (GUI.Button(new Rect(btnX, btnY, 80, btnH), "Curved Arc", _buttonStyle))
            {
                WorkspaceManager.Instance?.ArrangeInCurvedArc();
                ShowStatus("Arranged panels into 3D Curved Arc!");
            }
            btnX += 80 + gap;

            if (GUI.Button(new Rect(btnX, btnY, 70, btnH), "Grid Row", _buttonStyle))
            {
                WorkspaceManager.Instance?.ArrangeInHorizontalGrid();
                ShowStatus("Arranged panels into Horizontal Grid!");
            }
            btnX += 70 + gap;

            if (GUI.Button(new Rect(btnX, btnY, 85, btnH), "Save (Ctrl+S)", _accentBtnStyle))
            {
                WorkspaceManager.Instance?.SaveActiveWorkspace();
                ShowStatus("Workspace Saved to Disk Successfully!");
            }
            btnX += 85 + gap;

            if (GUI.Button(new Rect(btnX, btnY, 55, btnH), "Help", _buttonStyle))
            {
                _showHelp = !_showHelp;
            }
            btnX += 55 + gap;

            if (GUI.Button(new Rect(btnX, btnY, 65, btnH), "Menu", _buttonStyle))
            {
                SceneNavigationManager.Instance?.ReturnToMainMenu();
            }

            // 2. Status Toast
            if (!string.IsNullOrEmpty(_statusMessage))
            {
                float toastW = 420;
                GUI.Box(new Rect((Screen.width - toastW) * 0.5f, 66, toastW, 28), _statusMessage, _labelStyle);
            }

            // 3. Selected Panel Context Bar (Bottom of screen)
            if (WorkspaceManager.Instance != null && WorkspaceManager.Instance.SelectedPanel != null)
            {
                var sel = WorkspaceManager.Instance.SelectedPanel;
                float bottomW = 600;
                float bottomX = (Screen.width - bottomW) * 0.5f;
                float bottomY = Screen.height - 52;

                GUI.Box(new Rect(bottomX, bottomY, bottomW, 44), GUIContent.none, _boxStyle);
                GUI.Label(new Rect(bottomX + 12, bottomY + 10, 240, 24), $"Selected: {sel.Title} [{sel.PanelType}]", _subLabelStyle);

                float sX = bottomX + 260;
                if (GUI.Button(new Rect(sX, bottomY + 8, 80, 28), "Duplicate", _buttonStyle))
                {
                    sel.Duplicate();
                    ShowStatus("Duplicated panel!");
                }
                sX += 85;

                if (GUI.Button(new Rect(sX, bottomY + 8, 75, 28), "Focus (F)", _buttonStyle))
                {
                    sel.FocusPanel();
                }
                sX += 80;

                if (GUI.Button(new Rect(sX, bottomY + 8, 75, 28), "Reset (R)", _buttonStyle))
                {
                    sel.ResetOrientation();
                }
                sX += 80;

                if (GUI.Button(new Rect(sX, bottomY + 8, 80, 28), "Delete", _buttonStyle))
                {
                    sel.ClosePanel();
                    ShowStatus("Deleted panel.");
                }
            }

            // 4. Help Overlay Modal
            if (_showHelp)
            {
                float modalW = 540;
                float modalH = 360;
                float mX = (Screen.width - modalW) * 0.5f;
                float mY = (Screen.height - modalH) * 0.5f;

                GUI.Box(new Rect(mX, mY, modalW, modalH), GUIContent.none, _boxStyle);
                GUI.Label(new Rect(mX + 20, mY + 15, modalW - 40, 30), "RealityOS Desktop Controls & Guide", _headerStyle);

                string helpText =
                    "CAMERA NAVIGATION:\n" +
                    "  W / A / S / D  : Fly Forward, Left, Backward, Right in 3D\n" +
                    "  Left Shift     : Hold to Sprint (2x faster movement)\n" +
                    "  Right Mouse Btn: Hold and drag to Look Around / Orbit\n" +
                    "  Mouse Wheel    : Zoom distance into or out of panels\n" +
                    "  Space / Q      : Move camera straight Up / Down\n\n" +
                    "PANEL INTERACTION:\n" +
                    "  Left Click     : Select panel (Cyan outline indicates selection)\n" +
                    "  Click & Drag   : Reposition panel smoothly across 3D plane\n" +
                    "  F              : Focus camera directly in front of selected panel\n" +
                    "  R              : Reset panel rotation to face your camera\n" +
                    "  Ctrl + D       : Duplicate selected panel\n" +
                    "  Delete         : Remove selected panel from workspace\n" +
                    "  Ctrl + S       : Save workspace layout and content to local JSON\n";

                GUI.Label(new Rect(mX + 20, mY + 50, modalW - 40, 250), helpText, _subLabelStyle);

                if (GUI.Button(new Rect(mX + (modalW - 120) * 0.5f, mY + modalH - 42, 120, 30), "Close Help", _accentBtnStyle))
                {
                    _showHelp = false;
                }
            }
        }

        private void SpawnPanel(PanelType type)
        {
            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.SpawnPanel(type);
                ShowStatus($"Spawned new {type} Panel in 3D space!");
            }
        }

        public void ShowStatus(string msg)
        {
            _statusMessage = msg;
            _statusTimer = 3.5f;
        }

        private void OnWorkspaceLoaded(Persistence.WorkspaceData data)
        {
            ShowStatus($"Workspace '{data.workspaceName}' loaded with {data.panels.Count} panels.");
        }

        private void OnWorkspaceSaved()
        {
            ShowStatus("Auto-saved workspace state.");
        }
    }
}
