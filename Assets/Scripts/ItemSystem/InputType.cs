using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class InputType
{
    public bool full_auto; // if not, this input is tap to shoot (semi auto)  
    float use_cd;  // time before this input can be used again
    float curr_cd; // current time

    Item item;
    ItemEffect[] effects;
    public InputType() {}
    public abstract void Use();
    public abstract void Stop();
}

[System.Serializable]
public class NormalInput : InputType // standard input. Use() = Output
{
    public NormalInput(float use_cd) {}
    public override void Use()
    {
        
    }
    public override void Stop()
    {
        
    }
}
[System.Serializable]
public class ChargeInput : InputType // Use() to charge up overtime, output changes based on charge. Stop() = Output
{
    public ChargeInput() {}
    public override void Use()
    {
        
    }
    public override void Stop()
    {
        // (int) charge_levels * (max_charge)
    }
}
[System.Serializable]
public class IncrementInput : InputType // Output changes based on how long Use is pressed down
{
    public IncrementInput() {}
    public override void Use()
    {
        
    }
    public override void Stop()
    {
        
    }
}