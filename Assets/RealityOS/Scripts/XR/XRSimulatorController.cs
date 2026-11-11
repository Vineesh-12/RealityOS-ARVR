using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Workspace;

namespace RealityOS.XR
{
    public class XRSimulatorController : MonoBehaviour
    {
        [Header("XR Rig Elements")]
        [SerializeField] private Transform _xrOrigin;
        [SerializeField] private Camera _xrCamera;
        [SerializeField] private XRSimulatedController _leftHand;
        [SerializeField] private XRSimulatedController _rightHand;
        [SerializeField] private XRLocomotionProvider _locomotion;

        [Header("UI Overlay")]
        [SerializeField] private GameObject _controlsGuideModal;
        [SerializeField] private Button _toggleGuideButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private TextMeshProUGUI _modeNoticeText;

        private void Start()
        {
            if (_modeNoticeText != null)
            {
                _modeNoticeText.text = "Simulated XR Mode - Testing XR Interaction Toolkit via Mouse & Keyboard Simulation (No physical headset required)";
            }

            if (_toggleGuideButton != null)
            {
                _toggleGuideButton.onClick.AddListener(ToggleGuide);
            }
            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            // Spawn sample panels in XR space if none exist
            if (WorkspaceManager.Instance != null && WorkspaceManager.Instance.ActivePanels.Count == 0)
            {
                WorkspaceManager.Instance.ReconstructPanelsInScene();
            }
        }

        private void OnDestroy()
        {
            if (_toggleGuideButton != null) _toggleGuideButton.onClick.RemoveAllListeners();
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveAllListeners();
        }

        public void ToggleGuide()
        {
            if (_controlsGuideModal != null)
            {
                _controlsGuideModal.SetActive(!_controlsGuideModal.activeSelf);
            }
        }

        private void OnMainMenuClicked()
        {
            SceneNavigationManager.Instance.ReturnToMainMenu();
        }
    }
}
