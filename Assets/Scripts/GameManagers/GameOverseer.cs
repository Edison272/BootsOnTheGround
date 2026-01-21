using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;


public enum TargetType {Closest, Furthest, MostHP, LeastHP}
public class GameOverseer : MonoBehaviour // this thing starts up everything else and keeps track of the game
{
    public PlayerController player_control;
    public SquadManager squad_manager;
    public EnemyManager enemy_manager;
    public MapManager map_manager;
    public CanvasController canvas_control;

    public static GameOverseer THE_OVERSEER {get; private set;}

    void Awake()
    {
        THE_OVERSEER = this;

        if (!player_control) {player_control = GameObject.Find("Player Controller")?.GetComponent<PlayerController>();}
        if (!squad_manager) {squad_manager = GameObject.Find("Squad Manager")?.GetComponent<SquadManager>();}
        if (!enemy_manager) {enemy_manager = GameObject.Find("Enemy Manager")?.GetComponent<EnemyManager>();}
        if (!map_manager) {map_manager = GameObject.Find("Map")?.GetComponent<MapManager>();}
        if (!map_manager) {canvas_control = GameObject.Find("Canvas")?.GetComponent<CanvasController>();}

    }

    void Start()
    {
        // generate map first
        if (!map_manager) {
            Debug.LogWarning("MAP MANAGER NULL");

        }
        else
        {
            map_manager.GenerateMap();
            squad_manager.transform.position = (Vector2)map_manager.GetChunkWorldPos(map_manager.spawn_chunk);
            enemy_manager.transform.position = (Vector2)map_manager.GetChunkWorldPos(map_manager.final_chunk);
        }
        // let everyone get to know eachother
        squad_manager.player = player_control;
        player_control.squad = squad_manager;

        // initialize squad and enemy managers in the right places
        squad_manager.CreateSquad();
        enemy_manager.CreateEnemies();
        canvas_control.SetOperatorProfiles();

        // start player controller
        player_control.StartPlayer();
    }
    

    #region Targetting
    public Character GetTargetCharacter(bool is_squad, Character curr_character, TargetType targ_type = TargetType.Closest)
    {
        Character[] check_data = is_squad ? enemy_manager.enemies : squad_manager.squad;

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
        foreach(Character character in check_data)
        {
            float score = ScoringFunc(curr_character, character);
            if (score > highest_score)
            {
                prime_target = character;
            }
        }

        return prime_target;
    }

    private float GetNearestScore(Character curr_character, Character character)
    {
        float score = 1/(curr_character.GetPosition() - character.GetPosition()).sqrMagnitude + 0.001f;
        return score;
    }

    private float GetFurthestScore(Character curr_character, Character character)
    {
        float score = (curr_character.GetPosition() - character.GetPosition()).sqrMagnitude;
        if (score > curr_character.curr_range)
        {
            score = -1;
        }
        return score;
    }

    #endregion
}
