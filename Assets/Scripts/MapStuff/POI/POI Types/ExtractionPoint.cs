using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class ExtractionPoint : MinorPOI, IInteractable
{
    [Header("UI")]
    public GameObject InteractPrompt;
    public string GetPromptText()
    {
        return "rest at campsite";
    }

    public void Interact(Character character)
    {
        GameOverseer.THE_OVERSEER.GetUpgrades();
        //this.gameObject.SetActive();
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
