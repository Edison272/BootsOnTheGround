using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapPreset", menuName = "ScriptableObjects/MapGeneration", order = 1)]
public class MapGenPreset : ScriptableObject
{
    public int map_chunks = 40;
    [Range(1, 8)] public int max_chunk_branching = 6;
    [Range(1, 8)] public int min_chunk_branching = 3;
    public int check_points = 3;
    public int places_of_interest = 7;

    public void OnValidate()
    {
        if (min_chunk_branching > max_chunk_branching)
        {
            min_chunk_branching = max_chunk_branching;
        }
    }
}