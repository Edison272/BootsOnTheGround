using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public GameObject ItemTransform;

    [field: Header("Input")]
    // Classify Input Type
    enum InputEnum {Normal, Charge, Increment}
    [SerializeField] InputEnum input_enum;

    // Input Instance
    public InputType input_type;

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
    
    // Setup immutable item data in start
    void Start()
    {
        // setup input type
        switch (input_enum) {
            case InputEnum.Normal:
                input_type = new NormalInput(3);
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
    void Setup()
    {
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
        Debug.Log("Doot");
    }

    public void Stop()
    {
        Debug.Log("Stop the Doot");
    }

    public void Reset()
    {
        Debug.Log("ReDoot");
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
}
