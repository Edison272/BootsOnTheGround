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
public class BehaviorController
{
    protected Dictionary<string, BehaviorModule> movement_behaviors; 
    protected Dictionary<string, BehaviorModule> action_behaviors; 

    protected Queue<Action> movement_queue = new Queue<Action>();
    protected float move_wait_time = 1;
    protected float curr_move_time = 0;
    protected float estimated_travel_time = 0; // time expected to be taken to complete a move
    protected float curr_travel_time = 0;
    protected int movement_priority = 1;
    protected Queue<Action> action_queue = new Queue<Action>();
    protected float action_wait_time = 0;
    protected float curr_action_time = 0;

    public CommandMode command;
    public TargetType favorite_target = TargetType.Closest;
    
    [SerializeField] protected Character character;
    [SerializeField] protected Character leader; // who they follow/ base their strategies around

    [Header("Actions")]
    protected float aggro_time = 1; // do an attack or something
    protected float rest_time = 0f; // don't attack
    protected float curr_time; // time buffer
    protected bool is_acting = true;

    [Header("Positioning")]
    public float base_engage_dist = 6f;
    public Vector2 anchor_position; // the general point which the operator hovers around
    public Vector2 move_to_pos; // the resulting position the bot aims to move to
    private float avoidance_range = 1;

    // Move Command
    protected Action MovementType;

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

    public virtual void UpdateAI()
    {   
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

        if (character.target)
        {
            // reset if target untrackable
            if (!character.target.IsInAction() || (character.GetPosition() - character.target.GetPosition()).sqrMagnitude > character.curr_range * character.curr_range)
            {
                character.target = null;
                return;
            }
            
            // only switch targets if the target is in close quarters range
            Character targ = GameOverseer.GetTargetCharacter(character.is_player_squad, character, character.curr_range, TargetType.Closest);
            float sqr_close_range = character.close_range * character.close_range;
            if ((character.GetPosition() - targ.GetPosition()).sqrMagnitude <= sqr_close_range)
            {
                character.target = targ;
            }

            // shoot target with clear Line of Sight
            RaycastHit2D contact = Physics2D.Linecast(character.GetPosition(), character.target.GetPosition(), GameOverseer.avoid_map_mask);
            if (!contact)
            {
                character.Look(character.target.GetPosition());
                character.UseMainItem();
                Debug.DrawLine(character.GetPosition(), contact.point, Color.white);
            } else
            {
                Debug.DrawLine(character.GetPosition(), contact.point, Color.grey);
            }
        } 
        else
        {
            Character targ = GameOverseer.GetTargetCharacter(character.is_player_squad, character, character.curr_range, favorite_target);
            character.Look(character.GetPosition() + character.last_move_dir);
            Debug.DrawLine(character.GetPosition(), character.GetPosition() + character.aim_dir * character.curr_range, Color.black);
            if (targ)
            {
                character.target = targ;
            }
        }
        
        //MovementType();

        // orders & context will add a bunch of stuff to the queue which the AI will handle 1 by 1
        if (character.destination_reached)
        {
            if (movement_queue.Count > 0)
            {
                Action move = movement_queue.Dequeue();
                move();
                estimated_travel_time = character.GetTravelTime();
                curr_travel_time = 0;
                Debug.Log("movement queue in use");
            }

            estimated_travel_time = 0;
            curr_travel_time = 0;
        } 
        else
        {
            // character couldn't reach path on time. something went wrong
            curr_travel_time += Time.fixedDeltaTime;
            if (curr_travel_time >= estimated_travel_time)
            {
                character.StopMove();
                Debug.Log("I'm lost...");
                movement_queue.Clear();
                curr_travel_time = 0;
            }
        }
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
        movement_queue.Clear();
        character.StopMove();
        MovementType();
        estimated_travel_time = character.GetTravelTime();
        curr_travel_time = 0;
    }
    protected virtual void FollowCommand()
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

        // constantly follow that player
        movement_queue.Enqueue(FollowCommand);
    }
    protected virtual void DisperseCommand()
    {
        Vector2 move_dir = Vector2.zero;
    }
    protected virtual void EngageCommand()
    {
        Vector2 move_dir = Vector2.zero;
        if (character.target)
        {
            anchor_position = character.target.GetPosition();
            
        }
        move_dir = (anchor_position - character.GetPosition()).normalized;
        character.SetMove(move_dir);
    }
    protected virtual void HoldCommand()
    {
        Vector2 dir = (anchor_position - character.GetPosition()).normalized;
        Vector2 projection_pos = character.GetPosition() + dir * (character.hitbox_radius + 0.05f);
        character.SetMovePos(anchor_position);
    }
    #endregion
    #region Helper Vector Weight Functions
    protected Vector2 ObstacleAvoidanceVector(Vector2 target_dir)
    {
        Vector2 net_dir = Vector2.zero;
        foreach(Vector2 dir in Directions2D.eight_directions)
        {   
            Vector2 normal_dir = dir.normalized;
            // project a detection raycast from edge of hitbox
            Vector2 projection_pos = character.GetPosition() + normal_dir * (character.hitbox_radius + 0.05f);
            Vector2 check_pos = character.GetPosition() + normal_dir * (character.hitbox_radius + avoidance_range);

            RaycastHit2D contact = Physics2D.Linecast(projection_pos, check_pos, GameOverseer.avoid_map_mask);
            float contact_weight = 1;
            float alignment_weight = Vector2.Dot(target_dir, (check_pos.normalized - projection_pos).normalized);;
            Debug.DrawLine(projection_pos, projection_pos + normal_dir, Color.gray);
            if (contact) // go in opposite direction if theres something in the way
            {
                contact_weight = -((contact.point - projection_pos).magnitude - avoidance_range);
                Debug.DrawLine(projection_pos, projection_pos + normal_dir * contact_weight * alignment_weight, Color.white);
            } else
            {
                Debug.DrawLine(projection_pos, projection_pos + normal_dir, Color.white);
            }
            
            net_dir += normal_dir * contact_weight * alignment_weight;

            
        }
        Debug.DrawLine(character.GetPosition(), character.GetPosition() + net_dir, Color.yellow);
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
