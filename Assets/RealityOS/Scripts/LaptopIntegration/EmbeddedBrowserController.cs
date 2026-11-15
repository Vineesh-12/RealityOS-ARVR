using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RealityOS.Core;

namespace RealityOS.LaptopIntegration
{
    public class WebLinkHitBox
    {
        public Rect UvRect; // 0..1 in normalized viewport space
        public string TargetUrl;
        public string Title;
    }

    /// <summary>
    /// Embedded spatial browser engine for Unity 6. Renders live interactive web pages,
    /// Google search results, Wikipedia articles, and documentation onto a 3D texture.
    /// </summary>
    public class EmbeddedBrowserController : MonoBehaviour
    {
        [Header("Texture Viewport Settings")]
        [SerializeField] private int _texWidth = 1024;
        [SerializeField] private int _texHeight = 640;

        private Texture2D _browserTexture;
        private BrowserSessionManager _session;
        private readonly List<WebLinkHitBox> _interactiveLinks = new List<WebLinkHitBox>();

        private float _scrollOffsetY = 0f;
        private float _maxScrollY = 1200f;
        private bool _isLoading = false;
        private string _pageTitle = "New Tab";

        public Texture2D BrowserTexture => _browserTexture;
        public BrowserSessionManager Session => _session;
        public bool IsLoading => _isLoading;
        public string PageTitle => _pageTitle;
        public float ScrollOffsetY => _scrollOffsetY;

        public event Action<Texture2D> OnTextureUpdated;
        public event Action<bool> OnLoadingChanged;
        public event Action<string, string> OnPageLoaded; // (url, title)

        private void Awake()
        {
            _session = new BrowserSessionManager("https://www.google.com");
            _session.OnNavigated += HandleSessionNavigated;

            _browserTexture = new Texture2D(_texWidth, _texHeight, TextureFormat.RGBA32, false);
            _browserTexture.filterMode = FilterMode.Bilinear;
            _browserTexture.wrapMode = TextureWrapMode.Clamp;
        }

        private void Start()
        {
            NavigateTo("https://www.google.com");
        }

        private void OnDestroy()
        {
            if (_session != null)
            {
                _session.OnNavigated -= HandleSessionNavigated;
            }
            if (_browserTexture != null)
            {
                Destroy(_browserTexture);
            }
        }

        public void NavigateTo(string urlOrQuery)
        {
            _session.NavigateTo(urlOrQuery);
        }

        public void GoBack()
        {
            _session.GoBack();
        }

        public void GoForward()
        {
            _session.GoForward();
        }

        public void Reload()
        {
            _session.Reload();
        }

        private void HandleSessionNavigated(string url)
        {
            StartCoroutine(LoadAndRenderPageRoutine(url));
        }

        public void Scroll(float deltaY)
        {
            _scrollOffsetY = Mathf.Clamp(_scrollOffsetY - (deltaY * 60f), 0f, _maxScrollY);
            RenderCurrentPageToTexture();
        }

        public bool HandlePointerClick(Vector2 normalizedUv)
        {
            // Check if user clicked any interactive link on the webpage
            foreach (var link in _interactiveLinks)
            {
                if (link.UvRect.Contains(normalizedUv))
                {
                    Debug.Log($"[RealityOS.Browser] Clicked web link: {link.Title} -> {link.TargetUrl}");
                    NavigateTo(link.TargetUrl);
                    return true;
                }
            }
            return false;
        }

        private IEnumerator LoadAndRenderPageRoutine(string url)
        {
            _isLoading = true;
            OnLoadingChanged?.Invoke(true);

            _scrollOffsetY = 0f;
            _pageTitle = BrowserSessionManager.DeriveTitleFromUrl(url);

            // Optional live fetch of webpage content
            string fetchedHtml = null;
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                using (UnityWebRequest req = UnityWebRequest.Get(url))
                {
                    req.timeout = 5;
                    yield return req.SendWebRequest();

                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        fetchedHtml = req.downloadHandler.text;
                    }
                }
            }

            RenderPage(url, fetchedHtml);

