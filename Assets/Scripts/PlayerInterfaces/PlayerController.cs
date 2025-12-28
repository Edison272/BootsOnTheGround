using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.PlayerLoop;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    BOTG_Controls controls;
    InputAction movement;
    InputAction looking;
    InputAction main_action;
    bool main_continuous; // checked on update to see if the main action should be called
    public bool main_hold_input = false;  // if true, mouse will main action function will constantly be called
    InputAction alt_action;
    bool alt_continuous; // checked on update to see if the alt action should be called
    public bool alt_hold_input = false;  // if true, mouse will alt action function will constantly be called
    
    [Header("Character Control")]
    [SerializeField] private Camera cam;
    public float cam_bound = 1f;
    [SerializeField] Vector2 input_dir;
    [SerializeField] private Vector3 look_pos;
    [SerializeField] private Vector3 raw_look_pos;
    [SerializeField] Character active_character;

    void Awake()
    {
        // get map
        controls = new BOTG_Controls();
        
        // map input actions to the controls
        movement = controls.GroundActions.Move;
        looking = controls.GroundActions.Look;
        main_action = controls.GroundActions.MainAction;
        alt_action = controls.GroundActions.AltAction;
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        EnableControl();
    }

    public void SetPlayerCharacter(Character new_character)
    {
        active_character = new_character;

        // subscribe to the relevant events after active_character has been set
        movement.performed += Move;
        looking.performed += Look;
        // subscribe action types
        SetMainAction(true);
        SetAltAction(active_character.HasAltAction());
        // reset the item (reload, recharge, reset stance, cool animation etc.)
        controls.GroundActions.ResetItem.performed += ResetItem;
        // switch the item
        controls.GroundActions.SwitchItem.performed += SwitchItem;
        // Toggle Command Mode
        controls.GroundActions.ToggleCommandMode.performed += CmdMode;
    }

    public void EnableControl()
    {
        controls.Enable();
    }

    public void DisableControl()
    {
        controls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (active_character)
        {
            if (main_continuous) {MainAction();}
            if (alt_continuous) {AltAction();}
        }
    }

    void FixedUpdate()
    {
        if (active_character)
        {
            Vector3 char_pos = active_character.GetPosition();

            look_pos = cam.ScreenToWorldPoint(raw_look_pos);
            Vector3 cam_pos = (look_pos - char_pos) * 0.15f;
            cam_pos.z = -10;
            cam.transform.position = cam_pos + char_pos;
        }
    }

    #region Input Functions
    void Move(InputAction.CallbackContext context) {
        input_dir = context.ReadValue<Vector2>();
        if (input_dir.sqrMagnitude > 0.0001f) 
        {
            active_character.SetMove(input_dir);
        }
        else 
        {
            active_character.StopMove();
        }        
    }
    void StopMove(InputAction.CallbackContext context) {active_character.StopMove();}
    void Look(InputAction.CallbackContext context) {
        raw_look_pos = context.ReadValue<Vector2>();
        raw_look_pos.z = -cam.transform.localPosition.z;
        active_character.Look(cam.ScreenToWorldPoint(raw_look_pos));
    }
    void MainStart(InputAction.CallbackContext context) {
        active_character.UseMainItem();
        main_continuous = main_hold_input;
    }
    void MainStop(InputAction.CallbackContext context) {
        active_character.StopMainItem();
        main_continuous = false;
    }
    void MainAction() {active_character.UseMainItem();}
    void AltStart(InputAction.CallbackContext context) {
        active_character.UseAltItem();
        alt_continuous = alt_hold_input;
         
    }
    void AltStop(InputAction.CallbackContext context) {
        active_character.StopAltItem();
        alt_continuous = false;
    }
    void AltAction() {active_character.UseAltItem();}
    void ResetItem(InputAction.CallbackContext context) {active_character.ResetItems();}
    void SwitchItem(InputAction.CallbackContext context) {
        active_character.SwitchItem();
        // set new input functionality
        SetMainAction(true);
        SetAltAction(active_character.HasAltAction());
    }
    void CmdMode(InputAction.CallbackContext context) {}
    #endregion

    #region Action Enabling
    void SetMainAction(bool enable) // set main action. set parameter false to turn off main actions
    {
        if (enable)
        {
            main_action.started += MainStart;
            main_action.canceled += MainStop;
            main_hold_input = active_character.main_item.IsHoldInput();
            main_continuous = main_action.IsPressed() && main_hold_input;
        }
        else
        {
            main_action.started -= MainStart;
            main_action.canceled -= MainStop;
            main_continuous = false; // stop any ongoing attacks
            
        }
    } 
    void SetAltAction(bool enable) // set alt action. set parameter false to turn off main actions
    {
        if (enable)
        {
            alt_action.started += AltStart;
            alt_action.canceled += AltStop;
            alt_hold_input = active_character.alt_item.IsHoldInput(); // only set alt_hold_input if an alt_action exists
            alt_continuous = alt_action.IsPressed() && alt_hold_input;
        }
        else
        {
            alt_action.started -= AltStart;
            alt_action.canceled -= AltStop;   
            alt_continuous = false; // stop any ongoing attacks
        }
    }


    #endregion
}
