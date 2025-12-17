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
    public Transform main_hand; //always set to main hand object
    public Transform alt_hand; //always set to off hand object

    [field: Header("VFX")]
    public Animator anim;
    public Vector2 sprite_center;
    // character has 4 VFX states based on aim direciton, stored as two booleans, X and Y
    // T, F = Right Bottom | T, T = Right Top | F, T = Left Top | F, F = Left Bottom
    (bool, bool) direction_state = (true, false);

    //aim & handling
    [field: Header("Aiming")]
    public Vector2 look_pos = Vector2.zero; // where op is looking
    public Vector2 aim_pos = Vector2.zero; //where bro is aiming
    private Vector2 aim_dir = new Vector2(1, 0);
    public float aim_angle = 0;
    public float handle_speed = 20;

    [field: Header("IMovement")]
    public float move_speed = 1; //how fast an operator can move
    Vector2 velocity;
    Vector2 move_pos;
    Vector2 move_dir;
    public Rigidbody2D entity_rb;

    [field: Header("Inventory")]
    public Item[] inventory;
    public Vector2Int[] item_indexes;  // Access items from the items list with indexes. Vector X for Main Item, Vector Y for Alt Item
    int curr_item_index = 0;                  // access items indexes list
    public Item main_item;
    public Item alt_item;
    

    // Start is called before the first frame update
    void Start()
    {
        // set basic sibling order of entity VFX
        main_hand.SetSiblingIndex(4);
        front.SetSiblingIndex(3);
        vfx_body.transform.SetSiblingIndex(2);
        back.SetSiblingIndex(1);
        alt_hand.SetSiblingIndex(0);
    }

    // make sure the operator's data is set up
    public void GetReady()
    {
        curr_item_index = 0;
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
        this.look_pos = look_pos;
        Vector2 look_dir = (look_pos - entity_rb.position).normalized;
        if (look_dir.x > 0) 
            {vfx_body.transform.localScale = new Vector3 (1, 1, 1);} 
        else 
            {vfx_body.transform.localScale = new Vector3 (-1, 1, 1);}
        
        aim_angle = Mathf.Atan2(look_dir.y, look_dir.x) * Mathf.Rad2Deg+180;



        // if(main_item != null) {
        //     main_item.look(aim_pos);
        //     if(main_hand.localScale.y != vfx_body.transform.localScale.x) {
        //         main_hand.localScale = new Vector3(1f, vfx_body.transform.localScale.x, 1f);
        //     }
    }
    public void Aim()
    {
        // update weapon rotation
        main_hand.transform.rotation = Quaternion.Slerp(main_hand.transform.rotation, Quaternion.Euler(0, 0, aim_angle), handle_speed * Time.deltaTime);
        alt_hand.transform.rotation = Quaternion.Slerp(alt_hand.transform.rotation, Quaternion.Euler(0, 0, aim_angle), handle_speed * Time.deltaTime);

        

        // aim_pos is the direction the operator turns its weapon to over time
        float look_dist = (entity_rb.position + look_pos).magnitude;
        aim_pos.x = entity_rb.position.x + (-look_dist*Mathf.Cos((main_hand.transform.rotation.eulerAngles.z)*Mathf.Deg2Rad));
        aim_pos.y = entity_rb.position.y + (-look_dist*Mathf.Sin((main_hand.transform.rotation.eulerAngles.z)*Mathf.Deg2Rad));
        aim_dir = (aim_pos - entity_rb.position).normalized;

        // aim the items
        main_item.Aim(aim_pos, aim_angle);
        alt_item.Aim(aim_pos, aim_angle);


        // BR
        /* main hand in front, left side
         then front
         then body_vfx
         then back
         then alt hand in the back, right side
        */ 

        // TR
        /* main hand in front, right side
         then back
         then body_vfx
         then front
         then alt hand in the back, left side
        */ 

        // BL
        /* alt hand in front, right side
         then front
         then body_vfx
         then back
         then main hand in the back, left side
        */ 

        // TL
        /* alt hand in front, left side
         then back
         then body_vfx
         then front
         then main hand in the back, right side
        */ 
        
        // check if direction state has changed
        if (direction_state.Item1 != aim_pos.x > entity_rb.position.x) // direction_state.Item1 = true -> facing right
        {
            // switch hand index
            int mh_sib_index = main_hand.GetSiblingIndex();
            main_hand.SetSiblingIndex(alt_hand.GetSiblingIndex());
            alt_hand.SetSiblingIndex(mh_sib_index);

            // switch hand positions
            Vector3 mh_pos = main_hand.transform.localPosition;
            main_hand.transform.localPosition = alt_hand.transform.localPosition;
            alt_hand.localPosition = mh_pos;

            direction_state.Item1 = aim_pos.x > entity_rb.position.x; // update direction state
        }

        if (direction_state.Item2 != aim_pos.y > entity_rb.position.y) // direction_state.Item2 = true -> facing up
        {
            anim.SetBool("FaceFront", !(aim_pos.y > entity_rb.position.y)); // face front
            
            // switch front & back index
            int front_sib_index = front.GetSiblingIndex();
            front.SetSiblingIndex(back.GetSiblingIndex());
            back.SetSiblingIndex(front_sib_index);

            // switch hand positions
            Vector3 mh_pos = main_hand.transform.localPosition;
            main_hand.transform.localPosition = alt_hand.transform.localPosition;
            alt_hand.localPosition = mh_pos;

            direction_state.Item2 = aim_pos.y > entity_rb.position.y; // update direction state
        }

        // front.SetSiblingIndex(4);
        // main_hand.SetSiblingIndex(3);
        // vfx_body.transform.SetSiblingIndex(2);
        // alt_hand.SetSiblingIndex(1);
        // back.SetSiblingIndex(0);

        // // y-axis changes
        // if (aim_pos.y < entity_rb.position.y)
        // {
        //     // face right
        //     anim.SetBool("FaceFront", true);
        //     int temp_index = front.GetSiblingIndex();
        //     front.SetSiblingIndex(back.GetSiblingIndex());
        //     back.SetSiblingIndex(temp_index);
        // } else
        // {
        //     // face left
        //     anim.SetBool("FaceFront", false);
        // }



        // maintain sorting order
        // if(Mathf.Sign(aim_dir.y) == Mathf.Sign(aim_dir.x)) {
        //     // set back first
        //     front.SetSiblingIndex(0);
        //     vfx_body.transform.SetSiblingIndex(1);
        //     back.SetSiblingIndex(2);
        // } else {
        //     // set front first
        //     front.SetSiblingIndex(2);
        //     vfx_body.transform.SetSiblingIndex(1);
        //     back.SetSiblingIndex(0);
        // }
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
    #endregion

    #region  Inventory Management
    
    public bool HasAltAction() // returns true if the operator is currently wielding two items or an multi-state items
    {
        return item_indexes[curr_item_index].y != -1;  // return true for one action, and false for two actions
    }
    public void SetupActive(int index)  // setup the currently active weapons (the ones the operator is currently holding)
    {
        main_item = inventory[item_indexes[index].x];
        alt_item = inventory[item_indexes[index].y];
    }
    public void SwitchItem(int spec_index = -1) // cycle between item_indexes slots, or choose a select slot with spec_index
    {
        if (spec_index == curr_item_index)
        {
            return;
        }
        
        curr_item_index = spec_index == 0? curr_item_index + 1 : Mathf.Clamp(spec_index, 0, item_indexes.Length);
        
        // probably add something to disable the previous 

        SetupActive(curr_item_index); // set up the new shi
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
