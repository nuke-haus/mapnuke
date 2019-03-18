using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Flags]
public enum ConnectionType
{
    STANDARD = 0,
    MOUNTAINPASS = 1,
    RIVER = 2,
    MOUNTAIN = 4,
    ROAD = 8,
    SHALLOWRIVER = 16 // this is not an official enum but we need it 
}

/// <summary>
/// Conceptual class for connections. 
/// Keeps track of connection data, neighboring connections and connected nodes.
/// </summary>
public class Connection
{
    public ConnectionType ConnectionType
    {
        get;
        private set;
    }

    public bool Diagonal
    {
        get;
        private set;
    }

    public Node Node1
    {
        get;
        private set;
    }

    public Node Node2
    {
        get;
        private set;
    }

    public Vector2 Pos
    {
        get;
        private set;
    }

    public List<Connection> Adjacent
    {
        get;
        private set;
    }

    public List<Connection> TriangleLinked
    {
        get;
        private set;
    }

    public bool IsSeaConnection
    {
        get
        {
            return Node1.ProvinceData.IsWater && Node2.ProvinceData.IsWater;
        }
    }

    public bool IsTouchingSea
    {
        get
        {
            return Node1.ProvinceData.IsWater || Node2.ProvinceData.IsWater;
        }
    }

    public bool IsTouchingSeaOrSwamp
    {
        get
        {
            return Node1.ProvinceData.IsWaterSwamp || Node2.ProvinceData.IsWaterSwamp;
        }
    }

    public bool IsCapRing
    {
        get
        {
            return Node1.IsCapRing || Node2.IsCapRing;
        }
    }

    public bool IsInsideCapRing
    {
        get
        {
            return Node1.IsCapRing && Node2.IsCapRing;
        }
    }

    public bool IsCap
    {
        get
        {
            return Node1.HasNation || Node2.HasNation;
        }
    }

    public int NumSeaSwamp
    {
        get
        {
            int ct = 0;

            if (Node1.ProvinceData.IsWaterSwamp)
            {
                ct++;
            }

            if (Node2.ProvinceData.IsWaterSwamp)
            {
                ct++;
            }

            return ct;
        }
    }

    public Connection(Node n1, Node n2, ConnectionType c, bool diagonal = false): this(n1, n2)
    {
        ConnectionType = c;
        Diagonal = diagonal;
    }

    public Connection(Node n1, Node n2)
    {
        ConnectionType = ConnectionType.STANDARD;
        Node1 = n1;
        Node2 = n2;

        Vector2 p1 = new Vector2(n1.X, n1.Y);
        Vector2 p2 = new Vector2(n2.X, n2.Y);
        Vector2 pos = (p1 + p2) / 2;

        if (Mathf.Abs(n1.X - n2.X) > 3.0f)
        {
            pos.x = -0.5f;
        }

        if (Mathf.Abs(n1.Y - n2.Y) > 3.0f)
        {
            pos.y = -0.5f;
        }

        Pos = pos;
    }

    public void SetConnection(ConnectionType c)
    {
        ConnectionType = c;
    }

    public float DistanceTo(Connection c)
    {
        return Vector2.Distance(Pos, c.Pos);
    }

    public bool SharesNode(Connection c)
    {
        return (c.Node1 == Node1 || c.Node1 == Node2 || c.Node2 == Node1 || c.Node2 == Node2);
    }

    public List<Node> GetUniqueNodes(Connection c1, Connection c2)
    {
        List<Node> result = new List<Node>();

        if (!result.Contains(c1.Node1))
        {
            result.Add(c1.Node1);
        }
        if (!result.Contains(c1.Node2))
        {
            result.Add(c1.Node2);
        }
        if (!result.Contains(c2.Node1))
        {
            result.Add(c2.Node1);
        }
        if (!result.Contains(c2.Node2))
        {
            result.Add(c2.Node2);
        }
        if (!result.Contains(Node1))
        {
            result.Add(Node1);
        }
        if (!result.Contains(Node2))
        {
            result.Add(Node2);
        }

        return result;
    }

    public void AddTriangleLink(Connection c)
    {
        if (TriangleLinked == null)
        {
            TriangleLinked = new List<Connection>();
        }

        if (!TriangleLinked.Contains(c))
        {
            TriangleLinked.Add(c);
        }
    }

    public void CalcAdjacent(List<Connection> conns, NodeLayout nl) 
    {
        if (Diagonal)
        {
            List<Node> diags = Node1.ConnectedNodes.Where(x => Node2.ConnectedNodes.Contains(x)).ToList();
            diags.Add(Node1);
            diags.Add(Node2);

            Adjacent = conns.Where(x => diags.Contains(x.Node1) && diags.Contains(x.Node2) && x != this).ToList();
        }
        else
        {
            List<Node> diags = Node1.ConnectedNodes.Where(x => Node2.ConnectedNodes.Contains(x)).ToList();
            diags.Add(Node1);
            diags.Add(Node2);

            Adjacent = conns.Where(x => diags.Contains(x.Node1) && diags.Contains(x.Node2) && x.Diagonal && x != this).ToList();
        }
    }

    public static Connection Create(Node n1, Node n2, ConnectionType c)
    {
        Connection con = new Connection(n1, n2, c);
        return con;
    }
}
