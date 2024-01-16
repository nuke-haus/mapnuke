using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot]
public class CustomNameDataCollection
{
    [XmlElement("NameData")]
    public List<CustomNameData> Data;

    public CustomNameDataCollection()
    {
        Data = new List<CustomNameData>();
    }

    public void Add(CustomNameDataCollection n)
    {
        Data.AddRange(n.Data);
    }

    public string GetRandomCaveString(string id, Terrain terrain)
    {
        var cave_terrain = Terrain.CAVE | terrain;
        var valid_data = Data.Where(x => x.ID == id &&
                                        !x.BlockedTerrain.Where(y => cave_terrain.HasFlag(y)).Any() &&
                                        (x.Terrain == Terrain.GENERICSTART || ((x.Terrain != Terrain.PLAINS && cave_terrain.IsFlagSet(x.Terrain) && x.Terrain.IsFlagSet(Terrain.CAVE)))));

        var strings = new List<string>();

        if (!valid_data.Any())
        {
            return null;
        }

        foreach (var data in valid_data)
        {
            strings.AddRange(data.Strings);
        }

        return strings.GetRandom();
    }

    public string GetRandomString(string id, Terrain terrain, bool is_plains) 
    {
        var valid_data = Data.Where(x => x.ID == id &&
                                        !x.BlockedTerrain.Where(y => terrain.HasFlag(y)).Any() && 
                                        (x.Terrain == Terrain.GENERICSTART || ((x.Terrain != Terrain.PLAINS && terrain.HasFlag(x.Terrain)) || (x.Terrain == Terrain.PLAINS && is_plains))));

        var strings = new List<string>();

        if (!valid_data.Any())
        {
            return null;
        }

        foreach (var data in valid_data)
        {
            strings.AddRange(data.Strings);
        }

        return strings.GetRandom();
    }
}

[XmlRoot]
public class CustomNameFormatCollection
{
    [XmlElement("NameFormat")]
    public List<CustomNameFormat> Data;

    public CustomNameFormatCollection()
    {
        Data = new List<CustomNameFormat>();
    }

    public void Add(CustomNameFormatCollection n)
    {
        Data.AddRange(n.Data);
    }

    public CustomNameFormat GetRandom(Terrain terrain)
    {
        var valid = Data.Where(x => !x.BlockedTerrain.Where(y => terrain.HasFlag(y)).Any());
        return valid.GetRandom();
    }
}

/// <summary>
/// Represents a formatting rule for a custom name.
/// </summary>
public class CustomNameFormat
{
    [XmlElement("Segment")]
    public List<string> Strings;

    [XmlElement("BlockedTerrain")]
    public List<Terrain> BlockedTerrain = new List<Terrain> { Terrain.GENERICSTART }; // filter against this value

    public CustomNameFormat()
    {
    }
}

/// <summary>
/// Represents a collection of strings used by custom names.
/// </summary>
public class CustomNameData
{
    [XmlElement("ID")]
    public string ID;

    [XmlElement("Terrain")]
    public Terrain Terrain = Terrain.GENERICSTART; // default nothrone flag is treated as global data that gets used on all province types

    [XmlElement("BlockedTerrain")]
    public List<Terrain> BlockedTerrain = new List<Terrain> { Terrain.GENERICSTART }; // filter against this value

    [XmlElement("String")]
    public List<string> Strings;

    public CustomNameData()
    {
    }

    public string GetRandomString()
    {
        return Strings.GetRandom();
    }
}