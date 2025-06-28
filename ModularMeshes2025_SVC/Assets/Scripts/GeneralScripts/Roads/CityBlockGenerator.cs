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
        for (float x = bounds.xMin + sidewalkWidth; x <= bounds.xMax - sidewalkWidth; x += spacing)
        {
            for (float z = bounds.yMin + sidewalkWidth; z <= bounds.yMax - sidewalkWidth; z += spacing)
            {
                Vector3 position = new Vector3(x, 0f, z);
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
