using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

/// <summary>
/// Enum extensions for ease of use.
/// </summary>
public static class EnumExtensions
{

    public static bool IsFlagSet(this Terrain value, Terrain flag)
    {
        var val_long = (long)value;
        var flag_long = (long)flag;
        return ((val_long & flag_long) != 0) || (flag_long == 0); // The original implementation for this would return the wrong value when supplied 0.
    }

    public static IEnumerable<Terrain> GetFlags(this Terrain value)
    {
        foreach (var flag in Enum.GetValues(typeof(Terrain)).Cast<Terrain>())
        {
            if (value.IsFlagSet(flag))
                yield return flag;
        }
    }

    public static Terrain SetFlags(this Terrain value, Terrain flags, bool on)
    {
        var lValue = (long)value;
        var lFlag = (long)(flags);
        if (on)
        {
            lValue |= lFlag;
        }
        else
        {
            lValue &= (~lFlag);
        }
        return (Terrain)Enum.ToObject(typeof(Terrain), lValue);
    }

    public static Terrain SetFlags(this Terrain value, Terrain flags)
    {
        return value.SetFlags(flags, true);
    }

    public static Terrain ClearFlags(this Terrain value, Terrain flags)
    {
        return value.SetFlags(flags, false);
    }

    public static Terrain CombineFlags(this IEnumerable<Terrain> flags)
    {
        long lValue = 0;
        foreach (var flag in flags)
        {
            var lFlag = (long)(flag);
            lValue |= lFlag;
        }
        return (Terrain)Enum.ToObject(typeof(Terrain), lValue);
    }

    public static string GetDescription(this Terrain value)
    {
        var name = Enum.GetName(typeof(Terrain), value);
        if (name != null)
        {
            var field = typeof(Terrain).GetField(name);
            if (field != null)
            {
                var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
        }
        return null;
    }
}
