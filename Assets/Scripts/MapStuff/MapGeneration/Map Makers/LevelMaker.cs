using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random; // LEAVE ME ALONE DAMNIT
public class LevelMaker : MapMaker
{
    public override Vector2 GenerateMap( // return map center
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        Dictionary<Vector2Int, Vector2Int> adj_chunks, 
        MapChunk[] critical_locs,
        MapGenPreset gen_preset)
    {
        // temporary preset information
        Vector2Int start_pos = MapManager.START_POS;
        Vector2 unit_circ = Random.insideUnitCircle;
        Vector2 random_dir = new Vector2(unit_circ.x + Mathf.Sign(unit_circ.x) * Random.Range(1.25f, 1.75f), unit_circ.y*0.9f).normalized;
        
        float poi_partition = 1f/gen_preset.objectives;
        Vector2Int[] all_poi = new Vector2Int[critical_locs.Length];

        // initialize starting point
        all_poi[0] = start_pos;
        critical_locs[0] = new MapChunk(all_poi[0]);
        all_chunks[start_pos] = critical_locs[0];

        for (int i = 1; i < all_poi.Length; i++) 
        {
            Vector2 new_pos = start_pos + random_dir * gen_preset.map_size * poi_partition * i;
            float lat_scale = gen_preset.map_size * poi_partition;
            new_pos += new Vector2(random_dir.y, -random_dir.x).normalized * Random.Range(-lat_scale, lat_scale);
            all_poi[i] = new Vector2Int((int)new_pos.x, (int)new_pos.y);
            critical_locs[i] = new MapChunk(all_poi[i]);
        }

        // Draw a path between all poi
        for (int i = 1; i < all_poi.Length; i++)
        {
            Vector2 dir_vec = all_poi[i] - all_poi[i-1];
            int dx = all_poi[i].x - all_poi[i-1].x, dy = all_poi[i].y - all_poi[i-1].y;
            float diag_dist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
            for (int d = 1; d <= diag_dist; d++)
            {
                dir_vec = dir_vec.normalized;
                Vector2Int offset_vec = new Vector2Int(
                    (int)Mathf.Round(all_poi[i-1].x + dx * (d/diag_dist)), 
                    (int)Mathf.Round(all_poi[i-1].y + dy * (d/diag_dist))
                    );
                all_chunks[offset_vec] = new MapChunk(all_poi[i-1] + offset_vec);
            }
        }

        // set up more chunks around poi path
        Queue<Vector2Int> chunk_queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> in_chunk_queue = new HashSet<Vector2Int>();
        List<Vector2Int> list_buffer = new List<Vector2Int>(); // use for branching
        Vector2Int[] adjacent_array = gen_preset.four_adj_tiles ? Directions2D.four_directions : Directions2D.eight_directions;
        Directions2D.DirArray dir_array_type = gen_preset.four_adj_tiles ? Directions2D.DirArray.HORZ_WEIGHT_FOUR : Directions2D.DirArray.HORZ_WEIGHT_EIGHT;
        
        foreach(Vector2Int chunk in all_chunks.Keys)
        {
            chunk_queue.Enqueue(chunk);
        }

        int chunks = gen_preset.map_size * gen_preset.map_scale;
        for (int i = 0; i < chunks && chunk_queue.Count > 0; i++) {
            // add chunk to the hashset
            Vector2Int curr_chunk = chunk_queue.Dequeue();
            in_chunk_queue.Remove(curr_chunk);
            all_chunks[curr_chunk] = new MapChunk(curr_chunk);

            // see where the current chunk can branch to
            Directions2D.ValidPositionsFromPoint(list_buffer, dir_array_type, curr_chunk, all_chunks, in_chunk_queue);
            // preform semi-random branch
            int branches = Random.Range(gen_preset.min_chunk_branching, gen_preset.max_chunk_branching + 1);
            for (int b = 0; b < branches; b++)
            {
                Vector2Int new_chunk = curr_chunk;
                int randint = 0;
                if (list_buffer.Count > 0) // prevent overlapping with preexisting chunks
                {
                    randint = Random.Range(0, list_buffer.Count);
                    new_chunk = list_buffer[randint];
                    list_buffer.RemoveAt(randint);
                } 
                else
                {
                    // if the algo still needs to branch but all adjacent spots are taken, just queue a random adjacent chunk
                    randint = Random.Range(0, adj_chunks.Keys.Count);
                    new_chunk = adj_chunks.Keys.ElementAt(randint);
                }
                if ((i+1 + chunk_queue.Count) < chunks) // make sure the chunks in queue dont go outta control
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

        foreach (Vector2Int curr_chunk in all_chunks.Keys)
        {
            list_buffer.Clear();
            foreach (Vector2Int dir in Directions2D.eight_directions) // establish all chunk's dist from the final
            {
                Vector2Int next_chunk = curr_chunk + dir;
                if (all_chunks.ContainsKey(next_chunk) && all_chunks[next_chunk].dist_from_final == -1)
                {
                    all_chunks[next_chunk].dist_from_final = all_chunks[curr_chunk].dist_from_final + 1;
                    chunk_queue.Enqueue(next_chunk);
                }

                // find neighbors
                if (all_chunks.ContainsKey(next_chunk))
                {
                    list_buffer.Add(dir);
                }
            }
            // set neighbors
            all_chunks[curr_chunk].SetNeighbors(list_buffer);

            if (chunk_queue.Count == 0)
            {
                break;
            }
        }

        return Vector2.zero;
    }

    public override void GeneratePOI(
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        MapChunk[] critical_locs)
    {

    }
    
}