using GeneralScripts;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CityRoadGenerator))]
public class StreetGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var generator = (CityRoadGenerator)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Street Generation Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate City Roads"))
        {
            // restart generation
            generator.Start();
        }

        if (GUILayout.Button("Clear All"))
        {
            generator.ClearAll();
        }
    }
}