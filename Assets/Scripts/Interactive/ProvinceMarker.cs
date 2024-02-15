using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using UnityEngine;

/// <summary>
/// The behaviour class for provinces.
/// This class handles creation of province polygons and manages behaviour of the province's unity objects.
/// </summary>
public class ProvinceMarker : MonoBehaviour
{
    public Material MatUnderworldWall;
    public Material MatUnderworldNormal;
    public Material MatUnderworldForest;
    public Material MatUnderworldSwamp;
    public Material MatUnderworldHighland;
    public Material MatUnderworldSea;

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

    public Sprite NormalSprite;
    public Sprite CaveEntranceSprite;
    public SpriteRenderer Renderer;
    public TextMesh Text;
    public GameObject FortSprite;
    public MeshRenderer Mesh;
    public MeshFilter MeshFilter;
    public MeshCollider MeshCollider;
    public GameObject MeshObj;

    public SpriteGroup SpriteGroup;

    public List<TextMesh> TextOutlines;
    private ProvinceWidget m_widget;
    private List<ProvinceWrapMarker> m_wraps;
    private Node m_node;
    private List<ProvinceMarker> m_linked;
    private Vector3 m_dummy_offset = Vector3.zero;
    private Vector3 m_poly_center = Vector3.zero;
    private List<Vector3> m_poly;
    private List<Vector3> m_sprite_points;
    private List<ConnectionMarker> m_connections;
    private Dictionary<Terrain, Color> m_colors;
    private bool m_selected = false;
    private bool m_is_dummy = false;
    private float m_scale = 1.0f;
    private float m_center_size = 9000f;
    private bool m_needs_regen = false;

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
            var provs = new List<ProvinceMarker>();

            foreach (var m in m_connections)
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

    public void LockProvinceData(bool is_locked)
    {
        m_node.LockProvinceData(is_locked);
    }

