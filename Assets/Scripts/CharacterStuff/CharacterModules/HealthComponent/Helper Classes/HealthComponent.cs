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
    int bonus_health;

    public HealthComponent(int max_health, int start_shield, float spawn_health_perc = 1)
    {
        this.max_health = max_health;
        this.curr_health = (int)(max_health * spawn_health_perc);
        this.shield = start_shield;

        is_alive = true;
    }

    public void UpdateHealth()
    {
        
    }

    public void ChangeHealth(int damage_amt)
    {
        //Debug.Log(base_data.name + " has taken " + damage_amt + " damage");        
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

        } else // negative damage is healing
        {
            
        }
    }
    public void ChangeHealthTick(int change_amt, float duration, float tick_rate = 1)
    {

    }
    #region Stat Change Methods
    public void MaxHealthBoost(int boost_amt, float duration)
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