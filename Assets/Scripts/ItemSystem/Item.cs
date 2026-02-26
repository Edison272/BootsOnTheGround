using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{   
    [SerializeField] ItemSO base_data; // SO contains important base data
    // Input Instance
    InputType input_type;

    [field: Header("VFX Body")]
    public GameObject item_object;
    public Transform rotator_object; // rotate the object when aiming
    public Transform item_tip; // the "front" of an item which attack type vfx will align to
    public Animator animator;
    public float y_offset {get; private set;} // vfx y offset from the target position
    

    [field: Header("Aiming")]
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90f); // RotateTowards() is stupid so we need to offset it
    public float rot_scale = 1f; // 0 for no rotation, 1 for instantaneous rotation
    Quaternion curr_rot; // save the current quaternion rotation
    public Vector2 aim_pos; // where the item is supposed to be aimed towards;
    public Vector2 source_pos;
    public Vector2 target_pos; // where the item is actually aimed towards (based on rot_scale)
    bool freeze_aiming = false;     // stop this thing from aiming and updating target position
    public Action AimVFX;

    [field: Header("Data")]
    public Character user;

    [field: Header("Functionality")]
    public FuncModule func_module;
    public AttackType[] attacks;
    public bool is_equipped = false; // can only use an item if it is equipped
    public float reset_timer {get; private set;} // block this weapon's if disabled time > 0

    [field: Header("Modifiers")]
    public float use_spd_scale = 1f;
    public float reset_spd_scale = 1f;

    [Header("Interface")]
    public Sprite ui_image => base_data.ui_image;

    // Setup immutable item data when this object is made
    public void Setup(ItemSO base_data, InputType input_type, FuncModule func_module, AttackType[] atk_types)
    {            
        this.base_data = base_data;
        this.input_type = input_type;
        this.func_module = func_module;
        attacks = atk_types;
        func_module.attacks = atk_types;
        // set aim type
        if (base_data.dynamic_aim) {
            AimVFX = DynamicAim;
        } 
        else {
            AimVFX = StaticAim;
        }
        rot_scale = base_data.rot_scale;
    }

    // adjust item everytime theres a new user
    public void NewUser(Character new_user)
    {
        user = new_user;
        // if no rotator object, rotator object is the user's hand (items in use are always a child transform of something else)
        if (!rotator_object)
        {
            rotator_object = transform.parent;
        }

        y_offset = this.transform.position.y - user.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (reset_timer > 0)
        {
            reset_timer -= Time.deltaTime;
            if (reset_timer <= 0)
            {
                //functionality.
                animator.SetBool("Resetting", false);
                func_module.Reset();
                reset_timer = 0;

            }
        }
        func_module.UpdateModule(target_pos);
    }

    // setup the weapon for the user whenever it's picked up or equipped
    public void EquipItem()
    {
        animator.SetBool("IsEquipped", true);
    }

    public void UnequipItem()
    {
        animator.SetBool("IsEquipped", false);
    }

    public void SetEquipped(int int_is_equipped)
    {
        is_equipped = int_is_equipped > 0 ? true : false;
    }

    // Update the target position of this item and adjust VFX accordingly. Only perform when used by a character
    public void Aim(Vector2 aim_dir)
    {
        aim_pos = aim_dir + user.GetPosition();
        AimVFX();
        // the ACTUAL aiming aspect (get target position from aim_dir)
        Quaternion aim_rot = Quaternion.LookRotation(Vector3.forward, aim_dir) * ROTATION_OFFSET;
        curr_rot = Quaternion.Lerp(curr_rot, aim_rot, rot_scale);
        source_pos = user.GetPosition() + (Vector2)(curr_rot * Vector2.right * (user.hitbox_radius+0.1f));
        target_pos = user.GetPosition() + (Vector2)(curr_rot * Vector2.right * aim_dir.magnitude);
    }

    public void Use()
    {
        if (is_equipped)
        {
            if (func_module.CanFunction())
            {
                input_type.Use();
            }
            else
            {
                Reset();
            }
            
        }
    }

    public void Stop()
    {
        input_type.Stop();
    }

    public void Reset()
    {
        is_equipped = false; // after item finishes resetting, animator uses the SetEquipped() to set is_equipped back to true
        if (reset_timer == 0)
        {
            animator.SetBool("Resetting", true);
            reset_timer = base_data.item_stats["reset_speed"] * reset_spd_scale;
        }
    }

    public void Action(int effect_index)
    {
        animator.SetTrigger("Use");
        func_module.UseFunction(effect_index);
    }

    #region Aiming FX Types
    void StaticAim()
    {
        if((rotator_object.transform.localScale.y >= 0) != (target_pos.x >= user.GetPosition().x)) {
            Vector3 new_vec = rotator_object.transform.localScale;
            new_vec.x *= -1;
            rotator_object.transform.localScale = new_vec;
        }
    }
    void DynamicAim()
    {
        // set the item's rotation towards the target direction
        rotator_object.transform.rotation = curr_rot;
        // make sure item scale is correct
        if((rotator_object.transform.localScale.y >= 0) != (target_pos.x >= user.GetPosition().x)) {
            Vector3 new_vec = rotator_object.transform.localScale;
            new_vec.y *= -1;
            rotator_object.transform.localScale = new_vec;
        }

    }

    #endregion

    #region VFX Functions

    public void PlaceFront()
    {
        
    }

    public void PlaceBack()
    {
        
    }

    #endregion

    #region Getting Data
    public bool IsHoldInput()
    {
        return base_data.is_full_auto;
    }

    public int GetRange()
    {
        return base_data.bonus_range_scalar;
    }

    public float GetResetCompletion()
    {
        return reset_timer / base_data.item_stats["reset_speed"] * reset_spd_scale;
    }

    public float GetFunctionCompletion()
    {
        return func_module.FunctionCompletion();
    }
    #endregion
    #region Debug
    void OnDrawGizmosSelected()
    {
        Debug.DrawLine(user.GetPosition(), target_pos, Color.gray);
    }

    #endregion
}
