using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Components.ProceduralGeneration.BiomeGeneration.BiomeGenerator))]
public class BiomeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var generator = (Components.ProceduralGeneration.BiomeGeneration.BiomeGenerator)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== DEBUG TOOLS ===", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Debug Maps"))
        {
            generator.GenerateDebugMapsEditor(); // Appelle la version publique
        }

        if (generator.TemperatureMap != null)
        {
            EditorGUILayout.LabelField("Temperature");
            GUILayout.Label(generator.TemperatureMap, GUILayout.Width(128), GUILayout.Height(128));
        }

        if (generator.MoistureMap != null)
        {
            EditorGUILayout.LabelField("Moisture");
            GUILayout.Label(generator.MoistureMap, GUILayout.Width(128), GUILayout.Height(128));
        }

        if (generator.HeightMap != null)
        {
            EditorGUILayout.LabelField("Height");
            GUILayout.Label(generator.HeightMap, GUILayout.Width(128), GUILayout.Height(128));
        }

        if (generator.BiomeMap != null)
        {
            EditorGUILayout.LabelField("Biome Result");
            GUILayout.Label(generator.BiomeMap, GUILayout.Width(256), GUILayout.Height(256));
        }

    }
}
