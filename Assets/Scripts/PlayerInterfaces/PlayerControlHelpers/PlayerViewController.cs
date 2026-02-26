using System;
using UnityEngine;

// handles maintaining and switching between the different player views
public enum PlayerViewEnum {Primary, Order, Command}
[Serializable]
public class PlayerViewController
{
    [Header("Look Data")]
    public Vector2 screen_pos {get; private set;}
    public Vector2 viewport_pos {get; private set;}
    public Vector2 look_pos {get; private set;}
    public Vector2 clamped_look_pos {get; private set;}
    private Vector2 source_pos;

    // used to save/hold a look position
    public Vector2 hold_screen_pos {get; private set;}
    public Vector2 hold_viewport_pos {get; private set;}
    public Vector2 last_pointer_delta {get; private set;}
    private float range_scalar;

    [Header("Player View Type")]
    public PlayerViewEnum curr_view_enum = PlayerViewEnum.Primary;
    PlayerViewEnum prev_view_enum = PlayerViewEnum.Primary;
    Action ViewAction;
    Action LookPosUpdateAction;

    [Header("Camera & Rect")]
    private Camera main_cam;
    private RectTransform player_screen;
    [Header("Classmates")]
    public MainCameraController main_camera_controller;    

    #region Setup & Reset
    public PlayerViewController(Camera main_cam, RectTransform player_screen)
    {
        this.main_cam = main_cam;
        this.player_screen = player_screen;
        SetViewType(curr_view_enum);
    }
    public void ResetView(Vector2 character_position)
    {
        
        screen_pos = new Vector2(0, 0);
        Rect rect = player_screen.rect;
        float view_x = (screen_pos.x - rect.xMin) / rect.width;
        float view_y = (screen_pos.y - rect.yMin) / rect.height;
        viewport_pos = new Vector3(view_x, view_y, 0);
        //this.player_pos = player_pos;
        LookPosUpdateAction();
        look_pos = character_position;
        last_pointer_delta = Vector2.zero;
    }
    public void SetRange(float range_scalar)
    {
        this.range_scalar = range_scalar;
    }
    public void SetMCController(MainCameraController mcc)
    {
        this.main_camera_controller = mcc;
    }
    #endregion

    public void UpdateView(Vector2 pointer_delta, Vector2 source_pos)
    {
        // source pos is a character position
        this.source_pos = source_pos;
        
        // add pointer delta to look position data
        last_pointer_delta = pointer_delta;
        screen_pos += last_pointer_delta;
        
        Rect rect = player_screen.rect;
        screen_pos = new Vector2(
            Mathf.Clamp(screen_pos.x, rect.xMin, rect.xMax),
            Mathf.Clamp(screen_pos.y, rect.yMin, rect.yMax)
        );
        // float lowest_dimension = rect.xMax;
        // if (lowest_dimension > rect.yMax)
        // {
        //     lowest_dimension = rect.yMax;
        // }
        //screen_pos = CircularClamp(screen_pos, rect.center, rect.xMax);
        // convert to viewport coordinates (anchored to render texture)
        float view_x = (screen_pos.x - rect.xMin) / rect.width;
        float view_y = (screen_pos.y - rect.yMin) / rect.height;
        viewport_pos = new Vector3(view_x, view_y, 0);
        //this.player_pos = player_pos;
        LookPosUpdateAction();
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
                hold_screen_pos = screen_pos;
                hold_viewport_pos = viewport_pos;
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
    }
    public void ResetViewType() // order resets to previous state. primary and command reset to each other
    {
        
        switch(curr_view_enum)
        {
            case PlayerViewEnum.Primary:
                curr_view_enum = PlayerViewEnum.Command;
                break;
            case PlayerViewEnum.Order:
                screen_pos = hold_screen_pos;
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
        clamped_look_pos = CircularClamp(look_pos, source_pos, 5 + 1.5f * range_scalar);
    }

    public void LookHoldPos()
    {
        look_pos = (Vector2)main_cam.ViewportToWorldPoint(hold_viewport_pos);
        clamped_look_pos = CircularClamp(look_pos, source_pos, 5 + 1.5f * range_scalar);
    }
    
    #endregion

    #region Helpers
    public Vector2 CircularClamp(Vector2 clamp_pos, Vector2 clamp_src, float range)
    {
        Vector2 look_vec = clamp_pos - clamp_src;
        if (look_vec.magnitude > range)
        {
            look_vec = look_vec.normalized * range;
        }
        return look_vec + clamp_src;
    }

    #endregion
}