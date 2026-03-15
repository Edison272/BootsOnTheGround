using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("UI Core Components")]
    public GameObject UpgradeOption;
    public Image UpgradeImage;

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
    public void SetUpgradeUI(UpgradeSO upgrade_data)
    {
        UpgradeImage.sprite = upgrade_data.GetSprite();
        
        (string, string, string) text_data = upgrade_data.GetTextData();
        Name.text = text_data.Item1;
        Stats.text = text_data.Item2;
        Description.text = text_data.Item3;
    }

    public void SelectUpgrade()
    {
        
    }
}