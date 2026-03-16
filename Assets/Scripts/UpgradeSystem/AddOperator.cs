using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AddOperator", menuName = "ScriptableObjects/Upgrades/AddOperator", order = 0)]
public class AddOperator : UpgradeSO
{
    public OperatorSO add_operator;
    public override void ApplyUpgrade()
    {
        Debug.Log("new operator on the way!");
        GameOverseer.THE_OVERSEER.squad_manager.CreateOperator(add_operator, true);
        GameOverseer.THE_OVERSEER.player_control.order_controller.SetSquadData();
    }

    public override Sprite GetSprite()
    {
       return add_operator.character_image;
    }

    public override (string, string, string) GetTextData()
    {
        string op_name = "";
        op_name += add_operator.character_name;

        string op_stats = "";
        op_stats += "HP - " + add_operator.health + "\n";
        op_stats += "SPD - " + add_operator.speed + "\n";
        op_stats += "RANGE - " + add_operator.range + "\n";
        op_stats += "REDEPLOY - " + add_operator.redeployment_time + "s\n";
        op_stats += "WEAPON : " + add_operator.inventory[0].item_name;

        string op_description = add_operator.description;
        return (op_name, op_stats, op_description);
    }
}