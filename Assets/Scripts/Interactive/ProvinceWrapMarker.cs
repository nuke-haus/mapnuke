using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The behaviour class for edge province chunks.
/// This class handles creation of polygons and manages behaviour of province unity objects.
/// </summary>
public class ProvinceWrapMarker : MonoBehaviour
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
    private Node m_node;
    private ProvinceMarker m_parent;
    private List<Vector3> m_poly;
    private List<Vector3> m_sprite_points;
    private List<SpriteMarker> m_sprites;
    private readonly List<ConnectionMarker> m_connections;
    private Dictionary<Terrain, Color> m_colors;
    private readonly bool m_is_corner = false;

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

    public void UpdateArtStyle()
    {
        var art_config = ArtManager.s_art_manager.CurrentArtConfiguration;

        MatSwamp = art_config.MatSwamp;
        MatForest = art_config.MatForest;
        MatWaste = art_config.MatWaste;
        MatMountain = art_config.MatMountain;
        MatHighland = art_config.MatHighland;
        MatCave = art_config.MatCave;
        MatFarm = art_config.MatFarm;
        MatPlains = art_config.MatPlains;
        MatSea = art_config.MatSea;
        MatDeepSea = art_config.MatDeepSea;

        MatWinterSwamp = art_config.MatWinterSwamp;
        MatWinterForest = art_config.MatWinterForest;
        MatWinterWaste = art_config.MatWinterWaste;
        MatWinterMountain = art_config.MatWinterMountain;
        MatWinterHighland = art_config.MatWinterHighland;
        MatWinterCave = art_config.MatWinterCave;
        MatWinterFarm = art_config.MatWinterFarm;
        MatWinterPlains = art_config.MatWinterPlains;
        MatWinterSea = art_config.MatWinterSea;
        MatWinterDeepSea = art_config.MatWinterDeepSea;
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
            m_colors = new Dictionary<Terrain, Color>
            {
                { Terrain.DEEPSEA, new Color(0.2f, 0.3f, 0.9f) },
                { Terrain.SEA, new Color(0.4f, 0.6f, 0.9f) },
                { Terrain.FARM, new Color(0.9f, 0.8f, 0.2f) },
                { Terrain.SWAMP, new Color(0.6f, 0.8f, 0.1f) },
                { Terrain.WASTE, new Color(0.6f, 0.4f, 0.3f) },
                { Terrain.MOUNTAINS, new Color(0.4f, 0.3f, 0.4f) },
                { Terrain.HIGHLAND, new Color(0.5f, 0.5f, 0.7f) },
                { Terrain.FOREST, new Color(0.1f, 0.4f, 0.1f) },
                { Terrain.CAVE, new Color(0.1f, 0.4f, 0.5f) }
            };
        }
    }

    public void CreatePoly(List<Vector3> poly, Vector3 offset)
    {
        m_poly = poly;

        ConstructPoly();

        m_poly = new List<Vector3>();

        foreach (var p in poly)
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

        var tr = new Triangulator(get_pts_array(m_poly));
        var indices = tr.Triangulate();

        var m = new Mesh
        {
            vertices = m_poly.ToArray(),
            triangles = indices
        };

        calculate_uv(m, Vector3.zero);

        m.RecalculateNormals();
        m.RecalculateBounds();

        var norms = m.normals;
        var valid = validate_solid(transform.position);

        for (var i = 0; i < norms.Length - 1; i++)
        {
            if (valid)
            {
                norms[i] = Vector3.back;
            }
            else
            {
                norms[i] = Vector3.forward;
            }
        }

        m.normals = norms;

        MeshFilter.mesh = m;
        MeshCollider.sharedMesh = m;
        MeshObj.transform.localPosition = transform.position * -1f;

        assign_mat();
    }

    private bool validate_solid(Vector3 pt)
    {
        pt.z = -900f;

        if (Physics.Raycast(pt, Vector3.forward, out var hit, 9000))
        {
            if (hit.collider == MeshCollider)
            {
                return true;
            }
        }

        return false;
    }

    private void calculate_uv(Mesh m, Vector3 offset)
    {
        var uv = new Vector2[m_poly.Count];

        for (var i = 0; i < m_poly.Count; i++)
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

    private void assign_mat(Season s = Season.SUMMER)
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

    public void Delete()
    {
        if (m_sprites != null)
        {
            foreach (var sm in m_sprites)
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
            foreach (var sm in m_sprites)
            {
                GameObject.Destroy(sm.gameObject);
            }
        }

        m_sprites = new List<SpriteMarker>();
    }

    public List<SpriteMarker> PlaceSprites()
    {
        var set = ArtManager.s_art_manager.GetMapSpriteSet(m_node.ProvinceData.Terrain);

        if (!set.MapSprites.Any())
        {
            return m_sprites;
        }

        var valid_ct = set.MapSprites.Where(x => m_node.ProvinceData.Terrain.IsFlagSet(x.ValidTerrain)).Count();

        if (valid_ct == 0)
        {
            return m_sprites;
        }

        while (m_sprite_points.Any())
        {
            var pos = m_sprite_points[0];
            var ps = ArtManager.s_art_manager.GetProvinceSprite(m_node.ProvinceData.Terrain);

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

            var remove = m_sprite_points.Where(x => Vector3.Distance(x, pos) < ps.Size).ToList();

            foreach (var p in remove)
            {
                m_sprite_points.Remove(p);
            }

            pos.z = -3f;

            var g = GameObject.Instantiate(MapSpritePrefab);
            var sm = g.GetComponent<SpriteMarker>();
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
        var mins = get_mins();
        var maxs = get_maxs();
        var cur = new Vector3(mins.x, mins.y);
        var set = ArtManager.s_art_manager.GetMapSpriteSet(m_node.ProvinceData.Terrain);

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
        var result = new List<Vector3>();

        var cull = ArtManager.s_art_manager.CurrentArtConfiguration.GetCullChance(m_node.ProvinceData.Terrain);
        var cullcount = Mathf.RoundToInt((1.0f - cull) * m_sprite_points.Count);

        for (var i = 0; i < cullcount; i++)
        {
            result.Add(m_sprite_points[i]);
        }

        m_sprite_points = result;
    }

    private void do_ray_trace(Vector3 pt, MapSpriteSet set)
    {
        pt.z = -900;

        if (Physics.Raycast(pt, Vector3.forward, out var hit, 9000))
        {
            if (hit.collider == MeshCollider)
            {
                var hitpt = new Vector3(hit.point.x, hit.point.y, 0);

                if (m_poly.Any(x => Vector3.Distance(x, hitpt) < set.ProvinceEdgeThreshold))
                {
                    return;
                }

                hitpt.z = -10;
                m_sprite_points.Add(hitpt);
            }
        }
    }

    private Vector3 get_mins()
    {
        var result = MeshCollider.bounds.min;
        result.z = 0f;
        var mins = MapBorder.s_map_border.Mins;

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

    private Vector3 get_maxs()
    {
        var result = MeshCollider.bounds.max;
        result.z = 0f;
        var maxs = MapBorder.s_map_border.Maxs;

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
