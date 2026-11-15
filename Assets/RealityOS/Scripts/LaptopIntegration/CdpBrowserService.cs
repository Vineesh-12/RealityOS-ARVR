using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// Chromium DevTools Protocol (CDP) live browser engine service.
    /// Drives an underlying headless Google Chrome or Microsoft Edge instance,
    /// capturing genuine Blink-rendered web pages, streaming live JPEG frames into Unity,
    /// and routing mouse clicks, scrolling, and keyboard events back into the browser.
    /// </summary>
    public class CdpBrowserService : IDisposable
    {
        private const int CDP_PORT = 9222;
        private Process _browserProcess;
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cts;
        private int _commandCounter = 10;
        private string _tempUserDataDir;
        private bool _isInitialized = false;
        private bool _isConnecting = false;

        public bool IsRunning => _browserProcess != null && !_browserProcess.HasExited;
        public bool IsConnected => _webSocket != null && _webSocket.State == WebSocketState.Open;

        public event Action<byte[]> OnFrameReceived;
        public event Action<string> OnTitleReceived;
        public event Action<string> OnNavigated;

        public async Task<bool> InitializeAsync(string initialUrl = "https://www.google.com")
        {
            if (_isInitialized) return true;
            if (_isConnecting) return false;
            _isConnecting = true;

            try
            {
                string browserPath = ResolveBrowserExecutable();
                if (string.IsNullOrEmpty(browserPath))
                {
                    UnityEngine.Debug.LogWarning("[RealityOS.CDP] No Chrome or Edge installation found for headless browser. Using built-in renderer fallback.");
                    _isConnecting = false;
                    return false;
                }

                _tempUserDataDir = Path.Combine(Path.GetTempPath(), "RealityOS_Browser_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(_tempUserDataDir);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = browserPath,
                    Arguments = $"--headless=new --remote-debugging-port={CDP_PORT} --user-data-dir=\"{_tempUserDataDir}\" --window-size=1024,640 --run-all-compositor-stages-before-draw --no-first-run --no-default-browser-check \"{initialUrl}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                _browserProcess = Process.Start(psi);
                UnityEngine.Debug.Log($"[RealityOS.CDP] Headless browser process started (PID: {_browserProcess.Id}) at port {CDP_PORT}");

                // Wait for CDP endpoint to become ready
                string wsUrl = null;
                for (int attempt = 0; attempt < 12; attempt++)
                {
                    await Task.Delay(300);
                    wsUrl = await QueryTargetWebSocketUrlAsync();
                    if (!string.IsNullOrEmpty(wsUrl)) break;
                }

                if (string.IsNullOrEmpty(wsUrl))
                {
                    UnityEngine.Debug.LogWarning("[RealityOS.CDP] Timed out waiting for browser DevTools endpoint. Falling back to built-in renderer.");
                    _isConnecting = false;
                    return false;
                }

                _cts = new CancellationTokenSource();
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri(wsUrl), _cts.Token);

                // Enable Page domain
                await SendCommandRawAsync("Page.enable", "{}");

                // Start message receive loop in background
                _ = Task.Run(ReceiveLoopAsync, _cts.Token);

                _isInitialized = true;
                _isConnecting = false;
                UnityEngine.Debug.Log("[RealityOS.CDP] Live CDP Browser Engine connected successfully.");

                // Request initial frame
                await CaptureScreenshotAsync();
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[RealityOS.CDP] Failed to initialize live CDP browser engine: {ex.Message}");
                _isConnecting = false;
                return false;
            }
        }

        private static string ResolveBrowserExecutable()
        {
            string[] candidatePaths = new[]
            {
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
            };

            foreach (var p in candidatePaths)
            {
                if (File.Exists(p)) return p;
            }
            return null;
        }

        private async Task<string> QueryTargetWebSocketUrlAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) })
                {
                    string json = await client.GetStringAsync($"http://127.0.0.1:{CDP_PORT}/json/list");
                    // Simple parse for webSocketDebuggerUrl
                    int wsIdx = json.IndexOf("\"webSocketDebuggerUrl\": \"", StringComparison.Ordinal);
                    if (wsIdx >= 0)
                    {
                        int start = wsIdx + 25;
                        int end = json.IndexOf("\"", start);
                        if (end > start)
                        {
                            return json.Substring(start, end - start).Replace(@"\u0026", "&");
                        }
                    }
                }
            }
            catch
            {
                // Port not open yet
            }
            return null;
        }

        public async Task NavigateAsync(string url)
        {
            if (!IsConnected) return;

            string escapedUrl = url.Replace("\"", "\\\"");
            await SendCommandRawAsync("Page.navigate", $"{{\"url\":\"{escapedUrl}\"}}");
            OnNavigated?.Invoke(url);

            // Wait a brief moment for page layout then capture frame
            await Task.Delay(400);
            await CaptureScreenshotAsync();
        }

        public async Task CaptureScreenshotAsync()
        {
            if (!IsConnected) return;
            await SendCommandRawAsync("Page.captureScreenshot", "{\"format\":\"jpeg\",\"quality\":75}");
        }

        public async Task SendMouseClickAsync(float x, float y)
        {
            if (!IsConnected) return;

            int px = Mathf.Clamp((int)x, 0, 1024);
            int py = Mathf.Clamp((int)y, 0, 640);

            await SendCommandRawAsync("Input.dispatchMouseEvent",
                $"{{\"type\":\"mousePressed\",\"x\":{px},\"y\":{py},\"button\":\"left\",\"clickCount\":1}}");
            await Task.Delay(30);
            await SendCommandRawAsync("Input.dispatchMouseEvent",
                $"{{\"type\":\"mouseReleased\",\"x\":{px},\"y\":{py},\"button\":\"left\",\"clickCount\":1}}");

            await Task.Delay(150);
            await CaptureScreenshotAsync();
        }

        public async Task SendMouseScrollAsync(float x, float y, float deltaY)
        {
            if (!IsConnected) return;

            int px = Mathf.Clamp((int)x, 0, 1024);
            int py = Mathf.Clamp((int)y, 0, 640);
            int dY = -(int)(deltaY * 120f);

            await SendCommandRawAsync("Input.dispatchMouseEvent",
                $"{{\"type\":\"mouseWheel\",\"x\":{px},\"y\":{py},\"deltaX\":0,\"deltaY\":{dY}}}");

            await Task.Delay(80);
            await CaptureScreenshotAsync();
        }

        public async Task SendKeyAsync(string text, string key)
        {
            if (!IsConnected) return;

            string escapedText = text.Replace("\"", "\\\"");
            string escapedKey = key.Replace("\"", "\\\"");

            await SendCommandRawAsync("Input.dispatchKeyEvent",
                $"{{\"type\":\"keyDown\",\"text\":\"{escapedText}\",\"key\":\"{escapedKey}\"}}");
            await SendCommandRawAsync("Input.dispatchKeyEvent",
                $"{{\"type\":\"keyUp\",\"key\":\"{escapedKey}\"}}");

            await Task.Delay(100);
            await CaptureScreenshotAsync();
        }

        private async Task SendCommandRawAsync(string method, string paramsJson)
        {
            if (!IsConnected) return;
            try
            {
                int id = Interlocked.Increment(ref _commandCounter);
                string message = $"{{\"id\":{id},\"method\":\"{method}\",\"params\":{paramsJson}}}";
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts?.Token ?? CancellationToken.None);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[RealityOS.CDP] SendCommand exception: {ex.Message}");
            }
        }

        private async Task ReceiveLoopAsync()
        {
            byte[] buffer = new byte[65536];
            using (MemoryStream ms = new MemoryStream())
            {
                while (_webSocket != null && _webSocket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
                {
                    try
                    {
                        ms.SetLength(0);
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                                return;
                            }
                            ms.Write(buffer, 0, result.Count);
                        } while (!result.EndOfMessage);

                        string text = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                        ProcessCdpMessage(text);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"[RealityOS.CDP] WebSocket receive exception: {ex.Message}");
                        break;
                    }
                }
            }
        }

        private void ProcessCdpMessage(string json)
        {
            if (string.IsNullOrEmpty(json)) return;

            // Check if this message is a screenshot response
            int dataIndex = json.IndexOf("\"data\":\"", StringComparison.Ordinal);
            if (dataIndex >= 0)
            {
                int start = dataIndex + 8;
                int end = json.IndexOf("\"", start);
                if (end > start)
                {
                    string base64 = json.Substring(start, end - start);
                    try
                    {
                        byte[] jpegBytes = Convert.FromBase64String(base64);
                        OnFrameReceived?.Invoke(jpegBytes);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"[RealityOS.CDP] Failed to decode screenshot frame: {ex.Message}");
                    }
                }
            }

            // Check for navigation / title updates
            if (json.Contains("\"method\":\"Page.frameNavigated\"") || json.Contains("\"method\":\"Page.navigatedWithinDocument\""))
            {
                int titleIdx = json.IndexOf("\"title\":\"", StringComparison.Ordinal);
                if (titleIdx >= 0)
                {
                    int s = titleIdx + 9;
                    int e = json.IndexOf("\"", s);
                    if (e > s)
                    {
                        string pageTitle = json.Substring(s, e - s);
                        OnTitleReceived?.Invoke(pageTitle);
                    }
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _cts?.Cancel();
                _webSocket?.Dispose();
                _webSocket = null;

                if (_browserProcess != null && !_browserProcess.HasExited)
                {
                    _browserProcess.Kill();
                    _browserProcess.Dispose();
                    _browserProcess = null;
                }

                if (!string.IsNullOrEmpty(_tempUserDataDir) && Directory.Exists(_tempUserDataDir))
                {
                    Directory.Delete(_tempUserDataDir, true);
                }
            }
            catch
            {
                // Cleanup best effort
            }
        }
    }
}
