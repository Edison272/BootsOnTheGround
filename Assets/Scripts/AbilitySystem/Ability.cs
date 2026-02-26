using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Ability
{
    public AbilitySO base_data;
    public AbilityEffectComponent[] ability_effects;
    public AbilityRecoveryComponent ability_recovery;
    public AbilityDurationComponent ability_duration;
    private Operator user;

    [Header("Ability Cooldown")]

    public bool is_usable => GetAbilityCooldownProgress() >= 1;

    public GameObject[] ability_vfx;

    #region Initalization
    public Ability(
        AbilitySO base_data, 
        Operator user, 
        AbilityEffectComponent[] ability_effects,
        AbilityRecoveryComponent ability_recovery, 
        AbilityDurationComponent ability_duration
        )
    {
        this.base_data = base_data;
        this.user = user;
        this.ability_effects = ability_effects;
        this.ability_recovery = ability_recovery;
        this.ability_duration = ability_duration;

        ability_vfx = new GameObject[base_data.ability_vfx.Length];

        // assign vfx from base data
        int i = 0;
        foreach(BodyPartDictItem body_part_item in base_data.ability_vfx)
        {
            Transform body_part_location = user.GetBodyPart(body_part_item.value);            
            GameObject new_body_part = MonoBehaviour.Instantiate(body_part_item.key, body_part_location);
            new_body_part.SetActive(false);
            ability_vfx[i] = new_body_part;
            i++;
        }
    }
    #endregion

    #region Core
    public void UseAbility() // start the ability
    {
        foreach(AbilityEffectComponent effect_component in ability_effects)
        {
            effect_component.ActivateComponent();
        }
        // disable Ability Recovery
        ability_recovery.ResetRecovery();
        ability_recovery.SetRecoveryActive(false);

        // turn on ability state
        ability_duration.ResetDuration();
        ability_duration.SetDurationActive(true);

        ToggleAbilityVFX(true);
    }

    public void UpdateAbility()
    {
        if (ability_duration.duration_complete)
        {
            EndAbility();
        }
    }
    public void EndAbility() // end the ability and resolve all related components
    {
        foreach(AbilityEffectComponent effect_component in ability_effects)
        {
            effect_component.DeactivateComponent();
        }
        ability_recovery.ResetRecovery();
        ability_recovery.SetRecoveryActive(true);

        ability_duration.ResetDuration();
        ability_duration.SetDurationActive(false);
        ToggleAbilityVFX(false);
    }

    public void ResetAbility()
    {
        ToggleAbilityVFX(false);
        foreach (AbilityEffectComponent effect_component in ability_effects)
        {
            effect_component.ResetComponent();
        }
        ability_duration.ResetComponent();
        ability_recovery.ResetComponent();
    }
    #endregion

    #region VFX
    public void ToggleAbilityVFX(bool is_enabled)
    {
        foreach(GameObject vfx_object in ability_vfx)
        {
            vfx_object.SetActive(is_enabled);
        }
    }

    #endregion

    #region Info
    public float GetAbilityCooldownProgress()
    {
        return ability_recovery.AbilityRecoveryCompletion();
    }
    public bool IsActive()
    {
        return !ability_duration.duration_complete;
    }

    #endregion
}