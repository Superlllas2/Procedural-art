// Version 2023
//  (Updates: supports different root positions)

using UnityEngine;

namespace Demo
{
    public class GridCity : MonoBehaviour
    {
        public int rows = 10;
        public int columns = 10;
        public int rowWidth = 10;
        public int columnWidth = 10;
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
                DestroyChildren();
                Generate();
            }
        }

        void DestroyChildren()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        void Generate()
        {
            Vector2 cityCenter = new Vector2(columns / 2f, rows / 2f); // City center position

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    // Create a new building, chosen randomly from the prefabs:
                    int buildingIndex = Random.Range(0, buildingPrefabs.Length);
                    GameObject newBuilding = Instantiate(buildingPrefabs[buildingIndex], transform);

                    float xOffset = Random.Range(-columnWidth * 0.4f, columnWidth * 0.4f);
                    float zOffset = Random.Range(-rowWidth * 0.4f, rowWidth * 0.4f);

                    newBuilding.transform.localPosition = new Vector3(
                        col * columnWidth + xOffset,
                        0,
                        row * rowWidth + zOffset
                    );

                    newBuilding.transform.Rotate(0, Random.Range(0, 360), 0);

                    float distanceToCenter = Vector2.Distance(new Vector2(col, row), cityCenter) /
                                             (Mathf.Max(columns, rows) / 2f);
                    int minHeight = Mathf.RoundToInt(Mathf.Lerp(1, 6, 1 - distanceToCenter));
                    int maxHeight = Mathf.RoundToInt(Mathf.Lerp(2, 20, 1 - distanceToCenter));
                    
                    // float sizeFactor = Mathf.Lerp(1.0f, 0.01f, 1 - distanceToCenter);
                    // newBuilding.transform.localScale = new Vector3(
                    //     newBuilding.transform.localScale.x * sizeFactor,
                    //     newBuilding.transform.localScale.y,
                    //     newBuilding.transform.localScale.z * sizeFactor
                    // );

                    // If the building has a Shape (grammar) component, pass the height values
                    SimpleBuilding simpleBuilding = newBuilding.GetComponent<SimpleBuilding>();
                    if (simpleBuilding != null)
                    {
                        simpleBuilding.minHeight = minHeight;
                        simpleBuilding.maxHeight = maxHeight;
                    }

                    // If the building has a Shape (grammar) component, launch the grammar:
                    Shape shape = newBuilding.GetComponent<Shape>();
                    if (shape != null)
                    {
                        shape.Generate(buildDelaySeconds);
                    }
                }
            }
        }
    }
}