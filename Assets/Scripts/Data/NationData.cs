using System.Collections.Generic;
using System.Xml.Serialization;

public enum Age
{
    EARLY,
    MIDDLE,
    LATE,
    ALL
}

[XmlRoot]
public class NationCollection
{
    [XmlElement("Nation")]
    public List<NationData> Nations;

    public NationCollection()
    {
        Nations = new List<NationData>();
    }

    public NationCollection(List<NationData> list)
    {
        Nations = list;
    }

    public void Add(NationData n)
    {
        Nations.Add(n);
    }
}

/// <summary>
/// Represents a nation and all of its pertinent information.
/// </summary>
public class NationData
{
    [XmlElement("Name")]
    public string Name;

    [XmlElement("ID")]
    public int ID;

    [XmlElement("WaterPercentage")]
    public float WaterPercentage;

    [XmlElement("Age")]
    public Age Age;

    [XmlElement("CapRingSize")]
    public int CapRingSize;

    [XmlElement("CapTerrain")]
    public Terrain CapTerrain;

    [XmlElement("TerrainData")]
    public Terrain[] TerrainData;

    [XmlIgnore]
    public bool IsWater
    {
        get
        {
            return WaterPercentage > 0.0f;
        }
    }

    [XmlIgnore]
    public bool IsIsland
    {
        get
        {
            var water_ring = true;
            foreach (var terrain in TerrainData)
            {
                if (!terrain.IsFlagSet(Terrain.SEA))
                {
                    water_ring = false;
                    break;
                }
            }
            return !CapTerrain.IsFlagSet(Terrain.SEA) && water_ring;
        }
    }

    public NationData()
    {

    }

    public NationData(string name, int id, float water_percent, Age age, int capring_count, Terrain cap, Terrain[] capring_terrain_data)
    {
        Name = name;
        WaterPercentage = water_percent;
        ID = id;
        Age = age;
        CapTerrain = cap;
        TerrainData = capring_terrain_data;
        CapRingSize = capring_count;
    }
}

public static class AllNationData
{
    public static List<NationData> AllNations;

    public static void SortNations()
    {
        AllNations.Sort((x, y) => x.Name.CompareTo(y.Name));
    }

    public static void AddNations(NationCollection coll)
    {
        foreach (var d in coll.Nations)
        {
            AllNations.Add(d);
        }
    }

    public static void AddNation(NationData d)
    {
        AllNations.Add(d);
    }

    public static void Init()
    {
        if (AllNations == null)
        {
            AllNations = new List<NationData>();
        }
    }
}
