using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [field: SerializeField] public GameObject interact_prompt {get; set;}
    public ItemSO new_item;
    public Item used_item;
    [Header("Interaction Prompt")]
    public GameObject InteractionUI;
    [SerializeField] SpriteRenderer this_sprite;

    public void Start()
    {
        this_sprite.sprite = new_item.ui_image;
    }
    public void Interact(Character character)
    {        
        Item item_pickup = used_item ? used_item : character.GetItemSO(new_item);
        used_item = character.PickupItem(item_pickup);
        if (used_item)
        {        
            this_sprite.enabled = false;
            used_item.transform.parent = this.transform;
            used_item.transform.localPosition = Vector3.zero;
            used_item.DropItem();
        }
        else
        {
            this_sprite.enabled = true;
            this_sprite.sprite = used_item.base_data.ui_image;
        }
    }

    public void ToggleInteractPrompt(bool enable)
    {
        throw new NotImplementedException();
    }

    public string GetPromptText()
    {
        return "pick up item";
    }
}