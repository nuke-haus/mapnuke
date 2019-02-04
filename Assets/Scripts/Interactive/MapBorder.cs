using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behaviour class for the pink map border.
/// Has a global singleton.
/// </summary>
public class MapBorder: MonoBehaviour
{
    public static MapBorder s_map_border;

    public Vector3 Mins
    {
        get;
        private set;
    }

    public Vector3 Maxs
    {
        get;
        private set;
    }

    private void Awake()
    {
        s_map_border = this;
    }

    public void SetBorders(Vector3 mins, Vector3 maxs)
    {
        Mins = mins;
        Maxs = maxs;

        List<Vector3> pts = new List<Vector3>();
        pts.Add(mins);
        pts.Add(new Vector3(mins.x, maxs.y));
        pts.Add(maxs);
        pts.Add(new Vector3(maxs.x, mins.y));
        //pts.Add(mins);

        LineRenderer rend = GetComponent<LineRenderer>();
        rend.SetPositions(pts.ToArray());
    }
}
