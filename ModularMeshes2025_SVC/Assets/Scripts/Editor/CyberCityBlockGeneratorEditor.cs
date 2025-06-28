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

        if (GUILayout.Button("Clear Blocks"))
        {
            generator.ClearBlocks();
        }

        if (GUILayout.Button("Generate Blocks"))
        {
            generator.GenerateBlocks();
        }
    }
}