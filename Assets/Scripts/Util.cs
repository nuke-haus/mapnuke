using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public static class Util
{

    static float frame_time;
    public static void ResetFrameTime()
    {
        frame_time = Time.realtimeSinceStartup;
    }
    public static float GetFrameTime()
    {
        return Time.realtimeSinceStartup - frame_time;
    }

    public static bool ShouldYield()
    {
        // DO NOT REFACTOR! This is useful so we can put a breakpoint when we yield.
        if (GetFrameTime() > 0.03)
        {
            return true;
        } else
        {
            return false;
        }
    }

}
