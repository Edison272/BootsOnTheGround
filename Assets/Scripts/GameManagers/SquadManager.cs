using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

using Random = UnityEngine.Random;

public enum CommandMode {Follow, Disperse, Engage, Hold, Count}
/*
- Follow the player around closely, move when they move
- Disperse take cover and maintain distance from threats. Move further away from the player if necessary
- Engage pursue targets. Move further away from player with necessary
*/
public class SquadManager : MonoBehaviour
{
    public CharacterSO[] helper_presets;
    public OperatorSO[] operator_presets;
    public HashSet<Character> squad;
    public HashSet<Character> helpers;
    public Operator[] operators;
    
    public static readonly int c_player_char_index = 0; // PLAYER CHARACTER IS FIRST IN SQUAD ARRAY
    public Character player_character;

    public Vector3 drop_pos;

    [Header("Managers")]
    public PlayerController player;

    [Header("Squad Command Data")]
    public Operator selected_operator;
    public Vector2[] combat_formation; // how the operators align themselves towards the enemy in combat
    public Vector2[] explore_formation; // how the operators move with the player

    [Header("Squad UI")]
    public SquadUIController squad_ui_control;
    
    public void InitializeAllies()
    {
        squad = new HashSet<Character>();
        helpers = new HashSet<Character>();
        operators = new Operator[operator_presets.Length];
        combat_formation = new Vector2[operator_presets.Length];

        CreateOperators();

        // reconfigure settings for player character so they don't get taken over by a bot
        player_character = operators[c_player_char_index];
        player_character.faction_tag = GameOverseer.SQUAD_TAG;
        player.SetPlayerCharacter(player_character);
        DeployOperator(c_player_char_index, transform.position);
        player_character.ToggleAI(false);

        SetSquadLeader();
        SetSquadUI();

        // set explore formation

        // set combat formation
        
    }

    public void SetSquadLeader() // set squad leader to player characte
    {
        int i = 0;
        foreach(Character squadmate in squad)
        {
            squadmate.behavior_controller.SetLeader(player_character);

            if (squadmate != player_character)
            {
                squadmate.SetPosition(player_character.GetPosition() + (Vector2)Directions2D.four_directions[i] * 3);
                i++;
                i = Mathf.Clamp(i, 0, 3);
            }
        }
    }

    public void SetSquadUI()
    {
        squad_ui_control.SetSquadManager(this);
    }

    public void CreateOperators()
    {
        // setup all operators. Assign their values but do not set them to active
        for(int i = 0; i < operators.Length; i++)
        {
            operators[i] = operator_presets[i].GenerateOp(Vector2.zero);
            Operator this_op = operators[i];
            this_op.faction_tag = GameOverseer.SQUAD_TAG;
            this_op.behavior_controller.SetLeader(operators[c_player_char_index]);
            this_op.op_behavior_controller.squad_index = i;
            this_op.AssignIdString(i);
            squad.Add(operators[i]);

            DeployOperator(i, transform.position);
        }
    }

    public void Update()
    {
        
        UpdateFormation();

    }

    public void FixedUpdate()
    {
        foreach(Operator op in operators)
        {
            if (op.IsInAction())
            {
                GameOverseer.THE_OVERSEER.map_manager.SetNewPosition(op);
            }
        }
    }

    // set operator formation
    public void UpdateFormation()
    {
        Vector2 center = player_character.GetPosition();
        for(int i = 0; i < operators.Length; i++)
        {
            Operator curr_op = operators[i];
            Vector2 offset_vec = player_character.aim_dir.normalized * 10/((int)curr_op.op_class + 1);
            combat_formation[i] = center + offset_vec;
        }
    }

    #region Operator Control Commands
    // setup UI and other stuff to let player know the current op is selected
    public void SetSelectedOperator(int op_index)
    {
        // cancel command if it was done twice
        if (op_index == -1) {
            if (selected_operator)
            {
                selected_operator.ToggleSelectionIndicator(false);
            }
            selected_operator = null;
            squad_ui_control.SelectOperatorUI(op_index);
        } 
        // otherwise do smth cool n shi
        else {
            int selection_index = Mathf.Clamp(op_index, 0, operator_presets.Length-1);
            selected_operator?.ToggleSelectionIndicator(false);   
            if (operators[selection_index])
            {
                squad_ui_control.SelectOperatorUI(selection_index);
                selected_operator = operators[selection_index];
                selected_operator?.ToggleSelectionIndicator(true);             
            }
        }
    }

    public void DeployOperator(int op_index, Vector2 deploy_pos)
    {
        Operator select_op = operators[Mathf.Clamp(op_index, 0, operator_presets.Length-1)];
        select_op.SetPosition(deploy_pos);
        select_op.ToggleOp(true);
        select_op.ToggleAI(true);
        select_op.SetCommandBehavior(CommandMode.Follow);
        select_op.Deploy();
    }

    // used to deploy initialized operators and redeploy falled operators
    public void UseOpAbility(Vector2 deploy_pos)
    {
        if (selected_operator)
        {
            selected_operator.UseAbility(deploy_pos);
        }
    }

    public void SwitchOpBehavior(bool hold_or_follow = true)
    {
        if (hold_or_follow)
        {
            selected_operator.op_behavior_controller.anchor_position = GameOverseer.THE_OVERSEER.player_control.look_pos;
            selected_operator.SetCommandBehavior(CommandMode.Hold);
        } 
        else
        {
            selected_operator.SetCommandBehavior(CommandMode.Follow);
        }
    }
    // public void RetreatOperator(int op_index)
    // {
    //     Operator select_op = operators[Mathf.Clamp(op_index, 0, operator_presets.Length-1)];
    //     if (select_op.is_deployed)
    //     {
    //         select_op.ToggleOp(false);
    //         select_op.ToggleAI(false);
    //         select_op.is_deployed = false;
    //     }
    // }

    #endregion

    // public void ToggleCommandMode()
    // {
    //     curr_command = (CommandMode)(((int)curr_command + 1) % (int)CommandMode.Count-1);
    //     Debug.Log("Set Command to: "  + curr_command);
    // }
    #region Operator Redeploy
    public void RedeployOperator(Operator redeploy_op, float redploy_time)
    {
        StartCoroutine(EnumRedeployOperator(redeploy_op, redploy_time));
    }
    private IEnumerator EnumRedeployOperator(Operator redeploy_op, float redploy_time)
    {
        yield return new WaitForSeconds(redploy_time);
        redeploy_op.Redeploy();
    }
    #endregion
    public void SquadMateLost()
    {
        
    }
}
