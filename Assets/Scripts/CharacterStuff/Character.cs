using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Character : MonoBehaviour, IHealth, IMovement
{
    [SerializeField] CharacterSO base_data;

    [field: Header("Body Parts")]
    public GameObject main_body;//basically the hitbox
    public GameObject vfx_body; //the vfx body
    public Transform front;
    public Transform back;
    public Transform body_sprite;
    public Transform main_hand; //always set to main hand object
    public Transform alt_hand; //always set to off hand object

    [field: Header("VFX")]
    public Animator anim;
    private Vector2 sprite_center; // center of mass of this srpite
    public float hitbox_radius {get; private set;}
    // character has 4 VFX states based on aim direciton, stored as two booleans, X and Y
    // T, F = Right Bottom | T, T = Right Top | F, T = Left Top | F, F = Left Bottom
    (bool, bool) direction_state = (true, true);
    (Vector2, Vector2) akimbo_hand_pos = (new Vector2 (-0.2f, 0.6f), new Vector2 (0.5f, 0.6f));  // (main pos (left), alt pos (right))
    Vector2 single_hand_pos = new Vector2 (0, 0.6f);  // (main pos, alt pos)

    //aim & handling
    [field: Header("Aiming")]
    Vector2 aim_dir = Vector2.zero; // vector from operator to where they are looking. MAKE SURE ITS UN-NORMALIZED
    private Action AimStyle; // single-item or akimbo aiming?
    public  float aim_angle = 0; // angle (deg) the character is looking in
    
    [field: Header("Movement")]
    public float curr_speed {get; private set;} = 1; //how fast an operator can move
    public float base_speed => base_data.speed;
    public Vector2 move_dir {get; private set;} = Vector2.zero;
    public Vector2 force_dir {get; private set;} = Vector2.zero;
    public Rigidbody2D entity_rb {get; private set;}
    public float force_move_time {get; private set;}

    [field: Header("Damage")]
    public int curr_health {get; private set;}
    public int max_health => base_data.health;

    [field: Header("Inventory")]
    public Item[] inventory;
    public Vector2Int[] item_indexes;  // Access items from the items list with indexes. Vector X for Main Item, Vector Y for Alt Item
    int curr_item_index = 0;           // access items indexes list
    public Item main_item;
    public Item alt_item;
    (int, int) current_indexes;
    float switch_cd = 0.5f; // time the char must wait before they can switch to the next weapon
    float curr_switch_cd = 0;

    [field: Header("Detection")]
    // [SerializeField] CircleCollider2D range_collider;
    // ContactPoint2D[] things_in_range;
    public Character target = null;
    public int curr_range {get; private set;}
    public int base_range => base_data.range;

    [field: Header("AI")]
    public bool is_player_squad = false;
    bool is_AI_active = false;
    BehaviorController behavior_controller;

    #region initalizers
    // get base data from a scriptable object and assign them here. Called once at when this object is created
    public void AssignBaseData(CharacterSO base_data)
    {
        this.base_data = base_data;

        // setup movement
        entity_rb = this.GetComponent<Rigidbody2D>();
        curr_speed = base_speed;

        // setup VFX
        hitbox_radius = GetComponent<CircleCollider2D>().radius;
        akimbo_hand_pos = ((Vector2) main_hand.localPosition, (Vector2) alt_hand.localPosition);
        single_hand_pos = new Vector2(0, main_hand.localPosition.y);

        // setup health
        curr_health = max_health;
        
        // setup inventory
        inventory = new Item[base_data.inventory.Length];
        item_indexes = new Vector2Int[base_data.item_indexes.Length];
        for (int i = 0; i < base_data.item_indexes.Length; i++)
        {
            int main_item = base_data.item_indexes[i].x;
            int alt_item = base_data.item_indexes[i].y;
            item_indexes[i] = base_data.item_indexes[i];
            inventory[main_item] = base_data.inventory[main_item].GenerateItem(main_hand);
            if (alt_item > -1)
            {
                inventory[alt_item] = base_data.inventory[alt_item].GenerateItem(alt_hand);
            }
        }

        GetReady(); // set up all necessary data ready for the operator
    }

    // make sure the operator's data is set up
    public void GetReady()
    {
        // set basic sibling order of entity VFX (operator faces BOTTOM RIGHT by default)
        anim.SetBool("FaceFront", true);
        main_hand.SetSiblingIndex(4);
        front.SetSiblingIndex(3);
        vfx_body.transform.SetSiblingIndex(2);
        back.SetSiblingIndex(1);
        alt_hand.SetSiblingIndex(0);
        
        // set initial active items
        foreach(Item item in inventory)
        {
            item.NewUser(this);
            item.UnequipItem();
        }
        // setup first item and make it ready to aim and allat
        EquipActive(0);
        SetSwitchItem();
        SetAimStyle(alt_item);
        // initialize default look position
        aim_dir = new Vector2(1, -1);
        Look(entity_rb.position + aim_dir);

        // setup AI
        behavior_controller = new BehaviorController(this);
    }
    #endregion

    #region Updates
    // Update is called once per frame
    void Update()
    {
        // constantly adjust aim position, since aim doesn't snap
        Aim();

        // set switch item time duration
        if (curr_switch_cd > 0)
        {
            curr_switch_cd -= Time.deltaTime;
            if (curr_switch_cd <= 0)
            {
                SetSwitchItem();
            }
        }

        // Update AI
        if (is_AI_active)
        {
            behavior_controller.UpdateAI();
        }
    }

    void FixedUpdate()
    {
        // movement
        Move();
    }
    #endregion

    #region Looking & Aiming
    public void Look(Vector2 look_pos) {
        // look_dir is the direction the operator is set to look at
        Vector2 look_dir = (look_pos - entity_rb.position).normalized;

        if ((look_dir.x >= 0) != (aim_dir.x >= 0))
        {
            Vector3 look_scale = new Vector3 ((int)Mathf.Sign(look_dir.x), 1, 1);
            front.localScale = look_scale;
            body_sprite.localScale = look_scale;
            back.localScale = look_scale;
            akimbo_hand_pos.Item1.x *= -1;
            akimbo_hand_pos.Item2.x *= -1;
        }


        // aim hands and body to correct direction
        aim_dir = look_pos - entity_rb.position;
        AimStyle();
    }
    public void Aim()
    {        
        // aim the items
        main_item.Aim(aim_dir);
        alt_item?.Aim(aim_dir);
    }

    void SetAimStyle(bool is_akimbo) // set the position of main & alt hands for akimbo or non-akimbo weaponry whenever weapon switch
    {
        if (is_akimbo)
        {
            // set hand index
            main_hand.SetSiblingIndex(direction_state.Item1? 4 : 0);
            alt_hand.SetSiblingIndex(direction_state.Item1? 0 : 4);
            // adjust hand positions to either side of body
            main_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item1 : akimbo_hand_pos.Item2;
            alt_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item2 : akimbo_hand_pos.Item1;
            AimStyle = AkimboAim;
        } else {
            // set hand index
            main_hand.SetSiblingIndex(direction_state.Item2? 4 : 0);
            alt_hand.SetSiblingIndex(direction_state.Item2? 0 : 4);
            // adjust hand positions to center mass
            main_hand.localPosition = single_hand_pos;
            alt_hand.localPosition = single_hand_pos;
            AimStyle = SingleAim;
        }
    }

    void AkimboAim() // aim two weapons from two sides of body
    {
        // check if direction state has changed
        if (direction_state.Item1 != aim_dir.x > 0) // direction_state.Item1 = true -> facing right
        {
            direction_state.Item1 = aim_dir.x > 0; // update direction state
            
            // switch hand indexes
            main_hand.SetSiblingIndex(alt_hand.GetSiblingIndex());
            alt_hand.SetSiblingIndex(main_hand.GetSiblingIndex() == 4 ? 0 : 4);

            // switch hand positions
            main_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item1 : akimbo_hand_pos.Item2;
            alt_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item2 : akimbo_hand_pos.Item1;
        }

        if (direction_state.Item2 != aim_dir.y < 0) // direction_state.Item2 = true -> facing down (front)
        {
            direction_state.Item2 = aim_dir.y < 0; // update direction state
            
            // switch front & back index
            front.SetSiblingIndex(back.GetSiblingIndex());
            back.SetSiblingIndex(front.GetSiblingIndex() == 3 ? 1 : 3);

            // switch hand positions
            main_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item1 : akimbo_hand_pos.Item2;
            alt_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item2 : akimbo_hand_pos.Item1;

            anim.SetBool("FaceFront", direction_state.Item2); // face front
        }
    }

    void SingleAim() // aim one weapon from center of mass
    {   
        if(direction_state.Item2 != aim_dir.y < 0) {
            direction_state.Item2 = aim_dir.y < 0; // update direction state

            // switch hand  indexes
            main_hand.SetSiblingIndex(alt_hand.GetSiblingIndex());
            alt_hand.SetSiblingIndex(main_hand.GetSiblingIndex() == 4 ? 0 : 4);   
            // switch front/back indexes
            front.SetSiblingIndex(back.GetSiblingIndex());
            back.SetSiblingIndex(front.GetSiblingIndex() == 3 ? 1 : 3);

            anim.SetBool("FaceFront", direction_state.Item2); // face front
        }
    }

    #endregion

    #region Movement
    public void SetMove(Vector2 set_move_dir) // get directional movement
    {
        move_dir = set_move_dir;
        
        anim.SetBool("Moving", true);
    }
    public void SetMovePos(Vector2 set_move_pos) // get target_position
    {
        move_dir = (set_move_pos - entity_rb.velocity).normalized;
        
        anim.SetBool("Moving", true);
    }

    public void Move() 
    {
        entity_rb.velocity = move_dir*curr_speed;
        //entity_rb.MovePosition(entity_rb.position + velocity + move_dir*Time.deltaTime*Mathf.Max(0, move_speed));
    }
    void IMovement.ForceMove(Vector2 direction, float scalar)
    {

    }

    void IMovement.ChangeSpeed(float scale_base)
    {

    }

    public void StopMove()
    {
        move_dir = Vector2.zero;
        anim.SetBool("Moving", false);
    }

    public Vector2 GetPosition()
    {
        return entity_rb.position;
    }
    #endregion

    #region Damage/Health System
    public void TakeDamage(int damage_amt)
    {
        //Debug.Log(base_data.name + " has taken " + damage_amt + " damage");
        curr_health -= damage_amt;
        if (curr_health <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void TakeHeal(int heal_amt)
    {
        //Debug.Log(base_data.name + " has healed " + heal_amt + " health");
    }
    #endregion

    #region  AI Stuff

    // public ContactPoint2D[] GetAllInRange()
    // {
    //     range_collider.GetContacts(things_in_range);
    //     return things_in_range;
    // }

    public void ToggleAI(bool is_on)
    {
        is_AI_active = is_on;
    }

    public void SetLeader(Character new_leader)
    {
        behavior_controller.SetLeader(new_leader);
    }

    public void SetCommandBehavior(CommandMode command)
    {
        behavior_controller.SetCommand(command);
    }

    #endregion

    #region  Inventory Management
    
    public bool HasAltAction() // returns true if the operator is currently wielding two items or an multi-state items
    {
        return item_indexes[curr_item_index].y != -1;  // return true for one action, and false for two actions
    }
    void EquipActive(int index)  // equip the currently selected weapons (the ones the operator is currently holding)
    {
        main_item = inventory[item_indexes[index].x];
        if (item_indexes[index].y == -1)
        {
            alt_item = null;
        } 
        else
        {
            alt_item = inventory[item_indexes[index].y];
        }
    }
    void UnequipActive() // unequip animation for the currently selected weapons
    {
        main_item.UnequipItem();
        alt_item?.UnequipItem();
    }
    public void SwitchItem(int spec_index = -1) // cycle between item_indexes slots, or choose a select slot with spec_index
    {
        if (spec_index == curr_item_index) {return;} // dont do anything if switching to active items
        
        if (spec_index == -1) // typical incrementation
        {
            curr_item_index += 1;
            if (curr_item_index > item_indexes.Length - 1)
            {
                curr_item_index = 0;
            }
        }
        else // specific index
        {
            curr_item_index = Mathf.Clamp(spec_index, 0, item_indexes.Length);
        }
        UnequipActive(); //unequipped item will call the "SetSwitchItem" to set the new active item
        current_indexes = (item_indexes[curr_item_index].x, item_indexes[curr_item_index].y);
        EquipActive(curr_item_index); // set up the new shi
        curr_switch_cd = switch_cd;
    }

    public void SetSwitchItem() // only setup the new item VFX after the old one has been put away completely
    {
        main_item.EquipItem();
        alt_item?.EquipItem();
        SetAimStyle(alt_item); // adjust how the item(s) look in the player's hands
    }

    #endregion
    #region Item Interaction
    public void UseMainItem()
    {
        main_item.Use();
    }
    public void StopMainItem()
    {
        main_item.Stop();
    }
    public void UseAltItem()
    {
        alt_item.Use();
    }
    public void StopAltItem()
    {
        alt_item.Stop();
    }
    public void ResetItems()
    {
        main_item.Reset();
        alt_item?.Reset(); // reset if there's an alt item
    }
    #endregion
    public int GetRangeScalar()
    {
        return inventory[current_indexes.Item1].GetRange();
    }
}