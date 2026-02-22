using System;
using UnityEngine;

public class AbilityComponent
{
    public GameObject[] vfx_objects;
    public bool is_active;
    private Operator user;
    public AbilityComponent(Operator user)
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