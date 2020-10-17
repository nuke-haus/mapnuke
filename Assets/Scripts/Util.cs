using UnityEngine;

public static class Util
{
    private static float m_frame_time;

    public static void ResetFrameTime()
    {
        m_frame_time = Time.realtimeSinceStartup;
    }

    public static float GetFrameTime()
    {
        return Time.realtimeSinceStartup - m_frame_time;
    }

    public static bool ShouldYield()
    {
        // DO NOT REFACTOR! This is useful so we can put a breakpoint when we yield.
        if (GetFrameTime() > 0.03)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
