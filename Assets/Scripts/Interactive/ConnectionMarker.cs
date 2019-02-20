using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The behaviour class for connections.
/// This class handles creation of connection polygons (ie. rivers and roads) and manages behaviour of the connection's unity objects.
/// </summary>
public class ConnectionMarker: MonoBehaviour
{
    public Material MatSea;
    public Material MatDeepSea;
    public Material MatRoad;

    public Material MatWinterSea;
    public Material MatWinterDeepSea;
    public Material MatWinterRoad;

    public MeshRenderer Mesh;
    public MeshFilter MeshFilter;
    public GameObject MeshObj;
    public GameObject SpriteObj;
    public LineRenderer BorderLine;

    public GameObject WrapMarkerPrefab;
    public GameObject MapSpritePrefab;

    List<ConnectionWrapMarker> m_wraps;
    PolyBorder m_border;
    List<Vector3> m_poly;
    List<Vector3> m_culling_points;
    Vector3 m_pos1;
    Vector3 m_pos2;
    Vector3 m_midpt;
    List<Vector3> m_tri_centers;
    List<SpriteMarker> m_sprites;
    ConnectionMarker m_linked;
    Connection m_connection;
    ProvinceMarker m_p1;
    ProvinceMarker m_p2;
    bool m_edge = false;
    bool m_is_dummy = false;
    bool m_selected = false;
    float m_scale = 1.0f;

    static Dictionary<ConnectionType, Color> m_colors;

    public bool IsEdge
    {
        get
        {
            return m_edge;
        }
    }

    public bool IsDummy
    {
        get
        {
            return m_is_dummy;
        }
    }

    public PolyBorder PolyBorder
    {
        get
        {
            return m_border;
        }
    }

    public List<Vector3> CullingPoints
    {
        get
        {
            return m_culling_points;
        }
    }

    public Vector3 DummyOffset
    {
        get
        {
            if (Prov1.IsDummy)
            {
                return Prov1.DummyOffset;
            }
            else
            {
                return Prov2.DummyOffset;
            }
        }
    }

    public ProvinceMarker Dummy
    {
        get
        {
            if (Prov1.IsDummy)
            {
                return Prov1;
            }
            else
            {
                return Prov2;
            }
        }
    }

    public Connection Connection
    {
        get
        {
            return m_connection;
        }
    }

    public ConnectionMarker LinkedConnection
    {
        get
        {
            return m_linked;
        }
    }

    public Vector3 Midpoint
    {
        get
        {
            return m_midpt;
        }
    }

    public Vector3 EdgePoint
    {
        get
        {
            if (Vector3.Distance(Endpoint1, Prov1.transform.position) < 0.1f)
            {
                return Endpoint2;
            }

            return Endpoint1;
        }
    }

    public Vector3 Endpoint1
    {
        get
        {
            return m_pos1;
        }
    }

    public Vector3 Endpoint2
    {
        get
        {
            return m_pos2;
        }
    }

    public List<Vector3> TriCenters
    {
        get
        {
            if (m_tri_centers == null)
            {
                m_tri_centers = new List<Vector3>();
            }

            return m_tri_centers;
        }
    }

    public ProvinceMarker Prov1
    {
        get
        {
            return m_p1;
        }
    }

    public ProvinceMarker Prov2
    {
        get
        {
            return m_p2;
        }
    }

    public bool TouchingProvince(ProvinceMarker pm)
    {
        return Vector3.Distance(pm.transform.position, Endpoint1) < 0.01f || Vector3.Distance(pm.transform.position, Endpoint2) < 0.01f;
    }

    public void SetEndPoints(Vector3 p1, Vector3 p2)
    {
        m_pos1 = p1;
        m_pos2 = p2;
        m_midpt = (p1 + p2) / 2;
        Vector3 dir = (p1 - p2).normalized;

        p1.z = -4.0f;
        p2.z = -4.0f;

        LineRenderer rend = GetComponent<LineRenderer>();
        rend.SetPositions(new Vector3[] {p1, p2});
    }

    void make_sprite(Vector3 pos, ConnectionSprite cs, Vector3 offset)
    {
        GameObject g = GameObject.Instantiate(MapSpritePrefab);
        SpriteMarker sm = g.GetComponent<SpriteMarker>();
        sm.SetSprite(cs);
        sm.transform.position = pos + offset;

        m_sprites.Add(sm);
        m_sprites.AddRange(sm.CreateMirrorSprites());
    }

