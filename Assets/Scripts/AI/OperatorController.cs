
using UnityEngine;

[System.Serializable]
public class OperatorController : BehaviorController
{
    private Operator control_operator;
    [Header("Squad Manager Interaction")]
    private SquadManager squad_manager;
    public int squad_index;
    public OperatorController(Character c) : base(c)
    {
        control_operator = c.GetComponent<Operator>();
        squad_manager = GameOverseer.THE_OVERSEER.squad_manager;
        this.AddBehavior(CommandMode.Hold).AddBehavior(CommandMode.Follow).AddBehavior(CommandMode.Interact);
    }

    public override void UpdateAI()
    {
        base.UpdateAI();

        if (control_operator.can_use_ability)
        {
            control_operator.UseAbility(control_operator.GetPosition() + control_operator.aim_dir);
        }


    }

    // protected override void FollowCommand()
    // {
    //     // anchor_position = squad_manager.op_formation[squad_index];
    //     // Vector2 move_dir = (anchor_position - character.GetPosition()).normalized;
    //     // //Debug.Log(anchor_position);
    //     // character.SetMove(move_dir);
    //     base.FollowCommand();
    // }

    // protected override void HoldCommand()
    // {
    //     base.HoldCommand();
    // }
}