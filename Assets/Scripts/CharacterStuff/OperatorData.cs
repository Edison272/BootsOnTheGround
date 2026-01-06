// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// [System.Serializable]
// public class OperatorData : MonoBehaviour, IMovement, IHealthMod, IBodyVFX
// {
//     //id
//     public string callsign;
//     public OperatorSO base_data;
//     public bool ctrl_mode_on = false;
    
//     [field: Header("IHealthMod")]
//     [field: SerializeField] public int max_health {get; set;} //how many hits a operator can take
//     [field: SerializeField] public int health {get; set;} //how many hits a operator can take
//     public int armor; //affects how much damage an operator resists
//     public int range; //detect nearby enemies, operators, and items

//     public float throw_charge; //throwing speed 

//     //Action Data - attach these to the respective controls of different items
//     public delegate void OpAction();
//     public OpAction UseMain;
//     public OpAction UseOff;
//     public OpAction ResetMain;
//     public OpAction ResetOff;


//     [field: Header("INVENTORY - data")]
//     private int space = 0;
//     private int curr_index = 0;
//     public List<Item> equipment = new List<Item>();
//     public Vector2Int[] item_slots = new Vector2Int[2];
//     public float pickup_range = 10;

//     [field: Header("INVENTORY - objects")]
//     public Item main_item = null;
//     public Item off_item = null;

//     //vfx
//     [field: Header("VFX")]
//     [field: SerializeField] public float size {get; set;}
//     [field: SerializeField] public GameObject main_body {get; set;} //basically the hitbox
//     [field: SerializeField] public GameObject vfx_body {get; set;} //the vfx body
//     [field: SerializeField] public Animator anim {get; set;}
//     [field: SerializeField] public Transform front {get; set;}
//     [field: SerializeField] public Transform back {get; set;}
//     [field: SerializeField] public Transform main_hand {get; set;} //always set to main hand object
//     [field: SerializeField] public Transform off_hand {get; set;} //always set to off hand object

//     //target & handling
//     public Vector2 target_pos = Vector2.zero; //where bro is looking
//     private Vector2 target_dir = new Vector2(1, 0);
//     public float target_angle = 0;
//     public float handle_speed = 20;

//     public OperatorData suspect_target = null;

//     //action
//     public float focus_time = 0;


//     //deflection
//     public bool deflecting = false;
//     public int deflection_range = 0;

//     [field: Header("IMovement")]
//     [field: SerializeField] public float speed {get; set;} //how fast an operator can move
//     [field: SerializeField] public int weight {get; set;} //affects how much an operator is knocked back
//     [field: SerializeField] public Vector2 velocity {get; set;}
//     public Vector2 move_post;
//     public Rigidbody2D entity_rb;
    

//     public void Damage(int dmg) {
//         health -= dmg;
//         if(health <= 0) {
//             Destroy(this.gameObject);
//         }
//     }
//     public void Heal(int heal) {
//         health += heal;
//     }

//     void OnTriggerEnter2D (Collider2D thing) {
//         if(EntitySystem.PM.projectiles.ContainsKey(thing.gameObject)) {
//             Projectile projectile = EntitySystem.PM.projectiles[thing.gameObject];
//             if(!deflecting) {
//                 projectile.OnHit(this.gameObject, this, this);
//             } else {
//                 // Deflecting projectiles based on range
//                 Vector2 deflect_dir = AimScript.GetDir(projectile.rb.position,entity_rb.position);
//                 float incoming_angle = Mathf.Atan2(deflect_dir.x, deflect_dir.y) * Mathf.Rad2Deg;
//                 //Debug.Log(target_angle+", "+incoming_angle);
//                 if(Mathf.Abs(target_angle-incoming_angle) <= deflection_range/2) {
//                     projectile.ChangeTarget(entity_rb.position+deflect_dir*Random.Range(40,50)/10f);
//                 }
//             }
//         } 
//     }

