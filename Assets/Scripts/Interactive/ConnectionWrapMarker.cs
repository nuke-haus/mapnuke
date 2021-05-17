using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Behaviour class for duplicate river/road meshes used at map edge for horizontal/vertical wrapping cases
/// </summary>
public class ConnectionWrapMarker : MonoBehaviour
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
    public LineRenderer BorderLine;
    public LineRenderer RoadLine;

    public GameObject MapSpritePrefab;
    public GameObject InnerStrokePrefab;
    private PolyBorder m_border;
    private List<Vector3> m_poly;
    private ConnectionMarker m_parent;
    private Connection m_connection;
    private List<SpriteMarker> m_sprites;
    private InnerStroke m_stroke;
    private Vector3 m_offset;

    public PolyBorder PolyBorder
    {
        get
        {
            return m_border;
        }
    }

    public Vector3 Offset
    {
        get
        {
            return m_offset;
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

    public void SetParent(ConnectionMarker parent)
    {
        m_parent = parent;
        m_connection = parent.Connection;
    }

    public void CreatePoly(List<Vector3> poly, PolyBorder pb, Vector3 offset)
    {
        m_poly = poly;
        m_offset = offset;

        draw_border(pb, poly, offset);
        draw_shore(offset);

        if (m_connection.ConnectionType == ConnectionType.ROAD)
        {
            draw_road(offset);
        }
        if (m_connection.ConnectionType == ConnectionType.RIVER || m_connection.ConnectionType == ConnectionType.SHALLOWRIVER)
        {
            ConstructPoly(pb, offset);
            draw_river_shore(offset);
        }
        else
        {
            MeshFilter.mesh.Clear();
        }

        SetSeason(GenerationManager.s_generation_manager.Season);
    }

    private void draw_road(Vector3 offset)
    {
        var pts = new List<Vector3>();

        foreach (var pos in m_poly)
        {
            pts.Add(pos + offset);
        }

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

                jitter.AddKey(rand, UnityEngine.Random.Range(0.32f, 0.52f));
            }
        }

        RoadLine.widthCurve = jitter;
    }

    private void draw_river_shore(Vector3 offset)
    {
        if (m_connection.ConnectionType == ConnectionType.ROAD)
        {
            return;
        }

        if (m_stroke != null)
        {
            GameObject.Destroy(m_stroke.gameObject);
            m_stroke = null;
        }

        var g = GameObject.Instantiate(InnerStrokePrefab);
        m_stroke = g.GetComponent<InnerStroke>();
        m_stroke.UpdateArtStyle();
        m_stroke.DrawStroke(m_poly, offset + new Vector3(0f, 0f, -0.9f), false, 0.03f, 0.06f);
    }

    private void draw_shore(Vector3 offset)
    {
        if (m_stroke != null)
        {
            GameObject.Destroy(m_stroke.gameObject);
            m_stroke = null;
        }

        if ((m_parent.Prov1.Node.ProvinceData.IsWater && !m_parent.Prov2.Node.ProvinceData.IsWater) || (m_parent.Prov2.Node.ProvinceData.IsWater && !m_parent.Prov1.Node.ProvinceData.IsWater))
        {
            var g = GameObject.Instantiate(InnerStrokePrefab);
            m_stroke = g.GetComponent<InnerStroke>();
            m_stroke.UpdateArtStyle();
            m_stroke.DrawStroke(PolyBorder.GetFullLengthBorder(), offset);
        }
    }

    public void Delete()
    {
        if (m_sprites != null)
        {
            foreach (var sm in m_sprites)
            {
                GameObject.Destroy(sm.gameObject);
            }
        }

        MeshFilter.mesh.Clear();

        GameObject.Destroy(MeshObj);
        GameObject.Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (m_stroke != null)
        {
            GameObject.Destroy(m_stroke.gameObject);
        }
    }

    public void SetSeason(Season s)
    {
        if (m_stroke != null)
        {
            m_stroke.SetSeason(s);
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

    public void ConstructPoly(PolyBorder pb, Vector3 offset)
    {
        MeshFilter.mesh.Clear();
        RoadLine.positionCount = 2;
        RoadLine.SetPositions(new Vector3[] { new Vector3(900, 900, 0), new Vector3(901, 900, 0) });

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

        var m = new Mesh();
        m.vertices = m_poly.ToArray();
        m.uv = uv;
        m.triangles = indices;

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
        if (m_connection.ConnectionType == ConnectionType.ROAD)
        {
            pos.z = -2.0f;
        }
        else
        {
            pos.z = -0.9f;
        }

        MeshObj.transform.localPosition = pos;

        SetSeason(GenerationManager.s_generation_manager.Season);
    }

    public void draw_border(PolyBorder pb, List<Vector3> poly, Vector3 offset)
    {
        m_border = pb;

        var border = pb.GetFullLengthBorder();
        var fix = new List<Vector3>();

        foreach (var v in border)
        {
            fix.Add(new Vector3(v.x + offset.x, v.y + offset.y, -0.8f));
        }

        if (m_connection.ConnectionType == ConnectionType.RIVER || m_connection.ConnectionType == ConnectionType.SHALLOWRIVER)
        {
            var offset_poly = new List<Vector3>();
            foreach (var v in poly)
            {
                offset_poly.Add(new Vector3(v.x + offset.x, v.y + offset.y, -0.8f));
            }

            var key_start = new Keyframe(0f, ArtManager.s_art_manager.CurrentArtConfiguration.ProvinceBorderWidth * 2f);
            var key_end = new Keyframe(1f, ArtManager.s_art_manager.CurrentArtConfiguration.ProvinceBorderWidth * 2f);
            BorderLine.widthCurve = new AnimationCurve(key_start, key_end);
            BorderLine.startColor = GenerationManager.s_generation_manager.BorderColor;
            BorderLine.endColor = GenerationManager.s_generation_manager.BorderColor;
            BorderLine.positionCount = offset_poly.Count;
            BorderLine.SetPositions(offset_poly.ToArray());

            return;
        }

        var arr = fix.ToArray();
        var border_scale = 1f;

        if ((m_connection.Node1.ProvinceData.IsWater && !m_connection.Node2.ProvinceData.IsWater) ||
            (!m_connection.Node1.ProvinceData.IsWater && m_connection.Node2.ProvinceData.IsWater))
        {
            border_scale = 2f;
        }

        var key1 = new Keyframe(0f, ArtManager.s_art_manager.CurrentArtConfiguration.ProvinceBorderWidth * border_scale);
        var key2 = new Keyframe(1f, ArtManager.s_art_manager.CurrentArtConfiguration.ProvinceBorderWidth * border_scale);
        BorderLine.widthCurve = new AnimationCurve(key1, key2);
        BorderLine.startColor = GenerationManager.s_generation_manager.BorderColor;
        BorderLine.endColor = GenerationManager.s_generation_manager.BorderColor;
        BorderLine.positionCount = arr.Length;
        BorderLine.SetPositions(arr);
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

    private SpriteMarker make_sprite(Vector3 pos, ConnectionSprite cs, Vector3 offset)
    {
        pos.z = -3f;
        var g = GameObject.Instantiate(MapSpritePrefab);
        var sm = g.GetComponent<SpriteMarker>();
        sm.SetSprite(cs);
        sm.transform.position = pos + offset;

        m_sprites.Add(sm);
        //m_sprites.AddRange(sm.CreateMirrorSprites());

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

        if (m_connection.ConnectionType == ConnectionType.MOUNTAIN)
        {
            if (PolyBorder == null)
            {
                return m_sprites;
            }

            m_sprites = new List<SpriteMarker>();

            var last = new Vector3(-900, -900, 0);
            var cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

            if (cs == null)
            {
                return m_sprites;
            }

            var ct = 0;

            make_sprite(PolyBorder.P1, cs, Vector3.zero);
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            make_sprite(PolyBorder.P2, cs, Vector3.zero);
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

            foreach (var pt in PolyBorder.OrderedFinePoints)
            {
                if (Vector3.Distance(pt, PolyBorder.P2) < cs.Size) // if near to the endpoint, find the mid point between end point and current point and draw 1 sprite
                {
                    Vector3 midpoint = (last + PolyBorder.P2) / 2;
                    make_sprite(midpoint, cs, Vector3.zero);
                    break;
                }

                if (Vector3.Distance(pt, last) < cs.Size) // still too close to the last placed sprite, keep searching
                {
                    ct++;
                    continue;
                }

                ct++;
                last = pt;

                make_sprite(pt, cs, Vector3.zero);
                cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
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
            var other = PolyBorder.Reversed();

            make_sprite(PolyBorder.P1, cs, Vector3.zero); // draw a sprite at both endpoints
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            make_sprite(PolyBorder.P2, cs, Vector3.zero);
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

            var first_quarter = Mathf.RoundToInt(PolyBorder.OrderedFinePoints.Count * 0.3f);
            var last_quarter = Mathf.RoundToInt(PolyBorder.OrderedFinePoints.Count * 0.7f);
            var gap_distance = Vector3.Distance(PolyBorder.OrderedFinePoints[first_quarter], PolyBorder.OrderedFinePoints[last_quarter]);

            if (gap_distance < ArtManager.s_art_manager.CurrentArtConfiguration.MinimumMountainPassGapSize)
            {
                first_quarter = Mathf.RoundToInt(PolyBorder.OrderedFinePoints.Count * 0.2f);
                last_quarter = Mathf.RoundToInt(PolyBorder.OrderedFinePoints.Count * 0.8f);
            }

            for (int i = 0; i < PolyBorder.OrderedFinePoints.Count; i++)
            {
                var pt = PolyBorder.OrderedFinePoints[i];
                var is_mountain = i < first_quarter || i > last_quarter;

                if (Vector3.Distance(pt, PolyBorder.P2) < cs.Size) // if near to the endpoint, find the mid point between end point and current point and draw 1 sprite
                {
                    Vector3 midpoint = (last + PolyBorder.P2) / 2;
                    make_sprite(midpoint, cs, Vector3.zero);
                    break;
                }

                if (Vector3.Distance(pt, last) < cs.Size) // still too close to the last placed sprite, keep searching
                {
                    continue;
                }

                last = pt;
                make_sprite(pt, cs, Vector3.zero);

                if (is_mountain)
                {
                    cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
                }
                else
                {
                    cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAINPASS);
                }
            }
        }

        return m_sprites;
    }
}
