using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Operator : MonoBehaviour
{
    //vfx
    [field: Header("VFX")]
    [field: SerializeField] public GameObject main_body {get; set;} //basically the hitbox
    [field: SerializeField] public GameObject vfx_body {get; set;} //the vfx body
    [field: SerializeField] public Animator anim {get; set;}
    [field: SerializeField] public Transform front {get; set;}
    [field: SerializeField] public Transform back {get; set;}
    [field: SerializeField] public Transform main_hand {get; set;} //always set to main hand object
    [field: SerializeField] public Transform alt_hand {get; set;} //always set to off hand object

    //aim & handling
    public Vector2 look_pos = Vector2.zero; // where op is looking
    public Vector2 aim_pos = Vector2.zero; //where bro is aiming
    private Vector2 aim_dir = new Vector2(1, 0);
    public float aim_angle = 0;
    public float handle_speed = 20;

    [field: Header("IMovement")]
    public float move_speed = 1; //how fast an operator can move
    
    public Vector2 velocity {get; set;}
    Vector2 move_pos;
    Vector2 move_dir;
    public Rigidbody2D entity_rb;
    

    // Start is called before the first frame update
    void Start()
    {
        
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

        // make sprite face right direction
        anim.SetBool("FaceFront", !(aim_pos.y > entity_rb.position.y));

        // maintain sorting order
        if(Mathf.Sign(aim_dir.y) == Mathf.Sign(aim_dir.x)) {
            // set back first
            front.SetSiblingIndex(0);
            vfx_body.transform.SetSiblingIndex(1);
            back.SetSiblingIndex(2);
        } else {
            // set front first
            front.SetSiblingIndex(2);
            vfx_body.transform.SetSiblingIndex(1);
            back.SetSiblingIndex(0);
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
    #endregion
}
