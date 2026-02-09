using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
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
    [SerializeField] private AIManager ai_manager; // a class containing all the stuff the game ai would need
    public bool GenerateRandomMap = false;

    public static GameOverseer THE_OVERSEER {get; private set;}

    // Layermask stuff
    public static readonly LayerMask avoid_map_mask = 1 << 7; // check if map is in the way of this raycast
    public static readonly LayerMask avoid_obstacles_mask = (1 << 6) | (1 << 7); // check if map and entities are in the way of this raycast
    
    // Faction stuff
    [SerializeField] private Color serialize_squad_color;
    [SerializeField] private Color serialize_enemy_color;
    public static Color squad_color;
    public static Color enemy_color;
    public static Color empty_color = Color.white;
    public static readonly int squad_tag = 0;
    public static readonly int enemy_tag = 1;

    void Awake()
    {
        THE_OVERSEER = this;

        if (!player_control) {player_control = GameObject.Find("Player Controller")?.GetComponent<PlayerController>();}
        if (!squad_manager) {squad_manager = GameObject.Find("Squad Manager")?.GetComponent<SquadManager>();}
        if (!enemy_manager) {enemy_manager = GameObject.Find("Enemy Manager")?.GetComponent<EnemyManager>();}
        if (!map_manager) {map_manager = GameObject.Find("Map")?.GetComponent<MapManager>();}
        if (!canvas_control) {canvas_control = GameObject.Find("Canvas Controller")?.GetComponent<CanvasController>();}

        squad_color = serialize_squad_color;
        enemy_color = serialize_enemy_color;
    }

    void Start()
    {
        // generate map first
        if (!map_manager) {
            Debug.LogWarning("MAP MANAGER NULL");

        }
        else if (GenerateRandomMap)
        {
            map_manager.GenerateMap();
            squad_manager.transform.position = (Vector2)map_manager.GetChunkWorldPos(map_manager.spawn_chunk);
            enemy_manager.transform.position = (Vector2)map_manager.GetChunkWorldPos(map_manager.final_chunk);
        }
        // let everyone get to know eachother
        squad_manager.player = player_control;
        player_control.squad = squad_manager;

        // initialize squad and enemy managers in the right places
        squad_manager.InitializeAllies();
        canvas_control.SetOperatorProfiles();

        // start player controller
        player_control.StartPlayer();

        // setup an AI manager after base data has been created
        ai_manager = new AIManager(this, map_manager.Wall, map_manager.Floor);
    }
    #region Game Objective Events
    // when an objective is captured, send enemies to recature it
    public static void ObjectiveCaptured(Objective objective)
    {
        
    }

    // when an objective is lost, enemies will reinforce it again
    public static void ObjectiveLost(Objective objective)
    {
        
    }

    // when all enemies are defeated, the objective is secured
    public static void ObjectiveSecured()
    {
        
    }
    #endregion

    #region AI Manager stuff
    public static Character GetTargetCharacter(int faction_tag, Character curr_character, float max_range = 1000f, TargetType targ_type = TargetType.Closest)
    {
        return THE_OVERSEER.ai_manager.GetTargetCharacter(faction_tag, curr_character, max_range, targ_type);
    }
    #endregion

    #region Positioning
    public static Vector2Int GetValidTilePosition(Vector2 pos)
    {
        return Vector2Int.zero;
    }
    #endregion
}
