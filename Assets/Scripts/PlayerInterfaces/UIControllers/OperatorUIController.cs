using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OperatorUIController : HealthUIController
{
    private Operator active_operator;
    [Header("Operator Profile")]
    public TextMeshProUGUI operator_id;
    public RectTransform profile_image;
    [Header("Selection")]
    [SerializeField] GameObject selection_indicator;
    [SerializeField] TextMeshProUGUI selection_key;
    [Header("Ability")]
    public RectTransform cooldown_mask;
    public GameObject ready_vfx;

    public override void Awake()
    {
        base.Awake();
        ready_vfx.SetActive(false);
    }
    public override void SetActiveCharacter(Character active_character)
    {
        base.SetActiveCharacter(active_character);
        active_operator = (Operator)active_character;
        operator_id.text = active_operator.id_number;
        selection_key.text = "" + operator_id.text[^1];
        
        ToggleSelection(false);
    }

    public void ToggleSelection(bool is_selected)
    {
        selection_indicator.SetActive(is_selected);
    }

    public override void Update()
    {
        base.Update();
        if (active_operator)
        {   
            float cd_progress = active_operator.ability_cooldown_progress;
            cooldown_mask.localScale = new Vector3(1, cd_progress, 1);
            if (cd_progress >= 1 != ready_vfx.activeSelf)
            {
                ready_vfx.SetActive(cd_progress >= 1);
            }
        }
    }
}