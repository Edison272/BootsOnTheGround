using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

using Random = UnityEngine.Random; // LEAVE ME ALONE DAMNIT
public class MapManager : MonoBehaviour
{
    public Tilemap Floor;
    public Tilemap Wall;
    public MapGenPreset gen_preset;
    [SerializeField] TileBase wall_tile;
    [SerializeField] MapBiomePreset[] map_biome_pool = new MapBiomePreset[0];
    

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
    public MajorObjective[] critical_locs = new MajorObjective[0]; // start & final + POI
    public Dictionary<Character, int> chars_in_poi = new Dictionary<Character, int>();
    private List<Character> removal_buffer = new List<Character>();

    // format is chunk, chunk + 1,0 , chunk + 1,1 , chunk + 0,1
    HashSet<Vector3Int> draw_ground = new HashSet<Vector3Int>();
    HashSet<Vector3Int> draw_path = new HashSet<Vector3Int>();
    HashSet<Vector3Int> draw_border = new HashSet<Vector3Int>();

    [Header("Objective Point")]
    public CaptureArea objective_point_prefab;

    [Header("Gizmo Stuff")]
    [SerializeField] bool show_chunks = true;
    [SerializeField] bool show_border_chunks = true;
    [SerializeField] bool show_critical_chunks = true;
    [SerializeField] bool show_minor_poi = true;
    [SerializeField] bool show_start_dist_heatmap = true;
    [SerializeField] bool show_path_dist_heatmap = true;
    [SerializeField] bool show_poi_territories = true;

    public void Awake()
    {
        // Destroy POI made by map in editor. When editor destroys, it uses EditorDestroyMapObjects
        foreach(MajorObjective objective in critical_locs)
        {
            if (objective.objective_point)
            {
                Destroy(objective.objective_point.gameObject);
            }   
        }
    }
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
        path_chunks.Clear();
        critical_locs = new MajorObjective[1 + gen_preset.objectives];

        map_center = map_maker.GenerateMap(all_chunks, border_chunks, path_chunks, critical_locs, gen_preset);
        spawn_chunk = critical_locs[0].main_chunk.position;
        final_chunk = critical_locs[critical_locs.Length-1].main_chunk.position;
    }
    #endregion

    #region Generate POI
    private void GeneratePOI() // generate potential POI based off of chunks in the map
    {
        map_maker.GeneratePOI(all_chunks, critical_locs);
    }
#endregion

    private void HeatMap() // declare pathway chunks between all POI
    {
        
    }


#region Draw Map

    private void DrawMap() // put tiles on the map ts
    {
        Floor.ClearAllTiles();
        Wall.ClearAllTiles();

        foreach(MajorObjective major_objective in critical_locs)
        {
            draw_ground.Clear();
            draw_border.Clear();
            draw_path.Clear();
            
            MapBiomePreset rand_biome = map_biome_pool[Random.Range(0, map_biome_pool.Length)];

            foreach(MapChunk chunk in major_objective.territory_chunks)
            {
                Vector2Int pos = chunk.position;
                Vector2Int chunk_vec =  pos * chunk_size;
                for (int x = 0; x < chunk_size; x++)
                {
                    for (int y = 0; y < chunk_size; y++)
                    {
                        Vector2Int draw_offset = new Vector2Int(x, y);
                        if (path_chunks.Contains(pos))
                        {
                            draw_path.Add((Vector3Int)(draw_offset + chunk_vec));
                        }
                        else
                        {
                            draw_ground.Add((Vector3Int)(draw_offset + chunk_vec));
                        }
                        
                    }
                }    
            }

            foreach(Vector3Int chunk in draw_ground)
            {
                Floor.SetTile(chunk, rand_biome.BiomeTiles[Random.Range(0, rand_biome.BiomeTiles.Length)]);
            }
            foreach(Vector3Int chunk in draw_path)
            {
                Floor.SetTile(chunk, rand_biome.PathTiles[Random.Range(0, rand_biome.PathTiles.Length)]);
            }

            foreach (Vector2Int border_chunk in border_chunks)
            {
                Vector2Int chunk_vec =  border_chunk * chunk_size;
                for (int x = 0; x < chunk_size; x++)
                {
                    for (int y = 0; y < chunk_size; y++)
                    {
                        Vector2Int draw_offset = new Vector2Int(x, y);
                        draw_border.Add((Vector3Int)(draw_offset + chunk_vec));
                        
                    }
                }    
            }
            foreach(Vector3Int chunk in draw_border)
            {
                Wall.SetTile(chunk, wall_tile);
                // if (!Floor.GetTile(chunk))
                // {
                //     Wall.SetTile(chunk, wall_tile);
                // }
                
            }

        }

        // place Objective prefabs
        // for(int i = 1; i < critical_locs.Length; i++)
        // {
        //     MajorObjective maj_poi = critical_locs[i];
        //     Instantiate(objective_point_prefab, (Vector3)(Vector2)maj_poi.main_chunk.position * chunk_size, Quaternion.identity);
        // }
    }
