using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapPreset", menuName = "ScriptableObjects/Map Generation/Generation Preset", order = 1)]
public class MapGenPresetSO : ScriptableObject
{
    public enum AdjacentType {four_directions, eight_directions};
    [Range(10, 300)] public int map_size = 40;
    [Range(1f, 20f)] public int map_scale = 1;
    [Range(1, 24)]public int objectives = 3;
    
    public int minor_poi = 7; // extra goodies that might appear on the map

    public MapGenPreset CreateNewPreset()
    {
        return new MapGenPreset(map_size, map_scale, objectives, minor_poi);
    }
}