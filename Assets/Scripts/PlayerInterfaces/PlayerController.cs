using System;
using System.Drawing;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;
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
    
    [Header("Player Cams & Screen")]
    // move BOTH cameras to the player positon cuz they're both important. make play cam a child object of main cam tho
    [SerializeField] private Camera main_cam; // used to get the FULL area around the player and render that for player view
    [SerializeField] private Camera play_cam; // the area the player sees and interacts with
    [SerializeField] RawImage player_screen; // a canvas raw image the player uses to see the world
    
    [SerializeField] Vector2 input_dir;
    private Vector2 pointer_delta;
    public Vector3 viewport_pos => player_view_controller.viewport_pos;
    public Vector2 look_pos => player_view_controller.look_pos;
    public Vector2 screen_pos => player_view_controller.screen_pos;
    [SerializeField] Character active_character;
    bool in_command_mode = false; // can only move while in command mode

    [Header("Player View")]
    public PlayerViewController player_view_controller;
    
    [Header("Camera Control")]
    public MainCameraController main_cam_controller;
    [Header("UI Stuff")]
    [SerializeField] GameObject cursor;

    [Header("Squad Interactions")]
    public SquadManager squad;
    private int op_select_index = -1;

    [Header("Character UI")]
    public ItemUIController item_ui_control;
    public HealthUIController health_ui_control;
    
    void Awake() // initialize values before player assumes control
    {
        // get map
        controls = new BOTG_Controls();
        
        // map input actions to the controls
        movement = controls.GroundActions.Move;
        looking = controls.GroundActions.Look;
        main_action = controls.GroundActions.MainAction;
        alt_action = controls.GroundActions.AltAction;

        // set helper classes
        player_view_controller = new PlayerViewController(main_cam, player_screen.rectTransform);
        main_cam_controller = new MainCameraController(main_cam, player_screen.rectTransform);
        player_view_controller.SetMCController(main_cam_controller);
        main_cam_controller.SetPVController(player_view_controller);
    }

    // Start is called before the first frame update
    public void StartPlayer()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (active_character)
        {
            player_view_controller.ResetView(active_character.GetPosition());
            pointer_delta = Vector2.zero;
        }
        EnableControl();
        item_ui_control.SetActiveCharacter(active_character);
        health_ui_control.SetActiveCharacter(active_character);
    }

    public void SetPlayerCharacter(Character new_character)
    {
        active_character = new_character;
        in_command_mode = false;

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
        controls.GroundActions.ToggleCommandMode.performed += CommandView;
        // Deploy Op with num keys
        controls.GroundActions.OpDeploy1.performed += OpDeploy1;
        controls.GroundActions.OpDeploy2.performed += OpDeploy2;
        controls.GroundActions.OpDeploy3.performed += OpDeploy3;
        controls.GroundActions.OpDeploy4.performed += OpDeploy4;

        main_cam_controller.SetCameraZoom(active_character.GetRangeScalar());
        GameOverseer.THE_OVERSEER.canvas_control.SetCommandUI(in_command_mode);
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

            // set look pos
            player_view_controller.UpdateView(pointer_delta*0.5f, active_character.GetPosition());
            pointer_delta = Vector2.zero;
            active_character.Look(look_pos);
            main_cam_controller.UpdateCamData(active_character.GetPosition(), look_pos);

            if (!active_character.IsInAction())
            {
                GameOverseer.THE_OVERSEER.GameOver();
            }
            
        }
        // prepare camera data
        player_view_controller.UpdateLookPos();
    }
    void LateUpdate()
    {   
        // adjust cursor
        cursor.transform.position = look_pos;
        // update controllers
        main_cam_controller.UpdateCamRender();
        
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
        pointer_delta += looking.ReadValue<Vector2>();
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
        // set new input functionality
        if (!in_command_mode && op_select_index == -1)
        {
            active_character.SwitchItem();
            SetMainAction(true);
            SetAltAction(active_character.HasAltAction());
            main_cam_controller.SetCameraZoom(active_character.GetRangeScalar());
        }
    }
    #endregion
    #region Command View
    void CommandView(InputAction.CallbackContext context) // used to see the whole map ts
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
            //cursor.SetActive(false);
            main_cam_controller.SetCameraZoom(5);
            player_view_controller.SetViewType(PlayerViewEnum.Command);
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
            //cursor.SetActive(true);
            main_cam_controller.SetCameraZoom(active_character.GetRangeScalar());
            player_view_controller.ResetViewType();

            //Mouse.current.WarpCursorPosition(new Vector2(main_cam.pixelWidth * viewport_pos.x, main_cam.pixelHeight * viewport_pos.y));
        }
        //Cursor.visible = in_command_mode;
        GameOverseer.THE_OVERSEER.canvas_control.SetCommandUI(in_command_mode);
    }
    #endregion

    #region Operator Orders
    void OpDeploy1(InputAction.CallbackContext context) {GetOperator(1);} // start at 1 bc og player char is in index 0
    void OpDeploy2(InputAction.CallbackContext context) {GetOperator(2);}
    void OpDeploy3(InputAction.CallbackContext context) {GetOperator(3);}
    void OpDeploy4(InputAction.CallbackContext context) {GetOperator(4);}

    private void GetOperator(int deploy_index = 1)
    {
        // cancel command if it was done twice
        if (op_select_index == deploy_index) {
            ResetConfirm();
        } 
        // otherwise select operator and enable operator commands
        else {
            op_select_index = deploy_index;
            squad.SetSelectedOperator(op_select_index);
            ToggleCommandInput(true);
            GameOverseer.THE_OVERSEER.canvas_control.ToggleReticleCommandUI(true);
        }
        
    }

    void StartOrderState(InputAction.CallbackContext context)
    {
        player_view_controller.SetViewType(PlayerViewEnum.Order);
        GameOverseer.THE_OVERSEER.canvas_control.PlayerStartInput();
    }
    void CancelOrderState(InputAction.CallbackContext context)
    {
        player_view_controller.ResetViewType();
        pointer_delta = Vector2.zero;
        GameOverseer.THE_OVERSEER.canvas_control.PlayerEndInput();
        ResetConfirm();
    }

    void ResetConfirm()
    {
        op_select_index = -1;
        ToggleCommandInput(false);
        squad.SetSelectedOperator(op_select_index);
        GameOverseer.THE_OVERSEER.canvas_control.ToggleReticleCommandUI(false);
    }

    #endregion

    #region Action Enabling

    void ToggleCommandInput(bool cmd_on)
    {
        if (cmd_on)
        {
            main_action.started += StartOrderState;
            main_action.canceled += CancelOrderState;
            SetMainAction(false);
            SetAltAction(false);
        } 
        else
        {
            main_action.started -= StartOrderState;
            main_action.canceled -= CancelOrderState;
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
