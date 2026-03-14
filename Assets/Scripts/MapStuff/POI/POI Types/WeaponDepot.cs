using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class WeaponDepot : MinorPOI
{
    public ItemSO[] weapon_pool;
    public ItemPickup set_item;
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject set_item_instance = Instantiate(set_item, this.transform.position, Quaternion.identity).gameObject;
        set_item_instance.transform.SetParent(this.transform, true);
        set_item_instance.GetComponent<ItemPickup>().SetItem(weapon_pool[Random.Range(0, weapon_pool.Length)]);
    }

}
