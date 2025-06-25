using UnityEngine;

namespace Demo
{
    // Generates a cyberpunk-inspired city block using modular prefabs.
    // Buildings are arranged in a grid with random heights.
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

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    int index = Random.Range(0, buildingPrefabs.Length);
                    GameObject building = Instantiate(buildingPrefabs[index], transform);

                    float posX = x * spacing + Random.Range(-spacing * 0.4f, spacing * 0.4f);
                    float posZ = z * spacing + Random.Range(-spacing * 0.4f, spacing * 0.4f);

                    building.transform.localPosition = new Vector3(posX, 0f, posZ);
                    building.transform.Rotate(0f, Random.Range(0f, 360f), 0f);

                    SimpleBuilding simple = building.GetComponent<SimpleBuilding>();
                    if (simple != null)
                    {
                        simple.minHeight = Random.Range(1, 4);
                        simple.maxHeight = simple.minHeight + Random.Range(2, 8);
                        simple.Generate(buildDelaySeconds);
                    }
                }
            }
        }
    }
}
