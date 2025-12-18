using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{   
    // Classify Input Type
    enum InputEnum {Normal, Charge, Increment}
    [field: Header("Input")]
    [SerializeField] InputEnum input_enum;
    // Input Instance
    [SerializeReference] 
    InputType input_type;

    [field: Header("Body")]
    public GameObject item_object;
    

    [field: Header("Aiming")]
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90f); // RotateTowards() is stupid so we need to offset it
    public float rot_scale = 1f; // 0 for no rotation, 1 for instantaneous rotation
    Quaternion curr_rot; // save the current quaternion rotation
    public Vector2 aim_pos; // where the item is supposed to be aimed towards;
    public Vector2 target_pos; // where the item is actually aimed towards (based on rot_scale)
    bool freeze_aiming = false;     // stop this thing from aiming and updating target position
    [SerializeField] bool dynamic_aim = true;        // allow dynamic aim for the object to be able to turn to face the target
    float handle_speed = 20f;       // how quickly the weapon can be turned to face the target;
    public Action AimVFX;

    [field: Header("Data")]
    public Operator user;

    [field: Header("Functionality")]
    public bool is_full_auto = true;
    public bool is_equipped = false; // can only use an item if it is equipped
    public float use_speed;
    

    void Start()
    {
        Setup();
    }

    // Setup immutable item data in start
    public void Setup()
    {    
        // setup input type
        switch (input_enum) {
            case InputEnum.Normal:
                input_type = new NormalInput(use_speed);
                break;
            case InputEnum.Charge:
                input_type = new ChargeInput();
                break;
            case InputEnum.Increment:
                input_type = new IncrementInput();
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

    // Update is called once per frame
    void Update()
    {
        
    }

    // setup the weapon for the user whenever it's picked up or equipped
    public void EquipItem()
    {
        gameObject.SetActive(true); // temporary
    }


    public void UnequipItem()
    {
        gameObject.SetActive(false); // temporary
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
        Debug.DrawLine(user.GetPosition(), target_pos, Color.green, 0.2f);
    }

    public void Stop()
    {
        Debug.DrawLine(user.GetPosition(), target_pos, Color.red, 0.4f);
    }

    public void Reset()
    {
        Debug.Log("Reset the " + gameObject.name);
        Debug.DrawLine(user.GetPosition(), target_pos, Color.yellow, 0.5f);
    }

    #region Aiming FX Types
    void StaticAim()
    {
        if((item_object.transform.localScale.y >= 0) != (target_pos.x >= 0)) {
            Vector3 new_vec = item_object.transform.localScale;
            new_vec.x *= -1;
            item_object.transform.localScale = new_vec;
        }
    }
    void DynamicAim()
    {
        // set the item's rotation towards the target direction
        item_object.transform.rotation = curr_rot;
        // make sure item scale is correct
        if((item_object.transform.localScale.y >= 0) != (target_pos.x >= 0)) {
            Vector3 new_vec = item_object.transform.localScale;
            new_vec.y *= -1;
            item_object.transform.localScale = new_vec;
        }

    }

    #endregion

    #region Getting Data
    public bool IsHoldInput()
    {
        return is_full_auto;
    }
    #endregion
}
