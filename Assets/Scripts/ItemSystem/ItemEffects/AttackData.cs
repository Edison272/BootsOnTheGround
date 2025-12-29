using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackData
{
    // Attack on impact
    public int damage;
    [SerializeField] float knockback_amt;
    
    // status effect application
    [SerializeField] float effect_time;
    [SerializeField] float slow_amt;

}