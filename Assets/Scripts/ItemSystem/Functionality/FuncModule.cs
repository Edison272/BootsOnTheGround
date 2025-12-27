using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum FuncEnum {Gun, Melee, Shield}
public abstract class FuncModule
{    
    protected Item item;
    public FuncModule(Item item)
    {
        this.item = item;
    }

    public ItemAction[] item_actions;
    public abstract void UpdateModule(Vector2 target_pos);
    public abstract void UseFunction(int action_index);
    public abstract bool CanFunction();
    public abstract void Reset();
    
}


/*
Gun Platforms
- Rely on ammo to shoot (usually)
- Have Recoil that throws off aim
- weapon accuracy differs between standing & shooting
*/
public class Melee : FuncModule
/*
Melee Platforms
- use combo patterns to do different attacks
*/
{
    public Melee(Item item) : base(item)
    {
    }
    public override void UpdateModule(Vector2 target_pos)
    {
        throw new System.NotImplementedException();
    }

    public override void UseFunction(int action_index)
    {
        throw new System.NotImplementedException();
    }
    public override bool CanFunction()
    {
        return false;
    }

    public override void Reset()
    {
        throw new System.NotImplementedException();
    }
}
public class Shield : FuncModule
/*
Shield Platforms
- block attacks to trigger attacks
- blocking/damage reduction is based on direction
*/
{
    public Shield(Item item) : base(item)
    {
    }

    public override bool CanFunction()
    {
        throw new System.NotImplementedException();
    }

    public override void Reset()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateModule(Vector2 target_pos)
    {
        throw new System.NotImplementedException();
    }

    public override void UseFunction(int action_index)
    {
        throw new System.NotImplementedException();
    }
}
public class Conduit : FuncModule
/*
Conduit Platforms
- Switch between opposite states
- affect a target area varying in diameter
*/
{
    public Conduit(Item item) : base(item)
    {
    }

    public override bool CanFunction()
    {
        throw new System.NotImplementedException();
    }

    public override void Reset()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateModule(Vector2 target_pos)
    {
        throw new System.NotImplementedException();
    }

    public override void UseFunction(int action_index)
    {
        throw new System.NotImplementedException();
    }
}