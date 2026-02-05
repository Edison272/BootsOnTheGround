using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
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
    // move BOTH cameras to the player positon cuz they're both important. make play cam a child object of main cam tho
    [SerializeField] private Camera main_cam; // used to get the FULL area around the player and render that for player view
    [SerializeField] private Camera play_cam; // the area the player sees and interacts with
    [SerializeField] RawImage player_view; // a canvas raw image the player uses to see the world
    
    [SerializeField] Vector2 input_dir;
    public Vector3 canvas_pointer_pos {get; private set;}
    public Vector2 look_pos {get; private set;}
    private Vector2 raw_look_pos;
    [SerializeField] Character active_character;
    bool in_command_mode = false; // can only move while in command mode

    [Header("Camera Stuff")]
    [SerializeField] float camera_zoom_time = 0.5f;
    public float base_zoom_level = 1;
    public float zoom_factor = 0.5f;
    float zoom_diff; // set current zoom
    float curr_zoom; // set current zoom
    float target_zoom; // set current zoom
    float curr_zoom_time;
    Vector2 save_mouse_pos = Vector2.zero;

    [Header("UI Stuff")]
    [SerializeField] GameObject cursor;

    [Header("Squad Interactions")]
    public SquadManager squad;
    private int op_select_index = -1;
    
    void Awake() // initialize values before player assumes control
    {
        // get map
        controls = new BOTG_Controls();
        
        // map input actions to the controls
        movement = controls.GroundActions.Move;
        looking = controls.GroundActions.Look;
        main_action = controls.GroundActions.MainAction;
        alt_action = controls.GroundActions.AltAction;

        target_zoom = 1;
        curr_zoom = (int)target_zoom;
        zoom_diff = 0;

        // set starting zoom
        target_zoom = base_zoom_level;
    }

    // Start is called before the first frame update
    public void StartPlayer()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        EnableControl();
    }

    public void SetPlayerCharacter(Character new_character)
    {
        active_character = new_character;
        in_command_mode = false;

        // subscribe to the relevant events after active_character has been set
        movement.performed += Move;
        // subscribe action types
        SetMainAction(true);
        SetAltAction(active_character.HasAltAction());
        // reset the item (reload, recharge, reset stance, cool animation etc.)
        controls.GroundActions.ResetItem.performed += ResetItem;
        // switch the item
        controls.GroundActions.SwitchItem.performed += SwitchItem;
        // Toggle Command Mode
        controls.GroundActions.ToggleCommandMode.performed += CmdMode;
        // Deploy Op with num keys
        controls.GroundActions.OpDeploy1.performed += OpDeploy1;
        controls.GroundActions.OpDeploy2.performed += OpDeploy2;
        controls.GroundActions.OpDeploy3.performed += OpDeploy3;
        controls.GroundActions.OpDeploy4.performed += OpDeploy4;

        SetCameraZoom(active_character.GetRangeScalar());
        GameOverseer.THE_OVERSEER.canvas_control.SetCommandUI(in_command_mode);
    }

    public void SetCameraZoom(int zoom_scalar)
    {
        curr_zoom = player_view.rectTransform.localScale.x;
        curr_zoom_time = 0;
        target_zoom = base_zoom_level + (5 - zoom_scalar) * zoom_factor;
        zoom_diff = target_zoom - player_view.rectTransform.localScale.x;
    }

    public void EnableControl()
    {
        controls.Enable();
    }

    public void DisableControl()
    {
        controls.Disable();
    }

    #region Updates
    // Update is called once per frame
    void Update()
    {
        if (active_character)
        {
            if (main_continuous) {MainAction();}
            if (alt_continuous) {AltAction();}
        }
        if (curr_zoom_time < camera_zoom_time)
        {
            curr_zoom_time += Time.deltaTime;
            if (curr_zoom_time >= camera_zoom_time)
            {
                curr_zoom_time = camera_zoom_time;
                player_view.rectTransform.localScale = new Vector3(target_zoom, target_zoom, 0);
            }
        }

        // constantly read looking value
        RectTransformUtility.ScreenPointToLocalPointInRectangle(player_view.rectTransform, looking.ReadValue<Vector2>(), play_cam, out raw_look_pos);
        Rect rect = player_view.rectTransform.rect;
        // get viewport coordinates
        float view_x = (raw_look_pos.x - rect.xMin) / rect.width;
        float view_y = (raw_look_pos.y - rect.yMin) / rect.height;
        // set look pos
        canvas_pointer_pos = new Vector3(view_x, view_y, 0);
        active_character.Look(main_cam.ViewportToWorldPoint(canvas_pointer_pos));
        look_pos = (Vector2)main_cam.ViewportToWorldPoint(canvas_pointer_pos);

        // adjust cursor
        cursor.transform.position = (Vector2)look_pos;
    }

    void FixedUpdate()
    {
        
        // if (input_dir != Vector2.zero)
        // {
        //     active_character.SetMove();
        // }
    }

    void LateUpdate()
    {
        if (active_character)
        {
            Vector2 char_pos = active_character.gameObject.transform.position;
            // update cam position to move to look position within circular bounds
            float cam_range = 5 + 1.5f * active_character.GetRangeScalar();
            Vector2 offset = (look_pos - char_pos) * 0.5f;
            if (offset.magnitude > cam_range)
            {
                offset = offset.normalized * cam_range;
            }

            Vector3 cam_pos = char_pos + offset;
            cam_pos.z = -10;

            // Vector3 cam_pos = (look_pos + char_pos) * 0.5f;
            // cam_pos.x = Mathf.Clamp(cam_pos.x, -cam_range + char_pos.x, cam_range + char_pos.x);
            // cam_pos.y = Mathf.Clamp(cam_pos.y, -cam_range + char_pos.y, cam_range + char_pos.y);
            // cam_pos.z = -10;
            main_cam.transform.position = cam_pos;
        }
        if (curr_zoom_time > 0)
        {
            float scale_Val = curr_zoom + zoom_diff * curr_zoom_time/camera_zoom_time; 
            player_view.rectTransform.localScale = new Vector3(scale_Val, scale_Val, 0);
        }
    }
    #endregion
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
        // RectTransformUtility.ScreenPointToLocalPointInRectangle(player_view.rectTransform, context.ReadValue<Vector2>(), play_cam, out Vector2 local_pos);

        // Rect rect = player_view.rectTransform.rect;

        // // get viewport coordinates
        // float view_x = (local_pos.x - rect.xMin) / rect.width;
        // float view_y = (local_pos.y - rect.yMin) / rect.height;
        
        // raw_look_pos = new Vector3(view_x, view_y, 0);
        // active_character.Look(main_cam.ViewportToWorldPoint(raw_look_pos));
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
        SetCameraZoom(active_character.GetRangeScalar());
    }
    #endregion
    #region Command Input Functions
    void CmdMode(InputAction.CallbackContext context) // used to see the whole map ts
    {
        in_command_mode = !in_command_mode; // switch command mode type
        if (in_command_mode)
        {
            // // disable actions
            // SetMainAction(false);
            // SetAltAction(false);
            // controls.GroundActions.ResetItem.performed -= ResetItem;
            // controls.GroundActions.SwitchItem.performed -= SwitchItem;

            // // enable click detection on canvas
            // ToggleCommandInput(true);

            // change UI
            cursor.SetActive(false);
            SetCameraZoom(5);

            // save mouse pos
            save_mouse_pos = look_pos;
        } else
        {
            // // disable click detection on canvas
            // ToggleCommandInput(false);
            
            // // reenable actions
            // SetMainAction(true);
            // SetAltAction(active_character.HasAltAction());
            // controls.GroundActions.ResetItem.performed += ResetItem;
            // controls.GroundActions.SwitchItem.performed += SwitchItem;

            // change UI
            cursor.SetActive(true);
            SetCameraZoom(active_character.GetRangeScalar());

            //Mouse.current.WarpCursorPosition(new Vector2(Screen.width * canvas_pointer_pos.x, Screen.height * canvas_pointer_pos.y));
        }
        Cursor.visible = in_command_mode;
        GameOverseer.THE_OVERSEER.canvas_control.SetCommandUI(in_command_mode);
    }

    void OpDeploy1(InputAction.CallbackContext context) {GetOperator(1);} // start at 1 bc og player char is in index 0
    void OpDeploy2(InputAction.CallbackContext context) {GetOperator(2);}
    void OpDeploy3(InputAction.CallbackContext context) {GetOperator(3);}
    void OpDeploy4(InputAction.CallbackContext context) {GetOperator(4);}

    private void GetOperator(int deploy_index = 1)
    {
        // cancel command if it was done twice
        if (op_select_index == deploy_index) {
            op_select_index = -1;
            squad.SetSelectedOperator(op_select_index);
            ToggleCommandInput(false);
            GameOverseer.THE_OVERSEER.canvas_control.ToggleReticleCommandUI(false);
        } 
        // otherwise select operator and enable operator commands
        else {
            op_select_index = deploy_index;
            squad.SetSelectedOperator(op_select_index);
            ToggleCommandInput(true);
            GameOverseer.THE_OVERSEER.canvas_control.ToggleReticleCommandUI(true);
        }
        
    }

    void ConfirmClickM1(InputAction.CallbackContext context) {
        squad.UseOpAbility(look_pos);
    }
    void ConfirmClickM2(InputAction.CallbackContext context) {
        squad.SwitchOpBehavior();
    }

    #endregion

    #region Action Enabling

    void ToggleCommandInput(bool cmd_on)
    {
        if (cmd_on)
        {
            main_action.started += ConfirmClickM1;
            alt_action.started += ConfirmClickM2;
            SetMainAction(false);
            SetAltAction(false);
        } 
        else
        {
            main_action.started -= ConfirmClickM1;
            alt_action.started -= ConfirmClickM2;
            SetMainAction(true);
            SetAltAction(active_character.HasAltAction());

            // make sure the player doesn't leak an attack after commanding
            main_continuous = false;
            alt_continuous = false;
        }
    }
    void SetMainAction(bool enable) // set main action. set parameter false to turn off main actions
    {
        if (enable)
        {
            main_action.started += MainStart;
            main_action.canceled += MainStop;
            main_hold_input = active_character.main_item.IsHoldInput();
            // allow continuous attacking between weapon switches
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
