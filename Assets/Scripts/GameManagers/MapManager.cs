using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

using Random = UnityEngine.Random; // LEAVE ME ALONE DAMNIT
public class MapManager : MonoBehaviour
{
    public Tilemap Floor;
    public Tilemap Wall;
    public MapGenPreset gen_preset;

    [SerializeField] TileBase floor_tile;
    [SerializeField] TileBase wall_tile;

    [Header("ProcGen Stuff")]
    [SerializeField] MapMakerType map_maker_type;
    [SerializeField] MapMaker map_maker;

    [Header("ProcGen Stuff")]
    public static readonly int chunk_size = 4; // each chunk is 12z minimap tiles
    public static readonly int border_width = 4; // each chunk is 12z minimap tiles
    public static readonly Vector2Int START_POS = Vector2Int.zero;
    // each chunk expands to the TOP RIGHT when it becomes a 2D area instead of a single point
    public Dictionary<Vector2Int, MapChunk> all_chunks = new Dictionary<Vector2Int, MapChunk>();
    private HashSet<Vector2Int> border_chunks = new HashSet<Vector2Int>(); // chunks where the map is no go
    private HashSet<Vector2Int> path_chunks = new HashSet<Vector2Int>(); // chunks containing the paths between POI

    // important chunks/positions
    public Vector2Int spawn_chunk; // where the player spawns in
    public Vector2Int final_chunk; // chunk with the main objective
    public Vector2 map_center; // duh

    public MapChunk[] critical_locs = new MapChunk[0]; // start & final + POI

    // format is chunk, chunk + 1,0 , chunk + 1,1 , chunk + 0,1
    HashSet<Vector3Int> draw_ground = new HashSet<Vector3Int>();
    HashSet<Vector3Int> draw_border = new HashSet<Vector3Int>();

    [Header("Gizmo Stuff")]
    [SerializeField] bool show_chunks = true;
    [SerializeField] bool show_border_chunks = true;
    [SerializeField] bool show_critical_chunks = true;
    [SerializeField] bool show_quads = true;

    public void GenerateMap()
    {
        switch(map_maker_type)
        {
            case MapMakerType.Blob:
                map_maker = new BlobMaker();
                break;
            case MapMakerType.Level:
                map_maker = new LevelMaker();
                break;
        }
        GenerateChunks();
        GeneratePOI();
        DrawMap();
        Wall.CompressBounds();
        Floor.CompressBounds();
    }

    // Update is called once per frame


    #region Generate Chunks
    private void GenerateChunks() // generate the chunks and declare the start & final pos
    {
        // initialize data holders
        all_chunks.Clear();
        border_chunks.Clear();
        critical_locs = new MapChunk[1 + gen_preset.objectives];

        map_center = map_maker.GenerateMap(all_chunks, border_chunks, path_chunks, critical_locs, gen_preset);
        spawn_chunk = critical_locs[0].position;
        final_chunk = critical_locs[critical_locs.Length-1].position;
    }
    #endregion

    #region Generate POI
    private void GeneratePOI() // generate potential POI based off of chunks in the map
    {
        map_maker.GeneratePOI(all_chunks, critical_locs);
    }
#endregion

    private void Pathfind() // declare pathway chunks between all POI
    {
        
    }

#region Draw Map
    private void DrawMap() // put tiles on the map ts
    {
        Floor.ClearAllTiles();
        Wall.ClearAllTiles();

        draw_ground.Clear();
        draw_border.Clear();

        foreach(Vector2Int pos in all_chunks.Keys)
        {
            Vector2Int chunk_vec =  pos * chunk_size;
            int size_iter = chunk_size/2 + border_width;
            for (int x = -size_iter; x < size_iter; x++)
            {
                for (int y = -size_iter; y < size_iter; y++)
                {
                    Vector2Int draw_offset = new Vector2Int(x, y);
                    if (Mathf.Abs(x) <= (int)chunk_size/2 && Mathf.Abs(y) <= (int)chunk_size/2)
                    {
                        draw_ground.Add((Vector3Int)(draw_offset + chunk_vec));
                    }
                    else
                    {
                        draw_border.Add((Vector3Int)(draw_offset + chunk_vec));
                    }
                    
                }
            }    
        
            // foreach(Vector2Int dir in all_chunks[pos].neighbor_chunks)
            // {
            //     for (int i = 0; i <= chunk_size/2; i++)
            //     {
            //         Vector2Int chunk_vec = pos * chunk_size + dir * i;
            //         int size_iter = chunk_size/2 + border_width;
            //         for (int x = -size_iter; x < size_iter; x++)
            //         {
            //             for (int y = -size_iter; y < size_iter; y++)
            //             {
            //                 Vector2Int draw_offset = new Vector2Int(x, y);
            //                 if (Mathf.Abs(x) <= chunk_size/2 && Mathf.Abs(y) <= chunk_size/2)
            //                 {
            //                     draw_ground.Add((Vector3Int)(draw_offset + chunk_vec));
            //                 }
            //                 else
            //                 {
            //                     draw_border.Add((Vector3Int)(draw_offset + chunk_vec));
            //                 }
                            
            //             }
            //         }
            //     }
            // }
        }

        foreach(Vector3Int chunk in draw_ground)
        {
            Floor.SetTile(chunk, floor_tile);
        }
        foreach(Vector3Int chunk in draw_border)
        {
            if (!Floor.GetTile(chunk))
            {
                Wall.SetTile(chunk, wall_tile);
            }
            
        }
    }
#endregion

#region Tools 
    public Vector2 GetChunkWorldPos(Vector2Int chunk)
    {
        Vector2 pos = Vector2.zero;
        if (all_chunks.ContainsKey(chunk))
        {
            pos = all_chunks[chunk].position * chunk_size;
        }
        return pos;
    }
    private void DrawChunk(Vector2Int chunk_pos, Color line_color)
    {
        // Square Shape
        Debug.DrawLine(
            (chunk_pos + Vector2.up*0.5f + Vector2.right*0.5f) * chunk_size, 
            (chunk_pos + Vector2.up*0.5f + Vector2.left*0.5f) * chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (chunk_pos + Vector2.up*0.5f + Vector2.left*0.5f) * chunk_size, 
            (chunk_pos + Vector2.down*0.5f + Vector2.left*0.5f) * chunk_size, 
            line_color 
            );
        Debug.DrawLine(
            (chunk_pos + Vector2.down*0.5f + Vector2.left*0.5f) * chunk_size, 
            (chunk_pos + Vector2.down*0.5f + Vector2.right*0.5f) * chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (chunk_pos + Vector2.down*0.5f + Vector2.right*0.5f) * chunk_size, 
            (chunk_pos + Vector2.up*0.5f + Vector2.right*0.5f) * chunk_size, 
            line_color
            );
        // Crosses
        Debug.DrawLine(
            (chunk_pos + Vector2.up*0.5f + Vector2.right*0.5f) * chunk_size, 
            (chunk_pos + Vector2.down*0.5f + Vector2.left*0.5f) * chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (chunk_pos + Vector2.up*0.5f + Vector2.left*0.5f) * chunk_size, 
            (chunk_pos + Vector2.down*0.5f + Vector2.right*0.5f) * chunk_size, 
            line_color
            );
    }

