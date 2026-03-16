using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private CharacterSO[] enemy_presets;
    public HashSet<Character> enemies = new HashSet<Character>();
    private Dictionary<Character, int> enemy_group = new Dictionary<Character, int>();
    private Queue<int[]> wave_queue = new Queue<int[]>();

    public Vector3 drop_pos;
    
    public float curr_time = 0;

    [Header("Wave Spawning")]
    public float wave_time = 3;
    public float wave_radius = 3;
    public float spawn_dist = 14;
    public float min_wave_size;
    public float max_wave_size;
    public float min_wave_iterations;
    public float max_wave_iterations;
    public MajorObjective target_objective;

    [Header("Wave Spawning UI")]
    public Transform WaveBar;
    public TextMeshProUGUI enemies_remaining;

    public void CreateEnemy(int index, Vector3 position)
    {
        // setup the enemy character file
        Character new_enemy = enemy_presets[index].GenerateChar(position);
        new_enemy.gameObject.tag = this.gameObject.tag;
        new_enemy.faction_tag = GameOverseer.ENEMY_TAG;
        new_enemy.ConnectToEventBus(EnemyLost);
        new_enemy.SetFactionTag(1);

        // set up enemy behaviors
        new_enemy.ToggleAI(true);
        new_enemy.behavior_controller.anchor_position = GameOverseer.THE_OVERSEER.GetCurrentObjective().main_chunk.world_center_position;
        new_enemy.SetCommandBehavior(CommandMode.Engage);
        new_enemy.behavior_controller.SetActionTime(1, 2);
        enemies.Add(new_enemy);

        SetEnemyUI();
    }

    public void Update()
    {
        if (curr_time <= 0)
        {
            wave_queue.Dequeue();
            Debug.Log("Wave Inc");
            SummonEnemyWave((int)min_wave_size, (int)max_wave_size);
            curr_time += wave_time;
        }
        else if (wave_queue.Count > 0)
        {
            curr_time -= Time.deltaTime;
        }
    }

    public void FixedUpdate()
    {
        foreach(Character enemy in enemies)
        {
            if (enemy != null && enemy.IsInAction())
            {
                GameOverseer.THE_OVERSEER.map_manager.SetNewPosition(enemy);
            }
        }
    }

    public void AttackMajorObjective(MajorObjective major_objective)
    {
        target_objective = major_objective;
        SummonEnemyGroup();
    }

    public void SummonEnemyWave(int min_enemy_amt, int max_enemy_amt)
    {
        int enemy_amt = Random.Range(min_enemy_amt, max_enemy_amt+1);
        List<Vector2Int> random_chunk_list = target_objective.GetWeightedList();
        Vector2Int chunk_pos = random_chunk_list[Random.Range(0,random_chunk_list.Count)];
        Vector2 new_spawn_pos = MapManager.GetChunkWorldCenter(chunk_pos);

        for (int i = 0; i < enemy_amt; i++)
        {
            Vector3 spawn_pos = new_spawn_pos + Random.insideUnitCircle * Random.Range(0, MapManager.chunk_size/2.5f);
            CreateEnemy(Random.Range(0, enemy_presets.Length), spawn_pos);
        }
    }

    // summon an enemy group at target position. Use this for POI
    void SummonEnemyGroup()
    {
        SetEnemyUI("! ! !Cryptids Incoming ! ! !");
        int waves = Random.Range((int)min_wave_iterations, (int)max_wave_iterations+1);
        for (int w = 0; w < waves; w++)
        {
            wave_queue.Enqueue(new int[0] {});
        }
    }

    public void EnemyLost(Character character)
    {
        enemies.Remove(character);
        SetEnemyUI();
        if (enemies.Count == 0 && wave_queue.Count == 0)
        {
            GameOverseer.THE_OVERSEER.ObjectiveSecured();
            SetEnemyUI("=== Area Clear ===");

            // increment gang
            min_wave_size += 1f * GameOverseer.THE_OVERSEER.progression_level;
            max_wave_size += 1.5f * GameOverseer.THE_OVERSEER.progression_level;
            min_wave_iterations += 0.2f;
            max_wave_iterations += 0.4f;
        }
    }

    #region UI Stuff

    private void SetEnemyUI(string force_message = "")
    {
        string message = "Cryptids Remaining: " + enemies.Count;
        if (force_message != "")
        {
            message = force_message;
        }
        enemies_remaining.text = message;
    }

    #endregion
}
