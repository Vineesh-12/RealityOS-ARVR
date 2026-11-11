using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using RealityOS.Core;

namespace RealityOS.Persistence
{
    public class PersistenceService : IWorkspacePersistence
    {
        private readonly string _workspacesDirectory;
        private readonly string _settingsFilePath;

        public PersistenceService()
        {
            string rootDir = Path.Combine(Application.persistentDataPath, "RealityOS");
            _workspacesDirectory = Path.Combine(rootDir, "Workspaces");
            _settingsFilePath = Path.Combine(rootDir, "settings.json");

            EnsureDirectoriesExist();
        }

        public PersistenceService(string customRootPath)
        {
            _workspacesDirectory = Path.Combine(customRootPath, "Workspaces");
            _settingsFilePath = Path.Combine(customRootPath, "settings.json");
            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            try
            {
                if (!Directory.Exists(_workspacesDirectory))
                {
                    Directory.CreateDirectory(_workspacesDirectory);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.Persistence] Failed to create directories: {ex.Message}");
            }
        }

        public string GetWorkspaceFilePath(string workspaceId)
        {
            return Path.Combine(_workspacesDirectory, $"{workspaceId}.json");
        }

        public bool SaveWorkspace(WorkspaceData workspace)
        {
            if (workspace == null || string.IsNullOrEmpty(workspace.workspaceId))
            {
                Debug.LogError("[RealityOS.Persistence] Cannot save null or invalid workspace data.");
                return false;
            }

            try
            {
                workspace.UpdateTimestamp();
                ValidateWorkspaceData(workspace);

                string filePath = GetWorkspaceFilePath(workspace.workspaceId);
                string json = JsonUtility.ToJson(workspace, true);

                // Write atomically via temporary file
                string tempPath = filePath + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempPath, filePath);

                Debug.Log($"[RealityOS.Persistence] Successfully saved workspace '{workspace.workspaceName}' ({workspace.workspaceId}) to {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.Persistence] Failed to save workspace '{workspace.workspaceId}': {ex.Message}");
                return false;
            }
        }

