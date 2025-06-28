using UnityEngine;

namespace GeneralScripts
{
    public class CityGenerator : MonoBehaviour
    {
        public GameObject roadPrefab;
        public GameObject[] buildingPrefabs;

        public int width = 10;
        public int depth = 10;
        public float spacing = 10f;

        public void ClearScene()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        public void GenerateRoads()
        {
            ClearScene();

            for (int x = 0; x <= width; x++)
            {
                Vector3 pos = new Vector3(x * spacing, 0, (depth * spacing) / 2);
                Instantiate(roadPrefab, pos, Quaternion.identity, transform);
            }

            for (int z = 0; z <= depth; z++)
            {
                Vector3 pos = new Vector3((width * spacing) / 2, 0, z * spacing);
                Instantiate(roadPrefab, pos, Quaternion.Euler(0, 90, 0), transform);
            }
        }

        public void GenerateTown()
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    Vector3 pos = new Vector3(x * spacing + Random.Range(-2, 2), 0, z * spacing + Random.Range(-2, 2));
                    GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                    Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0, 360), 0), transform);
                }
            }
        }
    }
}