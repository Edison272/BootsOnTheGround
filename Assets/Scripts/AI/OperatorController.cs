
using UnityEngine;

[System.Serializable]
public class OperatorController : BehaviorController
{
    [Header("Squad Manager Interaction")]
    private SquadManager squad_manager;
    public int squad_index;
    public OperatorController(Character c) : base(c)
    {
        squad_manager = GameOverseer.THE_OVERSEER.squad_manager;
    }

    public override void UpdateAI()
    {
        base.UpdateAI();
        if ((character.GetPosition() - leader.GetPosition()).sqrMagnitude > 30 * 30)
        {
            SetCommand(CommandMode.Follow);
        }
    }

    protected override void FollowCommand()
    {
        anchor_position = squad_manager.op_formation[squad_index];
        Vector2 move_dir = (anchor_position - character.GetPosition()).normalized;
        //Debug.Log(anchor_position);
        character.SetMove(move_dir);
    }
}