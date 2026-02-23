using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityActiveType {STATS, ATTACK, EQUIP}
public enum AbilityEndType {INSTANT, TIME, KILLS}
public enum AbilityRecoveryType {Time, Kills, DamageTaken}
[CreateAssetMenu(fileName = "Items", menuName = "ScriptableObjects/Abilities", order = 1)]
public class AbilitySO : ScriptableObject
{   
    [Header("Ability Active Type")] // What the ability does
    [SerializeField] AbilityActiveType set_ability_active_type = AbilityActiveType.EQUIP;
    private AbilityActiveType curr_ability_active_type = AbilityActiveType.EQUIP;
    
    [Header("Ability End Type")] // When the ability ends
    [SerializeField] AbilityRecoveryType ability_end_type = AbilityRecoveryType.Time;

    [Header("Ability Recovery Type")] // How the ability recovers
    [SerializeField] AbilityRecoveryType ability_recovery_type = AbilityRecoveryType.Time;
    private AbilityRecoveryType curr_ability_recovery_type = AbilityRecoveryType.Kills;
    [SerializeField] StatDictionary serialized_recovery_stats;
    [Header("Ability VFX")]  // VFX prefabs which will be attached to the user in various areas for the duration of the ability
    public BodyPartDictionary ability_vfx;
    
    public Ability GenerateAbility(Operator give_operator) // generate an ability for an operator
    {
        return new Ability(this, give_operator, SetRecoveryType(give_operator));
    }
    private void SetActiveType()
    {
        switch (set_ability_active_type)
        {
            case AbilityActiveType.ATTACK:
                break;
        }
    }

    private AbilityRecoveryComponent SetRecoveryType(Operator give_operator)
    {
        Dictionary<string, float> recovery_stats = serialized_recovery_stats.ToDictionary();
        AbilityRecoveryComponent ability_recovery = null;
        ability_recovery = new AbilityRecoveryComponent(ability_recovery_type, give_operator, serialized_recovery_stats.Value(0));
        return ability_recovery;
    }

    #region Scriptable Object Serialization
    private bool ValidateDictionary(StatDictionary check_dict) // return true if the dict is empty or null
    {
        return check_dict == null || check_dict.Length == 0;
    }
    public void OnValidate()
    {
        if (curr_ability_recovery_type != ability_recovery_type || ValidateDictionary(serialized_recovery_stats))
        {
            if (serialized_recovery_stats == null)
            {
                serialized_recovery_stats = new StatDictionary();
            }
            
            serialized_recovery_stats.Clear();
            switch (ability_recovery_type)
            {
                case AbilityRecoveryType.Time:
                    serialized_recovery_stats.Add("recovery_time", 10f);
                    break;
                case AbilityRecoveryType.Kills:
                    serialized_recovery_stats.Add("kills", 5f);
                    break;
                case AbilityRecoveryType.DamageTaken:
                    serialized_recovery_stats.Add("damage_taken", 500f);
                    break;
            }
            curr_ability_recovery_type = ability_recovery_type;
        }
    }
    #endregion
}
