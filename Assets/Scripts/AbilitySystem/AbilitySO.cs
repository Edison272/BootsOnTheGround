using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityActiveType {STATS, ATTACK, EQUIP}
public enum AbilityDurationType {Time, ItemUses, DamageTaken, Endless}
public enum AbilityRecoveryType {Time, Kills, DamageTaken}
[CreateAssetMenu(fileName = "Items", menuName = "ScriptableObjects/Abilities", order = 1)]
public class AbilitySO : ScriptableObject
{   
    [Header("Ability Active Type")] // What the ability does
    [SerializeField] AbilityActiveType set_ability_active_type = AbilityActiveType.EQUIP;
    public AbilitySerializeEffect serialize_ability_effect;
    private AbilityActiveType curr_ability_active_type = AbilityActiveType.EQUIP;
    
    [Header("Ability Duration Type")] // When the ability ends
    [SerializeField] AbilityRecoveryType serialize_ability_duration = AbilityRecoveryType.Time;
    private AbilityRecoveryType curr_ability_duration = AbilityRecoveryType.Kills;
    [SerializeField] StatDictionary serialized_duration_stats;

    [Header("Ability Recovery Type")] // How the ability recovers
    [SerializeField] AbilityRecoveryType serialize_ability_recovery = AbilityRecoveryType.Time;
    private AbilityRecoveryType curr_ability_recovery = AbilityRecoveryType.Kills;
    [SerializeField] StatDictionary serialized_recovery_stats;
    [Header("Ability VFX")]  // VFX prefabs which will be attached to the user in various areas for the duration of the ability
    public BodyPartDictionary ability_vfx;
    
    public Ability GenerateAbility(Operator give_operator) // generate an ability for an operator
    {
        return new Ability(
            this, give_operator, 
            SetEffectComponents(give_operator), 
            SetRecoveryComponent(give_operator),
            SetDurationComponent(give_operator)
        );
    }
    #region  Create Ability Components
    private AbilityEffectComponent[] SetEffectComponents(Operator give_operator)
    {
        return serialize_ability_effect.CreateAbilityComponents(give_operator);
    }
    private AbilityRecoveryComponent SetRecoveryComponent(Operator give_operator)
    {
        Dictionary<string, float> recovery_stats = serialized_recovery_stats.ToDictionary();
        AbilityRecoveryComponent ability_recovery = new AbilityRecoveryComponent(serialize_ability_recovery, give_operator, serialized_recovery_stats.Value(0));
        return ability_recovery;
    }

    private AbilityDurationComponent SetDurationComponent(Operator give_operator)
    {
        Dictionary<string, float> recovery_stats = serialized_duration_stats.ToDictionary();
        AbilityDurationComponent ability_duration = new AbilityDurationComponent(serialize_ability_duration, give_operator, serialized_duration_stats.Value(0));
        return ability_duration;
    }
    #endregion

    #region SO Serialization
    private bool ValidateDictionary(StatDictionary check_dict) // return true if the dict is empty or null
    {
        return check_dict == null || check_dict.Length == 0;
    }
    public void OnValidate()
    {
        if (curr_ability_recovery != serialize_ability_recovery || ValidateDictionary(serialized_recovery_stats))
        {
            if (serialized_recovery_stats == null)
            {
                serialized_recovery_stats = new StatDictionary();
            }
            
            serialized_recovery_stats.Clear();
            switch (serialize_ability_recovery)
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
            curr_ability_recovery = serialize_ability_recovery;
        }

        serialize_ability_effect.OnValidate();

        if (curr_ability_duration != serialize_ability_duration || ValidateDictionary(serialized_duration_stats))
            {
                if (serialized_duration_stats == null)
                {
                    serialized_duration_stats = new StatDictionary();
                }
                
                serialized_duration_stats.Clear();
                switch (serialize_ability_duration)
                {
                    case AbilityRecoveryType.Time:
                        serialized_duration_stats.Add("recovery_time", 10f);
                        break;
                    case AbilityRecoveryType.Kills:
                        serialized_duration_stats.Add("kills", 5f);
                        break;
                    case AbilityRecoveryType.DamageTaken:
                        serialized_duration_stats.Add("damage_taken", 500f);
                        break;
                }
                curr_ability_duration = serialize_ability_duration;
            }
        }
    #endregion
}
