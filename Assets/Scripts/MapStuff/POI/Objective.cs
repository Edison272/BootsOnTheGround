using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using NUnit.Framework.Constraints;
using UnityEngine;

public enum CaptureState {Squad, Enemy, Contested}
public class Objective : MonoBehaviour
{
    [Header("id")]
    private MajorPOI objective_poi;
    [Header("VFX")]
    public Transform capture_point_mask;
    public SpriteRenderer[] color_components;
    public Animator objective_animator;

    [Header("Capture Data")]
    private float curr_capture_time = 0;
    private float max_capture_time = 5f;
    private int owner = -1;
    [SerializeField] private int squad_weight = 0;
    [SerializeField] private int enemy_weight = 0;
    public void Setup(MajorPOI poi)
    {
        objective_poi = poi;
        float scale = 0f;
        capture_point_mask.localScale = new Vector3(scale, scale, 1);
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
        if (character.faction_tag == GameOverseer.squad_tag) {squad_weight++;}
        else {enemy_weight++;}
    }

    public void RemoveOccupier(Character character)
    {
        if (character.faction_tag == GameOverseer.squad_tag) {squad_weight--;}
        else {enemy_weight--;}
    }

    public void Update()
    {
        if (squad_weight != enemy_weight)
        {
            GrowObjective();
        } 
        
    }

    public void GrowObjective()
    {
        float capture_delta = Time.deltaTime * (squad_weight > enemy_weight ? 1 : -1);        
        curr_capture_time += capture_delta;
        curr_capture_time = Mathf.Clamp(curr_capture_time, 0, max_capture_time);

        float scale = (curr_capture_time / max_capture_time) * 0.875f;
        capture_point_mask.localScale = new Vector3(scale, scale, 1);
        
        // assign victor based on point captured
        if (owner != -1)
        {
            if (curr_capture_time <= 0)
            {
                owner = -1;
                objective_animator.Play("Lost");
                curr_capture_time = 0;
                SetColor(GameOverseer.empty_color);
            }
        }
        else if (curr_capture_time >= max_capture_time && owner == -1)
        {
            SetColor(GameOverseer.squad_color);
            objective_animator.Play("Captured");
            owner = GameOverseer.squad_tag;
        } 
    }
}