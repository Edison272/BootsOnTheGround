using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public enum InputEnum {Normal, Charge, Increment}
public abstract class InputType
{    
    protected Action<int> InputEffect;
    public InputType(Action<int> InputEffect)
    {
        this.InputEffect = InputEffect; // connect item effect function
    }
    public abstract void Use();
    public abstract void Stop();
    public abstract float GetStatus(); // return a 0-1 float value for readiness
    public abstract void ChangeUseSpeed(float scalar); // multiple something by the scalar to increase/slowdown an item's use speed
}

[System.Serializable]
public class NormalInput : InputType // standard input. Use() = Output
{
    float use_cd;  // time between uses
    float next_use = 0; // the  point in time this can be used
    public NormalInput(float use_cd, Action<int> InputEffect) : base(InputEffect)
    {
        this.use_cd = use_cd;
    }
    public override void Use()
    {
        if (next_use > Time.time)
        {
            InputEffect(0);
            next_use = Time.time + use_cd;
        }
    }
    public override void Stop()
    {
        
    }

    public override float GetStatus() // return a float reflecting when the next attack will be ready. 0 = ready, 1 = not ready
    {
        return Mathf.Min(1, 1f - ((next_use - Time.time) / use_cd));
    }

    public override void ChangeUseSpeed(float scalar) // generate cooldown reduction
    {
        use_cd *= scalar;
    }
}
[System.Serializable]
public class ChargeInput : InputType // Use() to charge up overtime, output changes based on charge. Stop() = Output
{
    float curr_charge; // current charge
    float max_charge; // maximum charge
    int charge_states; // how many levels of charge in between
    public ChargeInput(float max_charge, int charge_states, Action<int> InputEffect) : base(InputEffect)
    {
        this.max_charge = max_charge;
        this.charge_states = charge_states;
    }
    public override void Use()
    {
        curr_charge += Time.deltaTime;
        if (curr_charge > max_charge) {
            curr_charge = max_charge;
        }
    }
    public override void Stop()
    {
        // (int) charge_levels * (max_charge)
        curr_charge = 0;
    }

    public override float GetStatus() // return a float for the percentage of charge
    {
        return curr_charge/max_charge;
    }
    public override void ChangeUseSpeed(float scalar) // significantly reduce the time for a max charge
    {
        curr_charge *= scalar;
        max_charge *= scalar;
    }
}
[System.Serializable]
public class IncrementInput : InputType // Output changes depending on extended duration of use
{
    float use_cd; // time between uses
    float next_use; // the next time this can be used
    float curr_inc; // current incrementation level
    float max_inc; // maximum incrememntation level
    int inc_states; // how many levels of incrementation
    public IncrementInput(float use_cd, float max_inc, float inc_states, Action<int> InputEffect) : base(InputEffect)
    {
        
    }
    public override void Use()
    {
        if (next_use > Time.time)
        {
            // do the effect or whatnot based on curr_inc/max_inc and some other stuff
            next_use = Time.time + use_cd;
        }
        // increment
        curr_inc += Time.deltaTime;
        if (curr_inc > max_inc) {
            curr_inc = max_inc;
        }
    }
    public override void Stop()
    {
        curr_inc = 0;
    }

    public override float GetStatus() // return a float for the percentage of incrementation
    {
        return Mathf.Min(1, curr_inc/max_inc);
    }
    public override void ChangeUseSpeed(float scalar) // reduce cooldown AND maximum incrementation
    {
        use_cd *= scalar;
        curr_inc *= scalar;
        max_inc *= scalar;
    }
}