using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AIManager
{
    private GameOverseer game_overseer;
    [Header("Astar")]
    [SerializeField] HashSet<PathNode> map_tiles = new HashSet<PathNode>(); 

    public AIManager(GameOverseer game_overseer, Tilemap wall_map, Tilemap ground_map)
    {
        this.game_overseer = game_overseer;
        SetupMap(wall_map, ground_map);
    }

    #region Map
    private void SetupMap(Tilemap wall_map, Tilemap ground_map)
    {
        BoundsInt full_bounds = wall_map.cellBounds;
        Debug.Log(full_bounds.xMin +" " + full_bounds.yMin +" " + full_bounds.xMax +" " + full_bounds.yMax);

        for (int x = full_bounds.xMin; x <= full_bounds.xMax; x++)
        {
            for (int y = full_bounds.yMin; y <= full_bounds.yMax; y++)
            {
                map_tiles.Add(new PathNode(new Vector2(x, y)));
            }
        }
    }
    #endregion

    #region Targetting
    public Character GetTargetCharacter(int faction_tag, Character curr_character, float max_range = 1000f, TargetType targ_type = TargetType.Closest)
    {
        IEnumerable<Character> check_data = faction_tag == GameOverseer.squad_tag ? game_overseer.enemy_manager.enemies : game_overseer.squad_manager.squad;
        Func<Character, Character, float> ScoringFunc = GetNearestScore;
        switch (targ_type)
        {
            case TargetType.Closest:
                ScoringFunc = GetNearestScore;
                break;
            case TargetType.Furthest:
                ScoringFunc = GetFurthestScore;
                break;
            case TargetType.MostHP:
                ScoringFunc = GetNearestScore;
                break;
            case TargetType.LeastHP:
                ScoringFunc = GetNearestScore;
                break;
        }

        float highest_score = -Mathf.Infinity;
        Character prime_target = null;
        foreach(Character target in check_data)
        {
            float weight = 1f;
            if (!target || !target.IsInAction() || (curr_character.GetPosition() - target.GetPosition()).sqrMagnitude > max_range * max_range)
            {
                continue;
            }
            float score = ScoringFunc(curr_character, target) * weight;
            if (score > highest_score)
            {
                prime_target = target;
                highest_score = score;
                
            }
        }

        return prime_target;
    }

    private float GetNearestScore(Character curr_character, Character target)
    {
        float score = 1/(curr_character.GetPosition() - target.GetPosition()).sqrMagnitude + 0.001f;
        return score;
    }

    private float GetFurthestScore(Character curr_character, Character target)
    {
        float score = (curr_character.GetPosition() - target.GetPosition()).sqrMagnitude;
        if (score > curr_character.curr_range)
        {
            score = -1;
        }
        return score;
    }

    #endregion

    #region Astar
    public void Astar(Vector2 start_pos, Vector2 end_pos, Vector2[] path)
    {
        
    }
    #endregion
}