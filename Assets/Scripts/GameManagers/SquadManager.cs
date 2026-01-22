using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CommandMode {Follow, Disperse, Engage, Hold, Count}
/*
- Follow the player around closely, move when they move
- Disperse take cover and maintain distance from threats. Move further away from the player if necessary
- Engage pursue targets. Move further away from player with necessary
*/
public class SquadManager : MonoBehaviour
{
    public CharacterSO[] squad_preset;
    public Character[] squad;
    public int player_char_index = 0;

    public Vector3 drop_pos;

    [Header("Managers")]
    public PlayerController player;

    [Header("Commands")]
    public CommandMode curr_command;
    

    public void CreateSquad()
    {
        squad = new Character[squad_preset.Length];
        for(int i = 0; i < squad_preset.Length; i++)
        {
            squad[i] = squad_preset[i].GenerateOp(transform.position);
            squad[i].is_player_squad = true;
            squad[i].ToggleAI(true);
        }
        player.SetPlayerCharacter(squad[player_char_index]);
        squad[player_char_index].ToggleAI(false);
    }

    public void ToggleCommandMode()
    {
        curr_command = (CommandMode)(((int)curr_command + 1) % (int)CommandMode.Count-1);
        Debug.Log("Set Command to: "  + curr_command);
    }
}
