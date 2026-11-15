using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Persistence;

namespace RealityOS.Panels
{
    /// <summary>
    /// Interactive Notes Panel supporting real-time in-scene typing, character counting,
    /// and two-way synchronization with physical .txt documents and Windows Notepad.
    /// </summary>
    public class NotesPanel : WorkspacePanelBase
    {
        [Header("Notes Specific UI")]
        [SerializeField] private TMP_InputField _notesInputField;
        [SerializeField] private TextMeshProUGUI _charCountText;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _syncNotepadButton;

        [SerializeField] private string _currentContent = "";

        private GUIStyle _editorBoxStyle;
        private GUIStyle _textAreaStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _statusStyle;
        private bool _guiStylesInit = false;

        public string CurrentContent => _currentContent;

        protected override void Awake()
        {
            _panelType = PanelType.Notes;
            base.Awake();

            EnsureWorldSpaceUI();

            if (_notesInputField != null)
            {
                _notesInputField.onValueChanged.AddListener(OnNotesTextChanged);
            }
            if (_clearButton != null)
            {
                _clearButton.onClick.AddListener(ClearNotes);
            }
            if (_syncNotepadButton != null)
            {
                _syncNotepadButton.onClick.AddListener(OpenInNotepad);
            }
        }

        private void OnDestroy()
        {
            if (_notesInputField != null)
            {
                _notesInputField.onValueChanged.RemoveListener(OnNotesTextChanged);
            }
        }

        private void EnsureWorldSpaceUI()
        {
            if (_notesInputField != null) return;

            // Check if World Space Canvas already exists
            Transform canvasTrans = transform.Find("NotesCanvas");
            if (canvasTrans == null)
            {
                GameObject cObj = new GameObject("NotesCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                cObj.transform.SetParent(transform, false);
                cObj.transform.localPosition = new Vector3(0f, 0f, -0.025f);
                cObj.transform.localScale = Vector3.one * 0.001f;

                Canvas c = cObj.GetComponent<Canvas>();
                if (c == null) c = cObj.AddComponent<Canvas>();
                c.renderMode = RenderMode.WorldSpace;
                var scaler = cObj.GetComponent<CanvasScaler>();
                if (scaler != null) scaler.dynamicPixelsPerUnit = 2;

                RectTransform rt = cObj.GetComponent<RectTransform>();
                if (rt != null) rt.sizeDelta = new Vector2(850, 600);
            }
        }

        private void OnNotesTextChanged(string newText)
        {
            _currentContent = newText ?? "";
            SetWorldBodyText(_currentContent);
            UpdateCharCount();

            if (RealityOS.Workspace.WorkspaceManager.Instance != null)
            {
                RealityOS.Workspace.WorkspaceManager.Instance.MarkDirty();
            }
        }

        private void UpdateCharCount()
        {
            int len = _currentContent?.Length ?? 0;
            if (_charCountText != null)
            {
                _charCountText.text = $"{len} chars";
            }
        }

        public void SetContent(string text)
        {
            _currentContent = text ?? "";
            if (_notesInputField != null)
            {
                _notesInputField.text = _currentContent;
            }
            SetWorldBodyText(_currentContent);
            UpdateCharCount();
        }

        public void ClearNotes()
        {
            SetContent("");
        }

        public void OpenInNotepad()
        {
            RealityOS.LaptopIntegration.NotesFileManager.Instance?.OpenNotesPanelInNotepad(this);
        }

        private void InitGuiStyles()
        {
            if (_guiStylesInit) return;

            _editorBoxStyle = new GUIStyle(GUI.skin.box);

            _textAreaStyle = new GUIStyle(GUI.skin.textArea)
            {
                fontSize = 13,
                wordWrap = true
            };
            _textAreaStyle.normal.textColor = new Color(0.95f, 0.96f, 0.98f);

            _btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            _btnStyle.normal.textColor = Color.white;

            _statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft
            };
            _statusStyle.normal.textColor = new Color(0.75f, 0.85f, 0.9f);

            _guiStylesInit = true;
        }

        private void OnGUI()
        {
            // When this note panel is actively selected, show a direct interactive editor dock
            if (!IsSelected) return;

            InitGuiStyles();

            float w = 360;
            float h = 280;
            float x = Screen.width - w - 20;
            float y = Screen.height - h - 45;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none, _editorBoxStyle);

            GUI.Label(new Rect(x + 15, y + 10, w - 30, 22), $"📝 Edit: {_title}", _statusStyle);

            // Multiline Editor Text Area
            string updated = GUI.TextArea(new Rect(x + 15, y + 35, w - 30, h - 85), _currentContent, _textAreaStyle);
            if (updated != _currentContent)
            {
                SetContent(updated);
                if (RealityOS.Workspace.WorkspaceManager.Instance != null)
                {
                    RealityOS.Workspace.WorkspaceManager.Instance.MarkDirty();
                }
            }

            // Bottom Buttons
            int charCount = _currentContent?.Length ?? 0;
            GUI.Label(new Rect(x + 15, y + h - 42, 100, 30), $"{charCount} chars", _statusStyle);

            if (GUI.Button(new Rect(x + 120, y + h - 42, 105, 30), "Open in Notepad", _btnStyle))
            {
                OpenInNotepad();
            }

            if (GUI.Button(new Rect(x + 235, y + h - 42, 110, 30), "Clear Notes", _btnStyle))
            {
                ClearNotes();
            }
        }

        public override PanelData ExportData()
        {
            PanelData data = base.ExportData();
            data.notesContent = _currentContent;
            return data;
        }

        public override void ImportData(PanelData data)
        {
            base.ImportData(data);
            if (data != null)
            {
                SetContent(data.notesContent);
            }
        }
    }
}
