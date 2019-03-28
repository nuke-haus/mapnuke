using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ArtStyle
{
    public abstract string GetName();
    public abstract void Generate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout);
    public abstract void Regenerate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout);
    public abstract void ChangeSeason(Season s);

    public bool JustChangedSeason
    {
        get;
        protected set;
    }
}

/// <summary>
/// This is the default art style, you can derive from ArtStyle to make your own art logic.
/// </summary>
public class DefaultArtStyle: ArtStyle
{
    List<ProvinceMarker> m_all_provs;
    List<ConnectionMarker> m_all_conns;
    List<SpriteMarker> m_all_sprites;

    public override string GetName()
    {
        return "Default Art Style";
    }

    public override void Regenerate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout)
    {
        List<GameObject> result = new List<GameObject>();
        List<ConnectionMarker> linked = new List<ConnectionMarker>();

        foreach (ConnectionMarker m in conns)
        {
            if (m.LinkedConnection != null)
            {
                linked.Add(m.LinkedConnection);
            }
        }

        conns.AddRange(linked);

        foreach (ConnectionMarker m in m_all_conns)
        {
            m.ClearTriangles();
        }

        foreach (ProvinceMarker pm in provs)
        {
            pm.UpdateConnections();
        }

        calc_triangles(m_all_conns, layout);

        foreach (ConnectionMarker cm in conns)
        {
            cm.CreatePolyBorder();
            cm.ClearWrapMeshes();
            cm.RecalculatePoly();
        }

        foreach (ProvinceMarker pm in provs)
        {
            pm.UpdateLabel();
            pm.RecalculatePoly();
            pm.ConstructPoly();
            pm.ClearWrapMeshes();
            result.AddRange(pm.CreateWrapMeshes()); // also create connection wrap meshes
        }

        foreach (ConnectionMarker cm in conns)
        {
            m_all_sprites.AddRange(cm.PlaceSprites());
        }

        foreach (ProvinceMarker pm in provs)
        {
            pm.CalculateSpritePoints();
            m_all_sprites.AddRange(pm.PlaceSprites());
        }

        List<SpriteMarker> all = new List<SpriteMarker>();

        foreach (SpriteMarker m in m_all_sprites)
        {
            if (m != null && m.gameObject != null)
            {
                all.Add(m);
                result.Add(m.gameObject);
            }
        }

        m_all_sprites = all;

        sort_sprites();

        ElementManager.s_element_manager.AddGeneratedObjects(result);
        CaptureCam.s_capture_cam.Render();
    }

    public override void Generate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout)
    {
        List<GameObject> result = new List<GameObject>();

        m_all_conns = conns;
        m_all_provs = provs;

        calc_triangles(conns, layout);

        foreach (ConnectionMarker cm in conns)
        {
            cm.CreatePolyBorder();
            cm.ClearWrapMeshes();
            cm.RecalculatePoly();
        }

        foreach (ProvinceMarker pm in provs)
        {
            pm.UpdateLabel();
            pm.RecalculatePoly();
            pm.ConstructPoly();
            pm.ClearWrapMeshes();
            result.AddRange(pm.CreateWrapMeshes());
        }

        m_all_sprites = new List<SpriteMarker>();

        foreach (ConnectionMarker cm in conns)
        {
            m_all_sprites.AddRange(cm.PlaceSprites());
        }

        foreach (ProvinceMarker pm in provs)
        {
            pm.CalculateSpritePoints();
            m_all_sprites.AddRange(pm.PlaceSprites());
        }

        sort_sprites();

        foreach (SpriteMarker m in m_all_sprites)
        {
            result.Add(m.gameObject);
        }

        ElementManager.s_element_manager.AddGeneratedObjects(result);
        CaptureCam.s_capture_cam.Render();
    }

    public override void ChangeSeason(Season s)
    {
        if (m_all_sprites == null)
        {
            return;
        }

        JustChangedSeason = false;

        foreach (SpriteMarker m in m_all_sprites)
        {
            if (m != null)
            {
                m.SetSeason(s);
            }
        }

        foreach (ProvinceMarker m in m_all_provs)
        {
            m.SetSeason(s);
        }

        foreach (ConnectionMarker m in m_all_conns)
        {
            m.SetSeason(s);
        }

        JustChangedSeason = true;
        CaptureCam.s_capture_cam.Render();
    }

    void sort_sprites()
    {
        List<SpriteMarker> fix = new List<SpriteMarker>();

        foreach (SpriteMarker m in m_all_sprites)
        {
            if (m != null)
            {
                fix.Add(m);
            }
        }

        m_all_sprites = fix;

        int max_y = Mathf.RoundToInt((MapBorder.s_map_border.Maxs.y + 1.0f) * 100f);

        foreach (SpriteMarker m in m_all_sprites) //.OrderBy(x => 9000f - x.transform.position.y))
        {
            if (m == null)
            {
                continue;
            }

            Vector3 pos = m.transform.position;
            int y = max_y - Mathf.RoundToInt(pos.y * 100f);

            m.SetOrder(y);
        }
    }

    /// <summary>
    /// Every diagonal connection forms a triangle with its adjacent connections, we want to compute the center of these triangles.
    /// Some trickery has to be done to account for the wrapping connections.
    /// </summary>
    void calc_triangles(List<ConnectionMarker> conns, NodeLayout layout)
    {
        foreach (ConnectionMarker c in conns)
        {
            List<ConnectionMarker> adj = get_adjacent(conns, c);

            if (adj.Count == 4)
            {
                if (c.IsEdge)
                {
                    if (c.Dummy.Node.X == 0 && c.Dummy.Node.Y == 0)
                    {
                        ConnectionMarker upper = adj.FirstOrDefault(x => x.Connection.Pos.y == 0f);
                        ConnectionMarker right = adj.FirstOrDefault(x => x.Connection.Pos.x == 0f);
                        ConnectionMarker lower = adj.FirstOrDefault(x => x != right && x != upper && x.Connection.Pos.x == c.Connection.Pos.x);
                        ConnectionMarker left = adj.FirstOrDefault(x => x != right && x != upper && x != lower);

                        Vector3 upperpos = upper.transform.position;
                        Vector3 rightpos = right.transform.position;
                        Vector3 upperoffset = Vector3.zero;
                        Vector3 rightoffset = Vector3.zero;
                        bool is_upper = false;
                        bool is_right = false;

                        if (Vector3.Distance(upperpos, c.transform.position) > 4f)
                        {
                            upperoffset = new Vector3(0f, c.DummyOffset.y);
                            upperpos += upperoffset;
                            is_upper = true;
                        }

                        if (Vector3.Distance(rightpos, c.transform.position) > 4f)
                        {
                            rightoffset = new Vector3(c.DummyOffset.x, 0f);
                            rightpos += rightoffset;
                            is_right = true;
                        }

                        if (unique_nodes(c, lower, left) == 3)
                        {
                            Vector3 mid1 = (c.transform.position + lower.transform.position + left.transform.position) / 3;
                            Vector3 mid2 = (c.transform.position + upperpos + rightpos) / 3;

                            c.AddTriangleCenter(mid1);
                            lower.AddTriangleCenter(mid1);
                            left.AddTriangleCenter(mid1);

                            c.AddTriangleCenter(mid2);

                            if (is_upper)
                            {
                                upper.AddTriangleCenter(mid2 - upperoffset);
                            }
                            else
                            {
                                upper.AddTriangleCenter(mid2);
                            }

                            if (is_right)
                            {
                                right.AddTriangleCenter(mid2 - rightoffset);
                            }
                            else
                            {
                                right.AddTriangleCenter(mid2);
                            }
                        }
                        else
                        {
                            Vector3 mid1 = (c.transform.position + lower.transform.position + rightpos) / 3;
                            Vector3 mid2 = (c.transform.position + upperpos + left.transform.position) / 3;

                            c.AddTriangleCenter(mid1);
                            lower.AddTriangleCenter(mid1);

                            if (is_right)
                            {
                                right.AddTriangleCenter(mid1 - rightoffset);
                            }
                            else
                            {
                                right.AddTriangleCenter(mid1);
                            }

                            c.AddTriangleCenter(mid2);
                            left.AddTriangleCenter(mid2);

                            if (is_upper)
                            {
                                upper.AddTriangleCenter(mid2 - upperoffset);
                            }
                            else
                            {
                                upper.AddTriangleCenter(mid2);
                            }
                        }
                    }
                    else
                    {
                        ConnectionMarker upper = adj.FirstOrDefault(x => x.Connection.Pos.y == c.Connection.Pos.y + 0.5f || c.Connection.Pos.y == 0f);
                        ConnectionMarker lower = adj.FirstOrDefault(x => x.Connection.Pos.y != c.Connection.Pos.y && x != upper);
                        ConnectionMarker right = adj.FirstOrDefault(x => x.Connection.Pos.x == c.Connection.Pos.x + 0.5f || c.Connection.Pos.x == 0f);
                        ConnectionMarker left = adj.FirstOrDefault(x => x.Connection.Pos.x != c.Connection.Pos.x && x != right);

                        Vector3 upperpos = upper.transform.position;
                        Vector3 rightpos = right.transform.position;
                        Vector3 upperoffset = Vector3.zero;
                        Vector3 rightoffset = Vector3.zero;
                        bool is_upper = false;
                        bool is_right = false;

                        if (Vector3.Distance(upperpos, c.transform.position) > 4f)
                        {
                            upperoffset = c.DummyOffset;
                            upperpos += upperoffset;
                            is_upper = true;
                        }

                        if (Vector3.Distance(rightpos, c.transform.position) > 4f)
                        {
                            rightoffset = c.DummyOffset;
                            rightpos += rightoffset;
                            is_right = true;
                        }

                        if (unique_nodes(c, lower, left) == 3)
                        {
                            Vector3 mid1 = (c.transform.position + lower.transform.position + left.transform.position) / 3;
                            Vector3 mid2 = (c.transform.position + upperpos + rightpos) / 3;

                            c.AddTriangleCenter(mid1);
                            lower.AddTriangleCenter(mid1);
                            left.AddTriangleCenter(mid1);

                            c.AddTriangleCenter(mid2);

                            if (is_upper)
                            {
                                upper.AddTriangleCenter(mid2 - upperoffset);
                            }
                            else
                            {
                                upper.AddTriangleCenter(mid2);
                            }

                            if (is_right)
                            {
                                right.AddTriangleCenter(mid2 - rightoffset);
                            }
                            else
                            {
                                right.AddTriangleCenter(mid2);
                            }
                        }
                        else
                        {
                            Vector3 mid1 = (c.transform.position + lower.transform.position + rightpos) / 3;
                            Vector3 mid2 = (c.transform.position + upperpos + left.transform.position) / 3;

                            c.AddTriangleCenter(mid1);
                            lower.AddTriangleCenter(mid1);

                            if (is_right)
                            {
                                right.AddTriangleCenter(mid1 - rightoffset);
                            }
                            else
                            {
                                right.AddTriangleCenter(mid1);
                            }

                            c.AddTriangleCenter(mid2);
                            left.AddTriangleCenter(mid2);

                            if (is_upper)
                            {
                                upper.AddTriangleCenter(mid2 - upperoffset);
                            }
                            else
                            {
                                upper.AddTriangleCenter(mid2);
                            }
                        }
                    }
                }
                else
                {
                    ConnectionMarker anchor = adj[0];
                    ConnectionMarker other = null;
                    adj.Remove(anchor);

                    foreach (ConnectionMarker c2 in adj)
                    {
                        if (unique_nodes(c, c2, anchor) == 3)
                        {
                            other = c2;

                            Vector3 mid = (c.transform.position + c2.transform.position + anchor.transform.position) / 3;
                            c.AddTriangleCenter(mid);
                            c2.AddTriangleCenter(mid);
                            anchor.AddTriangleCenter(mid);
                            break;
                        }
                    }

                    adj.Remove(other);

                    anchor = adj[0];
                    ConnectionMarker c3 = adj[1];

                    Vector3 mid2 = (c.transform.position + c3.transform.position + anchor.transform.position) / 3;
                    c.AddTriangleCenter(mid2);
                    c3.AddTriangleCenter(mid2);
                    anchor.AddTriangleCenter(mid2);
                }
            }
        }
    }

    List<ConnectionMarker> get_adjacent(List<ConnectionMarker> conns, ConnectionMarker c)
    {
        return conns.Where(x => c.Connection.Adjacent.Contains(x.Connection)).ToList();// && Vector3.Distance(c.transform.position, x.transform.position) < 4.0f).ToList();
    }

    int unique_nodes(ConnectionMarker m1, ConnectionMarker m2, ConnectionMarker m3)
    {
        List<Node> temp = new List<Node>();

        if (!temp.Contains(m1.Prov1.Node))
        {
            temp.Add(m1.Prov1.Node);
        }
        if (!temp.Contains(m1.Prov2.Node))
        {
            temp.Add(m1.Prov2.Node);
        }
        if (!temp.Contains(m2.Prov1.Node))
        {
            temp.Add(m2.Prov1.Node);
        }
        if (!temp.Contains(m2.Prov2.Node))
        {
            temp.Add(m2.Prov2.Node);
        }
        if (!temp.Contains(m3.Prov1.Node))
        {
            temp.Add(m3.Prov1.Node);
        }
        if (!temp.Contains(m3.Prov2.Node))
        {
            temp.Add(m3.Prov2.Node);
        }

        return temp.Count;
    }
}
