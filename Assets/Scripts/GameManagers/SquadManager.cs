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
    public static readonly int player_char_index = 0; // PLAYER CHARACTER IS FIRST IN SQUAD ARRAY
    public Character player_character;

    public Vector3 drop_pos;

    [Header("Managers")]
    public PlayerController player;
    
    public void InitializeAllies()
    {
        squad = new HashSet<Character>();
        helpers = new HashSet<Character>();
        operators = new Operator[operator_presets.Length];

        CreateOperators();

        // Activate the player-controlled operator
        player_character = operators[player_char_index];
        player_character.is_player_squad = true;
        player.SetPlayerCharacter(player_character);
        UseOperator(player_char_index, transform.position);
        player_character.ToggleAI(false);
        
    }

    public void CreateSquad()
    {


        SetSquadLeader();
    }

    public void SetSquadLeader() // set squad leader to player characte
    {
        foreach(Character squadmate in squad)
        {
            squadmate.behavior_controller.SetLeader(operators[player_char_index]);
        }
    }

    public void CreateOperators()
    {
        // setup all operators. Assign their values but do not set them to active
        for(int i = 0; i < operators.Length; i++)
        {
            operators[i] = operator_presets[i].GenerateOp(Vector2.zero);
            operators[i].is_player_squad = true;
            operators[i].ToggleAI(true);
            operators[i].ToggleOp(false);
            operators[i].behavior_controller.SetLeader(operators[player_char_index]);
            squad.Add(operators[i]);
        }
    }

    public void UseOperator(int op_index, Vector2 deploy_pos)
    {
        Operator select_op = operators[Mathf.Clamp(op_index, 0, operator_presets.Length-1)];
        if (!select_op.is_deployed)
        {
            select_op.SetPosition(deploy_pos);
            select_op.ToggleOp(true);
            select_op.ToggleAI(true);
            select_op.Deploy();
        }
        else
        {
            select_op.UseAbility(deploy_pos);
        }
    }

    public void RetreatOperator(int op_index)
    {
        Operator select_op = operators[Mathf.Clamp(op_index, 0, operator_presets.Length-1)];
        if (select_op.is_deployed)
        {
            select_op.ToggleOp(false);
            select_op.ToggleAI(false);
            select_op.is_deployed = false;
        }
    }

    // public void ToggleCommandMode()
    // {
    //     curr_command = (CommandMode)(((int)curr_command + 1) % (int)CommandMode.Count-1);
    //     Debug.Log("Set Command to: "  + curr_command);
    // }

    public void SquadMateLost()
    {
        
    }
}
