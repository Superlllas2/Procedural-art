using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SovietPanelBuildingGenerator))]
public class SovietPanelBuildingGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Building"))
        {
            foreach (var t in targets)
            {
                SovietPanelBuildingGenerator generator = (SovietPanelBuildingGenerator)t;
                generator.Generate();
            }
        }

        if (GUILayout.Button("Clear Generated"))
        {
            foreach (var t in targets)
            {
                SovietPanelBuildingGenerator generator = (SovietPanelBuildingGenerator)t;
                generator.ClearExisting();
            }
        }
    }
}
