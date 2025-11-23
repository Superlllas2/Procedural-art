using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a Soviet panel style block with symmetric sections, deterministic balconies,
/// and roof detailing. Long façades are assembled from configurable section widths while
/// short façades remain simple three-cell walls/windows.
/// </summary>
public class SovietPanelBuildingGenerator : MonoBehaviour
{
    public enum CellType
    {
        Wall,
        Window,
        Entrance,
        Stairwell,
        Balcony
    }

    [Serializable]
    public class PrefabFamily
    {
        [Tooltip("Variants to choose from when placing this cell type.")]
        public GameObject[] prefabs;

        public GameObject Choose(System.Random random)
        {
            if (prefabs == null || prefabs.Length == 0)
                return null;
            return prefabs[random.Next(prefabs.Length)];
        }

        public GameObject FirstOrDefault()
        {
            if (prefabs == null || prefabs.Length == 0)
                return null;
            return prefabs[0];
        }
    }

    [Serializable]
    public class RoofPrefabs
    {
        [Tooltip("Flat roof module used for filler cells.")]
        public GameObject flat;
        [Tooltip("Ramp module placed at the ends of the long façades, rotated to face inward.")]
        public GameObject rampInward;
        [Tooltip("Parapet module used only on the short façades.")]
        public GameObject parapet;
    }

    [Header("Layout")]
    [Tooltip("Number of floors for the block (valid options: 3, 4, 5, 9)."), Min(3)]
    public int floors = 5;
    [Tooltip("Number of long façade sections to build."), Min(1)]
    public int sectionCount = 3;
    [Tooltip("Width for each section. Use 3 or 5 to mirror real panel modules.")]
    public int[] sectionWidths = new[] { 5, 3, 5 };
    [Tooltip("Grid spacing applied along X and Z when placing modules.")]
    public float gridSize = 2.5f;
    [Tooltip("Vertical distance between floors.")]
    public float floorHeight = 3f;
    [Tooltip("Uniform scale multiplier applied to the generated building.")]
    [Min(0.01f)]
    public float scale = 1f;

    [Header("Randomness")]
    [Tooltip("When enabled, uses the provided seed for deterministic balcony/variant selection.")]
    public bool useSeed = false;
    public int seed = 12345;

    [Header("Prefabs")]
    [Tooltip("Basement modules placed at ground level to lift the building.")]
    public PrefabFamily foundationPrefabs;
    public PrefabFamily entrancePrefabs;
    public PrefabFamily wallPrefabs;
    public PrefabFamily windowPrefabs;
    public PrefabFamily stairwellPrefabs;
    public PrefabFamily balconyPrefabs;
    public RoofPrefabs roofPrefabs;

    [Header("Placement Tweaks")]
    [Tooltip("Applies an extra yaw to every module to align models that are authored facing -X.")]
    public float moduleRotationOffset = 90f;
    [Tooltip("When disabled, the entrance column is left without a foundation so the doorway reaches the ground.")]
    public bool includeFoundationUnderEntrances = false;

    const int shortFacadeDepth = 3;

    readonly List<SectionLayout> sectionLayouts = new List<SectionLayout>();
    readonly Dictionary<GameObject, float> bottomOffsetCache = new Dictionary<GameObject, float>();
    readonly Dictionary<GameObject, float> heightCache = new Dictionary<GameObject, float>();

    class SectionLayout
    {
        public int width;
        public bool balconiesEnabled;
    }

    public void ClearExisting()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    void OnValidate()
    {
        floors = SnapToAllowedFloor(floors);
        sectionCount = Mathf.Max(1, sectionCount);
        gridSize = Mathf.Max(0.1f, gridSize);
        floorHeight = Mathf.Max(0.1f, floorHeight);
        scale = Mathf.Max(0.01f, scale);
    }

