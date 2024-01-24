using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Conceptual class for a province.
/// Keeps track of adjacencies, connections and other relevant data.
/// </summary>
public class Node
{
    public PlayerData Nation
    {
        get;
        private set;
    }

    public bool HasNation
    {
        get
        {
            return Nation != null;
        }
    }

    public bool IsCapRing
    {
        get;
        private set;
    }

    public bool IsAssignedTerrain
    {
        get;
        private set;
    }

    public bool IsWrapCorner
    {
        get;
        private set;
    }

    public int ID
    {
        get
        {
            return ProvinceData.ID;
        }
        private set
        {
            ProvinceData.SetID(value);
        }
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

    public Vector2 Vector
    {
        get
        {
            return new Vector2(X, Y);
        }
    }

    public List<Connection> Connections
    {
        get;
        private set;
    }

    public ProvinceData ProvinceData
    {
        get;
        private set;
    }

    public ProvinceData TempCaveProvinceData
    {
        get;
        private set;
    }

    public ProvinceData LockedProvinceData
    {
        get;
        private set;
    }

    public List<Node> ConnectedNodes
    {
        get
        {
            var result = new List<Node>();

            foreach (var c in Connections)
            {
                if (c.Node1 == this)
                {
                    result.Add(c.Node2);
                }
                else
                {
                    result.Add(c.Node1);
                }
            }

            return result;
        }
    }

    public bool HasOnlyStandardConnections
    {
        get
        {
            if (ProvinceData.IsWater)
            {
                return false;
            }

            foreach (var c in Connections)
            {
                if (c.ConnectionType != ConnectionType.STANDARD)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public bool HasMostlyStandardConnections
    {
        get
        {
            if (ProvinceData.IsWater)
            {
                return false;
            }

            var bad = 0;
            var good = 0;

            foreach (var c in Connections)
            {
                if (c.ConnectionType == ConnectionType.STANDARD)
                {
                    good++;
                }
                else
                {
                    bad++;
                }
            }

            return good > bad;
        }
    }

    public int NumStandardConnections
    {
        get
        {
            var num = 0;

            foreach (var c in Connections)
            {
                if (c.ConnectionType == ConnectionType.STANDARD)
                {
                    if (c.Node1 == this)
                    {
                        if (c.Node2.ProvinceData.Terrain.IsFlagSet(Terrain.SEA))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (c.Node1.ProvinceData.Terrain.IsFlagSet(Terrain.SEA))
                        {
                            continue;
                        }
                    }

                    num++;
                }
            }

            return num;
        }
    }

    public Node(int x, int y, ProvinceData pd)
    {
        Connections = new List<Connection>();
        X = x;
        Y = y;
        ProvinceData = pd;
        TempCaveProvinceData = pd.Clone();
        IsCapRing = false;
        IsAssignedTerrain = false;
        IsWrapCorner = false;
    }

    public Node(int x, int y, ProvinceData pd, PlayerData n) : this(x, y, pd)
    {
        Nation = n;
    }

    public void LockProvinceData(bool is_locked)
    {
        if (is_locked)
        {
            LockedProvinceData = ProvinceData.Clone();
            TempCaveProvinceData = ProvinceData.Clone();
        }
        else
        {
            ProvinceData = LockedProvinceData.Clone();
            ProvinceData.SetCaveTerrainFlags(TempCaveProvinceData.CaveTerrain);
        }
    }

    public bool IsTouchingEnemyCapRing(Node ignore_node)
    {
        var ignore_nodes = ignore_node.ConnectedNodes;
        return ConnectedNodes.Any(x => x.IsCapRing && !ignore_nodes.Contains(x) && !x.ProvinceData.IsWater);
    }

    public void SetPlayerInfo(PlayerData n, ProvinceData pd)
    {
        Nation = n;
        ProvinceData = pd;
    }

    public List<Node> GetConnectedProvincesOfType(Terrain t, bool recursive = false)
    {
        var nodes = new List<Node>();

        foreach (var n in ConnectedNodes)
        {
            if (n.ProvinceData.Terrain.IsFlagSet(t))
            {
                if (recursive)
                {
                    var connected = n.GetConnectedProvincesOfType(t).Where(x => !nodes.Contains(x));

                    nodes.Add(n);
                    nodes.AddRange(connected);
                }
                else
                {
                    nodes.Add(n);
                }
            }
        }

        return nodes;
    }

    public float DistanceTo(Node n)
    {
        return Vector2.Distance(n.Vector, Vector); // Mathf.Sqrt(Mathf.Pow((n.X - this.X), 2) + Mathf.Pow(n.Y - this.Y, 2));
    }

    public bool HasConnection(Node n)
    {
        return Connections.Any(x => (x.Node1 == n && x.Node1 != this) || (x.Node2 == n && x.Node2 != this));
    }

    public Connection GetConnectionTo(Node n)
    {
        return Connections.FirstOrDefault(x => x.Node2 == n || x.Node1 == n);
    }

    public void SetID(int id)
    {
        ID = id;
    }

    public void AddConnection(Connection c)
    {
        Connections.Add(c);
    }

    public void AddConnection(Node n, ConnectionType t)
    {
        Connections.Add(new Connection(this, n, t));
    }

    public void SetWrapCorner(bool b)
    {
        IsWrapCorner = b;
    }

    public void SetCapRing(bool cap)
    {
        IsCapRing = cap;
    }

    public void SetAssignedTerrain(bool b)
    {
        IsAssignedTerrain = b;
    }
}
