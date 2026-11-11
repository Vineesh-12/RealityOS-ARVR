using System;
using System.Collections.Generic;
using UnityEngine;
using RealityOS.Core;

namespace RealityOS.Persistence
{
    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [Serializable]
    public struct SerializableQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public SerializableQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public SerializableQuaternion(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion ToQuaternion() => new Quaternion(x, y, z, w);
    }

    [Serializable]
    public class TaskItemData
    {
        public string id = Guid.NewGuid().ToString();
        public string title = "New Task";
        public TaskStatus status = TaskStatus.Pending;
        public string dueDate = "";

        public TaskItemData() { }

        public TaskItemData(string title, TaskStatus status = TaskStatus.Pending)
        {
            this.title = title;
            this.status = status;
        }
    }

    [Serializable]
    public class PanelData
    {
        public string panelId = Guid.NewGuid().ToString();
        public PanelType panelType = PanelType.Notes;
        public string title = "Panel";
        
        // Transform
        public SerializableVector3 position = new SerializableVector3(0f, 1.2f, 2.0f);
        public SerializableQuaternion rotation = new SerializableQuaternion(Quaternion.identity);
        public SerializableVector3 scale = new SerializableVector3(Vector3.one);
        
        // Visibility
        public bool isVisible = true;
        public bool isPinned = false;
        
        // Content Payloads
        public string notesContent = "";
        public List<TaskItemData> tasks = new List<TaskItemData>();
        public string infoDescription = "";
        public string infoIconName = "info_default";
        public string viewerModelName = "DefaultPrimitive";
        public float viewerRotationY = 0f;
        public float viewerZoom = 1f;
        public List<string> quickLinks = new List<string>();

        public PanelData() { }

        public PanelData(PanelType type, string title, Vector3 pos)
        {
            panelId = Guid.NewGuid().ToString();
            panelType = type;
            this.title = title;
            position = new SerializableVector3(pos);
            rotation = new SerializableQuaternion(Quaternion.identity);
            scale = new SerializableVector3(Vector3.one);
        }

        public PanelData Clone()
        {
            string json = JsonUtility.ToJson(this);
            PanelData clone = JsonUtility.FromJson<PanelData>(json);
            clone.panelId = Guid.NewGuid().ToString();
            clone.title += " (Copy)";
            // Slight offset so clone does not completely occlude original
            Vector3 pos = clone.position.ToVector3() + new Vector3(0.15f, 0.05f, -0.05f);
            clone.position = new SerializableVector3(pos);
            return clone;
        }
    }

    [Serializable]
    public class WorkspaceData
    {
        public int schemaVersion = 1;
        public string workspaceId = Guid.NewGuid().ToString();
        public string workspaceName = "Default Workspace";
        public string createdAt = DateTime.UtcNow.ToString("o");
        public string lastModified = DateTime.UtcNow.ToString("o");
        public string description = "RealityOS Spatial Workspace";
        
        public List<PanelData> panels = new List<PanelData>();

        public WorkspaceData() { }

        public WorkspaceData(string name, string description = "")
        {
            workspaceId = Guid.NewGuid().ToString();
            workspaceName = name;
            this.description = description;
            createdAt = DateTime.UtcNow.ToString("o");
            lastModified = createdAt;
            panels = new List<PanelData>();
        }

        public void UpdateTimestamp()
        {
            lastModified = DateTime.UtcNow.ToString("o");
        }
    }

    [Serializable]
    public class AppSettingsData
    {
        public float mouseSensitivity = 1.0f;
        public float cameraMoveSpeed = 4.0f;
        public float cameraLookSensitivity = 2.0f;
        public float masterVolume = 0.8f;
        public float uiScale = 1.0f;
        public int graphicsQualityLevel = 2; // 0: Low, 1: Medium, 2: High
        public bool showGridReference = true;
        public bool showFPSCounter = false;
        public bool comfortSnapTurn = true;
        public float snapTurnAngle = 45f;
    }
}
