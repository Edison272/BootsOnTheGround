using System;
using UnityEngine;

[Serializable]
public class Ability
{
    public AbilityComponent[] ability_components;
    public Character user;

    [Header("Ability Cooldown")]
    public float cooldown = 0f;

    public Action UpdateAction;

    public bool is_usable => cooldown >= 1;

    #region Initalization
    public Ability(float cooldown_preset = 0f)
    {
        if (cooldown_preset < 1)
        {
            UpdateAction = UpdateCooldown;
        } 
        else
        {
            UpdateAction = UpdateActive;
        }

        ability_components = new AbilityComponent[0];
    }
    #endregion

    #region Core
    public void UseAbility() // start the ability
    {
        UpdateAction = UpdateActive;
        cooldown = 0;
        foreach(AbilityComponent component in ability_components)
        {
            component.ActivateComponent();
        }
    }
    public void UpdateActive() // update any components while the ability is active
    {
        if (!IsActive()) // if ability is fully ended, go back on cooldown
        {
            UpdateAction = UpdateCooldown;
        }
    }
    public void UpdateCooldown() // check cooldowns while ability is on cooldown
    {
        cooldown += Time.deltaTime * 0.5f;
        if (cooldown >= 1)
        {
            cooldown = 1;
        }
    }

    public void EndAbility() // end the ability and resolve all related components
    {
        
    }
    #endregion

    #region Info
    public float GetAbilityCooldownProgress()
    {
        return cooldown;
    }
    public bool IsActive()
    {
        return false;
    }

    #endregion
}