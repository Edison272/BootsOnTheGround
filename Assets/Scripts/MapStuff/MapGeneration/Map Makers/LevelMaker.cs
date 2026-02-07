using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random; // LEAVE ME ALONE DAMNIT
public class LevelMaker : MapMaker
{
    public override Vector2 GenerateMap( // return map center
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        HashSet<Vector2Int> border_chunks, 
        HashSet<Vector2Int> path_chunks, 
        MajorPOI[] critical_locs,
        MapGenPreset gen_preset)
    {
        #region GenMap - Spine
        Vector2Int start_pos = MapManager.START_POS;
        Vector2 unit_circ = Random.insideUnitCircle;
        Vector2 random_dir = new Vector2(unit_circ.x + Mathf.Sign(unit_circ.x) * Random.Range(1.25f, 1.75f), unit_circ.y*0.9f).normalized;

        float poi_partition = 1f/gen_preset.objectives;
        Vector2Int[] all_poi = new Vector2Int[critical_locs.Length];

        // initialize starting point
        all_poi[0] = start_pos;
        critical_locs[0] = new MajorPOI(all_poi[0]);
        all_chunks[start_pos] = critical_locs[0].main_chunk;
        path_chunks.Add(start_pos);
        int total_minor_poi = 0;
        for (int i = 1; i < all_poi.Length; i++) 
        {
            Vector2 new_pos = start_pos + random_dir * gen_preset.map_size * poi_partition * i;
            float lat_scale = gen_preset.map_size * poi_partition;
            new_pos += new Vector2(random_dir.y, -random_dir.x).normalized * Random.Range(-lat_scale, lat_scale);
            all_poi[i] = new Vector2Int((int)new_pos.x, (int)new_pos.y);
            critical_locs[i] = new MajorPOI(all_poi[i], critical_locs[i-1]);
            critical_locs[i-1].SetNextPOI(critical_locs[i]);
            
            int generate_poi = Random.Range(0, gen_preset.minor_poi);
            critical_locs[i-1].GenerateMinorPOI(generate_poi, poi_partition * gen_preset.map_size * 0.5f);
            total_minor_poi += generate_poi;
        }

        // Draw a path between all poi
        for (int i = 1; i < all_poi.Length; i++)
        {
            int dx = all_poi[i].x - all_poi[i-1].x, dy = all_poi[i].y - all_poi[i-1].y;
            float diag_dist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
            for (float d = 1; d <= diag_dist; d++)
            {
                float t = d / (float)diag_dist;
                Vector2Int offset_vec = new Vector2Int(
                    (int)Mathf.Round(all_poi[i-1].x + dx * t), 
                    (int)Mathf.Round(all_poi[i-1].y + dy * t)
                    );
                all_chunks[offset_vec] = new MapChunk(offset_vec);
                path_chunks.Add(offset_vec);
            }

            foreach (Vector2Int m_poi in critical_locs[i].minor_poi)
            {
                int poi_dx = m_poi.x - all_poi[i].x, poi_dy = m_poi.y - all_poi[i].y;
                float poi_diag_dist = Mathf.Max(Mathf.Abs(poi_dx), Mathf.Abs(poi_dy));
                for (float d = 1; d <= poi_diag_dist; d++)
                {
                    float t = d / (float)poi_diag_dist;
                    Vector2Int offset_vec = new Vector2Int(
                        (int)Mathf.Round(all_poi[i].x + poi_dx * t), 
                        (int)Mathf.Round(all_poi[i].y + poi_dy * t)
                        );
                    all_chunks[offset_vec] = new MapChunk(offset_vec);
                    path_chunks.Add(offset_vec);
                }
            }
        }
        #endregion

        #region GenMap - Body
        // set up more chunks around poi path
        Queue<Vector2Int> chunk_queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> in_chunk_queue = new HashSet<Vector2Int>();
        List<Vector2Int> list_buffer = new List<Vector2Int>(); // use for branching
        
        // queue starting path chunks
        foreach(Vector2Int chunk in all_chunks.Keys)
        {
            foreach (Vector2Int dir in Directions2D.eight_directions)
            {
                Vector2Int chunk_dir = chunk + dir;
                if (!all_chunks.ContainsKey(chunk_dir) && !in_chunk_queue.Contains(chunk_dir))
                {
                    chunk_queue.Enqueue(chunk_dir);
                    in_chunk_queue.Add(chunk_dir);
                }
            }
        }
        // add adjacent chunks
        foreach(Vector2Int in_queue in in_chunk_queue)
        {
            foreach (Vector2Int adj in Directions2D.four_directions)
            {
                Vector2Int chunk_dir_adj = in_queue + adj;
                if (!all_chunks.ContainsKey(chunk_dir_adj) && !in_chunk_queue.Contains(chunk_dir_adj))
                {
                    border_chunks.Add(chunk_dir_adj);
                }
            }
        }

        // random propagation to adjacent chunks
        int chunks = gen_preset.map_size * gen_preset.map_scale + total_minor_poi * gen_preset.map_scale;
        for (int i = 0; i < chunks && chunk_queue.Count > 0; i++) {
            // remove chunk from queue add chunk to all chunks
            Vector2Int curr_chunk = chunk_queue.Dequeue();
            in_chunk_queue.Remove(curr_chunk);
            all_chunks[curr_chunk] = new MapChunk(curr_chunk);

            //queue a random adjacent chunk
            int randint = Random.Range(0, border_chunks.Count);
            Vector2Int new_chunk = border_chunks.ElementAt(randint);

            if ((i+1 + chunk_queue.Count) < chunks) // dont add anymore chunks if the queue is at the limit
            {
                if (in_chunk_queue.Add(new_chunk)) {
                    border_chunks.Remove(new_chunk);
                    chunk_queue.Enqueue(new_chunk);
                }
                foreach (Vector2Int adjacent in Directions2D.four_directions) // update adjacent chunks
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
        #endregion

        #region GenMap - Post
        // fill isolated border chunks
        list_buffer.Clear(); // use list buffer to destroy all  surrounded borders
        foreach(Vector2Int border in border_chunks)
        {
            bool adj_to_empty = false;
            foreach(Vector2Int dir in Directions2D.eight_directions)
            {
                // convert border chunks that aren't diagonally adjacent to a completely empty area into normal chunks
                Vector2Int border_dir = border + dir;
                if (!border_chunks.Contains(border_dir) && !all_chunks.ContainsKey(border_dir))
                {
                    adj_to_empty = true;
                    break;
                }
            }
            if (!adj_to_empty)
            {
                list_buffer.Add(border);
                all_chunks[border] = new MapChunk(border);
            }
        }
        foreach(Vector2Int border in list_buffer)
        {
            border_chunks.Remove(border);
        }

        // get neighbors
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
        #endregion
    }

    public override void GeneratePOI(
        Dictionary<Vector2Int, MapChunk> all_chunks, 
        MajorPOI[] critical_locs)
    {

    }
    
}