using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatIncrease", menuName = "ScriptableObjects/Upgrades/StatIncrease", order = 2)]
public class StatIncrease : UpgradeSO
{
    public string upgrade_name;
    public string upgrade_description;
    public Sprite upgrade_sprite;
    public CharStatModifier stat_modifier;
    public bool affect_entire_team = false;
    public float duration = 100000000f;
    public override void ApplyUpgrade()
    {
        if (affect_entire_team)
        {
            foreach(Operator squad_op in GameOverseer.THE_OVERSEER.squad_manager.operators)
            {
                stat_modifier.ApplyStats(squad_op, duration);
            }
        }
        else
        {
            stat_modifier.ApplyStats(GameOverseer.THE_OVERSEER.player_control.active_character, duration);
        }
    }

    public override (string, string, string) GetTextData()
    {
        string upgrade_stats = affect_entire_team ? "TEAM EFFECT\n" : "PLAYER EFFECT\n";
        
        // i need to find a better way to do this
        if (stat_modifier.max_health_boost != 0) {upgrade_stats += "Max HP Boost: " + stat_modifier.max_health_boost + "\n";}
        if (stat_modifier.health_regen != 0) {upgrade_stats += "HP Regen: " + stat_modifier.health_regen + " / " + stat_modifier.regen_rate + "s\n";}
        if (stat_modifier.shield_boost != 0) {upgrade_stats += "Shield: " + stat_modifier.shield_boost + "\n";}
        if (stat_modifier.speed_scale != 1) {upgrade_stats += "Speed Modifier" + 100 * stat_modifier.speed_scale + "% \n";}
        upgrade_stats += "Duration: " + duration + "s\n";


        return (upgrade_name, upgrade_stats, upgrade_description);
    }

    public override Sprite GetSprite()
    {
        return upgrade_sprite;
    }
}