using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

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
    [Header("Recoil Current Stats")]
    float curr_recoil;
    float recoil_decrement;
    Vector2 target_pos;
    Vector2 recoil_dir;
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

        recoil_decrement = recoil_max / recoil_recovery;
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
        recoil_dir = Vector2.zero;
    }

    public override void UpdateModule(Vector2 targ_pos)
    {
        target_pos = targ_pos + recoil_dir;

        curr_recoil = Mathf.Max(0, curr_recoil - recoil_decrement * Time.deltaTime);
        recoil_dir *= 1-Time.deltaTime;
    }

    public override void UseFunction(int action_index)
    {
        attacks[action_index].Attack(item.source_pos, target_pos, item.item_tip.position, new Vector2(0, item.y_offset), item.user);
        ammo -= 1;

        // for player recoil
        if (item.user == GameOverseer.THE_OVERSEER.player_control.active_character)
        {
            GameOverseer.THE_OVERSEER.player_control.ApplyCameraRecoil(item.source_pos - target_pos, recoil_increment);
        }
        
        curr_recoil = Mathf.Min(recoil_max, (recoil_increment + curr_recoil) * recoil_multiplier);
        recoil_dir += Random.insideUnitCircle * curr_recoil;
    }
}
