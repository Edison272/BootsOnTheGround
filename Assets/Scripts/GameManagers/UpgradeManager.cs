using UnityEngine;
public class UpgradeManager : MonoBehaviour
{
    public GameObject UpgradeScreen;
    public Transform UpgradeLayout;
    public UpgradeUI[] UpgradeUIs;
    public int upgrade_choices;

    public UpgradeSO[] upgrade_pool;
    private UpgradeSO[] upgrades = new UpgradeSO[3];
    public int selected_upgrade_index;
    
    public void Awake()
    {
        ToggleUpgradeScreen(false);
        // UpgradeUIs = new UpgradeUI[3];
        // for(int i = 0; i < UpgradeUIs.Length; i++)
        // {
        //     UpgradeUIs[i] = Instantiate(Resources.Load<GameObject>("UI/Upgrades/UpgradeOption"), UpgradeLayout).GetComponent<UpgradeUI>();
        // }
    }

    public void ToggleUpgradeScreen(bool is_enabled)
    {
        UpgradeScreen.SetActive(is_enabled);
    }
    public void Inject()
    {
        
    }

    public void GenerateUpgrades()
    {
        ToggleUpgradeScreen(true);

        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i] = upgrade_pool[Random.Range(0, upgrade_pool.Length)];
            UpgradeUIs[i].SetUpgradeUI(upgrades[i]);
        }
        SelectUpgrade(-1);
    }
    public void SelectUpgrade(int index)
    {
        for(int i = 0; i < UpgradeUIs.Length; i++)
        {
            if (i != index)
            {
                UpgradeUIs[i].SetSelected(false);
            }
        }
        if (index >= 0 && index < UpgradeUIs.Length)
        {
            selected_upgrade_index = index;
        }
    }

    public void ApplySelectUpgrade()
    {
        upgrades[selected_upgrade_index].ApplyUpgrade();
    }
}