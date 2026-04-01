// using UnityEngine;
// using UnityEditor;
// using ItemStatModules;

// [CustomEditor(typeof(ItemSO))]
// public class ItemSoEditor : Editor
// {
//     // Interface
//     SerializedProperty item_prefab_prop;
//     SerializedProperty ui_image_prop;
//     SerializedProperty item_name_prop;

//     // Input
//     SerializedProperty input_enum_prop;
//     SerializedProperty is_full_auto_prop;

//     // Aiming
//     SerializedProperty dynamic_aim_prop;
//     SerializedProperty rotation_scale_prop;
//     SerializedProperty bonus_range_scalar_prop;

//     // Ammo
//     SerializedProperty has_ammo_prop;
//     SerializedProperty ammo_module_prop;
//     // Ammo
//     SerializedProperty has_ammo_prop;
//     SerializedProperty ammo_module_prop;
//     void OnEnable()
//     {
//         item_prefab_prop = serializedObject.FindProperty("item_prefab");
//         ui_image_prop = serializedObject.FindProperty("ui_image");
//         item_name_prop = serializedObject.FindProperty("item_name");

//         input_enum_prop = serializedObject.FindProperty("input_enum");
//         is_full_auto_prop = serializedObject.FindProperty("is_full_auto");

//         dynamic_aim_prop = serializedObject.FindProperty("dynamic_aim");
//         rotation_scale_prop = serializedObject.FindProperty("rotation_scale");
//         bonus_range_scalar_prop = serializedObject.FindProperty("bonus_range_scalar");

//         has_ammo_prop = serializedObject.FindProperty("has_ammo");
//         ammo_module_prop = serializedObject.FindProperty("ammo_module");

//         has_ammo_prop = serializedObject.FindProperty("has_constant_input");
//         ammo_module_prop = serializedObject.FindProperty("constant_input_module");

//         has_final_input_prop = serializedObject.FindProperty("has_final_input");
//         ammo_module_prop = serializedObject.FindProperty("final_input_module");
//     }

//     public override void OnInspectorGUI()
//     {
//         serializedObject.Update();

//         // Interface
//         EditorGUILayout.LabelField("Interface", EditorStyles.boldLabel);
//         EditorGUILayout.PropertyField(item_prefab_prop, new GUIContent("Prefab"));
//         EditorGUILayout.PropertyField(ui_image_prop, new GUIContent("UI Image"));
//         EditorGUILayout.PropertyField(item_name_prop, new GUIContent("Item Name"));

//         EditorGUILayout.Space();

//         // Input
//         EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
//         EditorGUILayout.PropertyField(input_enum_prop, new GUIContent("Input Type"));
//         EditorGUILayout.PropertyField(is_full_auto_prop, new GUIContent("Is Full Auto"));

//         EditorGUILayout.Space();

//         // Aiming
//         EditorGUILayout.LabelField("Aiming", EditorStyles.boldLabel);
//         EditorGUILayout.PropertyField(dynamic_aim_prop, new GUIContent("Dynamic Aim"));
//         EditorGUILayout.PropertyField(rotation_scale_prop, new GUIContent("Rotation Scale"));
//         EditorGUILayout.PropertyField(bonus_range_scalar_prop, new GUIContent("Bonus Range Scalar"));

//         EditorGUILayout.Space();

//         // Ammo
//         EditorGUILayout.LabelField("Ammo", EditorStyles.boldLabel);
//         EditorGUILayout.PropertyField(has_ammo_prop, new GUIContent("Uses Ammo"));
//         if (has_ammo_prop.boolValue)
//         {
//             EditorGUI.indentLevel++;
//             EditorGUILayout.PropertyField(ammo_module_prop, new GUIContent("Ammo Module"), true);
//             EditorGUI.indentLevel--;
//         }

//         EditorGUILayout.LabelField("Constant Input", EditorStyles.boldLabel);
//         EditorGUILayout.PropertyField(has, new GUIContent("Uses Ammo"));
//         if (has_ammo_prop.boolValue)
//         {
//             EditorGUI.indentLevel++;
//             EditorGUILayout.PropertyField(ammo_module_prop, new GUIContent("Ammo Module"), true);
//             EditorGUI.indentLevel--;
//         }

//         serializedObject.ApplyModifiedProperties();
//     }
// }