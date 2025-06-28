using System.Collections.Generic;
using Demo;
using UnityEngine;

namespace GeneralScripts
{
    /// <summary>
    /// Spawns procedural buildings inside the rooms produced by <see cref="CityRoadGenerator"/>.
    /// Buildings are aligned to the closest street edge and use grammar based prefabs
    /// to vary their appearance.
    /// </summary>
    public class CyberCityBlockGenerator : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject[] buildingPrefabs;

        [Header("Generation Settings")]
        public float spacing = 8f;
        public float sidewalkWidth = 2f;
        public float buildDelaySeconds = 0.1f;
        public int minHeight = 1;
        public int maxHeight = 5;

        public void ClearBlocks()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        public void GenerateBlocks()
        {
            ClearBlocks();

            var roadGen = CityRoadGenerator.Instance;
            if (roadGen == null)
                return;

            foreach (Room room in roadGen.Rooms)
            {
                RectInt inner = new RectInt(room.Bounds.xMin + 1, room.Bounds.yMin + 1,
                    room.Bounds.width - 2, room.Bounds.height - 2);
                GenerateForRoom(inner);
            }
        }

        void GenerateForRoom(RectInt bounds)
        {
            for (float x = bounds.xMin + sidewalkWidth; x <= bounds.xMax - sidewalkWidth; x += spacing)
            {
                for (float z = bounds.yMin + sidewalkWidth; z <= bounds.yMax - sidewalkWidth; z += spacing)
                {
                    Vector3 pos = new Vector3(x, 0f, z);
                    Quaternion rotation = CalculateRotation(bounds, pos);
                    PlaceBuilding(pos, rotation);
                }
            }
        }

        Quaternion CalculateRotation(RectInt bounds, Vector3 pos)
        {
            float left = pos.x - bounds.xMin;
            float right = bounds.xMax - pos.x;
            float bottom = pos.z - bounds.yMin;
            float top = bounds.yMax - pos.z;
            float min = Mathf.Min(Mathf.Min(left, right), Mathf.Min(top, bottom));

            if (Mathf.Approximately(min, left)) return Quaternion.Euler(0f, 90f, 0f);
            if (Mathf.Approximately(min, right)) return Quaternion.Euler(0f, -90f, 0f);
            if (Mathf.Approximately(min, top)) return Quaternion.Euler(0f, 180f, 0f);
            return Quaternion.identity;
        }

        void PlaceBuilding(Vector3 position, Quaternion rotation)
        {
            if (buildingPrefabs == null || buildingPrefabs.Length == 0)
                return;

            GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            GameObject building = Instantiate(prefab, position, rotation, transform);

            var simple = building.GetComponent<SimpleBuilding>();
            if (simple != null)
            {
                simple.minHeight = minHeight;
                simple.maxHeight = maxHeight;
            }

            var shape = building.GetComponent<Shape>();
            if (shape != null)
            {
                shape.Generate(buildDelaySeconds);
            }
        }
    }
}
