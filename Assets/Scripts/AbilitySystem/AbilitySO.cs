using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Items", menuName = "ScriptableObjects/Abilities", order = 1)]
public class AbilitySO : ScriptableObject
{
    enum AbilityActiveType {STATS, ATTACK, EQUIP}
    enum AbilityEndType {INSTANT, TIME, KILLS}
    enum AbilityCooldownType {TIME, KILLS}
    [Header("Ability Active Type")] // What the ability does
    [SerializeField] AbilityActiveType ability_active_type = AbilityActiveType.EQUIP;
    
    [Header("Ability End Type")] // When the ability ends
    [SerializeField] AbilityCooldownType ability_end_type = AbilityCooldownType.TIME;

    [Header("Ability Cooldown Type")] // How the ability recovers
    [SerializeField] AbilityCooldownType ability_cooldown_type = AbilityCooldownType.TIME;
    [Header("Ability VFX")]  // VFX prefabs which will be attached to the user in various areas for the duration of the ability
    [SerializeField] GameObject[] vfx_prefabs;
    
    public Ability GenerateAbility(Operator give_operator) // generate an ability for an operator
    {
        return new Ability();
    }
    private void SetActiveType()
    {
        switch (ability_active_type)
        {
            case AbilityActiveType.ATTACK:
                break;
        }
    }

    private void SetRecoveryType()
    {
        switch (ability_cooldown_type)
        {
            case AbilityCooldownType.TIME:
                break;
        }
    }
}
