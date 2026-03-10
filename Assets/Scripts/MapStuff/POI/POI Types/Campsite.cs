using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campsite : MinorPOI, IInteractable
{
    [Header("UI")]
    public GameObject InteractPrompt;
    public GameObject UsesBar;

    [Header("Functionality")]
    [SerializeField] int heal_amount;
    [SerializeField] int curr_uses;
    [SerializeField] int total_uses;
    public bool is_usable => curr_uses > 0;

    public string GetPromptText()
    {
        return "rest at campsite";
    }

    public void Interact(Character character)
    {
        if (is_usable)
        {
            character.ChangeHealth(-heal_amount);
            total_uses -= 1;
        }
    }

    public void ToggleInteractPrompt(bool is_enabled)
    {
        InteractPrompt.SetActive(is_enabled);
    }

    // Start is called before the first frame update
    void Start()
    {
        ToggleInteractPrompt(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
