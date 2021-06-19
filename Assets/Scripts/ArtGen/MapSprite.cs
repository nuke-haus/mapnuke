using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ProvinceSprite))]
[CanEditMultipleObjects]
public class MapSpriteEditor : Editor
{
    private SerializedProperty sprite;
    private SerializedProperty spawn_chance;
    private SerializedProperty size;
    private SerializedProperty terrain;

    private void OnEnable()
    {
        sprite = serializedObject.FindProperty("Sprite");
        spawn_chance = serializedObject.FindProperty("SpawnChance");
        size = serializedObject.FindProperty("Size");
        terrain = serializedObject.FindProperty("ValidTerrain");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sprite);
        EditorGUILayout.PropertyField(terrain);
        EditorGUILayout.PropertyField(size);
        EditorGUILayout.PropertyField(spawn_chance);

        serializedObject.ApplyModifiedProperties();
    }
}

#endif

/// <summary>
/// Stores valid sprite data for each province type.
/// </summary>
[Serializable]
public class MapSprite
{
    public Sprite Sprite;
    public Sprite WinterSprite;

    public float SpawnChance;
    public float Size;
    public bool CanFlip;
    public bool CanFlipWinter;
    public bool IsCenterpiece;
    public bool PlaceAtLeastOne = true;
    public bool CullSpritePositions = true;

    public List<Color> ValidColors;
    public List<Color> ValidWinterColors;

    public bool Overlaps(Vector3 pos, Vector3 my_pos)
    {
        return Vector3.Distance(my_pos, pos) <= Size;
    }
}

[Serializable]
public class ProvinceSprite : MapSprite
{
    // [CustomEnumFlag] for some reason this doesn't work, thanks unity
    public Terrain ValidTerrain;
}

[Serializable]
public class ConnectionSprite : MapSprite
{

}