using System.Runtime.InteropServices;
using UnityEngine;

public interface IInteractable
{       
    public void Interact(Character character);
    public void ToggleInteractPrompt(bool enable);
    public string GetPromptText();
}