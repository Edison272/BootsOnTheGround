using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random; // LEAVE ME ALONE DAMNIT
public class BlobMaker : MapMaker
{
    public override Vector2 GenerateMap(
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        HashSet<Vector2Int> border_chunks, 
        HashSet<Vector2Int> path_chunks, 
        MajorPOI[] critical_locs,
        MapGenPreset gen_preset
    )
    {
        HashSet<Vector2Int> in_chunk_queue = new HashSet<Vector2Int>();
        List<Vector2Int> list_buffer = new List<Vector2Int>(); // use for branching
        Vector2Int[] adjacent_array = Directions2D.eight_directions;
        Directions2D.DirArray dir_array_type = Directions2D.DirArray.HORZ_WEIGHT_EIGHT;
        
        // important chunks/positions
        Vector2Int spawn_chunk; // where the player spawns in
        Vector2Int final_chunk; // chunk with the main objective
        Vector2 map_center; // duh

        
        // get the first chunk
        Queue<Vector2Int> chunk_queue = new Queue<Vector2Int> {};
        chunk_queue.Enqueue(MapManager.START_POS);
        in_chunk_queue.Add(MapManager.START_POS);
        foreach (Vector2Int adjacent in adjacent_array) // update adjacent chunks
        {
            Vector2Int new_chunk = MapManager.START_POS + adjacent;
            border_chunks.Add(new_chunk);
        }

        // values for final pos and spawn pos
        final_chunk = MapManager.START_POS;
        for (int i = 0; i < gen_preset.map_size && chunk_queue.Count > 0; i++) {
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
            Directions2D.ValidPositionsFromPoint(list_buffer, dir_array_type, curr_chunk, all_chunks, in_chunk_queue);
            // preform semi-random branch
            int branches = Random.Range(1, 4);
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
                    randint = Random.Range(0, border_chunks.Count);
                    new_chunk = border_chunks.ElementAt(randint);
                }
                if ((i+1 + chunk_queue.Count) < gen_preset.map_size) // make sure the chunks in queue dont go outta control
                {
                    if (in_chunk_queue.Add(new_chunk)) {
                        border_chunks.Remove(new_chunk);
                        chunk_queue.Enqueue(new_chunk);
                    }

                    foreach (Vector2Int adjacent in adjacent_array) // update adjacent chunks
                    {
                        Vector2Int new_adj_chunk = new_chunk + adjacent;
                        if (!all_chunks.ContainsKey(new_adj_chunk) && !in_chunk_queue.Contains(new_adj_chunk))
                        {
                            // add to adjacent chunks list/dict
                            border_chunks.Add(new_adj_chunk);
                        }
                    }
                }
            }
        }
        // get spawn position based on tile step distance
        // set neighbors of each chunk
        chunk_queue.Clear();
        chunk_queue.Enqueue(final_chunk);
        all_chunks[final_chunk].dist_from_final = 0;
        for (int i = 0; i < all_chunks.Count; i ++)
        {
            Vector2Int curr_chunk = chunk_queue.Dequeue();
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

        // find spawn chunk, map center, draw tiles, and chunk connections
        spawn_chunk = final_chunk;
        map_center = Vector2.zero;
        foreach(Vector2Int pos in all_chunks.Keys)
        {
            map_center += pos;
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
        map_center /= all_chunks.Keys.Count;

        // declare first and last pos of critical locs
        critical_locs[0] = new MajorPOI(all_chunks[spawn_chunk]);
        critical_locs[1] = new MajorPOI(all_chunks[final_chunk]);

        return map_center;
    }

    public override void GeneratePOI(Dictionary<Vector2Int, MapChunk> all_chunks, MajorPOI[] critical_locs)
    {
        for (int i = 2; i < critical_locs.Length; i++) // fill in betweens of the list
        {
            float highest_short = -Mathf.Infinity;
            foreach(MapChunk mc in all_chunks.Values)
            {   
                float shortest_dist = Mathf.Infinity;
                if (Array.IndexOf(critical_locs, mc) != -1)
                {
                    continue;
                }

                for (int l = 0; l < critical_locs.Length; l++)
                {
                    if (critical_locs[l] == null)
                    {
                        continue; // exit loop bc theres no more critical locs to compare to
                    }
                    float dist = (mc.position - critical_locs[l].main_chunk.position).sqrMagnitude;
                    if (dist < shortest_dist)
                    {
                        shortest_dist = dist;
                    }
                }

                if (shortest_dist > highest_short)
                {
                    critical_locs[i] = new MajorPOI(mc);
                    highest_short = shortest_dist;
                }
            }
        }
    }
}