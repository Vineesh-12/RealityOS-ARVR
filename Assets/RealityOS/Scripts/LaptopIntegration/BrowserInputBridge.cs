using System;
using UnityEngine;
using UnityEngine.EventSystems;
using RealityOS.Core;

namespace RealityOS.LaptopIntegration
{
    /// <summary>
    /// Translates 3D spatial raycast hits, mouse wheel scrolling, and keyboard typing
    /// into browser navigation and link click events on the embedded 3D browser panel.
    /// </summary>
    public class BrowserInputBridge : MonoBehaviour, IPointerClickHandler, IScrollHandler
    {
        [SerializeField] private EmbeddedBrowserController _controller;
        [SerializeField] private RectTransform _viewportRect;

        private BoxCollider _panelCollider;
        private bool _isHovered = false;

        private void Awake()
        {
            if (_controller == null)
            {
                _controller = GetComponentInParent<EmbeddedBrowserController>() ?? GetComponent<EmbeddedBrowserController>();
            }
            if (_viewportRect == null)
            {
                _viewportRect = GetComponent<RectTransform>();
            }
            _panelCollider = GetComponent<BoxCollider>();
        }

        private void Update()
        {
            if (_controller == null) return;

            // Handle mouse wheel scrolling when hovering over this browser panel
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f && _isHovered)
            {
                _controller.Scroll(scroll);
            }

            // Forward physical keyboard input to live browser when hovered
            if (_isHovered && Input.anyKeyDown)
            {
                string inputString = Input.inputString;
                if (!string.IsNullOrEmpty(inputString))
                {
                    foreach (char c in inputString)
                    {
                        string s = c.ToString();
                        _controller.SendKey(s, s);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    _controller.SendKey("\r", "Enter");
                }
                else if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    _controller.SendKey("", "Backspace");
                }
            }
        }

        public void SetHovered(bool hovered)
        {
            _isHovered = hovered;
        }

        public void SetViewportRect(RectTransform rt)
        {
            _viewportRect = rt;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_controller == null || _viewportRect == null) return;

            // Convert screen click point to local UV coordinates (0..1) on the viewport
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_viewportRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                Rect r = _viewportRect.rect;
                float uvX = Mathf.InverseLerp(r.xMin, r.xMax, localPoint.x);
                float uvY = Mathf.InverseLerp(r.yMin, r.yMax, localPoint.y);

                _controller.HandlePointerClick(new Vector2(uvX, uvY));
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_controller != null && Mathf.Abs(eventData.scrollDelta.y) > 0.01f)
            {
                _controller.Scroll(eventData.scrollDelta.y);
            }
        }

        /// <summary>
        /// Raycast entrypoint for 3D desktop/XR raycasters.
        /// </summary>
        public bool RaycastClick(Vector3 worldHitPoint)
        {
            if (_controller == null) return false;

            // Convert world hit point into local coordinates
            Vector3 localPos = transform.InverseTransformPoint(worldHitPoint);
            
            // Map box collider dimensions (-0.45..0.45, -0.325..0.325) to normalized UV (0..1)
            float uvX = Mathf.Clamp01((localPos.x + 0.45f) / 0.9f);
            float uvY = Mathf.Clamp01((localPos.y + 0.325f) / 0.65f);

            return _controller.HandlePointerClick(new Vector2(uvX, uvY));
        }

        public void RaycastScroll(float delta)
        {
            if (_controller != null)
            {
                _controller.Scroll(delta);
            }
        }
    }
}
