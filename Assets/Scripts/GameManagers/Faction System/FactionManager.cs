using System.Collections.Generic;
using System;
using UnityEngine;

public sealed class FactionManager
{
    #region Singleton
    private static readonly Lazy<FactionManager> Lazy_Singleton = new Lazy<FactionManager>(() => new FactionManager());
    // Private constructor prevents direct instantiation
    private FactionManager() { }

    public static FactionManager Singleton
    {
        get
        {
            return Lazy_Singleton.Value;
        }
    }
    #endregion
    
    public List<Faction> factions = new List<Faction>();
    public int AddToFaction(Character chracter)
    {
        Faction new_faction = new Faction();
        factions.Add(new_faction);
        return factions.Count-1; // faction tag is the faction index
    }
}