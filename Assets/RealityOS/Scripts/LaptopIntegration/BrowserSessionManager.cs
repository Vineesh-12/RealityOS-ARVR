using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// Manages browser navigation history, bookmarks, tabs, and session state.
    /// </summary>
    public class BrowserSessionManager
    {
        public static readonly (string Title, string Url)[] DefaultBookmarks = new[]
        {
            ("Google", "https://www.google.com"),
            ("GitHub", "https://github.com"),
            ("Wikipedia", "https://en.wikipedia.org"),
            ("Unity 6 Manual", "https://docs.unity3d.com/6000.0/Documentation/Manual/index.html"),
            ("Spatial Web 3D", "https://threejs.org")
        };

        private readonly Stack<string> _backStack = new Stack<string>();
        private readonly Stack<string> _forwardStack = new Stack<string>();
        private string _currentUrl = "https://www.google.com";
        private string _pageTitle = "Google Search";

        public string CurrentUrl => _currentUrl;
        public string PageTitle => _pageTitle;
        public bool CanGoBack => _backStack.Count > 0;
        public bool CanGoForward => _forwardStack.Count > 0;

        public event Action<string> OnNavigated;
        public event Action<bool, bool> OnHistoryStateChanged; // (canBack, canForward)

        public BrowserSessionManager(string initialUrl = "https://www.google.com")
        {
            _currentUrl = string.IsNullOrWhiteSpace(initialUrl) ? "https://www.google.com" : initialUrl;
        }

        public void NavigateTo(string url, string title = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            string normalized = NormalizeUrlOrSearch(url);
            if (normalized == _currentUrl) return;

            if (!string.IsNullOrEmpty(_currentUrl))
            {
                _backStack.Push(_currentUrl);
            }

            _forwardStack.Clear();
            _currentUrl = normalized;
            _pageTitle = title ?? DeriveTitleFromUrl(normalized);

            OnHistoryStateChanged?.Invoke(CanGoBack, CanGoForward);
            OnNavigated?.Invoke(_currentUrl);
        }

        public string GoBack()
        {
            if (!CanGoBack) return _currentUrl;

            _forwardStack.Push(_currentUrl);
            _currentUrl = _backStack.Pop();
            _pageTitle = DeriveTitleFromUrl(_currentUrl);

            OnHistoryStateChanged?.Invoke(CanGoBack, CanGoForward);
            OnNavigated?.Invoke(_currentUrl);
            return _currentUrl;
        }

        public string GoForward()
        {
            if (!CanGoForward) return _currentUrl;

            _backStack.Push(_currentUrl);
            _currentUrl = _forwardStack.Pop();
            _pageTitle = DeriveTitleFromUrl(_currentUrl);

            OnHistoryStateChanged?.Invoke(CanGoBack, CanGoForward);
            OnNavigated?.Invoke(_currentUrl);
            return _currentUrl;
        }

        public void Reload()
        {
            OnNavigated?.Invoke(_currentUrl);
        }

        public static string NormalizeUrlOrSearch(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "https://www.google.com";

            string trimmed = input.Trim();

            // If it contains spaces or doesn't have a dot/slash, treat as Google Search
            if (trimmed.Contains(" ") || (!trimmed.Contains(".") && !trimmed.StartsWith("localhost")))
            {
                string encoded = Uri.EscapeDataString(trimmed);
                return $"https://www.google.com/search?q={encoded}";
            }

            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = "https://" + trimmed;
            }

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out Uri uri))
            {
                return uri.AbsoluteUri;
            }

            return "https://www.google.com/search?q=" + Uri.EscapeDataString(input);
        }

        public static string DeriveTitleFromUrl(string url)
        {
            try
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                {
                    if (uri.Host.Contains("google"))
                    {
                        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        string q = query.Get("q");
                        return string.IsNullOrEmpty(q) ? "Google Search" : $"{q} - Google Search";
                    }
                    if (uri.Host.Contains("github")) return "GitHub - Where the world builds software";
                    if (uri.Host.Contains("wikipedia")) return "Wikipedia, the free encyclopedia";
                    if (uri.Host.Contains("unity3d")) return "Unity Documentation";
                    return uri.Host;
                }
            }
            catch { }
            return url;
        }
    }
}
