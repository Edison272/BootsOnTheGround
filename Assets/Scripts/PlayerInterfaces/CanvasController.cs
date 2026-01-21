using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    public Animator ui_animator;

    [Header("Squad Sidebar")]
    [SerializeField] GameObject[] op_layout_group;
    [SerializeField] GameObject op_status_instance;

    [SerializeField] TextMeshProUGUI player_mode_text;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //  set up UI for all operators after they've been initialized. Called by Game Overseer
    public void SetOperatorProfiles()
    {
        Character[] squad_members = GameOverseer.THE_OVERSEER.squad_manager.squad;
        foreach(Character op in squad_members)
        {
            GameObject op_status = Instantiate(op_status_instance, op_layout_group.transform);
        }
    }

    public void SetCommandUI(bool is_cmd_mode_on)
    {
        if (is_cmd_mode_on)
        {
            player_mode_text.text = "Command Mode";
        }
        else
        {
            player_mode_text.text = "Action Mode";
        }
    }
}
