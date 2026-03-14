using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;


public enum TargetType {Closest, Furthest, MostHP, LeastHP}
public class GameOverseer : MonoBehaviour // this thing starts up everything else and keeps track of the game
{
    [Header("Monobehaviour Managers")]
    public PlayerController player_control;
    public PlayerActionController player_action_control;
    public SquadManager squad_manager;
    public EnemyManager enemy_manager;
    public MapManager map_manager;

    [Header("Canvas Managers")]
    public CanvasController canvas_control;
    public ItemUIController item_ui_control;
    public HealthUIController health_ui_control;
    public SquadUIController squad_ui_control;
    public OrderUIController order_ui_control;

    [Header("Private Managers")]
    [SerializeField] private AIManager ai_manager; // a class containing all the stuff the game ai would need
    [SerializeField] private ObjectiveManager objective_manager;
    public bool GenerateRandomMap = false;

    public static GameOverseer THE_OVERSEER {get; private set;}

    // Layermask stuff
    public static readonly LayerMask avoid_map_mask = 1 << 7; // check if map is in the way of this raycast
    public static readonly LayerMask avoid_obstacles_mask = (1 << 6) | (1 << 7); // check if map and entities are in the way of this raycast
    public static readonly LayerMask find_interactable_mask = 1 << 9;
    public static readonly LayerMask find_characters_mask = 1 << 6;
    public static readonly LayerMask characters_and_interactables_mask = (1 << 6) | (1 << 9);
    // Faction stuff
    [SerializeField] private Color serialize_squad_color;
    [SerializeField] private Color serialize_enemy_color;
    public static Color SQUAD_COLOR;
    public static Color ENEMY_COLOR;
    public static Color EMPTY_COLOR = Color.white;
    public static readonly int SQUAD_TAG = 0;
    public static readonly int ENEMY_TAG = 1;

    [Header("Game Over")]
    public GameObject game_over_screen;
    public GameObject game_succeed_screen;
    public Action GameOverBus;

    [Header("Game Progression")]
    public int progression_level {get; private set;} = 1;
    private GameObject ExtractionPoint;
    public GameObject UpgradeScreen;

    // Objective Management
    #region Initialziation
    void Awake()
    {
        THE_OVERSEER = this;

        if (!player_control) {player_control = GameObject.Find("Player Controller")?.GetComponent<PlayerController>();}
        if (!player_action_control) {player_action_control = GameObject.Find("Action Controller")?.GetComponent<PlayerActionController>();}
        if (!squad_manager) {squad_manager = GameObject.Find("Squad Manager")?.GetComponent<SquadManager>();}
        if (!enemy_manager) {enemy_manager = GameObject.Find("Enemy Manager")?.GetComponent<EnemyManager>();}
        if (!map_manager) {map_manager = GameObject.Find("Map")?.GetComponent<MapManager>();}
        
        if (!canvas_control) {canvas_control = GameObject.Find("Canvas Controller")?.GetComponent<CanvasController>();}
        if (!item_ui_control) {item_ui_control = GameObject.Find("Item UI Controller")?.GetComponent<ItemUIController>();}
        if (!health_ui_control) {health_ui_control = GameObject.Find("Health UI Controller")?.GetComponent<HealthUIController>();}
        if (!squad_ui_control) {squad_ui_control = GameObject.Find("Squad UI Controller")?.GetComponent<SquadUIController>();}
        if (!order_ui_control) {order_ui_control = GameObject.Find("Order UI Controller")?.GetComponent<OrderUIController>();}

        map_manager.SetMapGenPreset();
        objective_manager = new ObjectiveManager(this, map_manager.GetMapGenPreset().objectives);
        game_over_screen.SetActive(false);
        UpgradeScreen.SetActive(false);

        SQUAD_COLOR = serialize_squad_color;
        ENEMY_COLOR = serialize_enemy_color;
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
        // prepare objectives after map generation
        objective_manager.PrepareObjectives();

        // let everyone get to know eachother
        squad_manager.player = player_control;
        squad_manager.squad_ui_control = squad_ui_control;

        player_control.squad = squad_manager;
        player_control.item_ui_control = item_ui_control;
        player_control.health_ui_control = health_ui_control;
        player_control.order_ui_control = order_ui_control;
        player_control.player_action_controller = player_action_control;

        // initialize squad and enemy managers in the right places
        squad_manager.InitializeAllies();
        canvas_control.SetOperatorProfiles();

        // start player controller
        player_control.StartPlayer();

        // setup an AI manager after base data has been created
        ai_manager = new AIManager(this, map_manager.Wall, map_manager.Floor);
    }

    #region  Next Level
    // create an extraction zone that allows the player to leave the level
    public void CreateExtraction()
    {
        MajorObjective final_mo = map_manager.critical_locs[map_manager.critical_locs.Length - 1];
        GameObject final_objective_point = final_mo.objective_point.gameObject;
        final_objective_point.SetActive(false);
        if (!ExtractionPoint)
        {
            ExtractionPoint = GameObject.Instantiate(Resources.Load<GameObject>("MapObjects/Areas/Extraction"), final_objective_point.transform.position, Quaternion.identity);
        }
    }
    // called by the extraction zone script. open ui to give player upgrades
    public void GetUpgrades()
    {
        UpgradeScreen.SetActive(true);
        player_control.FreeCursor(true);
    }
    public void LevelUp() // called when player interacts with extraction
    {
        player_control.FreeCursor(true);
        
        progression_level++;
        Destroy(ExtractionPoint);
        map_manager.GenerateMap(progression_level);
        squad_manager.transform.position = (Vector2)map_manager.GetChunkWorldPos(map_manager.spawn_chunk);
        enemy_manager.transform.position = (Vector2)map_manager.GetChunkWorldPos(map_manager.final_chunk);

        objective_manager.ResetObjectives(map_manager.GetMapGenPreset().objectives);

        squad_manager.SetPlayerSquad();
        UpgradeScreen.SetActive(false);
    }
    #endregion

    void Update()
    {
        objective_manager.UpdateObjectiveManager();
        //Debug.Log(THE_OVERSEER.objective_manager.frontier_objective + ", " + THE_OVERSEER.objective_manager.total_objectives);
    }
    #endregion
    #region Game Events
    public void GameOver()
    {
        if (!game_succeed_screen.activeSelf)
        {
            game_over_screen.SetActive(true);
        }
        player_control.FreeCursor(true);
    }

    public void GameWin()
    {
        Debug.Log("win");
        game_succeed_screen.SetActive(true);
        player_control.FreeCursor(true);
    }

    public void ReturnToMain()
    {
        SceneManager.LoadScene("MainMenu");
    }
    #endregion
    #region Objective Manager Stuff
    // when an objective is captured, send enemies to recature it
    public void ObjectiveCaptured(MajorObjective maj_poi)
    {
        THE_OVERSEER.objective_manager.ObjectiveCaptured(maj_poi);
    }

    // when an objective is lost, enemies will reinforce it again
    public void ObjectiveLost(MajorObjective maj_poi)
    {
        THE_OVERSEER.objective_manager.ObjectiveLost(maj_poi);
    }

    // when all enemies are defeated, the objective is secured
    public void ObjectiveSecured()
    {
        THE_OVERSEER.objective_manager.ObjectiveSecured();
        if (THE_OVERSEER.objective_manager.objectives_complete)
        {
            CreateExtraction();
        }
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
