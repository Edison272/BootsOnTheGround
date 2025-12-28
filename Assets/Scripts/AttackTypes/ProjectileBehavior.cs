using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    Vector2 source_pos;
    Transform target_char;
    Vector2 target_pos;

    [field: Header("VFX")]
    public GameObject main_body;
    public Transform vfx_body;
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90f);

    float stick_duration; // stick to a target for a set duration

    [field: Header("Projectile Data")]
    AttackData atk_data;
    public float speed;

    [field: Header("Physics")]
    public Rigidbody2D proj_rb;
    float travel_time;


    // Update is called once per frame
    void FixedUpdate()
    {
        if (target_char) {
            target_pos = ((Vector2)target_char.position - proj_rb.position).normalized * speed;
            proj_rb.velocity = Vector2.Lerp(proj_rb.velocity, target_pos, speed * Time.fixedDeltaTime);
        }

        // check if destination has been reached?
        travel_time -= Time.fixedDeltaTime;
        if (travel_time <= 0)
        {
            EndProjectile();
        }
    }

    public void StartProjectile(Projectile proj_data, Vector2 src_pos, Vector2 targ_pos, Vector2 item_tip) // straight shot variant
    {
        atk_data = proj_data.atk_data;
        speed = proj_data.projectile_speed;
        source_pos = src_pos;
        target_pos = targ_pos;

        // adjust vfx height from vfx body
        vfx_body.position = (Vector3)item_tip;

        // adjust vfx rotation
        Vector3 vfx_og_pos = vfx_body.transform.position;  // save original position for later
        Quaternion targ_rot = Quaternion.LookRotation(Vector3.forward, target_pos - source_pos) * ROTATION_OFFSET;
        main_body.transform.rotation = targ_rot;
        vfx_body.transform.position = vfx_og_pos;  // return vfx_body to original position after offset from rotation

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

    private void EndProjectile()
    {
        Destroy(this.gameObject);
    }
}
