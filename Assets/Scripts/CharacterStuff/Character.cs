using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
public class Character : MonoBehaviour, IHealth, IMovement
{
    private CharacterSO base_data;
    [field: Header("Body Parts")]
    public GameObject main_body;//basically the hitbox
    public GameObject vfx_body; //the vfx body
    public Transform front;
    public Transform back;
    public Transform body;
    public Transform body_sprite;
    public Transform main_hand; //always set to main hand object
    public Transform alt_hand; //always set to off hand object
    public Transform head;
    public GameObject front_particles;
    public GameObject back_particles;
    enum CharacterBodyParts {Hitbox, Front, Back, SpriteBody, MainHand, AltHand, Head, FrontParticles, BackParticles};

    [field: Header("VFX")]
    public Animator animator;
    protected Vector2 sprite_center; // center of mass of this srpite
    public float hitbox_radius {get; private set;}
    // character has 4 VFX states based on aim direciton, stored as two booleans, X and Y
    // T, F = Right Bottom | T, T = Right Top | F, T = Left Top | F, F = Left Bottom
    protected (bool, bool) direction_state = (true, true);
    protected (Vector2, Vector2) akimbo_hand_pos = (new Vector2 (-0.2f, 0.6f), new Vector2 (0.5f, 0.6f));  // (main pos (left), alt pos (right))
    Vector2 single_hand_pos = new Vector2 (0, 0.6f);  // (main pos, alt pos)
    //aim & handling
    [field: Header("Aiming")]
    public Vector2 aim_dir {get; private set;} = Vector2.zero; // vector from operator to where they are looking. MAKE SURE ITS UN-NORMALIZED
    protected Action AimStyle; // single-item or akimbo aiming?
    public  float aim_angle = 0; // angle (deg) the character is looking in
    readonly Vector2 SingleWeaponRestPosition = new Vector2(1, -1);
    
    [field: Header("Movement")]
    [field: SerializeField] public MovementComponent movement_component {get; private set;}
    public float move_speed => movement_component.move_speed; //maximum speed an operator can move at
    public float base_speed => movement_component.base_speed;
    public Vector2 move_dir => movement_component.move_dir;
    public Vector2 move_pos => movement_component.move_pos;
    public bool destination_reached => movement_component.destination_reached;
    public Vector2 last_move_dir => movement_component.last_move_dir;
    public Vector2 force_dir => movement_component.force_dir;
    public Rigidbody2D entity_rb {get; private set;}
    public Vector2Int current_tile_pos = Vector2Int.zero;

    [Header("Acceleration")]
    public float curr_speed => movement_component.curr_speed;
    public float curr_accel_time => movement_component.curr_accel_time;
    public float max_accel_time => movement_component.max_accel_time; // amount of time operator needs to get to top move speed

    [field: Header("Health Stuff")]
    [field: SerializeField] public HealthComponent health_component {get; private set;}
    public int curr_health => health_component.curr_health;
    public int max_health => base_data.health;
    public int shield => health_component.shield;
    public float health_ratio => health_component.health_ratio;
    public bool is_alive => health_component.is_alive;

    [field: Header("Health UI")]
    [SerializeField] HealthUI health_ui = new HealthUI();

    [field: Header("Inventory")]
    public Item[] inventory;
    public Vector2Int[] item_indexes;  // Access items from the items list with indexes. Vector X for Main Item, Vector Y for Alt Item
    protected int curr_item_index = 0;           // access items indexes list
    public Item main_item;
    public Item alt_item;
    public (int, int) current_indexes {get; protected set;}
    protected float switch_cd = 0.5f; // time the char must wait before they can switch to the next weapon
    protected float curr_switch_cd = 0;

    [field: Header("Detection")]
    // [SerializeField] CircleCollider2D range_collider;
    // ContactPoint2D[] things_in_range;
    public Character target = null;
    public float curr_range {get; private set;}
    public float base_range => base_data.range;
    public float close_range => base_data.close_range;

    [field: Header("AI")]
    public int faction_tag = 1;
    protected bool is_AI_active = false;
    public BehaviorController behavior_controller;    
    
    [field: Header("Event Bus")]
    public Action<Character> OnDeath;

