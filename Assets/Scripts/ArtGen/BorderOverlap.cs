using System.Collections.Generic;
using UnityEngine;

public static class BorderOverlap
{

    public static void DupOnce(GameObject o, Transform into, List<GameObject> spawns, Vector3 dir)
    {

        var n = spawns.Count;
        for (var i = 0; i < n; ++i)
        {
            var sr2 = GameObject.Instantiate(spawns[i]);
            sr2.transform.position = spawns[i].transform.position + dir;
            sr2.transform.SetParent(into);
            spawns.Add(sr2);
        }
        {

            var sr2 = GameObject.Instantiate(o);
            sr2.transform.position = o.transform.position + dir;
            sr2.transform.SetParent(into);
            spawns.Add(sr2);
        }


    }

    public static List<GameObject> Duplicate(GameObject o, Bounds b, Bounds bounds, Transform into = null)
    {
        if (into == null) into = o.transform.parent;
        var spawns = new List<GameObject>();
        if (b.min.x <= bounds.min.x)
        {
            DupOnce(o, into, spawns, Vector3.right * bounds.size.x);
        }
        if (b.min.y <= bounds.min.y)
        {
            DupOnce(o, into, spawns, Vector3.up * bounds.size.y);
        }
        if (b.max.x >= bounds.max.x)
        {
            DupOnce(o, into, spawns, -Vector3.right * bounds.size.x);
        }
        if (b.max.y >= bounds.max.y)
        {
            DupOnce(o, into, spawns, -Vector3.up * bounds.size.y);
        }
        return spawns;
    }
}
