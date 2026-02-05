using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackData
{
    // Attack on impact
    public int damage;
    public int pierce;
    [SerializeField] float knockback_amt;
    
    // status effect application
    [SerializeField] float effect_time;
    [SerializeField] float slow_amt;

    public void ApplyData(Vector3 source_pos, GameObject target)
    {
        target.GetComponent<IHealth>()?.ChangeHealth(damage);
        target.GetComponent<IMovement>()?.ForceMove((target.transform.position - source_pos).normalized, knockback_amt);
    }

}