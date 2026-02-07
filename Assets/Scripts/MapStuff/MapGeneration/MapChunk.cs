using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapChunk
{
    public Vector2 center_position;
    public Vector2Int position;
    public int dist_from_final = -1;

    public Vector2Int[] neighbor_chunks {get; private set;} // get initialized in map manager

    public MapChunk(Vector2Int pos)
    {
        position = pos;
        center_position = new Vector2(pos.x + ((float)MapManager.chunk_size)/2, pos.y + ((float)MapManager.chunk_size)/2);
    }

    public void SetNeighbors(List<Vector2Int> directions)
    {
        neighbor_chunks = directions.ToArray();
    }
}