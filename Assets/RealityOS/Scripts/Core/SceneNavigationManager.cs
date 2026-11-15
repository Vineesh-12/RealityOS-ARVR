using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using RealityOS.Core;

namespace RealityOS.Core
{
    public class SceneNavigationManager : MonoBehaviour
    {
        private static SceneNavigationManager _instance;
        public static SceneNavigationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<SceneNavigationManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(nameof(SceneNavigationManager));
                        _instance = go.AddComponent<SceneNavigationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        public const string SCENE_BOOTSTRAP = "Bootstrap";
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_DESKTOP_WORKSPACE = "DesktopWorkspace";
        public const string SCENE_AR_WORKSPACE = "ARWorkspace";
        public const string SCENE_XR_SIMULATOR = "XRSimulator";
        public const string SCENE_DEMO_WORKSPACE = "DemoWorkspace";

        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;

        public bool IsLoading { get; private set; }
        public float LoadingProgress { get; private set; }
        public string CurrentSceneName => SceneManager.GetActiveScene().name;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void LoadScene(string sceneName, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            if (IsLoading)
            {
                Debug.LogWarning($"[RealityOS.SceneNavigation] Already loading a scene. Ignored request for {sceneName}");
                return;
            }

            if (gameObject != null && gameObject.activeInHierarchy)
            {
                StartCoroutine(LoadSceneRoutine(sceneName, onComplete));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
                ApplicationBootstrap.EnsureSceneSetup(sceneName);
                onComplete?.Invoke();
            }
        }

        private IEnumerator LoadSceneRoutine(string sceneName, Action onComplete)
        {
            IsLoading = true;
            LoadingProgress = 0f;
            OnSceneLoadStarted?.Invoke(sceneName);
            Debug.Log($"[RealityOS.SceneNavigation] Transitioning to scene: {sceneName}");

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOp == null)
            {
                Debug.LogError($"[RealityOS.SceneNavigation] Scene '{sceneName}' could not be loaded. Ensure it is added to Build Settings.");
                IsLoading = false;
                yield break;
            }

            while (!asyncOp.isDone)
            {
                LoadingProgress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                yield return null;
            }

            LoadingProgress = 1f;
            IsLoading = false;
            ApplicationBootstrap.EnsureSceneSetup(sceneName);
            OnSceneLoadCompleted?.Invoke(sceneName);
            onComplete?.Invoke();
            Debug.Log($"[RealityOS.SceneNavigation] Finished loading scene: {sceneName}");
        }

        public void ReturnToMainMenu()
        {
            LoadScene(SCENE_MAIN_MENU);
        }

        public void LoadModeScene(InteractionMode mode)
        {
            switch (mode)
            {
                case InteractionMode.DesktopSpatial:
                    LoadScene(SCENE_DESKTOP_WORKSPACE);
                    break;
                case InteractionMode.MobileAR:
                    LoadScene(SCENE_AR_WORKSPACE);
                    break;
                case InteractionMode.SimulatedXR:
                    LoadScene(SCENE_XR_SIMULATOR);
                    break;
            }
        }
    }
}
