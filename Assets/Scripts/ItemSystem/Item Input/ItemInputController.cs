using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class ItemInputController
{    
    protected Item item;
    public ItemInputController(Item item)
    {
        this.item = item;
    }
    public abstract void Use();
    public abstract void Stop();
    public abstract void Reset();
    public abstract float GetStatus(); // return a 0-1 float value for readiness
    public abstract int PredictActionIndex();
    public abstract void ChangeUseSpeed(float scalar); // multiple something by the scalar to increase/slowdown an item's use speed
}