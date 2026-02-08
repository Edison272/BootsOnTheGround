using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public enum CaptureState {Squad, Enemy, Contested}
public class Objective : MonoBehaviour
{
    [Header("id")]
    private MajorPOI objective_poi;
    [Header("VFX")]
    public Transform capture_point_mask;
    public SpriteRenderer[] color_components;
    public void Setup(MajorPOI poi)
    {
        objective_poi = poi;
        SetCompletion(1);
    }

    public void SetCompletion(float progress)
    {
        Debug.Log(GameOverseer.squad_color);
        Color enemy_color = GameOverseer.enemy_color;
        
        foreach(SpriteRenderer color_component in color_components)
        {
            enemy_color.a = color_component.color.a;
            color_component.color = enemy_color;
        }
    }

    public void PointCaptured()
    {
        Color squad_color = GameOverseer.squad_color;
        foreach(SpriteRenderer color_component in color_components)
        {
            squad_color.a = color_component.color.a;
            color_component.color = squad_color;
        }
    }
}