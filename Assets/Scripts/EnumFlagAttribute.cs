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

        var oldValue = (Terrain)property.longValue; 
        var newValue = (Terrain)EditorGUI.EnumFlagsField(position, label, oldValue);

        if (!newValue.Equals(oldValue))
        {
            var longVal = (long)newValue;
            property.longValue = longVal;
        }

        EditorGUI.EndProperty();
    }
}

public class CustomEnumFlagAttribute : PropertyAttribute
{
}

#endif