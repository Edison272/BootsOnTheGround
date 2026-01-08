using UnityEngine;
using System.Collections.Generic;

public class MapChunk
{
    Vector2Int position;
    public int dist_from_final = -1;

    public MapChunk(Vector2Int pos)
    {
        position = pos;
    }
}