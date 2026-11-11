using System;
using System.Collections.Generic;
using UnityEngine;
using RealityOS.Core;
using RealityOS.Persistence;

namespace RealityOS.Workspace
{
    public class WorkspaceManager : MonoBehaviour
    {
        public static WorkspaceManager Instance { get; private set; }

        [Header("Persistence")]
        private PersistenceService _persistence;
        public IWorkspacePersistence Persistence => _persistence;

        [Header("Active Workspace State")]
        [SerializeField] private WorkspaceData _activeWorkspace;
        public WorkspaceData ActiveWorkspace => _activeWorkspace;

        private readonly List<IWorkspacePanel> _activePanels = new List<IWorkspacePanel>();
        public IReadOnlyList<IWorkspacePanel> ActivePanels => _activePanels;

        [Header("Prefab References")]
        [SerializeField] private GameObject _notesPanelPrefab;
        [SerializeField] private GameObject _taskBoardPanelPrefab;
        [SerializeField] private GameObject _informationPanelPrefab;
        [SerializeField] private GameObject _objectViewerPanelPrefab;
        [SerializeField] private GameObject _quickAccessPanelPrefab;

        [Header("Runtime Anchors")]
        [SerializeField] private Transform _panelsParent;

        public event Action<WorkspaceData> OnWorkspaceLoaded;
        public event Action OnWorkspaceSaved;
        public event Action<IWorkspacePanel> OnPanelAdded;
        public event Action<IWorkspacePanel> OnPanelRemoved;
        public event Action<IWorkspacePanel> OnPanelSelected;

        public IWorkspacePanel SelectedPanel { get; private set; }

        private float _autoSaveTimer = 0f;
        private const float AUTO_SAVE_INTERVAL = 60f;
        private bool _isDirty = false;

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

            if (_activeWorkspace == null)
            {
                // Load first workspace or generate sample
                List<WorkspaceData> all = _persistence.GetAllWorkspaces();
                if (all != null && all.Count > 0)
                {
                    _activeWorkspace = all[0];
                }
                else
                {
                    _activeWorkspace = PersistenceService.CreateSampleDemonstrationWorkspace();
                    _persistence.SaveWorkspace(_activeWorkspace);
                }
            }
        }

        private void Update()
        {
            if (_isDirty)
            {
                _autoSaveTimer += Time.deltaTime;
                if (_autoSaveTimer >= AUTO_SAVE_INTERVAL)
                {
                    SaveActiveWorkspace();
                    _autoSaveTimer = 0f;
                }
            }
        }

        public void SetPanelsParent(Transform parent)
        {
            _panelsParent = parent;
        }

        public void SetPanelPrefabs(GameObject notes, GameObject tasks, GameObject info, GameObject viewer, GameObject quick)
        {
            _notesPanelPrefab = notes;
            _taskBoardPanelPrefab = tasks;
            _informationPanelPrefab = info;
            _objectViewerPanelPrefab = viewer;
            _quickAccessPanelPrefab = quick;
        }

        public void CreateNewWorkspace(string name, string description = "")
        {
            ClearPanelsInScene();
            _activeWorkspace = new WorkspaceData(name, description);
            _persistence.SaveWorkspace(_activeWorkspace);
            _isDirty = false;
            OnWorkspaceLoaded?.Invoke(_activeWorkspace);
        }

        public bool LoadWorkspaceById(string id)
        {
            WorkspaceData ws = _persistence.LoadWorkspace(id);
            if (ws != null)
            {
                _activeWorkspace = ws;
                _isDirty = false;
                ReconstructPanelsInScene();
                OnWorkspaceLoaded?.Invoke(_activeWorkspace);
                return true;
            }
            return false;
        }

        public bool SaveActiveWorkspace()
        {
            if (_activeWorkspace == null) return false;

            // Collect current states from active panels
            _activeWorkspace.panels.Clear();
            foreach (var panel in _activePanels)
            {
                _activeWorkspace.panels.Add(panel.ExportData());
            }

            bool success = _persistence.SaveWorkspace(_activeWorkspace);
            if (success)
            {
                _isDirty = false;
                _autoSaveTimer = 0f;
                OnWorkspaceSaved?.Invoke();
            }
            return success;
        }

