using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class UpgradeSO : ScriptableObject
{
    public abstract void ApplyUpgrade();
    public abstract (string, string, string) GetTextData();
    public abstract Sprite GetSprite();
}


