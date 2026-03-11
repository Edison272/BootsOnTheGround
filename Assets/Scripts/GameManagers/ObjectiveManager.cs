using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

[System.Serializable]
public class ObjectiveManager
{
    private GameOverseer game_overseer;
    private MapManager map_manager;

    [Header("Progression Data")]
    public int frontier_objective = 0; 
    public int total_objectives = 0;
    public bool objectives_complete => frontier_objective >= total_objectives;

    
    public MajorObjective current_mo;

    // manage the frontier
    public ObjectiveManager(GameOverseer game_overseer, int total_objectives)
    {
        this.game_overseer = game_overseer;
        this.map_manager = game_overseer.map_manager;
        frontier_objective = 0;
        this.total_objectives = total_objectives + 1;
        current_mo = map_manager.critical_locs[frontier_objective + 1];
    }

    public void UpdateMapManager()
    {
        current_mo = map_manager.critical_locs[frontier_objective + 1];
        // check for out of bounds
        Vector2 player_pos = game_overseer.player_control.active_character.GetPosition();
        Vector2Int player_curr_chunk = MapManager.GetWorldToChunkSpace(player_pos);
        if (!current_mo.territory_chunks.Contains(player_curr_chunk))
        {
            game_overseer.player_control.active_character.SetMovePos(current_mo.main_chunk.center_position);
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
        current_mo = map_manager.critical_locs[frontier_objective + 1];
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