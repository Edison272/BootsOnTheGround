using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "MapBiome", menuName = "ScriptableObjects/Map Generation/Biome Preset", order = 2)]
public class MapBiomePreset : ScriptableObject
{
    public TileBase[] BiomeTiles;
    public TileBase[] PathTiles;
    public GameObject[] SpawnableObjects;
}