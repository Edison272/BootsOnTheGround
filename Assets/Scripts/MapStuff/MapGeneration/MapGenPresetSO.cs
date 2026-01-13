using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapPreset", menuName = "ScriptableObjects/MapGeneration", order = 1)]
public class MapGenPreset : ScriptableObject
{
    public enum AdjacentType {four_directions, eight_directions};
    public int map_chunks = 40;
    [Range(1, 8)] public int max_chunk_branching = 6;
    private int og_max = 6;
    [Range(1, 8)] public int min_chunk_branching = 3;
    [SerializeField] AdjacentType adj_type = AdjacentType.four_directions;
    public bool four_adj_tiles = true;

    public int check_points = 3;
    public int objectives = 3;
    public int minor_poi = 7; // extra goodies that might appear on the map

    public void OnValidate()
    {
        og_max = max_chunk_branching;
        if (adj_type == AdjacentType.four_directions)
        {
            four_adj_tiles = true;
            if (max_chunk_branching > 4) // chang max branching in editor so users don't think that max_branching of 8 does anything in a four_direction adjacency brancher
            {
                max_chunk_branching = 4;
            }
        } else
        {
            four_adj_tiles = false;
            max_chunk_branching = og_max;
        }
        if (min_chunk_branching > max_chunk_branching)
        {
            min_chunk_branching = max_chunk_branching;
        }


    }
}