using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InnerStroke: MonoBehaviour
{
    public Material RiverShore;
    public Material SeaShore;

    public Material RiverShoreWinter;
    public Material SeaShoreWinter;

    bool m_is_sea = false;

    /// <summary>
    /// Method used for water provinces
    /// </summary>
    public void DrawStroke(List<Vector3> pts, Vector3 offset, bool is_sea = true, float min = 0.1f, float max = 0.2f)
    {
        List<Vector3> offset_pts = new List<Vector3>();
        LineRenderer rend = GetComponent<LineRenderer>();
       
        foreach (Vector3 p in pts)
        {
            offset_pts.Add(p + offset);
        }

        rend.positionCount = pts.Count;
        rend.SetPositions(offset_pts.ToArray());

        AnimationCurve jitter = new AnimationCurve();
        int num_keys = UnityEngine.Random.Range(2, 6); 
        List<float> floats = new List<float>();

        for (int i = 0; i < num_keys; i++)
        {
            float rand = UnityEngine.Random.Range(0f, 1f);
            int ct = 0;

            while (floats.Any(x => Mathf.Abs(rand - x) < 0.1f) && ct < 10)
            {
                rand = UnityEngine.Random.Range(0f, 1f);
                ct++;
            }

            if (ct < 10)
            {
                floats.Add(rand);

                jitter.AddKey(rand, UnityEngine.Random.Range(min, max));
            }
        }

        rend.widthCurve = jitter;
        m_is_sea = is_sea;
    }

    public void SetSeason(Season s)
    {
        LineRenderer rend = GetComponent<LineRenderer>();

        if (s == Season.SUMMER)
        {
            if (m_is_sea)
            {
                rend.material = SeaShore;
            }
            else
            {
                rend.material = RiverShore;
            }
        }
        else
        {
            if (m_is_sea)
            {
                rend.material = SeaShoreWinter;
            }
            else
            {
                rend.material = RiverShoreWinter;
            }
        }
    }
}
