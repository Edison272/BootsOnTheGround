using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items", order = 1)]
public class ItemSO : ScriptableObject, ISerializationCallbackReceiver
{   
    public GameObject item_prefab;

    // Classify Input Type
    [field: Header("Input")]
    public InputEnum input_enum = InputEnum.Normal;
    public bool is_full_auto = true;

    [field: Header("Aiming")]
    public bool dynamic_aim = true;        // allow dynamic aim for the object to be able to turn to face the target

    [field: Header("Functionality")]
    public FuncEnum func_enum = FuncEnum.Gun;
    public ItemActionEnum action_enum = ItemActionEnum.Projectile;
    public Dictionary<string, float> item_stats;
    public ItemAction[] item_actions = new ItemAction[1];

    [field: Header("Serialization")]
    [SerializeField] StatDictionary serialized_input_stats;
    [SerializeField] StatDictionary serialized_functionality_stats;
    [SerializeField] StatDictionary[] serialized_action_stats;
    private InputEnum curr_input = InputEnum.Increment; // detect when the input type has changed to update it
    private FuncEnum curr_func = FuncEnum.Shield; // detect when the function type has changed to update it
    private ItemActionEnum curr_actions = ItemActionEnum.Projectile; // detect when the function type has changed to update it

    #region Creating the Item
    public Item GenerateItem(Vector3 pos, Quaternion rotation) // summon an item on the ground
    {
        GameObject item_object = MonoBehaviour.Instantiate(item_prefab, pos, rotation);
        Item new_item = item_object.GetComponent<Item>();
        SetupItem(new_item);
        return new_item;
    }
    public Item GenerateItem(Transform holder) // summon an item on a holder
    {
        GameObject item_object = MonoBehaviour.Instantiate(item_prefab, holder);
        Item new_item = item_object.GetComponent<Item>();
        SetupItem(new_item);
        return new_item;
    }
    #endregion

    public void SetupItem(Item new_item) // Setup immutable item stats
    {
        item_stats = serialized_input_stats.ToDictionary();
        foreach(var stat in serialized_functionality_stats.ToDictionary())
        {
            item_stats[stat.Key] = stat.Value; 
        }
        
        // setup input type
        InputType input_type = null;
        switch (input_enum) {
            case InputEnum.Normal:
                input_type = new NormalInput(item_stats["use_speed"], new_item);
                break;
            case InputEnum.Charge:
                input_type = new ChargeInput(item_stats["threshold"], item_stats["max_charge"], item_actions.Length, new_item);
                break;
            case InputEnum.Increment:
                input_type = new IncrementInput(item_stats["use_speed"], item_stats["max_increment"], item_actions.Length, new_item);
                break;
        }

        // setup input type
        FuncModule func_module = null;
        switch (func_enum) {
            case FuncEnum.Gun:
                func_module = new Gun(new_item, item_stats["max_ammo"]);
                break;
            case FuncEnum.Melee:
                func_module = new Melee(new_item); 
                break;
            case FuncEnum.Shield:
                func_module = new Shield(new_item);
                break;
        }

        new_item.Setup(this, input_type, func_module);
    }

    #region Scriptable Object Serialization
    public void OnBeforeSerialize()
    {
        if (curr_input != input_enum)
        {
            switch (input_enum)
            {
                case InputEnum.Normal:
                    serialized_input_stats = new StatDictionary
                    {
                        {"reset_speed", 0.1f},
                        {"use_speed", 0.1f}
                    };
                    break;
                case InputEnum.Charge:
                    serialized_input_stats = new StatDictionary
                    {
                        {"reset_speed", 0.1f},
                        {"threshold", 1f},
                        {"max_charge", 1f}
                    };
                    break;
                case InputEnum.Increment:
                    serialized_input_stats = new StatDictionary
                    {
                        {"reset_speed", 0.1f},
                        {"use_speed", 0.1f},
                        {"max_increment", 1f}
                    };
                    break;
            }
            curr_input = input_enum;
        }
        if (curr_func != func_enum)
        {
            switch (func_enum)
            {
                case FuncEnum.Gun:
                    serialized_functionality_stats = new StatDictionary
                    {
                        {"max_ammo", 30f},
                        {"use_speed", 0.1f}
                    };
                    break;
                case FuncEnum.Melee:
                    serialized_functionality_stats = new StatDictionary
                    {
                        {"reset_speed", 0.1f},
                        {"threshold", 1f},
                        {"max_charge", 1f}
                    };
                    break;
                case FuncEnum.Shield:
                    serialized_functionality_stats = new StatDictionary
                    {
                        {"reset_speed", 0.1f},
                        {"use_speed", 0.1f},
                        {"max_increment", 1f}
                    };
                    break;
            }
            curr_func = func_enum;
        }
        if (curr_actions != action_enum)
            {
                switch (action_enum)
                {
                    case ItemActionEnum.Projectile:
                        serialized_functionality_stats = new StatDictionary
                        {
                            {"max_ammo", 30f},
                            {"use_speed", 0.1f}
                        };
                        break;
                    case ItemActionEnum.Linecast:
                        serialized_functionality_stats = new StatDictionary
                        {
                            {"reset_speed", 0.1f},
                            {"threshold", 1f},
                            {"max_charge", 1f}
                        };
                        break;
                    case ItemActionEnum.MeleeAttack:
                        serialized_functionality_stats = new StatDictionary
                        {
                            {"reset_speed", 0.1f},
                            {"use_speed", 0.1f},
                            {"max_increment", 1f}
                        };
                        break;
                }
                curr_func = func_enum;
            }
    }
    public void OnAfterDeserialize()
    {

    }
    #endregion

}