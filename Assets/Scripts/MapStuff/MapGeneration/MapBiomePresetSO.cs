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
    public MinorPOI[] MinorPOIs;

    public MinorPOI SetMinorPOI(Vector2Int chunk)
    {
        if (MinorPOIs.Length == 0)
        {
            return null;
        }

        Vector2 chunk_center = MapManager.GetChunkWorldCenter(chunk);
        MinorPOI new_minor_poi = MinorPOIs[Random.Range(0,MinorPOIs.Length)].GetNewMinorPOI(chunk_center);
        
        return new_minor_poi;
    }
}