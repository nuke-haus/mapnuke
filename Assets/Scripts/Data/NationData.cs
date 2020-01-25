using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Age
{
    EARLY,
    MIDDLE,
    LATE,
    ALL
}

/// <summary>
/// Represents a nation and all of its pertinent information.
/// </summary>
public class NationData
{
    public string Name
    {
        get;
        private set;
    }

    public bool IsWater
    {
        get
        {
            return WaterPercentage > 0.0f;
        }
    }

    public float WaterPercentage
    {
        get;
        private set;
    }

    public int ID
    {
        get;
        private set;
    }

    public int CapRing
    {
        get;
        private set;
    }

    public Terrain[] TerrainData
    {
        get;
        private set;
    }

    public Terrain CapTerrain
    {
        get;
        private set;
    }

    public Age Age
    {
        get;
        private set;
    }

    public NationData(string name, int id, float water_percent, Age age, Terrain cap, Terrain[] capring_terrain_data)
    {
        Name = name;
        WaterPercentage = water_percent;
        ID = id;
        Age = age;
        CapTerrain = cap;
        TerrainData = capring_terrain_data;
        CapRing = TerrainData.Length;
    }
}

public static class AllNationData
{
    public static List<NationData> AllNations;

    public static void AddNation(NationData d)
    {
        if (AllNations == null)
        {
            AllNations = new List<NationData>();
        }

        AllNations.Add(d);
    }

