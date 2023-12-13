using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;

/// <summary>
/// This class handles the creation of all conceptual parts of the world.
/// This is where province types and connection types are determined.
/// </summary>
internal static class WorldGenerator
{
    private static bool m_nat_starts = true;
    private static bool m_teamplay = false;
    private static bool m_cluster_water = true;
    private static bool m_cluster_islands = false;
    private static NodeLayoutData m_layout;
    private static List<Node> m_nodes;
    private static List<Node> m_starts;
    private static List<Connection> m_connections;
    private static List<PlayerData> m_nations;

    public static void GenerateWorld(bool teamplay, bool cluster_water, bool cluster_islands, bool nat_starts, List<PlayerData> picks, NodeLayoutData layout)
    {
        m_nations = picks;
        m_nat_starts = nat_starts;
        m_teamplay = teamplay;
        m_cluster_water = cluster_water;
        m_cluster_islands = cluster_islands;
        m_layout = layout;

        generate_nodes();
        generate_connections();
        calculate_triangles();
        generate_caprings();
        generate_seas();
        generate_lakes();
        assign_water_terrain();

        var valid = m_nodes.Where(x => !x.HasNation && !x.IsAssignedTerrain && !x.ProvinceData.IsWater).ToList();
        var valid_conn = m_connections.Where(x => !x.IsCap && !x.IsInsideCapRing && !x.IsTouchingSea).ToList();

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
        generate_cave_entrances();
        generate_cave_terrain();
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

    public static NodeLayoutData GetLayout()
    {
        return m_layout;
    }

    private static void calculate_triangles()
    {
        foreach (var c in m_connections.Where(x => x.Diagonal))
        {
            var anchor = c.Adjacent[0];

            foreach (var c2 in c.Adjacent)
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

                    var others = c.Adjacent.Where(x => x != c && x != c2 && x != anchor).ToList();

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

    private static int unique_nodes(Connection m1, Connection m2, Connection m3)
    {
        var temp = new List<Node>();

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

    private static void cleanup_connections()
    {
        foreach (var n in m_nodes)
        {
            if (n.ProvinceData.IsWater) // make sure no water provinces have incorrect connections
            {
                foreach (var c in n.Connections)
                {
                    c.SetConnection(ConnectionType.STANDARD);
                }
            }
            else if (n.HasNation)
            {
                foreach (var c in n.Connections)
                {
                    if (c.ConnectionType != ConnectionType.STANDARD)
                    {
                        c.SetConnection(ConnectionType.STANDARD);
                    }
                }
            }
        }
    }

    private static void generate_roads(List<Connection> valid)
    {
        var starts = valid.Where(x => x.ConnectionType == ConnectionType.STANDARD).ToList();

        var max_roads = (int)(valid.Count * GeneratorSettings.s_generator_settings.RoadFreq.GetRandom());
        var count = 0;

        while (count < max_roads && starts.Any())
        {
            var con = starts.GetRandom();
            starts.Remove(con);

            con.SetConnection(ConnectionType.ROAD);
            count++;
        }
    }

    private static void generate_cliffs(List<Connection> valid)
    {
        var starts = valid.Where(x => x.ConnectionType == ConnectionType.STANDARD).ToList();

        var max_passes = (int)(valid.Count * GeneratorSettings.s_generator_settings.CliffPassFreq.GetRandom());
        var max_cliffs = (int)(valid.Count * GeneratorSettings.s_generator_settings.CliffFreq.GetRandom());
        var count = 0;

        var water = m_nodes.Where(x => x.ProvinceData.IsWater).ToList();
        var bad = new List<Connection>();

        if (water.Any())
        {
            foreach (var n in water)
            {
                foreach (var n1 in n.ConnectedNodes)
                {
                    foreach (var n2 in n.ConnectedNodes)
                    {
                        if (n1 == n2)
                        {
                            continue;
                        }

                        var c = n1.GetConnectionTo(n2);

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
            var con = starts.GetRandom();
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
            var con = starts.GetRandom();
            starts.Remove(con);

            if (con.Node1.NumStandardConnections < 3 || con.Node2.NumStandardConnections < 3 || con.ConnectionType != ConnectionType.STANDARD || con.IsCap || bad.Contains(con) ||
                    con.TriangleLinked.Any(x => x.ConnectionType == ConnectionType.RIVER || x.ConnectionType == ConnectionType.SHALLOWRIVER))
            {
                continue;
            }

            con.SetConnection(ConnectionType.MOUNTAINPASS);

            count++;
        }

        var num_flips = Mathf.Max(max_passes, max_cliffs);

        var mounts = m_connections.Where(x => x.ConnectionType == ConnectionType.MOUNTAIN).ToList();
        var passes = m_connections.Where(x => x.ConnectionType == ConnectionType.MOUNTAINPASS).ToList();

        for (var i = 0; i < num_flips; i++)
        {
            var c1 = mounts.GetRandom();
            var c2 = passes.GetRandom();

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

    private static Connection get_connection_weighted(List<Connection> conns)
    {
        if (conns.Count < 5)
        {
            return conns.GetRandom();
        }

        conns.Shuffle();
        conns = conns.OrderBy(x => x.NumSeaSwamp).ToList();

        var pos = UnityEngine.Random.Range(0, Mathf.RoundToInt(conns.Count * 0.5f));

        return conns[pos];
    }

    private static void generate_rivers(List<Connection> valid)
    {
        var water = m_nodes.Where(x => x.ProvinceData.IsWaterSwamp).ToList();

        if (!water.Any())
        {
            water = m_nodes;
        }

        var tertiary = new List<Connection>();
        var starts = new List<Connection>();

        foreach (var n in water)
        {
            foreach (var n1 in n.ConnectedNodes)
            {
                foreach (var n2 in n.ConnectedNodes)
                {
                    if (n1 == n2)
                    {
                        continue;
                    }

                    var c = n1.GetConnectionTo(n2);

                    if (c != null && valid.Contains(c) && !starts.Contains(c))
                    {
                        starts.Add(c);
                    }
                }
            }
        }

        var max_rivers = (int)(valid.Count * GeneratorSettings.s_generator_settings.DeepRiverFreq.GetRandom());
        var max_shallow = (int)(valid.Count * GeneratorSettings.s_generator_settings.RiverFreq.GetRandom());
        var count = 0;

        var cur = get_connection_weighted(starts);

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

        var num_flips = Mathf.Max(max_rivers, max_shallow);

        var rivers = m_connections.Where(x => x.ConnectionType == ConnectionType.RIVER).ToList();
        var shallows = m_connections.Where(x => x.ConnectionType == ConnectionType.SHALLOWRIVER).ToList();

        for (var i = 0; i < num_flips; i++)
        {
            var c1 = rivers.GetRandom();
            var c2 = shallows.GetRandom();

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

    private static void generate_seas()
    {
        foreach (var n in m_starts)
        {
            if (n.Nation.NationData.IsWater)
            {
                var modifier = n.ConnectedNodes.Count + 1; // subtract their capring and cap from the total count
                var count = 0;
                var num_water = Mathf.RoundToInt((m_layout.ProvsPerPlayer - modifier) * n.Nation.NationData.WaterPercentage);
                var total_iterations = 0;

                while (count < num_water)
                {
                    var iterations = UnityEngine.Random.Range(1, 3);

                    if (UnityEngine.Random.Range(0, 10) == 0)
                    {
                        iterations = UnityEngine.Random.Range(1, 4);
                    }

                    if (total_iterations > 100)
                    {
                        break;
                    }

                    var i = 0;
                    var cur = n.ConnectedNodes.GetRandom().ConnectedNodes.GetRandom();
                    total_iterations++;

                    while (i < iterations && count < num_water)
                    {
                        if (!cur.HasNation && !cur.IsCapRing && !cur.ProvinceData.IsWater && !cur.IsTouchingEnemyCapRing(n))
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

    private static void assign_water_terrain()
    {
        var num_forest = GeneratorSettings.s_generator_settings.ForestFreq.GetRandom() + GeneratorSettings.s_generator_settings.FarmFreq.GetRandom();
        var num_trench = GeneratorSettings.s_generator_settings.HighlandFreq.GetRandom();
        var num_deeps = GeneratorSettings.s_generator_settings.MountainFreq.GetRandom();
        var num_cave = GeneratorSettings.s_generator_settings.CaveFreq.GetRandom();

        var water = m_nodes.Where(x => x.ProvinceData.IsWater && !x.HasNation && !x.IsAssignedTerrain).ToList();

        var dict = new Dictionary<Terrain, float>();
        dict.Add(Terrain.HIGHLAND, num_trench);
        dict.Add(Terrain.FOREST, num_forest);
        dict.Add(Terrain.CAVE, num_cave);

        if (!m_nat_starts)
        {
            water = m_nodes.Where(x => x.ProvinceData.IsWater).ToList();
        }

        var ct = water.Count;

        foreach (var pair in dict)
        {
            var num = Mathf.RoundToInt(pair.Value * ct);

            if (pair.Value > 0.01f && num == 0)
            {
                num = 1;
            }

            if (num == 0 || !water.Any())
            {
                continue;
            }

            for (var i = 0; i < num; i++)
            {
                var n = water.GetRandom();
                water.Remove(n);

                n.ProvinceData.AddTerrainFlag(pair.Key);
            }
        }

        water = m_nodes.Where(x => x.ProvinceData.IsWater && !x.HasNation && !x.IsAssignedTerrain && !x.ProvinceData.Terrain.IsFlagSet(Terrain.FARM) && !x.ProvinceData.Terrain.IsFlagSet(Terrain.FOREST)).ToList();

        if (!m_nat_starts)
        {
            water = m_nodes.Where(x => x.ProvinceData.IsWater && !x.ProvinceData.Terrain.IsFlagSet(Terrain.FARM) && !x.ProvinceData.Terrain.IsFlagSet(Terrain.FOREST)).ToList();
        }

        var total_deep = Mathf.RoundToInt(num_deeps * ct);

        if (num_deeps > 0.01f && total_deep == 0)
        {
            total_deep = 1;
        }

        for (var i = 0; i < total_deep; i++)
        {
            if (!water.Any())
            {
                break;
            }

            var n = water.GetRandom();
            water.Remove(n);

            n.ProvinceData.AddTerrainFlag(Terrain.DEEPSEA);
        }
    }

    private static void generate_farms(List<Node> original)
    {
        var num_farms = GeneratorSettings.s_generator_settings.FarmFreq.GetRandom();

        var valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && x.ProvinceData.IsPlains).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => x.ProvinceData.IsPlains).ToList();
        }

        var max = Mathf.RoundToInt(num_farms * original.Count);
        var i = 0;

        while (i < max && valid.Any())
        {
            var n = valid.GetRandom();
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


    private static void generate_cave_walls()
    {
        var valid_cave_nodes = m_nodes.Where(x => !x.HasNation && !x.ProvinceData.HasCaveEntrance).ToList();
        var num_underworld_caves = Mathf.RoundToInt(GeneratorSettings.s_generator_settings.UnderworldCaveFreq * valid_cave_nodes.Count);
        var current_caves_count = valid_cave_nodes.Count;

        foreach (var node in valid_cave_nodes)
        {
            node.ProvinceData.SetIsCaveWall(false);
        }

        while (current_caves_count > num_underworld_caves && valid_cave_nodes.Any())
        {
            var node = valid_cave_nodes.GetRandom();
            node.ProvinceData.SetIsCaveWall(true);

            valid_cave_nodes.Remove(node);

            current_caves_count--;
        }
    }

    private static void generate_cave_terrain()
    {
        var cave_entrance_nodes = m_nodes.Where(x => x.ProvinceData.HasCaveEntrance).ToList();
        var cap_nodes = m_nodes.Where(x => x.HasNation).ToList();

        foreach (var node in cap_nodes)
        {
            node.ProvinceData.SetIsCaveWall(true);
        }

        generate_cave_walls();

        int attempt_count = 0;
        while (!cave_entrances_are_valid(cave_entrance_nodes) && attempt_count < 20)
        {
            generate_cave_walls();
            attempt_count++;
        }

        if (attempt_count == 20)
        {
            Debug.LogWarning("Generating valid caves took too long, settling with imperfect results");
        }

        var non_cave_wall_nodes = m_nodes.Where(x => !x.ProvinceData.IsCaveWall).ToList();
        non_cave_wall_nodes.Shuffle();

        var num_forests = Mathf.RoundToInt(GeneratorSettings.s_generator_settings.ForestFreq.GetRandom() * non_cave_wall_nodes.Count);
        var num_swamps = Mathf.RoundToInt(GeneratorSettings.s_generator_settings.SwampFreq.GetRandom() * non_cave_wall_nodes.Count);
        var num_highlands = Mathf.RoundToInt(GeneratorSettings.s_generator_settings.HighlandFreq.GetRandom() * non_cave_wall_nodes.Count);
        var count = 0;

        while (count < num_forests)
        {
            var node = non_cave_wall_nodes[0];
            non_cave_wall_nodes.RemoveAt(0);

            node.ProvinceData.CaveTerrain.SetFlags(Terrain.FOREST, true);

            count++;
        }

        count = 0;

        while (count < num_swamps)
        {
            var node = non_cave_wall_nodes[0];
            non_cave_wall_nodes.RemoveAt(0);

            node.ProvinceData.CaveTerrain.SetFlags(Terrain.SWAMP, true);

            count++;
        }

        count = 0;

        while (count < num_highlands)
        {
            var node = non_cave_wall_nodes[0];
            non_cave_wall_nodes.RemoveAt(0);

            node.ProvinceData.CaveTerrain.SetFlags(Terrain.HIGHLAND, true);

            count++;
        }
    }

    private static bool cave_entrances_are_valid(List<Node> cave_entrance_nodes)
    {
        var skip_nodes = new List<Node>();
        foreach (var cave_node in cave_entrance_nodes)
        {
            if (skip_nodes.Contains(cave_node))
            {
                continue;
            }

            var node_list = new List<Node>() { cave_node };
            var nodes_to_process = cave_node.ConnectedNodes.Where(node => !node.HasNation && !node.ProvinceData.IsCaveWall);

            while (nodes_to_process.Any())
            {
                var node = nodes_to_process[0];
                node_list.Add(node);
                nodes_to_process.RemoveAt(0);
                nodes_to_process.AddRange(node.ConnectedNodes.Where(connected_node => !node_list.Contains(connected_node) && !connected_node.HasNation && !connected_node.ProvinceData.IsCaveWall));
            }

            if (node_list.Count(node => node.ProvinceData.HasCaveEntrance) < 2)
            {
                return false;
            }

            skip_nodes.AddRange(node_list.Where(node => node.ProvinceData.HasCaveEntrance));
        }

        return true;
    }

    private static void generate_cave_entrances()
    {
        var valid = m_nodes.Where(x => !x.HasNation && !x.IsAssignedTerrain && !x.IsCapRing).ToList();
        var max = Mathf.Max(1, GeneratorSettings.s_generator_settings.NumCaveEntrancesPerPlayer);
        var dict = new Dictionary<Node, List<Node>>();

        foreach (var n in m_starts)
        {
            var nodes = new List<Node>();

            foreach (var conn in n.ConnectedNodes)
            {
                foreach (var t in conn.ConnectedNodes.Where(x => !x.IsCapRing && !x.HasNation))
                {
                    nodes.Add(t);
                }
            }

            nodes.Shuffle();
            dict.Add(n, nodes);
        }

        foreach (var pair in dict)
        {
            int count = 0;

            foreach (var node in pair.Value)
            {
                if (!node.ProvinceData.HasCaveEntrance)
                {
                    count++;
                    node.ProvinceData.SetHasCaveEntrance(true);
                }

                if (count >= max)
                {
                    break;
                }
            }
        }
    }

    private static void generate_thrones()
    {
        var num_water = m_starts.Where(x => x.Nation.NationData.WaterPercentage > 0.3f).Count();
        var water_ct = UnityEngine.Random.Range(-2, 1);

        foreach (var n in m_nodes)
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

    private static void generate_sized(List<Node> original)
    {
        var num_large = GeneratorSettings.s_generator_settings.LargeFreq.GetRandom();
        var num_small = GeneratorSettings.s_generator_settings.SmallFreq.GetRandom();

        var valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.HasNation).ToList();
        }

        var dict = new Dictionary<Node, List<Node>>();

        foreach (var n in m_starts)
        {
            var nodes = new List<Node>();

            foreach (var conn in n.ConnectedNodes)
            {
                foreach (var t in conn.ConnectedNodes.Where(x => !x.IsCapRing && !x.HasNation))
                {
                    nodes.Add(t);
                }
            }

            dict.Add(n, nodes);
        }

        var max = Mathf.RoundToInt((num_large * original.Count) / m_starts.Count);
        var i = 0;

        while (i < max) // fairly assign large provinces to each nation 
        {
            foreach (var pair in dict)
            {
                if (pair.Value.Any())
                {
                    var n = pair.Value.GetRandom();
                    pair.Value.Remove(n);

                    n.ProvinceData.AddTerrainFlag(Terrain.LARGEPROV);
                }
                else
                {
                    if (valid.Any())
                    {
                        var alt = valid.GetRandom();
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
            var n = valid.GetRandom();
            valid.Remove(n);

            if (n.GetConnectedProvincesOfType(Terrain.SMALLPROV, true).Count > 1 && num_small < 0.3f)
            {
                continue;
            }

            n.ProvinceData.AddTerrainFlag(Terrain.SMALLPROV);

            i++;
        }
    }

    private static void generate_misc(List<Node> original)
    {
        var num_highlands = GeneratorSettings.s_generator_settings.HighlandFreq.GetRandom();
        var num_mountains = GeneratorSettings.s_generator_settings.MountainFreq.GetRandom();
        var num_forests = GeneratorSettings.s_generator_settings.ForestFreq.GetRandom();
        var num_caves = GeneratorSettings.s_generator_settings.CaveFreq.GetRandom();
        var num_waste = GeneratorSettings.s_generator_settings.WasteFreq.GetRandom();

        if (GeneratorSettings.s_generator_settings.UseClassicMountains)
        {
            num_mountains = 0f;
        }

        var dict = new Dictionary<Terrain, float>();
        dict.Add(Terrain.WASTE, num_waste);
        dict.Add(Terrain.HIGHLAND, num_highlands);
        dict.Add(Terrain.MOUNTAINS, num_mountains);
        dict.Add(Terrain.FOREST, num_forests);
        dict.Add(Terrain.CAVE, num_caves);

        var valid = m_nodes.Where(x => !x.HasNation && !x.IsAssignedTerrain && x.ProvinceData.IsPlains).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => x.ProvinceData.IsPlains).ToList();
        }

        if (valid.Count <= dict.Count) // if there's less than 1 for each type then we randomly distribute what we have
        {
            var terrains = new List<Terrain> { Terrain.HIGHLAND, Terrain.MOUNTAINS, Terrain.FOREST, Terrain.CAVE, Terrain.WASTE };
            terrains.Shuffle();

            foreach (var t in terrains)
            {
                if (!valid.Any())
                {
                    break;
                }

                var n = valid.GetRandom();
                valid.Remove(n);

                n.ProvinceData.SetTerrainFlags(t);

                var rand = UnityEngine.Random.Range(0, 10);

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

        var skipped = new List<Node>();

        foreach (var pair in dict)
        {
            var max = Mathf.RoundToInt(pair.Value * original.Count);

            if (max == 0)
            {
                continue;
            }

            var i = 0;

            if (pair.Key != Terrain.WASTE && skipped.Any())
            {
                valid.AddRange(skipped);
                skipped = new List<Node>();
            }

            while (i < max && valid.Any())
            {
                var n = valid.GetRandom();
                valid.Remove(n);

                if ((n.GetConnectedProvincesOfType(pair.Key, true).Count > 1 && pair.Value < 0.3f) || (n.IsCapRing && pair.Key == Terrain.WASTE))
                {
                    skipped.Add(n);
                    continue;
                }

                n.ProvinceData.SetTerrainFlags(pair.Key);

                var rand = UnityEngine.Random.Range(0, 10);

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

    private static void generate_swamps(List<Node> original)
    {
        var num_swamps = GeneratorSettings.s_generator_settings.SwampFreq.GetRandom();
        var valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.ProvinceData.IsWater).ToList();
        }

        var swamps = Mathf.Max(Mathf.RoundToInt(num_swamps * original.Count), 1);
        var i = 0;

        while (i < swamps && valid.Any())
        {
            var n = valid.GetRandom();
            valid.Remove(n);

            if (n.GetConnectedProvincesOfType(Terrain.SWAMP, true).Count > 1 && num_swamps < 0.3f)
            {
                continue;
            }

            n.ProvinceData.SetTerrainFlags(Terrain.SWAMP);

            i++;
        }
    }

    private static void generate_lakes() // random placement logic for small bodies of water
    {
        if (!m_nations.Any(x => !x.NationData.IsWater))
        {
            return;
        }

        var num_lakes = GeneratorSettings.s_generator_settings.LakeFreq.GetRandom();
        var valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.HasNation && !x.ProvinceData.IsWater).ToList();
        }

        var lakes = Mathf.RoundToInt(num_lakes * valid.Count);
        var preferred = valid.Where(x => !x.ConnectedNodes.Any(y => y.ProvinceData.IsWater)).ToList();

        if (lakes == 0)
        {
            return;
        }

        valid.Shuffle();
        preferred.Shuffle();

        for (var i = 0; i < lakes; i++)
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

            var n = preferred[0];
            preferred.Remove(n);

            n.ProvinceData.SetTerrainFlags(Terrain.SEA);
        }
    }

    private static void generate_caprings()
    {
        foreach (var n in m_nodes)
        {
            if (n.HasNation) // capring logic
            {
                bool did_place_cave_gate = false;

                if (!m_nat_starts && !n.Nation.NationData.CapTerrain.HasFlag(Terrain.SEA))
                {
                    n.ProvinceData.SetTerrainFlags(Terrain.PLAINS);

                    continue;
                }

                n.Connections.Shuffle();
                var i = 0;

                foreach (var c in n.Connections)
                {
                    if (i >= n.Nation.NationData.TerrainData.Length)
                    {
                        break;
                    }

                    if (c.Node1 == n)
                    {
                        if (!did_place_cave_gate && n.Nation.NationData.HasCaveEntranceInCapRing)
                        {
                            did_place_cave_gate = true;
                            c.Node2.ProvinceData.SetHasCaveEntrance(true);
                        }

                        c.Node2.ProvinceData.SetTerrainFlags(n.Nation.NationData.TerrainData[i]);
                        c.Node2.SetAssignedTerrain(true);
                    }
                    else
                    {
                        if (!did_place_cave_gate && n.Nation.NationData.HasCaveEntranceInCapRing)
                        {
                            did_place_cave_gate = true;
                            c.Node1.ProvinceData.SetHasCaveEntrance(true);
                        }

                        c.Node1.ProvinceData.SetTerrainFlags(n.Nation.NationData.TerrainData[i]);
                        c.Node1.SetAssignedTerrain(true);
                    }

                    i++;
                }
            }
        }
    }

    private static void generate_connections()
    {
        // connect the top right to bottom left all the time
        var basenode = m_nodes.FirstOrDefault(n => n.X == 0 && n.Y == 0);
        var corner = m_nodes.FirstOrDefault(n => n.X == m_layout.X - 1 && n.Y == m_layout.Y - 1);
        var con = new Connection(basenode, corner, ConnectionType.STANDARD, true);

        m_connections.Add(con);
        basenode.AddConnection(con);
        corner.AddConnection(con);

        // basic connections
        foreach (var n in m_nodes)
        {
            var up = get_node_with_wrap(n.X, n.Y + 1);
            var right = get_node_with_wrap(n.X + 1, n.Y);
            var down = get_node_with_wrap(n.X, n.Y - 1);
            var left = get_node_with_wrap(n.X - 1, n.Y);

            if (!n.HasConnection(up))
            {
                var conn = new Connection(n, up, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                up.AddConnection(conn);
            }
            if (!n.HasConnection(right))
            {
                var conn = new Connection(n, right, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                right.AddConnection(conn);
            }
            if (!n.HasConnection(down))
            {
                var conn = new Connection(n, down, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                down.AddConnection(conn);
            }
            if (!n.HasConnection(left))
            {
                var conn = new Connection(n, left, ConnectionType.STANDARD);

                m_connections.Add(conn);
                n.AddConnection(conn);
                left.AddConnection(conn);
            }
        }

        // capring connections - every player is assigned the proper amount of capring provinces
        foreach (var n in m_nodes.Where(x => x.HasNation))
        {
            var diag = new List<Node>
            {
                get_node_with_wrap(n.X + 1, n.Y + 1),
                get_node_with_wrap(n.X + 1, n.Y - 1),
                get_node_with_wrap(n.X - 1, n.Y - 1),
                get_node_with_wrap(n.X - 1, n.Y + 1)
            };

            while (n.Connections.Count < n.Nation.NationData.CapRingSize)
            {
                var next = diag.GetRandom();
                diag.Remove(next);

                var conn = new Connection(n, next, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                next.AddConnection(conn);
                n.AddConnection(conn);
            }
        }

        // random diagonal connections
        foreach (var n in m_nodes)
        {
            if (n == corner)
            {
                continue;
            }

            var up_right = get_node_with_wrap(n.X + 1, n.Y + 1);
            var up = get_node_with_wrap(n.X, n.Y + 1);
            var right = get_node_with_wrap(n.X + 1, n.Y);

            if (up_right.HasConnection(n) || up.HasConnection(right))
            {
                continue;
            }

            if (n.HasNation || up_right.HasNation)
            {
                var conn = new Connection(up, right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up.AddConnection(conn);
                right.AddConnection(conn);

                continue;
            }

            if (up.HasNation || right.HasNation)
            {
                var conn = new Connection(n, up_right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up_right.AddConnection(conn);
                n.AddConnection(conn);

                continue;
            }

            if (UnityEngine.Random.Range(0, 2) == 0 && (!n.HasNation && !up_right.HasNation))
            {
                var conn = new Connection(n, up_right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up_right.AddConnection(conn);
                n.AddConnection(conn);
            }
            else
            {
                var conn = new Connection(up, right, ConnectionType.STANDARD, true);

                m_connections.Add(conn);
                up.AddConnection(conn);
                right.AddConnection(conn);
            }
        }

        var caps = m_nodes.Where(x => x.HasNation);

        // designate caprings 
        foreach (var n in caps)
        {
            foreach (var c in n.Connections)
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
        foreach (var c in m_connections)
        {
            c.CalcAdjacent(m_connections, m_layout);
        }
    }

    private static Node get_node_with_wrap(int x, int y)
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

        var n = m_nodes.FirstOrDefault(node => node.X == x && node.Y == y);
        return n;
    }

    private static List<PlayerData> fix_team_numbers(List<PlayerData> data)
    {
        var i = 0;
        var res = new List<PlayerData>();

        while (data.Any())
        {
            var p = data[0];
            var all_data = data.Where(x => x.TeamNum == p.TeamNum);

            foreach (var d in all_data)
            {
                res.Add(new PlayerData(d.NationData, i));
            }

            data.RemoveAll(x => x.TeamNum == p.TeamNum);
            i++;
        }

        return res;
    }

    private static void generate_nodes()
    {
        m_nodes = new List<Node>();
        m_starts = new List<Node>();
        m_connections = new List<Connection>();

        var scrambled = new List<PlayerData>();

        if (m_teamplay)
        {
            var temp = new List<PlayerData>();
            temp.AddRange(m_nations);
            temp = fix_team_numbers(temp);
            temp.Shuffle();

            while (temp.Any())
            {
                var pd = temp[0];

                temp.Remove(pd);
                scrambled.Add(pd);

                while (temp.Any(x => x.TeamNum == pd.TeamNum))
                {
                    var next = temp.FirstOrDefault(x => x.TeamNum == pd.TeamNum);

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
            var water = new List<PlayerData>();

            foreach (var player in scrambled)
            {
                if (player.NationData.WaterPercentage >= 0.3f || (player.NationData.IsIsland && m_cluster_islands)) 
                {
                    water.Add(player);
                }
            }

            foreach (var player in water)
            {
                scrambled.Remove(player);
            }

            create_basic_nodes(m_layout, scrambled, water);
        }
    }

    private static void create_team_nodes(NodeLayoutData nl, List<PlayerData> nats)
    {
        for (var x = 0; x < nl.X; x++) // create all nodes first
        {
            for (var y = 0; y < nl.Y; y++)
            {
                var node = new Node(x, y, new ProvinceData());

                if ((x == 0 && y == 0) || (x == nl.X - 1 && y == nl.Y - 1))
                {
                    node.SetWrapCorner(true);
                }

                m_nodes.Add(node);
            }
        }

        var spawns = new List<SpawnPoint>();
        spawns.AddRange(nl.Spawns.Where(x => x.SpawnType == SpawnType.PLAYER));
        spawns = spawns.OrderBy(x => x.Y).ThenBy(x => x.X).ToList();

        while (nats.Any())
        {
            var pd = nats[0];

            var all = new List<PlayerData>();
            all.AddRange(nats.Where(x => x.TeamNum == pd.TeamNum));

            nats.RemoveAll(x => x.TeamNum == pd.TeamNum);

            var anchor = spawns[0];
            var ordered = spawns.OrderBy(x => x.DistanceTo(anchor)).ToList();

            var i = 0;

            foreach (var p in all)
            {
                var s = ordered[i];
                spawns.Remove(s);

                var n = m_nodes.FirstOrDefault(x => x.X == s.X && x.Y == s.Y);
                n.SetPlayerInfo(p, new ProvinceData(p.NationData.CapTerrain));
                n.SetAssignedTerrain(true);

                m_starts.Add(n);

                i++;
            }
        }
    }

    private static List<Node> get_closest_nodes(Node n)
    {
        var dict = new Dictionary<Node, float>();
        var nodes = m_nodes.Where(x => x.ProvinceData.Terrain.IsFlagSet(Terrain.START)).ToList();
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

    private static void create_basic_nodes(NodeLayoutData nl, List<PlayerData> nats, List<PlayerData> water)
    {
        for (var x = 0; x < nl.X; x++) // create all nodes first
        {
            for (var y = 0; y < nl.Y; y++)
            {
                var node = new Node(x, y, new ProvinceData());

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
            var starts = m_nodes.Where(x => x.ProvinceData.Terrain.IsFlagSet(Terrain.START)).ToList();

            var start = starts.GetRandom();
            var nodes = get_closest_nodes(start);

            foreach (var d in water)
            {
                var n = nodes[0];
                nodes.Remove(n);
                starts.Remove(n);

                n.SetPlayerInfo(d, new ProvinceData(d.NationData.CapTerrain));
                n.SetAssignedTerrain(true);
            }

            foreach (var d in nats)
            {
                var n = starts.GetRandom();
                starts.Remove(n);

                n.SetPlayerInfo(d, new ProvinceData(d.NationData.CapTerrain));
                n.SetAssignedTerrain(true);
            }
        }
        else
        {
            nats.AddRange(water);

            foreach (var n in m_nodes.Where(x => x.ProvinceData.Terrain.IsFlagSet(Terrain.START)))
            {
                var d = nats.GetRandom();
                nats.Remove(d);

                n.SetPlayerInfo(d, new ProvinceData(d.NationData.CapTerrain));
                n.SetAssignedTerrain(true);
            }
        }
    }
}
