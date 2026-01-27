using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    Vector2 source_pos;
    Transform target_char;
    Vector2 target_pos;
    Vector2 vfx_target_offset;

    [field: Header("VFX")]
    public GameObject main_body;
    public Transform vfx_body;

    float stick_duration; // stick to a target for a set duration

    [field: Header("Projectile Data")]
    AttackData atk_data;
    public float speed;

    [field: Header("Physics")]
    public Rigidbody2D proj_rb;
    float travel_time;
    float curr_travel_time = 0;

    [field: Header("Ownership")]
    string faction_tag = "Untagged";
    Character owner;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != faction_tag && collision.gameObject.tag != "NoHit")
        {
            collision.gameObject.GetComponent<IHealth>()?.ChangeHealth(atk_data.damage);
            ProjectileEffects();
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (target_char) {
            target_pos = ((Vector2)target_char.position - proj_rb.position).normalized * speed;
            proj_rb.velocity = Vector2.Lerp(proj_rb.velocity, target_pos, speed * Time.fixedDeltaTime);
        }

        // check if destination has been reached?
        curr_travel_time += Time.fixedDeltaTime;
        if (curr_travel_time >= travel_time)
        {
            EndProjectile();
        }

        // set vfx
        vfx_body.position = Vector2.MoveTowards(vfx_body.position, proj_rb.position + vfx_target_offset, travel_time * Time.fixedDeltaTime);

    }

    public void StartProjectile(Projectile proj_data, Vector2 src_pos, Vector2 targ_pos, Vector2 output_pos, Vector2 vfx_targ_offset, Character sender = null) // straight shot variant
    {
        atk_data = proj_data.atk_data;
        speed = proj_data.projectile_speed;
        source_pos = src_pos;
        target_pos = targ_pos;
        vfx_target_offset = vfx_targ_offset;
        owner = sender;
        if (owner)
        {
            faction_tag = owner.gameObject.tag;
        }

        // adjust vfx rotation
        Vector2 target_dir = (target_pos - source_pos).normalized;
        float angle = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        vfx_body.rotation = Quaternion.Euler(0, 0, angle);

        // adjust vfx height from vfx body
        vfx_body.position = output_pos;

        // Vector2 source_offset = output_pos - target_dir * (output_pos.x - source_pos.x)/target_dir.x;
        // float offset_y = source_offset.y - source_pos.y;
        // vfx_body.localPosition = new Vector3(0, offset_y, 0);

        // adjust vfx rotation
        // Vector3 vfx_og_pos = vfx_body.transform.position;  // save original position for later
        // Quaternion targ_rot = Quaternion.LookRotation(Vector3.forward, target_pos - source_pos) * ROTATION_OFFSET;
        // main_body.transform.rotation = targ_rot;
        // vfx_body.transform.position = vfx_og_pos;  // return vfx_body to original position after offset from rotation
        // vfx_body.localPosition = new Vector3(0, vfx_body.localPosition.y, 0);

        // move this thing
        proj_rb.velocity = (target_pos - source_pos).normalized * speed;

        // set travel time to know when to terminate the projectile
        float distance = Vector2.Distance(source_pos, target_pos);
        travel_time = distance / speed;
    }

    public void StartProjectile(Transform target_char) // homing vairant
    {
        this.target_char = target_char;
    }

    private void ProjectileEffects()
    {
        EndProjectile();
    }

    private void EndProjectile()
    {
        Destroy(this.gameObject);
    }
}
