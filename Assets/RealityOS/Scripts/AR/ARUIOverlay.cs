using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RealityOS.Core;
using RealityOS.Workspace;

namespace RealityOS.AR
{
    public class ARUIOverlay : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private ARWorkspaceController _arController;

        private PanelType _selectedType = PanelType.Notes;
        private string[] _panelTypeNames = new string[] { "Notes", "TaskBoard", "Information", "ObjectViewer", "QuickAccess" };
        private int _selectedIdx = 0;

        private GUIStyle _headerStyle;
        private GUIStyle _subStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _boxStyle;
        private bool _stylesInit = false;

        private void Start()
        {
            if (_arController == null)
            {
                _arController = FindAnyObjectByType<ARWorkspaceController>();
            }
        }

        private void InitStyles()
        {
            if (_stylesInit) return;

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _headerStyle.normal.textColor = new Color(0.0f, 0.95f, 0.9f);

            _subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal
            };
            _subStyle.normal.textColor = Color.white;

            _btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            _btnStyle.normal.textColor = Color.white;

            _boxStyle = new GUIStyle(GUI.skin.box);
            _stylesInit = true;
        }

        private void OnGUI()
        {
            InitStyles();

            // Top Guidance Bar
            float barW = Mathf.Min(Screen.width - 20, 850);
            float startX = (Screen.width - barW) * 0.5f;

            GUI.Box(new Rect(startX, 10, barW, 46), GUIContent.none, _boxStyle);
            GUI.Label(new Rect(startX + 14, 16, 260, 26), "RealityOS | Mobile AR Mode", _headerStyle);

            GUI.Label(new Rect(startX + 260, 18, 440, 24), "Point camera at floor or surface -> Click 'Place Panel'", _subStyle);

            if (GUI.Button(new Rect(startX + barW - 100, 16, 85, 28), "Main Menu", _btnStyle))
            {
                SceneNavigationManager.Instance?.ReturnToMainMenu();
            }

            // Bottom Placement Bar
            float botW = 680;
            float botX = (Screen.width - botW) * 0.5f;
            float botY = Screen.height - 56;

            GUI.Box(new Rect(botX, botY, botW, 46), GUIContent.none, _boxStyle);

            GUI.Label(new Rect(botX + 10, botY + 12, 110, 24), "Spawn Type:", _subStyle);

            float bX = botX + 90;
            for (int i = 0; i < _panelTypeNames.Length; i++)
            {
                bool isSelected = (_selectedIdx == i);
                GUI.color = isSelected ? Color.cyan : Color.white;
                if (GUI.Button(new Rect(bX, botY + 10, 75, 26), _panelTypeNames[i], _btnStyle))
                {
                    _selectedIdx = i;
                    _selectedType = (PanelType)i;
                    _arController?.SetSelectedPanelType(_selectedType);
                }
                bX += 80;
            }
            GUI.color = Color.white;

            if (GUI.Button(new Rect(bX + 10, botY + 8, 95, 30), "Place Panel", _btnStyle))
            {
                _arController?.PlaceSelectedPanelAtReticle();
            }

            if (GUI.Button(new Rect(bX + 115, botY + 8, 65, 30), "Clear", _btnStyle))
            {
                _arController?.ClearAllARPanels();
            }
        }
    }
}
