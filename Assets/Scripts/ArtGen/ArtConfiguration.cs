using System;
using UnityEngine;

/// <summary>
/// Collection of all sprite data used in sprite generation. 
/// Edit this sprite data through the unity editor.
/// </summary>
public class ArtConfiguration : MonoBehaviour
{
    [Header("Parameters")]
    public string ArtConfigurationName;
    [Range(0.1f, 2.0f)]
    public float MinimumMountainPassGapSize = 0.5f;
    [Range(0.01f, 0.2f)]
    public float MinimumRiverWidth = 0.02f;
    [Range(0.01f, 0.2f)]
    public float MaximumRiverWidth = 0.08f;
    [Range(0.01f, 0.16f)]
    public float MinimumRiverShoreWidth = 0.03f;
    [Range(0.01f, 0.16f)]
    public float MaximumRiverShoreWidth = 0.06f;
    [Range(0.01f, 0.5f)]
    public float MinimumSeaShoreWidth = 0.1f;
    [Range(0.01f, 0.5f)]
    public float MaximumSeaShoreWidth = 0.2f;
    [Range(0.01f, 0.8f)]
    public float MinimumRoadWidth = 0.30f;
    [Range(0.01f, 0.8f)]
    public float MaximumRoadWidth = 0.50f;
    [Range(0f, 0.4f)]
    public float RoadShortening = 0.2f;
    [Range(0f, 0.12f)]
    public float RoadMaximumJitter = 0.09f;
    [Range(0f, 0.32f)]
    public float ProvinceBorderMaximumJitter = 0.16f;
    [Range(1f, 4f)]
    public float ProvinceSizeX = 2.56f;
    [Range(1f, 4f)]
    public float ProvinceSizeY = 1.60f;

    [Header("Province Sprite Sets")]
    public MapSpriteSet Plains;
    public MapSpriteSet Sea;
    public MapSpriteSet Farm;
    public MapSpriteSet Swamp;
    public MapSpriteSet Cave;
    public MapSpriteSet Forest;
    public MapSpriteSet Highland;
    public MapSpriteSet Mountains;
    public MapSpriteSet Waste;

    [Header("Connection Sprite Sets")]
    public ConnectionSpriteSet Road;
    public ConnectionSpriteSet Mountain;
    public ConnectionSpriteSet MountainPass;
    public ConnectionSpriteSet RiverHorizontal;
    public ConnectionSpriteSet RiverVertical;

    [Header("Summer Province Mesh Materials")]
    public Material MatSwamp;
    public Material MatForest;
    public Material MatWaste;
    public Material MatMountain;
    public Material MatHighland;
    public Material MatCave;
    public Material MatFarm;
    public Material MatPlains;
    public Material MatSea;
    public Material MatDeepSea;
    public Material MatShore;

    [Header("Winter Province Mesh Materials")]
    public Material MatWinterSwamp;
    public Material MatWinterForest;
    public Material MatWinterWaste;
    public Material MatWinterMountain;
    public Material MatWinterHighland;
    public Material MatWinterCave;
    public Material MatWinterFarm;
    public Material MatWinterPlains;
    public Material MatWinterSea;
    public Material MatWinterDeepSea;
    public Material MatWinterShore;

    [Header("Summer Connection Mesh Materials")]
    public Material MatRiver;
    public Material MatDeepRiver;
    public Material MatRiverShore;
    public Material MatRoad;

    [Header("Winter Connection Mesh Materials")]
    public Material MatWinterRiver;
    public Material MatWinterDeepRiver;
    public Material MatWinterRiverShore;
    public Material MatWinterRoad;

    public float GetCullChance(Terrain flags)
    {
        if (flags.IsFlagSet(Terrain.SEA))
        {
            return Sea.CullChance;
        }
        else if (flags.IsFlagSet(Terrain.FARM))
        {
            return Farm.CullChance;
        }
        else if (flags.IsFlagSet(Terrain.SWAMP))
        {
            return Swamp.CullChance;
        }
        else if (flags.IsFlagSet(Terrain.CAVE))
        {
            return Cave.CullChance;
        }
        else if (flags.IsFlagSet(Terrain.FOREST))
        {
            return Forest.CullChance;
        }
        else if (flags.IsFlagSet(Terrain.HIGHLAND))
        {
            return Highland.CullChance;
        }
        else if (flags.IsFlagSet(Terrain.MOUNTAINS))
        {
            return Mountains.CullChance;
        }
        else if (flags.IsFlagSet(Terrain.WASTE))
        {
            return Waste.CullChance;
        }
        else // plains
        {
            return Plains.CullChance;
        }
    }

    public MapSpriteSet GetMapSpriteSet(Terrain flags)
    {
        if (flags.IsFlagSet(Terrain.SEA))
        {
            return Sea;
        }
        else if (flags.IsFlagSet(Terrain.FARM))
        {
            return Farm;
        }
        else if (flags.IsFlagSet(Terrain.SWAMP))
        {
            return Swamp;
        }
        else if (flags.IsFlagSet(Terrain.CAVE))
        {
            return Cave;
        }
        else if (flags.IsFlagSet(Terrain.FOREST))
        {
            return Forest;
        }
        else if (flags.IsFlagSet(Terrain.HIGHLAND))
        {
            return Highland;
        }
        else if (flags.IsFlagSet(Terrain.MOUNTAINS))
        {
            return Mountains;
        }
        else if (flags.IsFlagSet(Terrain.WASTE))
        {
            return Waste;
        }
        else
        {
            return Plains;
        }
    }

    public ProvinceSprite GetMapSprite(Terrain flags)
    {
        if (flags.IsFlagSet(Terrain.SEA))
        {
            return Sea.GetSprite(flags);
        }
        else if (flags.IsFlagSet(Terrain.FARM))
        {
            return Farm.GetSprite(flags);
        }
        else if (flags.IsFlagSet(Terrain.SWAMP))
        {
            return Swamp.GetSprite(flags);
        }
        else if (flags.IsFlagSet(Terrain.CAVE))
        {
            return Cave.GetSprite(flags);
        }
        else if (flags.IsFlagSet(Terrain.FOREST))
        {
            return Forest.GetSprite(flags);
        }
        else if (flags.IsFlagSet(Terrain.HIGHLAND))
        {
            return Highland.GetSprite(flags);
        }
        else if (flags.IsFlagSet(Terrain.MOUNTAINS))
        {
            return Mountains.GetSprite(flags);
        }
        else if (flags.IsFlagSet(Terrain.WASTE))
        {
            return Waste.GetSprite(flags);
        }
        else
        {
            return Plains.GetSprite(flags);
        }
    }

    public ConnectionSprite GetConnectionSprite(ConnectionType t)
    {
        if (t == ConnectionType.ROAD)
        {
            return Road.GetSprite();
        }
        else if (t == ConnectionType.MOUNTAIN)
        {
            return Mountain.GetSprite();
        }
        else if (t == ConnectionType.MOUNTAINPASS)
        {
            return MountainPass.GetSprite();
        }
        else if (t == ConnectionType.SHALLOWRIVER)
        {
            return RiverHorizontal.GetSprite();
        }
        else if (t == ConnectionType.RIVER)
        {
            return RiverVertical.GetSprite();
        }

        return null;
    }
}
