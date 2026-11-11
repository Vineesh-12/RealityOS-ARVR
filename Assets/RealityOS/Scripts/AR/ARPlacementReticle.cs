using System.Collections.Generic;
using UnityEngine;
#if UNITY_XR_ARFOUNDATION
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace RealityOS.AR
{
    public class ARPlacementReticle : MonoBehaviour
    {
        [Header("Visual Elements")]
        [SerializeField] private GameObject _visualChild;
        [SerializeField] private Renderer _reticleRenderer;
        [SerializeField] private Color _validColor = new Color(0f, 0.82f, 0.77f, 0.8f);
        [SerializeField] private Color _invalidColor = new Color(1f, 0.3f, 0.3f, 0.4f);

        [Header("Smoothing")]
        [SerializeField] private float _positionLerpSpeed = 15f;
        [SerializeField] private float _rotationLerpSpeed = 15f;

        public bool IsValidPlacement { get; private set; }
        public Pose CurrentPose { get; private set; }

#if UNITY_XR_ARFOUNDATION
        [SerializeField] private ARRaycastManager _raycastManager;
        private static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
#endif

        private void Start()
        {
#if UNITY_XR_ARFOUNDATION
            if (_raycastManager == null)
            {
                _raycastManager = FindAnyObjectByType<ARRaycastManager>();
            }
#endif
            SetVisualVisible(false);
        }

        private void Update()
        {
            UpdatePlacement();
        }

        private void UpdatePlacement()
        {
#if UNITY_XR_ARFOUNDATION
            if (_raycastManager != null)
            {
                Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                if (_raycastManager.Raycast(screenCenter, s_Hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
                {
                    Pose hitPose = s_Hits[0].pose;
                    CurrentPose = hitPose;
                    IsValidPlacement = true;

                    transform.position = Vector3.Lerp(transform.position, hitPose.position, Time.deltaTime * _positionLerpSpeed);
                    transform.rotation = Quaternion.Slerp(transform.rotation, hitPose.rotation, Time.deltaTime * _rotationLerpSpeed);

                    SetVisualVisible(true);
                    SetColor(_validColor);
                    return;
                }
            }
#endif

            // Editor or Fallback simulation: Raycast against ground plane
            Ray ray = Camera.main != null ? Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)) : new Ray(Vector3.zero, Vector3.forward);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 pt = ray.GetPoint(enter);
                CurrentPose = new Pose(pt, Quaternion.identity);
                IsValidPlacement = true;
                transform.position = Vector3.Lerp(transform.position, pt, Time.deltaTime * _positionLerpSpeed);
                transform.rotation = Quaternion.identity;
                SetVisualVisible(true);
                SetColor(_validColor);
            }
            else
            {
                IsValidPlacement = false;
                SetVisualVisible(false);
            }
        }

        private void SetVisualVisible(bool visible)
        {
            if (_visualChild != null)
            {
                _visualChild.SetActive(visible);
            }
        }

        private void SetColor(Color col)
        {
            if (_reticleRenderer != null && _reticleRenderer.material != null)
            {
                _reticleRenderer.material.color = col;
            }
        }
    }
}