    public List<SpriteMarker> PlaceSprites()
    {
        if (m_sprites != null)
        {
            foreach (SpriteMarker sm in m_sprites)
            {
                GameObject.Destroy(sm.gameObject);
            }
        }

        if (m_connection.ConnectionType == ConnectionType.MOUNTAIN)
        {
            if (PolyBorder == null)
            {
                return new List<SpriteMarker>();
            }

            m_sprites = new List<SpriteMarker>();

            Vector3 last = new Vector3(-900, -900, 0);
            ConnectionSprite cs = ArtManager.s_art_manager.GetConnectionSprite(m_connection.ConnectionType);

            if (cs == null)
            {
                return new List<SpriteMarker>();
            }

            while (UnityEngine.Random.Range(0f, 1f) > cs.SpawnChance)
            {
                cs = ArtManager.s_art_manager.GetConnectionSprite(m_connection.ConnectionType);
            }

            int ct = 0;

            make_sprite(PolyBorder.P1, cs, Vector3.zero);
            cs = ArtManager.s_art_manager.GetConnectionSprite(m_connection.ConnectionType);
            make_sprite(PolyBorder.P2, cs, Vector3.zero);
            cs = ArtManager.s_art_manager.GetConnectionSprite(m_connection.ConnectionType);

            foreach (Vector3 pt in PolyBorder.OrderedPoints)
            {
                if (Vector3.Distance(last, pt) < cs.Size && ct < PolyBorder.OrderedPoints.Count - 1)
                {
                    ct++;
                    continue;
                }

                ct++;
                Vector3 pos = pt;
                last = pt;
                pos.z = -3f;

                make_sprite(pos, cs, Vector3.zero);

                cs = ArtManager.s_art_manager.GetConnectionSprite(m_connection.ConnectionType);

                while (UnityEngine.Random.Range(0f, 1f) > cs.SpawnChance)
                {
                    cs = ArtManager.s_art_manager.GetConnectionSprite(m_connection.ConnectionType);
                }
            }
        }
        else if (m_connection.ConnectionType == ConnectionType.MOUNTAINPASS)
        {
            if (PolyBorder == null)
            {
                return new List<SpriteMarker>();
            }

            m_sprites = new List<SpriteMarker>();

            Vector3 last = new Vector3(-900, -900, 0);
            ConnectionSprite cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            PolyBorder other = PolyBorder.Reversed();

            int right_ct = 0;
            int right_pos = 0;

            make_sprite(PolyBorder.P1, cs, Vector3.zero);
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            make_sprite(PolyBorder.P2, cs, Vector3.zero);
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

            foreach (Vector3 pt in other.OrderedPoints)
            {
                if (Vector3.Distance(last, pt) < cs.Size)
                {
                    right_pos++;
                    continue;
                }

                Vector3 pos = pt;
                last = pt;
                pos.z = -3f;

                make_sprite(pos, cs, Vector3.zero);

                if (right_ct > 1)
                {
                    break;
                }

                cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

                right_ct++;
                right_pos++;
            }

            Vector3 endpt = other.OrderedPoints[right_pos];
            right_ct = -1;
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            bool is_mountain = true;

            foreach (Vector3 pt in PolyBorder.OrderedPoints)
            {
                if (Vector3.Distance(pt, endpt) < 0.08f)
                {
                    break;
                }

                if (Vector3.Distance(last, pt) < cs.Size)
                {
                    continue;
                }

                Vector3 pos = pt;
                last = pt;
                pos.z = -3f;

                if (!is_mountain)
                {
                    make_sprite(pos, cs, new Vector3(0, 0.01f));
                }
                else
                {
                    make_sprite(pos, cs, Vector3.zero);
                }

                if (Vector3.Distance(pt, endpt) < 0.6f)
                {
                    cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAINPASS);
                    is_mountain = false;
                }
                else
                {
                    cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
                    is_mountain = true;
                }

                right_ct--;
            }
        }
        else
        {
            m_sprites = new List<SpriteMarker>();
        }

        List<SpriteMarker> all = new List<SpriteMarker>();
        all.AddRange(m_sprites);

        foreach (ConnectionWrapMarker m in m_wraps)
        {
            all.AddRange(m.PlaceSprites());
        }

