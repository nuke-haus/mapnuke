using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(CustomEnumFlagAttribute))]
internal class EnumFlagAttributePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);

        var oldValue = (Enum)fieldInfo.GetValue(property.serializedObject.targetObject);
        var newValue = EditorGUI.EnumFlagsField(position, label, oldValue);

        if (!newValue.Equals(oldValue))
        {
            property.intValue = (int)Convert.ChangeType(newValue, fieldInfo.FieldType);
        }

        EditorGUI.EndProperty();
    }
}

public class CustomEnumFlagAttribute : PropertyAttribute
{
}

#endif