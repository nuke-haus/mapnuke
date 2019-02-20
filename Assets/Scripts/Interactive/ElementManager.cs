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
    public static ElementManager s_element_manager;

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

    public List<GameObject> GeneratedObjects
    {
        get
        {
            return m_generated;
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

    void Start()
    {
        s_element_manager = this;

        m_generated = new List<GameObject>();
    }

    public void ShowLabels(bool on)
    {
        foreach (ProvinceMarker pm in m_provinces)
        {
            pm.ShowLabel(on);
        }
    }

    public void AddGeneratedObjects(List<GameObject> objs)
    {
        m_generated.AddRange(objs);
    }

    public void WipeGeneratedObjects()
    {
        if (m_generated != null)
        {
            foreach (GameObject o in m_generated)
            {
                GameObject.Destroy(o);
            }

            m_generated = new List<GameObject>();
        }

        MapBorder.SetBorders(new Vector3(9000, 9000), new Vector3(9001, 9001));
    }

    public void GenerateElements(List<Node> nodes, List<Connection> conns, NodeLayout layout)
    {
        if (m_generated != null)
        {
            foreach (GameObject o in m_generated)
            {
                GameObject.Destroy(o);
            }

            m_generated = new List<GameObject>();
        }

        m_generated = new List<GameObject>();
        m_provinces = new List<ProvinceMarker>();
        m_connections = new List<ConnectionMarker>();

        Vector3 min = Vector3.zero - new Vector3(m_size_x, m_size_y);
        Vector3 max = new Vector3(m_size_x * (layout.X - 1), m_size_y * (layout.Y - 1));
        Vector3 min_top = new Vector3(min.x, max.y);
        Vector3 max_bot = new Vector3(max.x, min.y);
        float dist_up = Vector3.Distance(min, min_top);
        float dist_horz = Vector3.Distance(min, max_bot);

        MapBorder.SetBorders(min, max);

        foreach (Node n in nodes) // create basic provinces
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

        List<ProvinceMarker> dummies = new List<ProvinceMarker>();

        foreach (ProvinceMarker m in m_provinces) // create dummy provinces
        {
            if (m.Node.X == 0 && m.Node.Y == 0)
            {
                Vector3 pos_up = new Vector3(m.transform.position.x, m.transform.position.y + dist_up);
                Vector3 pos_right = new Vector3(m.transform.position.x + dist_horz, m.transform.position.y);
                Vector3 pos_diag = new Vector3(m.transform.position.x + dist_horz, m.transform.position.y + dist_up);

                GameObject g1 = GameObject.Instantiate(ProvinceMarker);
                g1.transform.position = pos_up;

                ProvinceMarker m1 = g1.GetComponent<ProvinceMarker>();
                m1.AddLinkedProvince(m);
                m.AddLinkedProvince(m1);
                m1.SetDummy(true, m);

                GameObject g2 = GameObject.Instantiate(ProvinceMarker);
                g2.transform.position = pos_right;

                ProvinceMarker m2 = g2.GetComponent<ProvinceMarker>();
                m2.AddLinkedProvince(m);
                m.AddLinkedProvince(m2);
                m2.SetDummy(true, m);

                GameObject g3 = GameObject.Instantiate(ProvinceMarker);
                g3.transform.position = pos_diag;

                ProvinceMarker m3 = g3.GetComponent<ProvinceMarker>();
                m3.AddLinkedProvince(m);
                m.AddLinkedProvince(m3);
                m3.SetDummy(true, m);

                dummies.Add(m1);
                dummies.Add(m2);
                dummies.Add(m3);
                m_generated.Add(g1);
                m_generated.Add(g2);
                m_generated.Add(g3);
            }
            else if (m.Node.X == 0)
            {
                Vector3 pos_right = new Vector3(m.transform.position.x + dist_horz, m.transform.position.y);

                GameObject g1 = GameObject.Instantiate(ProvinceMarker);
                g1.transform.position = pos_right;

                ProvinceMarker m1 = g1.GetComponent<ProvinceMarker>();
                m1.AddLinkedProvince(m);
                m.AddLinkedProvince(m1);
                m1.SetDummy(true, m);

                dummies.Add(m1);
                m_generated.Add(g1);
            }
            else if (m.Node.Y == 0)
            {
                Vector3 pos_up = new Vector3(m.transform.position.x, m.transform.position.y + dist_up);

                GameObject g1 = GameObject.Instantiate(ProvinceMarker);
                g1.transform.position = pos_up;

                ProvinceMarker m1 = g1.GetComponent<ProvinceMarker>();
                m1.AddLinkedProvince(m);
                m.AddLinkedProvince(m1);
                m1.SetDummy(true, m);

                dummies.Add(m1);
                m_generated.Add(g1);
            }
        }

        m_provinces.AddRange(dummies);

        foreach (Connection c in conns) // create connection markers
        {
            ProvinceMarker prov1 = m_provinces.FirstOrDefault(x => x.Node == c.Node1 && !x.IsDummy);
            ProvinceMarker prov2 = m_provinces.FirstOrDefault(x => x.Node == c.Node2 && !x.IsDummy);
            List<ProvinceMarker> pd1 = m_provinces.Where(x => x.Node == c.Node1 && x.IsDummy).ToList();
            List<ProvinceMarker> pd2 = m_provinces.Where(x => x.Node == c.Node2 && x.IsDummy).ToList();

            if (pd1.Any() || pd2.Any()) // edge case
            {
                if (pd1.Any() && pd2.Any()) // both have an edge clone
                {
                    if (pd1.Any(x => x.Node.X == 0 && x.Node.Y == 0))
                    {
                        pd1.Add(prov1);
                        pd1 = pd1.OrderBy(x => Vector3.Distance(x.transform.position, prov2.transform.position)).ToList();
                        prov1 = pd1[0];
                    }
                    else if (pd2.Any(x => x.Node.X == 0 && x.Node.Y == 0))
                    {
                        pd2.Add(prov2);
                        pd2 = pd2.OrderBy(x => Vector3.Distance(x.transform.position, prov1.transform.position)).ToList();
                        prov2 = pd2[0];
                    }
                    else if (pd1.Any(x => x.Node.X == layout.X - 1 || x.Node.Y == layout.Y - 1))
                    {
                        pd2.Add(prov2);
                        pd2 = pd2.OrderBy(x => Vector3.Distance(x.transform.position, prov1.transform.position)).ToList();
                        prov2 = pd2[0];
                    }
                    else if (pd2.Any(x => x.Node.X == layout.X - 1 || x.Node.Y == layout.Y - 1))
                    {
                        pd1.Add(prov1);
                        pd1 = pd1.OrderBy(x => Vector3.Distance(x.transform.position, prov2.transform.position)).ToList();
                        prov1 = pd1[0];
                    }
                }
                else if (pd1.Any())
                {
                    pd1.Add(prov1);
                    pd1 = pd1.OrderBy(x => Vector3.Distance(x.transform.position, prov2.transform.position)).ToList();
                    prov1 = pd1[0];
                }
                else
                {
                    pd2.Add(prov2);
                    pd2 = pd2.OrderBy(x => Vector3.Distance(x.transform.position, prov1.transform.position)).ToList();
                    prov2 = pd2[0];
                }

                GameObject g = GameObject.Instantiate(ConnectionMarker);
                ConnectionMarker m = g.GetComponent<ConnectionMarker>();
                Vector3 p1 = prov1.transform.position;
                Vector3 p2 = prov2.transform.position;

                prov1.AddConnection(m);
                prov2.AddConnection(m);

                m_connections.Add(m);
                m_generated.Add(g);

                m.SetEdgeConnection(true);
                m.SetProvinces(prov1, prov2);
                m.SetEndPoints(p1, p2);
                m.SetConnection(c);

                Vector3 center = get_weighted_center(p1, p2, prov1.Node, prov2.Node);
                g.transform.position = center;
            }
            else // base case
            {
                GameObject g = GameObject.Instantiate(ConnectionMarker);
                ConnectionMarker m = g.GetComponent<ConnectionMarker>();
                Vector3 p1 = prov1.transform.position;
                Vector3 p2 = prov2.transform.position;

                prov1.AddConnection(m);
                prov2.AddConnection(m);

                m_connections.Add(m);
                m_generated.Add(g);

                m.SetProvinces(prov1, prov2);
                m.SetEndPoints(p1, p2);
                m.SetConnection(c);

                Vector3 center = get_weighted_center(p1, p2, prov1.Node, prov2.Node);
                g.transform.position = center;
            }
        }

        number_provinces();

        ArtManager.s_art_manager.GenerateElements(m_provinces, m_connections, layout, layout.X * m_size_x, layout.Y * m_size_y);
    }

    void adjust_province_positions() // we need to ensure that no province centers are on the same horizontal line
    {
        int ypos = 0;
        var row = m_provinces.Where(x => x.Node.Y == ypos && !x.IsDummy).ToList();

        while (row.Any())
        {
            foreach (ProvinceMarker m in row)
            {
                Vector3 pos = m.transform.position;
                float add = 0.02f;

                if (UnityEngine.Random.Range(0,2) == 0)
                {
                    add = -0.02f;
                }

                while (row.Any(x => x != m && Mathf.Abs(x.transform.position.y - pos.y) < 0.02f))
                {
                    pos.y += add;
                }

                m.transform.position = pos;
            }

            ypos++;
            row = m_provinces.Where(x => x.Node.Y == ypos).ToList();
        }
    }

    void number_provinces() // assign province numbers based on X and Y position, this is how dominions does it
    {
        var valid = m_provinces.Where(x => !x.IsDummy).OrderBy(x => x.gameObject.transform.position.y).ThenBy(x => x.gameObject.transform.position.x);
        int num = 1;

        foreach (ProvinceMarker pm in valid)
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
