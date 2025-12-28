using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AttackData
{
    // Attack on impact
    [SerializeField] int attack_dmg;
    [SerializeField] float knockback_amt;
    
    // status effect application
    [SerializeField] float effect_time;
    [SerializeField] float slow_amt;

}