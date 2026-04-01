using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemInputModules{
    public abstract class ItemInputModule
    {
        public ItemInputController input_controller;
        public AttackType[] attack_types;
        public ItemInputModule(ItemInputController input_controller)
        {
            this.input_controller = input_controller;
        }
        public abstract void Use();
        public abstract void Stop();
        public abstract void Reset();
        public abstract float GetStatus(); // return a 0-1 float value for readiness
        public abstract int PredictActionIndex();
        public abstract void ChangeUseSpeed(float scalar); // multiple something by the scalar to increase/slowdown an item's use speed
    }

    #region Constant Input
    public class ConstantInputModule : ItemInputModule // standard input. Use() = Output
    {
        float use_cd = 0.1f;  // time between uses
        float tap_use_cd = 0.1f;
        float next_use = 0; // the  point in time this can be used
        public ConstantInputModule(ItemInputController input_controller, float hold_attack_speed, float tap_attack_speed = -1) : base (input_controller)
        {
            this.use_cd = hold_attack_speed;
            this.tap_use_cd = hold_attack_speed;
            if (tap_attack_speed != -1)
            {
                this.tap_use_cd = tap_attack_speed;
            }
            
        }
        public override void Use()
        {
            if (next_use <= Time.time)
            {
                next_use = Time.time + use_cd;
            }
        }
        public override void Stop()
        {
            next_use = Time.time + tap_use_cd;
        }
        public override void Reset()
        {
            next_use = Time.time;
        }

        public override float GetStatus() // return a float reflecting when the next attack will be ready. 0 = ready, 1 = not ready
        {
            return Mathf.Min(1, 1f - ((next_use - Time.time) / use_cd));
        }
        public override int PredictActionIndex()
        {
            return 0;
        }

        public override void ChangeUseSpeed(float scalar) // generate cooldown reduction
        {
            use_cd *= scalar;
        }
    }
    #endregion

    #region Charge Input
    public class ChargeInput : ItemInputModule // Use() to charge up overtime, output changes based on charge. Stop() = Output
    {
        float curr_charge; // current charge
        float threshold; // minimum required charge
        float max_charge; // maximum charge
        int charge_states; // how many levels of charge in between
        public ChargeInput(ItemInputController input_controller, float threshold, float max_charge, int charge_states) : base (input_controller)
        {
            this.threshold = threshold;
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
            //item.Action((int)(charge_states * ((curr_charge - threshold) / (max_charge+0.01f))));
            curr_charge = 0;
        }
        public override void Reset()
        {
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

        public override int PredictActionIndex()
        {
            throw new NotImplementedException();
        }
    }
    #endregion
    #region Uses Input
    public class UsesInput : ItemInputModule // Control how many times this can be used before it can't function
    {
        int curr_uses; // current charge
        int max_uses; // maximum charge
        public UsesInput(ItemInputController input_controller, int max_uses) : base (input_controller)
        {
            this.max_uses = max_uses;
            curr_uses = max_uses;
        }
        public override void Use()
        {
            if (curr_uses > 0)
            {
                curr_uses -= 1;
            }
        }
        public override void Stop(){}
        public override void Reset()
        {
            curr_uses = max_uses;
        }

        public override float GetStatus() // return a float for the percentage of charge
        {
            return curr_uses/max_uses;
        }
        public override void ChangeUseSpeed(float scalar) // nothing
        {

        }

        public override int PredictActionIndex()
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}