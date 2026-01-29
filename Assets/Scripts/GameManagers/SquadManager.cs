using System.Collections;
using System.Collections.Generic;
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
    public CharacterSO[] squad_preset;
    public Character[] squad;
    public int player_char_index = 0;
    public Character player_character;

    public Vector3 drop_pos;

    [Header("Managers")]
    public PlayerController player;
    

    public void CreateSquad()
    {
        squad = new Character[squad_preset.Length];

        // player position
        squad[player_char_index] = squad_preset[player_char_index].GenerateOp(transform.position);
        player_character = squad[player_char_index];
        player_character.is_player_squad = true;
        player.SetPlayerCharacter(player_character);
        player_character.ToggleAI(false);
        

        // set squadmates around player
        for(int i = 0; i < squad_preset.Length; i++)
        {
            if (i == player_char_index)
            {
                continue;
            }
            Vector3 vec_offset = (Vector3)(Vector2)Directions2D.eight_directions[(int)(7 * (i/(float)squad_preset.Length))];
            squad[i] = squad_preset[i].GenerateOp(transform.position + vec_offset * Random.Range(1f, 2f));
            squad[i].is_player_squad = true;
            squad[i].ToggleAI(true);
        }

        SetSquadLeader();
    }

    public void SetSquadLeader() // set squad leader to player characte
    {
        foreach(Character squadmate in squad)
        {
            squadmate.behavior_controller.SetLeader(squad[player_char_index]);
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
