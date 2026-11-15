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

        [Header("Camera Look Settings")]
        [SerializeField] private float _lookSensitivity = 2.0f;
        private float _yaw = 0f;
        private float _pitch = 0f;
        private bool _isLooking = false;

        private GUIStyle _headerStyle;
        private GUIStyle _subStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _boxStyle;
        private bool _stylesInit = false;

        private void Start()
        {
            EnsureXRRig();

            if (WorkspaceManager.Instance != null && WorkspaceManager.Instance.ActivePanels.Count == 0)
            {
                WorkspaceManager.Instance.ReconstructPanelsInScene();
            }
        }

        private void Update()
        {
            HandleCameraLook();
        }

        private void HandleCameraLook()
        {
            if (_xrCamera == null) _xrCamera = Camera.main;
            if (_xrCamera == null) return;

            if (Input.GetMouseButtonDown(1))
            {
                _isLooking = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                _isLooking = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (_isLooking)
            {
                float mx = Input.GetAxis("Mouse X") * _lookSensitivity;
                float my = Input.GetAxis("Mouse Y") * _lookSensitivity;

                _yaw += mx;
                _pitch -= my;
                _pitch = Mathf.Clamp(_pitch, -75f, 75f);

                _xrCamera.transform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                _isLooking = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void EnsureXRRig()
        {
            if (_xrCamera == null) _xrCamera = Camera.main;

            if (_leftHand == null)
            {
                Transform lh = transform.Find("LeftHand_Simulator");
                GameObject lhObj = lh != null ? lh.gameObject : new GameObject("LeftHand_Simulator");
                lhObj.transform.SetParent(transform, false);
                _leftHand = lhObj.GetComponent<XRSimulatedController>() ?? lhObj.AddComponent<XRSimulatedController>();
                _leftHand.SetHandType(XRSimulatedController.HandType.Left);
            }

            if (_rightHand == null)
            {
                Transform rh = transform.Find("RightHand_Simulator");
                GameObject rhObj = rh != null ? rh.gameObject : new GameObject("RightHand_Simulator");
                rhObj.transform.SetParent(transform, false);
                _rightHand = rhObj.GetComponent<XRSimulatedController>() ?? rhObj.AddComponent<XRSimulatedController>();
                _rightHand.SetHandType(XRSimulatedController.HandType.Right);
            }

            if (_locomotion == null)
            {
                _locomotion = GetComponent<XRLocomotionProvider>() ?? gameObject.AddComponent<XRLocomotionProvider>();
            }

            // Ensure floor grid in XR scene
            if (FindAnyObjectByType<SpatialReferenceGrid>() == null)
            {
                GameObject gridObj = new GameObject("SpatialGrid");
                gridObj.AddComponent<SpatialReferenceGrid>();
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
            GUI.Label(new Rect(startX + 14, 16, 280, 26), "RealityOS | Simulated XR Mode", _headerStyle);

            string controls = "Aim: Mouse | Trigger: Left Click | Grab: Hold 'G' | Snap Turn: Q/E (45 deg) | Teleport: Hold 'T'";
            GUI.Label(new Rect(startX + 270, 18, 560, 24), controls, _subStyle);

            if (GUI.Button(new Rect(startX + barW - 95, 12, 85, 30), "Main Menu", _btnStyle))
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
