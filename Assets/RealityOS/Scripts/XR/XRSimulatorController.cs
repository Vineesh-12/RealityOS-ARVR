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

        private GUIStyle _headerStyle;
        private GUIStyle _subStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _boxStyle;
        private bool _stylesInit = false;

        private void Start()
        {
            EnsureXRRig();

            // Spawn sample panels in XR space if none exist
            if (WorkspaceManager.Instance != null && WorkspaceManager.Instance.ActivePanels.Count == 0)
            {
                WorkspaceManager.Instance.ReconstructPanelsInScene();
            }
        }

        private void EnsureXRRig()
        {
            if (_xrCamera == null) _xrCamera = Camera.main;

            if (_rightHand == null)
            {
                Transform rh = transform.Find("RightHand_Simulator");
                GameObject rhObj = rh != null ? rh.gameObject : new GameObject("RightHand_Simulator");
                rhObj.transform.SetParent(transform, false);
                rhObj.transform.localPosition = new Vector3(0.2f, 1.1f, 0.4f);
                _rightHand = rhObj.GetComponent<XRSimulatedController>() ?? rhObj.AddComponent<XRSimulatedController>();
            }

            if (_locomotion == null)
            {
                _locomotion = GetComponent<XRLocomotionProvider>() ?? gameObject.AddComponent<XRLocomotionProvider>();
            }
        }

        private void InitStyles()
        {
            if (_stylesInit) return;

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _headerStyle.normal.textColor = new Color(0.0f, 0.95f, 0.9f); // Cyan

            _subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal
            };
            _subStyle.normal.textColor = Color.white;

            _btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            _btnStyle.normal.textColor = Color.white;

            _boxStyle = new GUIStyle(GUI.skin.box);
            _stylesInit = true;
        }

        private void OnGUI()
        {
            InitStyles();

            // Top Notice Bar
            float barW = Mathf.Min(Screen.width - 20, 1000);
            float startX = (Screen.width - barW) * 0.5f;

            GUI.Box(new Rect(startX, 10, barW, 46), GUIContent.none, _boxStyle);
            GUI.Label(new Rect(startX + 14, 16, 320, 26), "RealityOS | Simulated XR Mode", _headerStyle);

            string controls = "Aim: Mouse | Trigger: Left Click | Grab: Hold 'G' | Snap Turn: Q/E (45 deg) | Teleport: Hold 'T'";
            GUI.Label(new Rect(startX + 280, 18, 520, 24), controls, _subStyle);

            if (GUI.Button(new Rect(startX + barW - 100, 16, 85, 28), "Main Menu", _btnStyle))
            {
                SceneNavigationManager.Instance?.ReturnToMainMenu();
            }

            // Notice badge at bottom
            float badgeW = 680;
            float badgeX = (Screen.width - badgeW) * 0.5f;
            GUI.Box(new Rect(badgeX, Screen.height - 38, badgeW, 28), GUIContent.none, _boxStyle);
            GUI.Label(new Rect(badgeX, Screen.height - 34, badgeW, 20), "Simulated XR Environment (Testing XR Interaction Toolkit concepts without physical headset)", _subStyle);
        }
    }
}
