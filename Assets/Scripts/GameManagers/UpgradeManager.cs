using UnityEngine;
public class UpgradeManager : MonoBehaviour
{
    public GameObject UpgradeScreen;
    public Transform UpgradeLayout;
    public UpgradeUI[] UpgradeUIs;
    public string[] upgrade_text = new string[1] {"new op"};
    public int upgrade_choices;
    
    public void Awake()
    {
        ToggleUpgradeScreen(false);
        UpgradeUIs = new UpgradeUI[3];
        for(int i = 0; i < UpgradeUIs.Length; i++)
        {
            UpgradeUIs[i] = Instantiate(Resources.Load<GameObject>("UI/Upgrades/UpgradeOption"), UpgradeLayout).GetComponent<UpgradeUI>();
        }
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

        foreach(UpgradeUI ui in UpgradeUIs)
        {
            ui.SetUpgradeType(upgrade_text[Random.Range(0, upgrade_text.Length)]);
        }
    }
    public void SetUpgrade()
    {
        
    }

    public void ApplyUpgrade()
    {
        
    }
}