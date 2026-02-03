using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private CharacterSO[] enemy_presets;
    public HashSet<Character> enemies = new HashSet<Character>();

    public Vector3 drop_pos;
    
    public float curr_time = 0;

    [Header("Wave Spawning")]
    public float wave_time = 3;
    public float wave_radius = 3;
    public float spawn_dist = 14;
    public int min_wave_size;
    public int max_wave_size;
    public Vector2[] spawn_positions = {
        new Vector2(-1, 0), 
        new Vector2(1, 0), 
        new Vector2(-0.8660254f, 0.5f), 
        new Vector2(-0.8660254f, -0.5f), 
        new Vector2(0.8660254f, 0.5f),
        new Vector2(0.8660254f, -0.5f)
    }; // TEMPORARY

    public void CreateEnemy(int index, Vector3 position)
    {
        // setup the enemy character file
        Character new_enemy = enemy_presets[index].GenerateChar(position);
        new_enemy.gameObject.tag = this.gameObject.tag;
        new_enemy.ConnectToEventBus(EnemyLost);

        // set up enemy behaviors
        new_enemy.ToggleAI(true);
        new_enemy.behavior_controller.anchor_position = GameOverseer.THE_OVERSEER.squad_manager.player_character.GetPosition();
        new_enemy.SetCommandBehavior(CommandMode.Engage);
        new_enemy.behavior_controller.SetActionTime(1, 2);
    }

    public void Update()
    {
        if (curr_time <= 0)
        {
            Vector2 position = GameOverseer.THE_OVERSEER.squad_manager.player_character.GetPosition() + spawn_positions[Random.Range(0, spawn_positions.Length)] * spawn_dist;
            SummonEnemyWave(position, wave_radius, min_wave_size, max_wave_size);
            curr_time += wave_time;
        }
        else
        {
            curr_time -= Time.deltaTime;
        }
    }

    public void SummonEnemyWave(Vector2 position, float radius, int min_enemy_amt, int max_enemy_amt)
    {
        int enemy_amt = Random.Range(min_enemy_amt, max_enemy_amt+1);
        for (int i = 0; i < enemy_amt; i++)
        {
            Vector3 spawn_pos = position + Random.insideUnitCircle * Random.Range(0, radius);
            CreateEnemy(Random.Range(0, enemy_presets.Length), spawn_pos);
        }
    }

    public void EnemyLost(Character character)
    {
        enemies.Remove(character);
    }
}
