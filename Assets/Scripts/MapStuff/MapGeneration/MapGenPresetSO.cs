using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapPreset", menuName = "ScriptableObjects/MapGeneration", order = 1)]
public class MapGenPreset : ScriptableObject
{
    public enum AdjacentType {four_directions, eight_directions};
    [Range(10, 300)] public int map_size = 40;
    [Range(1f, 20f)] public int map_scale = 1;
    public int check_points = 3;
    [Range(1, 24)]public int objectives = 3;
    public int minor_poi = 7; // extra goodies that might appear on the map
}