using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Collection of all sprite data used in sprite generation. 
/// Edit this sprite data through the unity editor.
/// </summary>
public class SpriteSetCollection : MonoBehaviour
{
    public MapSpriteSet Plains;
    public MapSpriteSet Sea;
    public MapSpriteSet DeepSea;
    public MapSpriteSet Farm;
    public MapSpriteSet Swamp;
    public MapSpriteSet Cave;
    public MapSpriteSet Forest;
    public MapSpriteSet Highland;
    public MapSpriteSet Mountains;
    public MapSpriteSet Waste;

    public ConnectionSpriteSet Road;
    public ConnectionSpriteSet Mountain;
    public ConnectionSpriteSet MountainPass;

    [Obsolete]
    public float GetMaxSize(Terrain flags)
    {
        if (flags.IsFlagSet(Terrain.SEA))
        {
            return Sea.GetMaxSize(flags);
        }
        else if (flags.IsFlagSet(Terrain.DEEPSEA))
        {
            return DeepSea.GetMaxSize(flags);
        }
        else if (flags.IsFlagSet(Terrain.FARM))
        {
            return Farm.GetMaxSize(flags);
        }
        else if (flags.IsFlagSet(Terrain.SWAMP))
        {
            return Swamp.GetMaxSize(flags);
        }
        else if (flags.IsFlagSet(Terrain.CAVE))
        {
            return Cave.GetMaxSize(flags);
        }
        else if (flags.IsFlagSet(Terrain.FOREST))
        {
            return Forest.GetMaxSize(flags);
        }
        else if (flags.IsFlagSet(Terrain.HIGHLAND))
        {
            return Highland.GetMaxSize(flags);
        }
        else if (flags.IsFlagSet(Terrain.MOUNTAINS))
        {
            return Mountains.GetMaxSize(flags);
        }
        else if (flags.IsFlagSet(Terrain.WASTE))
        {
            return Waste.GetMaxSize(flags);
        }
        else
        {
            return Plains.GetMaxSize(flags);
        }
    }

    public float GetCullChance(Terrain flags)
    {
        if (flags.IsFlagSet(Terrain.SEA))
        {
            return Sea.CullChance;
        }
        else if (flags.IsFlagSet(Terrain.DEEPSEA))
        {
            return DeepSea.CullChance;
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
        else if (flags.IsFlagSet(Terrain.DEEPSEA))
        {
            return DeepSea;
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
        else if (flags.IsFlagSet(Terrain.DEEPSEA))
        {
            return DeepSea.GetSprite(flags);
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

        return null;
    }
}