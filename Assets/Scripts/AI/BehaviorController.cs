using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
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
[System.Serializable]
public class BehaviorController
{
    private Dictionary<string, BehaviorModule> movement_behaviors; 
    private Dictionary<string, BehaviorModule> action_behaviors; 

    public CommandMode command;
    
    [SerializeField] Character character;
    [SerializeField] Character leader; // who they follow/ base their strategies around

    [Header("Actions")]
    float aggro_time = 1; // do an attack or something
    float rest_time = 0f; // don't attack
    float curr_time; // time buffer
    bool is_acting = true;

    [Header("Positioning")]
    public float base_engage_dist = 6f;
    public Vector2 anchor_position; // the general point which the operator hovers around
    public Vector2 move_to_pos; // the resulting position the bot aims to move to

    // Move Command
    Action MovementType;

    public BehaviorController(Character c)
    {
        character = c;
        SetCommand(CommandMode.Hold);
        anchor_position = c.GetPosition();
    }
    public void SetLeader(Character new_leader)
    {
        leader = new_leader;
    }

    public void UpdateAI()
    {
        Character targ = GameOverseer.THE_OVERSEER.GetTargetCharacter(character.is_player_squad, character, character.curr_range);
        if (targ && character.target != targ)
        {
            character.target = targ;
        }

        if (curr_time > 0)
        {
            curr_time -= Time.fixedDeltaTime;
        } else
        {
            curr_time = 0;
            is_acting = !is_acting;
            if (!is_acting)
            {
                character.StopMainItem();
            }
            
            curr_time += is_acting ? aggro_time : rest_time;
        }

        if (character.target && is_acting)
        {
            if ((character.GetPosition() - character.target.GetPosition()).sqrMagnitude <= character.curr_range * character.curr_range)
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
            } 
            else
            {
                character.target = null;
            }
        } 
        else
        {
            character.Look(character.GetPosition() + character.last_move_dir);
            Debug.DrawLine(character.GetPosition(), character.GetPosition() + character.aim_dir * character.curr_range, Color.black);
        }
        
        MovementType();
    }

    #region Commands
    public void SetCommand(CommandMode command)
    {
        //Debug.Log(character.name + " will " + command);
        this.command = command;
        switch (command)
        {
            case CommandMode.Follow:
                MovementType = FollowCommand;
                break;
            case CommandMode.Disperse:
                MovementType = DisperseCommand;
                break;
            case CommandMode.Engage:
                MovementType = EngageCommand;
                break;
            case CommandMode.Hold:
                MovementType = HoldCommand;
                break;
        }
    }
    private void FollowCommand()
    {
        anchor_position = (character.GetPosition() - leader.GetPosition()).normalized * 2f + leader.GetPosition();

        Vector2 obj_pos = anchor_position + leader.move_dir * 3;
        if (character.target)
        {
            obj_pos = character.target.GetPosition();

        }
        Vector2 move_pos = (obj_pos - anchor_position).normalized * Mathf.Clamp((anchor_position - obj_pos).magnitude, 2, base_engage_dist);

        Debug.DrawLine(character.GetPosition(), move_pos);
        character.SetMovePos(anchor_position + move_pos);
    }
    private void DisperseCommand()
    {
        Vector2 move_dir = Vector2.zero;
    }
    private void EngageCommand()
    {
        Vector2 move_dir = Vector2.zero;
        if (character.target && (character.target.GetPosition() - character.GetPosition()).sqrMagnitude >= character.curr_range * character.curr_range)
        {
            move_dir = (character.target.GetPosition() - character.GetPosition()).normalized;
        } else
        {
            move_dir = (anchor_position - character.GetPosition()).normalized;
        }
        
        character.SetMove(move_dir);
    }
    private void HoldCommand()
    {
        Vector2 dir = (anchor_position - character.GetPosition()).normalized;
        Vector2 projection_pos = character.GetPosition() + dir * (character.hitbox_radius + 0.05f);
        RaycastHit2D contact = Physics2D.Linecast(projection_pos, anchor_position, GameOverseer.avoid_map_mask);
        if (contact) // go in opposite direction if theres something in the way
        {
            Vector2 net_dir = (character.GetPosition() - contact.point).normalized;
            Debug.DrawLine(projection_pos, contact.point, Color.white);
            character.SetMove(net_dir + ObstacleAvoidanceVector(0.1f));
        } else
        {
            character.SetMovePos(anchor_position);
        }
        
    }
    #endregion
    #region Helper Vector Weight Functions
    private Vector2 ObstacleAvoidanceVector(float avoidance_range)
    {
        Vector2 net_dir = Vector2.zero;
        foreach(Vector2 dir in Directions2D.eight_directions)
        {   
            Vector2 projection_pos = character.GetPosition() + dir * (character.hitbox_radius + 0.05f);
            Vector2 check_pos = character.GetPosition() + dir * (character.hitbox_radius + avoidance_range);
            RaycastHit2D contact = Physics2D.Linecast(projection_pos, check_pos, GameOverseer.avoid_map_mask);
            if (contact) // go in opposite direction if theres something in the way
            {
                Vector2 opp_vector = (character.GetPosition() - contact.point).normalized;
                net_dir += opp_vector;
                Debug.DrawLine(projection_pos, contact.point, Color.white);
                //Debug.DrawLine(character.GetPosition(), net_dir, Color.red);
            } else
            {
                Debug.DrawLine(projection_pos, check_pos, Color.white);
            }
        }
        return net_dir;
    }
    #endregion
    #region Data Modification Functions
    public void SetActionTime(float a_time, float r_time, bool set_acting = false)
    {
        aggro_time = a_time;
        rest_time = r_time;
        is_acting = set_acting;
    }
    #endregion
}
