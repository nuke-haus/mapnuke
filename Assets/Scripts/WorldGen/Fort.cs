using System.Collections.Generic;
using System.Linq;

public enum Fort
{
    NONE = 0,
    PALISADE = 1,
    ROCKWALLS = 5,
    KELP = 9,
    BRAMBLES = 10,
    CITYPALISADE = 11,
    ICEWALLS = 20,
    WOODENFORT = 28
}

public static class FortHelper
{
    public static Fort GetFort(Node n)
    {
        var terrain = n.ProvinceData.Terrain;
        
        if (terrain.IsFlagSet(Terrain.SEA))
        {
            return Fort.KELP;
        }

        List<Fort> forts = new List<Fort> { Fort.PALISADE };

        if (terrain.IsFlagSet(Terrain.FOREST))
        {
            forts.Add(Fort.BRAMBLES);
            forts.Add(Fort.WOODENFORT);
        }
        if (terrain.IsFlagSet(Terrain.HIGHLAND) || terrain.IsFlagSet(Terrain.WASTE))
        {
            forts.Add(Fort.ROCKWALLS);
        }
        if (terrain.IsFlagSet(Terrain.COLDER))
        {
            forts.Add(Fort.ICEWALLS);
        }
        if (terrain.IsFlagSet(Terrain.SWAMP))
        {
            forts.Add(Fort.WOODENFORT);
        }
        if (terrain.IsFlagSet(Terrain.FARM) || terrain.IsFlagSet(Terrain.LARGEPROV) || terrain == Terrain.PLAINS)
        {
            forts.Add(Fort.CITYPALISADE);
            forts.Add(Fort.PALISADE);
        }
        if (n.Connections.Any(x => x.ConnectionType == ConnectionType.MOUNTAINPASS || x.ConnectionType == ConnectionType.MOUNTAIN))
        {
            forts.Add(Fort.ROCKWALLS);
        }

        return forts.GetRandom();
    }
}