//     public void Aim(Vector2 aim_pos) {
//         //aim_pos -= (Vector2)vfx_body.transform.localPosition;
//         AimScript.FaceDir(aim_pos, main_body.transform, vfx_body.transform);
//         Vector2 aim_dir = AimScript.GetDir(entity_rb.position, aim_pos);
//         target_angle = Mathf.Atan2(aim_dir.y, aim_dir.x) * Mathf.Rad2Deg+180;
//         float aim_dist = Vector2.Distance(entity_rb.position, aim_pos);

//         main_hand.transform.rotation = Quaternion.Slerp(main_hand.transform.rotation, Quaternion.Euler(0, 0, target_angle), handle_speed * Time.deltaTime);
//         if(off_item != null) {
//             off_hand.transform.rotation = Quaternion.Slerp(off_hand.transform.rotation, Quaternion.Euler(0, 0, target_angle), handle_speed * Time.deltaTime);
//         }

//         target_pos.x = entity_rb.position.x + (-aim_dist*Mathf.Cos((main_hand.transform.rotation.eulerAngles.z-180)*Mathf.Deg2Rad));
//         target_pos.y = entity_rb.position.y + (-aim_dist*Mathf.Sin((main_hand.transform.rotation.eulerAngles.z-180)*Mathf.Deg2Rad));
//         target_dir = AimScript.GetDir(entity_rb.position, target_pos);

//         if(target_pos.y > entity_rb.position.y+1) {
//             anim.SetBool("Facing Front", false);
//             front.SetSiblingIndex(0);
//             vfx_body.transform.SetSiblingIndex(1);
//             back.SetSiblingIndex(2);
//         } else {
//             anim.SetBool("Facing Front", true);
//             front.SetSiblingIndex(2);
//             vfx_body.transform.SetSiblingIndex(1);
//             back.SetSiblingIndex(0);
//         }

//         if(main_item != null) {
//             main_item.Aim(target_pos);
//             if(main_hand.localScale.y != vfx_body.transform.localScale.x) {
//                 main_hand.localScale = new Vector3(1f, vfx_body.transform.localScale.x, 1f);
//             }

//             //off item only exists if a main item does
//             if(off_item != null) {
//                 off_item.Aim(target_pos);
//                 if(off_hand.localScale.y != vfx_body.transform.localScale.x) {
//                     off_hand.localScale = new Vector3(1f, vfx_body.transform.localScale.x, 1f);
//                     //swap places with other hand when flipping
//                     Vector3 temp = main_hand.localPosition;
//                     main_hand.localPosition = off_hand.localPosition;
//                     off_hand.localPosition = temp;
//                 }
//             }
//         }
//     }
    
//     public void Move(Vector2 move_dir) {
//         if(move_dir != Vector2.zero) {
//             anim.SetBool("Moving", true);
//         } else {
//             anim.SetBool("Moving", false);
//         }
//         entity_rb.MovePosition(entity_rb.position + velocity + move_dir*Time.deltaTime*Mathf.Max(0, speed));
//     }

//     public void Knockback(Vector2 knock_dir, float duration = 0.1f) {
//         velocity += knock_dir;
//         KnockbackStats knock_stats = new KnockbackStats();
//         knock_stats.mover = this;
//         knock_stats.direction = knock_dir;
//         knock_stats.duration = duration;
//         EntitySystem.SEM.knocked_back.Add(knock_stats);
//     }

//     public void SpeedMod(float amount, float duration) {
//         speed += amount;
//         MoveModStat speed_stat = new MoveModStat();
//         speed_stat.mover = this;
//         speed_stat.speed_change = amount;
//         speed_stat.duration = duration;
//         EntitySystem.SEM.move_modded.Add(speed_stat);
//     }

//     public void ToggleHolster(Transform item, bool set_front) { //function moves item from the front of the operator to the back. when front 
//         if(set_front) {
//             if(off_item != null && off_item.gameObject.transform == item) { // if the item is an off item, set it to the off hand instead
//                 item.SetParent(off_hand, false);
//             } else {
//                 item.SetParent(main_hand, false);
//             }
//             item.localScale = new Vector3(1,1,1);
//         } else {
//             item.SetParent(back, false);
//         }
//     }

