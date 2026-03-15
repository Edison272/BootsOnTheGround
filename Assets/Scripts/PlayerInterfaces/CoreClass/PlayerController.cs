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
    InputAction order_action;
    bool hold_order = false; // checked on update to see if the alt action should be called
    public float curr_hold_order_time {get; private set;}
    public float max_hold_order_time {get; private set;} = 0.5f; // when maximum time is reached, call all operators back to the player
    public bool alt_hold_input = false;  // if true, mouse will alt action function will constantly be called
    
    [Header("Player Cams & Screen")]
    // move BOTH cameras to the player positon cuz they're both important. make play cam a child object of main cam tho
    [SerializeField] private Camera main_cam; // used to get the FULL area around the player and render that for player view
    [SerializeField] private Transform MainCameraHolder; // hold the main camera, so that the main_cam can do animations n shi
    [SerializeField] private Camera play_cam; // the area the player sees and interacts with
    [SerializeField] RawImage player_screen; // a canvas raw image the player uses to see the world
    [SerializeField] Vector2 input_dir;
    private Vector2 pointer_delta;
    public Vector3 viewport_pos => player_view_controller.viewport_pos;
    public Vector2 look_pos => player_view_controller.look_pos;
    public Vector2 screen_pos => player_view_controller.screen_pos;
    [SerializeField] public Character active_character {get; private set;}
    bool in_command_mode = false; // can only move while in command mode

    [Header("Player View")]
    public PlayerViewController player_view_controller;
    
    [Header("Camera Control")]
    public MainCameraController main_cam_controller;
    [Header("Player Action Control")]
    public PlayerActionController player_action_controller; // seperate class for ui-based player actions
    [Header("Player Targetting Control")]
    public TargettingController player_targetting_controller; // seperate class for ui-based player actions
    [SerializeField] LineRenderer main_line_renderer;
    [SerializeField] LineRenderer vfx_line_renderer;
    [SerializeField] Color origin_color_serialized; // line always starts out white and semi transparent
    [SerializeField] Color wall_color_serialized; // line render color when the player is looking at a wall
    [SerializeField] Color base_color_serialized;
    [SerializeField] Color crit_color_serialized; // line render color when the player is aiming at center of mass
    public static Color origin_color;
    public static Color wall_color;
    public static Color base_color;
    public static Color crit_color;

    [Header("Order Controller")]
    public OrderController order_controller;

    [Header("UI Stuff")]
    [SerializeField] GameObject cursor;

    [Header("Item Inteaction")]
    IInteractable closest_interactable;
    IInteractable previous_interactable = null;

    [Header("Squad Interactions")]
    public SquadManager squad;
    private int op_select_index = -1;

    [Header("Character UI")]
    public ItemUIController item_ui_control;
    public HealthUIController health_ui_control;
    [Header("Order UI")]
    public OrderUIController order_ui_control;

    [Header("Settings")]
    [Range(0.1f, 3)] public float sensitivity = 2f;
    
