using UnityEngine;
using RealityOS.Workspace;

namespace RealityOS.Core
{
    public class ApplicationBootstrap : MonoBehaviour
    {
        [SerializeField] private bool _autoLoadMainMenu = true;
        [SerializeField] private float _splashDelay = 0.5f;

        private void Awake()
        {
            Debug.Log("[RealityOS] Bootstrapping Spatial Computing Environment...");

            EnsureManager<AppModeManager>();
            EnsureManager<SettingsManager>();
            EnsureManager<SceneNavigationManager>();
            EnsureManager<WorkspaceManager>();
        }

        private void Start()
        {
            if (_autoLoadMainMenu)
            {
                Invoke(nameof(TransitionToMainMenu), _splashDelay);
            }
        }

        private void TransitionToMainMenu()
        {
            SceneNavigationManager.Instance.LoadScene(SceneNavigationManager.SCENE_MAIN_MENU);
        }

        private T EnsureManager<T>() where T : MonoBehaviour
        {
            T existing = FindAnyObjectByType<T>();
            if (existing != null) return existing;

            GameObject go = new GameObject(typeof(T).Name);
            T comp = go.AddComponent<T>();
            DontDestroyOnLoad(go);
            return comp;
        }
    }
}
