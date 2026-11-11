using UnityEngine;

namespace RealityOS.Workspace
{
    [ExecuteAlways]
    public class SpatialReferenceGrid : MonoBehaviour
    {
        [Header("Grid Dimensions")]
        [SerializeField] private int _gridSize = 20;
        [SerializeField] private float _spacing = 1.0f;
        [SerializeField] private Color _mainLineColor = new Color(0.25f, 0.35f, 0.45f, 0.35f);
        [SerializeField] private Color _axisLineColor = new Color(0.0f, 0.82f, 0.77f, 0.6f);

        private void OnDrawGizmos()
        {
            float half = (_gridSize * _spacing) * 0.5f;

            for (int i = 0; i <= _gridSize; i++)
            {
                float pos = -half + i * _spacing;
                bool isCenter = Mathf.Approximately(pos, 0f);

                Gizmos.color = isCenter ? _axisLineColor : _mainLineColor;

                // X-parallel lines
                Gizmos.DrawLine(new Vector3(-half, 0f, pos), new Vector3(half, 0f, pos));
                // Z-parallel lines
                Gizmos.DrawLine(new Vector3(pos, 0f, -half), new Vector3(pos, 0f, half));
            }
        }
    }
}

