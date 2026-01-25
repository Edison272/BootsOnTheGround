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
    
    // Start is called before the first frame update
    void Start()
    {
        foreach(OperatorStatusUI stat_ui in op_statuses)
        {
            stat_ui.gameObject.SetActive(false);
        }
    }
    //  set up UI for all operators after they've been initialized. Called by Game Overseer
    public void SetOperatorProfiles()
    {
        Character[] squad_members = GameOverseer.THE_OVERSEER.squad_manager.squad;
        for (int i = 1; i < squad_members.Length; i++) // make UI for every operator EXCEPT the player
        {
            op_statuses[i].gameObject.SetActive(true);
            op_statuses[i].ConstructUI(squad_members[i], i);
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
        CommandMode cmd_mode = (CommandMode)command_mode;
        switch (cmd_mode)
        {
            case CommandMode.Follow:
                break;
            case CommandMode.Disperse:
                break;
            case CommandMode.Engage:
                break;
            case CommandMode.Hold:
                break;
        }

        int ops_commanded = 0;
        foreach(OperatorStatusUI stat_ui in op_statuses)
        {
            if (character_buffer[stat_ui.squad_index] != null)
            {
                character_buffer[stat_ui.squad_index].SetCommandBehavior(cmd_mode);
                ops_commanded += 1;
            }
            stat_ui.ConfirmOperator(false);
        }
    }
}