        public WorkspaceData LoadWorkspace(string workspaceId)
        {
            if (string.IsNullOrEmpty(workspaceId))
            {
                Debug.LogError("[RealityOS.Persistence] Workspace ID is null or empty.");
                return null;
            }

            string filePath = GetWorkspaceFilePath(workspaceId);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[RealityOS.Persistence] Workspace file not found at {filePath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new InvalidDataException("Workspace file is empty.");
                }

                WorkspaceData workspace = JsonUtility.FromJson<WorkspaceData>(json);
                if (workspace == null)
                {
                    throw new InvalidDataException("Failed to deserialize workspace JSON.");
                }

                ValidateWorkspaceData(workspace);
                return workspace;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.Persistence] Corrupted workspace detected '{workspaceId}': {ex.Message}");
                BackupCorruptFile(filePath);
                return null;
            }
        }

        public bool DeleteWorkspace(string workspaceId)
        {
            try
            {
                string filePath = GetWorkspaceFilePath(workspaceId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[RealityOS.Persistence] Deleted workspace {workspaceId}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.Persistence] Error deleting workspace {workspaceId}: {ex.Message}");
                return false;
            }
        }

        public List<WorkspaceData> GetAllWorkspaces()
        {
            List<WorkspaceData> list = new List<WorkspaceData>();
            EnsureDirectoriesExist();

            try
            {
                string[] files = Directory.GetFiles(_workspacesDirectory, "*.json");
                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        WorkspaceData ws = JsonUtility.FromJson<WorkspaceData>(json);
                        if (ws != null && !string.IsNullOrEmpty(ws.workspaceId))
                        {
                            list.Add(ws);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[RealityOS.Persistence] Skipping unreadable file {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.Persistence] Error scanning workspaces: {ex.Message}");
            }

            // If empty, generate default demonstration workspace
            if (list.Count == 0)
            {
                WorkspaceData sample = CreateSampleDemonstrationWorkspace();
                SaveWorkspace(sample);
                list.Add(sample);
            }

            return list;
        }

        public bool WorkspaceExists(string workspaceId)
        {
            return File.Exists(GetWorkspaceFilePath(workspaceId));
        }

        public bool SaveSettings(AppSettingsData settings)
        {
            try
            {
                string json = JsonUtility.ToJson(settings, true);
                File.WriteAllText(_settingsFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.Persistence] Failed to save settings: {ex.Message}");
                return false;
            }
        }

        public AppSettingsData LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonUtility.FromJson<AppSettingsData>(json);
                    if (settings != null) return settings;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RealityOS.Persistence] Could not load settings, using defaults: {ex.Message}");
            }
            return new AppSettingsData();
        }

        private void BackupCorruptFile(string filePath)
        {
            try
            {
                string backup = filePath + $".corrupt.{DateTime.UtcNow.Ticks}.bak";
                File.Move(filePath, backup);
                Debug.LogWarning($"[RealityOS.Persistence] Backed up corrupted workspace to {backup}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.Persistence] Failed to backup corrupt file: {ex.Message}");
            }
        }

        public static void ValidateWorkspaceData(WorkspaceData workspace)
        {
            if (string.IsNullOrEmpty(workspace.workspaceId))
                workspace.workspaceId = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(workspace.workspaceName))
                workspace.workspaceName = "Untitled Workspace";

            if (workspace.panels == null)
                workspace.panels = new List<PanelData>();

            foreach (var panel in workspace.panels)
            {
                if (string.IsNullOrEmpty(panel.panelId))
                    panel.panelId = Guid.NewGuid().ToString();

                if (panel.tasks == null)
                    panel.tasks = new List<TaskItemData>();

                if (panel.quickLinks == null)
                    panel.quickLinks = new List<string>();

                // Clamp extreme scale values to keep UI usable in 3D
                Vector3 scale = panel.scale.ToVector3();
                scale.x = Mathf.Clamp(scale.x, 0.3f, 5.0f);
                scale.y = Mathf.Clamp(scale.y, 0.3f, 5.0f);
                scale.z = Mathf.Clamp(scale.z, 0.3f, 5.0f);
                panel.scale = new SerializableVector3(scale);
            }
        }

        public static WorkspaceData CreateSampleDemonstrationWorkspace()
        {
            WorkspaceData ws = new WorkspaceData("RealityOS Showcase Workspace", "Comprehensive sample demonstrating all panel types and 3D spatial organization.");
            ws.workspaceId = "sample-showcase-01";

            // 1. Information Panel - Center
            PanelData info = new PanelData(PanelType.Information, "Welcome to RealityOS", new Vector3(0f, 1.4f, 2.2f));
            info.infoDescription = "RealityOS is a unified multi-modal spatial computing environment supporting Desktop Spatial Mode, Mobile AR, and Simulated XR.\n\nInteract with floating panels using mouse/keyboard, touch gestures, or simulated controllers.";
            info.infoIconName = "realityos_logo";
            ws.panels.Add(info);

            // 2. Notes Panel - Left
            PanelData notes = new PanelData(PanelType.Notes, "Project Notes", new Vector3(-1.1f, 1.35f, 2.0f));
            notes.rotation = new SerializableQuaternion(Quaternion.Euler(0f, 20f, 0f));
            notes.notesContent = "Meeting with spatial computing team:\n- Finalize 3D manipulation kinematics\n- Verify ARCore plane detection on Android\n- Validate XR Interaction Toolkit simulation\n- Optimize draw calls for Dell G15 RTX 3050.";
            ws.panels.Add(notes);

            // 3. Task Board Panel - Right
            PanelData tasks = new PanelData(PanelType.TaskBoard, "Sprint Tasks", new Vector3(1.1f, 1.35f, 2.0f));
            tasks.rotation = new SerializableQuaternion(Quaternion.Euler(0f, -20f, 0f));
            tasks.tasks.Add(new TaskItemData("Inspect Unity Editor environment", TaskStatus.Completed));
            tasks.tasks.Add(new TaskItemData("Build 3D Spatial Workspace Engine", TaskStatus.Completed));
            tasks.tasks.Add(new TaskItemData("Implement Desktop Mouse/Keyboard controls", TaskStatus.InProgress));
            tasks.tasks.Add(new TaskItemData("Implement ARCore plane reticle & gestures", TaskStatus.InProgress));
            tasks.tasks.Add(new TaskItemData("Configure XR device simulator ray interaction", TaskStatus.Pending));
            ws.panels.Add(tasks);

            // 4. 3D Object Viewer Panel - Bottom Center
            PanelData viewer = new PanelData(PanelType.ObjectViewer, "3D Spatial Inspector", new Vector3(0f, 0.75f, 1.8f));
            viewer.rotation = new SerializableQuaternion(Quaternion.Euler(-15f, 0f, 0f));
            viewer.viewerModelName = "SpatialTorus";
            viewer.viewerRotationY = 45f;
            viewer.viewerZoom = 1.0f;
            ws.panels.Add(viewer);

            // 5. Quick Access Panel - Far Left Lower
            PanelData quick = new PanelData(PanelType.QuickAccess, "Quick Navigation", new Vector3(-1.0f, 0.75f, 1.7f));
            quick.rotation = new SerializableQuaternion(Quaternion.Euler(-10f, 25f, 0f));
            quick.quickLinks.Add("Desktop Workspace");
            quick.quickLinks.Add("Mobile AR Mode");
            quick.quickLinks.Add("Simulated XR Lab");
            quick.quickLinks.Add("Syllabus Mapping (21CSE353T)");
            ws.panels.Add(quick);

            return ws;
        }
    }
}

