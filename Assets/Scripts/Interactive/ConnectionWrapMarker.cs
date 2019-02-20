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

    public GameObject MapSpritePrefab;

    PolyBorder m_border;
    List<Vector3> m_poly;
    ConnectionMarker m_parent;
    Connection m_connection;
    List<SpriteMarker> m_sprites;

    public void SetParent(ConnectionMarker parent)
    {
        m_parent = parent;
        m_connection = parent.Connection;
    }

    public void CreatePoly(List<Vector3> poly, PolyBorder pb, Vector3 offset)
    {
        m_poly = poly;

        draw_border(pb, offset);

        if (m_connection.ConnectionType == ConnectionType.ROAD || m_connection.ConnectionType == ConnectionType.RIVER || m_connection.ConnectionType == ConnectionType.SHALLOWRIVER)
        {
            ConstructPoly(pb, offset);
        }
        else
        {
            MeshFilter.mesh.Clear();
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

    public void ConstructPoly(PolyBorder pb, Vector3 offset)
    {
        MeshFilter.mesh.Clear();

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
            PolyBorder pb = m_border;//def.GetPolyBorder(m_connection);

            if (pb == null)
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

            foreach (Vector3 pt in pb.OrderedPoints)
            {
                if (Vector3.Distance(last, pt) < cs.Size && ct < pb.OrderedPoints.Count - 1)
                {
                    ct++;
                    continue;
                }

                ct++;
                Vector3 pos = pt;
                last = pt;
                pos.z = -3f;
                //pos.y -= 0.04f;

                GameObject g = GameObject.Instantiate(MapSpritePrefab);
                SpriteMarker sm = g.GetComponent<SpriteMarker>();
                sm.SetSprite(cs);
                sm.transform.position = pos;

                m_sprites.Add(sm);
                m_sprites.AddRange(sm.CreateMirrorSprites());

                cs = ArtManager.s_art_manager.GetConnectionSprite(m_connection.ConnectionType);

                while (UnityEngine.Random.Range(0f, 1f) > cs.SpawnChance)
                {
                    cs = ArtManager.s_art_manager.GetConnectionSprite(m_connection.ConnectionType);
                }
            }
        }
        else if (m_connection.ConnectionType == ConnectionType.MOUNTAINPASS)
        {
            PolyBorder pb = m_border;//def.GetPolyBorder(m_connection);

            if (pb == null)
            {
                return new List<SpriteMarker>();
            }

            m_sprites = new List<SpriteMarker>();

            Vector3 last = new Vector3(-900, -900, 0);
            ConnectionSprite cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

            if (cs == null)
            {
                return new List<SpriteMarker>();
            }

            while (UnityEngine.Random.Range(0f, 1f) > cs.SpawnChance)
            {
                cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
            }

            int ct = 0;
            int mid_ct = 0;
            int mid = Mathf.RoundToInt(pb.OrderedPoints.Count * 0.35f);
            int num_mid = UnityEngine.Random.Range(6, 10);//Mathf.RoundToInt(pb.OrderedPoints.Count * 0.20f);

            foreach (Vector3 pt in pb.OrderedPoints)
            {
                if (Vector3.Distance(last, pt) < cs.Size && ct < pb.OrderedPoints.Count - 1)
                {
                    ct++;
                    continue;
                }

                ct++;
                Vector3 pos = pt;
                last = pt;
                pos.z = -3f;

                GameObject g = GameObject.Instantiate(MapSpritePrefab);
                SpriteMarker sm = g.GetComponent<SpriteMarker>();
                sm.SetSprite(cs);
                sm.transform.position = pos;

                m_sprites.Add(sm);

                if (ct > mid && mid_ct < num_mid)
                {
                    mid_ct++;

                    cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAINPASS);

                    while (UnityEngine.Random.Range(0f, 1f) > cs.SpawnChance)
                    {
                        cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAINPASS);
                    }
                }
                else
                {
                    cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);

                    while (UnityEngine.Random.Range(0f, 1f) > cs.SpawnChance)
                    {
                        cs = ArtManager.s_art_manager.GetConnectionSprite(ConnectionType.MOUNTAIN);
                    }
                }
            }
        }
        else
        {
            m_sprites = new List<SpriteMarker>();
        }

        return m_sprites;
    }
}
