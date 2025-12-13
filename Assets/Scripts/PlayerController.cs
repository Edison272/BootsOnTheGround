using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    BOTG_Controls controls;
    InputAction movement;
    InputAction looking;
    InputAction main_action;
    bool main_continuous; // checked on update to see if the main action should be called
    public bool main_hold_input = false;  // if true, mouse will main action function will constantly be called
    InputAction alt_action;
    bool alt_continuous; // checked on update to see if the alt action should be called
    public bool alt_hold_input = false;  // if true, mouse will alt action function will constantly be called
    InputAction reset_action;

    
    [Header("Character Control")]
    [SerializeField] private Camera cam;
    [SerializeField] Vector2 input_dir;
    [SerializeField] Vector2 look_pos;

    void Awake()
    {
        // get map
        controls = new BOTG_Controls();
        
        // map input actions to the controls
        movement = controls.GroundActions.Move;
        movement.Enable();
        looking = controls.GroundActions.Look;
        looking.Enable();
        main_action = controls.GroundActions.MainAction;
        main_action.Enable();
        alt_action = controls.GroundActions.AltAction;
        alt_action.Enable();

        // subscribe to the relevant events
        movement.performed += Move;
        looking.performed += Look;

        // special subscription based on whetehr or not item is full auto
        main_action.started += MainStart;
        main_action.canceled += MainRelease;
        alt_action.started += AltStart;
        alt_action.canceled += AltRelease;
        
        // reset the weapon (reload, recharge, reset stance, etc.)
        controls.GroundActions.Reset.performed += ResetAction;
        controls.GroundActions.Reset.Enable();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (main_continuous)
        {
            MainAction();
        }
        if (alt_continuous)
        {
            AltAction();
        }
    }

    #region Controls
    public void Move(InputAction.CallbackContext context) {input_dir = context.ReadValue<Vector2>();}
    public void Look(InputAction.CallbackContext context) {
        Vector3 pos = context.ReadValue<Vector2>();
        pos.z = -10;
        look_pos = cam.ScreenToWorldPoint(pos);
    }
    public void MainStart(InputAction.CallbackContext context) {
        Debug.Log("Main");
        main_continuous = main_hold_input;
    }
    public void MainRelease(InputAction.CallbackContext context) {
        Debug.Log("End Main");
        main_continuous = false;
    }
    public void MainAction()
    {
        Debug.Log("Main");
    }
    public void AltStart(InputAction.CallbackContext context) {
        Debug.Log("Alt");
        alt_continuous = alt_hold_input;
         
    }
    public void AltRelease(InputAction.CallbackContext context) {
        Debug.Log("End Alt");
        alt_continuous = false;
    }
    public void AltAction()
    {
        Debug.Log("Alt");
    }
    public void ResetAction(InputAction.CallbackContext context) {Debug.Log("Resetting");}
    #endregion
}
