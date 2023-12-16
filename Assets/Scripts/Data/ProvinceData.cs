using System;

[Flags]
public enum Terrain
{
    PLAINS = 0,
    SMALLPROV = 1,
    LARGEPROV = 2,
    SEA = 4,
    FRESHWATER = 8,
    HIGHLAND = 16,
    SWAMP = 32,
    WASTE = 64,
    FOREST = 128,
    FARM = 256,
    NOSTART = 512,
    MANYSITES = 1024,
    DEEPSEA = 2048,
    CAVE = 4096,
    MOUNTAINS = 4194304,
    THRONE = 16777216,
    START = 33554432,
    NOTHRONE = 67108864,
    WARMER = 536870912,
    COLDER = 1073741824
}

/// <summary>
/// Keeps track of terrain data and province ID.
/// Each Node has a ProvinceData.
/// </summary>
public class ProvinceData
{
    // Dom6 has a new cave layer we need to support for those maps, it doesn't fit into the Terrain enum though 
    public static readonly long Dom6CaveWall = 68719476736;
    public static readonly long Dom6Cave = 576460752303423488;

    public Terrain Terrain
    {
        get;
        private set;
    }

    public Terrain CaveTerrain
    {
        get;
        private set;
    }

    public int ID
    {
        get;
        private set;
    }

    public string CustomName 
    {
        get;
        private set;
    }

    public bool HasCaveEntrance
    {
        get;
        private set;
    }

    public bool IsCaveWall
    {
        get;
        private set;
    }

    public bool IsWater
    {
        get
        {
            return Terrain.IsFlagSet(Terrain.SEA);
        }
    }

    public bool IsWaterSwamp
    {
        get
        {
            return Terrain.IsFlagSet(Terrain.SEA) || Terrain.IsFlagSet(Terrain.SWAMP);
        }
    }

    public bool IsPlains
    {
        get
        {
            return !IsWaterSwamp &&
                !Terrain.IsFlagSet(Terrain.HIGHLAND) &&
                !Terrain.IsFlagSet(Terrain.WASTE) &&
                !Terrain.IsFlagSet(Terrain.FOREST) &&
                !Terrain.IsFlagSet(Terrain.FARM) &&
                !Terrain.IsFlagSet(Terrain.CAVE) &&
                !Terrain.IsFlagSet(Terrain.MOUNTAINS);
        }
    }

    public bool IsThrone
    {
        get
        {
            return Terrain.IsFlagSet(Terrain.THRONE);
        }
    }

    public void SetID(int id)
    {
        ID = id;
    }

    public ProvinceData()
    {
        CaveTerrain = Terrain.PLAINS;
        Terrain = Terrain.PLAINS;
        ID = -1;
        CustomName = string.Empty;
    }

    public ProvinceData(Terrain t)
    {
        Terrain = t;
        ID = -1;
        CustomName = string.Empty;
    }

    public void SetHasCaveEntrance(bool has_entrance)
    {
        HasCaveEntrance = has_entrance;
    }

    public void SetIsCaveWall(bool is_cave_wall)
    {
        IsCaveWall = is_cave_wall;  
    }

    public void SetCustomName(string name)
    {
        CustomName = name.Trim();
    }

    public void SetCaveTerrainFlags(Terrain flags)
    {
        CaveTerrain = flags;
    }

    public void SetTerrainFlags(Terrain flags)
    {
        Terrain = flags;
    }

    public void AddCaveTerrainFlag(Terrain flag)
    {
        CaveTerrain = CaveTerrain | flag;
    }

    public void AddTerrainFlag(Terrain flag)
    {
        Terrain = Terrain | flag;
    }

    public ProvinceData Clone()
    {
        ProvinceData clone = new ProvinceData();
        clone.SetCaveTerrainFlags(CaveTerrain);
        clone.SetTerrainFlags(Terrain);
        clone.SetCustomName(CustomName);
        clone.SetID(ID);
        clone.SetIsCaveWall(IsCaveWall);
        clone.SetHasCaveEntrance(HasCaveEntrance);
        return clone;
    }
}