    public void DrawQuad((Vector2Int, Vector2Int, Vector2Int, Vector2Int) quad, Color line_color)
    {
        // straights
        // Debug.DrawLine((Vector2)quad.Item1 * chunk_size, (Vector2)(quad.Item2 + Vector2Int.right) * chunk_size, line_color);
        // Debug.DrawLine((Vector2)(quad.Item2 + Vector2Int.right) * chunk_size, (Vector2)(quad.Item3 + Vector2Int.up + Vector2Int.right) * chunk_size, line_color);
        // Debug.DrawLine((Vector2)(quad.Item3 + Vector2Int.up + Vector2Int.right) * chunk_size, (Vector2)(quad.Item4 + Vector2Int.up) * chunk_size, line_color);
        // Debug.DrawLine((Vector2)(quad.Item4 + Vector2Int.up) * chunk_size, (Vector2)quad.Item1 * chunk_size, line_color);

        Debug.DrawLine((Vector2)quad.Item1 * chunk_size, (Vector2)(quad.Item2) * chunk_size, line_color);
        Debug.DrawLine((Vector2)(quad.Item2) * chunk_size, (Vector2)(quad.Item3) * chunk_size, line_color);
        Debug.DrawLine((Vector2)(quad.Item3) * chunk_size, (Vector2)(quad.Item4) * chunk_size, line_color);
        Debug.DrawLine((Vector2)(quad.Item4) * chunk_size, (Vector2)quad.Item1 * chunk_size, line_color);

        Debug.DrawLine((quad.Item1 + Vector2.down*0.5f + Vector2.left*0.5f) * chunk_size, (quad.Item2 + Vector2.down*0.5f + Vector2.right*0.5f) * chunk_size, line_color);
        Debug.DrawLine((quad.Item2 + Vector2.down*0.5f + Vector2.right*0.5f) * chunk_size, (quad.Item3 + Vector2.up*0.5f + Vector2.right*0.5f) * chunk_size, line_color);
        Debug.DrawLine((quad.Item3 + Vector2.up*0.5f + Vector2.right*0.5f) * chunk_size, (quad.Item4 + Vector2.up*0.5f + Vector2.left*0.5f) * chunk_size, line_color);
        Debug.DrawLine((quad.Item4 + Vector2.up*0.5f + Vector2.left*0.5f) * chunk_size, (quad.Item1 + Vector2.down*0.5f + Vector2.left*0.5f) * chunk_size, line_color);
    }

    public static void DrawStar(Vector2 point, Color line_color, float duration = 0.01f)
    {
        foreach (Vector2Int dir in Directions2D.eight_directions)
        {
            Debug.DrawLine(point * chunk_size, point * chunk_size + dir * chunk_size/4, line_color, duration);
        }
    }

    void OnDrawGizmos()
    {
        if (show_chunks)
        {
            foreach (Vector2Int chunk in all_chunks.Keys)
            {
                DrawChunk(chunk, Color.white);
            }
        }
        if (show_border_chunks)
        {
            foreach (Vector2Int chunk in border_chunks)
            {
                DrawChunk(chunk, Color.black);
            }
        }
        if (show_critical_chunks)
        {
            DrawChunk(START_POS, Color.black);
            DrawChunk(final_chunk, Color.red);
            DrawChunk(spawn_chunk, Color.green);
            for (int i = 0; i < critical_locs.Length; i++)
            {
                MapChunk chunk = critical_locs[i];
                if (chunk.position != spawn_chunk && chunk.position != final_chunk)
                {
                    DrawChunk(chunk.position, Color.yellow);
                }
                if (i < critical_locs.Length-1)
                {
                    Debug.DrawLine((Vector2)chunk.position * chunk_size, (Vector2)critical_locs[i+1].position * chunk_size);
                }
                // foreach(MapChunk other_chunk in critical_locs)
                // {
                //     Debug.DrawLine((Vector2)chunk.position * chunk_size, (Vector2)other_chunk.position * chunk_size);
                // }
            }
        }
    }

    #endregion
}
