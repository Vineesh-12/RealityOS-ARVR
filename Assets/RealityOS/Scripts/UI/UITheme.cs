using System;
using UnityEngine;

namespace RealityOS.UI
{
    /// <summary>
    /// Centralized color palette and typography configuration for RealityOS.
    /// Professional Calm Graphite – Warm Neutral – Muted Teal design system.
    /// </summary>
    public static class UITheme
    {
        // 1. Background and Environment
        public static readonly Color DeepGraphite = new Color(0.063f, 0.094f, 0.125f, 1f); // #101820
        public static readonly Color SlateBackground = new Color(0.090f, 0.137f, 0.176f, 1f); // #17232D
        public static readonly Color FloorLayer = new Color(0.125f, 0.173f, 0.212f, 1f); // #202C36
        public static readonly Color FloorGrid = new Color(0.251f, 0.318f, 0.365f, 0.35f); // #40515D low opacity

        // 2. Panels and Surfaces
        public static readonly Color PanelBase = new Color(0.125f, 0.169f, 0.212f, 0.98f); // #202B36
        public static readonly Color RaisedPanel = new Color(0.161f, 0.220f, 0.267f, 1f); // #293844
        public static readonly Color PanelBorder = new Color(0.227f, 0.290f, 0.341f, 1f); // #3A4A57

        // 3. Accent and Interaction (Muted Teal Palette)
        public static readonly Color PrimaryTeal = new Color(0.239f, 0.580f, 0.545f, 1f); // #3D948B
        public static readonly Color HoverTeal = new Color(0.345f, 0.686f, 0.639f, 1f); // #58AFA3
        public static readonly Color Success = new Color(0.404f, 0.663f, 0.549f, 1f); // #67A98C
        public static readonly Color Warning = new Color(0.851f, 0.608f, 0.322f, 1f); // #D99B52
        public static readonly Color Danger = new Color(0.784f, 0.345f, 0.345f, 1f); // #C85858

        // 4. Typography
        public static readonly Color PrimaryText = new Color(0.949f, 0.961f, 0.969f, 1f); // #F2F5F7
        public static readonly Color SecondaryText = new Color(0.776f, 0.816f, 0.847f, 1f); // #C6D0D8
        public static readonly Color MutedText = new Color(0.529f, 0.588f, 0.635f, 1f); // #8796A2

        /// <summary>
        /// Applies theme ambient and fog settings to the active scene.
        /// </summary>
        public static void ApplyEnvironmentTheme()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = SlateBackground;
            RenderSettings.fog = true;
            RenderSettings.fogColor = DeepGraphite;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.008f;
        }
    }
}
