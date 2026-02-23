using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using NUnit.Framework.Constraints;
using UnityEngine;

public class CaptureArea : MonoBehaviour
{
    [Header("id")]
    private MajorObjective objective_poi;
    [Header("VFX")]
    public Transform capture_point_mask;
    public SpriteRenderer[] color_components;
    public Animator objective_animator;

    [Header("Capture Data")]
    private float curr_capture_time = 0;
    private float max_capture_time = 1f;
    private int owner = -1;
    [SerializeField] private int squad_weight = 0;
    [SerializeField] private int enemy_weight = 0;
    public void Setup(MajorObjective poi, bool is_captured)
    {
        objective_poi = poi;
        curr_capture_time = max_capture_time * (is_captured ? 1f : 0f);
    }

    public void SetColor(Color color)
    {
        foreach(SpriteRenderer color_component in color_components)
        {
            color.a = color_component.color.a;
            color_component.color = color;
        }
    }

    public void AddOccupier(Character character)
    {
        if (character.faction_tag == GameOverseer.SQUAD_TAG) {squad_weight++;}
        else {enemy_weight++;}
    }

    public void RemoveOccupier(Character character)
    {
        if (character.faction_tag == GameOverseer.SQUAD_TAG) {squad_weight--;}
        else {enemy_weight--;}
    }



    public void Update()
    {
        if (squad_weight != enemy_weight)
        {
            GrowObjective();
        } 
        
    }

    // called to change the objective's capture progress and the relevant VFX
    private  void GrowObjective()
    {
        float capture_delta = Time.deltaTime * (squad_weight > enemy_weight ? 1 : -1);        
        curr_capture_time += capture_delta;
        curr_capture_time = Mathf.Clamp(curr_capture_time, 0, max_capture_time);
        SetObjectiveState();
    }

    // set VFX and variables based on the current capture time
    private void SetObjectiveState()
    {
        float scale = (curr_capture_time / max_capture_time) * 0.875f;
        capture_point_mask.localScale = new Vector3(scale, scale, 1);
        
        // assign victor based on point captured
        if (owner != -1)
        {
            if (curr_capture_time <= 0)
            {
                owner = -1;
                objective_animator.Play("Lost");
                GameOverseer.ObjectiveLost(objective_poi);
                curr_capture_time = 0;
                SetColor(GameOverseer.EMPTY_COLOR);
            }
        }
        else if (curr_capture_time >= max_capture_time && owner == -1)
        {
            SetColor(GameOverseer.SQUAD_COLOR);
            objective_animator.Play("Captured");
            owner = GameOverseer.SQUAD_TAG;
            GameOverseer.ObjectiveCaptured(objective_poi);
        } 
    }
}