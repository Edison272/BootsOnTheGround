using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items", order = 1)]
public class ItemSO : ScriptableObject, ISerializationCallbackReceiver
{   
    public GameObject item_prefab;

    // Classify Input Type
    [field: Header("Input")]
    public InputEnum input_enum;


    public bool is_full_auto = true;

    [field: Header("Aiming")]
    public bool dynamic_aim = true;        // allow dynamic aim for the object to be able to turn to face the target

    [field: Header("Functionality")]
    public Dictionary<string, float> item_stats;
    public ItemEffect[] item_effects = new ItemEffect[1];

    [field: Header("Stat Dictionaries")]
    [SerializeField] StatDictionary serialized_item_stats;
    private InputEnum curr_input;

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
        item_stats = serialized_item_stats.ToDictionary();
        
        // setup input type
        InputType input_type = null;
        switch (input_enum) {
            case InputEnum.Normal:
                input_type = new NormalInput(item_stats["use_speed"], new_item);
                break;
            case InputEnum.Charge:
                input_type = new ChargeInput(item_stats["threshold"], item_stats["max_charge"], item_effects.Length, new_item);
                break;
            case InputEnum.Increment:
                input_type = new IncrementInput(item_stats["use_speed"], item_stats["max_increment"], item_effects.Length, new_item);
                break;
        }
        new_item.Setup(this, input_type);
    }

    #region Scriptable Object Serialization
    public void OnBeforeSerialize()
    {
        if (curr_input != input_enum)
        {
            switch (input_enum)
            {
                case InputEnum.Normal:
                    serialized_item_stats = new StatDictionary
                    {
                        {"reset_speed", 0.1f},
                        {"use_speed", 0.1f}
                    };
                    break;
                case InputEnum.Charge:
                    serialized_item_stats = new StatDictionary
                    {
                        {"reset_speed", 0.1f},
                        {"threshold", 1f},
                        {"max_charge", 1f}
                    };
                    break;
                case InputEnum.Increment:
                    serialized_item_stats = new StatDictionary
                    {
                        {"reset_speed", 0.1f},
                        {"use_speed", 0.1f},
                        {"max_increment", 1f}
                    };
                    break;
            }
            curr_input = input_enum;
        }

    }
    public void OnAfterDeserialize()
    {

    }
    #endregion

}