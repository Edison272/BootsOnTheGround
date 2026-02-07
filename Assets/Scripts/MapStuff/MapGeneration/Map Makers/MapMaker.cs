using System.Collections.Generic;
using UnityEngine;
enum MapMakerType {Blob, Level};
public abstract class MapMaker
{
    public abstract Vector2 GenerateMap(
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        Dictionary<Vector2Int, Vector2Int> adj_chunks, 
        MapChunk[] critical_locs,
        MapGenPreset gen_preset);

    public abstract void GeneratePOI(
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        MapChunk[] critical_locs
    );
}