using System.Runtime.InteropServices;
using UnityEngine;

public interface IInteractable
{   
    public string Identify()
    {
        return "hello";
    }
    
    public void Interact(Character character);
}