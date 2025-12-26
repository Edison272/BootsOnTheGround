using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Entities/Operator", order = 1)]
public class CharacterSO : ScriptableObject
{
    //id
    public string name = "Entity";

    //stats
    public int base_health;
    public int base_speed;
    public int base_range;

    public GameObject char_prefab;

    public LayerMask detection_mask;

    public int max_space = 5;
    

    public ItemSO[] inventory;
    public Vector2Int[] item_indexes;

    public Character GenerateOp()
    {
        GameObject op_object = MonoBehaviour.Instantiate(char_prefab);
        Character new_op = op_object.GetComponent<Character>();
        new_op.AssignBaseData(this);

        return new_op;
    }
}
