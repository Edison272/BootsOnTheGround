using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum AttackEnum { Projectile, Linecast, MeleeAttack }
public abstract class AttackType
{
    protected Character user;
    protected GameObject instance;

    public abstract void Attack(Vector2 source_pos, Vector2 target_pos, Vector2 output_pos, Vector2 vfx_target_offset, Character sender);
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
        projectile_count = (int)prj_count;
        projectile_spread = prj_sprd;
        even_spread = even_sprd > 0 ? true : false;
    }

    public override void Attack(Vector2 source_pos, Vector2 target_pos, Vector2 output_pos, Vector2 vfx_target_offset, Character sender = null)
    {
        // declare info that doesn't need to be in a loop
        float target_dist = (target_pos - source_pos).magnitude;
        Vector2 target_dir = (target_pos - source_pos).normalized;
        float target_ang = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        for (int i = 0; i < projectile_count; i++)
        {
            GameObject projectile = GameObject.Instantiate(instance, source_pos, Quaternion.identity);
            ProjectileBehavior projectile_data = projectile.GetComponent<ProjectileBehavior>();

            // add the inherent inaccuracy value of projectile
            if (even_spread)
            {
                float angle_inc = projectile_spread / (projectile_count - 1);
                float offset_ang = (-projectile_spread / 2f) + (angle_inc * i);
                float final_ang = target_ang + offset_ang;

                Vector2 dir = new Vector2(Mathf.Cos(final_ang * Mathf.Deg2Rad),Mathf.Sin(final_ang * Mathf.Deg2Rad));
                
                target_pos = source_pos + dir * target_dist;
            }
            else
            {
                target_pos += Random.insideUnitCircle * target_dist * projectile_spread/360;
            }
            // new projectile! 
            projectile_data.StartProjectile(this, source_pos, target_pos, output_pos, vfx_target_offset, sender);
        }

    }

}
public class Linecast : AttackType
{
    public Linecast(GameObject instance) : base(instance)
    {
    }

    public override void Attack(Vector2 source_pos, Vector2 target_pos, Vector2 output_pos, Vector2 vfx_target_offset, Character sender)
    {
        throw new System.NotImplementedException();
    }
}



public class MeleeAttack : AttackType
{
    public MeleeAttack(GameObject instance) : base(instance)
    {
    }

    public override void Attack(Vector2 source_pos, Vector2 target_pos, Vector2 output_pos, Vector2 vfx_target_offset, Character sender)
    {
        throw new System.NotImplementedException();
    }
}
