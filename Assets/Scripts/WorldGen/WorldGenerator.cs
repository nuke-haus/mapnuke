using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// This class handles the creation of all conceptual parts of the world.
/// This is where province types and connection types are determined.
/// </summary>
static class WorldGenerator
{
    static int m_num_players;
    static bool m_nat_starts = true;
    static bool m_teamplay = false;
    static bool m_cluster_water = true;
    static NodeLayout m_layout;
    static List<Node> m_nodes;
    static List<Node> m_starts;
    static List<Connection> m_connections;
    static List<NodeLayout> m_layouts;
    static List<PlayerData> m_nations;

    public static void GenerateWorld(bool teamplay, bool cluster_water, bool nat_starts, List<PlayerData> picks, NodeLayout layout)
    {
        m_num_players = picks.Count;
        m_nations = picks;
        m_nat_starts = nat_starts;
        m_teamplay = teamplay;
        m_cluster_water = cluster_water;
        m_layout = layout;

        generate_nodes();
        generate_connections();
        calculate_triangles();
        generate_caprings();
        generate_seas();
        generate_lakes();
        assign_water_terrain();

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsAssignedTerrain && !x.ProvinceData.IsWater).ToList();
        List<Connection> valid_conn = m_connections.Where(x => !x.IsCap && !x.IsInsideCapRing && !x.IsTouchingSea).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.ProvinceData.IsWater).ToList();
        }

        generate_swamps(valid);
        generate_misc(valid);
        generate_rivers(valid_conn);
        generate_cliffs(valid_conn);
        generate_roads(valid_conn);
        generate_farms(valid);
        generate_sized(valid);
        generate_thrones();
        cleanup_connections();
    }

    public static List<Connection> GetConnections()
    {
        return m_connections;
    }

    public static List<Node> GetNodes()
    {
        return m_nodes;
    }

    public static NodeLayout GetLayout()
    {
        return m_layout;
    }

    static void calculate_triangles()
    {
        foreach (Connection c in m_connections.Where(x => x.Diagonal))
        {
            Connection anchor = c.Adjacent[0];

            foreach (Connection c2 in c.Adjacent)
            {
                if (c2 == anchor)
                {
                    continue;
                }

                if (unique_nodes(c, c2, anchor) == 3)
                {
                    c.AddTriangleLink(c2);
                    c.AddTriangleLink(anchor);

                    c2.AddTriangleLink(c);
                    c2.AddTriangleLink(anchor);

                    anchor.AddTriangleLink(c);
                    anchor.AddTriangleLink(c2);

                    List<Connection> others = c.Adjacent.Where(x => x != c && x != c2 && x != anchor).ToList();

                    c.AddTriangleLink(others[0]);
                    c.AddTriangleLink(others[1]);

                    others[0].AddTriangleLink(c);
                    others[0].AddTriangleLink(others[1]);

                    others[1].AddTriangleLink(c);
                    others[1].AddTriangleLink(others[0]);
                }
            }
        }
    }

    static int unique_nodes(Connection m1, Connection m2, Connection m3)
    {
        List<Node> temp = new List<Node>();

        if (!temp.Contains(m1.Node1))
        {
            temp.Add(m1.Node1);
        }
        if (!temp.Contains(m1.Node2))
        {
            temp.Add(m1.Node2);
        }
        if (!temp.Contains(m2.Node1))
        {
            temp.Add(m2.Node1);
        }
        if (!temp.Contains(m2.Node2))
        {
            temp.Add(m2.Node2);
        }
        if (!temp.Contains(m3.Node1))
        {
            temp.Add(m3.Node1);
        }
        if (!temp.Contains(m3.Node2))
        {
            temp.Add(m3.Node2);
        }

        return temp.Count;
    }
    
    static void cleanup_connections()
    {
        foreach (Node n in m_nodes)
        {
            if (n.ProvinceData.IsWater) // make sure no water provinces have incorrect connections
            {
                foreach (Connection c in n.Connections)
                {
                    c.SetConnection(ConnectionType.STANDARD);
                }
            }
            else if (n.HasNation)
            {
                foreach (Connection c in n.Connections)
                {
                    if (c.ConnectionType != ConnectionType.STANDARD)
                    {
                        c.SetConnection(ConnectionType.STANDARD);
                    }
                }
            }
        }
    }

    static void generate_roads(List<Connection> valid)
    {
        List<Connection> starts = valid.Where(x => x.ConnectionType == ConnectionType.STANDARD).ToList();

        int max_roads = (int)(valid.Count * GeneratorSettings.s_generator_settings.RoadFreq.GetRandom()); 
        int count = 0;

        while (count < max_roads && starts.Any())
        {
            Connection con = starts.GetRandom();
            starts.Remove(con);

            con.SetConnection(ConnectionType.ROAD);
            count++;
        }
    }

    static void generate_cliffs(List<Connection> valid)
    {
        List<Connection> starts = valid.Where(x => x.ConnectionType == ConnectionType.STANDARD).ToList();

        int max_passes = (int)(valid.Count * GeneratorSettings.s_generator_settings.CliffPassFreq.GetRandom()); 
        int max_cliffs = (int)(valid.Count * GeneratorSettings.s_generator_settings.CliffFreq.GetRandom()); 
        int count = 0;

        List<Node> water = m_nodes.Where(x => x.ProvinceData.IsWater).ToList();
        List<Connection> bad = new List<Connection>();

        if (water.Any())
        {
            foreach (Node n in water)
            {
                foreach (Node n1 in n.ConnectedNodes)
                {
                    foreach (Node n2 in n.ConnectedNodes)
                    {
                        if (n1 == n2)
                        {
                            continue;
                        }

                        Connection c = n1.GetConnectionTo(n2);

                        if (c != null && valid.Contains(c) && !bad.Contains(c))
                        {
                            bad.Add(c);
                        }
                    }
                }
            }
        }

        while (count < max_cliffs && starts.Any())
        {
            Connection con = starts.GetRandom();
            starts.Remove(con);

            if (con.Node1.NumStandardConnections < 3 || con.Node2.NumStandardConnections < 3 || con.ConnectionType != ConnectionType.STANDARD || con.IsCap || bad.Contains(con) ||
                    con.TriangleLinked.Any(x => x.ConnectionType == ConnectionType.RIVER || x.ConnectionType == ConnectionType.SHALLOWRIVER))
            {
                continue;
            }

            con.SetConnection(ConnectionType.MOUNTAIN);

            count++;
        }

        count = 0;

        while (count < max_passes && starts.Any())
        {
            Connection con = starts.GetRandom();
            starts.Remove(con);

            if (con.Node1.NumStandardConnections < 3 || con.Node2.NumStandardConnections < 3 || con.ConnectionType != ConnectionType.STANDARD || con.IsCap || bad.Contains(con) ||
                    con.TriangleLinked.Any(x => x.ConnectionType == ConnectionType.RIVER || x.ConnectionType == ConnectionType.SHALLOWRIVER))
            {
                continue;
            }

            con.SetConnection(ConnectionType.MOUNTAINPASS);

            count++;
        }

        int num_flips = Mathf.Max(max_passes, max_cliffs);

        List<Connection> mounts = m_connections.Where(x => x.ConnectionType == ConnectionType.MOUNTAIN).ToList();
        List<Connection> passes = m_connections.Where(x => x.ConnectionType == ConnectionType.MOUNTAINPASS).ToList();

        for (int i = 0; i < num_flips; i++)
        {
            Connection c1 = mounts.GetRandom();
            Connection c2 = passes.GetRandom();

            if (c1 == null || c2 == null)
            {
                break;
            }

            mounts.Remove(c1);
            passes.Remove(c2);
            mounts.Add(c2);
            passes.Add(c1);

            c1.SetConnection(ConnectionType.MOUNTAINPASS);
            c2.SetConnection(ConnectionType.MOUNTAIN);
        }
    }

    static Connection get_connection_weighted(List<Connection> conns)
    {
        if (conns.Count < 5)
        {
            return conns.GetRandom();
        }

        conns.Shuffle();
        conns = conns.OrderBy(x => x.NumSeaSwamp).ToList();

        int pos = UnityEngine.Random.Range(0, Mathf.RoundToInt(conns.Count * 0.5f));

        return conns[pos];
    }

    static void generate_rivers(List<Connection> valid)
    {
        List<Node> water = m_nodes.Where(x => x.ProvinceData.IsWaterSwamp).ToList();

        if (!water.Any())
        {
            water = m_nodes;
        }

        List<Connection> tertiary = new List<Connection>();
        List<Connection> starts = new List<Connection>();
        //List<Connection> invalid = m_connections.Where(x => x.ConnectionType != ConnectionType.STANDARD || x.Node1.HasNation || x.Node2.HasNation || x.IsTouchingSea || x.IsInsideCapRing).ToList();

        foreach (Node n in water)
        {
            foreach (Node n1 in n.ConnectedNodes)
            {
                foreach (Node n2 in n.ConnectedNodes)
                {
                    if (n1 == n2)
                    {
                        continue;
                    }

                    Connection c = n1.GetConnectionTo(n2);

                    if (c != null && valid.Contains(c) && !starts.Contains(c)) 
                    {
                        starts.Add(c);
                    }
                }
            }
        }

        int max_rivers = (int)(valid.Count * GeneratorSettings.s_generator_settings.DeepRiverFreq.GetRandom());
        int max_shallow = (int)(valid.Count * GeneratorSettings.s_generator_settings.RiverFreq.GetRandom());
        int count = 0;

        Connection cur = get_connection_weighted(starts);

        while (count < max_shallow && starts.Any())
        {
            starts.Remove(cur);

            if (!starts.Any())
            {
                starts.AddRange(tertiary);
                tertiary = new List<Connection>();
            }

            if (cur.Node1.NumStandardConnections < 3 || cur.Node2.NumStandardConnections < 3 || cur.ConnectionType != ConnectionType.STANDARD || cur.IsCap ||
                cur.Adjacent.Any(x => x.ConnectionType == ConnectionType.MOUNTAIN || x.ConnectionType == ConnectionType.MOUNTAINPASS))
            {
                cur = get_connection_weighted(starts);
                continue;
            }

            cur.SetConnection(ConnectionType.SHALLOWRIVER);

            var adj = cur.Adjacent.Where(x => valid.Contains(x) && !tertiary.Contains(x) && !starts.Contains(x) && x.ConnectionType == ConnectionType.STANDARD && x.SharesNode(cur));

            if (UnityEngine.Random.Range(0, 10) < 4)
            {
                tertiary.AddRange(adj);
            }
            else
            {
                starts.AddRange(adj);
            }

            cur = get_connection_weighted(starts);
            count++;
        }

        count = 0;

        while (count < max_rivers && starts.Any())
        {
            starts.Remove(cur);

            if (!starts.Any())
            {
                starts.AddRange(tertiary);
                tertiary = new List<Connection>();
            }

            if (cur.Node1.NumStandardConnections < 3 || cur.Node2.NumStandardConnections < 3 || cur.ConnectionType != ConnectionType.STANDARD || cur.IsCap ||
                cur.Adjacent.Any(x => x.ConnectionType == ConnectionType.MOUNTAIN || x.ConnectionType == ConnectionType.MOUNTAINPASS))
            {
                cur = get_connection_weighted(starts);
                continue;
            }

            cur.SetConnection(ConnectionType.RIVER);

            var adj = cur.Adjacent.Where(x => valid.Contains(x) && !tertiary.Contains(x) && !starts.Contains(x) && x.ConnectionType == ConnectionType.STANDARD && x.SharesNode(cur));

            if (UnityEngine.Random.Range(0, 10) < 4)
            {
                tertiary.AddRange(adj);
            }
            else
            {
                starts.AddRange(adj);
            }

            cur = get_connection_weighted(starts);
            count++;
        }

        int num_flips = Mathf.Max(max_rivers, max_shallow);

        List<Connection> rivers = m_connections.Where(x => x.ConnectionType == ConnectionType.RIVER).ToList();
        List<Connection> shallows = m_connections.Where(x => x.ConnectionType == ConnectionType.SHALLOWRIVER).ToList();

        for (int i = 0; i < num_flips; i++)
        {
            Connection c1 = rivers.GetRandom();
            Connection c2 = shallows.GetRandom();

            if (c1 == null || c2 == null)
            {
                break;
            }

            rivers.Remove(c1);
            shallows.Remove(c2);
            rivers.Add(c2);
            shallows.Add(c1);

            c1.SetConnection(ConnectionType.SHALLOWRIVER);
            c2.SetConnection(ConnectionType.RIVER);
        }
    }

    static void generate_seas()
    {
        foreach (Node n in m_starts)
        {
            if (n.Nation.NationData.IsWater)
            {
                int modifier = n.ConnectedNodes.Count + 1; // subtract their capring from the total count
                int count = 0;
                int num_water = Mathf.RoundToInt(m_layout.ProvsPerPlayer * n.Nation.NationData.WaterPercentage);

                if (modifier >= num_water) // there should be at least 1 water province added
                {
                    modifier = num_water - 1;
                }

                int total_iterations = 0;
                
                while (count < num_water - modifier)
                {
                    int iterations = UnityEngine.Random.Range(1, 3);

                    if (total_iterations > 30)
                    {
                        iterations = 3;
                    }

                    int i = 0;
                    Node cur = n.ConnectedNodes.GetRandom().ConnectedNodes.GetRandom();
                    total_iterations++;

                    while (i < iterations && count < num_water - modifier)
                    {
                        if (!cur.HasNation && !cur.IsCapRing && !cur.ProvinceData.IsWater)
                        {
                            cur.ProvinceData.SetTerrainFlags(Terrain.SEA);

                            count++;
                        }

                        i++;
                        cur = cur.ConnectedNodes.GetRandom();
                    }
                }
            }
        }
    }

    static void assign_water_terrain()
    {
        //float num_farm = GeneratorSettings.s_generator_settings.FarmFreq.GetRandom() * 0.5f; 
        float num_forest = GeneratorSettings.s_generator_settings.ForestFreq.GetRandom() + GeneratorSettings.s_generator_settings.FarmFreq.GetRandom();
        float num_trench = GeneratorSettings.s_generator_settings.HighlandFreq.GetRandom();
        float num_deeps = GeneratorSettings.s_generator_settings.MountainFreq.GetRandom();
        float num_cave = GeneratorSettings.s_generator_settings.CaveFreq.GetRandom();

        List<Node> water = m_nodes.Where(x => x.ProvinceData.IsWater && !x.HasNation && !x.IsAssignedTerrain).ToList();

        Dictionary<Terrain, float> dict = new Dictionary<Terrain, float>();
        //dict.Add(Terrain.FARM, num_farm);
        dict.Add(Terrain.HIGHLAND, num_trench);
        dict.Add(Terrain.FOREST, num_forest);
        dict.Add(Terrain.CAVE, num_cave);

        if (!m_nat_starts)
        {
            water = m_nodes.Where(x => x.ProvinceData.IsWater).ToList();
        }

        int ct = water.Count;

        foreach (KeyValuePair<Terrain, float> pair in dict)
        {
            int num = Mathf.RoundToInt(pair.Value * ct);

            if (pair.Value > 0.01f && num == 0)
            {
                num = 1;
            }

            if (num == 0 || !water.Any())
            {
                continue;
            }

            for (int i = 0; i < num; i++)
            {
                Node n = water.GetRandom();
                water.Remove(n);

                n.ProvinceData.AddTerrainFlag(pair.Key);
            }
        }

        water = m_nodes.Where(x => x.ProvinceData.IsWater && !x.HasNation && !x.IsAssignedTerrain && !x.ProvinceData.Terrain.IsFlagSet(Terrain.FARM) && !x.ProvinceData.Terrain.IsFlagSet(Terrain.FOREST)).ToList();

        if (!m_nat_starts)
        {
            water = m_nodes.Where(x => x.ProvinceData.IsWater && !x.ProvinceData.Terrain.IsFlagSet(Terrain.FARM) && !x.ProvinceData.Terrain.IsFlagSet(Terrain.FOREST)).ToList();
        }

        int total_deep = Mathf.RoundToInt(num_deeps * ct);

        if (num_deeps > 0.01f && total_deep == 0)
        {
            total_deep = 1;
        }

        for (int i = 0; i < total_deep; i++)
        {
            if (!water.Any())
            {
                break;
            }

            Node n = water.GetRandom();
            water.Remove(n);

            n.ProvinceData.AddTerrainFlag(Terrain.DEEPSEA);
        }
    }

    static void generate_farms(List<Node> original)
    {
        float num_farms = GeneratorSettings.s_generator_settings.FarmFreq.GetRandom();

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && x.ProvinceData.IsPlains).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => x.ProvinceData.IsPlains).ToList();
        }

        int max = Mathf.RoundToInt(num_farms * original.Count);
        int i = 0;
        
        while (i < max && valid.Any())
        {
            Node n = valid.GetRandom();
            valid.Remove(n);

            if (n.GetConnectedProvincesOfType(Terrain.FARM, true).Count > 1 && num_farms < 0.3f)
            {
                valid.Remove(n);
                continue;
            }

            n.ProvinceData.SetTerrainFlags(Terrain.FARM);

            i++;
        }
    }

    static void generate_thrones()
    {
        int num_water = m_starts.Where(x => x.Nation.NationData.WaterPercentage > 0.3f).Count();
        int water_ct = UnityEngine.Random.Range(-2, 1);

        foreach (Node n in m_nodes)
        {
            if (m_layout.Spawns.Any(x => x.X == n.X && n.Y == x.Y && x.SpawnType == SpawnType.THRONE))
            {
                if (n.ProvinceData.IsWater)
                {
                    water_ct++;

                    if (water_ct > num_water)
                    {
                        n.ProvinceData.SetTerrainFlags(Terrain.SWAMP);
                    }
                }

                n.ProvinceData.AddTerrainFlag(Terrain.THRONE);
            }
        }
    }

    static void generate_sized(List<Node> original)
    {
        float num_large = GeneratorSettings.s_generator_settings.LargeFreq.GetRandom();  
        float num_small = GeneratorSettings.s_generator_settings.SmallFreq.GetRandom(); 

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.HasNation).ToList();
        }

        Dictionary<Node, List<Node>> dict = new Dictionary<Node, List<Node>>();

        foreach (Node n in m_starts)
        {
            List<Node> nodes = new List<Node>();

            foreach (Node conn in n.ConnectedNodes)
            {
                foreach (Node t in conn.ConnectedNodes.Where(x => !x.IsCapRing && !x.HasNation))
                {
                    nodes.Add(t);
                }
            }

            dict.Add(n, nodes);
        }

        int max = Mathf.RoundToInt((num_large * original.Count) / m_starts.Count);
        int i = 0;

        while (i < max) // fairly assign large provinces to each nation 
        {
            foreach (KeyValuePair<Node, List<Node>> pair in dict)
            {
                if (pair.Value.Any())
                {
                    Node n = pair.Value.GetRandom();
                    pair.Value.Remove(n);

                    n.ProvinceData.AddTerrainFlag(Terrain.LARGEPROV);
                }
                else
                {
                    if (valid.Any())
                    {
                        Node alt = valid.GetRandom();
                        valid.Remove(alt);

                        alt.ProvinceData.AddTerrainFlag(Terrain.LARGEPROV);
                    }
                    /*else
                    {
                        Debug.LogError("No provinces left to tag as large");
                    }*/
                }
            }

            i++;
        }

        valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV)).ToList();
        max = Mathf.RoundToInt(num_small * original.Count);
        i = 0;

        while (i < max && valid.Any())
        {
            Node n = valid.GetRandom();
            valid.Remove(n);

            if (n.GetConnectedProvincesOfType(Terrain.SMALLPROV, true).Count > 1 && num_small < 0.3f)
            {
                continue;
            }

            n.ProvinceData.AddTerrainFlag(Terrain.SMALLPROV);

            i++;
        }
    }

    static void generate_misc(List<Node> original)
    {
        float num_highlands = GeneratorSettings.s_generator_settings.HighlandFreq.GetRandom(); 
        float num_mountains = GeneratorSettings.s_generator_settings.MountainFreq.GetRandom(); 
        float num_forests = GeneratorSettings.s_generator_settings.ForestFreq.GetRandom();
        float num_caves = GeneratorSettings.s_generator_settings.CaveFreq.GetRandom(); 
        float num_waste = GeneratorSettings.s_generator_settings.WasteFreq.GetRandom(); 

        if (GeneratorSettings.s_generator_settings.UseClassicMountains)
        {
            num_mountains = 0f;
        }

        Dictionary<Terrain, float> dict = new Dictionary<Terrain, float>();
        dict.Add(Terrain.WASTE, num_waste);
        dict.Add(Terrain.HIGHLAND, num_highlands);
        dict.Add(Terrain.MOUNTAINS, num_mountains);
        dict.Add(Terrain.FOREST, num_forests);
        dict.Add(Terrain.CAVE, num_caves);

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsAssignedTerrain && x.ProvinceData.IsPlains).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => x.ProvinceData.IsPlains).ToList();
        }

        if (valid.Count <= dict.Count) // if there's less than 1 for each type then we randomly distribute what we have
        {
            List<Terrain> terrains = new List<Terrain> { Terrain.HIGHLAND, Terrain.MOUNTAINS, Terrain.FOREST, Terrain.CAVE, Terrain.WASTE };
            terrains.Shuffle();

            foreach (Terrain t in terrains)
            {
                if (!valid.Any())
                {
                    break;
                }

                Node n = valid.GetRandom();
                valid.Remove(n);

                n.ProvinceData.SetTerrainFlags(t);

                int rand = UnityEngine.Random.Range(0, 10);

                if (t == Terrain.CAVE && rand < 2)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.FOREST);
                }
                if (t == Terrain.WASTE)
                {
                    if (rand == 0)
                    {
                        n.ProvinceData.AddTerrainFlag(Terrain.WARMER);
                    }
                    else if (rand == 1)
                    {
                        n.ProvinceData.AddTerrainFlag(Terrain.COLDER);
                    }
                }
            }

            return;
        }

        List<Node> skipped = new List<Node>();

        foreach (KeyValuePair<Terrain, float> pair in dict)
        {
            int max = Mathf.RoundToInt(pair.Value * original.Count);
            
            if (max == 0)
            {
                continue;
            }

            int i = 0;

            if (pair.Key != Terrain.WASTE && skipped.Any())
            {
                valid.AddRange(skipped);
                skipped = new List<Node>();
            }

            while (i < max && valid.Any())
            {
                Node n = valid.GetRandom();
                valid.Remove(n);

                if ((n.GetConnectedProvincesOfType(pair.Key, true).Count > 1 && pair.Value < 0.3f) || (n.IsCapRing && pair.Key == Terrain.WASTE))
                {
                    skipped.Add(n);
                    continue;
                }

                n.ProvinceData.SetTerrainFlags(pair.Key);

                int rand = UnityEngine.Random.Range(0, 10);

                if (pair.Key == Terrain.CAVE && rand < 2)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.FOREST);
                }
                if (pair.Key == Terrain.WASTE)
                {
                    if (rand == 0)
                    {
                        n.ProvinceData.AddTerrainFlag(Terrain.WARMER);
                    }
                    else if (rand == 1)
                    {
                        n.ProvinceData.AddTerrainFlag(Terrain.COLDER);
                    }
                }

                i++;
            }
        }
    }

    static void generate_swamps(List<Node> original)
    {
        float num_swamps = GeneratorSettings.s_generator_settings.SwampFreq.GetRandom();
        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.ProvinceData.IsWater).ToList();
        }

        int swamps = Mathf.Max(Mathf.RoundToInt(num_swamps * original.Count), 1);
        int i = 0;

        while (i < swamps && valid.Any())
        {
            Node n = valid.GetRandom();
            valid.Remove(n);

            if (n.GetConnectedProvincesOfType(Terrain.SWAMP, true).Count > 1 && num_swamps < 0.3f)
            {
                continue;
            }

            n.ProvinceData.SetTerrainFlags(Terrain.SWAMP);

            i++;
        }
    }

    static void generate_lakes() // random placement logic for small bodies of water
    {
        if (!m_nations.Any(x => !x.NationData.IsWater))
        {
            return;
        }

        float num_lakes = GeneratorSettings.s_generator_settings.LakeFreq.GetRandom();
        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.HasNation && !x.ProvinceData.IsWater).ToList();
        }

        int lakes = Mathf.RoundToInt(num_lakes * valid.Count);
        List<Node> preferred = valid.Where(x => !x.ConnectedNodes.Any(y => y.ProvinceData.IsWater)).ToList();

        if (lakes == 0)
        {
            return;
        }

        valid.Shuffle();
        preferred.Shuffle();
        
        for (int i = 0; i < lakes; i++)
        {
            if (!preferred.Any())
            {
                if (valid.Any())
                {
                    preferred = valid.Where(x => !x.ProvinceData.IsWater).ToList();
                    valid = new List<Node>();
                }
                else
                {
                    break;
                }
            }

            Node n = preferred[0];
            preferred.Remove(n);

            n.ProvinceData.SetTerrainFlags(Terrain.SEA);
        }  
    }

    static void generate_caprings()
    {
        foreach (Node n in m_nodes)
        {
            if (n.HasNation) // capring logic
            {
                if (!m_nat_starts && !n.Nation.NationData.CapTerrain.HasFlag(Terrain.SEA))
                {
                    n.ProvinceData.SetTerrainFlags(Terrain.PLAINS);

                    continue;
                }
                
                n.Connections.Shuffle();
                int i = 0;

                foreach (Connection c in n.Connections)
                {
                    if (i >= n.Nation.NationData.TerrainData.Length)
                    {
                        break;
                    }

                    if (c.Node1 == n)
                    {
                        c.Node2.ProvinceData.SetTerrainFlags(n.Nation.NationData.TerrainData[i]);
                        c.Node2.SetAssignedTerrain(true);
                    }
                    else
                    {
                        c.Node1.ProvinceData.SetTerrainFlags(n.Nation.NationData.TerrainData[i]);
                        c.Node1.SetAssignedTerrain(true);
                    }

                    i++;
                }
            }
        }
    }

    static void generate_connections()
    {
        // connect the top right to bottom left all the time
        Node basenode = m_nodes.FirstOrDefault(n => n.X == 0 && n.Y == 0);
        Node corner = m_nodes.FirstOrDefault(n => n.X == m_layout.X - 1 && n.Y == m_layout.Y - 1);
        Connection con = new Connection(basenode, corner, ConnectionType.STANDARD, true);

        m_connections.Add(con);
        basenode.AddConnection(con);
        corner.AddConnection(con);

        // basic connections
        foreach (Node n in m_nodes)
        {
            Node up = get_node_with_wrap(n.X, n.Y + 1);
            Node right = get_node_with_wrap(n.X + 1, n.Y);
            Node down = get_node_with_wrap(n.X, n.Y - 1);
            Node left = get_node_with_wrap(n.X - 1, n.Y);

            if (!n.HasConnection(up))
            {
                Connection conn = new Connection(n, up, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                up.AddConnection(conn);
            }
            if (!n.HasConnection(right))
            {
                Connection conn = new Connection(n, right, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                right.AddConnection(conn);
            }
            if (!n.HasConnection(down))
            {
                Connection conn = new Connection(n, down, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                down.AddConnection(conn);
            }
            if (!n.HasConnection(left))
            {
                Connection conn = new Connection(n, left, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                left.AddConnection(conn);
            }
        }

        // capring connections - every player should have 5 provinces in their capring
        foreach (Node n in m_nodes.Where(x => x.HasNation))
        {
            List<Node> diag = new List<Node>();
            diag.Add(get_node_with_wrap(n.X + 1, n.Y + 1));
            diag.Add(get_node_with_wrap(n.X + 1, n.Y - 1));
            diag.Add(get_node_with_wrap(n.X - 1, n.Y - 1));
            diag.Add(get_node_with_wrap(n.X - 1, n.Y + 1));

            while (n.Connections.Count < 5) //(n.Connections.Count < n.Nation.CapRing)
            {
                Node next = diag.GetRandom();
                diag.Remove(next);

                Connection conn = new Connection(n, next, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                next.AddConnection(conn);
                n.AddConnection(conn);
            }
        }

        // random diagonal connections
        foreach (Node n in m_nodes)
        {
            if (n == corner)
            {
                continue;
            }

            Node up_right = get_node_with_wrap(n.X + 1, n.Y + 1);
            Node up = get_node_with_wrap(n.X, n.Y + 1);
            Node right = get_node_with_wrap(n.X + 1, n.Y);

            if (up_right.HasConnection(n) || up.HasConnection(right))
            {
                continue;
            }

            if (n.HasNation || up_right.HasNation)
            {
                Connection conn = new Connection(up, right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up.AddConnection(conn);
                right.AddConnection(conn);

                continue;
            }

            if (up.HasNation || right.HasNation)
            {
                Connection conn = new Connection(n, up_right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up_right.AddConnection(conn);
                n.AddConnection(conn);

                continue;
            }

            if (UnityEngine.Random.Range(0, 2) == 0 && (!n.HasNation && !up_right.HasNation))
            {
                Connection conn = new Connection(n, up_right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up_right.AddConnection(conn);
                n.AddConnection(conn);
            }
            else
            {
                Connection conn = new Connection(up, right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up.AddConnection(conn);
                right.AddConnection(conn);
            }
        }

        var caps = m_nodes.Where(x => x.HasNation);

        // designate caprings 
        foreach (Node n in caps)
        {
            foreach (Connection c in n.Connections)
            {
                if (c.Node1 != n)
                {
                    c.Node1.SetCapRing(true);
                }
                else
                {
                    c.Node2.SetCapRing(true);
                }
            }
        }

        // calculate adjacent connections
        foreach (Connection c in m_connections)
        {
            c.CalcAdjacent(m_connections, m_layout);
        }
    }

    static Node get_node_with_wrap(int x, int y)
    {
        if (x >= m_layout.X)
        {
            x = 0;
        }
        else if (x < 0)
        {
            x = m_layout.X - 1;
        }

        if (y >= m_layout.Y)
        {
            y = 0;
        }
        else if (y < 0)
        {
            y = m_layout.Y - 1;
        }

        Node n = m_nodes.FirstOrDefault(node => node.X == x && node.Y == y);
        return n;
    }

    static List<PlayerData> fix_team_numbers(List<PlayerData> data)
    {
        int i = 0;
        List<PlayerData> res = new List<PlayerData>();

        while (data.Any())
        {
            PlayerData p = data[0];
            var all_data = data.Where(x => x.TeamNum == p.TeamNum);

            foreach (PlayerData d in all_data)
            {
                res.Add(new PlayerData(d.NationData, i));
            }

            data.RemoveAll(x => x.TeamNum == p.TeamNum);
            i++;
        }

        return res;
    }

    static void generate_nodes()
    {
        m_nodes = new List<Node>();
        m_starts = new List<Node>();
        m_connections = new List<Connection>();

        List<PlayerData> scrambled = new List<PlayerData>();

        if (m_teamplay)
        {
            List<PlayerData> temp = new List<PlayerData>();
            temp.AddRange(m_nations);
            temp = fix_team_numbers(temp);
            temp.Shuffle();

            while (temp.Any())
            {
                PlayerData pd = temp[0];

                temp.Remove(pd);
                scrambled.Add(pd);

                while (temp.Any(x => x.TeamNum == pd.TeamNum))
                {
                    PlayerData next = temp.FirstOrDefault(x => x.TeamNum == pd.TeamNum);

                    if (next == null)
                    {
                        break;
                    }

                    temp.Remove(next);
                    scrambled.Add(next);
                }
            }
        }
        else
        {
            scrambled.AddRange(m_nations);
            scrambled.Shuffle();
        }

        if (m_teamplay)
        {
            create_team_nodes(m_layout, scrambled);
        }
        else
        {
            List<PlayerData> water = new List<PlayerData>();

            foreach (PlayerData d in scrambled)
            {
                if (d.NationData.WaterPercentage >= 0.5f) // Only water nations with a lot of water should be clustered together 
                {
                    water.Add(d);
                }
            }

            foreach (PlayerData d in water)
            {
                scrambled.Remove(d);
            }

            create_basic_nodes(m_layout, scrambled, water);
        }
    }

    static void create_team_nodes(NodeLayout nl, List<PlayerData> nats)
    {
        for (int x = 0; x < nl.X; x++) // create all nodes first
        {
            for (int y = 0; y < nl.Y; y++)
            {
                Node node = new Node(x, y, new ProvinceData());

                if ((x == 0 && y == 0) || (x == nl.X - 1 && y == nl.Y - 1))
                {
                    node.SetWrapCorner(true);
                }

                m_nodes.Add(node);
            }
        }

        List<SpawnPoint> spawns = new List<SpawnPoint>();
        spawns.AddRange(nl.Spawns.Where(x => x.SpawnType == SpawnType.PLAYER));
        spawns = spawns.OrderBy(x => x.Y).ThenBy(x => x.X).ToList();

        while (nats.Any())
        {
            PlayerData pd = nats[0];

            List<PlayerData> all = new List<PlayerData>();
            all.AddRange(nats.Where(x => x.TeamNum == pd.TeamNum));

            nats.RemoveAll(x => x.TeamNum == pd.TeamNum);

            SpawnPoint anchor = spawns[0];
            List<SpawnPoint> ordered = spawns.OrderBy(x => x.DistanceTo(anchor)).ToList();

            int i = 0;

            foreach (PlayerData p in all)
            {
                SpawnPoint s = ordered[i];
                spawns.Remove(s);

                Node n = m_nodes.FirstOrDefault(x => x.X == s.X && x.Y == s.Y);
                n.SetPlayerInfo(p, new ProvinceData(p.NationData.CapTerrain));
                n.SetAssignedTerrain(true);

                m_starts.Add(n);

                i++;
            } 
        }
    }

    static List<Node> get_closest_nodes(Node n)
    {
        Dictionary<Node, float> dict = new Dictionary<Node, float>();
        List<Node> nodes = m_nodes.Where(x => x.ProvinceData.Terrain.IsFlagSet(Terrain.START)).ToList();
        nodes.Shuffle();

        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            return nodes.OrderBy(x => n.DistanceTo(x)).ThenBy(x => Mathf.Abs(x.X - n.X)).ToList();
        }
        else
        {
            return nodes.OrderBy(x => n.DistanceTo(x)).ThenBy(x => Mathf.Abs(x.Y - n.Y)).ToList();
        }
    }

    static void create_basic_nodes(NodeLayout nl, List<PlayerData> nats, List<PlayerData> water)
    {
        for (int x = 0; x < nl.X; x++) // create all nodes first
        {
            for (int y = 0; y < nl.Y; y++)
            {
                Node node = new Node(x, y, new ProvinceData());

                if ((x == 0 && y == 0) || (x == nl.X - 1 && y == nl.Y - 1))
                {
                    node.SetWrapCorner(true);
                }

                m_nodes.Add(node);

                if (nl.HasSpawn(x, y, SpawnType.PLAYER))
                {
                    node.ProvinceData.AddTerrainFlag(Terrain.START);
                    m_starts.Add(node);
                }
            }
        }

        if (m_cluster_water) // put water nations close together if the user has this option ticked
        {
            List<Node> starts = m_nodes.Where(x => x.ProvinceData.Terrain.IsFlagSet(Terrain.START)).ToList();

            Node start = starts.GetRandom();
            List<Node> nodes = get_closest_nodes(start);

            foreach (PlayerData d in water)
            {
                Node n = nodes[0];
                nodes.Remove(n);
                starts.Remove(n);

                n.SetPlayerInfo(d, new ProvinceData(d.NationData.CapTerrain));
                n.SetAssignedTerrain(true);
            }

            foreach (PlayerData d in nats)
            {
                Node n = starts.GetRandom();
                starts.Remove(n);

                n.SetPlayerInfo(d, new ProvinceData(d.NationData.CapTerrain));
                n.SetAssignedTerrain(true);
            }
        }
        else
        {
            nats.AddRange(water);

            foreach (Node n in m_nodes.Where(x => x.ProvinceData.Terrain.IsFlagSet(Terrain.START)))
            {
                PlayerData d = nats.GetRandom();
                nats.Remove(d);

                n.SetPlayerInfo(d, new ProvinceData(d.NationData.CapTerrain));
                n.SetAssignedTerrain(true);
            }
        }
    }
}
