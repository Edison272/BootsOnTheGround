using UnityEngine;

[System.Serializable]
public class HealthComponent
{
    [field: SerializeField] public int curr_health {get; private set;}
    [field: SerializeField] public int max_health {get; private set;}
    [field: SerializeField] public int shield {get; private set;}
    [field: SerializeField] public float health_ratio {get; private set;}
    [field: SerializeField] public bool is_alive {get; private set;}

    public HealthComponent(int max_health, int start_shield, float spawn_health_perc = 1)
    {
        this.max_health = max_health;
        this.curr_health = (int)(max_health * spawn_health_perc);
        this.shield = start_shield;

        is_alive = true;
        health_ratio = curr_health/max_health;
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

        health_ratio = curr_health/(float)max_health;
    }
}