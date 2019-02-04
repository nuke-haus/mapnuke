using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager class that handles all things related to generation of basic province and connection objects.
/// Might be too many manager classes. Perhaps this will be redundant after a refactor. Idk.
/// Has a global singleton.
/// </summary>
public class ElementManager: MonoBehaviour
{
    public GameObject ProvinceMarker;
    public GameObject ConnectionMarker;
    public MapBorder MapBorder;

    List<GameObject> m_generated;
    List<ProvinceMarker> m_provinces;
    List<ConnectionMarker> m_connections;
    float m_size_y = 1.60f;
    float m_size_x = 2.56f;
    float m_edge_tolerance = 0.35f; // use 0.5f for testing purposes.

    public float Y
    {
        get
        {
            return m_size_y;
        }
    }

    public float X
    {
        get
        {
            return m_size_x;
        }
    }

    public List<ProvinceMarker> Provinces
    {
        get
        {
            return m_provinces;
        }
    }

    public RenderTexture Texture
    {
        get
        {
            return ArtManager.s_art_manager.Texture;
        }
    }

    public void GenerateElements(List<Node> nodes, List<Connection> conns, NodeLayout layout)
    {
        if (m_generated != null)
        {
            foreach (GameObject o in m_generated)
            {
                GameObject.Destroy(o);
            }
        }

        m_generated = new List<GameObject>();
        m_provinces = new List<ProvinceMarker>();
        m_connections = new List<ConnectionMarker>();

        Vector3 min = Vector3.zero - new Vector3(m_size_x, m_size_y);
        Vector3 max = new Vector3(m_size_x * (layout.X - 1), m_size_y * (layout.Y - 1));

        MapBorder.SetBorders(min, max);

        foreach (Node n in nodes)
        {
            Vector3 pos = new Vector3(n.X * m_size_x, n.Y * m_size_y, 0);
            Vector3 randpos = pos - new Vector3(m_edge_tolerance * m_size_x + (UnityEngine.Random.Range(0f, m_size_x - (m_edge_tolerance * 2 * m_size_x))), m_edge_tolerance * m_size_y + (UnityEngine.Random.Range(0f, m_size_y - (m_edge_tolerance * 2 * m_size_y))));

            GameObject g = GameObject.Instantiate(ProvinceMarker);
            g.transform.position = randpos;//new Vector3(n.X, n.Y, 0);

            ProvinceMarker m = g.GetComponent<ProvinceMarker>();
            m.SetNode(n);

            m_provinces.Add(m);
            m_generated.Add(g);
        }

        adjust_province_positions();

        foreach (Connection c in conns)
        {
            ProvinceMarker prov1 = m_provinces.FirstOrDefault(x => x.Node == c.Node1);
            ProvinceMarker prov2 = m_provinces.FirstOrDefault(x => x.Node == c.Node2);
            Vector3 p1 = prov1.transform.position;
            Vector3 p2 = prov2.transform.position;

            GameObject g = GameObject.Instantiate(ConnectionMarker);
            ConnectionMarker m = g.GetComponent<ConnectionMarker>();

            prov1.AddConnection(m);
            prov2.AddConnection(m);

            m_connections.Add(m);
            m_generated.Add(g);

            bool edge = false;

            if (Vector3.Distance(p1, p2) > m_size_x * 3.0f) // wrap case
            {
                edge = true;

                if (prov1.DistanceTo(new Vector2(layout.X - 1,0)) <= 0.01f && (prov2.DistanceTo(new Vector2(0, layout.Y - 1)) <= 0.01f))
                {
                    p2.x = max.x;
                    p2.y = min.y;
                }
                else if (prov2.DistanceTo(new Vector2(layout.X - 1, 0)) <= 0.01f && (prov1.DistanceTo(new Vector2(0, layout.Y - 1)) <= 0.01f))
                {
                    p1.x = max.x;
                    p1.y = min.y;
                }
                else if (prov1.DistanceTo(Vector2.zero) >= prov2.DistanceTo(Vector2.zero))
                {
                    bool vertical = prov2.Node.X == 0;
                    bool horz = prov2.Node.Y == 0;

                    if (vertical && horz)
                    {
                        if (prov1.Node.X > prov2.Node.X + 1)
                        {
                            p1.x = -1 * m_size_x;
                        }
                        if (prov1.Node.Y > prov2.Node.Y + 1)
                        {
                            p1.y = -1 * m_size_y;
                        }
                    }
                    else
                    {
                        if (vertical)
                        {
                            p1.x = -1 * m_size_x;
                        }
                        if (horz)
                        {
                            p1.y = -1 * m_size_y;
                        }
                    }                    
                }
                else
                {
                    bool vertical = prov1.Node.X == 0;
                    bool horz = prov1.Node.Y == 0;

                    if (vertical && horz)
                    {
                        if (prov2.Node.X > prov1.Node.X + 1)
                        {
                            p2.x = -1 * m_size_x;
                        }
                        if (prov2.Node.Y > prov1.Node.Y + 1)
                        {
                            p2.y = -1 * m_size_y;
                        }
                    }
                    else
                    {
                        if (vertical)
                        {
                            p2.x = -1 * m_size_x;
                        }
                        if (horz)
                        {
                            p2.y = -1 * m_size_y;
                        }
                    }
                }

                if (Mathf.Abs(p1.x - min.x) < 0.01f && Mathf.Abs(p1.y - min.y) > 0.01f)
                {
                    float diff = (p1.y - p2.y) * UnityEngine.Random.Range(0.40f, 0.60f);
                    p1.y -= diff;
                }

                if (Mathf.Abs(p1.y - min.y) < 0.01f && Mathf.Abs(p1.x - min.x) > 0.01f && Mathf.Abs(p1.x - max.x) > 0.01f)
                {
                    float diff = (p1.x - p2.x) * UnityEngine.Random.Range(0.40f, 0.60f);
                    p1.x -= diff;
                }

                if (Mathf.Abs(p2.x - min.x) < 0.01f && Mathf.Abs(p2.y - min.y) > 0.01f)
                {
                    float diff = (p2.y - p1.y) * UnityEngine.Random.Range(0.40f, 0.60f);
                    p2.y -= diff;
                }

                if (Mathf.Abs(p2.y - min.y) < 0.01f && Mathf.Abs(p2.x - min.x) > 0.01f && Mathf.Abs(p2.x - max.x) > 0.01f)
                {
                    float diff = (p2.x - p1.x) * UnityEngine.Random.Range(0.40f, 0.60f);
                    p2.x -= diff;
                }
            }

            if (edge)
            {
                m.SetEdgeConnection(true);
            }

            m.SetProvinces(prov1, prov2);
            m.SetEndPoints(p1, p2);
            m.SetConnection(c);

            Vector3 center = get_weighted_center(p1, p2, prov1.Node, prov2.Node);
            g.transform.position = center;
        }

        List<ConnectionMarker> new_dummies = new List<ConnectionMarker>();

        // add dummy connections 
        foreach (ConnectionMarker m in m_connections)
        {
            if (m.IsEdge)
            {
                Vector3 pos = m.Endpoint1;
                ProvinceMarker target = m.Prov1;

                if (Vector3.Distance(m.Endpoint1, m.Prov1.transform.position) < 0.10f) // grab the point that's not attached to any province
                {
                    pos = m.Endpoint2;
                    target = m.Prov2;
                }

                Vector3 mirrored = get_mirrored_pos(min, max, pos);

                GameObject g = GameObject.Instantiate(ConnectionMarker);
                ConnectionMarker dummy = g.GetComponent<ConnectionMarker>();
                dummy.SetDummy(true);
                dummy.SetEdgeConnection(true);
                dummy.SetLinkedConnection(m);
                m.SetLinkedConnection(dummy);

                m.Prov1.AddConnection(dummy);
                m.Prov2.AddConnection(dummy);

                new_dummies.Add(dummy);
                m_generated.Add(g);

                if (target == m.Prov1)
                {
                    dummy.SetProvinces(m.Prov1, m.Prov2);
                    dummy.SetEndPoints(target.transform.position, mirrored);

                    Vector3 center = get_weighted_center(target.transform.position, mirrored, m.Prov1.Node, m.Prov2.Node);
                    g.transform.position = center;
                }
                else
                {
                    dummy.SetProvinces(m.Prov2, m.Prov1);
                    dummy.SetEndPoints(target.transform.position, mirrored);

                    Vector3 center = get_weighted_center(target.transform.position, mirrored, m.Prov2.Node, m.Prov1.Node);
                    g.transform.position = center;
                }   
            }
        }

        m_connections.AddRange(new_dummies);

        number_provinces();

        List<GameObject> objs = ArtManager.s_art_manager.GenerateElements(m_provinces, m_connections, layout, layout.X * m_size_x, layout.Y * m_size_y);
        m_generated.AddRange(objs);
    }

