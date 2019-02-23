using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The behaviour class for edge province chunks.
/// This class handles creation of polygons and manages behaviour of province unity objects.
/// </summary>
public class ProvinceWrapMarker: MonoBehaviour
{
    public Material MatSwamp;
    public Material MatForest;
    public Material MatWaste;
    public Material MatMountain;
    public Material MatHighland;
    public Material MatCave;
    public Material MatFarm;
    public Material MatPlains;
    public Material MatSea;
    public Material MatDeepSea;

    public Material MatWinterSwamp;
    public Material MatWinterForest;
    public Material MatWinterWaste;
    public Material MatWinterMountain;
    public Material MatWinterHighland;
    public Material MatWinterCave;
    public Material MatWinterFarm;
    public Material MatWinterPlains;
    public Material MatWinterSea;
    public Material MatWinterDeepSea;

    public MeshRenderer Mesh;
    public MeshFilter MeshFilter;
    public MeshCollider MeshCollider;
    public GameObject MeshObj;

    public GameObject MapSpritePrefab;

    Node m_node;
    ProvinceMarker m_parent;
    Vector3 m_offset;
    List<Vector3> m_poly;
    List<Vector3> m_sprite_points;
    List<SpriteMarker> m_sprites;
    List<ConnectionMarker> m_connections;
    Dictionary<Terrain, Color> m_colors;
    bool m_is_corner = false;

    public Node Node
    {
        get
        {
            return m_node;
        }
    }

    public ProvinceMarker Parent
    {
        get
        {
            return m_parent;
        }
    }

    public bool IsCorner
    {
        get
        {
            return m_is_corner;
        }
    }

    public void SetParent(ProvinceMarker pm)
    {
        m_parent = pm;
    }

    public void SetNode(Node n)
    {
        m_node = n;

        if (m_colors == null)
        {
            Dictionary<Terrain, Color> dict = new Dictionary<Terrain, Color>();
            dict.Add(Terrain.DEEPSEA, new Color(0.2f, 0.3f, 0.9f));
            dict.Add(Terrain.SEA, new Color(0.4f, 0.6f, 0.9f));
            dict.Add(Terrain.FARM, new Color(0.9f, 0.8f, 0.2f));
            dict.Add(Terrain.SWAMP, new Color(0.6f, 0.8f, 0.1f));
            dict.Add(Terrain.WASTE, new Color(0.6f, 0.4f, 0.3f));
            dict.Add(Terrain.MOUNTAINS, new Color(0.4f, 0.3f, 0.4f));
            dict.Add(Terrain.HIGHLAND, new Color(0.5f, 0.5f, 0.7f));
            dict.Add(Terrain.FOREST, new Color(0.1f, 0.4f, 0.1f));
            dict.Add(Terrain.CAVE, new Color(0.1f, 0.4f, 0.5f));

            m_colors = dict;
        }
    }

    public void CreatePoly(List<Vector3> poly, Vector3 offset)
    {
        m_poly = poly; 
        m_offset = offset;
        
        ConstructPoly();

        m_poly = new List<Vector3>();

        foreach (Vector3 p in poly)
        {
            m_poly.Add(p + offset);
        }
    }

    public void ConstructPoly()
    {
        if (m_poly == null || !m_poly.Any())
        {
            return;
        }

        Triangulator tr = new Triangulator(get_pts_array(m_poly));
        int[] indices = tr.Triangulate();

        Mesh m = new Mesh();
        m.vertices = m_poly.ToArray();
        m.triangles = indices;

        calculate_uv(m, Vector3.zero);

        m.RecalculateNormals();
        m.RecalculateBounds();

        MeshFilter.mesh = m;
        MeshCollider.sharedMesh = m;
        MeshObj.transform.localPosition = transform.position * -1f;

        assign_mat();
    }

    void calculate_uv(Mesh m, Vector3 offset)
    {
        Vector2[] uv = new Vector2[m_poly.Count];

        for (int i = 0; i < m_poly.Count; i++)
        {
            uv[i] = new Vector2(m_poly[i].x + offset.x, m_poly[i].y + offset.y);
        }

        m.uv = uv;
    }

    public void UpdateMaterial()
    {
        assign_mat();
    }

    public void SetSeason(Season s)
    {
        assign_mat(s);
    }

