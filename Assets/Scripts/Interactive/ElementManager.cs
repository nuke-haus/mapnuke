using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager class that handles all things related to generation of basic province and connection objects.
/// Might be too many manager classes. Perhaps this will be redundant after a refactor. Idk.
/// Has a global singleton.
/// </summary>
public class ElementManager : MonoBehaviour
{
    public static ElementManager s_element_manager;

    public GameObject ProvinceMarker;
    public GameObject ConnectionMarker;
    public GameObject ProvinceWidget;
    public GameObject ConnectionWidget;
    public MapBorder MapBorder;

    public List<GameObject> m_generated;
    public List<ProvinceMarker> m_provinces;
    public List<ConnectionMarker> m_connections;
    private readonly float m_edge_tolerance = 0.38f; // use 0.5f for near-perfect grid 

    public float Y
    {
        get
        {
            return ArtManager.s_art_manager.CurrentArtConfiguration.ProvinceSizeY;
        }
    }

    public float X
    {
        get
        {
            return ArtManager.s_art_manager.CurrentArtConfiguration.ProvinceSizeX;
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

    private void Start()
    {
        s_element_manager = this;

        m_generated = new List<GameObject>();
    }

    public void ShowLabels(bool on)
    {
        foreach (var pm in m_provinces)
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
            foreach (var o in m_generated)
            {
                GameObject.Destroy(o);
            }

            m_generated = new List<GameObject>();
        }

        MapBorder.SetBorders(new Vector3(9000, 9000), new Vector3(9001, 9001));
    }

    public IEnumerator GenerateElements(List<Node> nodes, List<Connection> conns, NodeLayoutData layout)
    {
        if (m_generated != null)
        {
            foreach (var o in m_generated)
            {
                GameObject.Destroy(o);
            }

            m_generated = new List<GameObject>();
        }

        m_generated = new List<GameObject>();
        m_provinces = new List<ProvinceMarker>();
        m_connections = new List<ConnectionMarker>();

        var min = Vector3.zero - new Vector3(X, Y);
        var max = new Vector3(X * (layout.X - 1), Y * (layout.Y - 1));
        var min_top = new Vector3(min.x, max.y);
        var max_bot = new Vector3(max.x, min.y);
        var dist_up = Vector3.Distance(min, min_top);
        var dist_horz = Vector3.Distance(min, max_bot);

        MapBorder.SetBorders(min, max);

        foreach (var n in nodes) // create basic provinces
        {
            var pos = new Vector3(n.X * X, n.Y * Y, 0);
            var randpos = pos - new Vector3(m_edge_tolerance * X + (UnityEngine.Random.Range(0f, X - (m_edge_tolerance * 2 * X))), m_edge_tolerance * Y + (UnityEngine.Random.Range(0f, Y - (m_edge_tolerance * 2 * Y))));

            var marker_obj = GameObject.Instantiate(ProvinceMarker);
            marker_obj.transform.position = randpos;

            var province_marker = marker_obj.GetComponent<ProvinceMarker>();
            var widget_obj = GameObject.Instantiate(ProvinceWidget);
            var widget = widget_obj.GetComponent<ProvinceWidget>();

            widget.SetParent(province_marker);
            province_marker.SetWidget(widget);
            province_marker.UpdateArtStyle();
            province_marker.SetNode(n);

            m_provinces.Add(province_marker);
            m_generated.Add(marker_obj);
            m_generated.Add(widget_obj);

            if (Util.ShouldYield()) yield return null;
        }

        adjust_province_positions();

        foreach (var m in m_provinces)
        {
            m.OffsetWidget(); // properly position widget
            if (Util.ShouldYield()) yield return null;
        }

        var dummies = new List<ProvinceMarker>();

        foreach (var m in m_provinces) // create dummy provinces
        {
            if (m.Node.X == 0 && m.Node.Y == 0)
            {
                var pos_up = new Vector3(m.transform.position.x, m.transform.position.y + dist_up);
                var pos_right = new Vector3(m.transform.position.x + dist_horz, m.transform.position.y);
                var pos_diag = new Vector3(m.transform.position.x + dist_horz, m.transform.position.y + dist_up);

                var g1 = GameObject.Instantiate(ProvinceMarker);
                g1.transform.position = pos_up;

                var m1 = g1.GetComponent<ProvinceMarker>();
                m1.UpdateArtStyle();
                m1.AddLinkedProvince(m);
                m.AddLinkedProvince(m1);
                m1.SetDummy(true, m);
                
                var g2 = GameObject.Instantiate(ProvinceMarker);
                g2.transform.position = pos_right;

                var m2 = g2.GetComponent<ProvinceMarker>();
                m2.UpdateArtStyle();
                m2.AddLinkedProvince(m);
                m.AddLinkedProvince(m2);
                m2.SetDummy(true, m);

                var g3 = GameObject.Instantiate(ProvinceMarker);
                g3.transform.position = pos_diag;

                var m3 = g3.GetComponent<ProvinceMarker>();
                m3.UpdateArtStyle();
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
                var pos_right = new Vector3(m.transform.position.x + dist_horz, m.transform.position.y);

                var g1 = GameObject.Instantiate(ProvinceMarker);
                g1.transform.position = pos_right;

                var m1 = g1.GetComponent<ProvinceMarker>();
                m1.UpdateArtStyle();
                m1.AddLinkedProvince(m);
                m.AddLinkedProvince(m1);
                m1.SetDummy(true, m);

                dummies.Add(m1);
                m_generated.Add(g1);
            }
            else if (m.Node.Y == 0)
            {
                var pos_up = new Vector3(m.transform.position.x, m.transform.position.y + dist_up);

                var g1 = GameObject.Instantiate(ProvinceMarker);
                g1.transform.position = pos_up;

                var m1 = g1.GetComponent<ProvinceMarker>();
                m1.UpdateArtStyle();
                m1.AddLinkedProvince(m);
                m.AddLinkedProvince(m1);
                m1.SetDummy(true, m);

                dummies.Add(m1);
                m_generated.Add(g1);
            }

            if (Util.ShouldYield()) yield return null;
        }

        m_provinces.AddRange(dummies);
        var real_province = new Dictionary<Node, ProvinceMarker>();
        var dummy_province = new Dictionary<Node, List<ProvinceMarker>>();

        foreach (var pm in m_provinces)
        {
            if (!dummy_province.ContainsKey(pm.Node)) dummy_province[pm.Node] = new List<ProvinceMarker> { };
            if (pm.IsDummy)
            {
                dummy_province[pm.Node].Add(pm);
            }
            else
            {
                real_province[pm.Node] = pm;
            }
        }

        foreach (var c in conns) // create connection markers
        {
            var prov1 = real_province[c.Node1];
            var prov2 = real_province[c.Node2];
            var pd1 = dummy_province[c.Node1];
            var pd2 = dummy_province[c.Node2];

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

                var g = GameObject.Instantiate(ConnectionMarker);
                var m = g.GetComponent<ConnectionMarker>();
                var p1 = prov1.transform.position;
                var p2 = prov2.transform.position;

                prov1.AddConnection(m);
                prov2.AddConnection(m);

                var center = get_weighted_center(p1, p2, prov1.Node, prov2.Node);
                g.transform.position = center;

                var w = GameObject.Instantiate(ConnectionWidget);
                w.transform.position = center + new Vector3(500f, 0f, 0f);

                m.UpdateArtStyle();
                m.SetEdgeConnection(true);
                m.SetProvinces(prov1, prov2);
                m.SetEndPoints(p1, p2);

                var widget = w.GetComponent<ConnectionWidget>();
                widget.SetParent(m);
                m.SetWidget(widget);

                m.SetConnection(c);

                m_connections.Add(m);
                m_generated.Add(g);
                m_generated.Add(w);
            }
            else // base case
            {
                var g = GameObject.Instantiate(ConnectionMarker);
                var m = g.GetComponent<ConnectionMarker>();
                var p1 = prov1.transform.position;
                var p2 = prov2.transform.position;

                prov1.AddConnection(m);
                prov2.AddConnection(m);

                var center = get_weighted_center(p1, p2, prov1.Node, prov2.Node);
                g.transform.position = center;

                var w = GameObject.Instantiate(ConnectionWidget);
                w.transform.position = center + new Vector3(500f, 0f, 0f);

                m.SetProvinces(prov1, prov2);
                m.SetEndPoints(p1, p2);

                var widget = w.GetComponent<ConnectionWidget>();
                widget.SetParent(m);

                m.UpdateArtStyle();
                m.SetWidget(widget);
                m.SetConnection(c);

                m_connections.Add(m);
                m_generated.Add(g);
                m_generated.Add(w);
            }
            if (Util.ShouldYield()) yield return null;
        }

        number_provinces();

        yield return StartCoroutine(ArtManager.s_art_manager.GenerateElements(m_provinces, m_connections, layout, layout.X * X, layout.Y * Y));
    }

