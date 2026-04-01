using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {Weapon, Support}
public class Item : MonoBehaviour
{   
    [field: SerializeField] public ItemSO base_data {get; private set;} // SO contains important base data
    // Input Instance
    ItemInputController input_controller;
    public float get_input_ready => input_controller.GetStatus();

    [field: Header("VFX Body")]
    public GameObject item_object;
    public Transform rotator_object; // rotate the object when aiming
    public Transform item_tip; // the "front" of an item which attack type vfx will align to
    public Animator animator;
    public float y_offset {get; private set;} // vfx y offset from the target position
    
    [field: Header("VFX Body")]
    //public bool apply_recoil = false;

    [field: Header("Aiming")]
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90f); // RotateTowards() is stupid so we need to offset it
    public float rot_scale = 1f; // 0 for no rotation, 1 for instantaneous rotation
    Quaternion curr_rot; // save the current quaternion rotation
    public Vector2 aim_pos; // where the item is supposed to be aimed towards;
    public Vector2 source_pos;
    public Vector2 target_pos {get; private set;} // where the item is actually aimed towards (based on rot_scale)
    bool freeze_aiming = false;     // stop this thing from aiming and updating target position
    public Action AimVFX;

    [field: Header("Data")]
    public Character user;

    [field: Header("Functionality")]
    public FunctionalityController functionality_controller;
    public AttackType[] attacks;
    public bool is_equipped {get; private set;} = false; // can only use an item if it is equipped
    public float reset_timer {get; private set;} // block this weapon's if disabled time > 0

    public float equip_timer = 0.5f;

    [field: Header("Modifiers")]
    public float use_spd_scale = 1f;
    public float equip_spd_scale = 1f;
    public float reset_spd_scale = 1f;

    [Header("Interface")]
    public Sprite ui_image => base_data.ui_image;
    #region Initializers
    // Setup immutable item data when this object is made
    public void Setup(ItemSO base_data, ItemInputController input_controller, FunctionalityController functionality_controller, AttackType[] atk_types)
    {            
        this.base_data = base_data;
        this.input_controller = input_controller;
        this.functionality_controller = functionality_controller;
        attacks = atk_types;
        // set aim type
        if (base_data.dynamic_aim) {
            AimVFX = DynamicAim;
        } 
        else {
            AimVFX = StaticAim;
        }
        rot_scale = base_data.rotation_scale;
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
    public void DropItem()
    {
        user = null;
        rotator_object = null;
        y_offset = 0;
    }

    public void ResetData() // reset data to original state
    {
        Debug.Log("Resetting Item");
        use_spd_scale = 1;
        reset_spd_scale = 1;
        equip_spd_scale = 1;

        // reset animator state
        animator.Rebind();
        animator.Update(0f);

        is_equipped = false;
        reset_timer = 0;

        input_controller.Reset();
        functionality_controller.ResetData();
    }

    #endregion

    #region Update
    // Update is called once per frame
    void Update()
    {
        functionality_controller.UpdateModule(target_pos);

        if (reset_timer > 0)
        {
            reset_timer -= Time.deltaTime;
            if (reset_timer <= 0)
            {
                functionality_controller.ResetData();
                is_equipped = true;
                reset_timer = 0;
                animator.speed = 1;
            }
        }
        if (equip_timer > 0)
        {
            equip_timer -= Time.deltaTime;
            if (equip_timer <= 0)
            {
                is_equipped = true;
                equip_timer = 0;
                animator.speed = 1;
            }
        }   
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
    #endregion

    #region Inventory Management
    // setup the weapon for the user whenever it's picked up or equipped
    public void EquipItem()
    {
        equip_timer = base_data.item_stats["equip_speed"] * equip_spd_scale;
        animator.speed = 1/equip_timer;
        animator.SetBool("IsEquipped", true); // call set equipped function through editor
    }

    public void UnequipItem()
    {
        animator.SetBool("IsEquipped", false);
        animator.ResetTrigger("Resetting");
        animator.ResetTrigger("Use");
        animator.speed = 2;
        reset_timer = 0;
    }

    public void SetEquipped(int int_is_equipped) // 
    {
        is_equipped = int_is_equipped > 0 ? true : false;
    }
    #endregion

    #region Item Events
    public void Use()
    {
        if (is_equipped)
        {
            if (functionality_controller.CanFunction())
            {
                input_controller.Use();
            }
            else
            {
                Reset();
            }
        }
    }

    public void Stop()
    {
        input_controller.Stop();
    }

    public void Reset()
    {
        is_equipped = false; // after item finishes resetting, animator uses the SetEquipped() to set is_equipped back to true
        if (reset_timer == 0)
        {
            reset_timer = base_data.item_stats["reset_speed"] * reset_spd_scale;
            animator.speed = 1/reset_timer;
            animator.SetTrigger("Resetting");
            animator.ResetTrigger("Use");
        }
    }

    public void Action(int effect_index)
    {
        animator.speed = 1/base_data.item_stats["use_speed"] * use_spd_scale;
        animator.SetTrigger("Use");
        functionality_controller.UseFunction(effect_index);
    }
    #endregion

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
    public void PlaceFront() // place the item on the front of the user at all times
    {
        user.PlaceOnBody(CharacterBodyPart.Front, gameObject.transform);
    }

    public void PlaceBack() // place the item on the front of the user at all times
    {
        user.PlaceOnBody(CharacterBodyPart.Back, gameObject.transform);
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

    public float GetInnacuracy()
    {
        AttackType predicted_atk_type = attacks[input_controller.PredictActionIndex()];
        return predicted_atk_type.GetAtkSpread();
    }

    public float GetResetCompletion()
    {
        return reset_timer / base_data.item_stats["reset_speed"] * reset_spd_scale;
    }

    public float GetFunctionCompletion()
    {
        return functionality_controller.FunctionCompletion();
    }
    #endregion
    
    #region Debug
    void OnDrawGizmosSelected()
    {
        if (user)
        {
            Debug.DrawLine(user.GetPosition(), target_pos, Color.gray);
        }
    }

    #endregion
}
