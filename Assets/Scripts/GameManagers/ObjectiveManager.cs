using UnityEngine;

[System.Serializable]
public class ObjectiveManager
{
    private GameOverseer game_overseer;

    [Header("Progression Data")]
    public int frontier_objective = 0; 
    public int total_objectives = 0;
    public bool objectives_complete => frontier_objective >= total_objectives;

    // manage the frontier
    public ObjectiveManager(GameOverseer game_overseer, int total_objectives)
    {
        this.game_overseer = game_overseer;
        frontier_objective = 0;
        this.total_objectives = total_objectives + 1;
    }

    public void UpdateMapManager()
    {
        
    }

    public MajorObjective GetFrontierMajorObjective()
    {
        return game_overseer.map_manager.critical_locs[frontier_objective];
    }
    public void ObjectiveCaptured(MajorObjective maj_poi)
    {
        Debug.Log("CAP");
        frontier_objective++;
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