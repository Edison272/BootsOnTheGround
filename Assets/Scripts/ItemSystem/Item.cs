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
    public GameObject rotator_object; // rotate the object when aiming
    public Animator animator;
    

    [field: Header("Aiming")]
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90f); // RotateTowards() is stupid so we need to offset it
    public float rot_scale = 1f; // 0 for no rotation, 1 for instantaneous rotation
    Quaternion curr_rot; // save the current quaternion rotation
    public Vector2 aim_pos; // where the item is supposed to be aimed towards;
    public Vector2 target_pos; // where the item is actually aimed towards (based on rot_scale)
    bool freeze_aiming = false;     // stop this thing from aiming and updating target position
    public Action AimVFX;

    [field: Header("Data")]
    public Character user;

    [field: Header("Functionality")]
    public FuncModule func_module;
    public bool is_equipped = false; // can only use an item if it is equipped
    public float reset_timer; // block this weapon's if disabled time > 0

    [field: Header("Modifiers")]
    public float use_spd_scale = 1f;
    public float reset_spd_scale = 1f;

    // Setup immutable item data when this object is made
    public void Setup(ItemSO base_data, InputType input_type, FuncModule func_module)
    {            
        this.base_data = base_data;
        this.input_type = input_type;
        this.func_module = func_module;
        // set aim type
        if (base_data.dynamic_aim) {
            AimVFX = DynamicAim;
        } 
        else {
            AimVFX = StaticAim;
        }
    }

    // adjust item everytime theres a new user
    public void NewUser(Character new_user)
    {
        user = new_user;
        // if no rotator object, rotator object is the user's hand (items in use are always a child transform of something else)
        if (!rotator_object)
        {
            rotator_object = transform.parent.gameObject;
        }
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
        target_pos = Vector2.zero;
    }

    // Update the target position of this item and adjust VFX accordingly
    public void Aim(Vector2 aim_dir)
    {
        aim_pos = aim_dir + user.GetPosition();
        AimVFX();
        // the ACTUAL aiming aspect (get target position from aim_dir)
        Quaternion aim_rot = Quaternion.LookRotation(Vector3.forward, aim_dir) * ROTATION_OFFSET;
        curr_rot = Quaternion.Lerp(curr_rot, aim_rot, rot_scale);
        target_pos = user.GetPosition() + (Vector2)(curr_rot * Vector2.right * aim_dir.magnitude);
        
        Debug.DrawLine(user.GetPosition(), target_pos, Color.gray);
    }

    public void Use()
    {
        if (!animator.GetBool("Resetting") && func_module.CanFunction())
        {
            input_type.Use();
        }
    }

    public void Stop()
    {
        input_type.Stop();
    }

    public void Reset()
    {
        Debug.Log("Reset the " + gameObject.name);
        animator.SetBool("Resetting", true);
        reset_timer = base_data.item_stats["reset_speed"] * reset_spd_scale;
    }

    public void Action(int effect_index)
    {
        animator.SetTrigger("Use");
        func_module.UseFunction(effect_index);
        Debug.DrawLine(user.GetPosition(), target_pos, Color.green, 0.1f);
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

    #region Getting Functionality Data
    public bool IsHoldInput()
    {
        return base_data.is_full_auto;
    }
    #endregion
}
