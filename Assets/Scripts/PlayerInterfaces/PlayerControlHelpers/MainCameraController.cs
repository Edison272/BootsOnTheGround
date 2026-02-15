using System;
using UnityEngine;
[System.Serializable]
public class MainCameraController
{
    [Header("Controlled Objects")]
    private Camera main_cam;
    private RectTransform player_screen;

    [Header("Camera Render Data")]
    [SerializeField] float camera_zoom_time = 0.5f;
    [SerializeField] float base_zoom_level = 1;
    [SerializeField] float zoom_factor = 0.5f;
    float lerp_amount = 1;
    float zoom_diff; // set current zoom
    float curr_zoom; // set current zoom
    [SerializeField]float target_zoom; // set current zoom
    [SerializeField] float player_range; // player look range (based on weapons and base stats)
    float curr_zoom_time;

    [Header("Camera Positioning")]
    Vector2 source_position;
    Vector2 target_position;

    Action CamMovement;

    #region Constructor
    public MainCameraController(Camera main_cam, RectTransform player_screen)
    {
        this.main_cam = main_cam;
        this.player_screen = player_screen;

        // initialize values
        target_zoom = 1;
        curr_zoom = (int)target_zoom;
        zoom_diff = 0;
        target_zoom = base_zoom_level;

        CamMovement = SetCameraBetweenPositions;
    }
    #endregion

    #region Updates
    // update the data for the camera every frame
    public void UpdateCamData(Vector2 source_position, Vector2 target_position)
    {
        this.source_position = source_position;
        this.target_position = target_position;
    }

    // every late update (called by player controller), use the data to update how the camera looks
    public void UpdateCamRender()
    {
        // adjust zoom if necessary
        if (curr_zoom_time < camera_zoom_time)
        {
            curr_zoom_time += Time.deltaTime;
            if (curr_zoom_time >= camera_zoom_time)
            {
                curr_zoom_time = camera_zoom_time;
                player_screen.localScale = new Vector3(target_zoom, target_zoom, 0);
            }
        }
        
        if (curr_zoom_time > 0)
        {
            float scale_Val = curr_zoom + zoom_diff * curr_zoom_time/camera_zoom_time; 
            player_screen.localScale = new Vector3(scale_Val, scale_Val, 0);
        }
        // adjust position with SetCameraBetweenPositions()
        CamMovement();
    }
    #endregion

    #region Camera Modifiers  
    public void SetCameraZoom(int zoom_scalar)
    {
        player_range = zoom_scalar;
        curr_zoom = player_screen.localScale.x;
        curr_zoom_time = 0;
        target_zoom = base_zoom_level + (5 - zoom_scalar) * zoom_factor;
        zoom_diff = target_zoom - player_screen.localScale.x;
    }

    public void SetCameraBetweenPositions()
    {
        // update cam position to move to look position within circular bounds
        float cam_range = 5 + 1.5f * player_range;
        Vector2 offset = (target_position - source_position) * 0.5f;
        if (offset.magnitude > cam_range)
        {
            offset = offset.normalized * cam_range;
        }

        Vector3 cam_pos = source_position + offset;
        cam_pos.z = -10;

        main_cam.transform.position = Vector3.Lerp(main_cam.transform.position, cam_pos, 0.5f);
    }

    public void SetCameraAtPosition()
    {

        Vector3 cam_pos = source_position;
        cam_pos.z = -10;
        main_cam.transform.position = cam_pos;
    }
    #endregion
}