using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public GameObject ItemTransform;

    
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
    public Vector2 aim_pos;
    float aim_angle;
    public Vector2 target_pos;
    bool freeze_aiming = false;     // stop this thing from aiming and updating target position
    bool dynamic_aim = true;        // allow dynamic aim for the object to be able to turn to face the target
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
    public void Aim(Vector2 new_aim_pos, float new_aim_angle)
    {
        aim_pos = new_aim_pos;
        aim_angle = new_aim_angle;

        AimVFX();
        
    }

    public void Use()
    {
        Debug.Log(gameObject.name);
    }

    public void Stop()
    {
        Debug.Log("Stop the " + gameObject.name);
    }

    public void Reset()
    {
        Debug.Log("Reset the " + gameObject.name);
    }

    #region Aiming FX Types
    void StaticAim()
    {
        
    }
    void DynamicAim()
    {
        item_object.transform.rotation = Quaternion.Slerp(item_object.transform.rotation, Quaternion.Euler(0, 0, aim_angle), handle_speed * Time.deltaTime);
    }

    #endregion

    #region Getting Data
    public bool IsHoldInput()
    {
        return is_full_auto;
    }
    #endregion
}
