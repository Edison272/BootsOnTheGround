using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{   
    [SerializeField] ItemSO scriptable_object; // SO contains important base data
    // Classify Input Type
    [field: Header("Input")]
    [SerializeField] InputEnum input_enum;
    // Input Instance
    [SerializeReference] 
    InputType input_type;

    [field: Header("VFX Body")]
    public GameObject item_object;
    public GameObject rotator_object; // rotate the object when aiming
    [SerializeField] Animator animator;
    

    [field: Header("Aiming")]
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90f); // RotateTowards() is stupid so we need to offset it
    public float rot_scale = 1f; // 0 for no rotation, 1 for instantaneous rotation
    Quaternion curr_rot; // save the current quaternion rotation
    public Vector2 aim_pos; // where the item is supposed to be aimed towards;
    public Vector2 target_pos; // where the item is actually aimed towards (based on rot_scale)
    bool freeze_aiming = false;     // stop this thing from aiming and updating target position
    [SerializeField] bool dynamic_aim = true;        // allow dynamic aim for the object to be able to turn to face the target
    public Action AimVFX;

    [field: Header("Data")]
    public Operator user;

    [field: Header("Functionality")]
    public FuncModule functionality;
    public bool is_full_auto = true;
    public bool is_equipped = false; // can only use an item if it is equipped
    public float use_speed;
    public float reset_speed = 1f; // how much time this weapon takes to reset
    public float reset_timer; // block this weapon's if disabled time > 0

    // Setup immutable item data in start
    public void Setup()
    {            
        // setup input type
        switch (input_enum) {
            case InputEnum.Normal:
                input_type = new NormalInput(use_speed, Effect);
                break;
            case InputEnum.Charge:
                input_type = new ChargeInput(2f, 3, Effect);
                break;
            case InputEnum.Increment:
                input_type = new IncrementInput(use_speed, 2f, 3, Effect);
                break;
        }

        // set aim type
        if (dynamic_aim) {
            AimVFX = DynamicAim;
        } 
        else {
            AimVFX = StaticAim;
        }
    }

    // adjust item everytime theres a new user
    public void NewUser(Operator new_user)
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

            }
        }
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

    public void SetEquipped(int is_equipped)
    {
        this.is_equipped = is_equipped > 0 ? true : false;
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
        input_type.Use();
        animator.SetTrigger("Use");
        Debug.DrawLine(user.GetPosition(), target_pos, Color.green);
    }

    public void Stop()
    {
        input_type.Stop();
        Debug.DrawLine(user.GetPosition(), target_pos, Color.red, 0.4f);
    }

    public void Reset()
    {
        Debug.Log("Reset the " + gameObject.name);
        Debug.DrawLine(user.GetPosition(), target_pos, Color.yellow, 0.5f);
        animator.SetBool("Resetting", true);
        reset_timer = reset_speed;
    }

    public void Effect(int effect_index)
    {
        
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
        return is_full_auto;
    }
    #endregion
}
