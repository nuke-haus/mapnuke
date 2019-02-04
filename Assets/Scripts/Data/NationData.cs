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

    public NationData(string name, int id, bool water, Age age, Terrain cap, Terrain[] capring_terrain_data)
    {
        Name = name;
        IsWater = water;
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
        // dummy 
        //AddNation(new NationData("CHOOSE NATION", -1, Age.ALL, new Terrain[] { }));

        // TODO: VERIFY THAT ALL NATION IDs ARE RIGHT. THE DOM 5 MAP EDITING MANUAL IS OUTDATED AND MISSING THE NEWER PROVINCE INFO

        // early age
        AddNation(new NationData("(EA) Arcoscephale", 5, false, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Ermor", 6, false, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS | Terrain.SMALLPROV, Terrain.FOREST }));
        AddNation(new NationData("(EA) Ulm", 7, false, Age.EARLY, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.MOUNTAINS, Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Marverni", 8, false, Age.EARLY, Terrain.FOREST, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST, Terrain.FOREST, Terrain.FOREST | Terrain.SMALLPROV }));
        AddNation(new NationData("(EA) Sauromatia", 9, false, Age.EARLY, Terrain.SWAMP, new Terrain[] { Terrain.SWAMP, Terrain.SWAMP, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) T'ien Chi", 10, false, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Machaka", 11, false, Age.EARLY, Terrain.FOREST, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.FOREST, Terrain.FOREST }));
        AddNation(new NationData("(EA) Mictlan", 12, false, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Abysia", 13, false, Age.EARLY, Terrain.CAVE, new Terrain[] { Terrain.WASTE, Terrain.WASTE, Terrain.WASTE, Terrain.WASTE, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Caelum", 14, false, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) C'tis", 15, false, Age.EARLY, Terrain.SWAMP, new Terrain[] { Terrain.WASTE, Terrain.WASTE, Terrain.SWAMP, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Pangaea", 16, false, Age.EARLY, Terrain.FOREST, new Terrain[] { Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV }));
        AddNation(new NationData("(EA) Agartha", 17, false, Age.EARLY, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.CAVE, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Tir Na N'og", 18, false, Age.EARLY, Terrain.HIGHLAND, new Terrain[] { Terrain.SEA, Terrain.SEA, Terrain.SEA | Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Fomoria", 19, false, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.SEA | Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Vanheim", 20, false, Age.EARLY, Terrain.CAVE, new Terrain[] { Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.SEA }));
        AddNation(new NationData("(EA) Helheim", 21, false, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.CAVE, Terrain.HIGHLAND, Terrain.PLAINS, Terrain.SEA }));
        AddNation(new NationData("(EA) Niefelheim", 22, false, Age.EARLY, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.SWAMP, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Rus", 24, false, Age.EARLY, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.CAVE, Terrain.PLAINS, Terrain.PLAINS | Terrain.SMALLPROV }));
        AddNation(new NationData("(EA) Kailasa", 25, false, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS | Terrain.SMALLPROV }));
        AddNation(new NationData("(EA) Lanka", 26, false, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.HIGHLAND, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Yomi", 27, false, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Hinnom", 28, false, Age.EARLY, Terrain.WASTE, new Terrain[] { Terrain.WASTE, Terrain.WASTE | Terrain.LARGEPROV, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Ur", 29, false, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.SWAMP, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Berytos", 30, false, Age.EARLY, Terrain.PLAINS, new Terrain[] { Terrain.WASTE, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Xibalba", 31, false, Age.EARLY, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Mekone", 32, false, Age.EARLY, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(EA) Atlantis", 36, true, Age.EARLY, Terrain.DEEPSEA, new Terrain[] { Terrain.DEEPSEA, Terrain.DEEPSEA, Terrain.DEEPSEA, Terrain.SEA }));
        AddNation(new NationData("(EA) R'lyeh", 37, true, Age.EARLY, Terrain.DEEPSEA, new Terrain[] { Terrain.DEEPSEA, Terrain.DEEPSEA, Terrain.DEEPSEA, Terrain.SEA }));
        AddNation(new NationData("(EA) Pelagia", 38, true, Age.EARLY, Terrain.DEEPSEA, new Terrain[] { Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.DEEPSEA }));
        AddNation(new NationData("(EA) Oceania", 39, true, Age.EARLY, Terrain.SEA, new Terrain[] { Terrain.SEA | Terrain.FOREST, Terrain.SEA | Terrain.FOREST, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(EA) Therodos", 40, false, Age.EARLY, Terrain.HIGHLAND, new Terrain[] { Terrain.SEA | Terrain.HIGHLAND, Terrain.SEA | Terrain.HIGHLAND, Terrain.SEA | Terrain.SMALLPROV, Terrain.SEA, Terrain.SEA }));

        // middle age
        AddNation(new NationData("(MA) Arcoscephale", 43, false, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Ermor", 44, false, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS | Terrain.SMALLPROV, Terrain.PLAINS | Terrain.SMALLPROV }));
        AddNation(new NationData("(MA) Sceleria", 45, false, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS | Terrain.SMALLPROV }));
        AddNation(new NationData("(MA) Pythium", 46, false, Age.MIDDLE, Terrain.SWAMP, new Terrain[] { Terrain.SWAMP, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Man", 47, false, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Eriu", 48, false, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.SWAMP, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Ulm", 49, false, Age.MIDDLE, Terrain.HIGHLAND, new Terrain[] { Terrain.HIGHLAND, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Marignon", 50, false, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Mictlan", 51, false, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.SWAMP }));
        AddNation(new NationData("(MA) T'ien Chi", 52, false, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Machaka", 53, false, Age.MIDDLE, Terrain.CAVE, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Agartha", 54, false, Age.MIDDLE, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.CAVE, Terrain.MOUNTAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Abysia", 55, false, Age.MIDDLE, Terrain.CAVE, new Terrain[] { Terrain.WASTE, Terrain.WASTE, Terrain.WASTE, Terrain.WASTE, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Caelum", 56, false, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.MOUNTAINS, Terrain.MOUNTAINS, Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) C'tis", 57, false, Age.MIDDLE, Terrain.SWAMP, new Terrain[] { Terrain.SWAMP, Terrain.WASTE, Terrain.WASTE, Terrain.WASTE, Terrain.WASTE }));
        AddNation(new NationData("(MA) Pangaea", 58, false, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV }));
        AddNation(new NationData("(MA) Asphodel", 59, false, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.FOREST }));
        AddNation(new NationData("(MA) Vanheim", 60, false, Age.MIDDLE, Terrain.CAVE, new Terrain[] { Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.SEA }));
        AddNation(new NationData("(MA) Jotunheim", 61, false, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST | Terrain.SMALLPROV, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Vanarus", 62, false, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.SWAMP, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Bandar Log", 63, false, Age.MIDDLE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.SWAMP }));
        AddNation(new NationData("(MA) Shinuyama", 64, false, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.HIGHLAND, Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS, Terrain.WASTE }));
        AddNation(new NationData("(MA) Ashdod", 65, false, Age.MIDDLE, Terrain.WASTE, new Terrain[] { Terrain.WASTE, Terrain.WASTE, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Uruk", 66, false, Age.MIDDLE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.SWAMP | Terrain.LARGEPROV }));
        AddNation(new NationData("(MA) Nazca", 67, false, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.WASTE, Terrain.WASTE, Terrain.MOUNTAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Xibalba", 68, false, Age.MIDDLE, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.SEA, Terrain.SWAMP, Terrain.SWAMP | Terrain.LARGEPROV, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(MA) Phlegra", 69, false, Age.MIDDLE, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS | Terrain.SMALLPROV }));
        AddNation(new NationData("(MA) Phaeacia", 70, false, Age.MIDDLE, Terrain.PLAINS | Terrain.SMALLPROV, new Terrain[] { Terrain.SEA | Terrain.HIGHLAND, Terrain.SEA, Terrain.SEA, Terrain.SEA | Terrain.SMALLPROV, Terrain.DEEPSEA }));
        AddNation(new NationData("(MA) Atlantis", 73, true, Age.MIDDLE, Terrain.DEEPSEA, new Terrain[] { Terrain.SEA, Terrain.SEA, Terrain.DEEPSEA, Terrain.DEEPSEA, Terrain.DEEPSEA }));
        AddNation(new NationData("(MA) R'lyeh", 74, true, Age.MIDDLE, Terrain.DEEPSEA, new Terrain[] { Terrain.SEA, Terrain.DEEPSEA, Terrain.DEEPSEA, Terrain.DEEPSEA }));
        AddNation(new NationData("(MA) Pelagia", 75, true, Age.MIDDLE, Terrain.SEA, new Terrain[] { Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA | Terrain.FOREST, Terrain.DEEPSEA }));
        AddNation(new NationData("(MA) Oceania", 76, true, Age.MIDDLE, Terrain.SEA | Terrain.FOREST, new Terrain[] { Terrain.SEA | Terrain.FOREST, Terrain.SEA | Terrain.FOREST, Terrain.SEA | Terrain.FOREST, Terrain.SEA | Terrain.FOREST }));
        AddNation(new NationData("(MA) Ys", 77, true, Age.MIDDLE, Terrain.SEA | Terrain.CAVE, new Terrain[] { Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA }));

        // late age 
        AddNation(new NationData("(LA) Arcoscephale", 80, false, Age.LATE, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Pythium", 81, false, Age.LATE, Terrain.SWAMP, new Terrain[] { Terrain.SWAMP, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Lemuria", 82, false, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS | Terrain.SMALLPROV }));
        AddNation(new NationData("(LA) Man", 83, false, Age.LATE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS | Terrain.SMALLPROV }));
        AddNation(new NationData("(LA) Ulm", 84, false, Age.LATE, Terrain.HIGHLAND, new Terrain[] { Terrain.HIGHLAND, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Marignon", 85, false, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.HIGHLAND | Terrain.LARGEPROV, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.SEA }));
        AddNation(new NationData("(LA) Mictlan", 86, false, Age.LATE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.SWAMP }));
        AddNation(new NationData("(LA) T'ien Chi", 87, false, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Jomon", 89, false, Age.LATE, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.HIGHLAND, Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Agartha", 90, false, Age.LATE, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.CAVE, Terrain.MOUNTAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Abysia", 91, false, Age.LATE, Terrain.CAVE, new Terrain[] { Terrain.WASTE, Terrain.WASTE, Terrain.WASTE, Terrain.WASTE, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Caelum", 92, false, Age.LATE, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.MOUNTAINS, Terrain.MOUNTAINS, Terrain.HIGHLAND, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) C'tis", 93, false, Age.LATE, Terrain.WASTE, new Terrain[] { Terrain.SWAMP, Terrain.WASTE, Terrain.WASTE, Terrain.WASTE, Terrain.WASTE }));
        AddNation(new NationData("(LA) Pangaea", 94, false, Age.LATE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Midgard", 95, false, Age.LATE, Terrain.CAVE, new Terrain[] { Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.FOREST | Terrain.SMALLPROV, Terrain.SEA }));
        AddNation(new NationData("(LA) Utgard", 96, false, Age.LATE, Terrain.FOREST, new Terrain[] { Terrain.FOREST, Terrain.FOREST | Terrain.SMALLPROV, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Bogarus", 97, false, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.FOREST, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Patala", 98, false, Age.LATE, Terrain.CAVE, new Terrain[] { Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.FOREST, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Gath", 99, false, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.WASTE, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Ragha", 100, false, Age.LATE, Terrain.MOUNTAINS, new Terrain[] { Terrain.HIGHLAND, Terrain.SEA, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Xibalba", 101, false, Age.LATE, Terrain.CAVE, new Terrain[] { Terrain.CAVE, Terrain.CAVE, Terrain.SWAMP, Terrain.SEA, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Phlegra", 102, false, Age.LATE, Terrain.MOUNTAINS, new Terrain[] { Terrain.MOUNTAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS, Terrain.PLAINS }));
        AddNation(new NationData("(LA) Atlantis", 106, true, Age.LATE, Terrain.SEA, new Terrain[] { Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.PLAINS, Terrain.SWAMP }));
        AddNation(new NationData("(LA) R'lyeh", 107, true, Age.LATE, Terrain.DEEPSEA, new Terrain[] { Terrain.DEEPSEA, Terrain.DEEPSEA, Terrain.SEA, Terrain.SEA }));
        AddNation(new NationData("(LA) Erytheia", 108, false, Age.LATE, Terrain.PLAINS, new Terrain[] { Terrain.SWAMP, Terrain.PLAINS, Terrain.PLAINS, Terrain.SEA, Terrain.SEA, Terrain.SEA, Terrain.SEA }));
    }
}
