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
    public GameObject InnerStrokePrefab;

    ConnectionWidget m_widget;
    List<ConnectionWrapMarker> m_wraps;
    InnerStroke m_stroke;
    PolyBorder m_border;
    List<Vector3> m_poly;
    List<Vector3> m_culling_points;
    Vector3 m_pos1;
    Vector3 m_pos2;
    Vector3 m_midpt;
    Vector3 m_forced_max;
    Vector3 m_forced_min;
    List<Vector3> m_tri_centers;
    List<SpriteMarker> m_sprites;
    ConnectionMarker m_linked;
    Connection m_connection;
    ProvinceMarker m_p1;
    ProvinceMarker m_p2;
    bool m_edge = false;
    bool m_is_dummy = false;
    bool m_selected = false;
    bool m_force_mirror = false;
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
        
        p1.z = -4.0f;
        p2.z = -4.0f;

        LineRenderer rend = GetComponent<LineRenderer>();
        rend.SetPositions(new Vector3[] {p1, p2});
    }

    SpriteMarker make_sprite(Vector3 pos, ConnectionSprite cs, Vector3 offset)
    {
        pos.z = -3f;
        GameObject g = GameObject.Instantiate(MapSpritePrefab);
        SpriteMarker sm = g.GetComponent<SpriteMarker>();
        sm.SetSprite(cs);
        sm.transform.position = pos + offset;

        m_sprites.Add(sm);
        m_sprites.AddRange(sm.CreateMirrorSprites(m_forced_max, m_forced_min, m_force_mirror));

        return sm;
    }

    SpriteMarker make_bridge_sprite(Vector3 pos, ConnectionSprite cs, Vector3 offset, bool force, bool flip_x)
    {
        pos.z = -3f;
        GameObject g = GameObject.Instantiate(MapSpritePrefab);
        SpriteMarker sm = g.GetComponent<SpriteMarker>();
        sm.SetSprite(cs);
        sm.SetFlip(force, flip_x);
        sm.transform.position = pos + offset;

        m_sprites.Add(sm);
        m_sprites.AddRange(sm.CreateMirrorSprites(m_forced_max, m_forced_min, m_force_mirror));

        return sm;
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

        m_sprites = new List<SpriteMarker>();
        List<SpriteMarker> all = new List<SpriteMarker>();

        foreach (ConnectionWrapMarker m in m_wraps)
        {
            //all.AddRange(m.PlaceSprites());
        }

        if (PolyBorder != null)
        {
            Vector3 max = MapBorder.s_map_border.Maxs;
            Vector3 min = MapBorder.s_map_border.Mins;
            bool p1 = false;
            bool p2 = false;

            if (PolyBorder.P1.x < min.x || 
                    PolyBorder.P1.y < min.y || 
                    PolyBorder.P1.x > max.x ||
                    PolyBorder.P1.y > max.y)
            {
                m_force_mirror = true;
                m_forced_min = PolyBorder.P1;
                m_forced_max = PolyBorder.P1;
                p1 = true;
            }
            if (PolyBorder.P2.x < min.x ||
                    PolyBorder.P2.y < min.y ||
                    PolyBorder.P2.x > max.x ||
                    PolyBorder.P2.y > max.y)
            {
                m_force_mirror = true;
                m_forced_min = PolyBorder.P2;
                m_forced_max = PolyBorder.P2;
                p2 = true;
            }

            if (p1 && p2)
            {
                m_forced_max = max + new Vector3(0.02f, 0.02f);
                m_forced_min = max - new Vector3(0.02f, 0.02f);
            }
        }

        if (m_connection.ConnectionType == ConnectionType.MOUNTAIN)
        {
            if (PolyBorder == null)
            {
                return m_sprites;
            }

            Vector3 last = new Vector3(-900, -900, 0);
            ConnectionSprite cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            ConnectionSprite bottom = ArtManager.s_art_manager.GetMountainSpecSprite();

            if (cs == null)
            {
                return m_sprites;
            }

            int ct = 0;

            make_sprite(PolyBorder.P1, cs, Vector3.zero);
            make_sprite(PolyBorder.P1, bottom, new Vector3(0, 0.01f));
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            make_sprite(PolyBorder.P2, cs, Vector3.zero);
            make_sprite(PolyBorder.P2, bottom, new Vector3(0, 0.01f));
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

            foreach (Vector3 pt in PolyBorder.OrderedFinePoints)
            {
                if (Vector3.Distance(last, pt) < cs.Size && ct < PolyBorder.OrderedFinePoints.Count - 3)
                {
                    ct++;
                    continue;
                }

                ct++;
                last = pt;

                make_sprite(pt, cs, Vector3.zero);
                make_sprite(pt, bottom, new Vector3(0, 0.01f));

                cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
                bottom = ArtManager.s_art_manager.GetMountainSpecSprite();
            }
        }
        else if (m_connection.ConnectionType == ConnectionType.MOUNTAINPASS)
        {
            if (PolyBorder == null)
            {
                return m_sprites;
            }

            Vector3 last = new Vector3(-900, -900, 0);
            ConnectionSprite cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            ConnectionSprite bottom = ArtManager.s_art_manager.GetMountainSpecSprite();
            PolyBorder other = PolyBorder.Reversed();

            make_sprite(PolyBorder.P1, cs, Vector3.zero);
            make_sprite(PolyBorder.P1, bottom, new Vector3(0, 0.01f));
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            bottom = ArtManager.s_art_manager.GetMountainSpecSprite();
            make_sprite(PolyBorder.P2, cs, Vector3.zero);
            make_sprite(PolyBorder.P2, bottom, new Vector3(0, 0.01f));
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            bottom = ArtManager.s_art_manager.GetMountainSpecSprite();

            make_sprite(other.OrderedFinePoints[0], cs, Vector3.zero);
            make_sprite(other.OrderedFinePoints[0], bottom, new Vector3(0, 0.01f));
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            bottom = ArtManager.s_art_manager.GetMountainSpecSprite();
            make_sprite(other.OrderedFinePoints[1], cs, Vector3.zero);
            make_sprite(other.OrderedFinePoints[1], bottom, new Vector3(0, 0.01f));
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            bottom = ArtManager.s_art_manager.GetMountainSpecSprite();
            make_sprite(other.OrderedFinePoints[2], cs, Vector3.zero);
            make_sprite(other.OrderedFinePoints[2], bottom, new Vector3(0, 0.01f));
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            bottom = ArtManager.s_art_manager.GetMountainSpecSprite();

            Vector3 endpt = other.OrderedFinePoints[2];
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            bool is_mountain = true;

            foreach (Vector3 pt in PolyBorder.OrderedFinePoints)
            {
                if (Vector3.Distance(pt, endpt) < 0.05f)
                {
                    break;
                }

                if (Vector3.Distance(last, pt) < cs.Size)
                {
                    continue;
                }

                last = pt;

                if (!is_mountain)
                {
                    make_sprite(pt, cs, new Vector3(0, 0.01f));
                }
                else
                {
                    make_sprite(pt, cs, Vector3.zero);
                    make_sprite(pt, bottom, new Vector3(0, 0.01f));
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

                bottom = ArtManager.s_art_manager.GetMountainSpecSprite();
            }
        }
        else if (m_connection.ConnectionType == ConnectionType.SHALLOWRIVER)
        {
            int mid = Mathf.RoundToInt(PolyBorder.OrderedPoints.Count * 0.25f);

            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                mid = Mathf.RoundToInt(PolyBorder.OrderedPoints.Count * 0.75f);
            }

            Vector3 pos = PolyBorder.OrderedPoints[mid];
            Vector3 pos2 = PolyBorder.OrderedPoints[mid + 1];
            Vector3 actual = (pos - pos2).normalized;//(PolyBorder.P2 - PolyBorder.P1).normalized;

            Vector3 dir = new Vector3(-0.05f, 1f);

            if (Mathf.Abs(actual.y) < 0.3f)
            {
                ConnectionSprite cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.RIVER);
                SpriteMarker s = make_sprite(pos, cs, Vector3.zero);
            }
            else
            {
                ConnectionSprite cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.SHALLOWRIVER);
      
                if ((actual.y < 0 && actual.x > 0) || (actual.y > 0 && actual.x < 0))
                {
                    make_bridge_sprite(pos, cs, Vector3.zero, true, true);
                    dir = new Vector3(1f, 0.3f);
                }
                else
                {
                    make_bridge_sprite(pos, cs, Vector3.zero, true, false);
                    dir = new Vector3(1f, -0.3f);
                }
            }

            float cull = 0.05f;
            List<Vector3> positions = new List<Vector3>();

            while (cull < 0.35f)
            {
                positions.Add(pos + (dir * cull));
                positions.Add(pos + (dir * -cull));

                cull += 0.05f;
            }

            m_culling_points = positions;
        }
        else if (m_connection.ConnectionType == ConnectionType.ROAD)
        {
            if (PolyBorder == null)
            {
                return m_sprites;
            }

            Vector3 last = new Vector3(-900, -900, 0);
            ConnectionSprite cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.ROAD);
            float size = UnityEngine.Random.Range(cs.Size, cs.Size + 0.08f);

            if (cs == null)
            {
                return m_sprites;
            }

            foreach (Vector3 pt in m_poly)
            {
                if (Vector3.Distance(last, pt) < size)
                {
                    continue;
                }

                make_sprite(pt, cs, Vector3.zero);

                last = pt;
                cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.ROAD);
                size = UnityEngine.Random.Range(cs.Size, cs.Size + 0.08f);
            }
        }

        all.AddRange(m_sprites);
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
        if (m_stroke != null)
        {
            m_stroke.SetSeason(s);
        }

        if (m_wraps != null)
        {
            foreach (ConnectionWrapMarker m in m_wraps)
            {
                m.SetSeason(s);
            }
        }

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

    public GameObject CreateWrapMesh(PolyBorder pb, Vector3 offset) // create a connection wrap marker and its polygon
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
            m_culling_points = null;
        } 
    }

    public void RecalculatePoly()
    {
        BorderLine.positionCount = 2;
        BorderLine.SetPositions(new Vector3[] { new Vector3(900, 900, 0), new Vector3(901, 900, 0) });
        MeshFilter.mesh.Clear();

        if (m_culling_points == null)
        {
            m_culling_points = new List<Vector3>();
        }

        if (PolyBorder == null)
        {
            return;
        }

        draw_shore();

        if (m_connection.ConnectionType == ConnectionType.RIVER || m_connection.ConnectionType == ConnectionType.SHALLOWRIVER)
        {
            m_poly = get_contour(PolyBorder, 0.02f, 0.08f);

            ConstructPoly();
            draw_river_shore();

            SetSeason(GenerationManager.s_generation_manager.Season);

            return;
        }
        else if (m_connection.ConnectionType == ConnectionType.ROAD)
        {
            Vector3 dir = (m_pos2 - m_pos1).normalized;
            Vector3 p1 = m_pos1 + dir * 0.4f;
            Vector3 p2 = m_pos2 - dir * 0.4f;

            if (Vector3.Distance(m_pos1, m_pos2) > 3f)
            {
                p1 = m_pos1 + dir * 0.6f;
                p2 = m_pos2 - dir * 0.6f;
            }

            if (Vector3.Distance(p1, p2) < 0.5f)
            {
                p1 = m_pos1 + dir * 0.3f;
                p2 = m_pos2 - dir * 0.3f;
            }

            PolyBorder fake = new PolyBorder(p1, p2, m_connection);
            m_culling_points = fake.OrderedPoints;

            m_poly = get_contour(fake, 0.012f, 0.014f);//get_contour(fake, 0.01f, 0.02f);
            ConstructPoly(true);
        }

        List<Vector3> border = PolyBorder.GetFullLengthBorder();
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

    void draw_river_shore()
    {
        if (m_stroke != null)
        {
            GameObject.Destroy(m_stroke.gameObject);
            m_stroke = null;
        }

        GameObject g = GameObject.Instantiate(InnerStrokePrefab);
        m_stroke = g.GetComponent<InnerStroke>();
        m_stroke.DrawStroke(m_poly, new Vector3(0, 0, -0.9f), false, 0.03f, 0.06f);
    }

    void draw_shore()
    {
        if (m_stroke != null)
        {
            GameObject.Destroy(m_stroke.gameObject);
            m_stroke = null;
        }

        if ((Prov1.Node.ProvinceData.IsWater && !Prov2.Node.ProvinceData.IsWater) || (Prov2.Node.ProvinceData.IsWater && !Prov1.Node.ProvinceData.IsWater))
        {
            GameObject g = GameObject.Instantiate(InnerStrokePrefab);
            m_stroke = g.GetComponent<InnerStroke>();
            m_stroke.DrawStroke(PolyBorder.GetFullLengthBorder(), Vector3.zero);
        }
    }

    List<Vector3> get_contour(PolyBorder pb, float min_lat, float max_lat)
    {
        float dist = 0.08f;
        List<Vector3> pts = new List<Vector3>();
        Vector3 norm = (pb.P2 - pb.P1).normalized;
        Vector3 prev = pb.P1;
        Vector3 lateral = Vector3.zero;

        for (int i = 0; i < pb.OrderedPoints.Count - 1; i++)
        {
            Vector3 pt = pb.OrderedPoints[i];
            
            if (Vector3.Distance(prev, pt) < dist)
            {
                continue;
            }

            Vector3 behind = pb.OrderedPoints[Mathf.Max(i - 1, 0)];
            Vector3 forward = pb.OrderedPoints[Mathf.Min(i + 1, pb.OrderedPoints.Count - 1)];

            dist = UnityEngine.Random.Range(0.04f, 0.16f);

            Vector3 dir = (pt - behind).normalized;
            Vector3 lateral1 = Vector3.Cross(dir, Vector3.forward);
            dir = (forward - pt).normalized;
            Vector3 lateral2 = Vector3.Cross(dir, Vector3.forward);
            Vector3 true_lat = (lateral1 + lateral2) * 0.5f;

            if (i == 0)
            {
                true_lat = lateral2;
            }
            else if (i == pb.OrderedPoints.Count - 1)
            {
                true_lat = lateral1;
            }

            Vector3 shift = pt + true_lat * UnityEngine.Random.Range(min_lat, max_lat);
            pts.Add(shift);

            lateral = true_lat;
            prev = pt;
        }

        Vector3 endpt = pb.P2 + norm * 0.05f + lateral * min_lat;
        Vector3 endpt2 = pb.P2 + norm * 0.05f - lateral * min_lat;

        pts.Add(endpt);
        pts.Add(endpt2);
        prev = endpt;
        dist = 0.08f;

        List<Vector3> ordered = pb.Reversed().OrderedPoints;

        for (int i = 0; i < ordered.Count - 1; i++)
        {
            Vector3 pt = ordered[i];

            if (Vector3.Distance(prev, pt) < dist)
            {
                continue;
            }

            Vector3 behind = ordered[Mathf.Max(i - 1, 0)];
            Vector3 forward = ordered[Mathf.Min(i + 1, ordered.Count - 1)];

            dist = UnityEngine.Random.Range(0.04f, 0.16f);

            Vector3 dir = (pt - behind).normalized;
            Vector3 lateral1 = Vector3.Cross(dir, Vector3.forward);
            dir = (forward - pt).normalized;
            Vector3 lateral2 = Vector3.Cross(dir, Vector3.forward);
            Vector3 true_lat = (lateral1 + lateral2) * 0.5f;

            if (i == 0)
            {
                true_lat = lateral2;
            }
            else if (i == ordered.Count - 1)
            {
                true_lat = lateral1;
            }

            Vector3 shift = pt + true_lat * UnityEngine.Random.Range(min_lat, max_lat);
            pts.Add(shift);

            lateral = true_lat;
            prev = pt;
        }

        endpt = pb.P1 - norm * 0.05f + lateral * min_lat;
        endpt2 = pb.P1 - norm * 0.05f - lateral * min_lat;

        pts.Add(endpt);
        pts.Add(endpt2);
        pts.Add(pts[0]); // - norm * 0.004f 

        CubicBezierPath path = new CubicBezierPath(pts.ToArray());

        float max = (float)(pts.Count - 1) - 0.04f;
        float j = 0.04f;
        float spacing = 0.04f;
        Vector3 last = pb.P1;
        ordered = new List<Vector3>();

        while (j < max)
        {
            Vector3 pt = path.GetPoint(j);

            if (Vector3.Distance(pt, last) >= spacing)
            {
                ordered.Add(pt);
                last = pt;
            }

            j += 0.04f;
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

        Vector3[] norms = m.normals;

        for (int i = 0; i < norms.Length - 1; i++)
        {
            norms[i] = Vector3.back;
        }

        m.normals = norms;

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

        if (m_widget == null)
        {
            return;
        }

        m_widget.SetConnection(c);
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

        if (m_widget == null)
        {
            return;
        }

        m_widget.SetConnection(m_connection);
    }

    public void SetWidget(ConnectionWidget w)
    {
        m_widget = w;
    }

    void OnDestroy()
    {
        if (m_stroke != null)
        {
            GameObject.Destroy(m_stroke.gameObject);
        }
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
        m_widget.SetSelected(b);
        m_selected = b;
        m_scale = 1.0f;
        SpriteObj.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
    }
}
