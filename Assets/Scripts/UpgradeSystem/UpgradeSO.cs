using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class UpgradeSO : ScriptableObject
{
    public string upgrade_name;
    public string upgrade_stats;
    public string upgrade_description;
    public abstract void ApplyUpgrade();
}

[CreateAssetMenu(fileName = "AddOperator", menuName = "ScriptableObjects/Upgrades/AddOperator", order = 0)]
public class AddOperator : UpgradeSO
{
    public OperatorSO add_operator;
    public override void ApplyUpgrade()
    {
        Debug.Log("new operator on the way!");
        GameOverseer.THE_OVERSEER.squad_manager.CreateOperator(add_operator);
    }
}

[CreateAssetMenu(fileName = "Temp Boost", menuName = "ScriptableObjects/Upgrades/Temporary Boost", order = 1)]
public class TempBoost : UpgradeSO
{
    public override void ApplyUpgrade()
    {
        //GameOverseer.THE_OVERSEER.squad_manager.CreateOperator(add_operator);
    }
}
[CreateAssetMenu(fileName = "Stat Increase", menuName = "ScriptableObjects/Upgrades/Stat Increase", order = 2)]
public class StatIncrease : UpgradeSO
{
    public override void ApplyUpgrade()
    {
        //GameOverseer.THE_OVERSEER.squad_manager.CreateOperator(add_operator);
    }
}