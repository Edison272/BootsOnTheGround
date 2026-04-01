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
    public abstract float GetAtkSpread();
}
#region Projectile
public class Projectile : AttackType
{
    public AttackData atk_data;
    public ProjectileTypeData type_data;
    public Projectile(GameObject instance, AttackData atk_data, ProjectileTypeData type_data) : base(instance)
    {
        this.atk_data = atk_data;
        this.type_data = type_data;
    }

    public override void Attack(Vector2 source_pos, Vector2 target_pos, Vector2 output_pos, Vector2 vfx_target_offset, Character sender = null)
    {
        // declare info that doesn't need to be in a loop
        float target_dist = (target_pos - source_pos).magnitude;
        float og_speed = type_data.projectile_speed;
        Vector2 target_dir = (target_pos - source_pos).normalized;
        float target_ang = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        for (int i = 0; i < type_data.projectile_count; i++)
        {
            GameObject projectile = GameObject.Instantiate(instance, source_pos, Quaternion.identity);
            ProjectileBehavior projectile_data = projectile.GetComponent<ProjectileBehavior>();

            // add the inherent inaccuracy value of projectile
            if (type_data.even_spread)
            {
                float angle_inc = type_data.projectile_spread / type_data.projectile_count;
                float offset_ang = (-type_data.projectile_spread / 2f) + (angle_inc * i);
                float final_ang = target_ang + offset_ang;

                Vector2 dir = new Vector2(Mathf.Cos(final_ang * Mathf.Deg2Rad),Mathf.Sin(final_ang * Mathf.Deg2Rad));
                
                target_pos = source_pos + dir * target_dist;
            }
            else
            {
                Vector2 og_targ_pos = target_pos;
                target_pos += Random.insideUnitCircle * target_dist * type_data.projectile_spread/360;
                
                type_data.projectile_speed *= Mathf.Clamp(1 - (target_pos - og_targ_pos).magnitude / target_dist, 0.5f, 1);
            }
            // new projectile! 
            projectile_data.StartProjectile(this, source_pos, target_pos, output_pos, vfx_target_offset, sender);
            type_data.projectile_speed = og_speed;
        }
    }

    public override float GetAtkSpread() {return type_data.projectile_spread;}
}
#endregion

#region Linecast
// linecasts occur once a projectile has exceeded a certain speed
public class Linecast : Projectile
{
    public Linecast(GameObject instance, AttackData atk_data, ProjectileTypeData type_data)
     : base(instance, atk_data, type_data){}

    public override void Attack(Vector2 source_pos, Vector2 target_pos, Vector2 output_pos, Vector2 vfx_target_offset, Character sender)
    {
        // declare info that doesn't need to be in a loop
        float target_dist = (target_pos - source_pos).magnitude;
        Vector2 target_dir = (target_pos - source_pos).normalized;
        float target_ang = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        for (int i = 0; i < type_data.projectile_count; i++)
        {
            GameObject linecast = GameObject.Instantiate(instance, source_pos, Quaternion.identity);
            LinecastBehavior linecast_data = linecast.GetComponent<LinecastBehavior>();

            // add the inherent inaccuracy value of projectile
            if (type_data.even_spread)
            {
                float angle_inc = type_data.projectile_spread / type_data.projectile_count;
                float offset_ang = (-type_data.projectile_spread / 2f) + (angle_inc * i);
                float final_ang = target_ang + offset_ang;

                Vector2 dir = new Vector2(Mathf.Cos(final_ang * Mathf.Deg2Rad),Mathf.Sin(final_ang * Mathf.Deg2Rad));
                
                target_pos = source_pos + dir * target_dist;
            }
            else
            {
                target_pos += Random.insideUnitCircle * target_dist * type_data.projectile_spread/360;
            }
            // new linecast! 
            linecast_data.StartLinecast(this, source_pos, target_pos, output_pos, vfx_target_offset, sender);
        }
    }

    public override float GetAtkSpread() {return type_data.projectile_spread;}
}
#endregion
#region Melee Attack
public class MeleeAttack : AttackType
{
    public AttackData atk_data;
    public MeleeTypeData type_data;


    public MeleeAttack(GameObject instance, AttackData atk_data, MeleeTypeData type_data) : base(instance)
    {
        this.atk_data = atk_data;
        this.type_data = type_data;
    }

    public override void Attack(Vector2 source_pos, Vector2 target_pos, Vector2 output_pos, Vector2 vfx_target_offset, Character sender = null)
    {
        // declare info that doesn't need to be in a loop
        float target_dist = (target_pos - source_pos).magnitude;
        Vector2 target_dir = (target_pos - source_pos).normalized;
        float target_ang = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        for (int i = 0; i < type_data.melee_count; i++)
        {
            GameObject melee_ins = GameObject.Instantiate(instance, source_pos, Quaternion.identity);
            MeleeBehavior melee_data = melee_ins.GetComponent<MeleeBehavior>();

            // add the inherent inaccuracy value of projectile
            if (type_data.even_spread)
            {
                float angle_inc = type_data.melee_spread / (type_data.melee_count - 1);
                float offset_ang = (-type_data.melee_spread / 2f) + (angle_inc * i);
                float final_ang = target_ang + offset_ang;

                Vector2 dir = new Vector2(Mathf.Cos(final_ang * Mathf.Deg2Rad),Mathf.Sin(final_ang * Mathf.Deg2Rad));
                
                target_pos = source_pos + dir * target_dist;
            }
            else
            {
                target_pos += Random.insideUnitCircle * target_dist * type_data.melee_spread/360;
            }
            // new projectile! 
            melee_data.StartMelee(this, source_pos, target_pos, output_pos, vfx_target_offset, sender);
        }
    }

    public override float GetAtkSpread() {return type_data.melee_spread;}
}
#endregion