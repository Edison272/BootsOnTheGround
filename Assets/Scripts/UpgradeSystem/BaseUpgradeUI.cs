using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject UpgradeOption;
    public TextMeshProUGUI UpgradeName;

    [Header("Upgrade Effect")]
    [SerializeField] private UpgradeUI Effect;

    public void SetUpgradeType(string text)
    {
        UpgradeName.text = text;
    }

    public void SelectUpgrade()
    {
        
    }
}