using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OperatorUIController : HealthUIController
{
    [Header("Operator Profile")]
    public TextMeshProUGUI operator_id;
    public RectTransform profile_image;
    [Header("Selection")]
    [SerializeField] GameObject selection_indicator;
    [SerializeField] TextMeshProUGUI selection_key;

    public override void SetActiveCharacter(Character active_character)
    {
        base.SetActiveCharacter(active_character);
        operator_id.text = ((Operator)active_character).id_number;
        selection_key.text = "" + operator_id.text[^1];
        
        ToggleSelection(false);
    }

    public void ToggleSelection(bool is_selected)
    {
        selection_indicator.SetActive(is_selected);
    }
}