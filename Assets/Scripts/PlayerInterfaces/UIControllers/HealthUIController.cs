using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIController : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] protected Character active_character = null;
    private HealthComponent health_component;
    [Header("Health Bar")]
    public RectTransform bg_bar;
    public TextMeshProUGUI hp_text;
    public RectTransform health_bar;
    public RectTransform health_drift_bar;
    public RectTransform shield_bar;
    private const float c_health_drift_lerp = 3;
    private float prev_health_width = 0;
    
    public virtual void Awake()
    {
        health_bar.sizeDelta = new Vector2(bg_bar.sizeDelta.x, bg_bar.rect.height);
        shield_bar.sizeDelta = new Vector2(bg_bar.sizeDelta.x, bg_bar.rect.height);
        health_drift_bar.sizeDelta = new Vector2(bg_bar.sizeDelta.x, bg_bar.rect.height);
    }
    public virtual void SetActiveCharacter(Character active_character)
    {
        this.active_character = active_character;
        health_component = active_character.health_component;
        //shield_bar.sizeDelta = new Vector2(0, shield_bar.sizeDelta.y);
    }

    public virtual void Update()
    {
        if (active_character)
        {
            float currentWidth = health_drift_bar.sizeDelta.x;
            float targetWidth = health_bar.sizeDelta.x;
            // drift effect
            float drift_x = Mathf.Lerp(health_drift_bar.sizeDelta.x, health_bar.sizeDelta.x, Time.deltaTime);
            health_drift_bar.sizeDelta = new Vector2(drift_x, health_drift_bar.sizeDelta.y);

            SetHealth();
        }
    }

    public void SetHealth()
    {
        float max_health_ratio = (float)health_component.max_health / health_component.total_max_hitpoints;
        float shield_ratio = 1 - max_health_ratio;

        float total_width = bg_bar.rect.width;
        float health_width = total_width * max_health_ratio * health_component.health_ratio;
        float shield_width = total_width * shield_ratio;

        health_bar.sizeDelta = new Vector2(health_width, bg_bar.rect.height);
        shield_bar.sizeDelta = new Vector2(shield_width, bg_bar.rect.height);
        if (health_width > prev_health_width)
        {
            health_drift_bar.sizeDelta = new Vector2(health_width, health_drift_bar.sizeDelta.y);
        }

        shield_bar.anchoredPosition = new Vector2(health_drift_bar.sizeDelta.x, shield_bar.anchoredPosition.y);

        if (hp_text)
        {
            hp_text.text = health_component.total_curr_hitpoints + "/" + health_component.max_health;
        }
           

        prev_health_width = health_width;    
    }
}