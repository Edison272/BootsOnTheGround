using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    public Animator ui_animator;
    public Canvas canvas;
    [Header("Cameras")]
    [SerializeField] private Camera main_cam; // used to get the FULL area around the player and render that for player view
    [SerializeField] private Camera play_cam; // the area the player sees and interacts with
    [SerializeField] private RawImage player_screen;

    [Header("Squad Sidebar")]
    [SerializeField] GameObject squad_sidebar;
    [SerializeField] GameObject op_layout_group;
    [SerializeField] GameObject op_status_instance;
    [SerializeField] OperatorStatusUI[] op_statuses;
    [Header("Enemy Wavebar")]
    [SerializeField] GameObject wave_bar;
    [SerializeField] RectTransform enemy_bar; // represent the amount of waves

    [Header("Command UI")]
    [SerializeField] Button[] available_commands;
    [SerializeField] TextMeshProUGUI player_mode_text;
    [SerializeField] public Character[] character_buffer;
    [SerializeField] GameObject command_reticle;

    [Header("Command Internals")]
    private CommandMode curr_command = CommandMode.Follow;

    [Header("Pointer UI")]
    bool pointer_requested = false;
    [SerializeField] GameObject[] movement_indicators;

    public RectTransform canvas_cursor;
    [Header("Player Controller")]
    private PlayerController player_controller;

    [Header("Action Seelctor UI")]
    public RectTransform action_selector;
    public RectTransform selector_base;
    public RectTransform action_indicator;
    private Vector2 selection_vector = Vector2.zero;
    public Vector2 converted_selection_vector = Vector2.zero;
    [SerializeField]private float indicator_range = 0;
    
    // Start is called before the first frame update
    void Awake()
    {
        foreach(OperatorStatusUI stat_ui in op_statuses)
        {
            stat_ui.gameObject.SetActive(false);
            command_reticle.SetActive(false);
            action_selector.gameObject.SetActive(false);
        }

        // ui action wheel
        indicator_range = new Vector2(selector_base.rect.width, selector_base.rect.height).magnitude/2;
    }
    
    void Start()
    {
        player_controller = GameOverseer.THE_OVERSEER.player_control;
    }

    void Update()
    {
        if (pointer_requested)
        {
            foreach(OperatorStatusUI stat_ui in op_statuses)
            {
                int i = stat_ui.squad_index;
                if (character_buffer[i] != null)
                {
                    movement_indicators[i].transform.position =  player_controller.look_pos;
                    LineRenderer lr = movement_indicators[i].GetComponent<LineRenderer>();
                    lr.SetPosition(0, character_buffer[i].GetPosition());
                    lr.SetPosition(1, movement_indicators[i].transform.position);
                }
            }
        }

        // Vector2 viewport_pos = GameOverseer.THE_OVERSEER.player_control.player_view_controller.viewport_pos;
        // Rect canvas_rect = canvas.GetComponent<RectTransform>().rect;
        // action_selector.anchoredPosition = new Vector2((viewport_pos.x-0.5f)*canvas_rect.width*2, (viewport_pos.y-0.5f)*canvas_rect.height*2);
        canvas_cursor.anchoredPosition = player_controller.player_view_controller.screen_pos*player_screen.GetComponent<RectTransform>().localScale.x;
        if (action_selector.gameObject.activeSelf)
        {
            Vector3 canvas_pos = player_controller.player_view_controller.screen_pos*player_screen.GetComponent<RectTransform>().localScale.x;
            if (action_selector.localScale.x == 1) // action selector only expands if player is holding m1 down
            {
                canvas_pos = player_controller.player_view_controller.hold_screen_pos*player_screen.GetComponent<RectTransform>().localScale.x;
            }
            action_selector.anchoredPosition = canvas_pos;
            selection_vector += GameOverseer.THE_OVERSEER.player_control.player_view_controller.last_pointer_delta * (int)action_selector.localScale.x;
            float abs_x = Mathf.Abs(selection_vector.x);
            float abs_y = Mathf.Abs(selection_vector.y);
            float manhat_distance = abs_x + abs_y;
            
            if (manhat_distance > indicator_range)
            {
                float scale = indicator_range / manhat_distance;

                selection_vector = new Vector2(selection_vector.x * scale, selection_vector.y * scale);
            }
            
            if (selection_vector.magnitude > indicator_range * 0.5f)
            {
                Vector2 center_to_select = selection_vector.normalized;
                bool x_over_y = Mathf.Abs(center_to_select.x) > Mathf.Abs(center_to_select.y);
                float convert_X = x_over_y ? (float)Math.Round(center_to_select.x) : (int)center_to_select.x;
                float convert_y = !x_over_y ? (float)Math.Round(center_to_select.y) : (int)center_to_select.y;
                converted_selection_vector = new Vector2(convert_X, convert_y);
                action_indicator.anchoredPosition = converted_selection_vector * indicator_range * 0.75f;
            }
            else
            {
                action_indicator.anchoredPosition = selection_vector;
            }
        }

        // if (command_reticle.activeSelf)
        // {
        //     command_reticle.transform.position = GameOverseer.THE_OVERSEER.player_control.look_pos;
        // }
    }
    //  set up UI for all operators after they've been initialized. Called by Game Overseer
    public void SetOperatorProfiles()
    {
        Character[] squad_members = GameOverseer.THE_OVERSEER.squad_manager.operators;
        movement_indicators = new GameObject[squad_members.Length];
        for (int i = 1; i < squad_members.Length; i++) // make UI for every operator EXCEPT the player
        {
            op_statuses[i].gameObject.SetActive(true);
            op_statuses[i].ConstructUI(squad_members[i], i);

            // get a movement indicator
            // movement_indicators[i] = Instantiate(Resources.Load<GameObject>("UI/MovePointer"));
            // movement_indicators[i].SetActive(false);
        }
        character_buffer = new Character[squad_members.Length];
    }

    public void SetCommandUI(bool is_cmd_mode_on)
    {
        if (is_cmd_mode_on)
        {
            squad_sidebar.SetActive(is_cmd_mode_on);
            player_mode_text.text = "Command Mode";
        }
        else
        {
            squad_sidebar.SetActive(is_cmd_mode_on);
            player_mode_text.text = "Action Mode";
        }
    }

    public void SetBuffer(int index, bool is_present)
    {
        character_buffer[index] = is_present ? GameOverseer.THE_OVERSEER.squad_manager.operators[index] : null;
    }
    public void SetCommandType(int command_mode)
    {
        pointer_requested = false;
        curr_command = (CommandMode)command_mode;
        if (command_mode <= (int)CommandMode.Engage) 
        {
            ConfirmCommand();
        } 
        else
        {
            pointer_requested = true;
            foreach(OperatorStatusUI stat_ui in op_statuses)
            {
                int i = stat_ui.squad_index;
                if (character_buffer[i] != null)
                {
                    movement_indicators[i].SetActive(true);
                }
            }
        }
    }

    public void ConfirmCommand() // used for holding position or engaging targets
    {
        int ops_commanded = 0;
        foreach(OperatorStatusUI stat_ui in op_statuses)
        {
            int i = stat_ui.squad_index;
            if (character_buffer[i] != null)
            {
                character_buffer[i].SetCommandBehavior(curr_command);
                ops_commanded += 1;

                if (pointer_requested)
                {
                    character_buffer[i].behavior_controller.anchor_position = player_controller.look_pos;
                }

                movement_indicators[i].SetActive(false);
            }
            stat_ui.ConfirmOperator(false);
        }
        pointer_requested = false;
    }

    #region Op Placement UI FX

    public void ToggleReticleCommandUI(bool enable)
    {
        command_reticle.SetActive(enable);
        action_selector.localScale = new Vector3(0.5f, 0.5f, 1); 
        action_selector.gameObject.SetActive(enable);
    }


    #endregion

    #region Player-Canvas Input
    public void PlayerStartInput()
    {
        action_selector.localScale = new Vector3(1f, 1f, 1);
        canvas_cursor.gameObject.SetActive(false);
        selection_vector = Vector2.zero;
        
    }
    public void PlayerEndInput()
    {
        action_selector.gameObject.SetActive(false);
        canvas_cursor.gameObject.SetActive(true);

        switch ((converted_selection_vector.x, converted_selection_vector.y))
        {
            case (-1f,0): // follow
                GameOverseer.THE_OVERSEER.squad_manager.SwitchOpBehavior(false);
                break;
            case (1f,0): // hold
                GameOverseer.THE_OVERSEER.squad_manager.SwitchOpBehavior(true);
                break;
            case (0,1f): // ability
                GameOverseer.THE_OVERSEER.squad_manager.UseOpAbility(player_controller.player_view_controller.look_pos);
                break;
            case (0,-1f): // interact
                GameOverseer.THE_OVERSEER.squad_manager.SwitchOpBehavior(true);
                break;
        }

    }
    
    #endregion
}
