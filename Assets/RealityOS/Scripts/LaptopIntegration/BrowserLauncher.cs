using System;
using System.Diagnostics;
using UnityEngine;
using RealityOS.Core;
using Debug = UnityEngine.Debug;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// Handles launching and opening validated web URLs in Google Chrome or the default Windows browser.
    /// </summary>
    public static class BrowserLauncher
    {
        public static event Action<string> OnBrowserOpened;

        public static readonly (string Name, string Url)[] DefaultBookmarks = new[]
        {
            ("Google Search", "https://www.google.com"),
            ("GitHub", "https://github.com"),
            ("Wikipedia", "https://en.wikipedia.org"),
            ("Unity 6 Docs", "https://docs.unity3d.com/6000.0/Documentation/Manual/index.html"),
            ("Three.js 3D Web", "https://threejs.org")
        };

        /// <summary>
        /// Validates and normalizes a web URL string.
        /// </summary>
        public static bool TrySanitizeUrl(string rawUrl, out string sanitizedUrl)
        {
            sanitizedUrl = null;
            if (string.IsNullOrWhiteSpace(rawUrl)) return false;

            string candidate = rawUrl.Trim();
            if (!candidate.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !candidate.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                candidate = "https://" + candidate;
            }

            if (Uri.TryCreate(candidate, UriKind.Absolute, out Uri uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                sanitizedUrl = uriResult.AbsoluteUri;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Opens a target URL in Chrome if available, or the system default web browser.
        /// </summary>
        public static bool OpenUrl(string rawUrl)
        {
            if (!TrySanitizeUrl(rawUrl, out string validUrl))
            {
                Debug.LogWarning($"[RealityOS.BrowserLauncher] Invalid URL rejected: '{rawUrl}'");
                return false;
            }

            try
            {
                // Prefer Chrome if installed
                if (WindowsApplicationLauncher.IsAppAvailable(ExternalAppType.Chrome))
                {
                    Process proc = WindowsApplicationLauncher.Launch(ExternalAppType.Chrome, $"\"{validUrl}\"");
                    if (proc != null)
                    {
                        OnBrowserOpened?.Invoke(validUrl);
                        return true;
                    }
                }

                // Fallback to default Windows browser
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = validUrl,
                    UseShellExecute = true
                };
                Process.Start(psi);
                Debug.Log($"[RealityOS.BrowserLauncher] Opened URL via default browser: {validUrl}");
                OnBrowserOpened?.Invoke(validUrl);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RealityOS.BrowserLauncher] Failed to open browser URL '{validUrl}': {ex.Message}");
                return false;
            }
        }
    }
}
