using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using RealityOS.Core;
using RealityOS.Persistence;
using RealityOS.Panels;

namespace RealityOS.Tests
{
    public class RealityOSTests
    {
        private string _testDir;
        private PersistenceService _persistence;

        [SetUp]
        public void Setup()
        {
            _testDir = Path.Combine(Application.temporaryCachePath, "RealityOSTests_" + Guid.NewGuid().ToString("N"));
            _persistence = new PersistenceService(_testDir);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_testDir))
            {
                try
                {
                    Directory.Delete(_testDir, true);
                }
                catch { }
            }
        }

        [Test]
        public void Test01_WorkspaceSerialization_ProducesValidJson()
        {
            WorkspaceData ws = new WorkspaceData("Test Serialization Workspace", "Unit test description");
            ws.panels.Add(new PanelData(PanelType.Notes, "Test Notes", new Vector3(1f, 2f, 3f)));
            ws.panels[0].notesContent = "Sample unit test note content.";

            string json = JsonUtility.ToJson(ws, true);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("Test Serialization Workspace"));
            Assert.IsTrue(json.Contains("Sample unit test note content."));
        }

        [Test]
        public void Test02_WorkspaceDeserialization_RestoresAllFieldsAccurately()
        {
            WorkspaceData ws = new WorkspaceData("Restore Workspace");
            PanelData panel = new PanelData(PanelType.TaskBoard, "Sprint 1", new Vector3(-0.5f, 1.2f, 2.0f));
            panel.tasks.Add(new TaskItemData("Write tests", TaskStatus.Completed));
            panel.tasks.Add(new TaskItemData("Validate persistence", TaskStatus.InProgress));
            ws.panels.Add(panel);

            string json = JsonUtility.ToJson(ws);
            WorkspaceData restored = JsonUtility.FromJson<WorkspaceData>(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual("Restore Workspace", restored.workspaceName);
            Assert.AreEqual(1, restored.panels.Count);
            Assert.AreEqual(PanelType.TaskBoard, restored.panels[0].panelType);
            Assert.AreEqual(2, restored.panels[0].tasks.Count);
            Assert.AreEqual("Write tests", restored.panels[0].tasks[0].title);
            Assert.AreEqual(TaskStatus.Completed, restored.panels[0].tasks[0].status);
            Assert.AreEqual(-0.5f, restored.panels[0].position.x, 0.001f);
        }

        [Test]
        public void Test03_PanelDataValidation_FixesNullsAndClampsExtremeScales()
        {
            WorkspaceData ws = new WorkspaceData("");
            ws.workspaceId = ""; // Invalid ID
            ws.workspaceName = ""; // Empty name

            PanelData p = new PanelData();
            p.panelId = null;
            p.scale = new SerializableVector3(50f, 0.01f, 1f); // Out of bounds
            p.tasks = null;
            p.quickLinks = null;
            ws.panels.Add(p);

            PersistenceService.ValidateWorkspaceData(ws);

            Assert.IsFalse(string.IsNullOrEmpty(ws.workspaceId));
            Assert.AreEqual("Untitled Workspace", ws.workspaceName);
            Assert.IsFalse(string.IsNullOrEmpty(ws.panels[0].panelId));
            Assert.IsNotNull(ws.panels[0].tasks);
            Assert.IsNotNull(ws.panels[0].quickLinks);
            Assert.LessOrEqual(ws.panels[0].scale.x, 5.0f);
            Assert.GreaterOrEqual(ws.panels[0].scale.y, 0.3f);
        }

        [Test]
        public void Test04_WorkspaceCRUD_FullLifecycle()
        {
            // Create
            WorkspaceData ws = new WorkspaceData("CRUD Test");
            string id = ws.workspaceId;
            bool saved = _persistence.SaveWorkspace(ws);
            Assert.IsTrue(saved, "Failed to save workspace.");
            Assert.IsTrue(_persistence.WorkspaceExists(id));

            // Read
            WorkspaceData loaded = _persistence.LoadWorkspace(id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual("CRUD Test", loaded.workspaceName);

            // Update
            loaded.workspaceName = "CRUD Test Renamed";
            loaded.panels.Add(new PanelData(PanelType.Information, "Info", Vector3.up));
            bool updated = _persistence.SaveWorkspace(loaded);
            Assert.IsTrue(updated);

            WorkspaceData reloaded = _persistence.LoadWorkspace(id);
            Assert.AreEqual("CRUD Test Renamed", reloaded.workspaceName);
            Assert.AreEqual(1, reloaded.panels.Count);

            // Delete
            bool deleted = _persistence.DeleteWorkspace(id);
            Assert.IsTrue(deleted);
            Assert.IsFalse(_persistence.WorkspaceExists(id));
            Assert.IsNull(_persistence.LoadWorkspace(id));
        }

        [Test]
        public void Test05_PanelClone_GeneratesUniqueIdAndOffsetPosition()
        {
            PanelData original = new PanelData(PanelType.Notes, "Original Note", new Vector3(0f, 1f, 2f));
            original.notesContent = "Secret thoughts";

            PanelData clone = original.Clone();

            Assert.AreNotEqual(original.panelId, clone.panelId, "Clone should have a new unique ID.");
            Assert.IsTrue(clone.title.Contains("(Copy)"));
            Assert.AreEqual(original.notesContent, clone.notesContent);
            Assert.AreNotEqual(original.position.x, clone.position.x, "Clone position should have an offset.");
        }

        [Test]
        public void Test06_CorruptJson_GracefulRecoveryWithoutCrash()
        {
            string corruptId = "corrupted-test-id";
            string path = _persistence.GetWorkspaceFilePath(corruptId);
            File.WriteAllText(path, "{ this is invalid broken json content !@#$ }");

            WorkspaceData result = _persistence.LoadWorkspace(corruptId);
            Assert.IsNull(result, "Loading corrupted file should return null.");
            Assert.IsFalse(File.Exists(path), "Corrupt file should be backed up and removed from primary path.");
        }
    }
}
