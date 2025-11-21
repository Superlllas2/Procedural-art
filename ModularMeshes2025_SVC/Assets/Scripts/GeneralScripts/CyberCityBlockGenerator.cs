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

        Transform buildingsParent;

        Transform GetOrCreateBuildingsParent()
        {
            if (buildingsParent == null)
            {
                Transform existing = transform.Find("Buildings");
                if (existing != null) buildingsParent = existing;
                else
                {
                    GameObject parentGo = new GameObject("Buildings");
                    buildingsParent = parentGo.transform;
                    buildingsParent.SetParent(transform, false);
                }
            }

            return buildingsParent;
        }

        public void ClearBlocks()
        {
            Transform parent = GetOrCreateBuildingsParent();
            for (int i = parent.childCount - 1; i >= 0; i--)
                DestroyImmediate(parent.GetChild(i).gameObject);
        }

        public void GenerateBlocks()
        {
            ClearBlocks();
            Transform parent = GetOrCreateBuildingsParent();

            var roadGen = CityRoadGenerator.Instance;
            if (roadGen == null)
                return;

            float cellSize = Mathf.Max(roadGen.CellSize, 0.001f);

            foreach (Room room in roadGen.Rooms)
            {
                RectInt inner = new RectInt(room.Bounds.xMin + 1, room.Bounds.yMin + 1,
                    room.Bounds.width - 2, room.Bounds.height - 2);
                GenerateForRoom(inner, cellSize, parent);
            }
        }

        void GenerateForRoom(RectInt bounds, float cellSize, Transform parent)
        {
            Rect worldBounds = new Rect(
                bounds.xMin * cellSize,
                bounds.yMin * cellSize,
                bounds.width * cellSize,
                bounds.height * cellSize);

            float worldSpacing = Mathf.Max(spacing, 0.001f);
            float worldSidewalk = Mathf.Max(worldSpacing * 0.5f, sidewalkWidth);

            if (worldBounds.width <= worldSidewalk * 2f || worldBounds.height <= worldSidewalk * 2f)
                return;

            float usableWidth = worldBounds.width - (worldSidewalk * 2f);
            float usableHeight = worldBounds.height - (worldSidewalk * 2f);

            int stepsX = Mathf.Max(1, Mathf.FloorToInt(usableWidth / worldSpacing) + 1);
            int stepsZ = Mathf.Max(1, Mathf.FloorToInt(usableHeight / worldSpacing) + 1);

            float leftoverX = Mathf.Max(0f, usableWidth - ((stepsX - 1) * worldSpacing));
            float leftoverZ = Mathf.Max(0f, usableHeight - ((stepsZ - 1) * worldSpacing));

            float startX = worldBounds.xMin + worldSidewalk + (leftoverX * 0.5f);
            float startZ = worldBounds.yMin + worldSidewalk + (leftoverZ * 0.5f);

            for (int ix = 0; ix < stepsX; ix++)
            {
                for (int iz = 0; iz < stepsZ; iz++)
                {
                    Vector3 pos = new Vector3(startX + (ix * worldSpacing), 0f, startZ + (iz * worldSpacing));
                    Quaternion rotation = CalculateRotation(worldBounds, pos);
                    PlaceBuilding(pos, rotation, parent);
                }
            }
        }

        Quaternion CalculateRotation(Rect bounds, Vector3 pos)
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

        void PlaceBuilding(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (buildingPrefabs == null || buildingPrefabs.Length == 0)
                return;

            GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
            GameObject building = Instantiate(prefab, position, rotation, parent);

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
