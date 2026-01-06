using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public interface IMovement
{
    Rigidbody2D entity_rb {get;}
    float curr_speed {get;} // speed of movement
    float base_speed {get;} // base speed
    Vector2 move_dir {get;} // mover's intended direction of movement
    float force_move_time {get;} // prevents mover from moving while > 0
    Vector2 force_dir {get;} // where mover is being forced
    
    void Move(); // update rb position based on move_dir
    void SetMove(Vector2 set_move_dir); // set the move_dir
    void StopMove(); // set move_dir to zero
    void ForceMove(Vector2 direction, float scalar); // apply knockback or dashing
    void ChangeSpeed(float scale_base); // increase or decrease speed
}