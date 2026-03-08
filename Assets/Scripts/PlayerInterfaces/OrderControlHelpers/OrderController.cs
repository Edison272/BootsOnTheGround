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

    [Header("distances")]
    float follow_distance = 3f;
    float interactable_distance = 1.5f;

    [Header("UI components")]
    [SerializeField] FollowPlayerZoneUI follow_player_ui;
    [SerializeField] GameObject hold_position_ui;
    public OrderController(PlayerController player_controller, SquadManager squad_manager)
    {
        this.player_controller = player_controller;
        this.squad_manager = squad_manager;
        operators = squad_manager.operators.ToList<Operator>();

        follow_player_ui = GameObject.Instantiate(Resources.Load<GameObject>("UI/FollowPlayerZone")).GetComponent<FollowPlayerZoneUI>();
        follow_player_ui.transform.localScale *= follow_distance;
        follow_player_ui.gameObject.SetActive(false);
        hold_position_ui = GameObject.Instantiate(Resources.Load<GameObject>("UI/MovePointer"));
        hold_position_ui.SetActive(false);
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
            Physics2D.OverlapCircle(look_pos, interactable_distance, interactable_filter, interactables_in_range);

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

            Operator current_closest_op = GetOperatorAtPointer();
            // add command prompt

        }

        if (chosen_op)
        {
            if ((look_pos - player_controller.active_character.GetPosition()).sqrMagnitude <= Mathf.Pow(follow_distance * 2, 2))
            {
                follow_player_ui.transform.position = player_controller.active_character.GetPosition();
                follow_player_ui.gameObject.SetActive(true);
            }
            else if(follow_player_ui.gameObject.activeSelf)
            {
                follow_player_ui.gameObject.SetActive(false);
            }
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
            if (get_op.is_alive && ((Vector2)get_op.transform.position - look_pos).sqrMagnitude <= Mathf.Pow(follow_distance, 2) && get_op != player_controller.active_character)
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
            chosen_op.op_behavior_controller.anchor_position = player_controller.look_pos;
            chosen_op.SetCommandBehavior(CommandMode.Interact);
        }
        else if ((look_pos - player_controller.active_character.GetPosition()).sqrMagnitude <= Mathf.Pow(follow_distance, 2))
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