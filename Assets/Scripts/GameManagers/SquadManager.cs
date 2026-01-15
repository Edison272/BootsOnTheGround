using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CommandMode {Follow, Disperse, Engage, Count}
/*
- Follow the player around closely, move when they move
- Disperse take cover and maintain distance from threats. Move further away from the player if necessary
- Engage pursue targets. Move further away from player with necessary
*/
public class SquadManager : MonoBehaviour
{
    public CharacterSO[] squad_preset;
    public Character[] squad;

    public Vector3 drop_pos;

    [Header("Managers")]
    public PlayerController player;
    public GameOverseer overseer;

    [Header("Commands")]
    public CommandMode curr_command;
    

    public void CreateSquad()
    {
        squad = new Character[squad_preset.Length];
        for(int i = 0; i < squad_preset.Length; i++)
        {
            squad[i] = squad_preset[i].GenerateOp(transform.position);
        }
        player.SetPlayerCharacter(squad[0]);
    }

    public void ToggleCommandMode()
    {
        curr_command = (CommandMode)(((int)curr_command + 1) % (int)CommandMode.Count-1);
        Debug.Log("Set Command to: "  + curr_command);
    }

    void Update()
    {
        foreach(Character character in squad)
        {
            if (!character.target)
            {
                overseer.GetTargetCharacter(true, character);
            }
            else
            {
                character.Look(character.target.GetPosition());
                character.UseMainItem();
            }
        }
    }
}
