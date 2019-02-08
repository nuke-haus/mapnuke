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
    public LineRenderer BorderLine;

    public GameObject MapSpritePrefab;

    Node m_node;
    ProvinceMarker m_parent;
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

    public void CreatePoly(List<ConnectionMarker> conns, ConnectionMarker ext, DefaultArtStyle art)
    {
        m_connections = conns;

        RecalculatePoly(ext, art);
        ConstructPoly(ext);
    }

    public void CreatePoly(List<ConnectionMarker> conns, DefaultArtStyle art, bool is_corner)
    {
        m_is_corner = is_corner;
        m_connections = conns;

        RecalculatePoly(null, art);
        ConstructPoly(null);
    }

    public void RecalculatePoly(ConnectionMarker ext, DefaultArtStyle art)
    {
        m_poly = new List<Vector3>();

        foreach (ConnectionMarker cm in m_connections)
        {
            foreach (Vector3 p in cm.TriCenters)
            {
                if (!m_poly.Any(x => Vector3.Distance(x, p) < 0.01f))
                {
                    m_poly.Add(p);
                }
            }

            if (cm.TriCenters.Count == 3) // corner case
            {
                m_poly = m_poly.OrderBy(x => x.y).ThenBy(x => 9000f - x.x).ToList();

                List<Vector3> vecs = new List<Vector3>();

                PolyBorder pb = art.AddPolyEdge(m_poly[2], m_poly[0], cm.Connection);
                m_poly.AddRange(pb.OrderedPoints);

                return;
            }
        }

        List<Vector3> reversi = new List<Vector3>();
        reversi.AddRange(m_poly);
        reversi = reversi.OrderBy(x => x.y).ThenBy(x => x.x).ToList();
        reversi.Reverse();

        Vector3 farpt = Vector3.negativeInfinity;

        if (ext != null)
        {
            Vector3 pos = ext.EdgePoint;
            farpt = m_poly.FirstOrDefault(x => Mathf.Abs(x.y - pos.y) < 0.01f || Mathf.Abs(x.x - pos.x) < 0.01f);
            Vector3 mid = (pos + farpt) / 2;

            m_poly.Add(mid);
        }

        m_poly = m_poly.OrderBy(x => x.y).ThenBy(x => x.x).ToList();        

        if (ext != null)
        {
            int ct = -1;
            Vector3 last = Vector3.negativeInfinity;
            Vector3 last_offset = Vector3.negativeInfinity;
            List<Vector3> reverse = new List<Vector3>();
            reverse.AddRange(m_poly.Where(x => Vector3.Distance(x, farpt) > 0.01f));
            reverse.Reverse();

            List<ConnectionMarker> used = new List<ConnectionMarker>();

            foreach (Vector3 vec in reverse) 
            {
                ct++;

                if (ct == 0)
                {
                    last = vec;
                    last_offset = vec;
                    continue;
                }

                ConnectionMarker m = m_connections.FirstOrDefault(x => x.TriCenters.Any(y => Vector3.Distance(vec, y) < 0.01f) && x.TriCenters.Any(y => Vector3.Distance(last, y) < 0.01f));

                if (m == null && m_connections.Any())
                {
                    m = m_connections.FirstOrDefault(x => !used.Contains(x));
                }

                if (m != null)
                {
                    used.Add(m);
                    Vector3 p1 = m.Endpoint1;

                    if (Vector3.Distance(p1, m.EdgePoint) < 0.01f)
                    {
                        p1 = m.Endpoint2;
                    }

                    Vector3 dir = (p1 - m.EdgePoint).normalized;
                    Vector3 offset = vec + dir * UnityEngine.Random.Range(0.05f, 0.25f);

                    if (ct == reverse.Count - 1)
                    {
                        offset = vec;
                    }

                    PolyBorder pb = art.AddPolyEdge(last_offset, offset, dir, m.Connection);
                    m_poly.AddRange(pb.OrderedPoints);

                    last = vec;
                    last_offset = offset;
                }
            }
        }
        else
        {
            int ct = -1;
            Vector3 last = Vector3.negativeInfinity;
            Vector3 last_offset = Vector3.negativeInfinity;
            List<Vector3> reverse = new List<Vector3>();
            reverse.AddRange(m_poly);
            reverse.Reverse();

            foreach (Vector3 vec in reverse)
            {
                ct++;

                if (ct == 0)
                {
                    last = vec;
                    last_offset = vec;
                    continue;
                }

                ConnectionMarker m = m_connections.FirstOrDefault(x => x.TriCenters.Any(y => Vector3.Distance(vec, y) < 0.01f) && x.TriCenters.Any(y => Vector3.Distance(last, y) < 0.01f));

                if (m != null)
                {
                    Vector3 p1 = m.Endpoint1;

                    if (Vector3.Distance(p1, m.EdgePoint) < 0.01f)
                    {
                        p1 = m.Endpoint2;
                    }

                    Vector3 dir = (p1 - m.EdgePoint).normalized;
                    Vector3 offset = vec + dir * UnityEngine.Random.Range(0.05f, 0.25f);

                    if (ct == reverse.Count - 1)
                    {
                        offset = vec;
                    }

                    PolyBorder pb = art.AddPolyEdge(last_offset, offset, dir, m.Connection);
                    m_poly.AddRange(pb.OrderedPoints);

                    last = vec;
                    last_offset = offset;
                }
            }
        }
    }

    /*public void RandomizePoly(DefaultArtStyle def)
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
                result.AddRange(pb.OrderedPoints);
            }

            last = v;
        }

        m_poly = result;
    }*/

    public void ConstructPoly(ConnectionMarker ext)
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

        calculate_uv(m, ext);

        m.RecalculateNormals();
        m.RecalculateBounds();

        MeshFilter.mesh = m;
        MeshCollider.sharedMesh = m;
        MeshObj.transform.localPosition = transform.position * -1f;

        assign_mat();
    }

    void calculate_uv(Mesh m, ConnectionMarker ext)
    {
        Vector2[] uv = new Vector2[m_poly.Count];
        Vector2 offset = Vector2.zero;
        Vector3 max = MapBorder.s_map_border.Maxs;
        Vector3 min = MapBorder.s_map_border.Mins;
        max.x = max.x + Mathf.Abs(min.x);
        max.y = max.y + Mathf.Abs(min.y);

        if (IsCorner)
        {
            offset.x = -max.x;
            offset.y = -max.y;
        }
        else if (m_node.X == 0 && m_node.Y == 0)
        {
            if (ext.Connection.Pos.x < 0)
            {
                offset.y = -max.y;
            }
            else
            {
                offset.x = -max.x;
            }
        }
        else if (m_node.X == 0)
        {
            offset.x = -max.x;
        }
        else
        {
            offset.y = -max.y;
        }

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
            if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.SEA))
            {
                Mesh.material = MatSea;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.DEEPSEA))
            {
                Mesh.material = MatDeepSea;
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
            if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.SEA))
            {
                Mesh.material = MatWinterSea;
            }
            else if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.DEEPSEA))
            {
                Mesh.material = MatWinterDeepSea;
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
        ClearSprites();

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
            m_sprites.AddRange(sm.CreateMirrorSprites());
        }

        return m_sprites;
    }

    public void CalculateSpritePoints()
    {
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
}
