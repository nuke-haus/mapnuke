using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

/// <summary>
/// List extensions for ease of use.
/// </summary>
public static class List
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1); 
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static T GetRandom<T>(this IList<T> list)
    {
        int n = list.Count;

        if (n == 0)
        {
            return default(T);
        }

        int k = UnityEngine.Random.Range(0, n);

        return list[k];
    }
}
