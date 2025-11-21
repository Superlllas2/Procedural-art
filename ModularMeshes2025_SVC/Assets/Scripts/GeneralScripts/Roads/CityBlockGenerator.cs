using Demo;
using UnityEngine;

public class CityBlockGenerator : MonoBehaviour
{
    public RectInt bounds;
    public GameObject[] buildingPrefabs;
    public float sidewalkWidth = 2f;
    public float spacing = 8f;
    public float buildDelaySeconds = 0.1f;

    public void Generate()
    {
        float usableWidth = bounds.width - (sidewalkWidth * 2f);
        float usableHeight = bounds.height - (sidewalkWidth * 2f);

        if (usableWidth <= 0f || usableHeight <= 0f)
            return;

        int stepsX = Mathf.Max(1, Mathf.FloorToInt(usableWidth / spacing) + 1);
        int stepsZ = Mathf.Max(1, Mathf.FloorToInt(usableHeight / spacing) + 1);

        float leftoverX = Mathf.Max(0f, usableWidth - ((stepsX - 1) * spacing));
        float leftoverZ = Mathf.Max(0f, usableHeight - ((stepsZ - 1) * spacing));

        float startX = bounds.xMin + sidewalkWidth + (leftoverX * 0.5f);
        float startZ = bounds.yMin + sidewalkWidth + (leftoverZ * 0.5f);

        for (int ix = 0; ix < stepsX; ix++)
        {
            for (int iz = 0; iz < stepsZ; iz++)
            {
                Vector3 position = new Vector3(startX + (ix * spacing), 0f, startZ + (iz * spacing));
                PlaceBuilding(position);
            }
        }
    }

    void PlaceBuilding(Vector3 position)
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
            return;

        GameObject prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
        GameObject building = Instantiate(prefab, position, Quaternion.identity, transform);

        float left = position.x - bounds.xMin;
        float right = bounds.xMax - position.x;
        float bottom = position.z - bounds.yMin;
        float top = bounds.yMax - position.z;

        float min = Mathf.Min(Mathf.Min(left, right), Mathf.Min(top, bottom));
        Quaternion rotation = Quaternion.identity;
        if (Mathf.Approximately(min, left)) rotation = Quaternion.Euler(0f, 90f, 0f);
        else if (Mathf.Approximately(min, right)) rotation = Quaternion.Euler(0f, -90f, 0f);
        else if (Mathf.Approximately(min, top)) rotation = Quaternion.Euler(0f, 180f, 0f);
        building.transform.rotation = rotation;

        var shape = building.GetComponent<Shape>();
        if (shape != null)
        {
            shape.Generate(buildDelaySeconds);
        }
    }
}
