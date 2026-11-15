using System;
using UnityEngine;
using RealityOS.Persistence;

namespace RealityOS.Core
{
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager _instance;
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null || !_instance)
                {
                    _instance = FindAnyObjectByType<SettingsManager>();
                    if (_instance == null || !_instance)
                    {
                        GameObject go = new GameObject(nameof(SettingsManager));
                        go.transform.SetParent(null);
                        _instance = go.AddComponent<SettingsManager>();
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

        [SerializeField] private AppSettingsData _currentSettings = new AppSettingsData();
        public AppSettingsData Current => _currentSettings;

        public event Action<AppSettingsData> OnSettingsChanged;

        private PersistenceService _persistence;

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

            _persistence = new PersistenceService();
            LoadSettings();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void LoadSettings()
        {
            if (_persistence == null) _persistence = new PersistenceService();
            _currentSettings = _persistence.LoadSettings();
            ApplySettings();
        }

        public void SaveSettings()
        {
            if (_persistence == null) _persistence = new PersistenceService();
            _persistence.SaveSettings(_currentSettings);
            ApplySettings();
            OnSettingsChanged?.Invoke(_currentSettings);
        }

        public void ResetToDefaults()
        {
            _currentSettings = new AppSettingsData();
            SaveSettings();
        }

        public void SetMouseSensitivity(float value)
        {
            _currentSettings.mouseSensitivity = Mathf.Clamp(value, 0.1f, 5.0f);
        }

        public void SetCameraMoveSpeed(float value)
        {
            _currentSettings.cameraMoveSpeed = Mathf.Clamp(value, 1.0f, 15.0f);
        }

        public void SetCameraLookSensitivity(float value)
        {
            _currentSettings.cameraLookSensitivity = Mathf.Clamp(value, 0.5f, 5.0f);
        }

        public void SetMasterVolume(float value)
        {
            _currentSettings.masterVolume = Mathf.Clamp01(value);
            AudioListener.volume = _currentSettings.masterVolume;
        }

        public void SetGraphicsQuality(int level)
        {
            _currentSettings.graphicsQualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(_currentSettings.graphicsQualityLevel, true);
        }

        public void SetComfortSnapTurn(bool enabled)
        {
            _currentSettings.comfortSnapTurn = enabled;
        }

        public void SetSnapTurnAngle(float angle)
        {
            _currentSettings.snapTurnAngle = angle;
        }

        public void ApplySettings()
        {
            AudioListener.volume = _currentSettings.masterVolume;
            if (_currentSettings.graphicsQualityLevel < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(_currentSettings.graphicsQualityLevel, true);
            }
        }
    }
}
