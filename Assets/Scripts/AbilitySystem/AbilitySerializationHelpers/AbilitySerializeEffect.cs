using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class AbilitySerializeEffect
{
    [Header("Stat Boosts")]
    [SerializeField] CharStatModifier[] char_stat_modifiers = new CharStatModifier[0];

    [Header("Special Item")]
    [SerializeField] ItemSO special_item = null;

    [Header("Ability Attack")]
    [SerializeField] AttackTypeInit[] attacks = null;
    public AbilityEffectComponent[] CreateAbilityComponents(Operator user)
    {
        List<AbilityEffectComponent> effect_components = new List<AbilityEffectComponent>();
        string stats_present = "Create Components: ";
        if (char_stat_modifiers.Length > 0)
        {
            effect_components.Add(new AbilityEffectStatModComponent(user, char_stat_modifiers[0]));
            stats_present += "stats modified, ";
        }
        if (special_item)
        {
            effect_components.Add(new AbilityEffectItemComponent(user));
            stats_present += "give special item, ";
        }
        if (attacks.Length > 0)
        {
            foreach(AttackTypeInit attack in attacks)
            {
                stats_present += "launch attack, ";
            }
        }
        Debug.Log(stats_present);
        return effect_components.ToArray();
    }
    public void OnValidate()
    {
        
    }

}