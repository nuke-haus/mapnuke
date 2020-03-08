using System.Collections;
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
        var job = new NativeCullNearby {
            border = new NativeArray<Vector3>(border.ToArray(), Allocator.TempJob),
            points = new NativeArray<Vector3>(points.ToArray(), Allocator.TempJob),
            n_out = new NativeArray<int>(1, Allocator.TempJob),
            min_dist = min_dist,
        };
        job.Schedule().Complete();
        points.Clear();
        int n = job.n_out[0];
        for (int i = 0; i < n; ++i)
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
        int n = 0;
        float sqr_dist = min_dist * min_dist;
        for (int i = 0; i < points.Length; ++i)
        {
            var cand = points[i];
            bool ok = true;
            for (int j = 0; j < border.Length; ++j)
            {
                var d = border[j] - cand;
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
