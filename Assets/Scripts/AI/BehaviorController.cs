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

    public Vector2 move_to_pos;

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
        Character targ = GameOverseer.THE_OVERSEER.GetTargetCharacter(character.is_player_squad, character, character.curr_range);
        if (character.target != targ)
        {
            character.target = targ;
        }

        if (character.target && (character.GetPosition() - character.target.GetPosition()).sqrMagnitude <= character.curr_range * character.curr_range)
        {
            character.Look(character.target.GetPosition());
            RaycastHit2D contact = Physics2D.Linecast(character.GetPosition(), character.GetPosition() + character.aim_dir * character.curr_range, GameOverseer.avoid_map_mask);
            if (!contact)
            {
                character.UseMainItem();
                Debug.DrawLine(character.GetPosition(), contact.point, Color.white);
            } else
            {
                Debug.DrawLine(character.GetPosition(), contact.point, Color.grey);
            }
        } else
        {
            Debug.DrawLine(character.GetPosition(), character.GetPosition() + character.aim_dir * character.curr_range, Color.black);
        }
        
        character.SetMove(GetMoveDir() + 0.5f * ObstacleAvoidanceVector());
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

    private Vector2 ObstacleAvoidanceVector()
    {
        Vector2 net_dir = Vector2.zero;
        foreach(Vector2 dir in Directions2D.eight_directions)
        {   
            Vector2 projection_pos = character.GetPosition() + dir * (character.hitbox_radius + 0.05f);
            Vector2 check_pos = character.GetPosition() + dir * (character.hitbox_radius + character.close_range);
            RaycastHit2D contact = Physics2D.Linecast(projection_pos, check_pos, GameOverseer.avoid_map_mask);
            if (contact) // go in opposite direction if theres something in the way
            {
                net_dir += (character.GetPosition() - contact.point).normalized;
                Debug.DrawLine(projection_pos, contact.point, Color.white);
                //Debug.DrawLine(character.GetPosition(), net_dir, Color.red);
            } else
            {
                Debug.DrawLine(projection_pos, check_pos, Color.white);
            }
        }
        return net_dir;
    }
}
