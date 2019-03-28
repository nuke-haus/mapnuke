using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

/// <summary>
/// These are all of the frequency ranges for the various connection and province types.
/// </summary>
public class GeneratorSettings
{
    public static GeneratorSettings s_generator_settings;

    public static void Initialize()
    {
        s_generator_settings = new GeneratorSettings();
    }

    public FloatRange SwampFreq = new FloatRange(0.04f, 0.06f);
    public FloatRange WasteFreq = new FloatRange(0.04f, 0.06f);
    public FloatRange MountainFreq = new FloatRange(0.05f, 0.07f);
    public FloatRange HighlandFreq = new FloatRange(0.04f, 0.06f);
    public FloatRange CaveFreq = new FloatRange(0.05f, 0.07f);
    public FloatRange FarmFreq = new FloatRange(0.16f, 0.18f);
    public FloatRange ForestFreq = new FloatRange(0.16f, 0.18f);
    public FloatRange LakeFreq = new FloatRange(0.06f, 0.08f);

    public FloatRange LargeFreq = new FloatRange(0.28f, 0.32f);
    public FloatRange SmallFreq = new FloatRange(0.10f, 0.14f);

    public FloatRange CliffFreq = new FloatRange(0.08f, 0.10f);
    public FloatRange CliffPassFreq = new FloatRange(0.08f, 0.10f);
    public FloatRange RiverFreq = new FloatRange(0.08f, 0.10f);
    public FloatRange DeepRiverFreq = new FloatRange(0.08f, 0.10f);
    public FloatRange RoadFreq = new FloatRange(0.02f, 0.04f);
}

public class FloatRange // simple class for managing float random ranges
{
    public float Min
    {
        get;
        private set;
    }

    public float Max
    {
        get;
        private set;
    }

    public int MinInt
    {
        get
        {
            return Mathf.RoundToInt(Min * 100f);
        }
    }

    float m_orig_min;
    float m_orig_max;

    public int MaxInt
    {
        get
        {
            return Mathf.RoundToInt(Max * 100f);
        }
    }

    public FloatRange(float min, float max)
    {
        m_orig_min = min;
        m_orig_max = max;

        Update(min, max);
    }

    public void Reset()
    {
        Min = m_orig_min;
        Max = m_orig_max;
    }

    public void Update(float min, float max)
    {
        Min = Mathf.Min(min, max);
        Max = max;
    }

    public void Update(int min, int max)
    {
        float fmin = min / 100f;
        float fmax = max / 100f;

        Update(fmin, fmax);
    }

    public float GetRandom()
    {
        return UnityEngine.Random.Range(Min, Max);
    }
}
