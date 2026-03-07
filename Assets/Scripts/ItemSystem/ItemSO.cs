using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Items", menuName = "ScriptableObjects/Items", order = 1)]
public class ItemSO : ScriptableObject
{   
    [SerializeField] GameObject item_prefab;
    [field: Header("Interface")]
    public Sprite ui_image;
    // Classify Input Type
    [field: Header("Input")]
    [SerializeField] InputEnum input_enum = InputEnum.Normal;
    public bool is_full_auto = true;

    [field: Header("Aiming")]
    public bool dynamic_aim = true; // allow dynamic aim for the object to be able to turn to face the target
    [Range(0.0f, 1.0f)] public float rot_scale = 1f;    // 0 to 1
    [Range(0, 5)] public int bonus_range_scalar = 1; // how far the user can see with this item DO NOT CHANGE THE RANGE
    [field: Header("Functionality")]
    [SerializeField] FuncEnum func_enum = FuncEnum.Gun;
    public Dictionary<string, float> item_stats {get; private set;}
    public AttackType[] item_attack_types = new AttackType[0];
    public ItemType item_type = ItemType.Weapon;

    [field: Header("Serialization")]
    [SerializeField] private InputEnum curr_input = InputEnum.Increment; // detect when the input type has changed to update it
    [SerializeField] private FuncEnum curr_func = FuncEnum.Shield; // detect when the function type has changed to update it
    [SerializeField] StatDictionary serialized_input_stats;
    [SerializeField] StatDictionary serialized_functionality_stats;
    [SerializeField] AttackTypeInit[] serialized_attacks;


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
        // attack types
        item_attack_types = new AttackType[serialized_attacks.Length];
        int i = 0;
        foreach(AttackTypeInit atk_type_init in serialized_attacks)
        {
            atk_type_init.OnValidate();
            if (atk_type_init.IsDictSetUp())
            {
                item_attack_types[i] = atk_type_init.CreateAttackType();
                i++;
            }
        }

        // setup input type
        InputType input_type = null;
        switch (input_enum) {
            case InputEnum.Normal:
                input_type = new NormalInput(item_stats["use_speed"], new_item);
                break;
            case InputEnum.Charge:
                input_type = new ChargeInput(item_stats["threshold"], item_stats["max_charge"], item_attack_types.Length, new_item);
                break;
            case InputEnum.Increment:
                input_type = new IncrementInput(item_stats["use_speed"], item_stats["max_increment"], item_attack_types.Length, new_item);
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
        new_item.Setup(this, input_type, func_module, item_attack_types);
    }

    #region Scriptable Object Serialization
    private bool ValidateDictionary(StatDictionary check_dict) // return true if the dict is empty or null
    {
        return check_dict == null || check_dict.Length == 0;
    }
    public void OnValidate()
    {
        // input type
        if (curr_input != input_enum || ValidateDictionary(serialized_input_stats))
        {
            if (serialized_input_stats == null)
            {
                serialized_input_stats = new StatDictionary();
            }
            
            serialized_input_stats.Clear();
            switch (input_enum)
            {
                case InputEnum.Normal:
                    serialized_input_stats.Add("reset_speed", 0.1f);
                    serialized_input_stats.Add("use_speed", 0.1f);
                    break;
                case InputEnum.Charge:
                    serialized_input_stats.Add("reset_speed", 0.1f);
                    serialized_input_stats.Add("threshold", 1f);
                    serialized_input_stats.Add("max_charge", 1f);
                    break;
                case InputEnum.Increment:
                    serialized_input_stats.Add("reset_speed", 0.1f);
                    serialized_input_stats.Add("use_speed", 0.1f);
                    serialized_input_stats.Add("max_increment", 1f);
                    break;
            }
            curr_input = input_enum;
        }

        // functionality types
        if (curr_func != func_enum || ValidateDictionary(serialized_functionality_stats))
        {
            if (serialized_functionality_stats == null)
            {
                serialized_functionality_stats = new StatDictionary();
            }

            serialized_functionality_stats.Clear();
            switch (func_enum)
            {
                case FuncEnum.Gun:
                    serialized_functionality_stats.Add("max_ammo", 30f);
                    serialized_functionality_stats.Add("recoil_scalar", 0.1f);
                    break;
                case FuncEnum.Melee:
                    serialized_functionality_stats.Add("combo_length", 0.1f);
                    serialized_functionality_stats.Add("dash_scalar", 1f);
                    break;
                case FuncEnum.Shield:
                    serialized_functionality_stats.Add("reset_speed", 0.1f);
                    serialized_functionality_stats.Add("use_speed", 0.1f);
                    serialized_functionality_stats.Add("max_increment", 1f);
                    break;
            }
            curr_func = func_enum;
        }

        // attack types
        item_attack_types = new AttackType[serialized_attacks.Length];
        int i = 0;
        foreach(AttackTypeInit atk_type_init in serialized_attacks)
        {
            atk_type_init.OnValidate();
            if (atk_type_init.IsDictSetUp())
            {
                item_attack_types[i] = atk_type_init.CreateAttackType();
                i++;
            }
        }
    }
    #endregion

}