using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public CharacterSO[] enemy_presets;
    public Character[] enemies;

    public Vector3 drop_pos;

    public void CreateEnemies()
    {
        enemies = new Character[enemy_presets.Length];
        for(int i = 0; i < enemy_presets.Length; i++)
        {
            enemies[i] = enemy_presets[i].GenerateOp(transform.position);
            enemies[i].gameObject.tag = this.gameObject.tag;
            enemies[i].ToggleAI(true);
            enemies[i].SetCommandBehavior(CommandMode.Hold);
        }
    }
}