#endregion

#region Update

    void Update()
    {
        if (chars_in_poi.Count > 0) {
            foreach (Character character in chars_in_poi.Keys)
            {
                MajorObjective poi = critical_locs[chars_in_poi[character]];
                if (!character.IsInAction())
                {
                    poi.objective_point.RemoveOccupier(character);
                    removal_buffer.Add(character);
                }
                else if (GetTileToChunkSpace(GetWorldToTileSpace(character.GetPosition())) != poi.main_chunk.position)
                {
                    poi.objective_point.RemoveOccupier(character);
                    removal_buffer.Add(character);
                }
            };
            foreach(Character character in removal_buffer)
            {
                chars_in_poi.Remove(character);
            }
            removal_buffer.Clear();
        }
    }


#endregion

#region Set/Get Loc Info
    private static Vector2Int GetWorldToTileSpace(Vector2 world_pos)
    {
        return new Vector2Int(
            (int)Mathf.Round(world_pos.x),
            (int)Mathf.Round(world_pos.y)
        );
    }
    public static Vector2 GetWorldToTileSpaceCenter(Vector2 world_pos)
    {
        return new Vector2Int(
            (int)Mathf.Round(world_pos.x),
            (int)Mathf.Round(world_pos.y)
        ) + new Vector2(0.5f, 0.5f);
    }
    public static Vector2Int GetTileToChunkSpace(Vector2Int tile_pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((float)tile_pos.x/chunk_size),
            Mathf.FloorToInt((float)tile_pos.y/chunk_size)
        );
    }
    public void SetNewPosition(Character character)
    {
        Vector2Int tile_pos = GetWorldToTileSpace(character.GetPosition());
        character.current_tile_pos = tile_pos;
        Vector2Int chunk_location = GetTileToChunkSpace(tile_pos);
        DrawTile(tile_pos, Color.white);
        DrawChunk(chunk_location, Color.cyan);

        // check for chunk interactions
        for(int i = 0; i< critical_locs.Length; i++)
        {
            MajorObjective poi = critical_locs[i];
            if (chunk_location == poi.main_chunk.position && !chars_in_poi.ContainsKey(character))
            {
                poi.objective_point.AddOccupier(character);
                chars_in_poi[character] = i;
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

    private void DrawTile(Vector2Int pos, Color line_color)
    {
        Debug.DrawLine(
            (Vector2)pos, 
            (Vector2)(pos + Directions2D.four_directions[1]), 
            line_color
            );
        Debug.DrawLine(
            (Vector2)(pos + Directions2D.four_directions[1]), 
            (Vector2)(pos + Directions2D.four_directions[1] + Directions2D.four_directions[0]), 
            line_color 
            );
        Debug.DrawLine(
            (Vector2)pos, 
            (Vector2)(pos + Directions2D.four_directions[0]), 
            line_color
            );
        Debug.DrawLine(
            (Vector2)(pos + Directions2D.four_directions[0]), 
            (Vector2)(pos + Directions2D.four_directions[0] + Directions2D.four_directions[1]), 
            line_color
            );
        // Crosses
        Debug.DrawLine(
            (Vector2)(pos), 
            (Vector2)(pos + Directions2D.four_directions[0] + Directions2D.four_directions[1]), 
            line_color
            );
        Debug.DrawLine(
            (Vector2)(pos + Directions2D.four_directions[0]), 
            (Vector2)(pos + Directions2D.four_directions[1]), 
            line_color
            );
    }
    private void DrawChunk(Vector2Int chunk_pos, Color line_color, bool draw_cross = true)
    {
        // Square Shape
        Debug.DrawLine(
            (Vector2)chunk_pos * chunk_size, 
            (Vector2)(chunk_pos + Directions2D.four_directions[1]) * chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (Vector2)(chunk_pos + Directions2D.four_directions[1]) * chunk_size, 
            (Vector2)(chunk_pos + Directions2D.four_directions[1] + Directions2D.four_directions[0]) * chunk_size, 
            line_color 
            );
        Debug.DrawLine(
            (Vector2)chunk_pos * chunk_size, 
            (Vector2)(chunk_pos + Directions2D.four_directions[0]) * chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (Vector2)(chunk_pos + Directions2D.four_directions[0]) * chunk_size, 
            (Vector2)(chunk_pos + Directions2D.four_directions[0] + Directions2D.four_directions[1]) * chunk_size, 
            line_color
            );
        // Crosses
        if (draw_cross)
        {
            Debug.DrawLine(
                (Vector2)(chunk_pos) * chunk_size, 
                (Vector2)(chunk_pos + Directions2D.four_directions[0] + Directions2D.four_directions[1]) * chunk_size, 
                line_color
                );
            Debug.DrawLine(
                (Vector2)(chunk_pos + Directions2D.four_directions[0]) * chunk_size, 
                (Vector2)(chunk_pos + Directions2D.four_directions[1]) * chunk_size, 
                line_color
                );
        }
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

    // Destroy 
    public void EditorDestroyMapObjects()
    {
        foreach(MajorObjective objective in critical_locs)
        {
            if (objective.objective_point)
            {
                DestroyImmediate(objective.objective_point.gameObject);
            }
            
        }
    }
    #endregion
#region Gizmos Drawer
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
                MapChunk chunk = critical_locs[i].main_chunk;
                if (chunk.position != spawn_chunk && chunk.position != final_chunk)
                {
                    DrawChunk(chunk.position, Color.yellow);
                    if (show_minor_poi)
                    {
                        foreach(Vector2Int minor_poi in critical_locs[i].minor_poi)
                        {
                            Debug.DrawLine((Vector2)chunk.position * chunk_size, (Vector2)minor_poi * chunk_size);
                            DrawChunk(minor_poi, Color.cyan);
                        }
                    }
                }
                if (i < critical_locs.Length-1)
                {
                    Debug.DrawLine((Vector2)chunk.position * chunk_size, (Vector2)critical_locs[i+1].main_chunk.position * chunk_size);
                }
                // foreach(MapChunk other_chunk in critical_locs)
                // {
                //     Debug.DrawLine((Vector2)chunk.position * chunk_size, (Vector2)other_chunk.position * chunk_size);
                // }
            }
        }
        if (show_start_dist_heatmap)
        {
            foreach (MapChunk chunk in all_chunks.Values)
            {
                DrawChunk(chunk.position,Color.blue, false);
                //Handles.Label(chunk.world_center_position + new Vector2(0, chunk_size*0.2f), "from start: "+chunk.dist_from_start);
            }
        }
        if (show_path_dist_heatmap)
        {
            foreach (MapChunk chunk in all_chunks.Values)
            {
                DrawChunk(chunk.position,Color.clear, false);
                //Handles.Label(chunk.world_center_position + new Vector2(0, -chunk_size*0.2f), "from path: "+chunk.path_relevancy);
            }
        }
        if (show_poi_territories)
        {
            for (int i = 0; i < critical_locs.Length; i++)
            {
                Color color = i % 2 == 0 ? Color.black : Color.white;
                foreach(MapChunk chunk in critical_locs[i].territory_chunks)
                {
                    DrawChunk(chunk.position,color);
                }
            }
        }
    }

    #endregion
}
