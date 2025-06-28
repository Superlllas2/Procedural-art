using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CityRoadGenerator : MonoBehaviour
{
    [Header("City Block Settings")]
    public RectInt initialBounds = new RectInt(0, 0, 100, 60);
    public int minSplitSize = 20;
    public int maxDepth = 4;

    [Header("Road Prefabs")]
    public GameObject roadPrefab;
    public GameObject roadHorizontalPrefab;
    public GameObject roadVerticalPrefab;

    public GameObject crosswalkPrefab;

    [Header("Road Settings")]
    public float roadHeight = 0.2f;
    public float roadThickness = 8f;
    public int overlap = 1;

    private BSPNode rootNode;
    private List<Room> allBlocks = new();
    private List<Vector2Int> crosswalks = new();

    private Transform roadParent;
    private Transform crosswalkParent;

    public static CityRoadGenerator Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
    
    public void Start()
    {
        roadParent = new GameObject("Roads").transform;
        crosswalkParent = new GameObject("Crosswalks").transform;

        rootNode = new BSPNode { Bounds = initialBounds };
        Split(rootNode, maxDepth);
        CreateBlocks(rootNode);
        ConnectBlocks(rootNode);
        CreateRoadsForBlocks();
        // CreateOuterRoads(initialBounds);
    }

    void Split(BSPNode node, int depth)
    {
        if (depth == 0 || node.Bounds.width < minSplitSize * 2 && node.Bounds.height < minSplitSize * 2)
            return;

        bool splitHorizontally = Random.value > 0.5f;

        if (node.Bounds.width > node.Bounds.height)
            splitHorizontally = false;
        else if (node.Bounds.height > node.Bounds.width)
            splitHorizontally = true;

        if (splitHorizontally)
        {
            int splitY = Random.Range(minSplitSize, node.Bounds.height - minSplitSize);
            node.Left = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x, node.Bounds.y, node.Bounds.width, splitY + overlap)
            };
            node.Right = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x, node.Bounds.y + splitY, node.Bounds.width,
                    node.Bounds.height - splitY)
            };
        }
        else
        {
            int splitX = Random.Range(minSplitSize, node.Bounds.width - minSplitSize);
            node.Left = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x, node.Bounds.y, splitX + overlap, node.Bounds.height)
            };
            node.Right = new BSPNode
            {
                Bounds = new RectInt(node.Bounds.x + splitX, node.Bounds.y, node.Bounds.width - splitX,
                    node.Bounds.height)
            };
        }

        Split(node.Left, depth - 1);
        Split(node.Right, depth - 1);
    }

    void CreateBlocks(BSPNode node)
    {
        if (node.Left != null || node.Right != null)
        {
            if (node.Left != null) CreateBlocks(node.Left);
            if (node.Right != null) CreateBlocks(node.Right);
            return;
        }

        Room block = new Room { Bounds = node.Bounds };
        node.Room = block;
        allBlocks.Add(block);
    }

    void ConnectBlocks(BSPNode node)
    {
        if (node.Left == null || node.Right == null)
            return;

        var blockA = GetBlockInSubtree(node.Left);
        var blockB = GetBlockInSubtree(node.Right);

        if (blockA != null && blockB != null)
        {
            var overlap = AlgorithmsUtils.Intersect(blockA.Bounds, blockB.Bounds);

            if (overlap.width > 0 || overlap.height > 0)
            {
                Vector2Int crosswalkPos;

                if (overlap.width > 1)
                {
                    int x = Mathf.Clamp(overlap.xMin + Random.Range(1, overlap.width - 1), overlap.xMin + 1,
                        overlap.xMax - 2);
                    int y = overlap.yMin;
                    crosswalkPos = new Vector2Int(x, y);
                }
                else if (overlap.height > 1)
                {
                    int x = overlap.xMin;
                    int y = Mathf.Clamp(overlap.yMin + Random.Range(1, overlap.height - 1), overlap.yMin + 1,
                        overlap.yMax - 2);
                    crosswalkPos = new Vector2Int(x, y);
                }
                else
                {
                    crosswalkPos = new Vector2Int(overlap.xMin, overlap.yMin);
                }

                crosswalks.Add(crosswalkPos);
            }
        }

        ConnectBlocks(node.Left);
        ConnectBlocks(node.Right);
    }

    Room GetBlockInSubtree(BSPNode node)
    {
        if (node == null) return null;

        if (node.Room != null)
            return node.Room;

        Room left = GetBlockInSubtree(node.Left);
        if (left != null) return left;

        return GetBlockInSubtree(node.Right);
    }

    void CreateRoadsForBlocks()
    {
        foreach (var block in allBlocks)
        {
            CreatePerimeterRoads(block.Bounds);
        }

        foreach (var crosswalk in crosswalks)
        {
            CreateCrosswalk(crosswalk);
        }
    }

    void CreatePerimeterRoads(RectInt bounds)
    {
        // Top
        CreateRoadSegment(new RectInt(bounds.xMin, bounds.yMax - 1, bounds.width, 1));
        // Bottom
        CreateRoadSegment(new RectInt(bounds.xMin, bounds.yMin, bounds.width, 1));
        // Left
        CreateRoadSegment(new RectInt(bounds.xMin, bounds.yMin, 1, bounds.height));
        // Right
        CreateRoadSegment(new RectInt(bounds.xMax - 1, bounds.yMin, 1, bounds.height));
    }

    void CreateRoadSegment(RectInt segment)
    {
        Vector3 position = new Vector3(segment.center.x, roadHeight / 2f, segment.center.y);
        
        float lengthX = segment.width;
        float lengthZ = segment.height;

        float scaleX = (lengthX > 1) ? lengthX : roadThickness;
        float scaleZ = (lengthZ > 1) ? lengthZ : roadThickness;

        Vector3 scale = new Vector3(scaleX, roadHeight, scaleZ);

        GameObject prefabToUse = roadPrefab; // default

        if (segment.height > segment.width)
        {
            prefabToUse = roadVerticalPrefab;  // assign in Inspector
        }
        else
        {
            prefabToUse = roadHorizontalPrefab; // assign in Inspector
        }

        var road = Instantiate(prefabToUse, position, Quaternion.identity, roadParent);
        road.transform.localScale = scale;
    }

    void CreateCrosswalk(Vector2Int pos)
    {
        if (crosswalkPrefab == null) return;

        Vector3 position = new Vector3(pos.x + 0.5f, 0.01f, pos.y + 0.5f);
        var crosswalk = Instantiate(crosswalkPrefab, position, Quaternion.identity, crosswalkParent);
        crosswalk.transform.localScale = new Vector3(2, 0.1f, 2); // visual marker only
    }

    void CreateOuterRoads(RectInt bounds)
    {
        int thickness = 1;

        // left
        CreateRoadSegment(new RectInt(bounds.xMin - thickness, bounds.yMin, thickness, bounds.height));
        // right
        CreateRoadSegment(new RectInt(bounds.xMax, bounds.yMin, thickness, bounds.height));
        // bottom
        CreateRoadSegment(new RectInt(bounds.xMin - thickness, bounds.yMin - thickness, bounds.width + 2 * thickness, thickness));
        // top
        CreateRoadSegment(new RectInt(bounds.xMin - thickness, bounds.yMax, bounds.width + 2 * thickness, thickness));
    }
    
    public void ClearAll()
    {
        if (roadParent != null)
            DestroyImmediate(roadParent.gameObject);

        if (crosswalkParent != null)
            DestroyImmediate(crosswalkParent.gameObject);

        roadParent = null;
        crosswalkParent = null;

        allBlocks.Clear();
        crosswalks.Clear();
    }
}
