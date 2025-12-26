// using UnityEngine;
// using System.Collections.Generic;

// [CreateAssetMenu(fileName = "Operator", menuName = "ScriptableObjects/Entities/Operator", order = 1)]
// public class OperatorSO : ScriptableObject
// {
//     //id
//     public string callsign = "Entity";

//     //stats
//     public int base_health;
//     public int base_speed;
//     public int base_range;

//     public GameObject main_body;

//     public LayerMask detection_mask;

//     public int max_space = 5;
    

//     [SerializeField] private ItemSO[] equipment = new ItemSO[4];
//     [SerializeField] private Vector2Int[] item_slots = {new Vector2Int(0, -1), new Vector2Int(1, -1)};



//     public OperatorData FillOpStats(GameObject op) {
//         OperatorData new_op = op.GetComponent<OperatorData>();
//         new_op.main_body = op;
//         new_op.callsign = callsign;
//         new_op.base_data = this;
//         new_op.max_health = base_health;
//         new_op.health = base_health;
//         new_op.speed = base_speed;
//         new_op.range = base_range;
//         new_op.entity_rb = new_op.main_body.GetComponent<Rigidbody2D>();
//         new_op.vfx_body = new_op.main_body.transform.GetChild(0).gameObject;
//         new_op.size = new_op.main_body.GetComponent<CircleCollider2D>().radius*1.5f;

//         for (int i = 0; i < item_slots.Length; i++) {
//             new_op.item_slots[i] = item_slots[i];
//         }

//         //fill equipment with proper items
//         if(equipment.Length > 0) {
//             foreach(ItemSO item in equipment) {
//                 new_op.AddToEquipment(item.GenerateItem(new_op));
//             }
//             new_op.EquipItemSlot(item_slots[0]);
//             // if(new_op.equipment.Length > 1 && new_op.equipment[0].base_data.one_handed == true) {
//             //     if(new_op.equipment[1].base_data.one_handed == true) {
//             //         new_op.EquipOff(new_op.equipment[1]);
//             //     }
//             // }
//         }
//         return new_op;
//     }
// }
