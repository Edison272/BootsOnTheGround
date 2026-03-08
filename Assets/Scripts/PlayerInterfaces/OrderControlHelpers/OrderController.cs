using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// handles setting the player target and determining critical hit
[Serializable]
public class OrderController
{
    PlayerController player_controller;
    Vector2 look_pos;
    public Operator chosen_op;  
    public List<Operator> operators;
    public List<Collider2D> interactables_in_range = new List<Collider2D>();
    SquadManager squad_manager;
    public OrderController(PlayerController player_controller, SquadManager squad_manager)
    {
        this.player_controller = player_controller;
        this.squad_manager = squad_manager;
        operators = squad_manager.operators.ToList<Operator>();
    }

    public void UpdateOrderControl(Vector2 look_pos)
    {
        this.look_pos = look_pos;
        if (chosen_op)
        {
            // get interactibles
            ContactFilter2D interactable_filter = new ContactFilter2D();
            interactable_filter.SetLayerMask(GameOverseer.find_interactable_mask);
            interactable_filter.useLayerMask = true; // Actively use the mask
            interactable_filter.useTriggers = true;
            Physics2D.OverlapCircle(look_pos, 1, interactable_filter, interactables_in_range);

            // sort interactables list
            interactables_in_range.Sort((a, b) =>
            {
                float da = ((Vector2)a.transform.position - look_pos).sqrMagnitude;
                float db = ((Vector2)b.transform.position - look_pos).sqrMagnitude;
                return da.CompareTo(db);
            });
        }
        else
        {
            // sort operator list
            operators.Sort((a, b) =>
            {
                float da = ((Vector2)a.transform.position - look_pos).sqrMagnitude;
                float db = ((Vector2)b.transform.position - look_pos).sqrMagnitude;
                return da.CompareTo(db);
            });            
        }
    }

    #region Core Functions
    public void GiveOrder()
    {
        if (!chosen_op)
        {
            chosen_op = GetOperatorAtPointer();
        }
        else
        {
            AssignOperatorAtPointer();
        }
    }
    Operator GetOperatorAtPointer() // select an operator at the pointer
    {
        foreach(Operator get_op in operators)
        {
            if (get_op.is_alive && ((Vector2)get_op.transform.position - look_pos).sqrMagnitude <= 9 && get_op != player_controller.active_character)
            {
                return get_op;
            }
        }
        return null;
    }

    void AssignOperatorAtPointer() // give an operator a command. Command differs based on what's at the location
    {
        if (interactables_in_range.Count > 0)
        {
            chosen_op.SetCommandBehavior(CommandMode.Interact);
        }
        else if ((look_pos - player_controller.active_character.GetPosition()).sqrMagnitude <= 9)
        {
            chosen_op.SetCommandBehavior(CommandMode.Follow);
        }
        else
        {
            chosen_op.op_behavior_controller.anchor_position = player_controller.look_pos;
            chosen_op.SetCommandBehavior(CommandMode.Hold);
        }
        

        chosen_op = null;
    }

    #endregion
}