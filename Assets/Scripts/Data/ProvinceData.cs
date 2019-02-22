using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public Terrain Terrain
    {
        get;
        private set;
    }

    public int ID
    {
        get;
        private set;
    }

    public bool IsWater
    {
        get
        {
            return Terrain.IsFlagSet(Terrain.SEA); // || Terrain.IsFlagSet(Terrain.DEEPSEA);
        }
    }

    public bool IsWaterSwamp
    {
        get
        {
            return Terrain.IsFlagSet(Terrain.SEA) || Terrain.IsFlagSet(Terrain.SWAMP); // || Terrain.IsFlagSet(Terrain.DEEPSEA)
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
        Terrain = Terrain.PLAINS;
        ID = -1;
    }

    public ProvinceData(Terrain t)
    {
        Terrain = t;
        ID = -1;
    }

    public void SetTerrainFlags(Terrain flags)
    {
        Terrain = flags;
    }

    public void AddTerrainFlag(Terrain flag)
    {
        Terrain = Terrain | flag;
    }
}