    public void UpdateArtStyle()
    {
        var art_config = ArtManager.s_art_manager.CurrentArtConfiguration;

        MatUnderworldWall = art_config.MatUnderworldImpassable;
        MatUnderworldForest = art_config.MatUnderworldForest;
        MatUnderworldHighland = art_config.MatUnderworldHighland;
        MatUnderworldNormal = art_config.MatUnderworldCave;
        MatUnderworldSwamp = art_config.MatUnderworldSwamp;
        MatUnderworldSea = art_config.MatUnderworldSea;

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
        if (on && m_node.ProvinceData.Fort != Fort.NONE && !ArtManager.s_art_manager.IsUsingUnderworldTerrain)
        {
            FortSprite.SetActive(true);
            FortSprite.gameObject.layer = 9;
        }
        else
        {
            FortSprite.SetActive(false);
        }

        if (Text.text != string.Empty)
        {
            if (on)
            {
                var pos = Text.gameObject.transform.localPosition;
                pos.y = 0f;

                Text.gameObject.transform.localPosition = pos;
                Text.fontSize = 44;
                Text.gameObject.layer = 9;

                var add_x = -0.01f;
                var add_y = -0.01f;

                foreach (var m in TextOutlines)
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
                var pos = Text.gameObject.transform.localPosition;
                pos.y = 0.16f;

                Text.gameObject.transform.localPosition = pos;
                Text.fontSize = 34;
                Text.gameObject.layer = 8;

                var add_x = -0.01f;
                var add_y = -0.01f;

                foreach (var m in TextOutlines)
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

        m_node = n;
        m_connections = new List<ConnectionMarker>();

        UpdateSprite();
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

        foreach (var m in TextOutlines)
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

    /*public void UpdateConnections()
    {
        if (IsDummy)
        {
            return;
        }

        foreach (var m in m_connections)
        {
            var center = get_weighted_center(m.Endpoint1, m.Endpoint2, m.Prov1.Node, m.Prov2.Node);
            m.gameObject.transform.position = center;
        }
    }*/

    public void UpdateLinked()
    {
        if (!IsDummy && LinkedProvinces != null && LinkedProvinces.Any())
        {
            foreach (var pm in LinkedProvinces)
            {
                pm.UpdateColor();
            }
        }
    }

    public void ValidateConnections()
    {
        if (Node.ProvinceData.Terrain.IsFlagSet(Terrain.SEA))
        {
            foreach (var c in m_connections)
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

    public void UpdateSprite()
    {
        if (m_widget == null)
        {
            return;
        }

        m_widget.SetNode(m_node);
    }

    public void UpdateColor()
    {
        //Renderer.color = get_node_color(m_node);

        assign_mat(GenerationManager.s_generation_manager.Season);

        if (m_widget == null)
        {
            return;
        }

        m_widget.SetNode(m_node);
    }

    private Vector3 get_weighted_center(Vector3 p1, Vector3 p2, Node n1, Node n2)
    {
        var dir1 = (p1 - p2).normalized;
        var dir2 = (p2 - p1).normalized;
        var center = (p1 + p2) / 2;
        var dist = Vector3.Distance(center, p1);

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

    private Color get_node_color(Node n)
    {
        var t = n.ProvinceData.Terrain;
        var colors = m_colors;

        if (ArtManager.s_art_manager.IsUsingUnderworldTerrain)
        {
            t = n.ProvinceData.CaveTerrain;
            colors = m_colors;
        }

        foreach (var pair in m_colors)
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

    private void Update()
    {
        if (m_selected)
        {
            m_scale = 1.1f + 0.3f * Mathf.Sin(Time.time * 5.5f);

            Renderer.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
        }
    }

    private void OnMouseOver()
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
            foreach (var m in m_wraps)
            {
                if (m != null)
                {
                    m.Delete();
                }
            }

            m_wraps = new List<ProvinceWrapMarker>();
        }
    }

    private List<GameObject> create_connection_wraps(ProvinceMarker pm, Vector3 offset)
    {
        var objs = new List<GameObject>();

        if (IsDummy)
        {
            return objs;
        }

        foreach (var m in pm.Connections)
        {
            var obj = m.CreateWrapMesh(m.PolyBorder, offset);

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

        var objs = new List<GameObject>();

        if (LinkedProvinces == null || !LinkedProvinces.Any()) // case for standard provinces
        {
            if (ConnectedProvinces.Any(x => x.IsDummy))
            {
                var mars = ConnectedProvinces.Where(x => x.IsDummy);
                var offsets = new List<Vector3>();

                foreach (var pm in mars)
                {
                    if (offsets.Any(x => Vector3.Distance(x, pm.DummyOffset) < 0.01f))
                    {
                        continue;
                    }

                    offsets.Add(pm.DummyOffset);

                    var obj = GameObject.Instantiate(WrapMarkerPrefab);
                    var wrap = obj.GetComponent<ProvinceWrapMarker>();

                    wrap.UpdateArtStyle();
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
            var ignore = new List<ProvinceMarker>();

            foreach (var pm in LinkedProvinces)
            {
                var obj = GameObject.Instantiate(WrapMarkerPrefab);
                var wrap = obj.GetComponent<ProvinceWrapMarker>();

                wrap.UpdateArtStyle();
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
                var mars = ConnectedProvinces.Where(x => x.IsDummy && !ignore.Contains(x));

                foreach (var pm in mars)
                {
                    if (pm.DummyOffset.x > 0f && pm.DummyOffset.y > 0f && mars.Count > 1)
                    {
                        continue;
                    }

                    var obj = GameObject.Instantiate(WrapMarkerPrefab);
                    var wrap = obj.GetComponent<ProvinceWrapMarker>();

                    wrap.UpdateArtStyle();
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

        var borders = new List<PolyBorder>();

        var conns = new List<ConnectionMarker>();
        conns.AddRange(m_connections);

        if (m_linked != null)
        {
            foreach (var pm in m_linked)
            {
                conns.AddRange(pm.Connections);
            }
        }

        if (Node.X == 0 && Node.Y == 0) // corner case
        {
            foreach (var cm in Connections) // main connections
            {
                borders.Add(cm.PolyBorder);
            }

            foreach (var pm in LinkedProvinces) // grab all children connections
            {
                foreach (var cm in pm.Connections)
                {
                    borders.Add(cm.GetOffsetBorder(-cm.DummyOffset));
                }
            }
        }
        else // bot side or left side case
        {
            foreach (var cm in conns)
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
        var cur = borders[0].P2;
        PolyBorder b = null;
        var result = new List<PolyBorder>();

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

                var rev = b.Reversed();

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

        var avg = Vector3.zero;
        var simplified = new List<Vector3>();

        foreach (var pb in result)
        {
            var mid1 = Mathf.RoundToInt(pb.OrderedPoints.Count * 0.33f);
            var mid2 = Mathf.RoundToInt(pb.OrderedPoints.Count * 0.66f);

            m_poly.AddRange(pb.GetFullLengthBorderMinusEnd());
            avg += pb.P1;
            avg += pb.OrderedPoints[mid1];
            avg += pb.OrderedPoints[mid2];
            simplified.Add(pb.P1);
            simplified.Add(pb.OrderedPoints[mid1]);
            simplified.Add(pb.OrderedPoints[mid2]);
        }

        var closest = new Vector3(9000, 9000);
        avg /= (result.Count * 3);
        avg -= new Vector3(0f, 0.2f); // shift the center down slightly for aesthetics
        simplified = simplified.OrderBy(x => Mathf.Abs(avg.y - x.y)).ToList();

        foreach (var p in simplified) // calculate the distance to left
        {
            if (p.x < avg.x)
            {
                m_center_size = Vector3.Distance(p, avg);
                break;
            }
        }

        foreach (var p in simplified) // calculate the distance to right
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

        assign_mat(GenerationManager.s_generation_manager.Season);
    }

    public void ClearSprites()
    {
        if (m_wraps != null)
        {
            foreach (var m in m_wraps)
            {
                m.ClearSprites();
            }
        }

        SpriteGroup.Clear();
    }

    public List<SpriteMarker> PlaceSprites()
    {
        ClearSprites();

        if (m_wraps != null)
        {
            foreach (var m in m_wraps)
            {
                m.PlaceSprites();
            }
        }

        Terrain terrain = m_node.ProvinceData.Terrain;
        if (ArtManager.s_art_manager.IsUsingUnderworldTerrain)
        {
            terrain = m_node.ProvinceData.CaveTerrain;
        }

        var set = ArtManager.s_art_manager.GetMapSpriteSet(terrain);

        if (!set.MapSprites.Any() || (m_node.ProvinceData.IsCaveWall && ArtManager.s_art_manager.IsUsingUnderworldTerrain))
        {
            return new List<SpriteMarker>();
        }

        var shuffled = set.MapSprites.Where(x => x.IsCenterpiece && terrain.IsFlagSet(x.ValidTerrain));

        if (m_node.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV) && !m_node.ProvinceData.IsWater && !ArtManager.s_art_manager.IsUsingUnderworldTerrain)
        {
            shuffled = set.MapSprites.Where(x => x.IsCenterpiece && x.ValidTerrain == Terrain.LARGEPROV);
        }

        shuffled.Shuffle();
        var centers = shuffled.OrderBy(x => 9000f - x.Size).ToList();

        if (centers.Any() && !IsDummy) // certain provinces have a centerpiece so place that first
        {
            foreach (var ps in centers)
            {
                if (ps.Size < m_center_size)
                {
                    var pos = m_poly_center;

                    if (ps.CullSpritePositions) // cull nearby sprite positions
                    {
                        pos.z = -10; // sprite points are at z = -10 so we need to match that
                        m_sprite_points = m_sprite_points.Where(x => Vector3.Distance(x, pos) > ps.CullingRadius);
                    }
                    
                    pos.z = -3; // sprite placement is at z = -3
                    SpriteGroup.PlaceSprite(ps, pos);

                    break;
                }
            }
        }

        set.MapSprites.Shuffle();
        var sprites = new List<ProvinceSprite>();

        foreach (var ps in set.MapSprites) // guarantee that we have at least 1 of each valid sprite
        {
            if (!terrain.IsFlagSet(ps.ValidTerrain) || ps.IsCenterpiece)
            {
                continue;
            }
            sprites.Add(ps);
        }

        if (sprites.Count == 0)
        {
            return new List<SpriteMarker>();
        }

        var sprite_items = new List<NativeObjectPlacer.Item>();
        var sprite_size = new List<float>();

        foreach (var s in sprites)
        {
            sprite_items.Add(new NativeObjectPlacer.Item
            {
                size = s.Size,
                cull_radius = s.CullingRadius,
                spawn_chance = s.SpawnChance,
                place_at_least_1 = s.PlaceAtLeastOne == true ? 1 : 0,
                extra_border_dist = s.ValidTerrain == Terrain.LARGEPROV ? 1 : 0,
                cull_nearby_points = s.CullSpritePositions == true ? 1 : 0
            });
        }

        var already_placed = new List<Vector3>();
        already_placed.AddRange(SpriteGroup.SpritePos());
        var objs = NativeObjectPlacer.Invoke(sprite_items, m_sprite_points, m_poly, already_placed);

        for (var i = 0; i < objs.Count; ++i)
        {
            SpriteGroup.PlaceSprite(sprites[objs[i]], m_sprite_points[i]);
        }

        m_sprite_points.Clear();
        var all = new List<SpriteMarker>();

        foreach (var m in m_wraps)
        {
            all.AddRange(m.PlaceSprites());
        }

        SpriteGroup.Build(CaptureCam.Bounds());

        return all;
    }

    private Vector3 get_valid_position()
    {
        foreach (var v in m_sprite_points)
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
            foreach (var m in m_wraps)
            {
                m.CalculateSpritePoints();
                if (Util.ShouldYield()) yield return null;
            }
        }

        m_sprite_points = new List<Vector3>();
        var mins = get_mins();
        var maxs = get_maxs();
        var cur = new Vector3(mins.x, mins.y);
        var roads_rivers = m_connections.Where(x => x.Connection.ConnectionType == ConnectionType.ROAD || x.Connection.ConnectionType == ConnectionType.SHALLOWRIVER);

        Terrain terrain = m_node.ProvinceData.Terrain;
        if (ArtManager.s_art_manager.IsUsingUnderworldTerrain)
        {
            terrain = m_node.ProvinceData.CaveTerrain;
        }

        var set = ArtManager.s_art_manager.GetMapSpriteSet(terrain);

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
        for (var i = 0; i < m_sprite_points.Count; ++i) m_sprite_points[i] += new Vector3(0, 0, -10);

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

    private List<Vector3> GetCullPoints(List<ConnectionMarker> roads_rivers)
    {
        var pts = new List<Vector3>();
        pts.AddRange(m_poly);
        foreach (var m in roads_rivers)
        {
            pts.AddRange(m.CullingPoints);
        }
        return pts;
    }

    private bool validate_solid(Vector3 pt)
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

    public void SetSeason(Season s)
    {
        assign_mat(s);

        if (m_wraps != null)
        {
            foreach (var m in m_wraps)
            {
                m.SetSeason(s);
            }
        }
    }

    private void assign_mat(Season s = Season.SUMMER)
    {
        SpriteGroup.SetSeason(s);
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

        if (ArtManager.s_art_manager.IsUsingUnderworldTerrain)
        {
            if (m_node.ProvinceData.IsCaveWall)
            {
                Mesh.material = MatUnderworldWall;
            }
            else if (m_node.ProvinceData.CaveTerrain.IsFlagSet(Terrain.SEA))
            {
                Mesh.material = MatUnderworldSea;
            }
            else if (m_node.ProvinceData.CaveTerrain.IsFlagSet(Terrain.FOREST))
            {
                Mesh.material = MatUnderworldForest;
            }
            else if (m_node.ProvinceData.CaveTerrain.IsFlagSet(Terrain.SWAMP))
            {
                Mesh.material = MatUnderworldSwamp;
            }
            else if (m_node.ProvinceData.CaveTerrain.IsFlagSet(Terrain.HIGHLAND))
            {
                Mesh.material = MatUnderworldHighland;
            }
            else
            {
                Mesh.material = MatUnderworldNormal;
            }
        }

        Mesh.material.color = get_node_color(m_node);

        if (m_wraps != null)
        {
            foreach (var m in m_wraps)
            {
                m.UpdateMaterial();
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
}
