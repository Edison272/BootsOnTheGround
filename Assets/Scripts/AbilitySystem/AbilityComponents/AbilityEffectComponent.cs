using System;
using UnityEngine;

public abstract class AbilityEffectComponent
{
    public GameObject[] vfx_objects;
    public bool is_active;
    protected Operator user;
    public AbilityEffectComponent(Operator user)
    {
        this.user = user;
    }
    public abstract void ActivateComponent(); // do the thing!
    public abstract void DeactivateComponent(); // stop doing the thing

}

public class AbilityEffectStatModComponent : AbilityEffectComponent
{
    public CharStatModifier stat_modifiers;
    public Action EndStatMod;
    public AbilityEffectStatModComponent(Operator user, CharStatModifier stat_modifiers) : base(user)
    {
        this.stat_modifiers = stat_modifiers;
    }
    public override void ActivateComponent()
    {
        stat_modifiers.ApplyStats(user, 5);
    }
    public override void DeactivateComponent()
    {
        //EndStatMod();
    }
}

public class AbilityEffectItemComponent : AbilityEffectComponent
{
    public AbilityEffectItemComponent(Operator user) : base(user)
    {

    }
    public override void ActivateComponent()
    {
        
    }
    public override void DeactivateComponent()
    {
        
    }
}