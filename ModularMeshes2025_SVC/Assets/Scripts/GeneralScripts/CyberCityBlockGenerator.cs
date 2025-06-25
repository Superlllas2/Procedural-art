using UnityEngine;

namespace Demo
{
    // Generates a cyberpunk-inspired city block using modular prefabs.
    // Buildings near the center are taller and shrink in height towards the edges.
    // One landmark building can be spawned in the center.
    public class CyberCityBlockGenerator : MonoBehaviour
    {
        [Tooltip("Number of blocks along the X axis")]
        public int width = 6;
        [Tooltip("Number of blocks along the Z axis")]
        public int depth = 6;
        [Tooltip("Spacing between each building")]
        public float spacing = 10f;
        [Tooltip("Available building prefabs")]
        public GameObject[] buildingPrefabs;

        [Tooltip("Optional landmark building prefab for the city center")]
        public GameObject specialBuildingPrefab;

        [Header("Building Height Settings")]
        public int edgeMinHeight = 1;
        public int edgeMaxHeight = 4;
        public int centerMinHeight = 8;
        public int centerMaxHeight = 20;

        public float buildDelaySeconds = 0.1f;

        void Start()
        {
            Generate();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                Clear();
                Generate();
            }
        }

        void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        void Generate()
        {
            if (buildingPrefabs == null || buildingPrefabs.Length == 0)
                return;

            Vector2 center = new Vector2((width - 1) * 0.5f, (depth - 1) * 0.5f);
            float maxDistance = Vector2.Distance(Vector2.zero, center);

            int landmarkX = width / 2;
            int landmarkZ = depth / 2;

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    bool isLandmark = x == landmarkX && z == landmarkZ && specialBuildingPrefab != null;

                    GameObject prefab = isLandmark ? specialBuildingPrefab : buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                    GameObject building = Instantiate(prefab, transform);

                    float posX = x * spacing + Random.Range(-spacing * 0.4f, spacing * 0.4f);
                    float posZ = z * spacing + Random.Range(-spacing * 0.4f, spacing * 0.4f);

                    building.transform.localPosition = new Vector3(posX, 0f, posZ);
                    building.transform.Rotate(0f, Random.Range(0f, 360f), 0f);

                    float dist = Vector2.Distance(new Vector2(x, z), center);
                    float t = 1f - dist / maxDistance;

                    int minH = Mathf.RoundToInt(Mathf.Lerp(edgeMinHeight, centerMinHeight, t));
                    int maxH = Mathf.RoundToInt(Mathf.Lerp(edgeMaxHeight, centerMaxHeight, t));

                    if (isLandmark)
                    {
                        minH = centerMaxHeight;
                        maxH = centerMaxHeight + 10;
                    }

                    SimpleBuilding simple = building.GetComponent<SimpleBuilding>();
                    if (simple != null)
                    {
                        simple.minHeight = minH;
                        simple.maxHeight = maxH;
                    }

                    Shape shape = building.GetComponent<Shape>();
                    if (shape != null)
                    {
                        shape.Generate(buildDelaySeconds);
                    }
                }
            }
        }
    }
}
