﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InnerStroke : MonoBehaviour
{
    public Material RiverShore;
    public Material SeaShore;
    public Material RiverShoreWinter;
    public Material SeaShoreWinter;
    public Material SeaShoreCave;
    public Material RiverShoreCave;

    private bool m_is_sea = false;

    public void UpdateArtStyle()
    {
        var art_config = ArtManager.s_art_manager.CurrentArtConfiguration;

        RiverShore = art_config.MatRiverShore;
        SeaShore = art_config.MatShore;
        RiverShoreWinter = art_config.MatWinterRiverShore;
        SeaShoreWinter = art_config.MatWinterShore;
        SeaShoreCave = art_config.MatUnderworldShore;
        RiverShoreCave = art_config.MatUnderworldRiverShore;
    }

    /// <summary>
    /// Method used for drawing shoreline for water provinces
    /// </summary>
    public void DrawStroke(List<Vector3> pts, Vector3 offset, bool is_sea = true, float min = 0.1f, float max = 0.2f)
    {
        var offset_pts = new List<Vector3>();
        var rend = GetComponent<LineRenderer>();

        foreach (var p in pts)
        {
            offset_pts.Add(p + offset);
        }

        rend.positionCount = pts.Count;
        rend.SetPositions(offset_pts.ToArray());

        var jitter = new AnimationCurve();
        var num_keys = UnityEngine.Random.Range(2, 6);
        var floats = new List<float>();

        for (var i = 0; i < num_keys; i++)
        {
            var rand = UnityEngine.Random.Range(0f, 1f);
            var ct = 0;

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
        var rend = GetComponent<LineRenderer>();

        if (s == Season.SUMMER)
        {
            if (m_is_sea)
            {
                rend.material = SeaShore;

                if (ArtManager.s_art_manager.IsUsingUnderworldTerrain)
                {
                    rend.material = SeaShoreCave;
                }
            }
            else
            {
                rend.material = RiverShore;

                if (ArtManager.s_art_manager.IsUsingUnderworldTerrain)
                {
                    rend.material = RiverShoreCave;
                }
            }
        }
        else
        {
            if (m_is_sea)
            {
                rend.material = SeaShoreWinter;

                if (ArtManager.s_art_manager.IsUsingUnderworldTerrain)
                {
                    rend.material = SeaShoreCave;
                }
            }
            else
            {
                rend.material = RiverShoreWinter;

                if (ArtManager.s_art_manager.IsUsingUnderworldTerrain)
                {
                    rend.material = RiverShoreCave;
                }
            }
        }
    }
}
