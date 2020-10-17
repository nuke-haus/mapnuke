using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public enum SpawnType
{
    PLAYER,
    THRONE
}

public class SpawnPoint
{
    [XmlElement("X")]
    public int X;

    [XmlElement("Y")]
    public int Y;

    [XmlElement("SpawnType")]
    public SpawnType SpawnType;

    public SpawnPoint(int x, int y, SpawnType s = SpawnType.PLAYER)
    {
        X = x;
        Y = y;
        SpawnType = s;
    }

    public SpawnPoint()
    {
        X = 0;
        Y = 0;
        SpawnType = SpawnType.PLAYER;
    }

    public float DistanceTo(SpawnPoint s)
    {
        var v1 = new Vector2(X, Y);
        var v2 = new Vector2(s.X, s.Y);

        return Vector2.Distance(v1, v2);
    }
}

[XmlRoot]
public class NodeLayoutCollection
{
    [XmlElement("Layout")]
    public List<NodeLayout> Layouts;

    public NodeLayoutCollection()
    {
        Layouts = new List<NodeLayout>();
    }

    public NodeLayoutCollection(List<NodeLayout> list)
    {
        Layouts = list;
    }

    public void Add(NodeLayoutCollection c)
    {
        Layouts.AddRange(c.Layouts);
    }
}

/// <summary>
/// Represents a map layout that determines which coordinates contain player and throne spawns.
/// </summary>
public class NodeLayout
{
    [XmlElement("Name")]
    public string Name;

    [XmlElement("XSize")]
    public int X;

    [XmlElement("YSize")]
    public int Y;

    public int ProvsPerPlayer
    {
        get
        {
            return Mathf.RoundToInt((float)TotalProvinces / NumPlayers);
        }
    }

    public int NumPlayers
    {
        get
        {
            return Spawns.Where(x => x.SpawnType == SpawnType.PLAYER).Count();
        }
    }

    public int NumThrones
    {
        get
        {
            return Spawns.Where(x => x.SpawnType == SpawnType.THRONE).Count();
        }
    }

    public int TotalProvinces
    {
        get
        {
            return X * Y;
        }
    }

    [XmlElement("Spawn")]
    public List<SpawnPoint> Spawns;

    public NodeLayout()
    {
        X = 8;
        Y = 8;
        Spawns = new List<SpawnPoint>();
    }

    public NodeLayout(int x, int y)
    {
        X = x;
        Y = y;
        Spawns = new List<SpawnPoint>();
    }

    public void AddThrone(int x, int y)
    {
        var p = new SpawnPoint(x, y, SpawnType.THRONE);
        Spawns.Add(p);
    }

    public void AddPlayer(int x, int y, int team_num = 0)
    {
        var p = new SpawnPoint(x, y, SpawnType.PLAYER);
        Spawns.Add(p);
    }

    public bool HasSpawn(int px, int py, SpawnType type)
    {
        return Spawns.Any(spawn => spawn.X == px && spawn.Y == py && spawn.SpawnType == type);
    }
}
