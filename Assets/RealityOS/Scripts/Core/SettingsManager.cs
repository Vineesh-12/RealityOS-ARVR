using System;
using UnityEngine;
using RealityOS.Persistence;

namespace RealityOS.Core
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        [SerializeField] private AppSettingsData _currentSettings = new AppSettingsData();
        public AppSettingsData Current => _currentSettings;

        public event Action<AppSettingsData> OnSettingsChanged;

        private PersistenceService _persistence;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _persistence = new PersistenceService();
            LoadSettings();
        }

        public void LoadSettings()
        {
            _currentSettings = _persistence.LoadSettings();
            ApplySettings();
        }

        public void SaveSettings()
        {
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

