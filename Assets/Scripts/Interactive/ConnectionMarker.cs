using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The behaviour class for connections.
/// This class handles creation of connection polygons (ie. rivers and roads) and manages behaviour of the connection's unity objects.
/// </summary>
public class ConnectionMarker : MonoBehaviour
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
    public LineRenderer RoadLine;

    public GameObject WrapMarkerPrefab;
    public GameObject MapSpritePrefab;
    public GameObject InnerStrokePrefab;
    private ConnectionWidget m_widget;
    private List<ConnectionWrapMarker> m_wraps;
    private InnerStroke m_stroke;
    private PolyBorder m_border;
    private List<Vector3> m_poly;
    private List<Vector3> m_culling_points;
    private Vector3 m_pos1;
    private Vector3 m_pos2;
    private Vector3 m_midpt;
    private Vector3 m_forced_max;
    private Vector3 m_forced_min;
    private List<Vector3> m_tri_centers;
    private List<SpriteMarker> m_sprites;
    private ConnectionMarker m_linked;
    private Connection m_connection;
    private ProvinceMarker m_p1;
    private ProvinceMarker m_p2;
    private bool m_edge = false;
    private bool m_is_dummy = false;
    private bool m_selected = false;
    private bool m_force_mirror = false;
    private float m_scale = 1.0f;
    private static Dictionary<ConnectionType, Color> m_colors;

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

    public void UpdateArtStyle()
    {
        var art_config = ArtManager.s_art_manager.CurrentArtConfiguration;

        MatSea = art_config.MatRiver;
        MatDeepSea = art_config.MatDeepRiver;
        MatRoad = art_config.MatRoad;
        MatWinterSea = art_config.MatWinterRiver;
        MatWinterDeepSea = art_config.MatWinterDeepRiver;
        MatWinterRoad = art_config.MatWinterRoad;
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

        var rend = GetComponent<LineRenderer>();
        rend.SetPositions(new Vector3[] { p1, p2 });
    }

    private SpriteMarker make_sprite(Vector3 pos, ConnectionSprite cs, Vector3 offset)
    {
        pos.z = -3f;
        var g = GameObject.Instantiate(MapSpritePrefab);
        var sm = g.GetComponent<SpriteMarker>();
        sm.SetSprite(cs);
        sm.transform.position = pos + offset;

        m_sprites.Add(sm);
        m_sprites.AddRange(sm.CreateMirrorSprites(m_forced_max, m_forced_min, m_force_mirror));

        return sm;
    }

    private SpriteMarker make_bridge_sprite(Vector3 pos, ConnectionSprite cs, Vector3 offset, bool force, bool flip_x)
    {
        pos.z = -3f;
        var g = GameObject.Instantiate(MapSpritePrefab);
        var sm = g.GetComponent<SpriteMarker>();
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
            foreach (var sm in m_sprites)
            {
                GameObject.Destroy(sm.gameObject);
            }
        }

        m_sprites = new List<SpriteMarker>();
        var all = new List<SpriteMarker>();

        /*foreach (var m in m_wraps)
        {
            //all.AddRange(m.PlaceSprites());
        }*/

        if (PolyBorder != null)
        {
            var max = MapBorder.s_map_border.Maxs;
            var min = MapBorder.s_map_border.Mins;
            var p1 = false;
            var p2 = false;

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

            var last = new Vector3(-900, -900, 0);
            var cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            var bottom = ArtManager.s_art_manager.GetMountainSpecSprite();

            if (cs == null)
            {
                return m_sprites;
            }

            var ct = 0;

            make_sprite(PolyBorder.P1, cs, Vector3.zero);
            make_sprite(PolyBorder.P1, bottom, new Vector3(0, 0.01f));
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            make_sprite(PolyBorder.P2, cs, Vector3.zero);
            make_sprite(PolyBorder.P2, bottom, new Vector3(0, 0.01f));
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

            foreach (var pt in PolyBorder.OrderedFinePoints)
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

            var last = new Vector3(-900, -900, 0);
            var cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            var bottom = ArtManager.s_art_manager.GetMountainSpecSprite();
            var other = PolyBorder.Reversed();

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

            var endpt = other.OrderedFinePoints[2];
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            var is_mountain = true;

            foreach (var pt in PolyBorder.OrderedFinePoints)
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
            var mid = Mathf.RoundToInt(PolyBorder.OrderedPoints.Count * 0.25f);

            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                mid = Mathf.RoundToInt(PolyBorder.OrderedPoints.Count * 0.75f);
            }

            var pos = PolyBorder.OrderedPoints[mid];
            var pos2 = PolyBorder.OrderedPoints[mid + 1];
            var actual = (pos - pos2).normalized;//(PolyBorder.P2 - PolyBorder.P1).normalized;

            var dir = new Vector3(-0.05f, 1f);

            if (Mathf.Abs(actual.y) < 0.3f)
            {
                var cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.RIVER);
                make_sprite(pos, cs, Vector3.zero);
            }
            else
            {
                var cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.SHALLOWRIVER);

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

            var cull = 0.05f;
            var positions = new List<Vector3>();

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

            var cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.ROAD);

            if (cs == null)
            {
                return m_sprites;
            }

            foreach (var pt in m_poly)
            {
                make_sprite(pt, cs, new Vector3(UnityEngine.Random.Range(-0.01f, 0.01f), UnityEngine.Random.Range(-0.01f, 0.01f)));

                cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.ROAD);
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
        m_border = new PolyBorder(m_tri_centers[0] + transform.position, m_tri_centers[1] + transform.position, Connection);
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
            foreach (var m in m_wraps)
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
                RoadLine.material = MatRoad;
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
                RoadLine.material = MatWinterRoad;
            }
        }
    }

    public GameObject CreateWrapMesh(PolyBorder pb, Vector3 offset) // create a connection wrap marker and its polygon
    {
        if (m_wraps == null)
        {
            m_wraps = new List<ConnectionWrapMarker>();
        }

        if (m_wraps.Any(x => Vector3.Distance(x.Offset, offset) < 0.1f))
        {
            return null;
        }

        var obj = GameObject.Instantiate(WrapMarkerPrefab);
        var wrap = obj.GetComponent<ConnectionWrapMarker>();

        wrap.UpdateArtStyle();
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
            foreach (var m in m_wraps)
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
        RoadLine.positionCount = 2;
        RoadLine.SetPositions(new Vector3[] { new Vector3(900, 900, 0), new Vector3(901, 900, 0) });
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
            var dir = (m_pos2 - m_pos1).normalized;
            var p1 = m_pos1 + dir * 0.4f;
            var p2 = m_pos2 - dir * 0.4f;

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

            var fake = new PolyBorder(p1, p2, m_connection);
            m_culling_points = fake.OrderedPoints;
            m_poly = fake.OrderedFinePoints;

            draw_road(fake);
        }

        var border = PolyBorder.GetFullLengthBorder();
        var fix = new List<Vector3>();

        foreach (var v in border)
        {
            fix.Add(new Vector3(v.x, v.y, -0.8f));
        }

        var arr = fix.ToArray();

        BorderLine.positionCount = arr.Length;
        BorderLine.SetPositions(arr);

        BorderLine.startColor = GenerationManager.s_generation_manager.BorderColor;
        BorderLine.endColor = GenerationManager.s_generation_manager.BorderColor;

        SetSeason(GenerationManager.s_generation_manager.Season);
    }

    private void draw_road(PolyBorder path)
    {
        var pts = new List<Vector3>();

        foreach (var pt in path.OrderedFinePoints)
        {
            pts.Add(pt + new Vector3(0, 0, -0.9f));
        }

        pts.RemoveAt(0);
        pts.RemoveAt(pts.Count - 1);

        RoadLine.positionCount = pts.Count;
        RoadLine.SetPositions(pts.ToArray());

        var jitter = new AnimationCurve();
        var num_keys = UnityEngine.Random.Range(2, 8);
        var floats = new List<float>();

        for (var i = 0; i < num_keys; i++)
        {
            var rand = UnityEngine.Random.Range(0f, 1f);
            var ct = 0;

            while (floats.Any(x => Mathf.Abs(rand - x) < 0.1f) && ct < 10)
            {
                rand = UnityEngine.Random.Range(0f, 1f);
                ct++;
            }

            if (ct < 10)
            {
                floats.Add(rand);

                jitter.AddKey(rand, UnityEngine.Random.Range(0.12f, 0.24f));
            }
        }

        RoadLine.widthCurve = jitter;
    }

    private void draw_river_shore()
    {
        if (m_stroke != null)
        {
            GameObject.Destroy(m_stroke.gameObject);
            m_stroke = null;
        }

        var g = GameObject.Instantiate(InnerStrokePrefab);
        m_stroke = g.GetComponent<InnerStroke>();
        m_stroke.UpdateArtStyle();
        m_stroke.DrawStroke(m_poly, new Vector3(0, 0, -0.9f), false, 0.03f, 0.06f);
    }

    private void draw_shore()
    {
        if (m_stroke != null)
        {
            GameObject.Destroy(m_stroke.gameObject);
            m_stroke = null;
        }

        if ((Prov1.Node.ProvinceData.IsWater && !Prov2.Node.ProvinceData.IsWater) || (Prov2.Node.ProvinceData.IsWater && !Prov1.Node.ProvinceData.IsWater))
        {
            var g = GameObject.Instantiate(InnerStrokePrefab);
            m_stroke = g.GetComponent<InnerStroke>();
            m_stroke.UpdateArtStyle();
            m_stroke.DrawStroke(PolyBorder.GetFullLengthBorder(), Vector3.zero);
        }
    }

    private List<Vector3> get_contour(PolyBorder pb, float min_lat, float max_lat)
    {
        var dist = 0.08f;
        var pts = new List<Vector3>();
        var norm = (pb.P2 - pb.P1).normalized;
        var prev = pb.P1;
        var lateral = Vector3.zero;

        for (var i = 0; i < pb.OrderedPoints.Count - 1; i++)
        {
            var pt = pb.OrderedPoints[i];

            if (Vector3.Distance(prev, pt) < dist)
            {
                continue;
            }

            var behind = pb.OrderedPoints[Mathf.Max(i - 1, 0)];
            var forward = pb.OrderedPoints[Mathf.Min(i + 1, pb.OrderedPoints.Count - 1)];

            dist = UnityEngine.Random.Range(0.04f, 0.16f);

            var dir = (pt - behind).normalized;
            var lateral1 = Vector3.Cross(dir, Vector3.forward);
            dir = (forward - pt).normalized;
            var lateral2 = Vector3.Cross(dir, Vector3.forward);
            var true_lat = (lateral1 + lateral2) * 0.5f;

            if (i == 0)
            {
                true_lat = lateral2;
            }
            else if (i == pb.OrderedPoints.Count - 1)
            {
                true_lat = lateral1;
            }

            var shift = pt + true_lat * UnityEngine.Random.Range(min_lat, max_lat);
            pts.Add(shift);

            lateral = true_lat;
            prev = pt;
        }

        var endpt = pb.P2 + norm * 0.05f + lateral * min_lat;
        var endpt2 = pb.P2 + norm * 0.05f - lateral * min_lat;

        pts.Add(endpt);
        pts.Add(endpt2);
        prev = endpt;
        dist = 0.08f;

        var ordered = pb.Reversed().OrderedPoints;

        for (var i = 0; i < ordered.Count - 1; i++)
        {
            var pt = ordered[i];

            if (Vector3.Distance(prev, pt) < dist)
            {
                continue;
            }

            var behind = ordered[Mathf.Max(i - 1, 0)];
            var forward = ordered[Mathf.Min(i + 1, ordered.Count - 1)];

            dist = UnityEngine.Random.Range(0.04f, 0.16f);

            var dir = (pt - behind).normalized;
            var lateral1 = Vector3.Cross(dir, Vector3.forward);
            dir = (forward - pt).normalized;
            var lateral2 = Vector3.Cross(dir, Vector3.forward);
            var true_lat = (lateral1 + lateral2) * 0.5f;

            if (i == 0)
            {
                true_lat = lateral2;
            }
            else if (i == ordered.Count - 1)
            {
                true_lat = lateral1;
            }

            var shift = pt + true_lat * UnityEngine.Random.Range(min_lat, max_lat);
            pts.Add(shift);

            lateral = true_lat;
            prev = pt;
        }

        endpt = pb.P1 - norm * 0.05f + lateral * min_lat;
        endpt2 = pb.P1 - norm * 0.05f - lateral * min_lat;

        pts.Add(endpt);
        pts.Add(endpt2);
        pts.Add(pts[0]); // - norm * 0.004f 

        var path = new CubicBezierPath(pts.ToArray());

        var max = (float)(pts.Count - 1) - 0.04f;
        var j = 0.04f;
        var spacing = 0.04f;
        var last = pb.P1;
        ordered = new List<Vector3>();

        while (j < max)
        {
            var pt = path.GetPoint(j);

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

        var tr = new Triangulator(get_pts_array(m_poly));
        var indices = tr.Triangulate();

        var uv = new Vector2[m_poly.Count];

        for (var i = 0; i < m_poly.Count; i++)
        {
            uv[i] = new Vector2(m_poly[i].x, m_poly[i].y);
        }

        var m = new Mesh
        {
            vertices = m_poly.ToArray(),
            uv = uv,
            triangles = indices
        };

        m.RecalculateNormals();
        m.RecalculateBounds();

        var norms = m.normals;

        for (var i = 0; i < norms.Length - 1; i++)
        {
            norms[i] = Vector3.back;
        }

        m.normals = norms;

        MeshFilter.mesh = m;
        var pos = transform.position * -1f;

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

    private Vector2[] get_pts_array(List<Vector3> list)
    {
        var vecs = new List<Vector2>();

        foreach (var vec in list)
        {
            vecs.Add(new Vector2(vec.x, vec.y));
        }

        return vecs.ToArray();
    }

    public void SetConnection(Connection c)
    {
        if (m_colors == null)
        {
            m_colors = new Dictionary<ConnectionType, Color>
            {
                { ConnectionType.STANDARD, new Color(1.0f, 1.0f, 0.4f) },
                { ConnectionType.SHALLOWRIVER, new Color(0.4f, 0.5f, 0.9f) },
                { ConnectionType.ROAD, new Color(0.8f, 0.5f, 0.2f) },
                { ConnectionType.MOUNTAINPASS, new Color(0.5f, 0.2f, 0.6f) },
                { ConnectionType.MOUNTAIN, new Color(0.2f, 0.2f, 0.3f) },
                { ConnectionType.RIVER, new Color(0.2f, 0.2f, 0.9f) }
            };
        }

        m_connection = c;
        var col = m_colors[c.ConnectionType];

        var rend = GetComponent<LineRenderer>();
        rend.startColor = col;
        rend.endColor = col;

        var rend2 = SpriteObj.GetComponent<SpriteRenderer>();
        rend2.color = col;

        if (m_widget == null)
        {
            return;
        }

        m_widget.SetConnection(c);
    }

    public void UpdateConnection(ConnectionType t)
    {
        var col = m_colors[t];

        var rend = GetComponent<LineRenderer>();
        rend.startColor = col;
        rend.endColor = col;

        var rend2 = SpriteObj.GetComponent<SpriteRenderer>();
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

    private void OnDestroy()
    {
        if (m_stroke != null)
        {
            GameObject.Destroy(m_stroke.gameObject);
        }
    }

    private void Update()
    {
        if (m_selected)
        {
            m_scale = 1.0f + 0.3f * Mathf.Sin(Time.time * 5.5f);

            SpriteObj.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
        }
    }

    private void OnMouseOver()
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
