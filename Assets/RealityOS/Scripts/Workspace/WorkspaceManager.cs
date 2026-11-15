using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RealityOS.Core;
using RealityOS.Persistence;

namespace RealityOS.Workspace
{
    public class WorkspaceManager : MonoBehaviour
    {
        private static WorkspaceManager _instance;
        public static WorkspaceManager Instance
        {
            get
            {
                if (_instance == null || !_instance)
                {
                    _instance = FindAnyObjectByType<WorkspaceManager>();
                    if (_instance == null || !_instance)
                    {
                        GameObject go = new GameObject(nameof(WorkspaceManager));
                        go.transform.SetParent(null);
                        _instance = go.AddComponent<WorkspaceManager>();
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

        [Header("Persistence")]
        private PersistenceService _persistence;
        public IWorkspacePersistence Persistence
        {
            get
            {
                if (_persistence == null) _persistence = new PersistenceService();
                return _persistence;
            }
        }

        [Header("Active Workspace State")]
        [SerializeField] private WorkspaceData _activeWorkspace;
        public WorkspaceData ActiveWorkspace => _activeWorkspace;

        private readonly List<IWorkspacePanel> _activePanels = new List<IWorkspacePanel>();
        public IReadOnlyList<IWorkspacePanel> ActivePanels
        {
            get
            {
                PruneDeadPanels();
                return _activePanels;
            }
        }

        [Header("Prefab References")]
        [SerializeField] private GameObject _notesPanelPrefab;
        [SerializeField] private GameObject _taskBoardPanelPrefab;
        [SerializeField] private GameObject _informationPanelPrefab;
        [SerializeField] private GameObject _objectViewerPanelPrefab;
        [SerializeField] private GameObject _quickAccessPanelPrefab;
        [SerializeField] private GameObject _laptopConnectPanelPrefab;
        [SerializeField] private GameObject _browserPanelPrefab;

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

            if (_persistence == null) _persistence = new PersistenceService();

            if (_activeWorkspace == null)
            {
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

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PruneDeadPanels();

            if (scene.name == SceneNavigationManager.SCENE_DESKTOP_WORKSPACE ||
                scene.name == SceneNavigationManager.SCENE_DEMO_WORKSPACE ||
                scene.name == SceneNavigationManager.SCENE_XR_SIMULATOR)
            {
                if (_activePanels.Count == 0)
                {
                    ReconstructPanelsInScene();
                }
            }
            else if (scene.name == SceneNavigationManager.SCENE_MAIN_MENU)
            {
                _activePanels.Clear();
                SelectedPanel = null;
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

        public void PruneDeadPanels()
        {
            _activePanels.RemoveAll(p => p == null || (p is MonoBehaviour mb && mb == null));
            if (SelectedPanel != null && SelectedPanel is MonoBehaviour selMb && selMb == null)
            {
                SelectedPanel = null;
            }
        }

        public void SetPanelsParent(Transform parent)
        {
            _panelsParent = parent;
        }

        public void CreateNewWorkspace(string name, string description = "")
        {
            ClearPanelsInScene();
            _activeWorkspace = new WorkspaceData(name, description);
            Persistence.SaveWorkspace(_activeWorkspace);
            _isDirty = false;
            OnWorkspaceLoaded?.Invoke(_activeWorkspace);
        }

        public bool LoadWorkspaceById(string id)
        {
            WorkspaceData ws = Persistence.LoadWorkspace(id);
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

            PruneDeadPanels();

            _activeWorkspace.panels.Clear();
            foreach (var panel in _activePanels)
            {
                if (panel != null)
                {
                    _activeWorkspace.panels.Add(panel.ExportData());
                }
            }

            bool success = Persistence.SaveWorkspace(_activeWorkspace);
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
            bool deleted = Persistence.DeleteWorkspace(id);
            if (deleted && _activeWorkspace != null && _activeWorkspace.workspaceId == id)
            {
                List<WorkspaceData> remaining = Persistence.GetAllWorkspaces();
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
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>($"Panels/{type}Panel");
            }
            Vector3 pos = spawnPosition ?? GetDefaultSpawnPosition();

            GameObject instance;
            Transform parent = _panelsParent;
            if (prefab != null)
            {
                instance = Instantiate(prefab, pos, Quaternion.identity, parent);
            }
            else
            {
                instance = CreateProceduralPanelFallback(type, pos, parent);
            }

            IWorkspacePanel panel = instance.GetComponent<IWorkspacePanel>();
            if (panel != null)
            {
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
            if (panel is MonoBehaviour mb && mb != null)
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
            PruneDeadPanels();
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
            PruneDeadPanels();
            if (_activePanels.Count == 0) return;

            int count = _activePanels.Count;
            float totalArc = Mathf.Min(120f, count * 30f);
            float startAngle = -totalArc * 0.5f;
            float step = count > 1 ? totalArc / (count - 1) : 0f;

            for (int i = 0; i < count; i++)
            {
                if (_activePanels[i] == null) continue;

                float angle = startAngle + i * step;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(rad) * radius, 1.3f, Mathf.Cos(rad) * radius);
                Quaternion rot = Quaternion.Euler(0f, angle, 0f);

                _activePanels[i].TransformTarget.position = pos;
                _activePanels[i].TransformTarget.rotation = rot;
            }
            MarkDirty();
        }

        public void ArrangeInHorizontalGrid(float spacingX = 0.95f)
        {
            PruneDeadPanels();
            if (_activePanels.Count == 0) return;

            int count = _activePanels.Count;
            float startX = -((count - 1) * spacingX) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                if (_activePanels[i] == null) continue;

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
                case PanelType.LaptopConnect: return _laptopConnectPanelPrefab;
                case PanelType.Browser: return _browserPanelPrefab;
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
            if (parent != null) go.transform.SetParent(parent, true);

            BoxCollider col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(0.9f, 0.65f, 0.05f);

            switch (type)
            {
                case PanelType.Notes:
                    var notes = go.AddComponent<RealityOS.Panels.NotesPanel>();
                    notes.Title = "Notes";
                    break;
                case PanelType.TaskBoard:
                    var tasks = go.AddComponent<RealityOS.Panels.TaskBoardPanel>();
                    tasks.Title = "Sprint Tasks";
                    break;
                case PanelType.Information:
                    var info = go.AddComponent<RealityOS.Panels.InformationPanel>();
                    info.Title = "Information";
                    break;
                case PanelType.ObjectViewer:
                    var viewer = go.AddComponent<RealityOS.Panels.ObjectViewerPanel>();
                    viewer.Title = "3D Object Viewer";
                    break;
                case PanelType.QuickAccess:
                    var quick = go.AddComponent<RealityOS.Panels.QuickAccessPanel>();
                    quick.Title = "Quick Access";
                    break;
                case PanelType.LaptopConnect:
                    var laptop = go.AddComponent<RealityOS.LaptopIntegration.LaptopConnectPanel>();
                    laptop.Title = "Laptop Integration Hub";
                    break;
                case PanelType.Browser:
                    var browser = go.AddComponent<RealityOS.LaptopIntegration.EmbeddedBrowserPanel>();
                    browser.Title = "Spatial Web Browser";
                    break;
            }

            return go;
        }
    }
}
