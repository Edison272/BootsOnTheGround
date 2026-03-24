using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        return true;
    }
    public override float FunctionCompletion()
    {
        throw new System.NotImplementedException();
    }
    public override void ResetData()
    {
        throw new System.NotImplementedException();
    }
}