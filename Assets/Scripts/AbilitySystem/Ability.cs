using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Ability
{
    public AbilitySO base_data;
    public AbilityComponent[] ability_components;
    private Operator user;

    [Header("Ability Cooldown")]
    public float cooldown = 0f;

    private Action UpdateAction;

    public bool is_usable => cooldown >= 1;

    public GameObject[] ability_vfx;

    #region Initalization
    public Ability(AbilitySO base_data, Operator user, float cooldown_preset = 0f)
    {
        this.base_data = base_data;
        this.user = user;
        ability_components = new AbilityComponent[0];
        ability_vfx = new GameObject[base_data.ability_vfx.Length];
        if (cooldown_preset < 1)
        {
            UpdateAction = UpdateCooldown;
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
        cooldown = 0;
        foreach(AbilityComponent component in ability_components)
        {
            component.ActivateComponent();
        }
        ToggleAbilityVFX(true);
    }

    public void UpdateAbility()
    {
        UpdateAction();
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
        cooldown += Time.deltaTime * 0.25f;
        if (cooldown >= 1)
        {
            ToggleAbilityVFX(false);
            cooldown = 1;
        }
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
        return cooldown;
    }
    public bool IsActive()
    {
        return false;
    }

    #endregion
}