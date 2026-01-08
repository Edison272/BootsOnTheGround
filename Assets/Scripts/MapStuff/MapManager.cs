using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField] Tilemap Floor;
    [SerializeField] Tilemap Wall;
    public MapGenPreset gen_preset;

    [SerializeField] TileBase floor_tile;
    [SerializeField] TileBase wall_tile;

    [Header("ProcGen Stuff")]
    public static readonly int chunk_size = 16; // each chunk is 16 minimap tiles
    public static readonly Vector2Int START_POS = Vector2Int.zero;
    public Dictionary<Vector2Int, MapChunk> all_chunks = new Dictionary<Vector2Int, MapChunk>();
    private Dictionary<Vector2Int, Vector2Int> adj_chunks = new Dictionary<Vector2Int, Vector2Int>();
    public Vector2Int spawn_chunk; // where the player spawns in
    public Vector2Int final_chunk; // chunk with the main objective

    public void GenerateMap()
    {
        GenerateChunks();
        Debug.Log(all_chunks.Count);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
        Debug.Log(all_chunks.Count);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Vector2Int chunk in all_chunks.Keys)
        {
            DrawChunk(chunk, Color.white);
        }
        foreach (Vector2Int chunk in adj_chunks.Keys)
        {
            DrawChunk(chunk, Color.grey);
        }
        DrawChunk(START_POS, Color.black);
        DrawChunk(final_chunk, Color.red);
        DrawChunk(spawn_chunk, Color.green);
    }

    #region Map Generation

    private void GenerateChunks() // generate the chunks and declare the start & final pos
    {
        // initialize data holders
        all_chunks.Clear();
        HashSet<Vector2Int> in_chunk_queue = new HashSet<Vector2Int>();
        adj_chunks.Clear();
        List<Vector2Int> array_buffer = new List<Vector2Int>(); // use for branching
        Vector2Int[] adjacent_array = gen_preset.four_adj_tiles ? Directions2D.four_directions : Directions2D.eight_directions;
        Directions2D.DirArray dir_array_type = gen_preset.four_adj_tiles ? Directions2D.DirArray.HORZ_WEIGHT_FOUR : Directions2D.DirArray.HORZ_WEIGHT_EIGHT;
        
        // get the first chunk
        Queue<Vector2Int> chunk_queue = new Queue<Vector2Int> {};
        chunk_queue.Enqueue(START_POS);
        in_chunk_queue.Add(START_POS);
        foreach (Vector2Int adjacent in adjacent_array) // update adjacent chunks
        {
            Vector2Int new_chunk = START_POS + adjacent;
            adj_chunks[new_chunk] = new_chunk;
        }

        // values for final pos and spawn pos
        final_chunk = START_POS;
        for (int i = 0; i < gen_preset.map_chunks && chunk_queue.Count > 0; i++) {
            // add chunk to the hashset
            Vector2Int curr_chunk = chunk_queue.Dequeue();
            in_chunk_queue.Remove(curr_chunk);
            all_chunks[curr_chunk] = new MapChunk(curr_chunk);

            // final pos is furthest from the start position
            if (curr_chunk.sqrMagnitude > final_chunk.sqrMagnitude)
            {
                final_chunk = curr_chunk;
            } 

            // see where the current chunk can branch to
            Directions2D.ValidPositionsFromPoint(array_buffer, dir_array_type, curr_chunk, all_chunks, in_chunk_queue);
            // preform semi-random branch
            int branches = Random.Range(gen_preset.min_chunk_branching, gen_preset.max_chunk_branching + 1);
            for (int b = 0; b < branches; b++)
            {
                Vector2Int new_chunk = curr_chunk;
                int randint = 0;
                if (array_buffer.Count > 0) // prevent overlapping with preexisting chunks
                {
                    randint = Random.Range(0, array_buffer.Count);
                    new_chunk = array_buffer[randint];
                    array_buffer.RemoveAt(randint);
                } 
                else
                {
                    // if the algo still needs to branch but all adjacent spots are taken, just queue a random adjacent chunk
                    randint = Random.Range(0, adj_chunks.Keys.Count);
                    new_chunk = adj_chunks.Keys.ElementAt(randint);
                }
                if ((i+1 + chunk_queue.Count) < gen_preset.map_chunks) // make sure the chunks in queue dont go outta control
                {
                    if (in_chunk_queue.Add(new_chunk)) {
                        adj_chunks.Remove(new_chunk);
                        chunk_queue.Enqueue(new_chunk);
                    }

                    foreach (Vector2Int adjacent in adjacent_array) // update adjacent chunks
                    {
                        Vector2Int new_adj_chunk = new_chunk + adjacent;
                        if (!all_chunks.ContainsKey(new_adj_chunk) && !in_chunk_queue.Contains(new_adj_chunk))
                        {
                            // add to adjacent chunks list/dict
                            adj_chunks[new_adj_chunk] = new_adj_chunk;
                        }
                    }
                }
            }
        }

        // get spawn position based on tile step distance
        chunk_queue.Clear();
        chunk_queue.Enqueue(final_chunk);
        all_chunks[final_chunk].dist_from_final = 0;
        for (int i = 0; i < all_chunks.Count; i ++)
        {
            Vector2Int curr_chunk = chunk_queue.Dequeue();
            foreach (Vector2Int dir in Directions2D.eight_directions) // establish all chunk's dist from the final
            {
                Vector2Int next_chunk = curr_chunk + dir;
                if (all_chunks.ContainsKey(next_chunk) && all_chunks[next_chunk].dist_from_final == -1)
                {
                    all_chunks[next_chunk].dist_from_final = all_chunks[curr_chunk].dist_from_final + 1;
                    chunk_queue.Enqueue(next_chunk);
                }
            }

            if (chunk_queue.Count == 0)
            {
                break;
            }
        }

        spawn_chunk = final_chunk;
        foreach(Vector2Int pos in all_chunks.Keys)
        {
            if (all_chunks[pos].dist_from_final == all_chunks[spawn_chunk].dist_from_final)
            {
                if ((final_chunk - pos).sqrMagnitude > (final_chunk - spawn_chunk).sqrMagnitude)
                spawn_chunk = pos;
            } 
            else if (all_chunks[pos].dist_from_final > all_chunks[spawn_chunk].dist_from_final)
            {
                spawn_chunk = pos;
            } 
        }
    }

    private void GeneratePOI() // generate potential POI based off of chunks in the map
    {
        
    }

    private void Pathfind() // declare pathway chunks between all POI
    {
        
    }

    private void DrawChunk(Vector2Int chunk_pos, Color line_color)
    {
        // Square Shape
        Debug.DrawLine(
            (Vector2)chunk_pos * chunk_size, 
            (Vector2)(chunk_pos + Directions2D.four_directions[1]) * chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (Vector2)(chunk_pos + Directions2D.four_directions[1]) * chunk_size, 
            (Vector2)(chunk_pos + Directions2D.four_directions[1] + Directions2D.four_directions[2]) * chunk_size, 
            line_color 
            );
        Debug.DrawLine(
            (Vector2)chunk_pos * chunk_size, 
            (Vector2)(chunk_pos + Directions2D.four_directions[2]) * chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (Vector2)(chunk_pos + Directions2D.four_directions[2]) * chunk_size, 
            (Vector2)(chunk_pos + Directions2D.four_directions[2] + Directions2D.four_directions[1]) * chunk_size, 
            line_color
            );
        // Crosses
        Debug.DrawLine(
            (Vector2)(chunk_pos) * chunk_size, 
            (Vector2)(chunk_pos + Directions2D.four_directions[2] + Directions2D.four_directions[1]) * chunk_size, 
            line_color
            );
        Debug.DrawLine(
            (Vector2)(chunk_pos + Directions2D.four_directions[2]) * chunk_size, 
            (Vector2)(chunk_pos + Directions2D.four_directions[1]) * chunk_size, 
            line_color
            );
    }

    #endregion
}
