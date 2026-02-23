using System;
using TMPro;
using UnityEngine;

public class Operator : Character
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
    public float ability_cooldown_progress => ability.GetAbilityCooldownProgress();

    [field: Header("Events")]
    public event Action<float> OnActiveUpdate;
    public event Action<float> OnEnemyKilled;
    public event Action<float> OnDamageTaken;

    public bool is_deployed = false;

    public OperatorController op_behavior_controller;

    public void AssignBaseOpData(OperatorSO base_op_data)
    {
        this.base_op_data = base_op_data;
        AssignBaseData(base_op_data);

        // get the operator ability
        ability = base_op_data.ability.GenerateAbility(this);

        // turn off any ui elements
        selection_indicator.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();
        OnActiveUpdate?.Invoke(Time.deltaTime);
        ability.UpdateAbility();
    }

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
        is_deployed = isActive;
    }

    public void Deploy()
    {
        behavior_controller.anchor_position = GetPosition();
        GetReady();
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
        return is_alive && is_deployed;
    }


    #region UI Switches
    public void ToggleSelectionIndicator(bool enable)
    {
        selection_indicator.SetActive(enable);
    }
    #endregion
}