    void assign_mat(Season s = Season.SUMMER)
    {
        if (s == Season.SUMMER)
        {
            if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.DEEPSEA))
            {
                Mesh.material = MatDeepSea;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.SEA))
            {
                Mesh.material = MatSea;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.FARM))
            {
                Mesh.material = MatFarm;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.SWAMP))
            {
                Mesh.material = MatSwamp;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.CAVE))
            {
                Mesh.material = MatCave;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.FOREST))
            {
                Mesh.material = MatForest;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.HIGHLAND))
            {
                Mesh.material = MatHighland;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.MOUNTAINS))
            {
                Mesh.material = MatMountain;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.WASTE))
            {
                Mesh.material = MatWaste;
            }
            else
            {
                Mesh.material = MatPlains;
            }
        }
        else
        {
            if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.DEEPSEA))
            {
                Mesh.material = MatWinterDeepSea;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.SEA))
            {
                Mesh.material = MatWinterSea;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.FARM))
            {
                Mesh.material = MatWinterFarm;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.SWAMP))
            {
                Mesh.material = MatWinterSwamp;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.CAVE))
            {
                Mesh.material = MatWinterCave;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.FOREST))
            {
                Mesh.material = MatWinterForest;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.HIGHLAND))
            {
                Mesh.material = MatWinterHighland;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.MOUNTAINS))
            {
                Mesh.material = MatWinterMountain;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.WASTE))
            {
                Mesh.material = MatWinterWaste;
            }
            else
            {
                Mesh.material = MatWinterPlains;
            }
        }

        //Mesh.material.color = get_node_color(m_node);//SetColor("_Color", get_node_color(m_node));
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

    Color get_node_color(Node n)
    {
        Terrain t = n.ProvinceData.Terrain;

        foreach (KeyValuePair<Terrain, Color> pair in m_colors)
        {
            if (t.IsFlagSet(pair.Key))
            {
                return pair.Value;
            }
        }

        return new Color(0.9f, 0.9f, 0.8f); //default is plains
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

        GameObject.Destroy(gameObject);
    }

    public void ClearSprites()
    {
        if (m_sprites != null)
        {
            foreach (SpriteMarker sm in m_sprites)
            {
                GameObject.Destroy(sm.gameObject);
            }
        }

        m_sprites = new List<SpriteMarker>();
    }

    public List<SpriteMarker> PlaceSprites()
    {
        MapSpriteSet set = ArtManager.s_art_manager.GetMapSpriteSet(m_node.ProvinceData.Terrain);

        if (!set.MapSprites.Any())
        {
            return m_sprites;
        }

        int valid_ct = set.MapSprites.Where(x => m_node.ProvinceData.Terrain.IsFlagSet(x.ValidTerrain)).Count();

        if (valid_ct == 0)
        {
            return m_sprites;
        }

        while (m_sprite_points.Any())
        {
            Vector3 pos = m_sprite_points[0];
            ProvinceSprite ps = ArtManager.s_art_manager.GetProvinceSprite(m_node.ProvinceData.Terrain);

            if (ps == null)
            {
                m_sprite_points.Remove(pos);
                continue;
            }

            while (UnityEngine.Random.Range(0f, 1f) > ps.SpawnChance)
            {
                ps = ArtManager.s_art_manager.GetProvinceSprite(m_node.ProvinceData.Terrain);

                if (ps == null)
                {
                    break;
                }
            }

            if (ps == null)
            {
                m_sprite_points.Remove(pos);
                continue;
            }

            List<Vector3> remove = m_sprite_points.Where(x => Vector3.Distance(x, pos) < ps.Size).ToList();

            foreach (Vector3 p in remove)
            {
                m_sprite_points.Remove(p);
            }

            //m_sprite_points.Remove(pos);
            pos.z = -3f;

            GameObject g = GameObject.Instantiate(MapSpritePrefab);
            SpriteMarker sm = g.GetComponent<SpriteMarker>();
            sm.SetSprite(ps);
            sm.transform.position = pos;

            m_sprites.Add(sm);
            m_sprites.AddRange(sm.CreateMirrorSprites(Vector3.zero, Vector3.zero));
        }

        return m_sprites;
    }

    public void CalculateSpritePoints()
    {
        m_sprite_points = new List<Vector3>();
        Vector3 mins = get_mins();
        Vector3 maxs = get_maxs();
        Vector3 cur = new Vector3(mins.x, mins.y);
        Vector3 cc = MeshCollider.bounds.center;
        //List<ConnectionMarker> roads = m_connections.Where(x => x.Connection.ConnectionType == ConnectionType.ROAD).ToList();
        MapSpriteSet set = ArtManager.s_art_manager.GetMapSpriteSet(m_node.ProvinceData.Terrain);

        while (cur.x < maxs.x)
        {
            while (cur.y < maxs.y)
            {
                do_ray_trace(cur, set);

                cur.y += UnityEngine.Random.Range(0.04f, 0.06f);
            }

            cur.y = mins.y + UnityEngine.Random.Range(0.04f, 0.06f);
            cur.x += 0.04f;
        }

        m_sprite_points.Shuffle();
        List<Vector3> result = new List<Vector3>();

        float cull = ArtManager.s_art_manager.SpriteSetCollection.GetCullChance(m_node.ProvinceData.Terrain);
        int cullcount = Mathf.RoundToInt((1.0f - cull) * m_sprite_points.Count);

        for (int i = 0; i < cullcount; i++)
        {
            result.Add(m_sprite_points[i]);
        }

        m_sprite_points = result;
    }

    void do_ray_trace(Vector3 pt, MapSpriteSet set)
    {
        pt.z = -900;
        RaycastHit hit;

        if (Physics.Raycast(pt, Vector3.forward, out hit, 9000))
        {
            if (hit.collider == MeshCollider)
            {
                Vector3 hitpt = new Vector3(hit.point.x, hit.point.y, 0);

                if (m_poly.Any(x => Vector3.Distance(x, hitpt) < set.ProvinceEdgeThreshold))
                {
                    return;
                }

                hitpt.z = -10;
                m_sprite_points.Add(hitpt);
            }
        }
    }

    Vector3 get_mins()
    {
        Vector3 result = MeshCollider.bounds.min;
        result.z = 0f;
        Vector3 mins = MapBorder.s_map_border.Mins;

        if (result.x < mins.x)
        {
            result.x = mins.x;
        }

        if (result.y < mins.y)
        {
            result.y = mins.y;
        }

        return result;
    }

    Vector3 get_maxs()
    {
        Vector3 result = MeshCollider.bounds.max;
        result.z = 0f;
        Vector3 maxs = MapBorder.s_map_border.Maxs;

        if (result.x > maxs.x)
        {
            result.x = maxs.x;
        }

        if (result.y > maxs.y)
        {
            result.y = maxs.y;
        }

        return result;
    }
}