    #region initalizers
    // get base data from a scriptable object and assign them here. Called once at when this object is created
    public void AssignBaseData(CharacterSO base_data)
    {
        this.base_data = base_data;

        // setup movement
        entity_rb = this.GetComponent<Rigidbody2D>();

        // setup VFX
        hitbox_radius = GetComponent<CircleCollider2D>().radius;
        akimbo_hand_pos = ((Vector2) main_hand.localPosition, (Vector2) alt_hand.localPosition);
        single_hand_pos = new Vector2(0, main_hand.localPosition.y);

        // set basic sibling order of entity VFX (operator faces BOTTOM RIGHT by default)
        animator.SetBool("FaceFront", true);
        main_hand.SetSiblingIndex(4);
        front.SetSiblingIndex(3);
        vfx_body.transform.SetSiblingIndex(2);
        back.SetSiblingIndex(1);
        alt_hand.SetSiblingIndex(0);

        // setup health & related ui
        health_component = new HealthComponent(max_health, base_data.spawn_shield);
        health_ui.InitializeHealthUI(health_component);

        // setup movement
        movement_component = new MovementComponent(base_data.speed, base_data.accel_time, entity_rb);
        
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
        // set initial active items
        foreach(Item item in inventory)
        {
            item.NewUser(this);
            item.UnequipItem();
        }
        EquipActive(0);
        SetSwitchItem();
        SetAimStyle(alt_item);

        // initialize default look position
        aim_dir = SingleWeaponRestPosition;
        Look(entity_rb.position + aim_dir);

        // setup AI
        CreateBehaviorController();
    }
    // make sure the operator LOOKS ready
    public void GetReady()
    {        
        // equip items
        SetSwitchItem();
        SetAimStyle(alt_item);
        // initialize default look position
        aim_dir = SingleWeaponRestPosition;
        Look(entity_rb.position + aim_dir);
    }

    public virtual void CreateBehaviorController()
    {
        behavior_controller = new BehaviorController(this);
    }
    public void ResetData()
    {
        
    }

    public void ConnectToEventBus(Action<Character> death)
    {
        OnDeath = death;
    }

    #endregion

    #region Updates
    // Update is called once per frame
    protected virtual void Update()
    {
        if (!IsInAction())
        {
            return;
        }
        
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

        if (is_AI_active && !target)
        {
            if (!destination_reached)
            {
                Look(move_pos);
            }
            // else
            // {
            //     Look(GetPosition() + SingleWeaponRestPosition);
            // }
        }

        // update ui helpers
        health_ui.UpdateHealthUI();
    }

    protected virtual void FixedUpdate()
    {
        if (!IsInAction())
        {
            return;
        }
        // movement
        Move();
        // Update AI
        if (is_AI_active)
        {
            behavior_controller.UpdateAI();
        }
    }

    protected virtual void LateUpdate()
    {
        if (!is_alive)
        {
            Destroy(this.gameObject);
        }
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
            body.localScale = look_scale;
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
        main_item?.Aim(aim_dir);
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

            animator.SetBool("FaceFront", direction_state.Item2); // face front
        }
    }

    void SingleAim() // aim one weapon from center of mass
    {   
        if(direction_state.Item2 != aim_dir.y <= 0) {
            direction_state.Item2 = aim_dir.y <= 0; // update direction state

            // switch hand  indexes
            main_hand.SetSiblingIndex(alt_hand.GetSiblingIndex());
            alt_hand.SetSiblingIndex(main_hand.GetSiblingIndex() == 4 ? 0 : 4);   
            // switch front/back indexes
            front.SetSiblingIndex(back.GetSiblingIndex());
            back.SetSiblingIndex(front.GetSiblingIndex() == 3 ? 1 : 3);

            animator.SetBool("FaceFront", direction_state.Item2); // face front
        }
    }

    #endregion

    #region Movement
    
    public void SetPosition(Vector2 new_position)  // completely change positions and forget where they wanted to go before
    {
        movement_component.SetPosition(new_position);
    }
    public void SetMove(Vector2 set_move_dir) // get directional movement, useful for dynamic & sudden maneuvers
    {
        movement_component.SetMove(set_move_dir);
    }
    public void SetMovePos(Vector2 set_move_pos) // get target_position, useful for AI with discrete positioning
    {
        movement_component.SetMovePos(set_move_pos);
    }
    public void Move() 
    {
        bool move_state = animator.GetBool("Moving");
        movement_component.FixedMoveUpdate();
        move_state = movement_component.Move(move_state);
        animator.SetBool("Moving", move_state);
    }
    public void StopMove()
    {
        movement_component.StopMove(is_AI_active);
        animator.SetBool("Moving", false);
    }
    public float GetTravelTime() // return how long it is expected to take for the operator to reach their position
    {
        return (move_pos - GetPosition()).magnitude / base_speed + max_accel_time;
    }

    public void ForceMove(Vector2 direction, float scalar, bool movement_override = false)
    {
        movement_component.ForceMove(direction, scalar, movement_override);
    }
    public Vector2 GetPosition()
    {
        return entity_rb.position;
    }
    public void ChangeSpeed(float scale_base, float duration, bool is_decaying = false)
    {
        movement_component.ChangeSpeed(scale_base, duration, is_decaying);
    }
    #endregion

    #region Damage/Health System
    public void ChangeHealth(int change_amt)
    {
        health_component.ChangeHealth(change_amt);
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
    public virtual bool IsInAction()
    {
        return is_alive;
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
        curr_range = base_range + GetRangeScalar();
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
        UnequipActive(); //unequipped item will call the "SetSwitchItem" in animator to set the new active item
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

    #region Body


    #endregion

    #region Stats & Status Changes


    #endregion

    #region Debug Gizmos
    void OnDrawGizmosSelected()
    {
        // Draw ranges when properly initialized
        if (base_data)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, curr_range);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, base_range);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, close_range);
        }
    }
    #endregion
}