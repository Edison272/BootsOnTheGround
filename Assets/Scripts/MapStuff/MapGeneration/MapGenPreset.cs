using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapGenPreset
{
    public enum AdjacentType { four_directions, eight_directions };
    [Range(10, 300)] public int map_size = 40;
    [Range(1f, 20f)] public int map_scale = 1;
    [Range(1, 24)] public int objectives = 3;

    public int minor_poi = 7; // extra goodies that might appear on the map

    public MapGenPreset(int map_size, int map_scale, int objectives, int minor_poi)
    {
        this.map_size = map_size;
        this.map_scale = map_scale;
        this.objectives = objectives;
        this.minor_poi = minor_poi;
    }

    public void IncrementGenPresets(int incrementation)
    {
        map_size += incrementation * 5;
        map_scale += (int)(incrementation / 4);
        objectives += (int)(incrementation / 2);
        minor_poi += (int)(incrementation / 2);
    }
}
