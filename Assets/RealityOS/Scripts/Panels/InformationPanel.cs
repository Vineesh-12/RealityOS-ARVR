using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Persistence;

namespace RealityOS.Panels
{
    public class InformationPanel : WorkspacePanelBase
    {
        [Header("Information UI")]
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _metadataText;

        [Header("State")]
        [SerializeField] private string _description = "";
        [SerializeField] private string _iconName = "info_default";

        protected override void Awake()
        {
            _panelType = PanelType.Information;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            UpdateUI();
        }

        public void SetInformation(string description, string iconName = "info_default")
        {
            _description = description ?? "";
            _iconName = iconName ?? "info_default";
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_descriptionText != null)
            {
                _descriptionText.text = _description;
            }
            SetWorldBodyText(_description);

            if (_metadataText != null)
            {
                Vector3 p = transform.position;
                _metadataText.text = $"Pos: ({p.x:F2}, {p.y:F2}, {p.z:F2}) | ID: {_panelId.Substring(0, Mathf.Min(8, _panelId.Length))}";
            }
        }

        public override PanelData ExportData()
        {
            PanelData data = base.ExportData();
            data.infoDescription = _description;
            data.infoIconName = _iconName;
            return data;
        }

        public override void ImportData(PanelData data)
        {
            base.ImportData(data);
            if (data != null)
            {
                _description = data.infoDescription;
                _iconName = data.infoIconName;
                UpdateUI();
            }
        }
    }
}

