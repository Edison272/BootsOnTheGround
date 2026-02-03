using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Rendering;
using UnityEngine;

public class Operator : Character
{
    [Header("ID")]
    private OperatorSO base_op_data;
    public OpClass op_class => this.base_op_data.op_class;

    [Header("Ability")]
    private float ability_cd;

    public bool is_deployed = false;

    public void AssignBaseOpData(OperatorSO base_op_data)
    {
        this.base_op_data = base_op_data;
        AssignBaseData(base_op_data);
    }

    public void ToggleOp(bool isActive)
    {
        vfx_body.SetActive(isActive);
        entity_rb.simulated = isActive;
        main_body.GetComponent<SpriteRenderer>().enabled = isActive;
        is_deployed = isActive;
    }

    public void Deploy()
    {
        behavior_controller.anchor_position = GetPosition();
        GetReady();
    }

    public void Retreat()
    {
    
    }

    public void UseAbility(Vector2 target_area)
    {
        Debug.Log("I CAST FIREBALL");
    }

    public override bool IsInAction()
    {
        return is_alive && is_deployed;
    }
}