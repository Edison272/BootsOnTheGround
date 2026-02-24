using System;
using UnityEngine;

public abstract class AbilityEffectComponent
{
    public GameObject[] vfx_objects;
    public bool is_active {get; protected set;}
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
    CharStatModifier stat_modifiers;
    public AbilityEffectStatModComponent(Operator user, CharStatModifier stat_modifiers) : base(user)
    {
        this.stat_modifiers = stat_modifiers;
    }
    public override void ActivateComponent()
    {
        is_active = true;
        stat_modifiers.ApplyStats(user, 100000, this);
    }
    public override void DeactivateComponent()
    {
        is_active = false;
    }
}

public class AbilityEffectItemComponent : AbilityEffectComponent
{
    Item ability_item;
    public AbilityEffectItemComponent(Operator user) : base(user)
    {

    }
    public override void ActivateComponent()
    {
        user.SwitchItem();
    }
    public override void DeactivateComponent()
    {
        user.ResetItems();
        user.SwitchItem();
    }
}