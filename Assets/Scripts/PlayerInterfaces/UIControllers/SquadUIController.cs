using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SquadUIController : MonoBehaviour
{
    [Header("Instantiate Op UI")]
    public GameObject op_ui_controller_instance;
    [Header("Active Op UI")]
    public OperatorUIController[] op_ui_controllers;

    [Header("UI Objects")]
    public RectTransform layout_group;

    [Header("Squad")]
    public SquadManager squad_manager;
    public void Awake()
    {
        op_ui_controller_instance.SetActive(false);
    }

    public void SetSquadManager(SquadManager squadManager)
    {
        this.squad_manager = squadManager;
        int i = 0;
        op_ui_controllers = new OperatorUIController[squadManager.operator_presets.Length - 1];
        foreach(Operator add_op in squadManager.operators)
        {
            if (add_op != squadManager.player_character)
            {
                GameObject new_op_ui = Instantiate(op_ui_controller_instance, layout_group);
                new_op_ui.SetActive(true);

                OperatorUIController new_op_health_ui = new_op_ui.GetComponent<OperatorUIController>();
                op_ui_controllers[i] = new_op_health_ui;
                new_op_health_ui.SetActiveCharacter(add_op);
                i++;
            }
        }
    }

    public void SelectOperatorUI(int index)
    {
        foreach(OperatorUIController op_ui in op_ui_controllers)
        {
            op_ui.ToggleSelection(false);
        }
        if (index != -1)
        {
            op_ui_controllers[index-1].ToggleSelection(true);
        }
    }
}