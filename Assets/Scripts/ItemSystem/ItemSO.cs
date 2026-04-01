using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using ItemStatModules;

[CreateAssetMenu(fileName = "Items", menuName = "ScriptableObjects/Items", order = 1)]
public class ItemSO : ScriptableObject
{   
    [SerializeField] GameObject item_prefab;
    [field: Header("Interface")]
    public Sprite ui_image;
    public string item_name = "";
    // Classify Input Type
    [field: Header("Input")]
    [SerializeField] InputEnum input_enum = InputEnum.Normal;
    public bool is_full_auto = true;

    [field: Header("Aiming")]
    public bool dynamic_aim = true; // allow dynamic aim for the object to be able to turn to face the target
    [Range(0.0f, 1.0f)] public float rotation_scale = 1f;    // 0 to 1
    [Range(0, 5)] public int bonus_range_scalar = 1; // how far the user can see with this item DO NOT CHANGE THE RANGE
    [field: Header("Functionality")]
    [SerializeField] FuncEnum func_enum = FuncEnum.Gun;
    public Dictionary<string, float> item_stats {get; private set;} = new Dictionary<string, float>();
    public AttackType[] item_attack_types = new AttackType[0];
    public ItemType item_type = ItemType.Weapon;

    [Header("Ammo Module")]
    public bool has_ammo;
    [ShowIf("has_ammo")] public AmmoModule ammo_module = new AmmoModule();

    [HideInInspector] public bool has_recoil;

    [HideInInspector] public bool has_end_effect;

    [Header("Input Modules")]
    public bool has_constant_input = true;
    [ShowIf("has_constant_input")] public SerializeConstantInput constant_input_module = new SerializeConstantInput();
    public bool has_final_input = false;
    [ShowIf("has_final_input")] public SerializeFinalInput final_input_module = new SerializeFinalInput();
    

    [field: Header("Serialization")]
    private InputEnum curr_input = InputEnum.Increment; // detect when the input type has changed to update it
    private FuncEnum curr_func = FuncEnum.Shield; // detect when the function type has changed to update it
    [SerializeField] StatDictionary serialized_input_stats = new StatDictionary();
    [SerializeField] StatDictionary serialized_functionality_stats = new StatDictionary();
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
        ItemInputController input_controller = new ItemInputController(new_item);
        if (has_constant_input)
        {
            constant_input_module.AddInputModules(input_controller);
        }
        if (has_final_input)
        {
            final_input_module.AddInputModules(input_controller);
        }


        // setup input type
        FuncModule func_module = null;
        switch (func_enum) {
            case FuncEnum.Gun:
                func_module = new Gun(new_item, item_stats["max_ammo"], item_stats["recoil_increment"], item_stats["recoil_multiplier"], item_stats["recoil_max_dist_ratio"], item_stats["recoil_recovery"]);
                break;
            case FuncEnum.Melee:
                func_module = new Melee(new_item); 
                break;
            case FuncEnum.Shield:
                func_module = new Shield(new_item);
                break;
        }
        new_item.Setup(this, input_controller, func_module, item_attack_types);
    }

    #region Scriptable Object Serialization
    private bool ValidateDictionary(StatDictionary check_dict) // return true if the dict is empty or null
    {
        return check_dict == null || check_dict.Length == 0;
    }
    public void OnValidate()
    {
        if (item_name == "")
        {
            item_name = item_prefab.name;
        }
        
        // input type
        if (curr_input != input_enum || ValidateDictionary(serialized_input_stats))
        {            
            // save previous data in the item_stats dictionary
            foreach(StatDictItem stat in serialized_input_stats)
            {
                item_stats[stat.key] = stat.value;
            }
            
            // regenerate serialization, and load data from stat list when necessary
            serialized_input_stats.Clear();
            switch (input_enum)
            {
                case InputEnum.Normal:
                    serialized_input_stats.Add("reset_speed", 0.1f);
                    serialized_input_stats.Add("equip_speed", 0.1f);
                    serialized_input_stats.Add("use_speed", 0.1f);
                    int s = 0;
                    foreach(StatDictItem stat in Gun.GunFuncStats)
                    {
                        serialized_functionality_stats.Add(stat.key, item_stats.ContainsKey(stat.key) ? item_stats[stat.key] : Gun.GunFuncStats[s].value);
                        s++;
                    }
                    break;
                case InputEnum.Charge:
                    serialized_input_stats.Add("reset_speed", 0.1f);
                    serialized_input_stats.Add("equip_speed", 0.1f);
                    serialized_input_stats.Add("threshold", 1f);
                    serialized_input_stats.Add("max_charge", 1f);
                    break;
                case InputEnum.Increment:
                    serialized_input_stats.Add("reset_speed", 0.1f);
                    serialized_input_stats.Add("equip_speed", 0.1f);
                    serialized_input_stats.Add("use_speed", 0.1f);
                    serialized_input_stats.Add("max_increment", 1f);
                    break;
            }
            curr_input = input_enum;
        }

        // functionality types
        if (curr_func != func_enum || ValidateDictionary(serialized_functionality_stats))
        {
            // save previous data in the item_stats dictionary
            foreach(StatDictItem stat in serialized_functionality_stats)
            {
                item_stats[stat.key] = stat.value;
            }
            
            // regenerate serialization, and load data from stat list when necessary
            serialized_functionality_stats.Clear();
            switch (func_enum)
            {
                case FuncEnum.Gun:
                    int s = 0;
                    foreach(StatDictItem stat in Gun.GunFuncStats)
                    {
                        serialized_functionality_stats.Add(stat.key, item_stats.ContainsKey(stat.key) ? item_stats[stat.key] : Gun.GunFuncStats[s].value);
                        s++;
                    }
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

