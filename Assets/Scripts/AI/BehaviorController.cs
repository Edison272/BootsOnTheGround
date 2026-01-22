using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// control a character using a series of behaviors

public enum BotStrategy{Offensive, Defensive, Evasive}
public enum MovementMode{Hold, Follow, Retreat, Pursue}
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

    public CommandMode command;
    
    [SerializeField] Character character;

    public float action_time;

    public Vector2 move_pos;

    // Move Command
    Func<Vector2> GetMoveDir; // follow by default

    public BehaviorController(Character c)
    {
        character = c;
        GetMoveDir = FollowCommand;
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

        character.SetMove(GetMoveDir());
    }

    public void SetCommand(CommandMode command)
    {
        Debug.Log(character.name + " will " + command);
        switch (command)
        {
            case CommandMode.Follow:
                GetMoveDir = FollowCommand;
                break;
            case CommandMode.Disperse:
                GetMoveDir = DisperseCommand;
                break;
            case CommandMode.Engage:
                GetMoveDir = EngageCommand;
                break;
            case CommandMode.Hold:
                GetMoveDir = HoldCommand;
                break;
        }
    }
    private Vector2 FollowCommand()
    {
        return Vector2.right;
    }
    private Vector2 DisperseCommand()
    {
        return Vector2.up;
    }
    private Vector2 EngageCommand()
    {
        return Vector2.down;
    }
    private Vector2 HoldCommand()
    {
        return Vector2.left;
    }
}
