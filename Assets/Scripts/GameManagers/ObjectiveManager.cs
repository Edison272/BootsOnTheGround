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
                curr_out_of_bounds_time = 0;
            }
        }
    }

    public MajorObjective GetFrontierMajorObjective()
    {
        return game_overseer.map_manager.critical_locs[frontier_objective];
    }
    public void ObjectiveCaptured(MajorObjective maj_poi)
    {
        Debug.Log("CAP");
        frontier_objective++;
        current_mo = map_manager.critical_locs[frontier_objective];
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
        
    }
}