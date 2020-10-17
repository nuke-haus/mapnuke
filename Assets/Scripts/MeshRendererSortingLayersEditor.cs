using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

//Expose SortingLayer  SortingOrder on MeshRenderer
//With nice drop down and revert to prefab functionality.

//Base exposing code by neror http://forum.unity3d.com/threads/212006-Drawing-order-of-Meshes-and-Sprites
//Get all sorting layer name and ID by guavaman  Ivan.Murashko http://answers.unity3d.com/questions/585108/how-do-you-access-sorting-layers-via-scripting.html
//Sorting Layer drop down menu, bold text on prefab override, revert to prefab and instant update on Order change functionality by 5argon

[CustomEditor(typeof(MeshRenderer))]
public class MeshRendererSortingLayersEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        var sortingLayerID = serializedObject.FindProperty("m_SortingLayerID");
        var sortingOrder = serializedObject.FindProperty("m_SortingOrder");

        var renderer = target as MeshRenderer;

        var firstHoriz = EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();

        EditorGUI.BeginProperty(firstHoriz, GUIContent.none, sortingLayerID);

        var layerNames = GetSortingLayerNames();
        var layerID = GetSortingLayerUniqueIDs();

        var selected = -1;
        //What is selected?
        var sID = sortingLayerID.intValue;
        for (var i = 0; i < layerID.Length; i++)
        {
            //Debug.Log(sID + " " + layerID[i]);
            if (sID == layerID[i])
            {
                selected = i;
            }
        }

        if (selected == -1)
        {
            //Select Default.
            for (var i = 0; i < layerID.Length; i++)
            {
                if (layerID[i] == 0)
                {
                    selected = i;
                }
            }
        }

        selected = EditorGUILayout.Popup("Sorting Layer", selected, layerNames);

        //Translate to ID
        sortingLayerID.intValue = layerID[selected];


        EditorGUI.EndProperty();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(sortingOrder, new GUIContent("Order in Layer"));


        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
    }

    public string[] GetSortingLayerNames()
    {
        var internalEditorUtilityType = typeof(InternalEditorUtility);
        var sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        return (string[])sortingLayersProperty.GetValue(null, new object[0]);
    }

    public int[] GetSortingLayerUniqueIDs()
    {
        var internalEditorUtilityType = typeof(InternalEditorUtility);
        var sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
        return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
    }
}

#endif