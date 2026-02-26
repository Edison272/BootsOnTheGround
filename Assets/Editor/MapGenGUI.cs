using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapManager))]
public class MapGenGUI : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Expand the 'Generation Presets' in order to adjust the settings for map generation");
        DrawDefaultInspector();
        MapManager map_gui = (MapManager)target;
        if (GUILayout.Button(" Generate the Map! "))
        {
            Repaint();
            map_gui.EditorDestroyMapObjects();
            map_gui.GenerateMap();
        }
    }
}