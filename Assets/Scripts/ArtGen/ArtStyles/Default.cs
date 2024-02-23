using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// !!!
/// The original plan was to support different art styles that handled all of their own generation logic.
/// This plan was way too ambitious and so this abstract class isn't needed (at least not in this state).
/// For now it's fine to leave it as is since this project has reached its conclusion more or less.
/// In a perfect world this class would be refactored out since art styles are handled by the ArtManager.
/// !!!

public abstract class ArtStyle
{
    public abstract string GetName();
    public abstract IEnumerator Generate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayoutData layout);
    public abstract void Regenerate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayoutData layout);
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
public class DefaultArtStyle : ArtStyle
{
    private List<ProvinceMarker> m_all_provs;
    private List<ConnectionMarker> m_all_conns;
    private List<SpriteMarker> m_all_sprites;

    public override string GetName()
    {
        return "Default Art Style";
    }

    public override void Regenerate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayoutData layout)
    {
        var result = new List<GameObject>();
        var linked = new List<ConnectionMarker>();

        foreach (var m in conns)
        {
            if (m.LinkedConnection != null)
            {
                linked.Add(m.LinkedConnection);
            }
        }

        conns.AddRange(linked);

        // Only recalculate triangles if we aren't generating dom6 map output
        if (!ArtManager.s_art_manager.IsLockingProvinceShapes)
        {
            foreach (var m in m_all_conns)
            {
                m.ClearTriangles();
            }

            calc_triangles(m_all_conns);
        }

        foreach (var cm in conns)
        {
            cm.CreatePolyBorder();
            cm.ClearWrapMeshes();
            cm.RecalculatePoly();
        }

        foreach (var pm in provs)
        {
            pm.UpdateLabel();
            pm.RecalculatePoly();
            pm.ConstructPoly();
            pm.ClearWrapMeshes();
            result.AddRange(pm.CreateWrapMeshes()); // also create connection wrap meshes
        }

        var bad = provs.Where(x => x.NeedsRegen).ToList();
        var add = new List<ProvinceMarker>();

        if (bad.Any())
        {
            Debug.LogError(bad.Count + " provinces have invalid PolyBorders. Regenerating additional provinces.");

            foreach (var b in bad)
            {
                foreach (var adj in b.ConnectedProvinces)
                {
                    if (provs.Contains(adj))
                    {
                        continue;
                    }

                    if (adj.IsDummy)
                    {
                        add.AddRange(adj.LinkedProvinces);

                        if (adj.LinkedProvinces[0].LinkedProvinces.Count > 1)
                        {
                            foreach (var link in adj.LinkedProvinces[0].LinkedProvinces) // 3 dummies
                            {
                                add.AddRange(adj.ConnectedProvinces);
                            }
                        }
                        else
                        {
                            add.AddRange(adj.LinkedProvinces[0].ConnectedProvinces);
                        }
                    }
                    else
                    {
                        add.Add(adj);

                        if (adj.LinkedProvinces != null && adj.LinkedProvinces.Any())
                        {
                            foreach (var link in adj.LinkedProvinces)
                            {
                                add.AddRange(link.ConnectedProvinces);
                            }
                        }
                    }

                    if (!add.Contains(adj))
                    {
                        add.Add(adj);
                    }
                }

                if (b.LinkedProvinces != null && b.LinkedProvinces.Any())
                {
                    foreach (var link in b.LinkedProvinces)
                    {
                        add.AddRange(link.ConnectedProvinces);
                    }
                }
            }

            foreach (var pm in add)
            {
                foreach (var m in pm.Connections)
                {
                    if (!conns.Contains(m) && ((provs.Contains(m.Prov1) || add.Contains(m.Prov1)) && (provs.Contains(m.Prov2) || add.Contains(m.Prov2))))
                    {
                        conns.Add(m);
                    }
                }
            }

            foreach (var cm in conns)
            {
                cm.CreatePolyBorder();
                cm.ClearWrapMeshes();
                cm.RecalculatePoly();
            }

            foreach (var pm in add)
            {
                pm.UpdateLabel();
                pm.RecalculatePoly();
                pm.ConstructPoly();
                pm.ClearWrapMeshes();
                result.AddRange(pm.CreateWrapMeshes()); // also create connection wrap meshes
            }

            foreach (var pm in provs)
            {
                pm.UpdateLabel();
                pm.RecalculatePoly();
                pm.ConstructPoly();
                pm.ClearWrapMeshes();
                result.AddRange(pm.CreateWrapMeshes()); // also create connection wrap meshes
            }
        }

        foreach (var cm in conns)
        {
            if (!ArtManager.s_art_manager.IsLockingProvinceShapes || ArtManager.s_art_manager.IsUsingUnderworldTerrain)
            {
                m_all_sprites.AddRange(cm.PlaceSprites());
            }
        }

        foreach (var pm in provs)
        {
            foreach (var unused in pm.CalculateSpritePoints()) { }
            m_all_sprites.AddRange(pm.PlaceSprites());
        }

        var all = new List<SpriteMarker>();

        foreach (var m in m_all_sprites)
        {
            if (m != null && m.gameObject != null)
            {
                all.Add(m);
                result.Add(m.gameObject);
            }
        }

        m_all_sprites = all;

        ElementManager.s_element_manager.AddGeneratedObjects(result);
        CaptureCam.s_capture_cam.Render();
    }

    public override IEnumerator Generate(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayoutData layout)
    {
        var result = new List<GameObject>();

        m_all_conns = conns;
        m_all_provs = provs;

        calc_triangles(conns);

        foreach (var cm in conns)
        {
            cm.CreatePolyBorder();
            cm.ClearWrapMeshes();
            cm.RecalculatePoly();
            if (Util.ShouldYield()) yield return null;
        }

        foreach (var pm in provs)
        {
            pm.UpdateLabel();
            pm.RecalculatePoly();
            pm.ConstructPoly();
            pm.ClearWrapMeshes();
            result.AddRange(pm.CreateWrapMeshes());
            if (Util.ShouldYield()) yield return null;
        }

        m_all_sprites = new List<SpriteMarker>();

        foreach (var cm in conns)
        {
            m_all_sprites.AddRange(cm.PlaceSprites());
            if (Util.ShouldYield()) yield return null;
        }

        foreach (var pm in provs)
        {
            foreach (var x in pm.CalculateSpritePoints())
            {
                yield return x;
            }
            m_all_sprites.AddRange(pm.PlaceSprites());
            if (Util.ShouldYield()) yield return null;
        }

        foreach (var m in m_all_sprites)
        {
            result.Add(m.gameObject);
            if (Util.ShouldYield()) yield return null;
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

        foreach (var m in m_all_sprites)
        {
            if (m != null)
            {
                m.SetSeason(s);
            }
        }

        foreach (var m in m_all_provs)
        {
            m.SetSeason(s);
        }

        foreach (var m in m_all_conns)
        {
            m.SetSeason(s);
        }

        JustChangedSeason = true;
        CaptureCam.s_capture_cam.Render();
    }

    /// <summary>
    /// Every diagonal connection forms a triangle with its adjacent connections, we want to compute the center of these triangles.
    /// Some trickery has to be done to account for the wrapping connections.
    /// </summary>
    private void calc_triangles(List<ConnectionMarker> conns)
    {
        foreach (var c in conns)
        {
            var adj = get_adjacent(conns, c);

            if (adj.Count == 4)
            {
                if (c.IsEdge)
                {
                    if (c.Dummy.Node.X == 0 && c.Dummy.Node.Y == 0)
                    {
                        var upper = adj.FirstOrDefault(x => x.Connection.Pos.y == 0f);
                        var right = adj.FirstOrDefault(x => x.Connection.Pos.x == 0f);
                        var lower = adj.FirstOrDefault(x => x != right && x != upper && x.Connection.Pos.x == c.Connection.Pos.x);
                        var left = adj.FirstOrDefault(x => x != right && x != upper && x != lower);

                        var upperpos = upper.transform.position;
                        var rightpos = right.transform.position;
                        var upperoffset = Vector3.zero;
                        var rightoffset = Vector3.zero;
                        var is_upper = false;
                        var is_right = false;

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
                            var mid1 = (c.transform.position + lower.transform.position + left.transform.position) / 3;
                            var mid2 = (c.transform.position + upperpos + rightpos) / 3;

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
                            var mid1 = (c.transform.position + lower.transform.position + rightpos) / 3;
                            var mid2 = (c.transform.position + upperpos + left.transform.position) / 3;

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
                        var upper = adj.FirstOrDefault(x => x.Connection.Pos.y == c.Connection.Pos.y + 0.5f || c.Connection.Pos.y == 0f);
                        var lower = adj.FirstOrDefault(x => x.Connection.Pos.y != c.Connection.Pos.y && x != upper);
                        var right = adj.FirstOrDefault(x => x.Connection.Pos.x == c.Connection.Pos.x + 0.5f || c.Connection.Pos.x == 0f);
                        var left = adj.FirstOrDefault(x => x.Connection.Pos.x != c.Connection.Pos.x && x != right);

                        var upperpos = upper.transform.position;
                        var rightpos = right.transform.position;
                        var upperoffset = Vector3.zero;
                        var rightoffset = Vector3.zero;
                        var is_upper = false;
                        var is_right = false;

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
                            var mid1 = (c.transform.position + lower.transform.position + left.transform.position) / 3;
                            var mid2 = (c.transform.position + upperpos + rightpos) / 3;

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
                            var mid1 = (c.transform.position + lower.transform.position + rightpos) / 3;
                            var mid2 = (c.transform.position + upperpos + left.transform.position) / 3;

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
                    var anchor = adj[0];
                    ConnectionMarker other = null;
                    adj.Remove(anchor);

                    foreach (var c2 in adj)
                    {
                        if (unique_nodes(c, c2, anchor) == 3)
                        {
                            other = c2;

                            var mid = (c.transform.position + c2.transform.position + anchor.transform.position) / 3;
                            c.AddTriangleCenter(mid);
                            c2.AddTriangleCenter(mid);
                            anchor.AddTriangleCenter(mid);
                            break;
                        }
                    }

                    adj.Remove(other);

                    anchor = adj[0];
                    var c3 = adj[1];

                    var mid2 = (c.transform.position + c3.transform.position + anchor.transform.position) / 3;
                    c.AddTriangleCenter(mid2);
                    c3.AddTriangleCenter(mid2);
                    anchor.AddTriangleCenter(mid2);
                }
            }
        }
    }

    private List<ConnectionMarker> get_adjacent(List<ConnectionMarker> conns, ConnectionMarker c)
    {
        return conns.Where(x => c.Connection.Adjacent.Contains(x.Connection)).ToList();
    }

    private int unique_nodes(ConnectionMarker m1, ConnectionMarker m2, ConnectionMarker m3)
    {
        var temp = new List<Node>();

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
