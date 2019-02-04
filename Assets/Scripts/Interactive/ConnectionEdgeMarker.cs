using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This class was supposed to handle edge connection polygons but my SSD died so i had to redo the logic.
/// Somewhere along the way this class got cut out and its logic moved to ConnectionMarker. Maybe it shouldn't have been cut out.
/// </summary>
[Obsolete]
public class ConnectionEdgeMarker: MonoBehaviour
{
    public Material MatSea;
    public Material MatDeepSea;

    public MeshRenderer Mesh;
    public MeshFilter MeshFilter;
    public GameObject MeshObj;

    List<Vector3> m_poly;
    PolyBorder m_path;

    public void CreatePoly(PolyBorder pb)
    {
        RecalculatePoly(pb);
        ConstructPoly();
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
        MeshObj.transform.localPosition = transform.position * -1f;

        assign_mat();
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

    void assign_mat()
    {
        if (m_path.Connection.ConnectionType == ConnectionType.RIVER)
        {
            //Mesh.material = MatSea;
            //Mesh.material.color = 
        }
        else // shallow river
        {

        }
    }

    public void RecalculatePoly(PolyBorder pb)
    {
        m_poly = new List<Vector3>();
        List<Vector3> vecs = pb.OrderedPoints;
        Vector3 last = Vector3.negativeInfinity;

        //float maxwidth = 0.05f;
        //float minwidth = 0.02f;
        float curwidth = 0.03f;

        foreach (Vector3 pt in vecs)
        {
            if (last == Vector3.negativeInfinity)
            {
                last = pt;
                continue;
            }

            Vector3 dir = (pt - last).normalized;
            Vector3 norm = Vector3.Cross(dir, Vector3.forward);

            m_poly.Add(pt + norm * curwidth);
        }

        last = Vector3.negativeInfinity;
        vecs.Reverse();

        foreach (Vector3 pt in vecs)
        {
            if (last == Vector3.negativeInfinity)
            {
                last = pt;
                continue;
            }

            Vector3 dir = (pt - last).normalized;
            Vector3 norm = Vector3.Cross(dir, Vector3.forward);

            m_poly.Add(pt + norm * curwidth);
        }
    }
}
