using UnityEngine;
using System;
using System.Collections.Generic;

public sealed class FunctionalityController
{
    private Item item;
    public List<FuncModule> function_modules = new List<FuncModule>();
    public FunctionalityController(Item item)
    {
        this.item = item;
    }
    public void UpdateModule(Vector2 target_pos)
    {

    }
    public void UseFunction(int action_index)
    {
        
    }

    public bool CanFunction()
    {
        return true;
    }
    public float FunctionCompletion()
    {
        return 1;
    }
    public void ResetData()
    {
        
    }
}