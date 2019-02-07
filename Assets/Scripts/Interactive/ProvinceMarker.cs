using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The behaviour class for provinces.
/// This class handles creation of province polygons and manages behaviour of the province's unity objects.
/// </summary>
public class ProvinceMarker: MonoBehaviour
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

    public GameObject WrapMarkerPrefab;
    public GameObject MapSpritePrefab;

    public SpriteRenderer Renderer;
    public TextMesh Text;
    public MeshRenderer Mesh;
    public MeshFilter MeshFilter;
    public MeshCollider MeshCollider;
    public GameObject MeshObj;

    List<ProvinceWrapMarker> m_wraps;
    Node m_node;
    List<Vector3> m_poly;
    List<Vector3> m_sprite_points;
    List<SpriteMarker> m_sprites;
    List<ConnectionMarker> m_connections;
    Dictionary<Terrain, Color> m_colors;
    bool m_selected = false;
    float m_scale = 1.0f;

    public Node Node
    {
        get
        {
            return m_node;
        }
    }

    public int ProvinceNumber
    {
        get
        {
            return m_node.ID;
        }
    }

    public List<ConnectionMarker> Connections
    {
        get
        {
            return m_connections;
        }
    }

    public List<ProvinceMarker> ConnectedProvinces
    {
        get
        {
            List<ProvinceMarker> provs = new List<ProvinceMarker>();

            foreach (ConnectionMarker m in m_connections)
            {
                if (m.Prov1 != this && !provs.Contains(m.Prov1))
                {
                    provs.Add(m.Prov1);
                }
                if (m.Prov2 != this && !provs.Contains(m.Prov2))
                {
                    provs.Add(m.Prov2);
                }
            }

            return provs;
        }
    }

    // Don't use this
    public List<ConnectionMarker> OrderedConnections
    {
        get
        {
            List<ConnectionMarker> result = new List<ConnectionMarker>();
            List<ConnectionMarker> temp = new List<ConnectionMarker>();
            temp.AddRange(m_connections.Where(x => x.TouchingProvince(this)));
            temp = temp.OrderBy(x => Vector3.Distance(x.transform.position, this.transform.position)).ToList();

            ConnectionMarker start = temp[0];
            temp.Remove(start);
            result.Add(start);

            while (temp.Any())
            {
                ConnectionMarker close = null;
                float dist = 9000f;

                foreach (ConnectionMarker m in temp)
                {
                    if (close == null || m.Connection.DistanceTo(start.Connection) < dist)
                    {
                        close = m;
                        dist = m.Connection.DistanceTo(start.Connection);
                    }
                }

                result.Add(close);
                temp.Remove(close);
                start = close;
            }

            return result;
        }
    }

    public void SetNode(Node n)
    {
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

        m_node = n;
        m_connections = new List<ConnectionMarker>();

        UpdateLabel();
        UpdateColor();
    }

    public void UpdateLabel()
    {
        if (m_node.HasNation)
        {
            Text.text = m_node.Nation.Name;
            Text.color = Color.black;
        }
        else if (m_node.ProvinceData.IsThrone)
        {
            Text.text = "THRONE";
            Text.color = Color.red;
        }
        else
        {
            Text.text = string.Empty;
        }
    }

    public float DistanceTo(Vector2 vec)
    {
        return Vector2.Distance(new Vector2(Node.X, Node.Y), vec);
    }

    public void AddConnection(ConnectionMarker m)
    {
        m_connections.Add(m);
    }

    public void UpdateConnections()
    {
        foreach (ConnectionMarker m in m_connections)
        {
            Vector3 center = get_weighted_center(m.Endpoint1, m.Endpoint2, m.Prov1.Node, m.Prov2.Node);
            m.gameObject.transform.position = center;
        }
    }

    public void UpdateColor()
    {
        Renderer.color = get_node_color(m_node);

        assign_mat(GenerationManager.s_generation_manager.Season);
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

    void Update()
    {
        if (m_selected)
        {
            m_scale = 1.1f + 0.3f * Mathf.Sin(Time.time * 5.5f);

            Renderer.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ProvinceManager.s_province_manager.SetProvince(this);

            SetSelected(true);
        }
    }

    public void SetSelected(bool b)
    {
        m_selected = b;
        m_scale = 1.0f;
        Renderer.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
    }

    public GameObject CreateWrapMesh(List<ConnectionMarker> conns, ConnectionMarker ext, DefaultArtStyle art)
    {
        GameObject obj = GameObject.Instantiate(WrapMarkerPrefab);
        ProvinceWrapMarker wrap = obj.GetComponent<ProvinceWrapMarker>();

        wrap.SetParent(this);
        wrap.SetNode(m_node);
        wrap.CreatePoly(conns, ext, art);

        Vector3 pos = obj.transform.position;
        pos.z = -1f;
        obj.transform.position = pos;

        if (m_wraps == null)
        {
            m_wraps = new List<ProvinceWrapMarker>();
        }

        m_wraps.Add(wrap);

        return obj;
    }

    public GameObject CreateWrapMesh(List<ConnectionMarker> conns, DefaultArtStyle art, bool is_corner = false)
    {
        GameObject obj = GameObject.Instantiate(WrapMarkerPrefab);
        ProvinceWrapMarker wrap = obj.GetComponent<ProvinceWrapMarker>();

        wrap.SetParent(this);
        wrap.SetNode(m_node);
        wrap.CreatePoly(conns, art, is_corner);

        Vector3 pos = obj.transform.position;
        pos.z = -1f;
        obj.transform.position = pos;

        if (m_wraps == null)
        {
            m_wraps = new List<ProvinceWrapMarker>();
        }

        m_wraps.Add(wrap);

        return obj;
    }

    public void RecalculatePoly()
    {
        m_poly = new List<Vector3>();
        Vector3 center = Vector3.zero;

        foreach (ConnectionMarker cm in m_connections)
        {
            foreach (Vector3 p in cm.TriCenters)
            {
                if (!m_poly.Any(x => Vector3.Distance(x, p) < 0.01f) && Vector3.Distance(p, transform.position) < 4.0f)
                {
                    m_poly.Add(p);
                }
            }
        }

        foreach (Vector3 pt in m_poly)
        {
            center += pt;
        }

        center /= m_poly.Count;

        m_poly = m_poly.OrderBy(x => Mathf.Atan2(x.y - center.y, x.x - center.x)).ToList();

        if (m_poly.Count < 3)
        {
            m_poly.Add(transform.position);
        }
    }

    public void RandomizePoly(DefaultArtStyle def)
    {
        List<Vector3> result = new List<Vector3>();
        Vector3 last = m_poly[m_poly.Count - 1];
        Connection lastconn = null;

        foreach (Vector3 v in m_poly)
        {
            bool found = false;

            foreach (ConnectionMarker m in m_connections)
            {
                if (m.TriCenters.Any(x => Vector3.Distance(x, v) < 0.01f) && m.TriCenters.Any(x => Vector3.Distance(x, last) < 0.01f))
                {
                    found = true;
                    lastconn = m.Connection;

                    if (m.IsEdge)
                    {
                        result.Add(last);
                    }
                    else
                    {
                        PolyBorder pb = def.AddPolyEdge(last, v, m.Connection);
                        result.Add(last);
                        result.AddRange(pb.OrderedPoints);
                    }

                    break;
                }
            }

            if (!found)
            {
                PolyBorder pb = def.AddPolyEdge(last, v, lastconn);
                result.Add(last);
                //result.AddRange(pb.OrderedPoints);
            }

            last = v;
        }

        m_poly = result;
    }

    public void ConstructPoly()
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
        MeshCollider.sharedMesh = m;
        MeshObj.transform.localPosition = transform.position * -1f;
        
        assign_mat(GenerationManager.s_generation_manager.Season);
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

        if (m_wraps != null)
        {
            foreach (ProvinceWrapMarker m in m_wraps)
            {
                m.ClearSprites();
            }
        }

        m_sprites = new List<SpriteMarker>();
    }

    public List<SpriteMarker> PlaceSprites()
    {
        ClearSprites();

        if (m_wraps != null)
        {
            foreach (ProvinceWrapMarker m in m_wraps)
            {
                m.PlaceSprites();
            }
        }

        MapSpriteSet set = ArtManager.s_art_manager.GetMapSpriteSet(m_node.ProvinceData.Terrain);

        foreach (ProvinceSprite ps in set.MapSprites) // guarantee that we have at least 1 of each valid sprite
        {
            if (!m_node.ProvinceData.Terrain.IsFlagSet(ps.ValidTerrain) || !m_sprite_points.Any())
            {
                continue;
            }

            Vector3 pos = m_sprite_points[0];
            List<Vector3> remove = m_sprite_points.Where(x => Vector3.Distance(x, pos) < ps.Size).ToList();

            foreach (Vector3 p in remove)
            {
                m_sprite_points.Remove(p);
            }

            pos.z = -3f;

            GameObject g = GameObject.Instantiate(MapSpritePrefab);
            SpriteMarker sm = g.GetComponent<SpriteMarker>();
            sm.SetSprite(ps);
            sm.transform.position = pos;

            m_sprites.Add(sm);
            m_sprites.AddRange(sm.CreateMirrorSprites());
        }

        while (m_sprite_points.Any()) // randomly sprinkle sprites
        {
            Vector3 pos = m_sprite_points[0];
            ProvinceSprite ps = ArtManager.s_art_manager.GetProvinceSprite(m_node.ProvinceData.Terrain);

            while (UnityEngine.Random.Range(0f, 1f) > ps.SpawnChance)
            {
                ps = ArtManager.s_art_manager.GetProvinceSprite(m_node.ProvinceData.Terrain);
            }

            List<Vector3> remove = m_sprite_points.Where(x => Vector3.Distance(x, pos) < ps.Size).ToList();

            foreach (Vector3 p in remove)
            {
                m_sprite_points.Remove(p);
            }

            pos.z = -3f;

            GameObject g = GameObject.Instantiate(MapSpritePrefab);
            SpriteMarker sm = g.GetComponent<SpriteMarker>();
            sm.SetSprite(ps);
            sm.transform.position = pos;

            m_sprites.Add(sm);
            m_sprites.AddRange(sm.CreateMirrorSprites());
        }

        return m_sprites;
    }

    public void CalculateSpritePoints()
    {
        if (m_wraps != null)
        {
            foreach (ProvinceWrapMarker m in m_wraps)
            {
                m.CalculateSpritePoints();
            }
        }

        m_sprite_points = new List<Vector3>();
        Vector3 mins = get_mins();
        Vector3 maxs = get_maxs();

        Vector3 cur = new Vector3(mins.x, mins.y);

        while (cur.x < maxs.x)
        {
            while (cur.y < maxs.y)
            {
                do_ray_trace(cur);

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

    static float MIN_DISTANCE = 0.10f;

    void do_ray_trace(Vector3 pt)
    {
        pt.z = -900;

        RaycastHit hit;

        if (Physics.Raycast(pt, Vector3.forward, out hit, 9000))
        {
            if (hit.collider == MeshCollider)
            {
                Vector3 hitpt = new Vector3(hit.point.x, hit.point.y, 0);

                if (m_poly.Any(x => Vector3.Distance(x, hitpt) < 0.15f))
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
        Vector3 result = new Vector3(9000, 9000);

        foreach (Vector3 p in m_poly)
        {
            if (result.x > p.x)
            {
                result.x = p.x;
            }
            if (result.y > p.y)
            {
                result.y = p.y;
            }
        }

        return result;
    }

    Vector3 get_maxs()
    {
        Vector3 result = new Vector3(-9000, -9000);

        foreach (Vector3 p in m_poly)
        {
            if (result.x < p.x)
            {
                result.x = p.x;
            }
            if (result.y < p.y)
            {
                result.y = p.y;
            }
        }

        return result;
    }

    public void SetSeason(Season s)
    {
        assign_mat(s);

        if (m_wraps != null)
        {
            foreach (ProvinceWrapMarker m in m_wraps)
            {
                m.SetSeason(s);
            }
        }
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

        Mesh.material.color = get_node_color(m_node);//SetColor("_Color", get_node_color(m_node));

        if (m_wraps != null)
        {
            foreach (ProvinceWrapMarker m in m_wraps)
            {
                m.UpdateMaterial();
            }
        }
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
}