        return all;
    }

    public void ClearTriangles()
    {
        m_tri_centers = new List<Vector3>();
    }

    public PolyBorder GetOffsetBorder(Vector3 offset)
    {
        return m_border.Offset(offset);
    }

    public void CreatePolyBorder()
    {
        m_border = new PolyBorder(m_tri_centers[0] + transform.position, m_tri_centers[1] + transform.position, this.Connection);
    }

    public void AddTriangleCenter(Vector3 pos) // this is in worldspace relative to this marker's position so we have to subtract its position to get localspace
    {
        pos = pos - transform.position;

        if (m_tri_centers == null)
        {
            m_tri_centers = new List<Vector3>();
        }

        if (!m_tri_centers.Any(x => Vector3.Distance(x, pos) < 0.01f))
        {
            m_tri_centers.Add(pos);
        }
    }

    public void SetLinkedConnection(ConnectionMarker m)
    {
        m_linked = m;
        SetConnection(m.Connection);
    }

    public void SetProvinces(ProvinceMarker m1, ProvinceMarker m2)
    {
        m_p1 = m1;
        m_p2 = m2;
    }

    public void SetEdgeConnection(bool b)
    {
        m_edge = b;
    }

    public void SetDummy(bool b)
    {
        m_is_dummy = b;
    }

    public void SetSeason(Season s)
    {
        if (s == Season.SUMMER)
        {
            if (m_connection.ConnectionType == ConnectionType.RIVER)
            {
                Mesh.material = MatDeepSea;
            }
            else if (m_connection.ConnectionType == ConnectionType.SHALLOWRIVER)
            {
                Mesh.material = MatSea;
            }
            else
            {
                Mesh.material = MatRoad;
            }
        }
        else
        {
            if (m_connection.ConnectionType == ConnectionType.RIVER)
            {
                Mesh.material = MatWinterDeepSea;
            }
            else if (m_connection.ConnectionType == ConnectionType.SHALLOWRIVER)
            {
                Mesh.material = MatWinterSea;
            }
            else
            {
                Mesh.material = MatWinterRoad;
            }
        }
    }

    public GameObject CreateWrapMesh(PolyBorder pb, Vector3 offset)
    {
        if (m_wraps == null)
        {
            m_wraps = new List<ConnectionWrapMarker>();
        }

        GameObject obj = GameObject.Instantiate(WrapMarkerPrefab);
        ConnectionWrapMarker wrap = obj.GetComponent<ConnectionWrapMarker>();

        wrap.SetParent(this);
        wrap.CreatePoly(m_poly, pb, offset);
        obj.transform.position = obj.transform.position + offset;

        m_wraps.Add(wrap);

        return obj;
    }

    public void ClearWrapMeshes()
    {
        if (m_wraps == null)
        {
            m_wraps = new List<ConnectionWrapMarker>();
        }
        else
        {
            foreach (ConnectionWrapMarker m in m_wraps)
            {
                if (m != null)
                {
                    m.Delete();
                }
            }

            m_wraps = new List<ConnectionWrapMarker>();
        }
    }

    public void RecalculatePoly()
    {
        BorderLine.positionCount = 2;
        BorderLine.SetPositions(new Vector3[] { new Vector3(900, 900, 0), new Vector3(901, 900, 0) });
        MeshFilter.mesh.Clear();

        PolyBorder pb = PolyBorder;// def.GetPolyBorder(m_connection);

        m_culling_points = new List<Vector3>();

        if (pb == null)
        {
            return;
        }

        if (m_connection.ConnectionType == ConnectionType.RIVER || m_connection.ConnectionType == ConnectionType.SHALLOWRIVER)
        {
            m_poly = get_contour(pb, 0.02f, 0.08f);
            //m_removal_points = pb.OrderedPoints;

            ConstructPoly();
            SetSeason(GenerationManager.s_generation_manager.Season);

            return;
        }
        else if (m_connection.ConnectionType == ConnectionType.ROAD)
        {
            Vector3 dir = (m_pos2 - m_pos1).normalized;
            Vector3 p1 = m_pos1 + dir * 0.24f;
            Vector3 p2 = m_pos2 - dir * 0.24f;

            if (IsEdge)
            {
                if (Vector3.Distance(EdgePoint, m_pos1) < 0.1f)
                {
                    p1 = EdgePoint;
                }
                else
                {
                    p2 = EdgePoint;
                }
            }

            PolyBorder fake = new PolyBorder(p1, p2, m_connection);
            m_culling_points = fake.OrderedPoints;

            m_poly = get_contour(fake, 0.01f, 0.02f);
            ConstructPoly(true);
        }

        List<Vector3> border = pb.GetFullLengthBorder();
        List<Vector3> fix = new List<Vector3>();

        foreach (Vector3 v in border)
        {
            fix.Add(new Vector3(v.x, v.y, -0.8f));
        }

        Vector3[] arr = fix.ToArray();

        BorderLine.positionCount = arr.Length;
        BorderLine.SetPositions(arr);

        SetSeason(GenerationManager.s_generation_manager.Season);
    }

    List<Vector3> get_contour(PolyBorder pb, float min_lat, float max_lat)
    {
        float dist = 0.08f;
        List<Vector3> pts = new List<Vector3>();
        Vector3 norm = (pb.P2 - pb.P1).normalized;
        Vector3 prev = pb.P1;
        
        foreach (Vector3 pt in pb.OrderedPoints)
        {
            if (Vector3.Distance(prev, pt) < dist)
            {
                continue;
            }

            dist = UnityEngine.Random.Range(0.04f, 0.16f);

            Vector3 dir = (pt - prev).normalized;
            Vector3 lateral = Vector3.Cross(dir, Vector3.forward);
            Vector3 shift = pt + lateral * UnityEngine.Random.Range(min_lat, max_lat);
            pts.Add(shift);

            prev = pt;
        }

        pts.Add(pb.P2 + norm * 0.02f);
        prev = pb.P2 + norm * 0.02f;
        dist = 0.08f;

        foreach (Vector3 pt in pb.Reversed().OrderedPoints)
        {
            if (Vector3.Distance(prev, pt) < dist)
            {
                continue;
            }

            dist = UnityEngine.Random.Range(0.04f, 0.16f);

            Vector3 dir = (pt - prev).normalized;
            Vector3 lateral = Vector3.Cross(dir, Vector3.forward);
            Vector3 shift = pt + lateral * UnityEngine.Random.Range(min_lat, max_lat);
            pts.Add(shift);

            prev = pt;
        }

        pts.Add(pb.P1 - norm * 0.05f);
        pts.Add(pts[0]);

        CubicBezierPath path = new CubicBezierPath(pts.ToArray());

        float max = (float)(pts.Count - 1) - 0.04f;
        float i = 0.04f;
        float spacing = 0.04f;
        Vector3 last = pb.P1;
        List<Vector3> ordered = new List<Vector3>();

        while (i < max)
        {
            Vector3 pt = path.GetPoint(i);

            if (Vector3.Distance(pt, last) >= spacing)
            {
                ordered.Add(pt);
            }

            i += 0.04f;
        }

        return ordered;
    }

    public void ConstructPoly(bool is_road = false)
    {
        if (m_poly == null || !m_poly.Any())
        {
            return;
        }

        Triangulator tr = new Triangulator(get_pts_array(m_poly));
        int[] indices = tr.Triangulate();

        Vector2[] uv = new Vector2[m_poly.Count];

        for (int i = 0; i < m_poly.Count; i++)
        {
            uv[i] = new Vector2(m_poly[i].x, m_poly[i].y);
        }

        Mesh m = new Mesh();
        m.vertices = m_poly.ToArray();
        m.uv = uv;
        m.triangles = indices;

        m.RecalculateNormals();
        m.RecalculateBounds();

        MeshFilter.mesh = m;

        Vector3 pos = transform.position * -1f;
        if (IsDummy || is_road)
        {
            pos.z = -2.0f;
        }
        else
        {
            pos.z = -0.9f;
        }

        MeshObj.transform.localPosition = pos;
    }

    Vector2[] get_pts_array(List<Vector3> list)
    {
        List<Vector2> vecs = new List<Vector2>();

        foreach (Vector3 vec in list)
        {
            vecs.Add(new Vector2(vec.x, vec.y));
        }

        return vecs.ToArray();
    }

    public void SetConnection(Connection c)
    {
        if (m_colors == null)
        {
            Dictionary<ConnectionType, Color> dict = new Dictionary<ConnectionType, Color>();
            dict.Add(ConnectionType.STANDARD, new Color(1.0f, 1.0f, 0.4f));
            dict.Add(ConnectionType.SHALLOWRIVER, new Color(0.4f, 0.5f, 0.9f));
            dict.Add(ConnectionType.ROAD, new Color(0.8f, 0.5f, 0.2f));
            dict.Add(ConnectionType.MOUNTAINPASS, new Color(0.5f, 0.2f, 0.6f));
            dict.Add(ConnectionType.MOUNTAIN, new Color(0.2f, 0.2f, 0.3f));
            dict.Add(ConnectionType.RIVER, new Color(0.2f, 0.2f, 0.9f));

            m_colors = dict;
        }

        m_connection = c;
        Color col = m_colors[c.ConnectionType];

        LineRenderer rend = GetComponent<LineRenderer>();
        rend.startColor = col;
        rend.endColor = col;

        SpriteRenderer rend2 = SpriteObj.GetComponent<SpriteRenderer>();
        rend2.color = col;
    }

    public void UpdateConnection(ConnectionType t)
    {
        Color col = m_colors[t];

        LineRenderer rend = GetComponent<LineRenderer>();
        rend.startColor = col;
        rend.endColor = col;

        SpriteRenderer rend2 = SpriteObj.GetComponent<SpriteRenderer>();
        rend2.color = col;

        m_connection.SetConnection(t);
    }

    void Update()
    {
		if (m_selected)
        {
            m_scale = 1.0f + 0.3f * Mathf.Sin(Time.time * 5.5f);

            SpriteObj.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
        }
	}

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ConnectionManager.s_connection_manager.SetConnection(this);

            SetSelected(true);
        }
    }

    public void SetSelected(bool b)
    {
        m_selected = b;
        m_scale = 1.0f;
        SpriteObj.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
    }
}
