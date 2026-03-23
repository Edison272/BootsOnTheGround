using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Faction
{
    [SerializeField] private CharacterSO[] enemy_presets;
    public HashSet<Character> enemies = new HashSet<Character>();
    private Dictionary<Character, int> enemy_group = new Dictionary<Character, int>();
    private Queue<int[]> wave_queue = new Queue<int[]>();

    public Faction()
    {
        
    }

}