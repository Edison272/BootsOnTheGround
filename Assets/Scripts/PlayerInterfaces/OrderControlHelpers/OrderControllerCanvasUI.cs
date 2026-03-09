using System;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UI;

// ui control specifically for the canvas elements
public class OrderUIController : MonoBehaviour
{
    [SerializeField] OrderController order_controller;
    PlayerController player_controller;
    
    [Header("On-Reticle UI")]
    public GameObject CanvasCursorGraphic;
    public GameObject RecallConfirmUI;

    [Header("CommandBar UI")]
    public RectTransform command_bar;
    public TextMeshProUGUI command_text;

    public void SetOrderController(PlayerController player_controller)
    {
        this.order_controller = player_controller.order_controller;
        this.player_controller = player_controller;
    }

    public void Update()
    {        
        string update_command_text = "";
        if (player_controller.curr_hold_order_time > 0)
        {
            if (order_controller.chosen_op != null)
            {
                update_command_text += "cancelling operator command...";
            }
            else
            {
                update_command_text += "recalling all operators...";
            }
            
        }
        
        
        CommandMode curr_order_command = order_controller.current_command;


        command_text.text = update_command_text;
    }
}