using UnityEngine;

namespace Demo
{
    // Generates a small fantasy village inspired by classic RPG towns.
    // Houses are placed around a central plaza and oriented randomly.
    public class FantasyVillageGenerator : MonoBehaviour
    {
        [Tooltip("Number of houses to spawn in the village")]
        public int houseCount = 12;
        [Tooltip("Maximum radius of the village")]
        public float radius = 25f;
        [Tooltip("Available house prefabs")]
        public GameObject[] housePrefabs;

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
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        void Generate()
        {
            if (housePrefabs == null || housePrefabs.Length == 0)
                return;

            for (int i = 0; i < houseCount; i++)
            {
                int prefabIndex = Random.Range(0, housePrefabs.Length);
                GameObject house = Instantiate(housePrefabs[prefabIndex], transform);

                float angle = (i / (float)houseCount) * Mathf.PI * 2f;
                float distance = Random.Range(radius * 0.4f, radius);
                Vector3 position = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;

                house.transform.localPosition = position;
                house.transform.Rotate(0f, Random.Range(0f, 360f), 0f);

                Shape shape = house.GetComponent<Shape>();
                if (shape != null)
                {
                    shape.Generate(buildDelaySeconds);
                }
            }
        }
    }
}
