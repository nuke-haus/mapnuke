using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Behavious class for duplicate river/road meshes
/// </summary>
public class ConnectionWrapMarker: MonoBehaviour
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

    PolyBorder m_border;
    List<Vector3> m_poly;
    ConnectionMarker m_parent;
    Connection m_connection;
    List<SpriteMarker> m_sprites;
    InnerStroke m_stroke;
    Vector3 m_offset;

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

    public void SetParent(ConnectionMarker parent)
    {
        m_parent = parent;
        m_connection = parent.Connection;
    }

    public void CreatePoly(List<Vector3> poly, PolyBorder pb, Vector3 offset)
    {
        m_poly = poly;
        m_offset = offset;

        draw_border(pb, offset);
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

    void draw_road(Vector3 offset)
    {
        List<Vector3> pts = new List<Vector3>();

        foreach (Vector3 pos in m_poly)
        {
            pts.Add(pos + offset);
        }

        RoadLine.positionCount = pts.Count;
        RoadLine.SetPositions(pts.ToArray());

        AnimationCurve jitter = new AnimationCurve();
        int num_keys = UnityEngine.Random.Range(2, 8);
        List<float> floats = new List<float>();

        for (int i = 0; i < num_keys; i++)
        {
            float rand = UnityEngine.Random.Range(0f, 1f);
            int ct = 0;

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

    void draw_river_shore(Vector3 offset)
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

        GameObject g = GameObject.Instantiate(InnerStrokePrefab);
        m_stroke = g.GetComponent<InnerStroke>();
        m_stroke.DrawStroke(m_poly, offset + new Vector3(0f, 0f, -0.9f), false, 0.03f, 0.06f);
    }

    void draw_shore(Vector3 offset)
    {
        if (m_stroke != null)
        {
            GameObject.Destroy(m_stroke.gameObject);
            m_stroke = null;
        }

        if ((m_parent.Prov1.Node.ProvinceData.IsWater && !m_parent.Prov2.Node.ProvinceData.IsWater) || (m_parent.Prov2.Node.ProvinceData.IsWater && !m_parent.Prov1.Node.ProvinceData.IsWater))
        {
            GameObject g = GameObject.Instantiate(InnerStrokePrefab);
            m_stroke = g.GetComponent<InnerStroke>();
            m_stroke.DrawStroke(PolyBorder.GetFullLengthBorder(), offset);
        }
    }

    public void Delete()
    {
        if (m_sprites != null)
        {
            foreach (SpriteMarker sm in m_sprites)
            {
                GameObject.Destroy(sm.gameObject);
            }
        }

        MeshFilter.mesh.Clear();

        GameObject.Destroy(MeshObj);
        GameObject.Destroy(gameObject);
    }

    void OnDestroy()
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

    public void draw_border(PolyBorder pb, Vector3 offset)
    {
        m_border = pb;
        BorderLine.positionCount = 2;
        BorderLine.SetPositions(new Vector3[] { new Vector3(900, 900, 0), new Vector3(901, 900, 0) });

        if (m_connection.ConnectionType == ConnectionType.RIVER || m_connection.ConnectionType == ConnectionType.SHALLOWRIVER)
        {
            return;
        }

        List<Vector3> border = pb.GetFullLengthBorder();
        List<Vector3> fix = new List<Vector3>();

        foreach (Vector3 v in border)
        {
            fix.Add(new Vector3(v.x + offset.x, v.y + offset.y, -0.8f));
        }

        Vector3[] arr = fix.ToArray();

        BorderLine.positionCount = arr.Length;
        BorderLine.SetPositions(arr);

        Color c = new Color(0f, 0f, 0f, ArtManager.s_art_manager.BorderOpacity);
        BorderLine.startColor = c;
        BorderLine.endColor = c;
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

    SpriteMarker make_sprite(Vector3 pos, ConnectionSprite cs, Vector3 offset)
    {
        pos.z = -3f;
        GameObject g = GameObject.Instantiate(MapSpritePrefab);
        SpriteMarker sm = g.GetComponent<SpriteMarker>();
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
            foreach (SpriteMarker sm in m_sprites)
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

            Vector3 last = new Vector3(-900, -900, 0);
            ConnectionSprite cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

            if (cs == null)
            {
                return m_sprites;
            }

            int ct = 0;

            make_sprite(PolyBorder.P1, cs, Vector3.zero);
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            make_sprite(PolyBorder.P2, cs, Vector3.zero);
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

            foreach (Vector3 pt in PolyBorder.OrderedFinePoints)
            {
                if (Vector3.Distance(last, pt) < cs.Size && ct < m_border.OrderedFinePoints.Count - 1)
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

            m_sprites = new List<SpriteMarker>();

            Vector3 last = new Vector3(-900, -900, 0);
            ConnectionSprite cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            PolyBorder other = PolyBorder.Reversed();

            int right_ct = 0;
            int right_pos = 0;

            foreach (Vector3 pt in other.OrderedFinePoints)
            {
                if (Vector3.Distance(last, pt) < cs.Size)
                {
                    right_pos++;
                    continue;
                }

                last = pt;

                make_sprite(pt, cs, Vector3.zero);

                if (right_ct > 1)
                {
                    break;
                }

                cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

                right_ct++;
                right_pos++;
            }

            Vector3 endpt = other.OrderedFinePoints[right_pos];
            right_ct = -1;
            cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            bool is_mountain = true;

            foreach (Vector3 pt in PolyBorder.OrderedFinePoints)
            {
                if (Vector3.Distance(pt, endpt) < 0.08f)
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
                }

                if (Vector3.Distance(pt, endpt) < 2.0f)
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

        return m_sprites;
    }
}
