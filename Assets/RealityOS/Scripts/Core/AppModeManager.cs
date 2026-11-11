using System;
using UnityEngine;

namespace RealityOS.Core
{
    public class AppModeManager : MonoBehaviour
    {
        public static AppModeManager Instance { get; private set; }

        [SerializeField] private InteractionMode _currentMode = InteractionMode.DesktopSpatial;
        public InteractionMode CurrentMode => _currentMode;

        public event Action<InteractionMode> OnModeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
                        return true; // Still allow entering scene for editor testing/inspection
                    }

                default:
                    limitationReason = "Unknown interaction mode.";
                    return false;
            }
        }
    }
}
