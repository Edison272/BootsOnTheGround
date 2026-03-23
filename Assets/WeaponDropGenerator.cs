using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesGenerator : MonoBehaviour
{
    public int lootbox_rarity;
    public int lootbox_size;
    public float luck;

    public UpgradeSO[] common_upgrades;
    public UpgradeSO[] rare_upgrades;
    public UpgradeSO[] elite_upgraddes;

    public void GenerateLootboxes(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            
        }
    }
}
