using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class InputType
{
    float use_cd;
    float curr_cd;
    public InputType() {}
    public abstract void Use();
    public abstract void Stop();
}

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

public class ChargeInput : InputType // Use() to charge up overtime, output changes based on charge. Stop() = Output
{
    public ChargeInput() {}
    public override void Use()
    {
        
    }
    public override void Stop()
    {
        
    }
}

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