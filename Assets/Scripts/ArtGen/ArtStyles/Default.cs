using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ArtStyle
{
    public abstract string GetName();
    public abstract List<GameObject> Generate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout);
    public abstract void Regenerate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout);
    public abstract void ChangeSeason(Season s);
}

/// <summary>
/// This is the default art style, you can derive from ArtStyle to make your own art logic.
/// </summary>
public class DefaultArtStyle: ArtStyle
{
    List<PolyBorder> m_entries;
    List<ProvinceMarker> m_all_provs;
    List<ConnectionMarker> m_all_conns;
    List<SpriteMarker> m_all_sprites;

    public override string GetName()
    {
        return "Default Art Style";
    }

    public override void Regenerate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout)
    {
        List<ConnectionMarker> linked = new List<ConnectionMarker>();

        foreach (ConnectionMarker m in conns)
        {
            if (m.LinkedConnection != null)
            {
                linked.Add(m.LinkedConnection);
            }
        }

        conns.AddRange(linked);

        List<PolyBorder> erase = new List<PolyBorder>();

        foreach (PolyBorder b in m_entries)
        {
            if (conns.Any(x => !x.IsEdge && x.Connection == b.Connection))
            {
                erase.Add(b);
            }
        }

        foreach (PolyBorder p in erase)
        {
            m_entries.Remove(p);
        }

        foreach (ConnectionMarker m in m_all_conns)
        {
            m.ClearTriangles();
        }

        foreach (ProvinceMarker pm in provs)
        {
            pm.UpdateConnections();
        }

        calc_triangles(m_all_conns, layout);

        foreach (ProvinceMarker pm in provs)
        {
            pm.UpdateLabel();
            pm.RecalculatePoly();
            pm.RandomizePoly(this);
            pm.ConstructPoly();
        }

        m_all_sprites = new List<SpriteMarker>();

        foreach (ConnectionMarker cm in conns)
        {
            cm.CreatePolygon(this);
            m_all_sprites.AddRange(cm.PlaceSprites(this));
        }

        foreach (ProvinceMarker pm in provs)
        {
            pm.CalculateSpritePoints();
            m_all_sprites.AddRange(pm.PlaceSprites());
        }

        sort_sprites();
    }

    public override List<GameObject> Generate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout)
    {
        List<GameObject> result = new List<GameObject>();

        m_entries = new List<PolyBorder>();
        m_all_conns = conns;
        m_all_provs = provs;

        calc_triangles(conns, layout); // basic triangles

        foreach (ProvinceMarker pm in provs)
        {
            pm.UpdateLabel();
            pm.RecalculatePoly();
            pm.RandomizePoly(this);
            pm.ConstructPoly();
        }

        foreach (ProvinceMarker pm in provs) // now figure out edge cases and create wrap meshes
        {
            if (pm.Node.X == 0 && pm.Node.Y == 0) // bottom left
            {
                List<ConnectionMarker> left = conns.Where(x => x.IsDummy && x.Connection.Pos.x == -0.5f && x.Connection.Pos.y > -0.5f && (x.Prov1 == pm || x.Prov2 == pm)).ToList();
                List<ConnectionMarker> bottom = conns.Where(x => x.IsDummy && x.Connection.Pos.x > -0.5f && x.Connection.Pos.y == -0.5f && (x.Prov1 == pm || x.Prov2 == pm)).ToList();
                List<ConnectionMarker> bottomleft = conns.Where(x => x.IsDummy && x.Connection.Pos.x == -0.5f && x.Connection.Pos.y == -0.5f && (x.Prov1 == pm || x.Prov2 == pm)).ToList();

                ConnectionMarker left_ext = conns.FirstOrDefault(x => Mathf.Abs(x.Connection.Pos.x - (layout.X - 1f)) < 0.01f && Mathf.Abs(x.Connection.Pos.y + 0.5f) < 0.01f);
                ConnectionMarker bottom_ext = conns.FirstOrDefault(x => Mathf.Abs(x.Connection.Pos.y - (layout.Y - 1f)) < 0.01f && Mathf.Abs(x.Connection.Pos.x + 0.5f) < 0.01f);

                GameObject obj = pm.CreateWrapMesh(left, left_ext, this);
                result.Add(obj);

                GameObject obj2 = pm.CreateWrapMesh(bottom, bottom_ext, this);
                result.Add(obj2);

                GameObject obj3 = pm.CreateWrapMesh(bottomleft, this, true);
                result.Add(obj3);
            }
            else if (pm.Node.X == 0 && pm.Node.Y == layout.Y - 1) // top left
            {
                List<ConnectionMarker> valid = conns.Where(x => x.Connection.Pos.x == -0.5f && x.IsDummy && (x.Prov1 == pm || x.Prov2 == pm)).ToList();

                GameObject obj = pm.CreateWrapMesh(valid, this);
                result.Add(obj);
            }
            else if (pm.Node.Y == 0 && pm.Node.X == layout.X - 1) // bottom right
            {
                List<ConnectionMarker> valid = conns.Where(x => x.Connection.Pos.y == -0.5f && x.IsDummy && (x.Prov1 == pm || x.Prov2 == pm)).ToList();

                GameObject obj = pm.CreateWrapMesh(valid, this);
                result.Add(obj);
            }
            else if (pm.Node.X == 0) // left
            {
                List<ConnectionMarker> valid = conns.Where(x => x.IsDummy && (x.Prov1 == pm || x.Prov2 == pm)).ToList();

                GameObject obj = pm.CreateWrapMesh(valid, this);
                result.Add(obj);
            }
            else if (pm.Node.Y == 0) // bottom
            {
                List<ConnectionMarker> valid = conns.Where(x => x.IsDummy && (x.Prov1 == pm || x.Prov2 == pm)).ToList();

                GameObject obj = pm.CreateWrapMesh(valid, this);
                result.Add(obj);
            }
        }

        m_all_sprites = new List<SpriteMarker>();

        foreach (ConnectionMarker cm in conns)
        {
            cm.CreatePolygon(this);
            m_all_sprites.AddRange(cm.PlaceSprites(this));
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

        return result;
    }

    public override void ChangeSeason(Season s)
    {
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

    void calc_triangles(List<ConnectionMarker> conns, NodeLayout layout)
    {
        foreach (ConnectionMarker c in conns)
        {
            List<ConnectionMarker> adj = get_adjacent(conns, c);

            if (c.IsEdge || c.IsDummy)
            {
                if (adj.Count == 1) // top left, bottom right case
                {
                    Vector3 p1 = (adj[0].EdgePoint + c.EdgePoint) / 2;

                    c.AddTriangleCenter(p1);
                    //c.AddTriangleCenter(c.EdgePoint); // old method

                    Vector3 mins = MapBorder.s_map_border.Mins;
                    Vector3 maxs = MapBorder.s_map_border.Maxs;

                    if (c.Connection.Pos.x == 0f) // top left
                    {
                        Vector3 pos = new Vector3(mins.x, maxs.y, 0f);
                        c.AddTriangleCenter(pos);
                    }
                    else // bottom right
                    {
                        Vector3 pos = new Vector3(maxs.x, mins.y, 0f);
                        c.AddTriangleCenter(pos);
                    }
                }
                else if (adj.Count == 2) // edge with 2 adjacent
                {
                    Vector3 p1 = (adj[0].EdgePoint + c.EdgePoint) / 2;
                    Vector3 p2 = (adj[1].EdgePoint + c.EdgePoint) / 2;

                    c.AddTriangleCenter(p1);
                    c.AddTriangleCenter(p2);

                    if (c.Prov1.Node.IsWrapCorner && c.Prov2.Node.IsWrapCorner) // special case: give the corner connection 3 triangle points
                    {
                        c.AddTriangleCenter(c.EdgePoint);
                    }
                }
                else if (adj.Count == 3) // edge with 3 adjacent
                {
                    ConnectionMarker non_edge = adj.FirstOrDefault(x => !x.IsEdge);

                    if (non_edge == null)
                    {
                        foreach (ConnectionMarker cm in adj)
                        {
                            if (cm.IsEdge && cm.Connection.DistanceTo(c.Connection) < 2.0f)
                            {
                                Vector3 pt = (cm.EdgePoint + c.EdgePoint) / 2;

                                c.AddTriangleCenter(pt);
                            }
                        }

                        /*if (c.Prov1.Node.IsWrapCorner && c.Prov2.Node.IsWrapCorner) // special case: give the corner connection 3 triangle points
                        {
                            c.AddTriangleCenter(c.EdgePoint);
                        }*/
                    }
                    else
                    {
                        ProvinceMarker pm = c.Prov1;

                        if (Vector3.Distance(pm.transform.position, c.EdgePoint) > Vector3.Distance(c.Prov2.transform.position, c.EdgePoint))
                        {
                            pm = c.Prov2;
                        }

                        List<ConnectionMarker> l1 = adj.Where(x => x != non_edge && (x.Prov1 == c.Prov1 || x.Prov2 == c.Prov1)).ToList();
                        List<ConnectionMarker> l2 = adj.Where(x => x != non_edge && (x.Prov1 == c.Prov2 || x.Prov2 == c.Prov2)).ToList();

                        foreach (ConnectionMarker cm in adj)
                        {
                            if (cm.IsEdge && cm.Connection.DistanceTo(c.Connection) < 2.0f)
                            {
                                Vector3 pt = (cm.EdgePoint + c.EdgePoint) / 2;

                                c.AddTriangleCenter(pt);

                                if (cm.Prov1 != pm && cm.Prov2 != pm)
                                {
                                    non_edge.AddTriangleCenter(pt);
                                }
                            }
                        }
                    }
                }
            }
            else if (adj.Count == 2)
            {
                /*ConnectionMarker a1 = adj[0];
                ConnectionMarker a2 = adj[1];

                if (c.TriCenters.Count < 2)
                {
                    if (a1.IsEdge)
                    {
                        c.AddTriangleCenter(a1.EdgePoint);
                    }
                    if (a2.IsEdge)
                    {
                        c.AddTriangleCenter(a2.EdgePoint);
                    }
                }*/
            }
            else if (adj.Count == 4)
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

    public PolyBorder AddPolyEdge(Vector3 p1, Vector3 p2, Connection c)
    {
        PolyBorder ex1 = m_entries.FirstOrDefault(x => Vector3.Distance(x.P1, p1) < 0.1f && Vector3.Distance(x.P2, p2) < 0.1f);
        PolyBorder ex2 = m_entries.FirstOrDefault(x => Vector3.Distance(x.P2, p1) < 0.1f && Vector3.Distance(x.P1, p2) < 0.1f);

        if (ex1 != null)
        {
            return ex1;
        }
        else if (ex2 != null)
        {
            return ex2.Reversed();
        }

        PolyBorder e = new PolyBorder(p1, p2, c);
        m_entries.Add(e);

        return e;
    }

    public PolyBorder AddPolyEdge(Vector3 p1, Vector3 p2, Vector3 dir, Connection c)
    {
        PolyBorder ex1 = m_entries.FirstOrDefault(x => Vector3.Distance(x.P1, p1) < 0.1f && Vector3.Distance(x.P2, p2) < 0.1f);
        PolyBorder ex2 = m_entries.FirstOrDefault(x => Vector3.Distance(x.P2, p1) < 0.1f && Vector3.Distance(x.P1, p2) < 0.1f);

        if (ex1 != null)
        {
            return ex1;
        }
        else if (ex2 != null)
        {
            return ex2.Reversed();
        }

        PolyBorder e = new PolyBorder(p1, p2, dir, c);
        m_entries.Add(e);

        return e;
    }

    public PolyBorder GetPolyBorder(Vector3 p1, Vector3 p2)
    {
        return m_entries.FirstOrDefault(x => (Vector3.Distance(x.P1, p1) < 0.01f && Vector3.Distance(x.P2, p2) < 0.01f) || (Vector3.Distance(x.P1, p2) < 0.01f && Vector3.Distance(x.P2, p1) < 0.01f));
    }

    public PolyBorder GetPolyBorder(Connection c)
    {
        return m_entries.FirstOrDefault(x => x.Connection == c);
    }

    List<ConnectionMarker> get_adjacent(List<ConnectionMarker> conns, ConnectionMarker c)
    {
        return conns.Where(x => c.Connection.Adjacent.Contains(x.Connection) && Vector3.Distance(c.transform.position, x.transform.position) < 4.0f).ToList();

        /*if (c.IsDummy)
        {
            return conns.Where(x => c.Connection.Adjacent.Contains(x.Connection) && Vector3.Distance(c.transform.position, x.transform.position) < 4.0f).ToList();
        }
        else
        {
            return conns.Where(x => c.Connection.Adjacent.Contains(x.Connection) && ((c.IsDummy && x.IsDummy) || (!c.IsDummy && !x.IsDummy))).ToList(); //&& Vector3.Distance(x.transform.position, c.transform.position) < 4.0f
        }*/
    }

    int unique_nodes(ConnectionMarker m1, ConnectionMarker m2, ConnectionMarker m3)
    {
        List<ProvinceMarker> temp = new List<ProvinceMarker>();

        if (!temp.Contains(m1.Prov1))
        {
            temp.Add(m1.Prov1);
        }
        if (!temp.Contains(m1.Prov2))
        {
            temp.Add(m1.Prov2);
        }
        if (!temp.Contains(m2.Prov1))
        {
            temp.Add(m2.Prov1);
        }
        if (!temp.Contains(m2.Prov2))
        {
            temp.Add(m2.Prov2);
        }
        if (!temp.Contains(m3.Prov1))
        {
            temp.Add(m3.Prov1);
        }
        if (!temp.Contains(m3.Prov2))
        {
            temp.Add(m3.Prov2);
        }

        return temp.Count;
    }
}
