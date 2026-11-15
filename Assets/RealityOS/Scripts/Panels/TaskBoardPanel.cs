using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Persistence;

namespace RealityOS.Panels
{
    /// <summary>
    /// Interactive Task Board Panel. Allows users to add tasks, cycle status
    /// (Pending -> In Progress -> Completed), delete tasks, and track sprint progress.
    /// </summary>
    public class TaskBoardPanel : WorkspacePanelBase
    {
        [Header("Task Board UI")]
        [SerializeField] private TMP_InputField _newTaskInputField;
        [SerializeField] private Button _addTaskButton;
        [SerializeField] private Transform _taskListContainer;
        [SerializeField] private GameObject _taskItemPrefab;
        [SerializeField] private TextMeshProUGUI _progressSummaryText;

        [Header("Runtime State")]
        [SerializeField] private List<TaskItemData> _tasks = new List<TaskItemData>();

        private string _inProgressNewTaskName = "New Task Item";
        private GUIStyle _cardBoxStyle;
        private GUIStyle _taskTitleStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _statusBtnStyle;
        private GUIStyle _summaryStyle;
        private bool _guiStylesInit = false;

        public IReadOnlyList<TaskItemData> Tasks => _tasks;

        protected override void Awake()
        {
            _panelType = PanelType.TaskBoard;
            base.Awake();

            if (_addTaskButton != null)
            {
                _addTaskButton.onClick.AddListener(OnAddTaskButtonClicked);
            }
        }

        protected override void Start()
        {
            base.Start();
            RefreshTaskUI();
        }

        public void AddTask(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return;

            TaskItemData newTask = new TaskItemData(title.Trim(), TaskStatus.Pending);
            _tasks.Add(newTask);
            RefreshTaskUI();

            if (RealityOS.Workspace.WorkspaceManager.Instance != null)
            {
                RealityOS.Workspace.WorkspaceManager.Instance.MarkDirty();
            }
        }

        public void ToggleTaskStatus(string taskId)
        {
            var task = _tasks.Find(t => t.id == taskId);
            if (task != null)
            {
                switch (task.status)
                {
                    case TaskStatus.Pending:
                        task.status = TaskStatus.InProgress;
                        break;
                    case TaskStatus.InProgress:
                        task.status = TaskStatus.Completed;
                        break;
                    case TaskStatus.Completed:
                        task.status = TaskStatus.Pending;
                        break;
                }
                RefreshTaskUI();
                if (RealityOS.Workspace.WorkspaceManager.Instance != null)
                {
                    RealityOS.Workspace.WorkspaceManager.Instance.MarkDirty();
                }
            }
        }

        public void RemoveTask(string taskId)
        {
            _tasks.RemoveAll(t => t.id == taskId);
            RefreshTaskUI();
            if (RealityOS.Workspace.WorkspaceManager.Instance != null)
            {
                RealityOS.Workspace.WorkspaceManager.Instance.MarkDirty();
            }
        }

        private void OnAddTaskButtonClicked()
        {
            if (_newTaskInputField != null && !string.IsNullOrWhiteSpace(_newTaskInputField.text))
            {
                AddTask(_newTaskInputField.text);
                _newTaskInputField.text = "";
            }
        }

        public void RefreshTaskUI()
        {
            int completedCount = 0;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (var t in _tasks)
            {
                if (t.status == TaskStatus.Completed) completedCount++;
                string mark = t.status == TaskStatus.Completed ? "[v]" : (t.status == TaskStatus.InProgress ? "[~]" : "[ ]");
                sb.AppendLine($"{mark} {t.title}");
            }

            SetWorldBodyText(sb.ToString());

            if (_progressSummaryText != null)
            {
                _progressSummaryText.text = $"{completedCount} / {_tasks.Count} Completed";
            }
        }

        private void InitGuiStyles()
        {
            if (_guiStylesInit) return;

            _cardBoxStyle = new GUIStyle(GUI.skin.box);

            _taskTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            _taskTitleStyle.normal.textColor = Color.white;

            _btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };
            _btnStyle.normal.textColor = Color.white;

            _statusBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };

            _summaryStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft
            };
            _summaryStyle.normal.textColor = new Color(0.7f, 0.85f, 0.95f);

            _guiStylesInit = true;
        }

        private void OnGUI()
        {
            // Interactive task manager controls when selected
            if (!IsSelected) return;

            InitGuiStyles();

            float w = 380;
            float h = 340;
            float x = Screen.width - w - 20;
            float y = Screen.height - h - 45;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none, _cardBoxStyle);

            int done = 0;
            foreach (var t in _tasks) if (t.status == TaskStatus.Completed) done++;
            GUI.Label(new Rect(x + 15, y + 10, w - 30, 22), $"📋 {_title} ({done}/{_tasks.Count} Done)", _taskTitleStyle);

            // Add new task row
            _inProgressNewTaskName = GUI.TextField(new Rect(x + 15, y + 36, w - 120, 26), _inProgressNewTaskName);
            if (GUI.Button(new Rect(x + w - 95, y + 36, 80, 26), "+ Add", _btnStyle))
            {
                AddTask(_inProgressNewTaskName);
                _inProgressNewTaskName = "New Task";
            }

            // Task Rows List
            float curY = y + 70;
            for (int i = 0; i < Mathf.Min(_tasks.Count, 6); i++)
            {
                var task = _tasks[i];
                string statusLabel = task.status == TaskStatus.Completed ? "✓ Done" : (task.status == TaskStatus.InProgress ? "~ WIP" : "○ Todo");
                
                // Status Toggle Button
                if (GUI.Button(new Rect(x + 15, curY, 70, 24), statusLabel, _statusBtnStyle))
                {
                    ToggleTaskStatus(task.id);
                }

                // Task Title
                GUI.Label(new Rect(x + 92, curY + 2, w - 150, 20), task.title, _summaryStyle);

                // Delete Button
                if (GUI.Button(new Rect(x + w - 45, curY, 30, 24), "×", _btnStyle))
                {
                    RemoveTask(task.id);
                    break;
                }

                curY += 28;
            }

            // Bottom Clear / Status
            if (GUI.Button(new Rect(x + 15, y + h - 34, 110, 26), "Clear Done", _btnStyle))
            {
                _tasks.RemoveAll(t => t.status == TaskStatus.Completed);
                RefreshTaskUI();
                if (RealityOS.Workspace.WorkspaceManager.Instance != null)
                {
                    RealityOS.Workspace.WorkspaceManager.Instance.MarkDirty();
                }
            }
        }

        public override PanelData ExportData()
        {
            PanelData data = base.ExportData();
            data.tasks = new List<TaskItemData>(_tasks);
            return data;
        }

        public override void ImportData(PanelData data)
        {
            base.ImportData(data);
            if (data != null && data.tasks != null)
            {
                _tasks = new List<TaskItemData>(data.tasks);
                RefreshTaskUI();
            }
        }
    }
}