//     public void ToggleAkimboHands(bool akimbo_on) { //changes the position of the hands on the operators
//         if(akimbo_on) {
//             main_hand.transform.localPosition = new Vector3(-size*vfx_body.transform.localScale.x*0.5f, 0, 0);
//             off_hand.transform.localPosition = new Vector3(size*vfx_body.transform.localScale.x*0.5f, 0, 0);
//         } else {
//             main_hand.transform.localPosition = new Vector3(0, 0, 0);
//             off_hand.transform.localPosition = new Vector3(0, 0, 0);
//             main_hand.transform.localScale = new Vector3(1, 1, 1);
//             off_hand.transform.localScale = new Vector3(1, 1, 1);
//         }
//     }

//     public void EquipItemSlot(Vector2Int slot) {
//         // If the off item exists, equip the off item
//         if (slot.y != -1) {
//             off_item = equipment[slot.y];
//             UseOff = off_item.Use;
//             ResetOff = off_item.Reset;
//             off_item.gameObject.transform.parent = off_hand;
//             ToggleAkimboHands(true); //reposition the hands
//             off_item.Equip();
//         }
//         // Equip the main item
//         main_item = equipment[slot.x];
//         UseMain = main_item.Use;
//         ResetMain = main_item.Reset;
//         main_item.Equip();

//         // set new handling and range data
//         handle_speed = main_item.handling.handle_speed;
//         range = main_item.handling.range_mod;

//         //reset where operator is looking
//         target_pos = entity_rb.position;
//     }

//     public IEnumerator SwitchItems() {
//         range = 1;
//         if(off_item != null) {
//             ToggleAkimboHands(false);
//             off_item.Unequip();
//             off_item = null;
//         }
//         //reset main after off item just in case we need to turn off the akimbo hands
//         main_item.Unequip();
//         main_item = null;

//         if(curr_index == item_slots.Length-1) {
//             curr_index = 0;
//         } else {curr_index++;}
        
//         main_item = equipment[item_slots[curr_index].x];
//         if (item_slots[curr_index].y != -1) {
//             off_item = equipment[item_slots[curr_index].y];
//         }
//         yield return new WaitForSeconds(0.5f);
//         EquipItemSlot(item_slots[curr_index]);
//     }
    
//     public void ChargeThrow() {
//         if(throw_charge < 3) {
//             throw_charge += Time.deltaTime;
//         }
//     }

//     public void ReloadItem() {
//         if(main_item != null) {
//             main_item.Refresh();
//             if(off_item != null) {    
//                 off_item.Refresh();
//             }
//         }
//     }


//     public void UnequipItem(Vector2 aim_pos) {
//         if(main_hand != null) {
//             main_hand.parent = null;
//             // Debug.Log("Unequipping " + main_item.base_data.item_name);
//             if(off_hand != null) {    
//                 //EquipMain(off_item);
//                 off_hand = null;
//                 off_item = null;
//             } else {
//                 main_hand = null;
//                 main_item = null;
//                 range = 1;
//             }
//         }

//         throw_charge = 0;
//     }

//     public void AddToEquipment(Item new_item) {
//         // if(space + new_item.weight <= base_data.max_space) {            
//         //     equipment.Add(new_item);
//         //     space += new_item.weight;
//         // }
//         equipment.Add(new_item);
//     }

//     public void PickupItem(Item new_item) {
//         range = 1;

//         // discard currently held weapons
//         equipment[item_slots[curr_index].x].Unequip();
//         if (item_slots[curr_index].y != -1) {
//             equipment[item_slots[curr_index].y].Unequip();
//         }
        
//         // reset position of the item to operator position
//         new_item.user = this;
//         new_item.animator.enabled = true;
//         new_item.gameObject.transform.parent = this.main_hand.transform;
//         new_item.gameObject.transform.localPosition = new Vector3(0,0,0);
//         new_item.gameObject.transform.localRotation = Quaternion.identity;

//         // add new item to equipment and equip it
        
//         equipment.Add(new_item);
//         this.item_slots[curr_index] = new Vector2Int(equipment.Count-1,-1);
//         EquipItemSlot(item_slots[curr_index]);
//     }

// }
