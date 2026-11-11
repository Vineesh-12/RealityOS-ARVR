using UnityEngine;
using TMPro;
using RealityOS.Core;
using RealityOS.Persistence;

namespace RealityOS.Panels
{
    public class NotesPanel : WorkspacePanelBase
    {
        [Header("Notes Specific UI")]
        [SerializeField] private TMP_InputField _notesInputField;
        [SerializeField] private TextMeshProUGUI _charCountText;

        [SerializeField] private string _currentContent = "";

        protected override void Awake()
        {
            _panelType = PanelType.Notes;
            base.Awake();

            if (_notesInputField != null)
            {
                _notesInputField.onValueChanged.AddListener(OnNotesTextChanged);
            }
        }

        private void OnDestroy()
        {
            if (_notesInputField != null)
            {
                _notesInputField.onValueChanged.RemoveListener(OnNotesTextChanged);
            }
        }

        private void OnNotesTextChanged(string newText)
        {
            _currentContent = newText;
            UpdateCharCount();

            if (RealityOS.Workspace.WorkspaceManager.Instance != null)
            {
                RealityOS.Workspace.WorkspaceManager.Instance.MarkDirty();
            }
        }

        private void UpdateCharCount()
        {
            if (_charCountText != null)
            {
                int len = _currentContent?.Length ?? 0;
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

