using UnityEngine;
using UnityEngine.SceneManagement;
using RealityOS.Workspace;
using RealityOS.Desktop;
using RealityOS.UI;

namespace RealityOS.Core
{
    public class ApplicationBootstrap : MonoBehaviour
    {
        [SerializeField] private bool _autoLoadMainMenu = true;
        [SerializeField] private float _splashDelay = 0.2f;

        private static bool _isInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _isInitialized = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void AutoBootstrapOnPlay()
        {
            if (_isInitialized) return;

            Debug.Log("[RealityOS] RuntimeInitializeOnLoad: Initializing Spatial Computing Environment...");
            EnsureManager<AppModeManager>();
            EnsureManager<SettingsManager>();
            EnsureManager<SceneNavigationManager>();
            EnsureManager<WorkspaceManager>();

            _isInitialized = true;

            string sceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"[RealityOS] Active Scene on Play: {sceneName}");

            // If user pressed Play directly in Bootstrap scene, transition to MainMenu or DesktopWorkspace
            if (sceneName == SceneNavigationManager.SCENE_BOOTSTRAP || string.IsNullOrEmpty(sceneName))
            {
                SceneNavigationManager.Instance.LoadScene(SceneNavigationManager.SCENE_MAIN_MENU);
            }
            else
            {
                // Ensure scene has camera, lighting, and controller components
                EnsureSceneSetup(sceneName);
            }
        }

        private void Awake()
        {
            AutoBootstrapOnPlay();
        }

        private void Start()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == SceneNavigationManager.SCENE_BOOTSTRAP && _autoLoadMainMenu)
            {
                Invoke(nameof(TransitionToMainMenu), _splashDelay);
            }
        }

        private void TransitionToMainMenu()
        {
            if (SceneNavigationManager.Instance != null)
            {
                SceneNavigationManager.Instance.LoadScene(SceneNavigationManager.SCENE_MAIN_MENU);
            }
        }

        public static T EnsureManager<T>() where T : MonoBehaviour
        {
            T existing = FindAnyObjectByType<T>();
            if (existing != null)
            {
                if (existing.transform.parent != null)
                {
                    existing.transform.SetParent(null);
                }
                DontDestroyOnLoad(existing.gameObject);
                return existing;
            }

            GameObject go = new GameObject(typeof(T).Name);
            go.transform.SetParent(null);
            T comp = go.AddComponent<T>();
            DontDestroyOnLoad(go);
            return comp;
        }

        public static void EnsureSceneSetup(string sceneName)
        {
            // Ensure Camera exists
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                camObj.transform.position = new Vector3(0f, 1.4f, 0f);
                camObj.transform.rotation = Quaternion.identity;
            }

            // Ensure Light exists
            Light light = FindAnyObjectByType<Light>();
            if (light == null)
            {
                GameObject lightObj = new GameObject("Directional Light");
                light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(0.95f, 0.96f, 1f);
                light.intensity = 1.2f;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // Ensure Scene-specific controllers
            if (sceneName == SceneNavigationManager.SCENE_MAIN_MENU)
            {
                if (FindAnyObjectByType<MainMenuUI>() == null)
                {
                    GameObject menuObj = new GameObject("MainMenu_Controller");
                    menuObj.AddComponent<MainMenuUI>();
                }
            }
            else if (sceneName == SceneNavigationManager.SCENE_DESKTOP_WORKSPACE || sceneName == SceneNavigationManager.SCENE_DEMO_WORKSPACE)
            {
                if (cam.GetComponent<DesktopCameraController>() == null)
                {
                    cam.gameObject.AddComponent<DesktopCameraController>();
                }
                if (cam.GetComponent<DesktopInteractionRaycaster>() == null)
                {
                    cam.gameObject.AddComponent<DesktopInteractionRaycaster>();
                }
                if (FindAnyObjectByType<DesktopUIOverlay>() == null)
                {
                    GameObject overlayObj = new GameObject("DesktopUIOverlay");
                    overlayObj.AddComponent<DesktopUIOverlay>();
                }
                if (FindAnyObjectByType<SpatialReferenceGrid>() == null)
                {
                    GameObject gridObj = new GameObject("SpatialGrid");
                    gridObj.AddComponent<SpatialReferenceGrid>();
                }

                // Populate demo panels if empty
                if (WorkspaceManager.Instance != null && WorkspaceManager.Instance.ActivePanels.Count == 0)
                {
                    WorkspaceManager.Instance.ReconstructPanelsInScene();
                }
            }
            else if (sceneName == SceneNavigationManager.SCENE_XR_SIMULATOR)
            {
                if (FindAnyObjectByType<RealityOS.XR.XRSimulatorController>() == null)
                {
                    GameObject xrObj = new GameObject("XRSimulator_Controller");
                    xrObj.AddComponent<RealityOS.XR.XRSimulatorController>();
                }
            }
            else if (sceneName == SceneNavigationManager.SCENE_AR_WORKSPACE)
            {
                if (FindAnyObjectByType<RealityOS.AR.ARWorkspaceController>() == null)
                {
                    GameObject arObj = new GameObject("AR_Controller");
                    arObj.AddComponent<RealityOS.AR.ARWorkspaceController>();
                    arObj.AddComponent<RealityOS.AR.ARUIOverlay>();
                }
            }
        }
    }
}
