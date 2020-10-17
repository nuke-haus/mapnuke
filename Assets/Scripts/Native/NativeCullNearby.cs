using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile(CompileSynchronously = false)]
public struct NativeCullNearby : IJob
{
    public static void Invoke(List<Vector3> points, List<Vector3> border, float min_dist)
    {
        var job = new NativeCullNearby
        {
            border = new NativeArray<Vector3>(border.ToArray(), Allocator.TempJob),
            points = new NativeArray<Vector3>(points.ToArray(), Allocator.TempJob),
            n_out = new NativeArray<int>(1, Allocator.TempJob),
            min_dist = min_dist,
        };

        job.Schedule().Complete();
        points.Clear();

        var n = job.n_out[0];

        for (var i = 0; i < n; ++i)
        {
            points.Add(job.points[i]);
        }

        job.border.Dispose();
        job.points.Dispose();
        job.n_out.Dispose();
    }

    [ReadOnly]
    public NativeArray<Vector3> border;

    [ReadOnly]
    public float min_dist;

    public NativeArray<Vector3> points;
    public NativeArray<int> n_out;

    public void Execute()
    {
        var n = 0;

        for (var i = 0; i < points.Length; ++i)
        {
            var cand = points[i];
            var ok = true;

            for (var j = 0; j < border.Length; ++j)
            {
                if (Vector3.Distance(border[j], cand) < min_dist)
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
            {
                points[n++] = cand;
            }
        }

        n_out[0] = n;
    }
}
