using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadManager : MonoBehaviour
{
    public CharacterSO[] squad_preset;
    public Character[] squad;

    public Vector3 drop_pos;

    public PlayerController player;

    public void Awake()
    {
        CreateSquad();
    }
    public void CreateSquad()
    {
        squad = new Character[squad_preset.Length];
        for(int i = 0; i < squad_preset.Length; i++)
        {
            squad[i] = squad_preset[i].GenerateOp();
        }
        player.SetPlayerCharacter(squad[0]);
    }
}
