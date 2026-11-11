using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Workspace;

namespace RealityOS.Desktop
{
    public class DesktopUIOverlay : MonoBehaviour
    {
        [Header("Header Info")]
        [SerializeField] private TextMeshProUGUI _workspaceTitleText;
        [SerializeField] private TextMeshProUGUI _saveStatusText;

        [Header("Toolbox Buttons")]
        [SerializeField] private Button _addNotesButton;
        [SerializeField] private Button _addTaskBoardButton;
        [SerializeField] private Button _addInfoButton;
        [SerializeField] private Button _addObjectViewerButton;
        [SerializeField] private Button _addQuickAccessButton;

        [Header("Action Buttons")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _curvedArcLayoutButton;
        [SerializeField] private Button _gridLayoutButton;
        [SerializeField] private Button _resetCameraButton;
        [SerializeField] private Button _helpButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("Help Dialog")]
        [SerializeField] private GameObject _helpModal;
        [SerializeField] private Button _closeHelpButton;

        [Header("Camera Controller Reference")]
        [SerializeField] private DesktopCameraController _cameraController;

        private void Start()
        {
            BindButtons();
            UpdateWorkspaceTitle();

            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.OnWorkspaceLoaded += OnWorkspaceLoaded;
                WorkspaceManager.Instance.OnWorkspaceSaved += OnWorkspaceSaved;
            }
        }

        private void OnDestroy()
        {
            UnbindButtons();

            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.OnWorkspaceLoaded -= OnWorkspaceLoaded;
                WorkspaceManager.Instance.OnWorkspaceSaved -= OnWorkspaceSaved;
            }
        }

        private void BindButtons()
        {
            if (_addNotesButton != null) _addNotesButton.onClick.AddListener(() => SpawnPanel(PanelType.Notes));
            if (_addTaskBoardButton != null) _addTaskBoardButton.onClick.AddListener(() => SpawnPanel(PanelType.TaskBoard));
            if (_addInfoButton != null) _addInfoButton.onClick.AddListener(() => SpawnPanel(PanelType.Information));
            if (_addObjectViewerButton != null) _addObjectViewerButton.onClick.AddListener(() => SpawnPanel(PanelType.ObjectViewer));
            if (_addQuickAccessButton != null) _addQuickAccessButton.onClick.AddListener(() => SpawnPanel(PanelType.QuickAccess));

            if (_saveButton != null) _saveButton.onClick.AddListener(OnSaveClicked);
            if (_curvedArcLayoutButton != null) _curvedArcLayoutButton.onClick.AddListener(OnCurvedLayoutClicked);
            if (_gridLayoutButton != null) _gridLayoutButton.onClick.AddListener(OnGridLayoutClicked);
            if (_resetCameraButton != null) _resetCameraButton.onClick.AddListener(OnResetCameraClicked);
            if (_helpButton != null) _helpButton.onClick.AddListener(ToggleHelpModal);
            if (_closeHelpButton != null) _closeHelpButton.onClick.AddListener(ToggleHelpModal);
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void UnbindButtons()
        {
            if (_addNotesButton != null) _addNotesButton.onClick.RemoveAllListeners();
            if (_addTaskBoardButton != null) _addTaskBoardButton.onClick.RemoveAllListeners();
            if (_addInfoButton != null) _addInfoButton.onClick.RemoveAllListeners();
            if (_addObjectViewerButton != null) _addObjectViewerButton.onClick.RemoveAllListeners();
            if (_addQuickAccessButton != null) _addQuickAccessButton.onClick.RemoveAllListeners();

            if (_saveButton != null) _saveButton.onClick.RemoveAllListeners();
            if (_curvedArcLayoutButton != null) _curvedArcLayoutButton.onClick.RemoveAllListeners();
            if (_gridLayoutButton != null) _gridLayoutButton.onClick.RemoveAllListeners();
            if (_resetCameraButton != null) _resetCameraButton.onClick.RemoveAllListeners();
            if (_helpButton != null) _helpButton.onClick.RemoveAllListeners();
            if (_closeHelpButton != null) _closeHelpButton.onClick.RemoveAllListeners();
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveAllListeners();
        }

        private void SpawnPanel(PanelType type)
        {
            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.SpawnPanel(type);
                ShowStatus($"Added {type} Panel");
            }
        }

        private void OnSaveClicked()
        {
            if (WorkspaceManager.Instance != null)
            {
                bool success = WorkspaceManager.Instance.SaveActiveWorkspace();
                ShowStatus(success ? "Workspace Saved Successfully!" : "Save Failed.");
            }
        }

        private void OnCurvedLayoutClicked()
        {
            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.ArrangeInCurvedArc();
                ShowStatus("Arranged in 3D Curved Arc");
            }
        }

        private void OnGridLayoutClicked()
        {
            if (WorkspaceManager.Instance != null)
            {
                WorkspaceManager.Instance.ArrangeInHorizontalGrid();
                ShowStatus("Arranged in Horizontal Grid");
            }
        }

        private void OnResetCameraClicked()
        {
            if (_cameraController != null)
            {
                _cameraController.ResetCamera();
                ShowStatus("Camera Reset");
            }
        }

        public void ToggleHelpModal()
        {
            if (_helpModal != null)
            {
                _helpModal.SetActive(!_helpModal.activeSelf);
            }
        }

        private void OnMainMenuClicked()
        {
            SceneNavigationManager.Instance.ReturnToMainMenu();
        }

        private void OnWorkspaceLoaded(Persistence.WorkspaceData data)
        {
            UpdateWorkspaceTitle();
            ShowStatus($"Loaded '{data.workspaceName}'");
        }

        private void OnWorkspaceSaved()
        {
            ShowStatus("Auto-saved Workspace");
        }

        private void UpdateWorkspaceTitle()
        {
            if (_workspaceTitleText != null && WorkspaceManager.Instance != null && WorkspaceManager.Instance.ActiveWorkspace != null)
            {
                _workspaceTitleText.text = $"Workspace: {WorkspaceManager.Instance.ActiveWorkspace.workspaceName}";
            }
        }

        private void ShowStatus(string msg)
        {
            if (_saveStatusText != null)
            {
                _saveStatusText.text = msg;
                CancelInvoke(nameof(ClearStatus));
                Invoke(nameof(ClearStatus), 3.0f);
            }
        }

        private void ClearStatus()
        {
            if (_saveStatusText != null)
            {
                _saveStatusText.text = "";
            }
        }
    }
}
