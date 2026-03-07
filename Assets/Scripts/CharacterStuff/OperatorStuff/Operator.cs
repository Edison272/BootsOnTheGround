using System;
using System.Collections;
using TMPro;
using UnityEngine;

using Random = UnityEngine.Random;

public class Operator : Character, IInteractable
{
    [Header("ID")]
    private OperatorSO base_op_data;
    public OpClass op_class => this.base_op_data.op_class;
    public string id_number {get; private set;}
    [SerializeField] TextMeshPro nametag;

    [field: Header("Op UI Elements")]
    public GameObject selection_indicator;

    [Header("Ability")]
    private Ability ability;
    //public bool ability_in_use => ability.ability_duration
    public float ability_cooldown_progress => ability.GetAbilityCooldownProgress();

    [field: Header("Events")]
    public event Action<float> OnActiveUpdate;
    public event Action<float> OnEnemyKilled;
    public event Action<float> OnDamageTaken;

    [field: Header("Events")]
    public float curr_redeployment_time = 0;

    public OperatorController op_behavior_controller;

    #region Initializers
    public void AssignBaseOpData(OperatorSO base_op_data)
    {
        this.base_op_data = base_op_data;
        AssignBaseData(base_op_data);

        // get the operator ability
        ability = base_op_data.ability.GenerateAbility(this);

        // turn off any ui elements
        selection_indicator.SetActive(false);
    }
    public override void ResetData() // reset data after redeploying
    {
        health_component.ResetHealthComponent();
        movement_component.ResetMovementComponent();

        movement_component.SetPosition(GameOverseer.THE_OVERSEER.squad_manager.player_character.GetPosition() + Random.insideUnitCircle * 2);
        GetReady();
    }
    #endregion
    #region Updates
    protected override void Update()
    {
        base.Update();
        OnActiveUpdate?.Invoke(Time.deltaTime);
        ability.UpdateAbility();

        // if (curr_redeployment_time > 0)
        // {
        //     curr_redeployment_time -= Time.deltaTime;
        //     if (curr_redeployment_time <= 0)
        //     {
        //         curr_redeployment_time = 0;
        //         ResetData();
        //     }
        // }
    }

    protected override void LateUpdate()
    {
        if (!is_alive && curr_redeployment_time == 0f)
        {
            GameOverseer.THE_OVERSEER.squad_manager.RedeployOperator(this, base_op_data.redeployment_time);
            curr_redeployment_time += base_op_data.redeployment_time;
            ResetItemData();
            ability.ResetAbility();
            gameObject.SetActive(false);
        }
    }
    #endregion

    public override void CreateBehaviorController()
    {
        op_behavior_controller = new OperatorController(this);
        behavior_controller = op_behavior_controller;
    }

    public void AssignIdString(int initialization_order)
    {
        string string_op_class = "Free";
        switch(op_class)
        {
            case OpClass.Vanguard:
                string_op_class = "Vang";
                break;
            case OpClass.Overwatch:
                string_op_class = "Over";
                break;
            case OpClass.Specialist:
                string_op_class = "Spec";
                break;
            case OpClass.Responder:
                string_op_class = "Resp";
                break;
        }
        id_number = string_op_class + "-" + initialization_order;
        nametag.text = id_number;
    }

    public void ToggleOp(bool isActive)
    {
        vfx_body.SetActive(isActive);
        entity_rb.simulated = isActive;
        main_body.GetComponent<SpriteRenderer>().enabled = isActive;
    }

    public void Deploy()
    {
        behavior_controller.anchor_position = GetPosition();
        GetReady();
    }

    public void Redeploy()
    {
        gameObject.SetActive(true);
        ResetData();
        curr_redeployment_time = 0;
    }

    public void Retreat()
    {
    
    }
    #region Damage/Health System
    public override void ChangeHealth(int change_amt)
    {
        if (change_amt > 0) // this is damage
        {
            OnDamageTaken?.Invoke(change_amt);
        }
        health_component.ChangeHealth(change_amt);
    }
    #endregion

    public void UseAbility(Vector2 target_area)
    {
        if (ability.is_usable)
        {
            ability.UseAbility();
        }
    }

    public override bool IsInAction()
    {
        return is_alive;
    }


    #region UI Switches
    public void ToggleSelectionIndicator(bool enable)
    {
        selection_indicator.SetActive(enable);
    }
    #endregion

    #region Interaction
    public void Interact(Character character)
    {
        throw new NotImplementedException();
    }
    #endregion
}