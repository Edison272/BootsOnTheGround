using UnityEditor;
using UnityEngine;
public class ShowIfAnyAttribute : PropertyAttribute
{
    public string condition_field;
    public object[] condition_values;

    public ShowIfAnyAttribute(string condition_field, params object[] condition_values)
    {
        this.condition_field = condition_field;
        this.condition_values = condition_values;
    }
}
[CustomPropertyDrawer(typeof(ShowIfAnyAttribute))]
public class ShowIfAnyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAnyAttribute show_if = (ShowIfAnyAttribute)attribute;
        string condition_path = property.propertyPath.Replace(property.name, show_if.condition_field);
        SerializedProperty condition_prop = property.serializedObject.FindProperty(condition_path);

        if (condition_prop != null && IsAnyConditionMet(condition_prop, show_if.condition_values))
            EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAnyAttribute show_if = (ShowIfAnyAttribute)attribute;
        string condition_path = property.propertyPath.Replace(property.name, show_if.condition_field);
        SerializedProperty condition_prop = property.serializedObject.FindProperty(condition_path);

        if (condition_prop != null && IsAnyConditionMet(condition_prop, show_if.condition_values))
            return EditorGUI.GetPropertyHeight(property, label, true) + 2;

        return 0;
    }

    bool IsAnyConditionMet(SerializedProperty prop, object[] values)
    {
        foreach (object value in values)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    if (prop.boolValue == (bool)value) return true;
                    break;
                case SerializedPropertyType.Integer:
                    if (prop.intValue == (int)value) return true;
                    break;
                case SerializedPropertyType.Enum:
                    if (prop.enumValueIndex == (int)value) return true;
                    break;
            }
        }
        return false;
    }
}