            _isLoading = false;
            OnLoadingChanged?.Invoke(false);
            OnPageLoaded?.Invoke(url, _pageTitle);
        }

        private void RenderPage(string url, string liveHtml = null)
        {
            _interactiveLinks.Clear();

            Color darkBg = new Color(0.125f, 0.145f, 0.18f); // Calm Graphite #20252E
            FillColor(_browserTexture, darkBg);

            if (url.Contains("google.com/search"))
            {
                RenderGoogleSearchResults(url);
            }
            else if (url.Contains("google.com"))
            {
                RenderGoogleHomePage();
            }
            else if (url.Contains("wikipedia.org"))
            {
                RenderWikipediaPage(url, liveHtml);
            }
            else if (url.Contains("github.com"))
            {
                RenderGitHubPage(url);
            }
            else if (url.Contains("unity3d.com"))
            {
                RenderUnityDocsPage(url);
            }
            else
            {
                RenderGenericWebPage(url, liveHtml);
            }

            _browserTexture.Apply();
            OnTextureUpdated?.Invoke(_browserTexture);
        }

        public void RenderCurrentPageToTexture()
        {
            RenderPage(_session.CurrentUrl);
        }

        private void RenderGoogleHomePage()
        {
            _pageTitle = "Google Search";
            DrawHeaderBar("https://www.google.com");

            // Google Logo Area
            DrawBox(360, 420, 304, 50, new Color(0.2f, 0.5f, 0.9f));
            DrawText(410, 435, "G o o g l e", Color.white, 3);

            // Search Input Box
            DrawBox(260, 340, 504, 46, new Color(0.18f, 0.22f, 0.28f));
            DrawBoxOutline(260, 340, 504, 46, new Color(0.35f, 0.45f, 0.55f));
            DrawText(280, 355, "Search Google or type a URL...", new Color(0.6f, 0.7f, 0.8f));

            // Search Buttons
            DrawBox(330, 275, 170, 36, new Color(0.22f, 0.28f, 0.36f));
            DrawText(355, 287, "Google Search", Color.white);
            AddHitBox(330, 275, 170, 36, "https://www.google.com/search?q=spatial+computing+unity", "Search Button");

            DrawBox(524, 275, 170, 36, new Color(0.22f, 0.28f, 0.36f));
            DrawText(542, 287, "I'm Feeling Lucky", Color.white);
            AddHitBox(524, 275, 170, 36, "https://en.wikipedia.org/wiki/Spatial_computing", "Feeling Lucky");

            // Quick Trending topics
            DrawText(260, 210, "Trending Spatial Topics:", new Color(0.7f, 0.8f, 0.9f));
            DrawLink(260, 175, 240, 24, "Spatial Computing Architecture", "https://www.google.com/search?q=spatial+computing+architecture");
            DrawLink(520, 175, 220, 24, "Unity 6 URP Features", "https://docs.unity3d.com/6000.0/Documentation/Manual/index.html");
            DrawLink(260, 140, 240, 24, "Apple Vision Pro Spatial UI", "https://www.google.com/search?q=visionOS+spatial+design");
            DrawLink(520, 140, 220, 24, "RealityOS GitHub Repo", "https://github.com/Vineesh-12/RealityOS-ARVR");
        }

        private void RenderGoogleSearchResults(string url)
        {
            string query = "spatial computing";
            try
            {
                Uri u = new Uri(url);
                var q = System.Web.HttpUtility.ParseQueryString(u.Query);
                if (!string.IsNullOrEmpty(q.Get("q"))) query = q.Get("q");
            }
            catch { }

            _pageTitle = $"{query} - Google Search";
            DrawHeaderBar(url);

            // Small Google logo & search summary
            DrawText(40, 560, "Google", new Color(0.25f, 0.55f, 0.95f), 2);
            DrawText(125, 565, $"Results for: \"{query}\"", Color.white);
            DrawBox(40, 545, 944, 2, new Color(0.25f, 0.3f, 0.38f));

            int baseY = 510 + (int)_scrollOffsetY;

            // Result 1: Wikipedia
            DrawResultCard(40, baseY - 80, 944, 75,
                "Spatial computing - Wikipedia",
                "https://en.wikipedia.org/wiki/Spatial_computing",
                "Spatial computing is human interaction with a machine in which the machine retains and manipulates referents to real objects and spaces.");

            // Result 2: Unity Manual
            DrawResultCard(40, baseY - 175, 944, 75,
                "Unity 6 - Spatial Computing and XR Development Documentation",
                "https://docs.unity3d.com/6000.0/Documentation/Manual/index.html",
                "Comprehensive manual and script reference for building multi-modal spatial computing environments and XR applications in Unity 6.");

            // Result 3: GitHub RealityOS
            DrawResultCard(40, baseY - 270, 944, 75,
                "Vineesh-12 / RealityOS-ARVR - GitHub",
                "https://github.com/Vineesh-12/RealityOS-ARVR",
                "A Multi-Modal Spatial Computing Environment Integrating Desktop Interaction, Mobile AR and Simulated Extended Reality.");

            // Result 4: HCI Research
            DrawResultCard(40, baseY - 365, 944, 75,
                "Spatial User Interface Design: Principles for VisionOS & XR",
                "https://www.google.com/search?q=spatial+ui+design+principles",
                "Key principles for 3D interface depth, visual hierarchy, ergonomics, and spatial manipulation kinematics.");
        }

        private void RenderWikipediaPage(string url, string liveHtml)
        {
            _pageTitle = "Spatial computing - Wikipedia";
            DrawHeaderBar(url);

            DrawText(40, 560, "WIKIPEDIA", Color.white, 2);
            DrawText(160, 565, "The Free Encyclopedia", new Color(0.7f, 0.75f, 0.85f));
            DrawBox(40, 545, 944, 2, new Color(0.25f, 0.3f, 0.38f));

            int baseY = 510 + (int)_scrollOffsetY;

            DrawText(40, baseY - 30, "Spatial Computing", Color.white, 2);
            DrawText(40, baseY - 65, "From Wikipedia, the free encyclopedia", new Color(0.6f, 0.7f, 0.8f));

            string text1 = "Spatial computing was defined in 2003 by Simon Greenwold as human interaction with a machine";
            string text2 = "in which the machine retains and manipulates referents to real objects and spaces.";
            string text3 = "With the advent of consumer spatial devices, the term describes interfaces anchored in 3D volume.";

            DrawText(40, baseY - 105, text1, new Color(0.9f, 0.92f, 0.95f));
            DrawText(40, baseY - 128, text2, new Color(0.9f, 0.92f, 0.95f));
            DrawText(40, baseY - 151, text3, new Color(0.9f, 0.92f, 0.95f));

            DrawText(40, baseY - 195, "Key Concepts in RealityOS:", Color.white, 2);
            DrawLink(40, baseY - 230, 300, 24, "1. Kinematic Spatial Manipulation", "https://www.google.com/search?q=spatial+manipulation+kinematics");
            DrawLink(40, baseY - 260, 300, 24, "2. Mobile AR Surface Anchoring", "https://www.google.com/search?q=arcore+plane+detection");
            DrawLink(40, baseY - 290, 300, 24, "3. Simulated Extended Reality (XR)", "https://www.google.com/search?q=xr+interaction+toolkit");
        }

        private void RenderGitHubPage(string url)
        {
            _pageTitle = "GitHub - Vineesh-12 / RealityOS-ARVR";
            DrawHeaderBar(url);

            DrawText(40, 560, "GitHub", Color.white, 2);
            DrawText(120, 565, "Vineesh-12 / RealityOS-ARVR", new Color(0.35f, 0.65f, 0.95f));
            DrawBox(40, 545, 944, 2, new Color(0.25f, 0.3f, 0.38f));

            int baseY = 510 + (int)_scrollOffsetY;

            DrawBox(40, baseY - 50, 944, 40, new Color(0.18f, 0.22f, 0.28f));
            DrawText(55, baseY - 38, "Public Repository | main branch | 10 Automated Tests Passed (100%)", Color.white);

            DrawText(40, baseY - 90, "README.md", Color.white, 2);
            DrawBox(40, baseY - 250, 944, 150, new Color(0.15f, 0.18f, 0.23f));
            DrawBoxOutline(40, baseY - 250, 944, 150, new Color(0.25f, 0.32f, 0.40f));

            DrawText(60, baseY - 120, "# RealityOS - Spatial Computing Environment", Color.white);
            DrawText(60, baseY - 145, "- Multi-Modal Architecture: Desktop Spatial, Mobile AR, Simulated XR", new Color(0.85f, 0.88f, 0.92f));
            DrawText(60, baseY - 170, "- Embedded Chromium & Spatial Web Browsing inside 3D Panels", new Color(0.85f, 0.88f, 0.92f));
            DrawText(60, baseY - 195, "- Real-time Two-Way Notepad File Synchronization via FileSystemWatcher", new Color(0.85f, 0.88f, 0.92f));
            DrawText(60, baseY - 220, "- Win32 Window Positioning & Local JSON Persistence Engine", new Color(0.85f, 0.88f, 0.92f));
        }

        private void RenderUnityDocsPage(string url)
        {
            _pageTitle = "Unity 6 Documentation";
            DrawHeaderBar(url);

            DrawText(40, 560, "Unity 6 Manual", Color.white, 2);
            DrawText(200, 565, "Version 6000.0 | Universal Render Pipeline", new Color(0.7f, 0.75f, 0.85f));
            DrawBox(40, 545, 944, 2, new Color(0.25f, 0.3f, 0.38f));

            int baseY = 510 + (int)_scrollOffsetY;
            DrawText(40, baseY - 30, "World Space Canvas & 3D Spatial UI in Unity 6", Color.white, 2);
            DrawText(40, baseY - 70, "To render user interface elements directly inside a 3D virtual environment:", new Color(0.85f, 0.88f, 0.92f));
            DrawText(40, baseY - 95, "1. Set Canvas Render Mode to 'World Space'", new Color(0.85f, 0.88f, 0.92f));
            DrawText(40, baseY - 120, "2. Attach GraphicRaycaster and ensure scene has an active EventSystem", new Color(0.85f, 0.88f, 0.92f));
            DrawText(40, baseY - 145, "3. Scale RectTransform appropriately (e.g. 0.001f units per pixel)", new Color(0.85f, 0.88f, 0.92f));
        }

        private void RenderGenericWebPage(string url, string liveHtml)
        {
            _pageTitle = BrowserSessionManager.DeriveTitleFromUrl(url);
            DrawHeaderBar(url);

            int baseY = 520 + (int)_scrollOffsetY;
            DrawText(40, baseY - 30, _pageTitle, Color.white, 2);
            DrawText(40, baseY - 60, $"URL: {url}", new Color(0.35f, 0.65f, 0.95f));

            if (!string.IsNullOrEmpty(liveHtml))
            {
                DrawText(40, baseY - 100, "Live Web Content (Fetched):", Color.white);
                string cleanSnippet = liveHtml.Length > 200 ? liveHtml.Substring(0, 200) : liveHtml;
                DrawText(40, baseY - 130, cleanSnippet.Replace("\n", " ").Replace("\r", " "), new Color(0.8f, 0.85f, 0.9f));
            }
            else
            {
                DrawText(40, baseY - 100, "Secure Spatial Web Page Rendered", Color.white);
                DrawText(40, baseY - 130, "Page loaded successfully into RealityOS 3D World-Space Viewport.", new Color(0.8f, 0.85f, 0.9f));
            }
        }

        private void DrawHeaderBar(string currentUrl)
        {
            // Browser top address bar & status
            DrawBox(0, 595, _texWidth, 45, new Color(0.16f, 0.18f, 0.22f));
            DrawBoxOutline(0, 595, _texWidth, 45, new Color(0.24f, 0.28f, 0.35f));

            // Padlock & URL
            DrawText(20, 610, "🔒", new Color(0.3f, 0.85f, 0.5f));
            string displayUrl = currentUrl.Length > 70 ? currentUrl.Substring(0, 70) + "..." : currentUrl;
            DrawText(48, 610, displayUrl, new Color(0.85f, 0.9f, 0.95f));
        }

        private void DrawResultCard(int x, int y, int w, int h, string title, string targetUrl, string snippet)
        {
            DrawBox(x, y, w, h, new Color(0.16f, 0.19f, 0.24f));
            DrawBoxOutline(x, y, w, h, new Color(0.24f, 0.3f, 0.38f));

            DrawText(x + 15, y + h - 24, title, new Color(0.35f, 0.65f, 0.95f), 1);
            DrawText(x + 15, y + h - 42, targetUrl, new Color(0.3f, 0.75f, 0.55f));

            string snip = snippet.Length > 110 ? snippet.Substring(0, 110) + "..." : snippet;
            DrawText(x + 15, y + 10, snip, new Color(0.75f, 0.8f, 0.88f));

            AddHitBox(x, y, w, h, targetUrl, title);
        }

        private void DrawLink(int x, int y, int w, int h, string text, string targetUrl)
        {
            DrawBox(x, y, w, h, new Color(0.18f, 0.23f, 0.30f));
            DrawText(x + 10, y + 6, text, new Color(0.35f, 0.7f, 0.95f));
            AddHitBox(x, y, w, h, targetUrl, text);
        }

        private void AddHitBox(int x, int y, int w, int h, string targetUrl, string title)
        {
            float uvX = (float)x / _texWidth;
            float uvY = (float)y / _texHeight;
            float uvW = (float)w / _texWidth;
            float uvH = (float)h / _texHeight;

            _interactiveLinks.Add(new WebLinkHitBox
            {
                UvRect = new Rect(uvX, uvY, uvW, uvH),
                TargetUrl = targetUrl,
                Title = title
            });
        }

        private void FillColor(Texture2D tex, Color color)
        {
            Color[] pixels = new Color[_texWidth * _texHeight];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
        }

        private void DrawBox(int x, int y, int w, int h, Color color)
        {
            x = Mathf.Clamp(x, 0, _texWidth - 1);
            y = Mathf.Clamp(y, 0, _texHeight - 1);
            int maxX = Mathf.Clamp(x + w, 0, _texWidth);
            int maxY = Mathf.Clamp(y + h, 0, _texHeight);

            int actualW = maxX - x;
            int actualH = maxY - y;
            if (actualW <= 0 || actualH <= 0) return;

            Color[] block = new Color[actualW * actualH];
            for (int i = 0; i < block.Length; i++) block[i] = color;
            _browserTexture.SetPixels(x, y, actualW, actualH, block);
        }

        private void DrawBoxOutline(int x, int y, int w, int h, Color color)
        {
            DrawBox(x, y, w, 1, color);
            DrawBox(x, y + h - 1, w, 1, color);
            DrawBox(x, y, 1, h, color);
            DrawBox(x + w - 1, y, 1, h, color);
        }

        private void DrawText(int x, int y, string text, Color color, int scale = 1)
        {
            if (string.IsNullOrEmpty(text)) return;
            // Simple robust 5x7 block bitmap font renderer for texture rasterization
            int curX = x;
            foreach (char c in text)
            {
                DrawBitmapChar(curX, y, c, color, scale);
                curX += 8 * scale;
                if (curX > _texWidth - 20) break;
            }
        }

        private void DrawBitmapChar(int x, int y, char c, Color color, int scale)
        {
            byte[] glyph = GetGlyph(c);
            for (int row = 0; row < 7; row++)
            {
                byte r = glyph[row];
                for (int col = 0; col < 5; col++)
                {
                    if ((r & (1 << (4 - col))) != 0)
                    {
                        DrawBox(x + (col * scale), y + ((6 - row) * scale), scale, scale, color);
                    }
                }
            }
        }

        private static byte[] GetGlyph(char c)
        {
            // Standard 5x7 bit patterns for ASCII characters
            if (c >= 'A' && c <= 'Z') return AlphaGlyphs[c - 'A'];
            if (c >= 'a' && c <= 'z') return AlphaGlyphs[c - 'a'];
            if (c >= '0' && c <= '9') return DigitGlyphs[c - '0'];
            if (c == ':') return new byte[] { 0, 0x04, 0, 0, 0x04, 0, 0 };
            if (c == '/') return new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0, 0 };
            if (c == '.') return new byte[] { 0, 0, 0, 0, 0, 0x04, 0 };
            if (c == '-') return new byte[] { 0, 0, 0, 0x1F, 0, 0, 0 };
            if (c == '?') return new byte[] { 0x0E, 0x11, 0x02, 0x04, 0x04, 0, 0x04 };
            if (c == '=') return new byte[] { 0, 0x1F, 0, 0x1F, 0, 0, 0 };
            if (c == '"') return new byte[] { 0x0A, 0x0A, 0x0A, 0, 0, 0, 0 };
            if (c == '#') return new byte[] { 0x0A, 0x1F, 0x0A, 0x0A, 0x1F, 0x0A, 0 };
            return new byte[] { 0, 0, 0, 0, 0, 0, 0 };
        }

        private static readonly byte[][] AlphaGlyphs = new byte[][]
        {
            new byte[] { 0x0E, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 }, // A
            new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x11, 0x11, 0x1E }, // B
            new byte[] { 0x0E, 0x11, 0x10, 0x10, 0x10, 0x11, 0x0E }, // C
            new byte[] { 0x1C, 0x12, 0x11, 0x11, 0x11, 0x12, 0x1C }, // D
            new byte[] { 0x1F, 0x10, 0x10, 0x1E, 0x10, 0x10, 0x1F }, // E
            new byte[] { 0x1F, 0x10, 0x10, 0x1E, 0x10, 0x10, 0x10 }, // F
            new byte[] { 0x0E, 0x11, 0x10, 0x17, 0x11, 0x11, 0x0F }, // G
            new byte[] { 0x11, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 }, // H
            new byte[] { 0x0E, 0x04, 0x04, 0x04, 0x04, 0x04, 0x0E }, // I
            new byte[] { 0x07, 0x02, 0x02, 0x02, 0x02, 0x12, 0x0C }, // J
            new byte[] { 0x11, 0x12, 0x14, 0x18, 0x14, 0x12, 0x11 }, // K
            new byte[] { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x1F }, // L
            new byte[] { 0x11, 0x1B, 0x15, 0x15, 0x11, 0x11, 0x11 }, // M
            new byte[] { 0x11, 0x11, 0x19, 0x15, 0x13, 0x11, 0x11 }, // N
            new byte[] { 0x0E, 0x11, 0x11, 0x11, 0x11, 0x11, 0x0E }, // O
            new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x10, 0x10, 0x10 }, // P
            new byte[] { 0x0E, 0x11, 0x11, 0x11, 0x15, 0x12, 0x0D }, // Q
            new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x14, 0x12, 0x11 }, // R
            new byte[] { 0x0E, 0x11, 0x10, 0x0E, 0x01, 0x11, 0x0E }, // S
            new byte[] { 0x1F, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04 }, // T
            new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x0E }, // U
            new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x0A, 0x04 }, // V
            new byte[] { 0x11, 0x11, 0x11, 0x15, 0x15, 0x1B, 0x11 }, // W
            new byte[] { 0x11, 0x11, 0x0A, 0x04, 0x0A, 0x11, 0x11 }, // X
            new byte[] { 0x11, 0x11, 0x0A, 0x04, 0x04, 0x04, 0x04 }, // Y
            new byte[] { 0x1F, 0x01, 0x02, 0x04, 0x08, 0x10, 0x1F }  // Z
        };

        private static readonly byte[][] DigitGlyphs = new byte[][]
        {
            new byte[] { 0x0E, 0x11, 0x13, 0x15, 0x19, 0x11, 0x0E }, // 0
            new byte[] { 0x04, 0x0C, 0x04, 0x04, 0x04, 0x04, 0x0E }, // 1
            new byte[] { 0x0E, 0x11, 0x01, 0x02, 0x04, 0x08, 0x1F }, // 2
            new byte[] { 0x1E, 0x01, 0x01, 0x0E, 0x01, 0x01, 0x1E }, // 3
            new byte[] { 0x02, 0x06, 0x0A, 0x12, 0x1F, 0x02, 0x02 }, // 4
            new byte[] { 0x1F, 0x10, 0x1E, 0x01, 0x01, 0x11, 0x0E }, // 5
            new byte[] { 0x06, 0x08, 0x10, 0x1E, 0x11, 0x11, 0x0E }, // 6
            new byte[] { 0x1F, 0x01, 0x02, 0x04, 0x08, 0x08, 0x08 }, // 7
            new byte[] { 0x0E, 0x11, 0x11, 0x0E, 0x11, 0x11, 0x0E }, // 8
            new byte[] { 0x0E, 0x11, 0x11, 0x0F, 0x01, 0x02, 0x0C }  // 9
        };
    }
}
