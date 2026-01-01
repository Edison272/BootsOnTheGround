using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackTypeInit // used to initialize ONE attack and its data
{
    public GameObject prefab;
    public AttackEnum attack_enum = AttackEnum.Projectile;
    private AttackEnum curr_atk = AttackEnum.MeleeAttack;

    public AttackData attack_data;
    public StatDictionary attack_stats;

    public bool IsDictSetUp()
    {
        return attack_enum == curr_atk;
    }
    public AttackType CreateAttackType()
    {
        AttackType atk_type = null;
        Dictionary<string, float> attack_stat_dict = attack_stats.ToDictionary();
        switch (attack_enum)
        {
            case AttackEnum.Projectile:
                atk_type = new Projectile(prefab,
                    attack_data,
                    attack_stat_dict["projectile_speed"],
                    attack_stat_dict["projectile_count"],
                    attack_stat_dict["projectile_spread"],
                    attack_stat_dict["even_spread"],
                    attack_stat_dict["homing_strength"]
                );
                break;
            case AttackEnum.Linecast:
                atk_type = new Linecast(prefab,
                    attack_data,
                    attack_stat_dict["linecast_duration"],
                    attack_stat_dict["linecast_count"],
                    attack_stat_dict["linecast_spread"],
                    attack_stat_dict["even_spread"]
                );
                break;
            case AttackEnum.MeleeAttack:
                atk_type = new MeleeAttack(prefab);
                break;
        }
        return atk_type;
    }
    public void OnValidate() // called by ItemSO
    {
        if (curr_atk != attack_enum)
        {
            switch (attack_enum)
            {
                case AttackEnum.Projectile:
                    attack_stats = new StatDictionary
                    {
                        {"projectile_speed", 0.1f},
                        {"projectile_count", 1f},
                        {"projectile_spread", 0f},
                        {"even_spread", 0f},
                        {"homing_strength", 0f}
                    };
                    break;
                case AttackEnum.Linecast:
                    attack_stats = new StatDictionary
                    {
                        {"linecast_duration", 0.1f},
                        {"linecast_count", 1f},
                        {"linecast_spread", 0f},
                        {"even_spread", 0f},
                    };
                    break;
                case AttackEnum.MeleeAttack:
                    attack_stats = new StatDictionary
                    {
                        {"melee_duration", 0.1f}
                    };
                    break;
            }
            curr_atk = attack_enum;
        }
    }
}