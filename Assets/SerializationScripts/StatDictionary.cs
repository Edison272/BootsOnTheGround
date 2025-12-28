using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StatDictItem
{
    public string key;
    public float value;
    public StatDictItem(string key, float value)
    {
        this.key = key;
        this.value = value;
    }
}

[System.Serializable]
public class StatDictionary : IEnumerable<StatDictItem>
{
    [SerializeField] List<StatDictItem> stat_array = new List<StatDictItem>();

    public void Add(string key, float value)
    {
        stat_array.Add(new StatDictItem(key, value));
    }

    public Dictionary<string, float> ToDictionary()
    {
        Dictionary<string, float> new_dict = new Dictionary<string, float>();
        foreach(StatDictItem stat in stat_array) 
        {
            new_dict[stat.key] = stat.value;
        }
        return new_dict;
    }
    
    public IEnumerator<StatDictItem> GetEnumerator()
    {
        return stat_array.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}