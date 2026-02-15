using System;
using UnityEngine;

// handles maintaining and switching between the different player views
public enum PlayerViewEnum {Primary, Order, Command}
[Serializable]
public class PlayerViewController
{
    [Header("Look Data")]
    private Vector2 viewport_pos;
    private Vector2 look_pos;
    // used to save/hold a look position
    
    public Vector2 hold_viewport_pos;
    private Vector2 hold_look_pos;

    private Vector2 player_pos;

    [Header("Player View Type")]
    public PlayerViewEnum curr_view_enum = PlayerViewEnum.Primary;
    PlayerViewEnum prev_view_enum = PlayerViewEnum.Primary;
    Action ViewAction;
    Action LookPosUpdateAction;

    [Header("Camera")]
    private Camera main_cam;

    public PlayerViewController(Camera main_cam)
    {
        this.main_cam = main_cam;
        SetViewType(curr_view_enum);
    }

    public Vector2 UpdateView(Vector2 viewport_pos)
    {
        this.viewport_pos = viewport_pos;
        //this.player_pos = player_pos;
        LookPosUpdateAction();

        return look_pos;
    }
    public void UpdateLookPos()
    {
        ViewAction();
    }
    
    #region View Pos Functions
    public void SetViewType(PlayerViewEnum view_type) // order view can override primary or command, but primary and command can only override each other
    {
        if (curr_view_enum == PlayerViewEnum.Order)
        {
            return;
        } 
        switch(view_type)
        {
            case PlayerViewEnum.Primary:
                ViewAction = PrimaryView;
                LookPosUpdateAction = LookRawPos;
                break;
            case PlayerViewEnum.Order:
                hold_viewport_pos = viewport_pos;
                hold_look_pos = look_pos;
                LookPosUpdateAction = LookHoldPos;
                ViewAction = OrderView;
                break;
            case PlayerViewEnum.Command:
                LookPosUpdateAction = LookRawPos;
                ViewAction = CommandView;
                break;
        }
        
        prev_view_enum = curr_view_enum;
        curr_view_enum = view_type;
        Debug.Log(view_type);
    }
    public void ResetViewType() // order resets to previous state. primary and command reset to each other
    {
        
        switch(curr_view_enum)
        {
            case PlayerViewEnum.Primary:
                curr_view_enum = PlayerViewEnum.Command;
                break;
            case PlayerViewEnum.Order:
                viewport_pos = hold_viewport_pos;
                curr_view_enum = prev_view_enum;
                break;
            case PlayerViewEnum.Command:
                curr_view_enum = PlayerViewEnum.Primary;
                break;
        }
        prev_view_enum = curr_view_enum;
        SetViewType(curr_view_enum);
    }

    void PrimaryView() // follow where the player's looking
    {
    }
    void OrderView() // lock camera while player is using an action wheel to order an operator
    {
    }
    void CommandView() // camera directly on player
    {
    }

    #endregion

    #region Look Actions
    public void LookRawPos() // look at viewport_pos
    {
        look_pos = (Vector2)main_cam.ViewportToWorldPoint(viewport_pos);
    }

    public void LookHoldPos()
    {
        look_pos = (Vector2)main_cam.ViewportToWorldPoint(hold_viewport_pos);
    }

    #endregion
}