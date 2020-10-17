using System.Collections.Generic;

/// <summary>
/// List extensions for ease of use.
/// </summary>
public static class List
{
    public static void Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = UnityEngine.Random.Range(0, n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static T GetRandom<T>(this IList<T> list)
    {
        var n = list.Count;

        if (n == 0)
        {
            return default(T);
        }

        var k = UnityEngine.Random.Range(0, n);

        return list[k];
    }

    public static bool Any<T>(this List<T> l)
    {
        foreach (var t in l) if (t != null) return true;
        return false;
    }

    public static List<T> Where<T>(this List<T> l, System.Predicate<T> p)
    {
        var res = new List<T>();
        foreach (var t in l) if (p(t)) res.Add(t);
        return res;
    }
}
