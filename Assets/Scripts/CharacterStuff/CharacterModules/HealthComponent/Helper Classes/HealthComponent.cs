using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HealthComponent
{
    [field: SerializeField] public int curr_health {get; private set;}
    [field: SerializeField] public int max_health {get; private set;}
    [field: SerializeField] public int shield {get; private set;}
    public float health_ratio => curr_health/(float)max_health;
    public int total_curr_hitpoints => curr_health + shield;
    public int total_max_hitpoints => max_health + shield;
    [field: SerializeField] public bool is_alive {get; private set;}
    //public float  {get; private set;}

    [Header("Stat Modifiers")]
    [SerializeField] List<ChangeHealthTick> health_ticks = new List<ChangeHealthTick>();

    public HealthComponent(int max_health, int start_shield, float spawn_health_perc = 1)
    {
        this.max_health = max_health;
        this.curr_health = (int)(max_health * spawn_health_perc);
        this.shield = start_shield;

        is_alive = true;
    }

    public void UpdateHealth()
    {
        if (health_ticks.Count > 0)
        {
            int net_health_change = 0;
            for(int i = health_ticks.Count-1; i >= 0; i--)
            {
                ChangeHealthTick health_tick = health_ticks[i];
                if (health_tick.effect_complete)
                {
                    // swap n pop removal
                    int list_end = health_ticks.Count - 1;
                    health_ticks[i] = health_ticks[list_end];
                    health_ticks.RemoveAt(list_end); 
                }
                else
                {
                    net_health_change += health_tick.UpdateTick();
                    health_ticks[i] = health_tick;
                }
            }
            if (net_health_change != 0)
            {
                ChangeHealth(net_health_change);
            }
        }
    }

    public void ChangeHealth(int damage_amt)
    {     
        if (damage_amt > 0) // this is damage
        {
            shield -= damage_amt;
            damage_amt = -shield;
            if (shield <= 0)
            {
                shield = 0;
            } 
            if (damage_amt > 0)
            {
                curr_health -= damage_amt;
                if (curr_health <= 0)
                {
                    is_alive = false;
                }
            }

        } 
        else // negative damage is healing
        {
            curr_health -= damage_amt;
            if (curr_health > max_health)
            {
                curr_health = max_health;
            }
        }
    }
    public void ChangeHealthTick(int change_amt, float duration, float tick_rate, AbilityEffectComponent effect_controller)
    {
        health_ticks.Add(new ChangeHealthTick(change_amt, tick_rate, duration, effect_controller));
    }
    #region Stat Change Methods
    public void MaxHealthBoost(int boost_amt, float duration, AbilityEffectComponent effect_controller)
    {
        float curr_ratio = health_ratio;
        max_health += boost_amt;
        curr_health = (int)(max_health * curr_ratio);
    }
    public void ShieldBoost(int boost_amt)
    {
        shield += boost_amt;
    }
    #endregion
}