    public void Generate()
    {
        ClearExisting();
        System.Random random = useSeed ? new System.Random(seed) : new System.Random();
        BuildLayouts(random);

        float zFront = 0f;
        float zBack = (shortFacadeDepth - 1f) * gridSize;
        float xLeft = -gridSize * 0.5f;
        float xRight = (TotalWidth() - 0.5f) * gridSize;
        float roofY = floors * floorHeight;

        Transform parent = CreateRootParent();
        Transform facadeParent = CreateChild(parent, "Facades");

        parent.localScale = Vector3.one * scale;

        BuildLongFacade(facadeParent, true, zFront, random);
        BuildLongFacade(facadeParent, false, zBack, random);
        BuildShortFacade(facadeParent, true, xLeft, zFront, zBack, random);
        BuildShortFacade(facadeParent, false, xRight, zFront, zBack, random);

        BuildRoof(parent, xLeft, xRight, zFront, zBack, roofY, random);
    }

    Transform CreateRootParent()
    {
        string rootName = "GeneratedPanel";
        Transform existing = transform.Find(rootName);
        if (existing != null)
            return existing;

        GameObject go = new GameObject(rootName);
        go.transform.SetParent(transform, false);
        return go.transform;
    }

    Transform CreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            for (int i = existing.childCount - 1; i >= 0; i--)
                DestroyImmediate(existing.GetChild(i).gameObject);
            return existing;
        }

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    void BuildLayouts(System.Random random)
    {
        sectionLayouts.Clear();
        if (sectionWidths == null || sectionWidths.Length == 0)
            return;

        for (int i = 0; i < sectionCount; i++)
        {
            int width = sectionWidths[Mathf.Clamp(i, 0, sectionWidths.Length - 1)];
            width = width == 3 ? 3 : 5;
            SectionLayout layout = new SectionLayout
            {
                width = width,
                balconiesEnabled = width == 5 && random.NextDouble() > 0.4
            };
            sectionLayouts.Add(layout);
        }
    }

    void BuildLongFacade(Transform parent, bool isFront, float zPos, System.Random random)
    {
        float xOffset = 0f;
        foreach (SectionLayout layout in sectionLayouts)
        {
            for (int localX = 0; localX < layout.width; localX++)
            {
                CellType groundCellType = DetermineCellType(layout, localX, 0);
                Quaternion foundationRot = GetLongFacadeRotation(isFront);
                bool placedFoundation = TryPlaceFoundation(parent, groundCellType, new Vector3((xOffset + localX) * gridSize, 0f, zPos), foundationRot, random);

                for (int floorIndex = 0; floorIndex < floors; floorIndex++)
                {
                    CellType cellType = DetermineCellType(layout, localX, floorIndex);
                    GameObject prefab = ChoosePrefab(cellType, layout, floorIndex, random);
                    if (prefab == null)
                        continue;

                    Vector3 bottom = GetCellBottomPosition(prefab, cellType, floorIndex, placedFoundation, new Vector3((xOffset + localX) * gridSize, 0f, zPos));

                    Quaternion rot = GetLongFacadeRotation(isFront);
                    InstantiateAligned(prefab, bottom, ApplyRotation(rot), parent);
                }
            }

            xOffset += layout.width;
        }
    }

    Quaternion GetLongFacadeRotation(bool isFront)
    {
        return isFront ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
    }

    void BuildShortFacade(Transform parent, bool isLeft, float xPos, float zFront, float zBack, System.Random random)
    {
        Quaternion rotation = isLeft ? Quaternion.Euler(0f, -90f, 0f) : Quaternion.Euler(0f, 90f, 0f);

        for (int floorIndex = 0; floorIndex < floors; floorIndex++)
        {
            for (int depthIndex = 0; depthIndex < shortFacadeDepth; depthIndex++)
            {
                float zPos = Mathf.Lerp(zFront, zBack, depthIndex / (float)(shortFacadeDepth - 1));
                bool isEdge = depthIndex == 0 || depthIndex == (shortFacadeDepth - 1);
                CellType cellType = isEdge ? CellType.Wall : CellType.Window;
                GameObject prefab = ChoosePrefab(cellType, null, floorIndex, random);
                if (prefab == null)
                    continue;

                bool placedFoundation = false;
                if (floorIndex == 0)
                {
                    CellType groundCellType = cellType;
                    placedFoundation = TryPlaceFoundation(parent, groundCellType, new Vector3(xPos, 0f, zPos), rotation, random);
                }

                Vector3 bottom = GetCellBottomPosition(prefab, cellType, floorIndex, placedFoundation, new Vector3(xPos, 0f, zPos));
                InstantiateAligned(prefab, bottom, ApplyRotation(rotation), parent);
            }
        }
    }

    void BuildRoof(Transform parent, float xLeft, float xRight, float zFront, float zBack, float roofY, System.Random random)
    {
        if (roofPrefabs == null)
            return;

        Transform roofParent = CreateChild(parent, "Roof");
        float totalWidth = TotalWidth();

        for (int facade = 0; facade < 2; facade++)
        {
            bool isFront = facade == 0;
            float zPos = isFront ? zFront : zBack;
            Quaternion inwardRotation = isFront ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);

            for (int xIndex = 0; xIndex < totalWidth; xIndex++)
            {
                GameObject prefabToUse;
                if (xIndex == 0 || xIndex == totalWidth - 1)
                    prefabToUse = roofPrefabs.rampInward;
                else
                    prefabToUse = roofPrefabs.flat;

                if (prefabToUse == null)
                    continue;

                Vector3 bottom = new Vector3(xIndex * gridSize, roofY, zPos);
                InstantiateAligned(prefabToUse, bottom, ApplyRotation(inwardRotation), roofParent);
            }
        }

        if (roofPrefabs.parapet != null)
        {
            Quaternion leftRot = Quaternion.Euler(0f, -90f, 0f);
            Quaternion rightRot = Quaternion.Euler(0f, 90f, 0f);
            for (int depthIndex = 0; depthIndex < shortFacadeDepth; depthIndex++)
            {
                float zPos = Mathf.Lerp(zFront, zBack, depthIndex / (float)(shortFacadeDepth - 1));
                Vector3 leftPos = new Vector3(xLeft, roofY, zPos);
                Vector3 rightPos = new Vector3(xRight, roofY, zPos);

                InstantiateAligned(roofPrefabs.parapet, leftPos, ApplyRotation(leftRot), roofParent);
                InstantiateAligned(roofPrefabs.parapet, rightPos, ApplyRotation(rightRot), roofParent);
            }
        }
    }

    CellType DetermineCellType(SectionLayout layout, int localX, int floorIndex)
    {
        bool isEdge = localX == 0 || localX == layout.width - 1;
        int center = layout.width / 2;

        if (isEdge)
            return CellType.Wall;

        if (localX == center)
            return floorIndex == 0 ? CellType.Entrance : CellType.Stairwell;

        if (layout.width == 3)
            return CellType.Wall;

        bool useBalcony = layout.balconiesEnabled && floorIndex >= 1;
        return useBalcony ? CellType.Balcony : CellType.Window;
    }

    GameObject ChoosePrefab(CellType type, SectionLayout layout, int floorIndex, System.Random random)
    {
        switch (type)
        {
            case CellType.Entrance:
                return entrancePrefabs?.Choose(random);
            case CellType.Stairwell:
                return stairwellPrefabs?.Choose(random) ?? windowPrefabs?.Choose(random);
            case CellType.Balcony:
                if (floorIndex == 0)
                    return windowPrefabs?.Choose(random);
                return balconyPrefabs?.Choose(random) ?? windowPrefabs?.Choose(random);
            case CellType.Window:
                return windowPrefabs?.Choose(random) ?? wallPrefabs?.Choose(random);
            default:
                return wallPrefabs?.Choose(random);
        }
    }

    float TotalWidth()
    {
        float width = 0f;
        foreach (SectionLayout layout in sectionLayouts)
            width += layout.width;
        return Mathf.Max(1f, width);
    }

    int SnapToAllowedFloor(int value)
    {
        int[] allowed = { 3, 4, 5, 9 };
        int closest = allowed[0];
        int bestDistance = Math.Abs(value - closest);
        for (int i = 1; i < allowed.Length; i++)
        {
            int distance = Math.Abs(value - allowed[i]);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = allowed[i];
            }
        }

        return closest;
    }

    Quaternion ApplyRotation(Quaternion baseRotation)
    {
        return Quaternion.Euler(0f, moduleRotationOffset, 0f) * baseRotation;
    }

    bool TryPlaceFoundation(Transform parent, CellType groundCellType, Vector3 bottom, Quaternion rotation, System.Random random)
    {
        GameObject prefab = foundationPrefabs?.Choose(random) ?? foundationPrefabs?.FirstOrDefault();
        if (prefab == null)
            return false;

        if (!includeFoundationUnderEntrances && groundCellType == CellType.Entrance)
            return false;

        InstantiateAligned(prefab, bottom, ApplyRotation(rotation), parent);
        return true;
    }

    void InstantiateAligned(GameObject prefab, Vector3 desiredBottom, Quaternion rotation, Transform parent)
    {
        float offsetY = GetBottomOffset(prefab);
        Vector3 position = desiredBottom + new Vector3(0f, offsetY, 0f);
        Instantiate(prefab, position, rotation, parent);
    }

    float GetBottomOffset(GameObject prefab)
    {
        if (prefab == null)
            return 0f;

        if (bottomOffsetCache.TryGetValue(prefab, out float cached))
            return cached;

        Bounds bounds = CalculateBounds(prefab);
        float offset = bounds.size == Vector3.zero ? 0f : -bounds.min.y;
        bottomOffsetCache[prefab] = offset;
        return offset;
    }

    Vector3 GetCellBottomPosition(GameObject prefab, CellType cellType, int floorIndex, bool hasFoundation, Vector3 basePosition)
    {
        float foundationHeight = GetFoundationHeight();
        float y = foundationHeight + floorIndex * floorHeight;

        if (floorIndex == 0)
        {
            if (!hasFoundation)
                y -= foundationHeight;

            if (cellType == CellType.Entrance)
            {
                float entranceHeight = GetPrefabHeight(prefab);
                float alignedTop = foundationHeight + floorHeight;
                y = alignedTop - entranceHeight;

                if (!hasFoundation)
                    y -= foundationHeight;
            }
        }

        return new Vector3(basePosition.x, y, basePosition.z);
    }

    float GetFoundationHeight()
    {
        return GetFamilyHeight(foundationPrefabs);
    }

    float GetFamilyHeight(PrefabFamily family)
    {
        GameObject prefab = family?.FirstOrDefault();
        return GetPrefabHeight(prefab);
    }

    float GetPrefabHeight(GameObject prefab)
    {
        if (prefab == null)
            return 0f;

        if (heightCache.TryGetValue(prefab, out float cached))
            return cached;

        Bounds bounds = CalculateBounds(prefab);
        float height = bounds.size.y;
        heightCache[prefab] = height;
        return height;
    }

    Bounds CalculateBounds(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

#if UNITY_EDITOR
    [ContextMenu("Apply Sample Prefab Mapping")]
    void ApplySampleMapping()
    {
        string basePath = "Assets/SovietHousing/";
        foundationPrefabs = LoadFamily(basePath + "panel_fund1.fbx", basePath + "panel_fund2.fbx");
        entrancePrefabs = LoadFamily(basePath + "panel_doorway.fbx");
        wallPrefabs = LoadFamily(basePath + "panel_wall1.fbx", basePath + "panel_wall2.fbx", basePath + "panel_wall3.fbx");
        windowPrefabs = LoadFamily(basePath + "panel_wall5.fbx", basePath + "panel_wall6.fbx");
        stairwellPrefabs = LoadFamily(basePath + "panel_wall7.fbx");
        balconyPrefabs = LoadFamily(basePath + "panel_balcony1.fbx", basePath + "panel_balcony2.fbx");
        roofPrefabs = new RoofPrefabs
        {
            flat = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "panel_roof1.fbx"),
            rampInward = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "panel_roof3.fbx"),
            parapet = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "panel_roof2.fbx"),
        };

        UnityEditor.EditorUtility.SetDirty(this);
    }

    PrefabFamily LoadFamily(params string[] paths)
    {
        var family = new PrefabFamily { prefabs = new GameObject[paths.Length] };
        for (int i = 0; i < paths.Length; i++)
        {
            family.prefabs[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(paths[i]);
        }
        return family;
    }
#endif
}
