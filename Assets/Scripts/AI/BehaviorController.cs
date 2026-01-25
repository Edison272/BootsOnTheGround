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
    [SerializeField] Character leader; // who they follow/ base their strategies around

    public float action_time;

    public Vector2 move_pos;

    // Move Command
    Func<Vector2> GetMoveDir; // follow by default

    public BehaviorController(Character c)
    {
        character = c;
        GetMoveDir = HoldCommand;
    }
    public void SetLeader(Character new_leader)
    {
        leader = new_leader;
    }

    public void UpdateAI()
    {
        Character targ = GameOverseer.THE_OVERSEER.GetTargetCharacter(character.is_player_squad, character);
        if (character.target != targ)
        {
            character.target = targ;
        }

        if (character.target)
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
        Vector2 move_dir = Vector2.zero;
        move_dir = (leader.GetPosition() - character.GetPosition()).normalized;
        return move_dir;
    }
    private Vector2 DisperseCommand()
    {
        Vector2 move_dir = Vector2.zero;
        return move_dir;
    }
    private Vector2 EngageCommand()
    {
        Vector2 move_dir = Vector2.zero;
        move_dir = (character.target.GetPosition() - character.GetPosition()).normalized;
        return move_dir;
    }
    private Vector2 HoldCommand()
    {
        character.StopMove();
        return Vector2.zero;
    }
}
