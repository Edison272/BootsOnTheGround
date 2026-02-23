using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Ability
{
    public AbilitySO base_data;
    public AbilityEffectComponent[] ability_components;
    public AbilityRecoveryComponent ability_recovery;
    private Operator user;

    [Header("Ability Cooldown")]
    private Action UpdateAction;

    public bool is_usable => GetAbilityCooldownProgress() == 1;

    public GameObject[] ability_vfx;

    #region Initalization
    public Ability(AbilitySO base_data, Operator user, AbilityRecoveryComponent ability_recovery, float cooldown_preset = 0f)
    {
        this.base_data = base_data;
        this.user = user;
        this.ability_recovery = ability_recovery;

        ability_components = new AbilityEffectComponent[0];
        ability_vfx = new GameObject[base_data.ability_vfx.Length];
        if (cooldown_preset < 1)
        {
            //UpdateAction = ability_recovery.UpdateRecovery;
        } 
        else
        {
            UpdateAction = UpdateActive;
        }

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
        UpdateAction = UpdateActive;
        ability_recovery.ResetRecovery();
        foreach(AbilityEffectComponent component in ability_components)
        {
            component.ActivateComponent();
        }
        ToggleAbilityVFX(true);
    }

    public void UpdateAbility()
    {
        //UpdateAction();
    }
    public void UpdateActive() // update any components while the ability is active
    {
        // if (!IsActive()) // if ability is fully ended, go back on cooldown
        // {
        //     UpdateAction = ability_recovery.UpdateRecovery;
        // }
    }

    public void EndAbility() // end the ability and resolve all related components
    {
            
    }
    #endregion

    #region 
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
        return false;
    }

    #endregion
}