    private void adjust_province_positions() // we need to ensure that no province centers are on the same horizontal line
    {
        var ypos = 0;
        var row = m_provinces.Where(x => x.Node.Y == ypos && !x.IsDummy).ToList();

        while (row.Any())
        {
            foreach (var m in row)
            {
                var pos = m.transform.position;
                var add = 0.02f;

                if (UnityEngine.Random.Range(0, 2) == 0)
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

    private void number_provinces() // assign province numbers based on X and Y position, this is how dominions 5 does it
    {
        var valid = m_provinces.Where(x => !x.IsDummy).OrderBy(x => x.gameObject.transform.position.y).ThenBy(x => x.gameObject.transform.position.x);
        var num = 1;

        foreach (var pm in valid)
        {
            pm.Node.SetID(num);
            num++;
        }
    }

    private Vector3 get_mirrored_pos(Vector3 min, Vector3 max, Vector3 vec)
    {
        var diff = vec - min;

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

    private Vector3 get_weighted_center(Vector3 p1, Vector3 p2, Node n1, Node n2)
    {
        var dir1 = (p1 - p2).normalized;
        var dir2 = (p2 - p1).normalized;
        var center = (p1 + p2) / 2;
        var dist = Vector3.Distance(center, p1);

        if (n1.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV) || n1.Connections.Count == 4)
        {
            center += (dir2 * (dist * 0.16f));
        }
        if (n2.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV) || n2.Connections.Count == 4)
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
