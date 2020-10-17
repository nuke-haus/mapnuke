using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behaviour class for the pink map border.
/// </summary>
public class MapBorder : MonoBehaviour
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

        mins.z = -3.9f;
        maxs.z = -3.9f;

        var pts = new List<Vector3>();
        pts.Add(mins);
        pts.Add(new Vector3(mins.x, maxs.y, maxs.z));
        pts.Add(maxs);
        pts.Add(new Vector3(maxs.x, mins.y, mins.z));

        var rend = GetComponent<LineRenderer>();
        rend.SetPositions(pts.ToArray());
    }
}
