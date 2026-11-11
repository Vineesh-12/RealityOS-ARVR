using System;
using System.Collections.Generic;
using UnityEngine;
using RealityOS.Core;
using RealityOS.Workspace;
using RealityOS.Persistence;

namespace RealityOS.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        private bool _showWorkspaceModal = false;
        private bool _showHelpModal = false;
        private string _newWorkspaceName = "New Workspace";

        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _cardTitleStyle;
        private GUIStyle _descStyle;
        private GUIStyle _cardBtnStyle;
        private GUIStyle _boxStyle;
        private bool _stylesInitialized = false;

        private void Start()
        {
            // Ensure bootstrap initialized
            ApplicationBootstrap.AutoBootstrapOnPlay();
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _titleStyle.normal.textColor = new Color(0.0f, 0.95f, 0.9f); // Cyan

            _subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            _subtitleStyle.normal.textColor = new Color(0.75f, 0.8f, 0.88f);

            _cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _cardTitleStyle.normal.textColor = Color.white;

            _descStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            };
            _descStyle.normal.textColor = new Color(0.8f, 0.85f, 0.9f);

            _cardBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            _cardBtnStyle.normal.textColor = new Color(0.0f, 0.95f, 0.9f);

            _boxStyle = new GUIStyle(GUI.skin.box);

            _stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            // Background Header Box
            float panelW = Mathf.Min(Screen.width - 40, 960);
            float startX = (Screen.width - panelW) * 0.5f;

            GUI.Box(new Rect(startX, 25, panelW, 80), GUIContent.none, _boxStyle);
            GUI.Label(new Rect(startX, 32, panelW, 35), "RealityOS", _titleStyle);
            GUI.Label(new Rect(startX, 68, panelW, 25), "A Multi-Modal Spatial Computing Environment | 21CSE353T", _subtitleStyle);

            // 3 Mode Cards side by side
            float cardGap = 16;
            float cardW = (panelW - (cardGap * 2)) / 3.0f;
            float cardH = 290;
            float cardY = 125;

            // CARD 1: Desktop Spatial Mode
            float c1X = startX;
            GUI.Box(new Rect(c1X, cardY, cardW, cardH), GUIContent.none, _boxStyle);
            GUI.Label(new Rect(c1X + 10, cardY + 15, cardW - 20, 26), "Desktop Spatial Mode", _cardTitleStyle);
            string d1 = "Navigate a complete 3D spatial workspace using your laptop's mouse and keyboard.\n\n" +
                        "* Fly with WASD + Mouse Look\n" +
                        "* 5 Floating 3D panel types\n" +
                        "* Drag, rotate, resize panels\n" +
                        "* Local persistent workspace";
            GUI.Label(new Rect(c1X + 15, cardY + 52, cardW - 30, 160), d1, _descStyle);
            if (GUI.Button(new Rect(c1X + 20, cardY + cardH - 55, cardW - 40, 38), "Launch Desktop Mode", _cardBtnStyle))
            {
                LaunchMode(InteractionMode.DesktopSpatial);
            }

            // CARD 2: Simulated XR Mode
            float c2X = c1X + cardW + cardGap;
            GUI.Box(new Rect(c2X, cardY, cardW, cardH), GUIContent.none, _boxStyle);
            GUI.Label(new Rect(c2X + 10, cardY + 15, cardW - 20, 26), "Simulated XR Mode", _cardTitleStyle);
            string d2 = "Test Extended Reality (XR) interactions without requiring physical VR headset hardware.\n\n" +
                        "* Simulated 6-DoF controllers\n" +
                        "* Laser pointer ray selection\n" +
                        "* Direct grabbing with 'G' key\n" +
                        "* 45-deg snap turns & teleport";
            GUI.Label(new Rect(c2X + 15, cardY + 52, cardW - 30, 160), d2, _descStyle);
            if (GUI.Button(new Rect(c2X + 20, cardY + cardH - 55, cardW - 40, 38), "Launch Simulated XR", _cardBtnStyle))
            {
                LaunchMode(InteractionMode.SimulatedXR);
            }

            // CARD 3: Mobile AR Mode
            float c3X = c2X + cardW + cardGap;
            GUI.Box(new Rect(c3X, cardY, cardW, cardH), GUIContent.none, _boxStyle);
            GUI.Label(new Rect(c3X + 10, cardY + 15, cardW - 20, 26), "Mobile AR Mode", _cardTitleStyle);
            string d3 = "Real augmented reality bringing digital panels onto detected real-world surfaces.\n\n" +
                        "* AR Foundation & ARCore\n" +
                        "* Real physical plane detection\n" +
                        "* Surface placement reticle\n" +
                        "* Multi-touch 3D gestures";
            GUI.Label(new Rect(c3X + 15, cardY + 52, cardW - 30, 160), d3, _descStyle);
            if (GUI.Button(new Rect(c3X + 20, cardY + cardH - 55, cardW - 40, 38), "Launch Mobile AR", _cardBtnStyle))
            {
                LaunchMode(InteractionMode.MobileAR);
            }

            // Bottom Bar: Workspace Quick Actions
            float botY = cardY + cardH + 18;
            GUI.Box(new Rect(startX, botY, panelW, 52), GUIContent.none, _boxStyle);

            string curWs = WorkspaceManager.Instance?.ActiveWorkspace?.workspaceName ?? "Default Workspace";
            int pCount = WorkspaceManager.Instance?.ActiveWorkspace?.panels?.Count ?? 0;
            GUI.Label(new Rect(startX + 15, botY + 14, 340, 26), $"Current Workspace: {curWs} ({pCount} panels)", _descStyle);

            if (GUI.Button(new Rect(startX + 370, botY + 10, 150, 32), "Manage Workspaces", GUI.skin.button))
            {
                _showWorkspaceModal = true;
            }

            if (GUI.Button(new Rect(startX + 535, botY + 10, 130, 32), "Controls & Help", GUI.skin.button))
            {
                _showHelpModal = true;
            }

            if (GUI.Button(new Rect(startX + 680, botY + 10, 130, 32), "Reset Sample Data", GUI.skin.button))
            {
                if (WorkspaceManager.Instance != null)
                {
                    var sample = PersistenceService.CreateSampleDemonstrationWorkspace();
                    WorkspaceManager.Instance.Persistence.SaveWorkspace(sample);
                    WorkspaceManager.Instance.LoadWorkspaceById(sample.workspaceId);
                }
            }

            // Workspace Management Modal
            if (_showWorkspaceModal)
            {
                DrawWorkspaceModal(startX, cardY, panelW, cardH + 80);
            }

            // Help Modal
            if (_showHelpModal)
            {
                DrawHelpModal(startX, cardY, panelW, cardH + 80);
            }
        }

        private void DrawWorkspaceModal(float x, float y, float w, float h)
        {
            GUI.Box(new Rect(x, y, w, h), GUIContent.none, _boxStyle);
            GUI.Label(new Rect(x + 20, y + 15, w - 40, 28), "Manage Workspaces", _cardTitleStyle);

            GUI.Label(new Rect(x + 30, y + 55, 140, 25), "New Workspace Name:", _descStyle);
            _newWorkspaceName = GUI.TextField(new Rect(x + 180, y + 55, 240, 25), _newWorkspaceName);
            if (GUI.Button(new Rect(x + 430, y + 53, 110, 28), "Create New", GUI.skin.button))
            {
                WorkspaceManager.Instance?.CreateNewWorkspace(_newWorkspaceName);
            }

            // List Workspaces
            var list = WorkspaceManager.Instance?.Persistence.GetAllWorkspaces();
            if (list != null)
            {
                float listY = y + 95;
                for (int i = 0; i < Mathf.Min(list.Count, 5); i++)
                {
                    var ws = list[i];
                    GUI.Label(new Rect(x + 30, listY, 320, 25), $"{ws.workspaceName} ({ws.panels.Count} panels)", _descStyle);

                    string id = ws.workspaceId;
                    if (GUI.Button(new Rect(x + 360, listY, 80, 24), "Load", GUI.skin.button))
                    {
                        WorkspaceManager.Instance?.LoadWorkspaceById(id);
                        _showWorkspaceModal = false;
                    }
                    if (GUI.Button(new Rect(x + 450, listY, 80, 24), "Delete", GUI.skin.button))
                    {
                        WorkspaceManager.Instance?.DeleteWorkspace(id);
                    }
                    listY += 30;
                }
            }

            if (GUI.Button(new Rect(x + (w - 120) * 0.5f, y + h - 42, 120, 30), "Close", _cardBtnStyle))
            {
                _showWorkspaceModal = false;
            }
        }

        private void DrawHelpModal(float x, float y, float w, float h)
        {
            GUI.Box(new Rect(x, y, w, h), GUIContent.none, _boxStyle);
            GUI.Label(new Rect(x + 20, y + 15, w - 40, 28), "RealityOS Quick Reference Guide", _cardTitleStyle);

            string help =
                "DESKTOP SPATIAL MODE:\n" +
                "  * Move: W / A / S / D  |  Sprint: Left Shift  |  Look: Right Mouse Button (Hold & Drag)\n" +
                "  * Select Panel: Left Click  |  Move Panel: Click & Drag across 3D space\n" +
                "  * Focus on Panel: Press 'F'  |  Reset Rotation: Press 'R'\n" +
                "  * Save Workspace: Ctrl + S  |  Duplicate Panel: Ctrl + D  |  Delete: Delete key\n\n" +
                "SIMULATED XR MODE (No Headset Required):\n" +
                "  * Laser Pointer: Aim with mouse  |  Trigger Select: Left Click\n" +
                "  * Grab Panel: Press 'G' (Right Hand Grip) while aiming  |  Release: Release 'G'\n" +
                "  * Snap Turning: 'Q' (45 deg Left) / 'E' (45 deg Right)\n" +
                "  * Teleportation: Hold 'T', aim at floor, release 'T'\n\n" +
                "MOBILE AR MODE:\n" +
                "  * Surface Detection: Scan physical room  |  Place Panel: Tap 'Place Panel'\n" +
                "  * Multi-touch: Single-finger drag (move), pinch (scale), two-finger twist (rotate)\n";

            GUI.Label(new Rect(x + 30, y + 50, w - 60, h - 105), help, _descStyle);

            if (GUI.Button(new Rect(x + (w - 120) * 0.5f, y + h - 42, 120, 30), "Close Help", _cardBtnStyle))
            {
                _showHelpModal = false;
            }
        }

        private void LaunchMode(InteractionMode mode)
        {
            AppModeManager.Instance?.SetMode(mode);
            SceneNavigationManager.Instance?.LoadModeScene(mode);
        }
    }
}
