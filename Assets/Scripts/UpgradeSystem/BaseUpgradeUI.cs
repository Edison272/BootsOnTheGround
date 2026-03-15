using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("UI Core Components")]
    public GameObject UpgradeOption;

    [Header("UI Text Components")]
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Stats;
    public TextMeshProUGUI Description;

    [Header("UI Visual Components")]
    public GameObject ButtonOutline;

    [Header("Upgrade Effect")]
    [SerializeField] private UpgradeUI Effect;

    public void SetSelected(bool is_enabled)
    {
        ButtonOutline.SetActive(is_enabled);
    }
    public void SetUpgradeType(string text)
    {
        Name.text = text;
    }

    public void SelectUpgrade()
    {
        
    }
}