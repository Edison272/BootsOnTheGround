using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CharStatModifierType {None = -1, HealthBoost, RegenRate, HealthRegen, ShieldBoost, SpeedScale}
[Serializable]
public class CharStatModifier
{
    [field: SerializeField] public CharStatModifierType stat_modifier_enum {get; private set;}
    [field: SerializeField] public float modifier_value {get; private set;}
    public CharStatModifier(CharStatModifierType key, float value)
    {
        stat_modifier_enum = key;
        modifier_value = value;
    }
    public void ApplyStat(Character target, float stat_duration)
    {
        switch (stat_modifier_enum)
        {
            case CharStatModifierType.HealthBoost:
                target.MaxHealthBoost((int)modifier_value, stat_duration);
                break;
            case CharStatModifierType.RegenRate:
                // handle reget rate if there's actually any health regen
                break;
            case CharStatModifierType.HealthRegen:
                target.ChangeHealthTick((int)modifier_value, stat_duration, 1);
                break;
            case CharStatModifierType.ShieldBoost:
                target.ShieldBoost((int)modifier_value);
                break;
            case CharStatModifierType.SpeedScale:
                target.ChangeSpeed(modifier_value, stat_duration);
                break;
        }
    }
}

[Serializable]
public class CharStatModDictionary : IEnumerable<CharStatModifier>
{
    [SerializeField] List<CharStatModifier> stat_list = new();
    public void ApplyStatsToCharacter(Character target, float duration)
    {
        foreach(CharStatModifier stat_mod in stat_list)
        {
            stat_mod.ApplyStat(target, duration);
        }
    }

    public void Add(CharStatModifierType key, float value)
    {
        stat_list.Add(new CharStatModifier(key, value));
    }
    public void Insert(int index, CharStatModifierType key, float value = 0)
    {
        stat_list.Insert(index, new CharStatModifier(key, value));
    }
    public CharStatModifierType Key(int index)
    {
        return index >= stat_list.Count ? CharStatModifierType.None : stat_list[index].stat_modifier_enum;
    }
    public float Value(int index)
    {
        return index >= stat_list.Count ? 0 : stat_list[index].modifier_value;
    }
    public void RemoveAt(int index)
    {
        if (index > 0 && index < stat_list.Count)
        {
            stat_list.RemoveAt(index);   
        }
    }
    public void RemoveNonMatching(CharStatModifierType[] match)
    {
        stat_list.RemoveAll(
            stat => !match.Contains(stat.stat_modifier_enum)
        );
    }
    public void Clear()
    {
        stat_list.Clear();
    }

    public bool Contains(CharStatModifierType key)
    {
        foreach(CharStatModifier item in stat_list)
        {
            if (item.stat_modifier_enum == key)
            {
                return true;
            }
        }
        return false;
    }
    public int Length => stat_list.Count;

    public Dictionary<CharStatModifierType, float> ToDictionary()
    {
        Dictionary<CharStatModifierType, float> new_dict = new Dictionary<CharStatModifierType, float>();
        foreach(CharStatModifier stat in stat_list) 
        {
            new_dict[stat.stat_modifier_enum] = stat.modifier_value;
        }
        return new_dict;
    }
    
    public IEnumerator<CharStatModifier> GetEnumerator()
    {
        return stat_list.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}