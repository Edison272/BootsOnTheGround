using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OperatorStatusUI : MonoBehaviour
{
    [Header("ID")]
    public Character ui_operator;
    public int squad_index = 0;

    [Header("UI Objects")]
    [SerializeField] GameObject selection_effects;

    [Header("Health Bar")]
    public TextMeshProUGUI hp_text;
    public RectTransform hp_bar;
    public RectTransform shield_bar;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hp_text.text = ui_operator.curr_health + "/" + ui_operator.max_health;
        hp_bar.transform.localScale = new Vector3(1 * ui_operator.health_ratio, 1, 0);
        shield_bar.transform.localScale = Vector3.zero;
    }

    public void ConstructUI(Character this_op, int index)
    {
        ui_operator = this_op;
        squad_index = index;
    }

    public void ConfirmOperator(bool is_present)
    {
        GameOverseer.THE_OVERSEER.canvas_control.SetBuffer(squad_index, is_present);
        selection_effects.SetActive(is_present);
    }
}
