using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Entities/Character", order = 1)]
public class CharacterSO : ScriptableObject
{
    //id
    public string character_name = "Entity";

    //stats
    public int health = 100;
    public float speed = 3;
    [Range(0.01f,5f)] public float accel_time = 0.5f;
    public float range = 4;
    public float close_range = 3; // entering close range forces character to retarget

    public GameObject char_prefab;

    public LayerMask detection_mask;

    public int max_space = 5;
    

    public ItemSO[] inventory;
    public Vector2Int[] item_indexes;

    public Character GenerateChar(Vector3 pos)
    {
        GameObject op_object = MonoBehaviour.Instantiate(char_prefab, pos, Quaternion.identity);
        Character new_op = op_object.GetComponent<Character>();
        new_op.AssignBaseData(this);
        new_op.GetReady();

        return new_op;
    }
}
