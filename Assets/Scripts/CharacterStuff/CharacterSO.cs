using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Entities/Operator", order = 1)]
public class CharacterSO : ScriptableObject
{
    //id
    public string character_name = "Entity";

    //stats
    public int health = 100;
    public int speed = 3;
    public int range = 4;
    public int close_range = 3; // entering close range forces character to retarget

    public GameObject char_prefab;

    public LayerMask detection_mask;

    public int max_space = 5;
    

    public ItemSO[] inventory;
    public Vector2Int[] item_indexes;

    public Character GenerateOp(Vector3 pos)
    {
        GameObject op_object = MonoBehaviour.Instantiate(char_prefab, pos, Quaternion.identity);
        Character new_op = op_object.GetComponent<Character>();
        new_op.AssignBaseData(this);

        return new_op;
    }
}
