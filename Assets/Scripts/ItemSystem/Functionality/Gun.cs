using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Gun : FuncModule
{
    // base data
    int ammo; // how much ammo the gun uses before reloading
    int max_ammo; // how much the weapon starts/reloads with
    float recoil; // how much the aimed position is offset with each shot
    ItemAction[] action_array; // what attacks this thing can do
    Vector2 target_pos;
    Vector2 recoil_dir;
    public Gun(Item item, float max_ammo) : base(item)
    {
        this.max_ammo = (int)max_ammo;
        ammo = this.max_ammo; 
    }

    public override bool CanFunction()
    {
        return ammo > 0;
    }

    public override void Reset()
    {
        ammo = max_ammo;
    }

    public override void UpdateModule(Vector2 targ_pos)
    {
        target_pos = targ_pos + recoil_dir;
    }

    public override void UseFunction(int action_index)
    {
        ammo -= 1;
    }
}
