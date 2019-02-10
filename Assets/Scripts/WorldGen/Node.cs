using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Conceptual class for a province.
/// Keeps track of adjacencies, connections and other relevant data.
/// </summary>
public class Node
{ 
    public NationData Nation
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

    public List<Node> ConnectedNodes
    {
        get
        {
            List<Node> result = new List<Node>();

            foreach (Connection c in Connections)
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

    public bool Untouched
    {
        get
        {
            if (ProvinceData.IsWater)
            {
                return false;
            }

            foreach (Connection c in Connections)
            {
                if (c.ConnectionType != ConnectionType.STANDARD)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public bool SemiUntouched
    {
        get
        {
            if (ProvinceData.IsWater)
            {
                return false;
            }

            int bad = 0;
            int good = 0;

            foreach (Connection c in Connections)
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

    public Node(int x, int y, ProvinceData pd)
    {
        Connections = new List<Connection>();
        X = x;
        Y = y;
        ProvinceData = pd;
        IsCapRing = false;
        IsAssignedTerrain = false;
        IsWrapCorner = false;
    }

    public Node(int x, int y, ProvinceData pd, NationData n) : this(x, y, pd)
    {
        Nation = n;
    }

    public void SetPlayerInfo(NationData n, ProvinceData pd)
    {
        Nation = n;
        ProvinceData = pd;
    }

    public float DistanceTo(Node n)
    {
        return Mathf.Sqrt(Mathf.Pow((n.X - this.X), 2) + Mathf.Pow(n.Y - this.Y, 2));
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
