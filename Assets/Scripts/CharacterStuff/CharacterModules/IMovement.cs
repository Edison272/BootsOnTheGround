using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public interface IMovement
{
    Rigidbody2D entity_rb {get;}

    // basic movement
    float move_speed {get;} // speed of movement
    float base_speed {get;} // base speed
    Vector2 move_dir {get;} // mover's intended direction of movement
    Vector2 move_pos {get;} // mover's intended direction of movement
    bool destination_reached {get;} // prevent mover from spamming movement when they're already there
    Vector2 last_move_dir {get;} // track previous move_dir

    // handle knockback
    float force_move_time {get;} // prevents mover from moving while > 0
    Vector2 force_dir {get;} // where mover is being forced

    // handle acceleration
    float curr_speed {get;}
    float curr_accel_time {get;}
    float max_accel_time {get;} // amount of time operator needs to get to top move speed
    
    void Move(); // update rb position based on move_dir
    void SetMove(Vector2 set_move_dir); // set the move_dir
    void StopMove(); // set move_dir to zero
    void ForceMove(Vector2 direction, float scalar, bool movement_override = false); // apply knockback or dashing
    void ChangeSpeed(float scale_base, float duration, bool is_decaying = false); // increase or decrease speed
}