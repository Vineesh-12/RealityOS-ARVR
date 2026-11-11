using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Persistence;

namespace RealityOS.Panels
{
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

        protected override void Awake()
        {
            _panelType = PanelType.TaskBoard;
            base.Awake();

            if (_addTaskButton != null)
            {
                _addTaskButton.onClick.AddListener(OnAddTaskClicked);
            }
        }

        private void OnDestroy()
        {
            if (_addTaskButton != null)
            {
                _addTaskButton.onClick.RemoveListener(OnAddTaskClicked);
            }
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

        private void OnAddTaskClicked()
        {
            if (_newTaskInputField != null && !string.IsNullOrWhiteSpace(_newTaskInputField.text))
            {
                AddTask(_newTaskInputField.text);
                _newTaskInputField.text = "";
            }
        }

        public void RefreshTaskUI()
        {
            if (_taskListContainer == null) return;

            // Clear old UI items
            for (int i = _taskListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_taskListContainer.GetChild(i).gameObject);
            }

            int completedCount = 0;

            foreach (var task in _tasks)
            {
                if (task.status == TaskStatus.Completed) completedCount++;

                GameObject itemObj;
                if (_taskItemPrefab != null)
                {
                    itemObj = Instantiate(_taskItemPrefab, _taskListContainer);
                }
                else
                {
                    itemObj = CreateProceduralTaskItem(task, _taskListContainer);
                }

                BindTaskItem(itemObj, task);
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var t in _tasks)
            {
                string mark = t.status == TaskStatus.Completed ? "[v]" : (t.status == TaskStatus.InProgress ? "[~]" : "[ ]");
                sb.AppendLine($"{mark} {t.title}");
            }
            SetWorldBodyText(sb.ToString());

            if (_progressSummaryText != null)
            {
                int total = _tasks.Count;
                _progressSummaryText.text = $"{completedCount} / {total} Completed";
            }
        }

        private void BindTaskItem(GameObject itemObj, TaskItemData task)
        {
            var titleTxt = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            if (titleTxt != null)
            {
                string prefix = task.status == TaskStatus.Completed ? "[v] " : (task.status == TaskStatus.InProgress ? "[~] " : "[ ] ");
                titleTxt.text = prefix + task.title;
                titleTxt.color = task.status == TaskStatus.Completed ? new Color(0.6f, 0.6f, 0.6f) : Color.white;
            }

            var btn = itemObj.GetComponent<Button>();
            if (btn != null)
            {
                string idCopy = task.id;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ToggleTaskStatus(idCopy));
            }
        }

        private GameObject CreateProceduralTaskItem(TaskItemData task, Transform parent)
        {
            GameObject go = new GameObject($"TaskItem_{task.id}");
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280f, 32f);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.28f, 0.9f);

            var btn = go.AddComponent<Button>();

            GameObject textObj = new GameObject("TitleText");
            textObj.transform.SetParent(go.transform, false);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(8f, 2f);
            textRt.offsetMax = new Vector2(-8f, -2f);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;

            return go;
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

