using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum AttackEnum {Projectile, Linecast, MeleeAttack}
public abstract class AttackType
{
    protected Character user;
    protected GameObject instance;

    public abstract void Attack(Vector3 source_pos, Vector3 target_pos, Vector3 vfx_point);
    public AttackType(GameObject instance)
    {
        this.instance = instance;
    }
}
public class Projectile : AttackType
{
    public AttackData atk_data;
    public float projectile_speed;
    int projectile_count;
    float projectile_spread;
    bool even_spread;
    

    public Projectile(GameObject instance, AttackData atk_data, float prj_spd, float prj_count, float prj_sprd, float even_sprd) : base(instance)
    {
        this.atk_data = atk_data;
        projectile_speed = prj_spd;
        projectile_count = (int) prj_count;
        projectile_spread = prj_sprd;
        even_spread = even_sprd > 0 ? true : false;
    }

    public override void Attack(Vector3 source_pos, Vector3 target_pos, Vector3 item_tip)
    {
        GameObject projectile = GameObject.Instantiate(instance,source_pos,Quaternion.identity);
        ProjectileBehavior projectile_data = projectile.GetComponent<ProjectileBehavior>();
        
        projectile_data.StartProjectile(this, source_pos, target_pos, item_tip);
    }

}
public class Linecast : AttackType
{
    public Linecast(GameObject instance) : base(instance)
    {
    }

    public override void Attack(Vector3 source_pos, Vector3 target_pos, Vector3 vfx_point)
    {
        throw new System.NotImplementedException();
    }
}



public class MeleeAttack : AttackType
{
    public MeleeAttack(GameObject instance) : base(instance)
    {
    }

    public override void Attack(Vector3 source_pos, Vector3 target_pos, Vector3 vfx_point)
    {
        throw new System.NotImplementedException();
    }
}
