using Demo;
using UnityEngine;

namespace GeneralScripts
{
    public class CyberCityBlockGenerator : MonoBehaviour
    {
        [Header("City Settings")]
        public int width = 6;
        public int depth = 6;
        public float spacing = 10f;

        [Header("Prefabs")]
        public GameObject[] buildingPrefabs;
        public GameObject specialBuildingPrefab;
        public GameObject roadPrefab;

        [Header("Building Height Settings")]
        public int edgeMinHeight = 1;
        public int edgeMaxHeight = 4;
        public int centerMinHeight = 8;
        public int centerMaxHeight = 20;
        
        [Header("References")]

        [Header("Timing")]
        public float buildDelaySeconds = 0.1f;
        public float buildingOffsetFromRoad = 4f;


        public void ClearScene()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        public void GenerateRoads()
        {
            if (roadPrefab == null)
            {
                Debug.LogWarning("Road prefab not assigned.");
                return;
            }

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
            // foreach (Transform road in roadManager.allRoads)
            // {
            //     Vector3 pos = road.position;
            //     Vector3 forward = road.forward;
            //     Vector3 right = road.right;
            //
            //     // left side
            //     Vector3 leftPos = pos - right * buildingOffsetFromRoad;
            //     PlaceBuilding(leftPos, Quaternion.LookRotation(-forward));
            //
            //     // right side
            //     Vector3 rightPos = pos + right * buildingOffsetFromRoad;
            //     PlaceBuilding(rightPos, Quaternion.LookRotation(forward));
            // }
        }
        
        void PlaceBuilding(Vector3 position, Quaternion rotation)
        {
            if (buildingPrefabs == null || buildingPrefabs.Length == 0) return;

            GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            GameObject building = Instantiate(prefab, position, rotation, transform);

            // height logic — optional
            var simple = building.GetComponent<SimpleBuilding>();
            if (simple != null)
            {
                simple.minHeight = edgeMinHeight;
                simple.maxHeight = centerMaxHeight;
            }

            var shape = building.GetComponent<Shape>();
            if (shape != null)
            {
                shape.Generate(buildDelaySeconds);
            }
        }
    }
}