#region Initializers
    void Awake() // initialize values before player assumes control
    {
        // get map
        controls = new BOTG_Controls();
        
        // map input actions to the controls
        movement = controls.GroundActions.Move;
        looking = controls.GroundActions.Look;
        main_action = controls.GroundActions.MainAction;
        order_action = controls.GroundActions.OrderAction;

        // set colors for targetting
        origin_color = origin_color_serialized;
        wall_color = wall_color_serialized;
        base_color = base_color_serialized;
        crit_color = crit_color_serialized;

        // set helper classes
        player_view_controller = new PlayerViewController(main_cam, player_screen.rectTransform);
        main_cam_controller = new MainCameraController(MainCameraHolder, main_cam, player_screen.rectTransform);
        player_targetting_controller = new TargettingController(main_line_renderer, vfx_line_renderer);
        player_view_controller.SetMCController(main_cam_controller);
        main_cam_controller.SetPVController(player_view_controller);
    }

    void Start()
    {
        // other helper classes dependent on multiple managers
        order_controller = new OrderController(this, squad);
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
        SetMainAction(true);
        EnableControl();
        item_ui_control.SetActiveCharacter(active_character);
        health_ui_control.SetActiveCharacter(active_character);
        order_ui_control.SetOrderController(this);
    }

    public void FreeCursor(bool is_free)
    {
        if (is_free)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        
        Cursor.visible = is_free;
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
        SetOrderAction(true);
        // reset the item (reload, recharge, reset stance, cool animation etc.)
        controls.GroundActions.ResetItem.performed += ResetItem;
        // switch the item
        controls.GroundActions.SwitchItem.performed += SwitchItem;
        // pickup / Interact
        controls.GroundActions.PickupItem.performed += PickupItem;
        // Toggle Command Mode
        controls.GroundActions.ToggleCommandMode.performed += CommandView;
        // Deploy Op with num keys
        controls.GroundActions.OpDeploy1.performed += OpDeploy1;
        controls.GroundActions.OpDeploy2.performed += OpDeploy2;
        controls.GroundActions.OpDeploy3.performed += OpDeploy3;
        controls.GroundActions.OpDeploy4.performed += OpDeploy4;
        // Activate selected operator ability with q
        controls.GroundActions.OpAbility.performed += GetOperatorAbility;

        main_cam_controller.SetCameraZoom(active_character.GetRangeScalar());
        GameOverseer.THE_OVERSEER.canvas_control.SetCommandUI(in_command_mode);
    }

    void OnDisable()
    {
        DisableControl();
        //unsubscribe from everything when scene is reset
        movement.performed -= Move;
        looking.performed -= Look;
        controls.GroundActions.ResetItem.performed -= ResetItem;
        controls.GroundActions.SwitchItem.performed -= SwitchItem;
        controls.GroundActions.PickupItem.performed -= PickupItem;
        controls.GroundActions.ToggleCommandMode.performed -= CommandView;
        controls.GroundActions.OpDeploy1.performed -= OpDeploy1;
        controls.GroundActions.OpDeploy2.performed -= OpDeploy2;
        controls.GroundActions.OpDeploy3.performed -= OpDeploy3;
        controls.GroundActions.OpDeploy4.performed -= OpDeploy4;
        controls.GroundActions.OpAbility.performed -= GetOperatorAbility;
        SetMainAction(false);
        SetOrderAction(false);
    }

    public void EnableControl()
    {
        controls.Enable();
    }

    public void DisableControl()
    {
        controls.Disable();
    }
    #endregion

    #region Updates
    // Update is called once per frame
    void Update()
    {
        // prepare camera data
        player_view_controller.UpdateLookPos();
        if (active_character)
        {
            // hold inputs
            if (main_continuous) {MainAction();}
            if (hold_order) {HoldOrderIput();}
            
            // set look pos
            player_view_controller.UpdateView(pointer_delta*0.75f, active_character.GetPosition());
            pointer_delta = Vector2.zero;
            active_character.Look(look_pos);
            main_cam_controller.UpdateCamData(active_character.GetPosition(), look_pos);
            player_targetting_controller.UpdateTargetting(active_character);

            if (!active_character.IsInAction())
            {
                GameOverseer.THE_OVERSEER.GameOver();
            }

            // do some ui stuff too
            closest_interactable = active_character.FindInteractables();
            if (previous_interactable != closest_interactable)
            {
                closest_interactable?.ToggleInteractPrompt(true);
                previous_interactable?.ToggleInteractPrompt(false);
                previous_interactable = closest_interactable;
            }
            
        }
        order_controller.UpdateOrderControl(look_pos);
    }
    void LateUpdate()
    {   
        // adjust cursor
        cursor.transform.position = look_pos;
        // update controllers
        main_cam_controller.UpdateCamRender();
        
    }
    #endregion
    #region
    public void ApplyCameraRecoil(Vector2 direction, float recoil_amount)
    {
        main_cam_controller.ApplyCameraRecoil(direction, recoil_amount);
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
        pointer_delta += sensitivity * looking.ReadValue<Vector2>()/main_cam_controller.target_zoom;
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
    // void AltStart(InputAction.CallbackContext context) {
    //     active_character.UseAltItem();
    //     hold_order = true; = alt_hold_input;
         
    // }
    // void AltStop(InputAction.CallbackContext context) {
    //     active_character.StopAltItem();
    //     hold_order = true; = false;
    // }
    void OrderAction() {active_character.UseAltItem();}
    void ResetItem(InputAction.CallbackContext context) {active_character.ResetItems();}
    void SwitchItem(InputAction.CallbackContext context) {
        // set new input functionality
        if (op_select_index == -1)
        {
            active_character.SwitchItem();
            SetMainAction(true);
            main_cam_controller.SetCameraZoom(active_character.GetRangeScalar());
        }
    }
    void PickupItem(InputAction.CallbackContext context)
    {
        if (closest_interactable != null)
        {
            active_character.UseInteractable(closest_interactable);
            SetMainAction(true);
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
            // SetOrderAction(false);
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
            // SetOrderAction(active_character.HasOrderAction());
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

    #region Operator Order
    void HoldOrderIput()
    {
        curr_hold_order_time += Time.deltaTime;
        // have all operators return to player when order button is held
        if (curr_hold_order_time >= max_hold_order_time)
        {
            order_controller.RecallAll();
            hold_order = false;
            curr_hold_order_time = 0;
        }
    }
    void StartOrderInput(InputAction.CallbackContext context)
    {
        hold_order = true;
        curr_hold_order_time = 0;
    }

    void StopOrderInput(InputAction.CallbackContext context)
    {
        if (curr_hold_order_time < max_hold_order_time)
        {
            order_controller.GiveOrder();
        }
        curr_hold_order_time = 0;
        hold_order = false;
    }


    #endregion

    #region Quick Ability cast
    void OpDeploy1(InputAction.CallbackContext context) {GetOperator(1);} // start at 1 bc og player char is in index 0
    void OpDeploy2(InputAction.CallbackContext context) {GetOperator(2);}
    void OpDeploy3(InputAction.CallbackContext context) {GetOperator(3);}
    void OpDeploy4(InputAction.CallbackContext context) {GetOperator(4);}
    void GetOperator(int deploy_index = 1)
    {
        squad.SetSelectedOperator(deploy_index);
        squad.UseOpAbility(look_pos);
        squad.SetSelectedOperator(-1);
        // // cancel command if it was done twice
        // if (op_select_index == deploy_index) {
        //     ResetOrder();
        // } 
        // // otherwise select operator and enable operator commands
        // else {
        //     op_select_index = deploy_index;
        //     squad.SetSelectedOperator(op_select_index);
        //     ToggleCommandInput(true);
        //     GameOverseer.THE_OVERSEER.canvas_control.ToggleReticleCommandUI(true);
        // }
        
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
        ResetOrder();
    }
    void ResetOrder()
    {
        op_select_index = -1;
        ToggleCommandInput(false);
        squad.SetSelectedOperator(op_select_index);
        GameOverseer.THE_OVERSEER.canvas_control.ToggleReticleCommandUI(false);
    }
    void GetOperatorAbility(InputAction.CallbackContext context)
    {
        
    }

    void CancelOperatorAbility()
    {
        
    }
    void ConfirmOperatorAbility()
    {
        
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
            SetOrderAction(false);
        } 
        else
        {
            main_action.started -= StartOrderState;
            main_action.canceled -= CancelOrderState;
            SetMainAction(true);

            // make sure the player doesn't leak an attack after commanding
            main_continuous = false;
            hold_order = false;
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

    void SetOrderAction(bool enable) // set alt action. set parameter false to turn off main actions
    {
        
        if(enable)
        {
            order_action.started += StartOrderInput;
            order_action.canceled += StopOrderInput;
        }
        else
        {
            order_action.started -= StartOrderInput;
            order_action.canceled -= StopOrderInput;
        }
        // if (enable)
        // {
        //     // order_action.started += GiveOrder;
        //     // order_action.canceled += AltStop;
        //     alt_hold_input = active_character.alt_item.IsHoldInput(); // only set alt_hold_input if an order_action exists
        //     hold_order = true; = order_action.IsPressed() && alt_hold_input;
        // }
        // else
        // {
        //     // order_action.started -= GiveOrder;
        //     // order_action.canceled -= AltStop;   
        //     hold_order = true; = false; // stop any ongoing attacks
        // }
    }
    #endregion
}
