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
    [SerializeField] AbilityActiveType set_ability_active_type = AbilityActiveType.EQUIP;
    private AbilityActiveType curr_ability_active_type = AbilityActiveType.EQUIP;
    
    [Header("Ability End Type")] // When the ability ends
    [SerializeField] AbilityCooldownType ability_end_type = AbilityCooldownType.TIME;

    [Header("Ability Cooldown Type")] // How the ability recovers
    [SerializeField] AbilityCooldownType ability_cooldown_type = AbilityCooldownType.TIME;
    [field: Header("Serialization")]
    [SerializeField] private FuncEnum curr_func = FuncEnum.Shield; // detect when the function type has changed to update it
    [SerializeField] StatDictionary serialized_input_stats;
    [SerializeField] StatDictionary serialized_functionality_stats;
    [Header("Ability VFX")]  // VFX prefabs which will be attached to the user in various areas for the duration of the ability
    public BodyPartDictionary ability_vfx;
    
    public Ability GenerateAbility(Operator give_operator) // generate an ability for an operator
    {
        return new Ability(this, give_operator);
    }
    private void SetActiveType()
    {
        switch (set_ability_active_type)
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

    #region Scriptable Object Serialization
    private bool ValidateDictionary(StatDictionary check_dict) // return true if the dict is empty or null
    {
        return check_dict == null || check_dict.Length == 0;
    }
    public void OnValidate()
    {

    }
    #endregion
}