        public bool DeleteWorkspace(string id)
        {
            bool deleted = _persistence.DeleteWorkspace(id);
            if (deleted && _activeWorkspace != null && _activeWorkspace.workspaceId == id)
            {
                List<WorkspaceData> remaining = _persistence.GetAllWorkspaces();
                if (remaining.Count > 0)
                {
                    LoadWorkspaceById(remaining[0].workspaceId);
                }
                else
                {
                    CreateNewWorkspace("Default Workspace");
                }
            }
            return deleted;
        }

        public void RenameActiveWorkspace(string newName)
        {
            if (_activeWorkspace != null && !string.IsNullOrWhiteSpace(newName))
            {
                _activeWorkspace.workspaceName = newName.Trim();
                MarkDirty();
                SaveActiveWorkspace();
            }
        }

        public void MarkDirty()
        {
            _isDirty = true;
        }

        public void RegisterPanel(IWorkspacePanel panel)
        {
            if (panel != null && !_activePanels.Contains(panel))
            {
                _activePanels.Add(panel);
                MarkDirty();
                OnPanelAdded?.Invoke(panel);
            }
        }

        public void UnregisterPanel(IWorkspacePanel panel)
        {
            if (panel != null && _activePanels.Contains(panel))
            {
                if (SelectedPanel == panel)
                {
                    SelectPanel(null);
                }
                _activePanels.Remove(panel);
                MarkDirty();
                OnPanelRemoved?.Invoke(panel);
            }
        }

        public void SelectPanel(IWorkspacePanel panel)
        {
            if (SelectedPanel == panel) return;

            if (SelectedPanel != null)
            {
                SelectedPanel.OnDeselect();
            }

            SelectedPanel = panel;

            if (SelectedPanel != null)
            {
                SelectedPanel.OnSelect();
            }

            OnPanelSelected?.Invoke(SelectedPanel);
        }

        public GameObject SpawnPanel(PanelType type, Vector3? spawnPosition = null)
        {
            GameObject prefab = GetPrefabForType(type);
            Vector3 pos = spawnPosition ?? GetDefaultSpawnPosition();

            GameObject instance;
            Transform parent = _panelsParent;
            if (prefab != null)
            {
                instance = Instantiate(prefab, pos, Quaternion.identity, parent);
            }
            else
            {
                // Fallback procedural panel creation if prefab reference is not yet wired
                instance = CreateProceduralPanelFallback(type, pos, parent);
            }

            IWorkspacePanel panel = instance.GetComponent<IWorkspacePanel>();
            if (panel != null)
            {
                panel.Title = $"{type} Panel";
                RegisterPanel(panel);
                SelectPanel(panel);
            }

            MarkDirty();
            return instance;
        }

        public void DuplicatePanel(IWorkspacePanel panel)
        {
            if (panel == null) return;

            PanelData data = panel.ExportData();
            PanelData copyData = data.Clone();

            GameObject prefab = GetPrefabForType(copyData.panelType);
            GameObject instance;
            Transform parent = _panelsParent;

            if (prefab != null)
            {
                instance = Instantiate(prefab, copyData.position.ToVector3(), copyData.rotation.ToQuaternion(), parent);
            }
            else
            {
                instance = CreateProceduralPanelFallback(copyData.panelType, copyData.position.ToVector3(), parent);
            }

            IWorkspacePanel copyPanel = instance.GetComponent<IWorkspacePanel>();
            if (copyPanel != null)
            {
                copyPanel.ImportData(copyData);
                RegisterPanel(copyPanel);
                SelectPanel(copyPanel);
            }
            MarkDirty();
        }

        public void DeletePanel(IWorkspacePanel panel)
        {
            if (panel == null) return;

            UnregisterPanel(panel);
            if (panel is MonoBehaviour mb)
            {
                Destroy(mb.gameObject);
            }
            MarkDirty();
        }

