using UnityEngine;

namespace RealityOS.Workspace
{
    [ExecuteAlways]
    public class SpatialReferenceGrid : MonoBehaviour
    {
        [Header("Grid Dimensions")]
        [SerializeField] private int _gridSize = 24;
        [SerializeField] private float _spacing = 1.0f;
        [SerializeField] private Color _floorColor = new Color(0.125f, 0.173f, 0.212f, 1.0f); // #202C36 Floor Layer
        [SerializeField] private Color _gridLineColor = new Color(0.251f, 0.318f, 0.365f, 0.30f); // #40515D low opacity

        private GameObject _floorPlane;

        private void Awake()
        {
            EnsurePhysicalFloor();
        }

        private void Start()
        {
            EnsurePhysicalFloor();
        }

        private void EnsurePhysicalFloor()
        {
            Transform existing = transform.Find("VisualFloorPlane");
            if (existing != null)
            {
                _floorPlane = existing.gameObject;
                return;
            }

            // Create a physical floor plane visible in Game View and Builds
            _floorPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _floorPlane.name = "VisualFloorPlane";
            _floorPlane.transform.SetParent(transform, false);
            _floorPlane.transform.localPosition = new Vector3(0f, -0.01f, 0f);
            _floorPlane.transform.localScale = new Vector3(3.5f, 1f, 3.5f); // 35m x 35m plane

            Renderer r = _floorPlane.GetComponent<Renderer>();
            if (r != null)
            {
                Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
                Material mat = new Material(s);
                mat.color = _floorColor;
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.2f);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.4f);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.4f);

                // Create a procedural grid texture
                mat.mainTexture = CreateGridTexture();
                mat.mainTextureScale = new Vector2(24, 24);
                r.material = mat;
            }
        }

        private Texture2D CreateGridTexture()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color bg = _floorColor;
            Color line = _gridLineColor;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = (x < 3 || y < 3);
                    tex.SetPixel(x, y, isBorder ? line : bg);
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }

        private void OnDrawGizmos()
        {
            float half = (_gridSize * _spacing) * 0.5f;
            Gizmos.color = new Color(0.0f, 0.82f, 0.77f, 0.4f);

            for (int i = 0; i <= _gridSize; i++)
            {
                float pos = -half + i * _spacing;
                Gizmos.DrawLine(new Vector3(-half, 0f, pos), new Vector3(half, 0f, pos));
                Gizmos.DrawLine(new Vector3(pos, 0f, -half), new Vector3(pos, 0f, half));
            }
        }
    }
}
