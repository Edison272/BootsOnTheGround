using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    public Animator ui_animator;

    [Header("Squad Sidebar")]
    [SerializeField] GameObject squad_sidebar;
    [SerializeField] GameObject op_layout_group;
    [SerializeField] GameObject op_status_instance;
    [SerializeField] OperatorStatusUI[] op_statuses;
    [Header("Command UI")]
    [SerializeField] Button[] available_commands;
    [SerializeField] TextMeshProUGUI player_mode_text;
    [SerializeField] public Character[] character_buffer;

    [Header("Command Internals")]
    private CommandMode curr_command = CommandMode.Follow;

    [Header("Pointer UI")]
    bool pointer_requested = false;
    [SerializeField] GameObject[] movement_indicators;
    
    // Start is called before the first frame update
    void Awake()
    {
        foreach(OperatorStatusUI stat_ui in op_statuses)
        {
            stat_ui.gameObject.SetActive(false);
        }
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
                    movement_indicators[i].transform.position =  GameOverseer.THE_OVERSEER.player_control.look_pos;
                    LineRenderer lr = movement_indicators[i].GetComponent<LineRenderer>();
                    lr.SetPosition(0, character_buffer[i].GetPosition());
                    lr.SetPosition(1, movement_indicators[i].transform.position);
                }
            }
        }
    }
    //  set up UI for all operators after they've been initialized. Called by Game Overseer
    public void SetOperatorProfiles()
    {
        Character[] squad_members = GameOverseer.THE_OVERSEER.squad_manager.squad;
        movement_indicators = new GameObject[squad_members.Length];
        for (int i = 1; i < squad_members.Length; i++) // make UI for every operator EXCEPT the player
        {
            op_statuses[i].gameObject.SetActive(true);
            op_statuses[i].ConstructUI(squad_members[i], i);

            // get a movement indicator
            movement_indicators[i] = Instantiate(Resources.Load<GameObject>("UI/MovePointer"));
            movement_indicators[i].SetActive(false);
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
        character_buffer[index] = is_present ? GameOverseer.THE_OVERSEER.squad_manager.squad[index] : null;
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
                    character_buffer[i].behavior_controller.anchor_position = GameOverseer.THE_OVERSEER.player_control.look_pos;
                }

                movement_indicators[i].SetActive(false);
            }
            stat_ui.ConfirmOperator(false);
        }
        pointer_requested = false;
    }

    #region Player-Canvas Input
    public void PlayerStartInput()
    {
        if (pointer_requested)
        {
            Debug.Log("Mouse down");
            ConfirmCommand();
        }
        
    }
    public void PlayerEndInput()
    {
        // Debug.Log("Mouse Up");
    }
    #endregion
}
