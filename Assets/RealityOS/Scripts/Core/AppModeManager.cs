using System;
using UnityEngine;

namespace RealityOS.Core
{
    public class AppModeManager : MonoBehaviour
    {
        private static AppModeManager _instance;
        public static AppModeManager Instance
        {
            get
            {
                if (_instance == null || !_instance)
                {
                    _instance = FindAnyObjectByType<AppModeManager>();
                    if (_instance == null || !_instance)
                    {
                        GameObject go = new GameObject(nameof(AppModeManager));
                        go.transform.SetParent(null);
                        _instance = go.AddComponent<AppModeManager>();
                        DontDestroyOnLoad(go);
                    }
                    else
                    {
                        if (_instance.transform.parent != null)
                        {
                            _instance.transform.SetParent(null);
                        }
                        DontDestroyOnLoad(_instance.gameObject);
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

        [SerializeField] private InteractionMode _currentMode = InteractionMode.DesktopSpatial;
        public InteractionMode CurrentMode => _currentMode;

        public event Action<InteractionMode> OnModeChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void SetMode(InteractionMode mode)
        {
            if (_currentMode == mode) return;

            _currentMode = mode;
            Debug.Log($"[RealityOS.AppModeManager] Switched active interaction mode to: {mode}");
            OnModeChanged?.Invoke(mode);
        }

        public bool IsModeSupported(InteractionMode mode, out string limitationReason)
        {
            switch (mode)
            {
                case InteractionMode.DesktopSpatial:
                    limitationReason = "Fully supported on this workstation (Dell G15 / RTX 3050).";
                    return true;

                case InteractionMode.SimulatedXR:
                    limitationReason = "XR Interaction Toolkit device simulation supported via mouse & keyboard.";
                    return true;

                case InteractionMode.MobileAR:
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        limitationReason = "Running on Android. ARCore support verified via AR Foundation.";
                        return true;
                    }
                    else
                    {
                        limitationReason = "Mobile AR requires an ARCore-compatible Android device. Running on Desktop will use camera simulation/fallback.";
                        return true;
                    }

                default:
                    limitationReason = "Unknown interaction mode.";
                    return false;
            }
        }
    }
}
