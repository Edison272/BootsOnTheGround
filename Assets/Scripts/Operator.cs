using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Operator : MonoBehaviour
{
    //vfx
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
    public Vector2 sprite_center; // center of mass of this srpite
    // character has 4 VFX states based on aim direciton, stored as two booleans, X and Y
    // T, F = Right Bottom | T, T = Right Top | F, T = Left Top | F, F = Left Bottom
    (bool, bool) direction_state = (true, true);
    [SerializeField] (Vector2, Vector2) akimbo_hand_pos = (new Vector2 (-0.2f, 0.975f), new Vector2 (0.5f, 0.975f));  // (main pos (left), alt pos (right))
    [SerializeField] readonly Vector2 single_hand_pos = new Vector2 (0, 0.975f);  // (main pos, alt pos)

    //aim & handling
    [field: Header("Aiming")]
    Vector2 aim_dir = Vector2.zero; // vector from operator to where they are looking. MAKE SURE ITS UN-NORMALIZED
    private Action AimStyle; // single-item or akimbo aiming?
    public  float aim_angle = 0; // angle (deg) the character is looking in
    [field: Header("IMovement")]
    public float move_speed = 1; //how fast an operator can move
    Vector2 velocity;
    Vector2 move_pos;
    Vector2 move_dir;
    [SerializeField] Rigidbody2D entity_rb;

    [field: Header("Inventory")]
    public Item[] inventory;
    public Vector2Int[] item_indexes;  // Access items from the items list with indexes. Vector X for Main Item, Vector Y for Alt Item
    int curr_item_index = 0;           // access items indexes list
    public Item main_item;
    public Item alt_item;

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
            item.Setup();
            item.NewUser(this);
            item.UnequipItem();
        }
        // setup first item and make it ready to aim and allat
        EquipActive(0);
        SetAimStyle(alt_item);
        // initialize default look position
        aim_dir = new Vector2(1, -1);
        Look(entity_rb.position + aim_dir);  
    }

    // Update is called once per frame
    void Update()
    {

        Aim();
        // movement
        Move();
    }
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
    public void SetMove(Vector2 move_dir) // called when the 
    {
        this.move_dir = move_dir;
        anim.SetBool("Moving", true);
    }

    public void Move() 
    {
        entity_rb.MovePosition(entity_rb.position + velocity + move_dir*Time.deltaTime*Mathf.Max(0, move_speed));
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

    #region  Inventory Management
    
    public bool HasAltAction() // returns true if the operator is currently wielding two items or an multi-state items
    {
        return item_indexes[curr_item_index].y != -1;  // return true for one action, and false for two actions
    }
    public void EquipActive(int index)  // equip the currently selected weapons (the ones the operator is currently holding)
    {
        main_item = inventory[item_indexes[index].x];
        main_item.EquipItem();
        if (item_indexes[index].y != -1)
        {
            alt_item = inventory[item_indexes[index].y];
            alt_item.EquipItem();
        }
    }
    public void UnequipActive() // unequip the currently selected weapons
    {
        main_item.UnequipItem();
        main_item = null;
        alt_item?.UnequipItem();
        alt_item = null;
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
        
        // probably add something to disable the previous items latee
        UnequipActive();
        EquipActive(curr_item_index); // set up the new shi
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
}
