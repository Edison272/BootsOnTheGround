using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OperatorUIController : HealthUIController
{
    [Header("Operator Profile")]
    public TextMeshProUGUI operator_id;
    public RectTransform profile_image;

    public override void SetActiveCharacter(Character active_character)
    {
        base.SetActiveCharacter(active_character);
        operator_id.text = ((Operator)active_character).id_number;
    }
}