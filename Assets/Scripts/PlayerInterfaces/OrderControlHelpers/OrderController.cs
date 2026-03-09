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
    float character_get_dist = 2f;
    float interactable_distance = 1.5f;

    [Header("UI components")]
    bool UI_is_active;
    [SerializeField] FollowPlayerZoneUI follow_player_ui;
    [SerializeField] HoldPositionUI hold_position_ui;
    [SerializeField] TargetSelectorUI target_selector_ui;

    [Header("Give Order")]
    [SerializeField] CommandMode current_command = CommandMode.Hold;
    public OrderController(PlayerController player_controller, SquadManager squad_manager)
    {
        this.player_controller = player_controller;
        this.squad_manager = squad_manager;
        operators = squad_manager.operators.ToList<Operator>();

        follow_player_ui = GameObject.Instantiate(Resources.Load<GameObject>("UI/FollowPlayerZone")).GetComponent<FollowPlayerZoneUI>();
        follow_player_ui.transform.localScale *= 1+character_get_dist;
        hold_position_ui = GameObject.Instantiate(Resources.Load<GameObject>("UI/HoldPositionLine")).GetComponent<HoldPositionUI>();
        target_selector_ui = GameObject.Instantiate(Resources.Load<GameObject>("UI/TargetSelectorUI")).GetComponent<TargetSelectorUI>();

        StopUI();
    }
    #region Update

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

            GetOperatorPrompt(GetOperatorAtPointer());
        }

        if (chosen_op)
        {
            GetOperatorPrompt(null);
            SetCommandMode();
            UpdateOrderUI();
        }
        else if (UI_is_active)
        {
            StopUI();
        }
    }

    public void FixedUpdateOrderControl()
    {
        
    }
    #endregion

    #region Core Functions
    public void SetCommandMode()
    {
        if ((look_pos - player_controller.active_character.GetPosition()).magnitude <= character_get_dist)
        {
            current_command = CommandMode.Follow;
        }
        else if (interactables_in_range.Count > 0)
        {
            current_command = CommandMode.Interact;
        }
        else
        {
            current_command = CommandMode.Hold;
        }
    }
    public void GiveOrder()
    {
        if (!chosen_op)
        {
            UI_is_active = true;
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
            if (get_op.is_alive && ((Vector2)get_op.transform.position - look_pos).magnitude <= character_get_dist && get_op != player_controller.active_character)
            {
                return get_op;
            }
        }
        return null;
    }

    void AssignOperatorAtPointer() // give an operator a command. Command differs based on what's at the location
    {
        switch(current_command)
        {
            case CommandMode.Follow:
                break;
            case CommandMode.Hold:
                chosen_op.op_behavior_controller.anchor_position = MapManager.GetWorldToTileSpaceCenter(player_controller.look_pos);
                break;
            case CommandMode.Interact:
                chosen_op.op_behavior_controller.anchor_position = player_controller.look_pos;
                break;
        }
        chosen_op.SetCommandBehavior(current_command);
        chosen_op = null;
    }

    #endregion

    #region Helper Commands

    public void ReturnToLeader()
    {
        foreach(Operator get_op in operators)
        {
            get_op.SetCommandBehavior(CommandMode.Follow);
        }
    }

    #endregion

    #region Order UI
    public void GetOperatorPrompt(Operator closest_op)
    {
        if (closest_op != null && closest_op.is_alive)
        {
            target_selector_ui.SetUIAlpha(1);
            target_selector_ui.SetPosition(closest_op.GetPosition());
            target_selector_ui.SetText("command this guy");

            follow_player_ui.transform.position = closest_op.GetPosition();
            float look_to_closest_dist = (look_pos - closest_op.GetPosition()).magnitude;
            float follow_ui_alpha = Mathf.Clamp01(((character_get_dist * 2) - look_to_closest_dist));
            follow_player_ui.SetUIAlpha(follow_ui_alpha * 0.5f);
        }
        else if (closest_op == null)
        {
            target_selector_ui.SetUIAlpha(0);
            follow_player_ui.SetUIAlpha(0);
        }
    }
    public void StopUI()
    {
        follow_player_ui.SetUIAlpha(0);
        player_controller.active_character.SetOutlineAlpha(0);
        hold_position_ui.SetUIAlpha(0);
        target_selector_ui.SetUIAlpha(0);
        UI_is_active = false;
    }

    public void UpdateOrderUI()
    {
        follow_player_ui.transform.position = player_controller.active_character.GetPosition();
        float look_to_player_dist = (look_pos - player_controller.active_character.GetPosition()).magnitude;
        float follow_ui_alpha = Mathf.Clamp01(((character_get_dist * 2) - look_to_player_dist)/character_get_dist);
        follow_player_ui.SetUIAlpha(follow_ui_alpha * 0.5f);
        player_controller.active_character.SetOutlineAlpha(follow_ui_alpha);


        hold_position_ui.SetPositions(chosen_op.GetPosition(), MapManager.GetWorldToTileSpaceCenter(player_controller.look_pos));
        float hold_ui_alpha = 0;
        if (follow_ui_alpha < 0.5)
        {
            hold_ui_alpha = 1 - follow_ui_alpha;
        }
        hold_ui_alpha *= 0.5f;
        hold_position_ui.SetUIAlpha(hold_ui_alpha);

        target_selector_ui.SetUIAlpha(1);
        switch(current_command)
        {
            case CommandMode.Follow:
                follow_player_ui.SetUIAlpha(1);
                target_selector_ui.SetPosition(player_controller.active_character.GetPosition());
                target_selector_ui.SetScale(player_controller.active_character.transform.localScale.x + 0.1f);
                target_selector_ui.SetText("follow this guy");
                break;
            case CommandMode.Hold:
                hold_position_ui.SetUIAlpha(1);
                target_selector_ui.SetPosition(MapManager.GetWorldToTileSpaceCenter(player_controller.look_pos));
                target_selector_ui.SetScale(1);
                target_selector_ui.SetText("hold this position");   
                break;
            case CommandMode.Interact:
                target_selector_ui.SetPosition(interactables_in_range[0].transform.position);
                target_selector_ui.SetScale(interactables_in_range[0].transform.localScale.x + 0.1f);
                target_selector_ui.SetText("pickup this thing");
                break;
        }
    }
    #endregion

}