using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Workspace;

namespace RealityOS.AR
{
    public class ARUIOverlay : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private ARWorkspaceController _arController;

        [Header("UI Controls")]
        [SerializeField] private Button _placePanelButton;
        [SerializeField] private Button _clearAllButton;
        [SerializeField] private Button _togglePlanesButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private TMP_Dropdown _panelTypeDropdown;
        [SerializeField] private TextMeshProUGUI _statusText;

        private bool _planesVisible = true;

        private void Start()
        {
            if (_arController == null)
            {
                _arController = FindAnyObjectByType<ARWorkspaceController>();
            }

            BindButtons();
            PopulateDropdown();
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        private void BindButtons()
        {
            if (_placePanelButton != null) _placePanelButton.onClick.AddListener(OnPlaceClicked);
            if (_clearAllButton != null) _clearAllButton.onClick.AddListener(OnClearClicked);
            if (_togglePlanesButton != null) _togglePlanesButton.onClick.AddListener(OnTogglePlanesClicked);
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            if (_panelTypeDropdown != null) _panelTypeDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        private void UnbindButtons()
        {
            if (_placePanelButton != null) _placePanelButton.onClick.RemoveAllListeners();
            if (_clearAllButton != null) _clearAllButton.onClick.RemoveAllListeners();
            if (_togglePlanesButton != null) _togglePlanesButton.onClick.RemoveAllListeners();
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveAllListeners();
            if (_panelTypeDropdown != null) _panelTypeDropdown.onValueChanged.RemoveAllListeners();
        }

        private void PopulateDropdown()
        {
            if (_panelTypeDropdown == null) return;

            _panelTypeDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>
            {
                "Notes Panel",
                "Task Board",
                "Information",
                "3D Object Viewer",
                "Quick Access"
            };
            _panelTypeDropdown.AddOptions(options);
        }

        private void OnDropdownChanged(int index)
        {
            PanelType selected = (PanelType)index;
            if (_arController != null)
            {
                _arController.SetSelectedPanelType(selected);
            }
        }

        private void OnPlaceClicked()
        {
            if (_arController != null)
            {
                _arController.PlaceSelectedPanelAtReticle();
            }
        }

        private void OnClearClicked()
        {
            if (_arController != null)
            {
                _arController.ClearAllARPanels();
            }
        }

        private void OnTogglePlanesClicked()
        {
            _planesVisible = !_planesVisible;
            if (_arController != null)
            {
                _arController.TogglePlaneVisualization(_planesVisible);
            }
            if (_statusText != null)
            {
                _statusText.text = _planesVisible ? "Planes Visible" : "Planes Hidden";
            }
        }

        private void OnMainMenuClicked()
        {
            SceneNavigationManager.Instance.ReturnToMainMenu();
        }
    }
}