    void adjust_province_positions() // we need to ensure that no province centers are on the same horizontal line
    {
        int xpos = 0;
        var row = m_provinces.Where(x => x.Node.X == xpos);

        while (row.Any())
        {
            foreach (ProvinceMarker m in row)
            {
                Vector3 pos = m.transform.position;

                while (row.Any(x => Mathf.Abs(x.transform.position.y - pos.y) < 0.02f))
                {
                    pos.y += 0.03f;
                }

                m.transform.position = pos;
            }

            xpos++;
            row = m_provinces.Where(x => x.Node.X == xpos);
        }
    }

    void number_provinces() // assign province numbers based on X and Y position, this is how dominions does it
    {
        m_provinces = m_provinces.OrderBy(x => x.gameObject.transform.position.y).ThenBy(x => x.gameObject.transform.position.x).ToList();

        int num = 1;

        foreach (ProvinceMarker pm in m_provinces)
        {
            pm.Node.SetID(num);
            num++;
        }
    }
    
    Vector3 get_mirrored_pos(Vector3 min, Vector3 max, Vector3 vec)
    {
        Vector3 diff = vec - min;

        if (Mathf.Abs(vec.x - min.x) < 0.01f)
        {
            vec.x = max.x;
        }
        else if (Mathf.Abs(vec.x - max.x) < 0.01f)
        {
            vec.x = min.x;
        }

        if (Mathf.Abs(vec.y - min.y) < 0.01f)
        {
            vec.y = max.y;
        }
        else if (Mathf.Abs(vec.y - max.y) < 0.01f)
        {
            vec.y = min.y;
        }

        return vec;
    }

    Vector3 get_weighted_center(Vector3 p1, Vector3 p2, Node n1, Node n2)
    {
        Vector3 dir1 = (p1 - p2).normalized;
        Vector3 dir2 = (p2 - p1).normalized;
        Vector3 center = (p1 + p2) / 2;
        float dist = Vector3.Distance(center, p1);

        if (n1.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV))
        {
            center += (dir2 * (dist * 0.16f));
        }
        if (n2.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV))
        {
            center += (dir1 * (dist * 0.16f));
        }
        
        if (n1.ProvinceData.Terrain.IsFlagSet(Terrain.SMALLPROV))
        {
            center += (dir1 * (dist * 0.20f));
        }
        if (n2.ProvinceData.Terrain.IsFlagSet(Terrain.SMALLPROV))
        {
            center += (dir2 * (dist * 0.20f));
        }

        return center;
    }
}
