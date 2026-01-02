using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class AttackTypeInit // used to initialize ONE attack and its data
{
    public GameObject prefab;
    public AttackEnum attack_enum = AttackEnum.Projectile;
    private AttackEnum curr_atk = AttackEnum.MeleeAttack;

    public AttackData attack_data;
    public StatDictionary attack_stats = new StatDictionary();
    public Dictionary<string, float> leftovers = new Dictionary<string, float>();

    // template stats used to build the dictionary ADD/REMOVE stats HERE
    static readonly string[] projectile_stats =
    {
        "projectile_speed",
        "projectile_count",
        "projectile_spread",
        "even_spread",
        "homing_strength"
    };

    static readonly string[] linecast_stats =
    {
        "linecast_duration",
        "linecast_count",
        "linecast_spread",
        "even_spread"
    };

    static readonly string[] melee_stats =
    {
        "melee_duration",
        "melee_count",
        "melee_spread",
        "even_spread",
        "melee_size",
        "movement"
    };

    static readonly string[][] all_stats = {projectile_stats, linecast_stats, melee_stats};

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
                atk_type = new MeleeAttack(prefab,
                    attack_data,
                    attack_stat_dict["melee_duration"],
                    attack_stat_dict["melee_count"],
                    attack_stat_dict["melee_spread"],
                    attack_stat_dict["even_spread"],
                    attack_stat_dict["melee_size"]
                );
                break;
        }
        return atk_type;
    }
    public void OnValidate() // called by ItemSO
    {
        if (curr_atk != attack_enum) // create a new dictionary since it is a different attack type
        {
            BuildStats(attack_enum, attack_stats); // build new stat dictionary!
            curr_atk = attack_enum;
            Debug.Log("new attack type! Adjusting . . .");
        } 
        else if (attack_stats.Length != all_stats[(int)attack_enum].Length) // adjust values of dictionary if it doesn't have all the same stats as the current attack type
        {
            BuildStats(attack_enum, attack_stats);    
            Debug.Log("new stats detected! Adjusting...");
        }
    }

    public void BuildStats(AttackEnum enum_type, StatDictionary stats)
    {
        // remove all stats, save them just incase they're important tho
        foreach(StatDictItem stat in stats)
        {
            if (all_stats[(int)enum_type].Contains(stat.key))
            {
                leftovers[stat.key] = stat.value; // save potentially useful stats for later
            }
        }
        stats.Clear();

        // Add in relevant stats
        for(int i = 0; i < all_stats[(int)enum_type].Length; i++)
        {
            string key = all_stats[(int)enum_type][i];
            stats.Add(key, leftovers.ContainsKey(key) ? leftovers[key] : 0); // built new dictionary, and add defaults whenever a leftover cant be found
        }
    }
}