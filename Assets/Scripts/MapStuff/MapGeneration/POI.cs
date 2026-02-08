using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class MajorPOI
{
    public MapChunk main_chunk;
    public MajorPOI prev = null;
    public MajorPOI next_poi = null;
    public Vector2Int[] minor_poi = new Vector2Int[0];

    [Header("Objective Point")]
    public Objective objective_point;

    public MajorPOI(Vector2Int position, MajorPOI prev = null)
    {
        main_chunk = new MapChunk(position);
        SetupPOI(prev);
    }

    public MajorPOI(MapChunk main_chunk, MajorPOI prev = null)
    {
        this.main_chunk = main_chunk;
        SetupPOI(prev);
    }

    public void SetupPOI(MajorPOI prev)
    {
        if (prev != null)
        {
            this.prev = prev;
        } else
        {
            this.prev = this;
        }

        GameObject objective_resource = Resources.Load<GameObject>("MapObjects/Objective");
        objective_point = MonoBehaviour.Instantiate(
            objective_resource, 
            main_chunk.center_position * MapManager.chunk_size, 
            Quaternion.identity).GetComponent<Objective>();
        objective_point.Setup(this);
    }

    public void SetNextPOI(MajorPOI next)
    {
        this.next_poi = next;
    }

    public void GenerateMinorPOI(int amount, float size)
    {
        if (amount <= 0)
        {
            return;
        }
        
        if (!(next_poi == null) && !(prev == null))
        {
            Vector2 splinter_dir = ((Vector2)prev.main_chunk.position + next_poi.main_chunk.position - 2*main_chunk.position).normalized;
            minor_poi = new Vector2Int[amount];
            float poi_partition = 1/amount;
            for (int i = 0; i < amount; i++)
            {
                splinter_dir *= -1;
                Vector2 new_pos = main_chunk.position + splinter_dir * (size * 0.5f + Random.Range(size * poi_partition, size * 0.5f));
                float lat_scale = size;
                new_pos += new Vector2(splinter_dir.y, -splinter_dir.x).normalized * Random.Range(-lat_scale, lat_scale);
                minor_poi[i] = new Vector2Int((int)new_pos.x, (int)new_pos.y);
            }
        }
    }
}