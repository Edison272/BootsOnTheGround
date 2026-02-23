using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class AttackStatData
{
    // template stats used to build the dictionary ADD/REMOVE stats HERE
    #region Attack Types
    public static readonly string[] projectile_stats =
    {
        "projectile_speed",
        "projectile_count",
        "projectile_spread",
        "even_spread",
        "homing_strength"
    };

    public static readonly string[] linecast_stats =
    {
        "linecast_duration",
        "linecast_count",
        "linecast_spread",
        "even_spread"
    };

    public static readonly string[] melee_atk_stats =
    {
        "melee_duration",
        "melee_count",
        "melee_spread",
        "even_spread",
        "melee_size",
        "movement"
    };

    public static readonly string[][] all_attack_stats = {projectile_stats, linecast_stats, melee_atk_stats};
    public static string[] GetAtkType(AttackEnum enum_type)
    {
        return all_attack_stats[(int)enum_type];
    }
    #endregion

    #region Input Types

    #endregion

    #region Functionality Types
    public static readonly string[] gun_stats =
    {
        
    };
    public static readonly string[] melee_stats =
    {
        
    };
    public static readonly string[] shield_stats =
    {
        
    };
    
    #endregion
}