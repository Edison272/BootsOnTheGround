using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Rendering;
using UnityEngine;

public class Operator : Character
{
    [Header("ID")]
    private OperatorSO base_data;
    public OpClass op_class => base_data.op_class;

    [Header("Ability")]
    private float ability_cd;

    public void AssignBaseOpData()
    {
        
    }

    public void UseAbility()
    {
        
    }
}