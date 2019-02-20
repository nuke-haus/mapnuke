using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(MapSpriteSet))]
[CanEditMultipleObjects]
public class SpriteSetEditor: Editor
{
    SerializedProperty sprites;

    void OnEnable()
    {
        sprites = serializedObject.FindProperty("MapSprites");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sprites, new GUIContent("Map Sprite List"), true);

        serializedObject.ApplyModifiedProperties();
    }
}

#endif

/// <summary>
/// Collection of sprite data used in sprite generation.
/// </summary>
[Serializable]
public class MapSpriteSet
{
    public float CullChance;
    public float ProvinceEdgeThreshold;
    public List<ProvinceSprite> MapSprites;

    public ProvinceSprite GetSprite(Terrain flag)
    {
        if (!MapSprites.Any())
        {
            return null;
        }

        List<ProvinceSprite> valid = MapSprites.Where(x => flag.IsFlagSet(x.ValidTerrain) && !x.IsCenterpiece).ToList();

        if (!valid.Any())
        {
            return null;
        }

        ProvinceSprite spr = valid.GetRandom();
        float rand = UnityEngine.Random.Range(0f, 1f);
        int ct = 0;
        int max = MapSprites.Count;

        while ((spr.SpawnChance < rand || !flag.IsFlagSet(spr.ValidTerrain)) && ct < max)
        {
            ct++;

            rand = UnityEngine.Random.Range(0f, 1f);
            spr = valid.GetRandom();
        }

        if (ct == max)
        {
            return null;
        }

        return spr;
    }

    public float GetMaxSize(Terrain flag)
    {
        var valid = MapSprites.Where(x => flag.IsFlagSet(x.ValidTerrain) || x.ValidTerrain == Terrain.PLAINS).ToList();
        float max = -9000f;

        foreach (ProvinceSprite m in valid)
        {
            if (m.Size > max)
            {
                max = m.Size;
            }
        }

        return max;
    }
}

[Serializable]
public class ConnectionSpriteSet
{
    public List<ConnectionSprite> ConnectionSprites;

    public ConnectionSprite GetSprite()
    {
        return ConnectionSprites.GetRandom();
    }
}

