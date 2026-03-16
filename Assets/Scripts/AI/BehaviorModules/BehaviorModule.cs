using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviorModule
{
    protected Character character;
    protected BehaviorController behavior_controller;
    public BehaviorModule(Character character, BehaviorController behavior_controller)
    {
        this.character = character;
        this.behavior_controller = behavior_controller;
    }
    public abstract void StartModule();
    public abstract void UpdateModule();
}

#region Hold
public class HoldPositionBM : BehaviorModule
{
    float max_anchor_distance = 3;
    float estimated_travel_time;
    float curr_travel_time;
    public HoldPositionBM(Character character, BehaviorController behavior_controller) : base(character, behavior_controller)
    {
        
    }

    public override void StartModule()
    {
        GotoAnchor();

        // character couldn't reach path on time. something went wrong
        estimated_travel_time = character.GetTravelTime();
        curr_travel_time = 0;
    }

    public override void UpdateModule()
    {
        if (character.destination_reached)
        {
            if ((character.GetPosition() - behavior_controller.anchor_position).sqrMagnitude >= max_anchor_distance * max_anchor_distance)
            {
                GotoAnchor();
            }
        }
        else
        {
            curr_travel_time += Time.fixedDeltaTime;
            if (curr_travel_time >= estimated_travel_time)
            {
                character.StopMove();
                curr_travel_time = 0;

                // reset and try again
                StartModule();
            }
        }
    }

    void GotoAnchor()
    {
        Vector2 dir = (behavior_controller.anchor_position - character.GetPosition()).normalized;
        Vector2 projection_pos = character.GetPosition() + dir * (character.hitbox_radius + 0.05f);
        character.SetMovePos(behavior_controller.anchor_position);        
    }
}
#endregion

#region Follow
public class FollowLeaderBM : BehaviorModule
{
    Vector2 leader_position;
    public float follow_until_dist = 3.5f;
    public FollowLeaderBM(Character character, BehaviorController behavior_controller) : base(character, behavior_controller)
    {
        
    }

    public override void StartModule(){}

    public override void UpdateModule()
    {
        leader_position = behavior_controller.leader ? behavior_controller.leader.GetPosition() : character.GetPosition();
        // when far away from the leader, get within range of them
        if ((character.GetPosition() - leader_position).magnitude > follow_until_dist)
        {
            Vector2 dir_to_leader = (character.GetPosition() - behavior_controller.leader.GetPosition()).normalized;
            behavior_controller.anchor_position = dir_to_leader * follow_until_dist + behavior_controller.leader.GetPosition();
            character.SetMovePos(behavior_controller.anchor_position);
        }
        // otherwise, *slightly* get in range pursue targets, and be ready to move when the player does
        else if (behavior_controller.leader.move_dir != character.move_dir)
        {
            Vector2 move_dir = behavior_controller.leader.move_dir;
            if (move_dir != Vector2.zero){character.SetMove(behavior_controller.leader.move_dir);}
            else{character.StopMove();}
            
        }
    }
}
#endregion

#region Engage
public class EngageEnemyBM : BehaviorModule
{
    Vector2 target_position;
    public float follow_until_dist = 3.5f;
    public EngageEnemyBM(Character character, BehaviorController behavior_controller) : base(character, behavior_controller)
    {
        
    }

    public override void StartModule()
    {
        character.SetMovePos(behavior_controller.anchor_position); 
    }

    public override void UpdateModule()
    {
        if (character.target)
        {
            target_position = character.target.GetPosition();
            follow_until_dist = character.curr_range * 0.75f;
            if ((character.GetPosition() - target_position).magnitude > follow_until_dist)
            {
                Vector2 dir_to_target = (character.GetPosition() - target_position).normalized;
                behavior_controller.anchor_position = dir_to_target * follow_until_dist + target_position;
                character.SetMovePos(target_position);
            }
        }
        else if (!character.destination_reached)
        {
            character.SetMovePos(behavior_controller.anchor_position);   
        }
    }
}
#endregion

public class InteractBM : BehaviorModule
{
    bool interaction_complete = false;
    public InteractBM(Character character, BehaviorController behavior_controller) : base(character, behavior_controller)
    {
        
    }

    public override void StartModule()
    {
        GotoAnchor();
        interaction_complete = false;
    }

    public override void UpdateModule()
    {
        if (character.destination_reached && !interaction_complete)
        {
            IInteractable closest_interactable = character.FindInteractables();
            if (closest_interactable != null)
            {
                character.UseInteractable(closest_interactable);
                Debug.Log("Found Something!");
            }
            else
            {
                Debug.Log("Nothign here");
            }
            interaction_complete = true;
        }
    }
    void GotoAnchor()
    {
        Vector2 dir = (behavior_controller.anchor_position - character.GetPosition()).normalized;
        Vector2 projection_pos = character.GetPosition() + dir * (character.hitbox_radius + 0.05f);
        character.SetMovePos(behavior_controller.anchor_position);        
    }
}
