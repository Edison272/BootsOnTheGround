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
    int item_index;
    public AbilityEffectItemComponent(Operator user, ItemSO new_item) : base(user)
    {
        ability_item = new_item.GenerateItem(user.main_hand);
        item_index = user.AddItem(ability_item);
    }
    public override void ActivateComponent()
    {
        user.SwitchItem(item_index);
    }
    public override void DeactivateComponent()
    {
        user.ResetItems();
        user.SwitchItem(0);
    }
}