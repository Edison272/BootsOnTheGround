using System;
using UnityEngine;

public class AbilityEffectComponent
{
    public GameObject[] vfx_objects;
    public bool is_active;
    private Operator user;
    public AbilityEffectComponent(Operator user)
    {
        this.user = user;
    }

    public void ActivateComponent() // do the thing!
    {
        
    }

    public void DeactivateComponent() // stop doing the thing
    {
        
    }
}