    public static void Init()
    {
        // dummy nation - not needed
        // AddNation(new NationData("CHOOSE NATION", -1, Age.ALL, new Terrain[] { }));

        // generic starts
        // AddNation(new NationData("Coastal Nation", -1, 0.3f, Age.ALL, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.FOREST, Terrain.SEA })); // Removed this since Dom5 does not properly assign nations to this start.
        AddNation(new NationData("Plains Start", -1, 0.0f, Age.ALL, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("Forest Start", -1, 0.0f, Age.ALL, Terrain.FOREST, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("Cave Start", -1, 0.0f, Age.ALL, Terrain.CAVE, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("Mountain Start", -1, 0.0f, Age.ALL, Terrain.MOUNTAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("Highland Start", -1, 0.0f, Age.ALL, Terrain.HIGHLAND, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("Wasteland Start", -1, 0.0f, Age.ALL, Terrain.WASTE, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("Swamp Start", -1, 0.0f, Age.ALL, Terrain.SWAMP, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("Island Start", -1, 0.7f, Age.ALL, Terrain.PLAINS, new Terrain[] { Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA | Terrain.FOREST, Terrain.SEA | Terrain.DEEPSEA }));
        AddNation(new NationData("Underwater Start", -1, 0.7f, Age.ALL, Terrain.SEA, new Terrain[] { Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA | Terrain.FOREST, Terrain.SEA | Terrain.DEEPSEA }));

        // early age
        AddNation(new NationData("(EA) Arcoscephale", 5, 0.0f, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Ermor", 6, 0.0f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("(EA) Ulm", 7, 0.0f, Age.EARLY, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.HIGHLAND, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Marverni", 8, 0.0f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.FOREST, Terrain.FOREST }));
        AddNation(new NationData("(EA) Sauromatia", 9, 0.0f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(EA) T'ien Chi", 10, 0.0f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Machaka", 11, 0.0f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Mictlan", 12, 0.0f, Age.EARLY, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Abysia", 13, 0.0f, Age.EARLY, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Caelum", 14, 0.0f, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) C'tis", 15, 0.0f, Age.EARLY, Terrain.SWAMP, new Terrain[] { Terrain.WASTE | Terrain.LARGEPROV, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS | Terrain.LARGEPROV }));
        AddNation(new NationData("(EA) Pangaea", 16, 0.0f, Age.EARLY, Terrain.FOREST, new Terrain[] { Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.HIGHLAND }));
        AddNation(new NationData("(EA) Agartha", 17, 0.3f, Age.EARLY, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Tir Na N'og", 18, 0.0f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.HIGHLAND, Terrain.PLAINS, Terrain.FOREST, Terrain.FOREST }));
        AddNation(new NationData("(EA) Fomoria", 19, 0.3f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Vanheim", 20, 0.2f, Age.EARLY, Terrain.HIGHLAND, new Terrain[] { Terrain.PLAINS, Terrain.HIGHLAND, Terrain.HIGHLAND }));
        AddNation(new NationData("(EA) Helheim", 21, 0.0f, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.PLAINS, Terrain.HIGHLAND, Terrain.HIGHLAND }));
        AddNation(new NationData("(EA) Niefelheim", 22, 0.0f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Rus", 24, 0.0f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.CAVE, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Kailasa", 25, 0.0f, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Lanka", 26, 0.0f, Age.EARLY, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Yomi", 27, 0.0f, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.HIGHLAND, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Hinnom", 28, 0.0f, Age.EARLY, Terrain.WASTE, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Ur", 29, 0.0f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS | Terrain.LARGEPROV, Terrain.FOREST, Terrain.SWAMP | Terrain.LARGEPROV }));
        AddNation(new NationData("(EA) Berytos", 30, 0.2f, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.WASTE | Terrain.LARGEPROV, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS | Terrain.LARGEPROV }));
        AddNation(new NationData("(EA) Xibalba", 31, 0.0f, Age.EARLY, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Mekone", 32, 0.0f, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("(EA) Atlantis", 36, 0.7f, Age.EARLY, Terrain.DEEPSEA | Terrain.SEA, new Terrain[] { Terrain.DEEPSEA | Terrain.SEA, Terrain.DEEPSEA | Terrain.SEA, Terrain.FOREST | Terrain.SEA, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(EA) R'lyeh", 37, 0.7f, Age.EARLY, Terrain.DEEPSEA | Terrain.SEA, new Terrain[] { Terrain.DEEPSEA | Terrain.SEA, Terrain.FOREST | Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(EA) Pelagia", 38, 0.7f, Age.EARLY, Terrain.SEA, new Terrain[] { Terrain.DEEPSEA | Terrain.SEA, Terrain.FOREST | Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(EA) Oceania", 39, 0.7f, Age.EARLY, Terrain.FOREST | Terrain.SEA, new Terrain[] { Terrain.SEA | Terrain.FOREST, Terrain.SEA | Terrain.FOREST, Terrain.SEA, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(EA) Therodos", 40, 0.7f, Age.EARLY, Terrain.SEA | Terrain.HIGHLAND, new Terrain[] { Terrain.SEA | Terrain.HIGHLAND, Terrain.SEA | Terrain.FOREST, Terrain.SEA, Terrain.SEA, Terrain.SEA }));

        // middle age
        AddNation(new NationData("(MA) Arcoscephale", 43, 0.0f, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Ermor", 44, 0.0f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.WASTE | Terrain.LARGEPROV }));
        AddNation(new NationData("(MA) Sceleria", 45, 0.0f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("(MA) Pythium", 46, 0.0f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("(MA) Man", 47, 0.0f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Eriu", 48, 0.0f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST, Terrain.FOREST }));
        AddNation(new NationData("(MA) Ulm", 49, 0.0f, Age.MIDDLE, Terrain.HIGHLAND, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Marignon", 50, 0.0f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Mictlan", 51, 0.0f, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(MA) T'ien Chi", 52, 0.0f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Machaka", 53, 0.0f, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.CAVE, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Agartha", 54, 0.3f, Age.MIDDLE, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("(MA) Abysia", 55, 0.0f, Age.MIDDLE, Terrain.CAVE, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("(MA) Caelum", 56, 0.0f, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) C'tis", 57, 0.0f, Age.MIDDLE, Terrain.SWAMP, new Terrain[] { Terrain.SWAMP | Terrain.LARGEPROV, Terrain.PLAINS, Terrain.FOREST, Terrain.FOREST }));
        AddNation(new NationData("(MA) Pangaea", 58, 0.0f, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.HIGHLAND }));
        AddNation(new NationData("(MA) Asphodel", 59, 0.0f, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Vanheim", 60, 0.2f, Age.MIDDLE, Terrain.HIGHLAND, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.HIGHLAND }));
        AddNation(new NationData("(MA) Jotunheim", 61, 0.0f, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Vanarus", 62, 0.0f, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Bandar Log", 63, 0.0f, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Shinuyama", 64, 0.0f, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.CAVE, Terrain.FOREST, Terrain.PLAINS, Terrain.HIGHLAND }));
        AddNation(new NationData("(MA) Ashdod", 65, 0.0f, Age.MIDDLE, Terrain.WASTE, new Terrain[] { Terrain.WASTE | Terrain.LARGEPROV, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Uruk", 66, 0.1f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST, Terrain.FOREST }));
        AddNation(new NationData("(MA) Nazca", 67, 0.0f, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.WASTE | Terrain.LARGEPROV, Terrain.FOREST, Terrain.HIGHLAND, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Xibalba", 68, 0.3f, Age.MIDDLE, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Phlegra", 69, 0.0f, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Phaeacia (Land)", 70, 0.2f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.SEA }));
        AddNation(new NationData("(MA) Phaeacia (Island)", 70, 0.2f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.SEA | Terrain.HIGHLAND, Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(MA) Ind", 71, 0.0f, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.CAVE }));
        AddNation(new NationData("(MA) Na'Ba", 72, 0.0f, Age.MIDDLE, Terrain.WASTE, new Terrain[] { Terrain.WASTE | Terrain.LARGEPROV, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Atlantis", 73, 0.7f, Age.MIDDLE, Terrain.DEEPSEA | Terrain.SEA, new Terrain[] { Terrain.DEEPSEA | Terrain.SEA, Terrain.DEEPSEA | Terrain.SEA, Terrain.FOREST | Terrain.SEA, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(MA) R'lyeh", 74, 0.7f, Age.MIDDLE, Terrain.DEEPSEA | Terrain.SEA, new Terrain[] { Terrain.DEEPSEA | Terrain.SEA, Terrain.FOREST | Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(MA) Pelagia", 75, 0.7f, Age.MIDDLE, Terrain.SEA, new Terrain[] { Terrain.SEA | Terrain.FOREST, Terrain.DEEPSEA | Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(MA) Oceania", 76, 0.7f, Age.MIDDLE, Terrain.SEA | Terrain.FOREST, new Terrain[] { Terrain.SEA | Terrain.FOREST, Terrain.SEA | Terrain.FOREST, Terrain.SEA | Terrain.HIGHLAND, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(MA) Ys", 77, 0.7f, Age.MIDDLE, Terrain.SEA | Terrain.CAVE, new Terrain[] { Terrain.SEA | Terrain.CAVE, Terrain.SEA | Terrain.FOREST, Terrain.SEA, Terrain.SEA, Terrain.SEA }));

        // late age 
        AddNation(new NationData("(LA) Arcoscephale", 80, 0.0f, Age.LATE, Terrain.MOUNTAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST, Terrain.FOREST }));
        AddNation(new NationData("(LA) Pythium", 81, 0.0f, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST, Terrain.FOREST }));
        AddNation(new NationData("(LA) Lemuria", 82, 0.0f, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.WASTE | Terrain.LARGEPROV }));
        AddNation(new NationData("(LA) Man", 83, 0.0f, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Ulm", 84, 0.0f, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Marignon", 85, 0.2f, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST, Terrain.SEA }));
        AddNation(new NationData("(LA) Mictlan", 86, 0.3f, Age.LATE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(LA) T'ien Chi", 87, 0.0f, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Jomon", 89, 0.3f, Age.LATE, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.SEA }));
        AddNation(new NationData("(LA) Agartha", 90, 0.0f, Age.LATE, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Abysia", 91, 0.0f, Age.LATE, Terrain.WASTE, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("(LA) Caelum", 92, 0.0f, Age.LATE, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS | Terrain.LARGEPROV }));
        AddNation(new NationData("(LA) C'tis", 93, 0.0f, Age.LATE, Terrain.SWAMP, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Pangaea", 94, 0.0f, Age.LATE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Midgard", 95, 0.2f, Age.LATE, Terrain.HIGHLAND, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.HIGHLAND }));
        AddNation(new NationData("(LA) Utgard", 96, 0.0f, Age.LATE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Bogarus", 97, 0.0f, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Patala", 98, 0.0f, Age.LATE, Terrain.CAVE, new Terrain[] { Terrain.FOREST, Terrain.CAVE, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Gath", 99, 0.0f, Age.LATE, Terrain.WASTE, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("(LA) Ragha", 100, 0.0f, Age.LATE, Terrain.MOUNTAINS, new Terrain[] { Terrain.SWAMP | Terrain.LARGEPROV, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Xibalba", 101, 0.3f, Age.LATE, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Phlegra", 102, 0.0f, Age.LATE, Terrain.MOUNTAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST }));
        AddNation(new NationData("(LA) Atlantis", 106, 0.7f, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.SEA }));
        AddNation(new NationData("(LA) R'lyeh", 107, 0.7f, Age.LATE, Terrain.DEEPSEA | Terrain.SEA, new Terrain[] { Terrain.DEEPSEA | Terrain.SEA, Terrain.FOREST | Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(LA) Erytheia", 108, 0.7f, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.SEA }));
    }
}
