using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemInputModules;
public sealed class ItemInputController
{    
    private Item item;
    private List<ItemInputModule> inputs = new List<ItemInputModule>();
    public ItemInputController(Item item)
    {
        this.item = item;
    }
    public void AddInputModule(ItemInputModule add_input)
    {
        inputs.Add(add_input);
    }
    public void Use()
    {
        foreach(ItemInputModule input in inputs)
        {
            input.Use();
        }
    }
    public void Stop()
    {
        foreach(ItemInputModule input in inputs)
        {
            input.Stop();
        }
    }
    public void Reset()
    {
        foreach(ItemInputModule input in inputs)
        {
            input.Reset();
        }
    }
    public int PredictActionIndex() {return 0;}
    public void ChangeUseSpeed(float scalar) // multiple something by the scalar to increase/slowdown an item's use speed
    {
        foreach(ItemInputModule input in inputs)
        {
            input.ChangeUseSpeed(scalar);
        }
    }

    public void ItemAction(float scale)
    {
        
    }

    public float GetStatus()
    {
        return inputs[0].GetStatus();
    }
}