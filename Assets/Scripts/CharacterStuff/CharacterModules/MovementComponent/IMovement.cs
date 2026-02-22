using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public interface IMovement
{
    Rigidbody2D entity_rb {get;}

    MovementComponent movement_component {get;}
    
    void Move(); // update rb position based on move_dir
    void SetMove(Vector2 set_move_dir); // set the move_dir
    void StopMove(); // set move_dir to zero
    void ForceMove(Vector2 direction, float scalar, bool movement_override = false); // apply knockback or dashing
    void ChangeSpeed(float scale_base, float duration, bool is_decaying = false); // increase or decrease speed
}

[System.Serializable]
public struct SpeedModifier
{
    public float modifier_scale {get; private set;}
    public float duration;
    float decay_rate;
    public SpeedModifier(float modifier_scale, float duration, bool is_decaying)
    {
        this.modifier_scale = modifier_scale;
        this.duration = duration;
        decay_rate = is_decaying ? modifier_scale/duration : 0;
    }
    public bool UpdateModifier(float deltaTime)
    {
        duration -= deltaTime;
        modifier_scale -= decay_rate;
        return duration <= 0;
    }

}