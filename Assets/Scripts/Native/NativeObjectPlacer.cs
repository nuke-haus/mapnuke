using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile(CompileSynchronously = false)]
public struct NativeObjectPlacer : IJob
{

    public struct Item
    {
        public float size;
        public float cull_radius;
        public float spawn_chance;
        public int place_at_least_1;
        public int extra_border_dist;
        public int cull_nearby_points;
    }

    public static List<int> Invoke(List<Item> item, List<Vector3> points, List<Vector3> border, List<Vector3> already_placed_items)
    {
        var max_res_count = points.Count + already_placed_items.Count;
        var points_res = new NativeArray<Vector3>(max_res_count, Allocator.TempJob);

        for (var i = 0; i < already_placed_items.Count; ++i) points_res[i] = already_placed_items[i];

        var job = new NativeObjectPlacer
        {
            item = new NativeArray<Item>(item.ToArray(), Allocator.TempJob),
            points = new NativeArray<Vector3>(points.ToArray(), Allocator.TempJob),
            border = new NativeArray<Vector3>(border.ToArray(), Allocator.TempJob),
            item_types = new NativeArray<int>(max_res_count, Allocator.TempJob),
            n_out = new NativeArray<int>(1, Allocator.TempJob),
            n_in = points.Count,
            points_res = points_res,
            seed = Random.Range(0, 1000000),
            n_v = already_placed_items.Count,
        };

        job.Schedule().Complete();

        var res = new List<int>();
        var n = job.n_out[0];

        for (var i = already_placed_items.Count; i < n; ++i)
        {
            points[i - already_placed_items.Count] = job.points_res[i];
            res.Add(job.item_types[i]);
        }

        job.item.Dispose();
        job.points.Dispose();
        job.border.Dispose();
        job.item_types.Dispose();
        job.n_out.Dispose();
        job.points_res.Dispose();

        return res;
    }

    [ReadOnly]
    public NativeArray<Item> item;
    public NativeArray<Vector3> border;
    public long seed;

    // Results.
    public NativeArray<Vector3> points;
    public int n_in;

    public NativeArray<int> item_types;
    public int n_v;
    public NativeArray<int> n_out;
    public NativeArray<Vector3> points_res;

    private float next_rand()
    {
        seed = (seed * 7768699 + 41953969) % 1000000009;
        float res = seed % 1000000;
        return res / 1000000;
    }

    private int next_rand(int n)
    {
        seed = (seed * 7768699 + 41953969) % 1000000009;
        return (int)(seed % n);
    }

    private void PlaceOneOfEachType()
    {

    }

    private int RandItem()
    {
        var pick = next_rand(item.Length);
        var max = item.Length;
        var ct = 0;

        while (item[pick].spawn_chance < next_rand() && ct < max)
        {
            ct++;
            pick = next_rand(item.Length);
        }

        if (ct == max) return -1;

        return pick;
    }

    private void RemovePointsNear(Vector3 p, float dist)
    {
        var new_n = 0;

        for (var i = 0; i < n_in; ++i)
        {
            if (Vector3.Distance(points[i], p) > dist) points[new_n++] = points[i];
        }

        n_in = new_n;
    }

    private void RemoveItem(Vector3 p)
    {
        for (var i = n_in - 1; i >= 0; --i)
        {
            if (points[i] == p)
            {
                for (var j = i; j < n_in - 1; ++j)
                {
                    points[j] = points[j + 1];
                }
                n_in -= 1;
                return;
            }
        }
    }

    private bool CheckBorder(Vector3 v, float d)
    {
        var vector = new Vector3(v.x, v.y, 0f);
        for (var i = 0; i < border.Length; ++i)
        {
            if (Vector3.Distance(vector, border[i]) < d)
            {
                return false;
            }
        }
        return true;
    }

    private Vector3 get_valid_position()
    {
        for (var i = n_in - 1; i >= 0; --i)
        {
            if (CheckBorder(points[i], 0.38f)) //TODO: Should this be the sprite size instead of .38?
            {
                return points[i];
            }
        }

        return new Vector3(-100000, 0, 0);
    }

    private void Place(int pick)
    {
        if (n_in == 0)
        {
            return;
        }
        
        var pos = points[n_in - 1];

        if (item[pick].extra_border_dist != 0)
        {
            pos = get_valid_position();
            if (pos.x == new Vector3(-100000, 0, 0).x) return;
        }

        for (var i = 0; i < n_v; ++i)
        {
            if (Vector3.Distance(points_res[i], pos) < item[pick].size)
            {
                RemoveItem(pos); // position is too close to an existing item, delete it
                return;
            }
        }

        if (item[pick].cull_nearby_points == 1)
        {
            RemovePointsNear(pos, item[pick].cull_radius);
        }

        pos.z = -3f;
        item_types[n_v] = pick;
        points_res[n_v] = pos;
        n_v++;
    }

    private void PlaceObjects()
    {
        for (var i = 0; i < item.Length; ++i) // guarantee that we have at least 1 of each valid sprite
        {
            if (item[i].place_at_least_1 == 1)
            {
                Place(i);
            }
        }

        while (n_in > 0) // randomly sprinkle sprites
        {
            var pick = RandItem();

            while (pick == -1 || item[pick].spawn_chance < next_rand())
            {
                pick = RandItem();

                if (pick == -1)
                {
                    break;
                }
            }

            if (pick == -1)
            {
                n_in -= 1;
            }
            else
            {
                Place(pick);
            }
        }

        n_out[0] = n_v;
    }

    public void Execute()
    {
        PlaceObjects();
    }
}
