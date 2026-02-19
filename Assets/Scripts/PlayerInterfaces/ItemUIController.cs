using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIController : MonoBehaviour
{
    [Header("UI Objects")]
    public Image item_sprite;
    public TMP_Text item_counter;
    public RectTransform reset_bar;

    [Header("Character")]
    public Character active_character = null;
    private (int, int) curr_items = (-1,-1);
    private Item ui_item;

    Action UpdateUIType;

    public void SetActiveCharacter(Character active_character)
    {
        this.active_character = active_character;
    }

    void Update()
    {
        if (active_character)
        {
            // change how ui looks when weapons are switched
            if (curr_items != active_character.current_indexes)
            {
                SetUI();
                curr_items = active_character.current_indexes;
            }
            // routine ui update
            else
            {
                UpdateUIType();
            }
        }
    }

    private void SetUI()
    {
        if (active_character.main_item.ui_image)
        {
            item_sprite.sprite = active_character.main_item.ui_image;
            ui_item = active_character.main_item;
        }
        switch (active_character.main_item.func_module)
        {
            case Gun:
                UpdateUIType = UpdateGunUI;
                break;
            case Melee:
                break;
            case Shield:
                break;
            case Conduit:
                break;
        }
    }

    #region Gun Update UI

    public void UpdateGunUI()
    {
        Gun gun_module = (Gun)ui_item.func_module;
        if (ui_item.reset_timer > 0)
        {
            reset_bar.localScale = new Vector3(ui_item.GetResetCompletion(), 1, 0);
            item_counter.text = string.Format("Reloading");
        } 
        else if (reset_bar.localScale.x > 0)
        {
            reset_bar.localScale = new Vector3(0, 1, 1);
        } 
        else
        {
            item_counter.text = string.Format("{0} / {1}", gun_module.ammo, gun_module.max_ammo);
        }
    }

    #endregion
}