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
    public Dictionary<Vector2Int, MapChunk> all_chunks;
    private Dictionary<Vector2Int, Vector2Int> adj_chunks;
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
        all_chunks = new Dictionary<Vector2Int, MapChunk>();
        HashSet<Vector2Int> in_chunk_queue = new HashSet<Vector2Int>();
        adj_chunks = new Dictionary<Vector2Int, Vector2Int>();
        List<Vector2Int> array_buffer = new List<Vector2Int>(); // use for branching
        Directions2D.GetListOfArray(array_buffer, Directions2D.DirArray.HORZ_WEIGHT_FOUR, true);
        
        // get the first chunk
        Queue<Vector2Int> chunk_queue = new Queue<Vector2Int> {};
        chunk_queue.Enqueue(START_POS);
        in_chunk_queue.Add(START_POS);

        // values for final pos and spawn pos
        final_chunk = START_POS;
        for (int i = 0; i < gen_preset.map_chunks && chunk_queue.Count > 0; i++) {
            // add chunk to the hashset
            Vector2Int curr_chunk = chunk_queue.Dequeue();
            in_chunk_queue.Remove(curr_chunk);
            all_chunks[curr_chunk] = new MapChunk(curr_chunk);
            foreach (Vector2Int adjacent in Directions2D.eight_directions) // update adjacent chunks
            {
                Vector2Int new_chunk = curr_chunk + adjacent;
                if (!all_chunks.ContainsKey(new_chunk) && !in_chunk_queue.Contains(new_chunk))
                {
                    // add to adjacent chunks list/dict
                    adj_chunks[new_chunk] = new_chunk;
                }
            }

            // final pos is furthest from the start position
            if (curr_chunk.sqrMagnitude > final_chunk.sqrMagnitude)
            {
                final_chunk = curr_chunk;
            } 

            // see where the current chunk can branch to
            int branches = Random.Range(gen_preset.min_chunk_branching, gen_preset.max_chunk_branching + 1);
            branches = Mathf.Min(branches, adj_chunks.Keys.Count);
            Debug.Log(branches);
            for (int b = 0; b < branches; b++)
            {
                Vector2Int new_chunk = curr_chunk + array_buffer[Random.Range(0, array_buffer.Count)];
                if (!adj_chunks.ContainsKey(new_chunk)) // prevent overlapping with preexisting chunks
                {
                    // if the algo still needs to branch but all adjacent spots are taken, just queue a random adjacent chunk
                    int randint = Random.Range(0, adj_chunks.Keys.Count);
                    Debug.Log(randint + " " + adj_chunks.Keys.Count + " " + new_chunk);
                    new_chunk = adj_chunks.Keys.ElementAt(randint);
                } else
                {
                    Debug.Log(adj_chunks.Keys.Count + " " + new_chunk);
                }

                if (in_chunk_queue.Add(new_chunk))
                {
                    adj_chunks.Remove(new_chunk);
                    chunk_queue.Enqueue(new_chunk);
                }
            }
        }
        foreach(Vector2Int chunk in in_chunk_queue)
        {
            adj_chunks[chunk] = chunk;
        }

        // get spawn position based on tile step distance
        int c = 0;
        chunk_queue.Clear();
        chunk_queue.Enqueue(final_chunk);
        in_chunk_queue = new HashSet<Vector2Int>(); // reuse in_chunk_queue as the "visited" hashset
        while(chunk_queue.Count > 0 && c < chunk_queue.Count*2)
        {
            Vector2Int curr_chunk = chunk_queue.Dequeue();
            in_chunk_queue.Add(curr_chunk);
            foreach (Vector2Int dir in Directions2D.four_directions) // establish all chunk's dist from the final
            {
                Vector2Int next_chunk = curr_chunk + dir;
                if (all_chunks.ContainsKey(next_chunk) && !in_chunk_queue.Contains(next_chunk))
                {
                    all_chunks[next_chunk].dist_from_final = all_chunks[curr_chunk].dist_from_final + 1;
                    chunk_queue.Enqueue(next_chunk);
                }
            }
            c++;
        }

        spawn_chunk = final_chunk;
        foreach(Vector2Int pos in all_chunks.Keys)
        {
            if (all_chunks[pos].dist_from_final > all_chunks[spawn_chunk].dist_from_final)
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
