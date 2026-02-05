using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackData
{
    // Attack on impact
    public int damage;
    public int pierce;
    [SerializeField] [Range(0f, 100f)] float knockback_amt;
    
    // status effect application
    [SerializeField] float effect_time;
    [SerializeField] [Range(0.01f, 10.0f)] float slow_amt;

    public void ApplyData(Vector3 source_pos, GameObject target)
    {
        target.GetComponent<IHealth>()?.ChangeHealth(damage);
        IMovement targ_move = target.GetComponent<IMovement>();
        if (knockback_amt > 0)
        {
            targ_move?.ForceMove((target.transform.position - source_pos).normalized, knockback_amt);
        }
        
        if (effect_time > 0)
        {
            targ_move?.ChangeSpeed(slow_amt, effect_time);
        }
    }

}