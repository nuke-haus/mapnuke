using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum SpawnType
{
    PLAYER,
    THRONE
}

public class SpawnPoint
{
    public readonly int TeamNum = -1;
    public readonly int X;
    public readonly int Y;
    public readonly SpawnType SpawnType;

    /// <summary>
    /// Throne constructor
    /// </summary>
    public SpawnPoint(int x, int y)
    {
        X = x;
        Y = y;
        SpawnType = SpawnType.THRONE;
    }

    /// <summary>
    /// Player constructor
    /// </summary>
    public SpawnPoint(int x, int y, int team_num)
    {
        X = x;
        Y = y;
        TeamNum = team_num;
        SpawnType = SpawnType.PLAYER;
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

    public void AddThrone(int x, int y)
    {
        SpawnPoint p = new SpawnPoint(x, y);
        Spawns.Add(p);
    }

    public void AddPlayer(int x, int y, int team_num = 0)
    {
        SpawnPoint p = new SpawnPoint(x, y, team_num);
        Spawns.Add(p);
    }

    public bool HasSpawn(int px, int py, SpawnType type)
    {
        return Spawns.Any(spawn => spawn.X == px && spawn.Y == py && spawn.SpawnType == type);
    }
}
