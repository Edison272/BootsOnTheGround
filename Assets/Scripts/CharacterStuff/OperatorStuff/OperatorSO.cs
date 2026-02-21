using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
public enum OpClass {Vanguard, Overwatch, Specialist, Responder, Length}
[CreateAssetMenu(fileName = "Operator", menuName = "ScriptableObjects/Entities/Operator", order = 1)]
public class OperatorSO : CharacterSO
{
    //id
    public OpClass op_class = OpClass.Vanguard;
    [Header("Deployment")]
    public int command_cost = 3;
    public float deployment_speed = 1f; // how long the player needs to wait before the operator is deployed to the field
    public float field_time = -1f; // how long the operator is on the field. -1 means the operator stays indefinitely
    [Header("Ability")]
    public AbilitySO ability;
    public Operator GenerateOp(Vector3 pos)
    {
        GameObject op_object = MonoBehaviour.Instantiate(char_prefab, pos, Quaternion.identity);
        Operator new_op = op_object.GetComponent<Operator>();
        new_op.AssignBaseOpData(this);

        return new_op;
    }
}
