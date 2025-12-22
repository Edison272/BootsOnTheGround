using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemSO : ScriptableObject
{   
    public GameObject item_prefab;
    public abstract Item GenerateItem();
}