using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    public Animator ui_animator;

    [SerializeField] GameObject[] op_status_positions;
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
        int pos_index = 0;
        foreach(Character op in squad_members)
        {
            GameObject op_status = Instantiate(op_status_instance, op_status_positions[pos_index].transform);
            pos_index += 1;
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