        public void ReconstructPanelsInScene()
        {
            ClearPanelsInScene();

            if (_activeWorkspace == null || _activeWorkspace.panels == null) return;

            foreach (var panelData in _activeWorkspace.panels)
            {
                GameObject prefab = GetPrefabForType(panelData.panelType);
                GameObject instance;
                Transform parent = _panelsParent;

                Vector3 pos = panelData.position.ToVector3();
                Quaternion rot = panelData.rotation.ToQuaternion();

                if (prefab != null)
                {
                    instance = Instantiate(prefab, pos, rot, parent);
                }
                else
                {
                    instance = CreateProceduralPanelFallback(panelData.panelType, pos, parent);
                }

                instance.transform.localScale = panelData.scale.ToVector3();

                IWorkspacePanel panel = instance.GetComponent<IWorkspacePanel>();
                if (panel != null)
                {
                    panel.ImportData(panelData);
                    RegisterPanel(panel);
                }
            }
        }

        public void ClearPanelsInScene()
        {
            var copy = new List<IWorkspacePanel>(_activePanels);
            foreach (var p in copy)
            {
                if (p is MonoBehaviour mb && mb != null)
                {
                    Destroy(mb.gameObject);
                }
            }
            _activePanels.Clear();
            SelectedPanel = null;
        }

        public void ArrangeInCurvedArc(float radius = 2.2f)
        {
            if (_activePanels.Count == 0) return;

            int count = _activePanels.Count;
            float totalArc = Mathf.Min(120f, count * 30f);
            float startAngle = -totalArc * 0.5f;
            float step = count > 1 ? totalArc / (count - 1) : 0f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + i * step;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(rad) * radius, 1.3f, Mathf.Cos(rad) * radius);
                Quaternion rot = Quaternion.Euler(0f, angle, 0f);

                _activePanels[i].TransformTarget.position = pos;
                _activePanels[i].TransformTarget.rotation = rot;
            }
            MarkDirty();
        }

        public void ArrangeInHorizontalGrid(float spacingX = 0.9f)
        {
            if (_activePanels.Count == 0) return;

            int count = _activePanels.Count;
            float startX = -((count - 1) * spacingX) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = new Vector3(startX + i * spacingX, 1.3f, 2.0f);
                _activePanels[i].TransformTarget.position = pos;
                _activePanels[i].TransformTarget.rotation = Quaternion.identity;
            }
            MarkDirty();
        }

        private GameObject GetPrefabForType(PanelType type)
        {
            switch (type)
            {
                case PanelType.Notes: return _notesPanelPrefab;
                case PanelType.TaskBoard: return _taskBoardPanelPrefab;
                case PanelType.Information: return _informationPanelPrefab;
                case PanelType.ObjectViewer: return _objectViewerPanelPrefab;
                case PanelType.QuickAccess: return _quickAccessPanelPrefab;
                default: return null;
            }
        }

        private Vector3 GetDefaultSpawnPosition()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                return cam.transform.position + cam.transform.forward * 1.8f;
            }
            return new Vector3(0f, 1.3f, 2.0f);
        }

        private GameObject CreateProceduralPanelFallback(PanelType type, Vector3 pos, Transform parent)
        {
            GameObject go = new GameObject($"{type}Panel_Instance");
            go.transform.position = pos;
            go.transform.SetParent(parent, true);

            // Add collider for spatial raycasting
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(0.8f, 0.6f, 0.05f);

            // Add panel script based on type
            switch (type)
            {
                case PanelType.Notes:
                    go.AddComponent<RealityOS.Panels.NotesPanel>();
                    break;
                case PanelType.TaskBoard:
                    go.AddComponent<RealityOS.Panels.TaskBoardPanel>();
                    break;
                case PanelType.Information:
                    go.AddComponent<RealityOS.Panels.InformationPanel>();
                    break;
                case PanelType.ObjectViewer:
                    go.AddComponent<RealityOS.Panels.ObjectViewerPanel>();
                    break;
                case PanelType.QuickAccess:
                    go.AddComponent<RealityOS.Panels.QuickAccessPanel>();
                    break;
            }

            return go;
        }
    }
}
