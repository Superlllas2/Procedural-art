using GeneralScripts;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(CyberCityBlockGenerator))]
public class CyberCityBlockGeneratorEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var generator = (CyberCityBlockGenerator)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Generation Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Clear Scene"))
        {
            generator.ClearScene();
        }

        if (GUILayout.Button("Generate Roads"))
        {
            generator.GenerateRoads();
        }

        if (GUILayout.Button("Generate Town"))
        {
            generator.GenerateTown();
        }
    }
}