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

    public SpriteGroup sprite_group;

    public List<TextMesh> TextOutlines;
    ProvinceWidget m_widget;
    List<ProvinceWrapMarker> m_wraps;
    Node m_node;
    List<ProvinceMarker> m_linked;
    Vector3 m_dummy_offset = Vector3.zero;
    Vector3 m_poly_center = Vector3.zero;
    List<Vector3> m_poly;
    List<Vector3> m_sprite_points;
    List<ConnectionMarker> m_connections;
    Dictionary<Terrain, Color> m_colors;
    bool m_selected = false;
    bool m_is_dummy = false;
    float m_scale = 1.0f;
    float m_center_size = 9000f;
    bool m_needs_regen = false;

    public bool NeedsRegen
    {
        get
        {
            return m_needs_regen;
        }
    }

    public bool IsDummy
    {
        get
        {
            return m_is_dummy;
        }
    }

    public Node Node
    {
        get
        {
            return m_node;
        }
    }

    public Vector3 DummyOffset
    {
        get
        {
            return m_dummy_offset;
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

    public List<ProvinceMarker> LinkedProvinces
    {
        get
        {
            return m_linked;
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

    public void SetDummy(bool b, ProvinceMarker owner)
    {
        m_is_dummy = b;
        m_dummy_offset = (transform.position - owner.transform.position);
    }

    public void AddLinkedProvince(ProvinceMarker m)
    {
        if (m_linked == null)
        {
            m_linked = new List<ProvinceMarker>();
        }

        if (m_linked.Contains(m))
        {
            return;
        }

        m_linked.Add(m);
        SetNode(m.Node);
    }

    public void ShowLabel(bool on)
    {
        if (Text.text != string.Empty)
        {
            if (on)
            {
                Vector3 pos = Text.gameObject.transform.localPosition;
                pos.y = 0f;

                Text.gameObject.transform.localPosition = pos;
                Text.fontSize = 44;
                Text.gameObject.layer = 9;

                float add_x = -0.01f;
                float add_y = -0.01f;

                foreach (TextMesh m in TextOutlines)
                {
                    pos = m.gameObject.transform.localPosition;
                    pos.y = add_y;
                    pos.x = add_x;

                    if (add_y > 0)
                    {
                        add_x = 0.01f;
                    }

                    add_y = -add_y;

                    m.gameObject.transform.localPosition = pos;
                    m.fontSize = 44;
                    m.gameObject.layer = 9;
                }
            }
            else
            {
                Vector3 pos = Text.gameObject.transform.localPosition;
                pos.y = 0.16f;

                Text.gameObject.transform.localPosition = pos;
                Text.fontSize = 34;
                Text.gameObject.layer = 8;

                float add_x = -0.01f;
                float add_y = -0.01f;

                foreach (TextMesh m in TextOutlines)
                {
                    pos = m.gameObject.transform.localPosition;
                    pos.y = 0.16f + add_y;
                    pos.x = add_x;

                    if (add_y > 0)
                    {
                        add_x = 0.01f;
                    }

                    add_y = -add_y;

                    m.gameObject.transform.localPosition = pos;
                    m.fontSize = 34;
                    m.gameObject.layer = 8;
                }
            }
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
        UpdateLinked();

        if (m_widget == null)
        {
            return;
        }

        m_widget.SetNode(n);
    }

    public void UpdateLabel()
    {
        if (m_node.HasNation)
        {
            Text.gameObject.SetActive(true);
            Text.text = m_node.Nation.NationData.Name;
            Text.color = new Color(1.0f, 0.5f, 1.0f); 
        }
        else if (m_node.ProvinceData.IsThrone)
        {
            Text.gameObject.SetActive(true);
            Text.text = "Throne";
            Text.color = new Color(1.0f, 0.3f, 0.3f); 
        }
        else
        {
            Text.gameObject.SetActive(false);
            Text.text = string.Empty;
        }

        foreach (TextMesh m in TextOutlines)
        {
            if (Text.text == string.Empty)
            {
                m.gameObject.SetActive(false);
            }
            else
            {
                m.gameObject.SetActive(true);
            }

            m.text = Text.text;
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

    public void UpdateLinked()
    {
        if (!IsDummy && LinkedProvinces != null && LinkedProvinces.Any())
        {
            foreach (ProvinceMarker pm in LinkedProvinces)
            {
                pm.UpdateColor();
            }
        }
    }

    public void ValidateConnections()
    {
        if (Node.ProvinceData.Terrain.IsFlagSet(Terrain.SEA))
        {
            foreach (ConnectionMarker c in m_connections)
            {
                if (c.Connection.ConnectionType != ConnectionType.STANDARD)
                {
                    c.UpdateConnection(ConnectionType.STANDARD);
                }
            }
        }
    }

    public void OffsetWidget()
    {
        if (m_widget == null)
        {
            return;
        }

        m_widget.transform.position = transform.position + new Vector3(500f, 0f, 0f);
    }

    public void UpdateColor()
    {
        Renderer.color = get_node_color(m_node);

        assign_mat(GenerationManager.s_generation_manager.Season);

        if (m_widget == null)
        {
            return;
        }

        m_widget.SetNode(m_node);
    }

    Vector3 get_weighted_center(Vector3 p1, Vector3 p2, Node n1, Node n2)
    {
        Vector3 dir1 = (p1 - p2).normalized;
        Vector3 dir2 = (p2 - p1).normalized;
        Vector3 center = (p1 + p2) / 2;
        float dist = Vector3.Distance(center, p1);

        if (n1.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV) || n1.Connections.Count == 4)
        {
            center += (dir2 * (dist * 0.16f));
        }
        if (n2.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV) || n2.Connections.Count == 4)
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

    public void SetWidget(ProvinceWidget w)
    {
        m_widget = w;
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
        if (m_is_dummy)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            ProvinceManager.s_province_manager.SetProvince(this);

            SetSelected(true);
        }
    }

    public void SetSelected(bool b)
    {
        m_widget.SetSelected(b);
        m_selected = b;
        m_scale = 1.0f;
        Renderer.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
    }

    public void ClearWrapMeshes()
    {
        if (m_wraps == null)
        {
            m_wraps = new List<ProvinceWrapMarker>();
        }
        else
        {
            foreach (ProvinceWrapMarker m in m_wraps)
            {
                if (m != null)
                {
                    m.Delete();
                }
            }

            m_wraps = new List<ProvinceWrapMarker>();
        }
    }

    List<GameObject> create_connection_wraps(ProvinceMarker pm, Vector3 offset)
    {
        List<GameObject> objs = new List<GameObject>();

        if (IsDummy)
        {
            return objs;
        }

        foreach (ConnectionMarker m in pm.Connections)
        {
            GameObject obj = m.CreateWrapMesh(m.PolyBorder, offset);

            if (obj != null)
            {
                objs.Add(obj);
            } 
        }

        return objs;
    }

    public List<GameObject> CreateWrapMeshes()
    {
        if (IsDummy)
        {
            new List<GameObject>();
        }

        if (m_wraps == null)
        {
            m_wraps = new List<ProvinceWrapMarker>();
        }

        List<GameObject> objs = new List<GameObject>();

        if (LinkedProvinces == null || !LinkedProvinces.Any()) // case for standard provinces
        {
            if (ConnectedProvinces.Any(x => x.IsDummy))
            {
                List<ProvinceMarker> mars = ConnectedProvinces.Where(x => x.IsDummy);
                List<Vector3> offsets = new List<Vector3>();

                foreach (ProvinceMarker pm in mars)
                {
                    if (offsets.Any(x => Vector3.Distance(x, pm.DummyOffset) < 0.01f))
                    {
                        continue;
                    }

                    offsets.Add(pm.DummyOffset);

                    GameObject obj = GameObject.Instantiate(WrapMarkerPrefab);
                    ProvinceWrapMarker wrap = obj.GetComponent<ProvinceWrapMarker>();

                    wrap.SetParent(this);
                    wrap.SetNode(m_node);
                    wrap.CreatePoly(m_poly, -pm.DummyOffset);
                    obj.transform.position = obj.transform.position - pm.DummyOffset;

                    m_wraps.Add(wrap);
                    objs.Add(obj);
                    objs.AddRange(create_connection_wraps(pm, -pm.DummyOffset));
                }
            }
        }
        else // case for provinces with linked dummy nodes
        {
            List<ProvinceMarker> ignore = new List<ProvinceMarker>();

            foreach (ProvinceMarker pm in LinkedProvinces)
            {
                GameObject obj = GameObject.Instantiate(WrapMarkerPrefab);
                ProvinceWrapMarker wrap = obj.GetComponent<ProvinceWrapMarker>();

                wrap.SetParent(this);
                wrap.SetNode(m_node);
                wrap.CreatePoly(m_poly, pm.DummyOffset);
                obj.transform.position = obj.transform.position + pm.DummyOffset;

                m_wraps.Add(wrap);
                ignore.Add(pm);
                objs.Add(obj);

                if (pm.DummyOffset.x > 0f && pm.DummyOffset.y > 0f)
                {
                    objs.AddRange(create_connection_wraps(pm, new Vector3(-pm.DummyOffset.x, 0f)));
                    objs.AddRange(create_connection_wraps(pm, new Vector3(0f, -pm.DummyOffset.y)));
                }
            }

            if (ConnectedProvinces.Any(x => x.IsDummy && !ignore.Contains(x)))
            {
                List<ProvinceMarker> mars = ConnectedProvinces.Where(x => x.IsDummy && !ignore.Contains(x));

                foreach (ProvinceMarker pm in mars)
                {
                    if (pm.DummyOffset.x > 0f && pm.DummyOffset.y > 0f && mars.Count > 1)
                    {
                        continue;
                    }

                    GameObject obj = GameObject.Instantiate(WrapMarkerPrefab);
                    ProvinceWrapMarker wrap = obj.GetComponent<ProvinceWrapMarker>();

                    wrap.SetParent(this);
                    wrap.SetNode(m_node);
                    wrap.CreatePoly(m_poly, -pm.DummyOffset);
                    obj.transform.position = obj.transform.position - pm.DummyOffset;

                    m_wraps.Add(wrap);
                    objs.Add(obj);
                    objs.AddRange(create_connection_wraps(pm, -pm.DummyOffset));
                }
            }
        }

        return objs;
    }

    public void RecalculatePoly()
    {
        m_poly = new List<Vector3>();
 
        if (IsDummy)
        {
            return;
        }

        List<PolyBorder> borders = new List<PolyBorder>();

        List<ConnectionMarker> conns = new List<ConnectionMarker>();
        conns.AddRange(m_connections);

        if (m_linked != null)
        {
            foreach (ProvinceMarker pm in m_linked)
            {
                conns.AddRange(pm.Connections);
            }
        }

        if (Node.X == 0 && Node.Y == 0) // corner case
        {
            foreach (ConnectionMarker cm in Connections) // main connections
            {
                borders.Add(cm.PolyBorder);
            }

            foreach (ProvinceMarker pm in LinkedProvinces) // grab all children connections
            {
                foreach (ConnectionMarker cm in pm.Connections) 
                {
                    borders.Add(cm.GetOffsetBorder(-cm.DummyOffset));
                }
            }
        }
        else // bot side or left side case
        {
            foreach (ConnectionMarker cm in conns)
            {
                if (cm.IsEdge && (Node.X == 0 || Node.Y == 0) && Vector3.Distance(cm.Midpoint, transform.position) > 3f)
                {
                    borders.Add(cm.GetOffsetBorder(-cm.DummyOffset));
                }
                else
                {
                    borders.Add(cm.PolyBorder);
                }
            }
        }

        m_needs_regen = false;
        Vector3 cur = borders[0].P2;
        PolyBorder b = null;
        List<PolyBorder> result = new List<PolyBorder>();

        while (borders.Any())
        {
            b = borders.FirstOrDefault(x => Vector3.Distance(x.P1, cur) < 0.01f);

            if (b == null)
            {
                b = borders.FirstOrDefault(x => Vector3.Distance(x.P2, cur) < 0.01f);

                if (b == null) // this probably should never happen 
                {
                    m_needs_regen = true;
                    Debug.LogError("Unable to find PolyBorder for province marker: " + ProvinceNumber);
                    break;
                }

                PolyBorder rev = b.Reversed();

                result.Add(rev);
                borders.Remove(b);
                cur = rev.P2;
                b = null;
            }
            else
            {
                borders.Remove(b);
                cur = b.P2;
            }

            if (b != null)
            {
                result.Add(b);
            }
        }

        Vector3 avg = Vector3.zero;
        List<Vector3> simplified = new List<Vector3>();

        foreach (PolyBorder pb in result)
        {
            int mid1 = Mathf.RoundToInt(pb.OrderedPoints.Count * 0.33f);
            int mid2 = Mathf.RoundToInt(pb.OrderedPoints.Count * 0.66f);

            m_poly.AddRange(pb.GetFullLengthBorderMinusEnd());
            avg += pb.P1;
            avg += pb.OrderedPoints[mid1];
            avg += pb.OrderedPoints[mid2];
            simplified.Add(pb.P1);
            simplified.Add(pb.OrderedPoints[mid1]);
            simplified.Add(pb.OrderedPoints[mid2]);
        }

        Vector3 closest = new Vector3(9000, 9000);
        avg /= (result.Count * 3);
        avg -= new Vector3(0f, 0.2f); // shift the center down slightly for aesthetics
        simplified = simplified.OrderBy(x => Mathf.Abs(avg.y - x.y)).ToList();

        foreach (Vector3 p in simplified) // calculate the distance to left
        {
            if (p.x < avg.x)
            {
                m_center_size = Vector3.Distance(p, avg);
                break;
            }
        }

        foreach (Vector3 p in simplified) // calculate the distance to right
        {
            if (p.x > avg.x)
            {
                m_center_size += Vector3.Distance(p, avg);
                m_center_size *= 0.75f;
                break;
            }
        }

        m_poly_center = avg;
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

        Vector3[] norms = m.normals;
        bool valid = validate_solid(transform.position);

        for (int i = 0; i < norms.Length - 1; i++)
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
        
        assign_mat(GenerationManager.s_generation_manager.Season);
    }

    public void ClearSprites()
    {
        if (m_wraps != null)
        {
            foreach (ProvinceWrapMarker m in m_wraps)
            {
                m.ClearSprites();
            }
        }

        sprite_group.Clear();
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

        if (!set.MapSprites.Any())
        {
            return new List<SpriteMarker>();
        }

        List<ProvinceSprite> shuffled = set.MapSprites.Where(x => x.IsCenterpiece && m_node.ProvinceData.Terrain.IsFlagSet(x.ValidTerrain));

        if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV) && !m_node.ProvinceData.IsWater)
        {
            shuffled = set.MapSprites.Where(x => x.IsCenterpiece && x.ValidTerrain == Terrain.LARGEPROV);
        }

        shuffled.Shuffle();
        List<ProvinceSprite> centers = shuffled.OrderBy(x => 9000f - x.Size).ToList();

        if (centers.Any() && !IsDummy) // certain provinces have a centerpiece so place that first
        {
            foreach (ProvinceSprite ps in centers)
            {
                if (ps.Size < m_center_size)
                {
                    Vector3 pos = m_poly_center;
                    m_sprite_points = m_sprite_points.Where(x => !(Vector3.Distance(x, pos) < ps.Size * 0.5f || Mathf.Abs(pos.y - x.y) < 0.2f));
                    pos.z = -3;
                    sprite_group.PlaceSprite(ps, pos);

                    break;
                }
            }
        }

        set.MapSprites.Shuffle();
        List<ProvinceSprite> sprites = new List<ProvinceSprite>();

        foreach (ProvinceSprite ps in set.MapSprites) // guarantee that we have at least 1 of each valid sprite
        {
            if (!m_node.ProvinceData.Terrain.IsFlagSet(ps.ValidTerrain) || ps.IsCenterpiece || !m_sprite_points.Any())
            {
                continue;
            }
            sprites.Add(ps);
        }

        if (sprites.Count == 0)
        {
            return new List<SpriteMarker>();
        }
        {
            List<NativeObjectPlacer.Item> sprite_items = new List<NativeObjectPlacer.Item>();
            List<float> sprite_size = new List<float>();
            foreach (var s in sprites)
            {
                sprite_items.Add(new NativeObjectPlacer.Item
                {
                    size = s.Size,
                    spawn_chance = s.SpawnChance,
                    extra_border_dist = s.ValidTerrain == Terrain.LARGEPROV ? 1 : 0,
                });
            }
            List<Vector3> already_placed = new List<Vector3>();
            already_placed.AddRange(sprite_group.SpritePos());
            List<int> objs = NativeObjectPlacer.Invoke(sprite_items, m_sprite_points, m_poly, already_placed);
            for (int i = 0; i < objs.Count; ++i)
            {
                sprite_group.PlaceSprite(sprites[objs[i]], m_sprite_points[i]);
            }
            m_sprite_points.Clear();
        }

        List<SpriteMarker> all = new List<SpriteMarker>();

        foreach (ProvinceWrapMarker m in m_wraps)
        {
            all.AddRange(m.PlaceSprites());
        }

        sprite_group.Build(CaptureCam.Bounds());

        return all;
    }

    Vector3 get_valid_position()
    {
        foreach (Vector3 v in m_sprite_points)
        {
            if (!m_poly.Any(y => Vector3.Distance(new Vector3(v.x, v.y, 0f), y) < 0.38f))
            {
                return v;
            }
        }

        return new Vector3(-9000, -9000, 0);
    }

    public IEnumerable CalculateSpritePoints()
    {
        if (m_wraps != null)
        {
            foreach (ProvinceWrapMarker m in m_wraps)
            {
                m.CalculateSpritePoints();
                if (Util.ShouldYield()) yield return null;
            }
        }

        m_sprite_points = new List<Vector3>();
        Vector3 mins = get_mins();
        Vector3 maxs = get_maxs();
        Vector3 cur = new Vector3(mins.x, mins.y);
        List<ConnectionMarker> roads_rivers = m_connections.Where(x => x.Connection.ConnectionType == ConnectionType.ROAD || x.Connection.ConnectionType == ConnectionType.SHALLOWRIVER);
        MapSpriteSet set = ArtManager.s_art_manager.GetMapSpriteSet(m_node.ProvinceData.Terrain);

        while (cur.x < maxs.x)
        {
            while (cur.y < maxs.y)
            {
                var pt = cur;
                pt.z = -900;
                RaycastHit hit;

                if (MeshCollider.Raycast(new Ray(pt, Vector3.forward), out hit, 9000))
                {
                    m_sprite_points.Add(new Vector3(pt.x, pt.y, 0));
                }
                cur.y += UnityEngine.Random.Range(0.04f, 0.06f);
            }

            cur.y = mins.y + UnityEngine.Random.Range(0.04f, 0.06f);
            cur.x += 0.04f;
            if (Util.ShouldYield()) yield return null;
        }

        NativeCullNearby.Invoke(m_sprite_points, GetCullPoints(roads_rivers), set.ProvinceEdgeThreshold);
        for (int i = 0; i < m_sprite_points.Count; ++i) m_sprite_points[i] += new Vector3(0, 0, -10);

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

    List<Vector3> GetCullPoints(List<ConnectionMarker> roads_rivers)
    {
        List<Vector3> pts = new List<Vector3>();
        pts.AddRange(m_poly);
        foreach (ConnectionMarker m in roads_rivers)
        {
            pts.AddRange(m.CullingPoints);
        }
        return pts;
    }

    bool validate_solid(Vector3 pt)
    {
        RaycastHit hit;
        pt.z = -900f;

        if (Physics.Raycast(pt, Vector3.forward, out hit, 9000))
        {
            if (hit.collider == MeshCollider)
            {
                return true;
            }
        }

        return false;
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
        sprite_group.SetSeason(s);
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
