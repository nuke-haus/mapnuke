using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public struct IntRange
{
    public int Min
    {
        get;
        private set;
    }

    public int Max
    {
        get;
        private set;
    }

    public int Random
    {
        get
        {
            return UnityEngine.Random.Range(Min, Max + 1);
        }
    }

    public IntRange(int min, int max)
    {
        Min = min;
        Max = max;
    }
}

public enum SpawnType
{
    PLAYER,
    THRONE
}

public class SpawnPoint
{
    public readonly IntRange XRange;
    public readonly IntRange YRange;
    public readonly int X;
    public readonly int Y;
    public readonly SpawnType SpawnType;

    [Obsolete]
    public SpawnPoint(IntRange xr, IntRange yr)
    {
        XRange = xr;
        YRange = yr;
        X = xr.Random;
        Y = yr.Random;
    }

    public SpawnPoint(int x, int y, SpawnType st)
    {
        XRange = new IntRange(x, x);
        YRange = new IntRange(y, y);
        X = x;
        Y = y;
        SpawnType = st;
    }
}

/// <summary>
/// Represents a map layout that determines which coordinates contain player and throne spawns.
/// </summary>
public class NodeLayout
{
    public int NumPlayers
    {
        get;
        private set;
    }

    public int NumThrones
    {
        get;
        private set;
    }

    public int X
    {
        get;
        private set;
    }

    public int Y
    {
        get;
        private set;
    }

    public int WaterNations
    {
        get;
        private set;
    }

    public int ProvsPerPlayer
    {
        get;
        private set;
    }

    public int TotalProvinces
    {
        get
        {
            return X * Y;
        }
    }

    public int TotalWaterProvinces
    {
        get;
        private set;
    }

    public List<SpawnPoint> Spawns
    {
        get;
        private set;
    }

    public NodeLayout(int num_players, int num_thrones, int x, int y, int provs_per_player)
    {
        NumPlayers = num_players;
        NumThrones = num_thrones;
        X = x;
        Y = y;
        ProvsPerPlayer = 16;
        Spawns = new List<SpawnPoint>();
    }

    public void AddSpawn(SpawnPoint p)
    {
        Spawns.Add(p);
    }

    public bool HasSpawn(int px, int py, SpawnType type)
    {
        return Spawns.Any(spawn => spawn.X == px && spawn.Y == py && spawn.SpawnType == type);
    }
}
