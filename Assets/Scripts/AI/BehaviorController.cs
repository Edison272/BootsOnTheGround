using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// control a character using a series of behaviors

public enum BotStrategy{Offensive, Defensive, Evasive}
/*
- Offensive
Pursue target position
- Defensive
Hold your ground, stay close to your objective position
- Evasive
Prioritize Self Preservation
*/
public class BehaviorController
{
    private Dictionary<string, BehaviorModule> movement_behaviors; 
    private Dictionary<string, BehaviorModule> action_behaviors; 

    [SerializeField] Character character;

    public BehaviorController(Character c)
    {
        character = c;
    }

    public void UpdateAI()
    {
        if (character.target == null)
        {
            character.target = GameOverseer.THE_OVERSEER.GetTargetCharacter(character.is_player_squad, character);
        }
        else
        {
            character.Look(character.target.GetPosition());
            character.UseMainItem();
        }
    }
}
