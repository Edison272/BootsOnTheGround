using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class ObjectiveManager
{
    public GameOverseer game_overseer;
    public MapManager map_manager;
    public MajorObjective[] major_objectives;

    [Header("Progression Data")]
    public int frontier_objective = 0; 
    public int total_objectives = 0;
    public bool objectives_complete => frontier_objective >= total_objectives;
    [Header("Contain Player")]
    const float c_max_out_of_bounds_time = 1f;
    private float curr_out_of_bounds_time = 0f;
    [Header("Fog of War")]
    public List<GameObject> FogObjects = new List<GameObject>();
    
    public MajorObjective current_mo;

    // manage the frontier
    public ObjectiveManager(GameOverseer game_overseer, int total_objectives)
    {
        this.game_overseer = game_overseer;
        this.map_manager = game_overseer.map_manager;
        frontier_objective = 0;
        this.total_objectives = total_objectives;
        current_mo = map_manager.critical_locs[frontier_objective];
    }

    public void PrepareObjectives()
    {
        current_mo = map_manager.critical_locs[frontier_objective];
        SetFogOfWar();
    }

    public void UpdateMapManager()
    {
        current_mo = map_manager.critical_locs[frontier_objective];
        MajorObjective next_mo = map_manager.critical_locs[frontier_objective + 1];
        // check for out of bounds
        Vector2 player_pos = game_overseer.player_control.active_character.GetPosition();
        Vector2Int player_curr_chunk = MapManager.GetWorldToChunkSpace(player_pos);
        if (next_mo.territory_chunks.Contains(player_curr_chunk))
        {
            if (curr_out_of_bounds_time < c_max_out_of_bounds_time)
            {
                curr_out_of_bounds_time += Time.deltaTime;
                game_overseer.player_control.active_character.ChangeSpeed(1- curr_out_of_bounds_time/c_max_out_of_bounds_time, Time.deltaTime, false);
            }
        }
        if (curr_out_of_bounds_time >= c_max_out_of_bounds_time)
        {
            game_overseer.player_control.active_character.SetMove(current_mo.main_chunk.world_center_position - player_pos);
            if (current_mo.territory_chunks.Contains(player_curr_chunk))
            {
                game_overseer.player_control.active_character.StopMove();
                game_overseer.player_control.active_character.ForceMove(current_mo.main_chunk.world_center_position - player_pos, 10);
                curr_out_of_bounds_time = 0;
            }
        }
    }

    #region Fog of War
    public void SetFogOfWar()
    {
        ClearFogOfWar(); // remove fog over previous territory
        if (!objectives_complete)
        {
            GenerateFogOfWar(); // only clear fog of war if there are objectives left
        }
        
    }
    private void GenerateFogOfWar()
    {
        MajorObjective next_mo = map_manager.critical_locs[frontier_objective + 1];
        Debug.Log( next_mo.territory_chunks.Count + ", needed chunks: " + FogObjects.Count);
        if (FogObjects.Count < next_mo.territory_chunks.Count)
        {
            int current_fog_chunks = FogObjects.Count;
            for (int i = 0; i < next_mo.territory_chunks.Count - current_fog_chunks; i++)
            {
                GameObject new_fog_object = GameObject.Instantiate(Resources.Load<GameObject>("MapObjects/ZoneFog"));
                FogObjects.Add(new_fog_object);
                new_fog_object.SetActive(false);
            }
        }

        int f = 0;
        Debug.Log( next_mo.territory_chunks.Count + ", created chunks: " + FogObjects.Count);
        foreach(Vector2Int chunk in next_mo.territory_chunks)
        {
            GameObject place_fog = FogObjects[f];
            place_fog.transform.position = map_manager.all_chunks[chunk].world_center_position;
            place_fog.SetActive(true);
            f++;
        }
    }

    private void ClearFogOfWar()
    {
        foreach(GameObject fog_object in FogObjects)
        {
            fog_object.SetActive(true);
        }
    }
    #endregion

    #region Objective Events
    public MajorObjective GetFrontierMajorObjective()
    {
        return game_overseer.map_manager.critical_locs[frontier_objective];
    }
    public void ObjectiveCaptured(MajorObjective maj_poi)
    {
        if (!objectives_complete && maj_poi.next_poi != null)
        {
            game_overseer.enemy_manager.SummonEnemyGroup(maj_poi.next_poi.main_chunk.world_position);
        }
    }

    // when an objective is lost, enemies will reinforce it again
    public void ObjectiveLost(MajorObjective maj_poi)
    {
        frontier_objective--;
        Debug.Log("LOSS");
    }

    // when all enemies are defeated, the objective is secured. call map to open up the frontier
    public void ObjectiveSecured()
    {
        frontier_objective++;
        SetFogOfWar();
        current_mo = map_manager.critical_locs[frontier_objective];
    }
    #endregion
}