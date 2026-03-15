using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

[System.Serializable]
public class Gun : FuncModule
{
    // base data
    public int ammo {get; private set;} // how much ammo the gun uses before reloading
    public int max_ammo {get; private set;}// how much the weapon starts/reloads with

    [Header("Recoil Base Stats")]
    float recoil_increment; // how much the aimed position is offset with each shot
    float recoil_multiplier; // how much the aimed position is offset with each shot
    float recoil_max;
    float recoil_recovery;
    float recoil_decrement;
    [Header("Recoil Current Stats")]
    public float curr_recoil {get; private set;}
    Vector2 target_pos;
    public Gun(Item item, 
        float max_ammo, 
        float recoil_increment, 
        float recoil_multiplier, 
        float recoil_max, 
        float recoil_recovery) : base(item)
    {
        this.max_ammo = (int)max_ammo;
        ammo = this.max_ammo; 
        curr_recoil = 0;
        this.recoil_increment = recoil_increment;
        this.recoil_multiplier = recoil_multiplier;
        this.recoil_max = recoil_max;
        this.recoil_recovery = recoil_recovery;
    }

    public override bool CanFunction()
    {
        return ammo > 0;
    }
    public override float FunctionCompletion()
    {
        return (float)ammo / max_ammo;
    }

    public override void ResetData()
    {
        ammo = max_ammo;
        curr_recoil = 0;
    }

    public override void UpdateModule(Vector2 targ_pos)
    {
        target_pos = targ_pos;
        if (item.get_input_ready > 0.75)
        {
            curr_recoil = Mathf.Max(0, curr_recoil - (Time.deltaTime/recoil_recovery)); 
        }
    }

    public override void UseFunction(int action_index)
    {
        float target_dist = (target_pos - item.source_pos).magnitude;
        Vector2 recoil_dir = Random.insideUnitCircle * curr_recoil * target_dist;
        attacks[action_index].Attack(item.source_pos, target_pos + recoil_dir, item.item_tip.position, new Vector2(0, item.y_offset), item.user);
        ammo -= 1;
        
        
        curr_recoil = Mathf.Min(recoil_max, (curr_recoil  + recoil_increment) * recoil_multiplier);
        if (item.user == GameOverseer.THE_OVERSEER.player_control.active_character)
        {
            GameOverseer.THE_OVERSEER.player_control.ApplyCameraRecoil(item.source_pos - target_pos, Math.Min(curr_recoil, curr_recoil/recoil_max));
        }
    }
}
