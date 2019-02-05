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
    const int PROVS_PER_PLAYER = 16;

    static int m_num_players;
    static bool m_nat_starts = false;
    static bool m_teamplay = false;
    static NodeLayout m_layout;
    static List<Node> m_nodes;
    static List<Node> m_starts;
    static List<Connection> m_connections;
    static List<NodeLayout> m_layouts;
    static List<NationData> m_nations;

    public static void GenerateWorld(bool teamplay, List<NationData> picks, bool nat_starts)
    {
        init();

        m_num_players = picks.Count;
        m_nations = picks;
        m_nat_starts = nat_starts;
        m_teamplay = teamplay;

        generate_nodes();
        generate_connections();
        generate_caprings();
        generate_seas();
        generate_lakes_swamps();
        generate_rivers();
        generate_cliffs();
        generate_roads();
        generate_misc();
        generate_farms();
        generate_sized();
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

    static void init()
    {
        m_layouts = new List<NodeLayout>(); // create all the layouts for all allowed player counts. ugly but functional.

        // A layout is organized as a grid.
        // This 2 player layout is 6x6 nodes.
        // Spawn point X and Y values start at 0, it helps to draw the grid on a piece of paper to visualize it.
        // Example: The bottom left position on a 6x6 grid is [0,0]. The top left position on the grid is [5,5].
        NodeLayout n2 = new NodeLayout(2, 2, 6, 6, 18);
        n2.AddSpawn(new SpawnPoint(1, 1, SpawnType.PLAYER));
        n2.AddSpawn(new SpawnPoint(4, 4, SpawnType.PLAYER));
        n2.AddSpawn(new SpawnPoint(1, 4, SpawnType.THRONE));
        n2.AddSpawn(new SpawnPoint(4, 1, SpawnType.THRONE));

        NodeLayout n3 = new NodeLayout(3, 3, 8, 8, 21);
        n3.AddSpawn(new SpawnPoint(1, 3, SpawnType.PLAYER));
        n3.AddSpawn(new SpawnPoint(5, 1, SpawnType.PLAYER));
        n3.AddSpawn(new SpawnPoint(5, 5, SpawnType.PLAYER));
        n3.AddSpawn(new SpawnPoint(3, 1, SpawnType.THRONE));
        n3.AddSpawn(new SpawnPoint(3, 5, SpawnType.THRONE));
        n3.AddSpawn(new SpawnPoint(5, 3, SpawnType.THRONE));

        NodeLayout n4 = new NodeLayout(4, 4, 8, 8, 16);
        n4.AddSpawn(new SpawnPoint(1, 1, SpawnType.PLAYER));
        n4.AddSpawn(new SpawnPoint(1, 5, SpawnType.PLAYER));
        n4.AddSpawn(new SpawnPoint(5, 1, SpawnType.PLAYER));
        n4.AddSpawn(new SpawnPoint(5, 5, SpawnType.PLAYER));
        n4.AddSpawn(new SpawnPoint(3, 3, SpawnType.THRONE));
        n4.AddSpawn(new SpawnPoint(3, 7, SpawnType.THRONE));
        n4.AddSpawn(new SpawnPoint(7, 3, SpawnType.THRONE));
        n4.AddSpawn(new SpawnPoint(7, 7, SpawnType.THRONE));

        NodeLayout n5 = new NodeLayout(5, 6, 9, 9, 16);
        n5.AddSpawn(new SpawnPoint(1, 1, SpawnType.PLAYER));
        n5.AddSpawn(new SpawnPoint(1, 6, SpawnType.PLAYER));
        n5.AddSpawn(new SpawnPoint(4, 4, SpawnType.PLAYER));
        n5.AddSpawn(new SpawnPoint(7, 2, SpawnType.PLAYER));
        n5.AddSpawn(new SpawnPoint(7, 7, SpawnType.PLAYER));
        n5.AddSpawn(new SpawnPoint(0, 3, SpawnType.THRONE));
        n5.AddSpawn(new SpawnPoint(2, 3, SpawnType.THRONE));
        n5.AddSpawn(new SpawnPoint(3, 7, SpawnType.THRONE));
        n5.AddSpawn(new SpawnPoint(5, 1, SpawnType.THRONE));
        n5.AddSpawn(new SpawnPoint(6, 5, SpawnType.THRONE));
        n5.AddSpawn(new SpawnPoint(8, 5, SpawnType.THRONE));

        NodeLayout n6 = new NodeLayout(6, 6, 12, 8, 16);
        n6.AddSpawn(new SpawnPoint(1, 1, SpawnType.PLAYER));
        n6.AddSpawn(new SpawnPoint(1, 5, SpawnType.PLAYER));
        n6.AddSpawn(new SpawnPoint(5, 1, SpawnType.PLAYER));
        n6.AddSpawn(new SpawnPoint(5, 5, SpawnType.PLAYER));
        n6.AddSpawn(new SpawnPoint(9, 1, SpawnType.PLAYER));
        n6.AddSpawn(new SpawnPoint(9, 5, SpawnType.PLAYER));
        n6.AddSpawn(new SpawnPoint(3, 3, SpawnType.THRONE));
        n6.AddSpawn(new SpawnPoint(3, 7, SpawnType.THRONE));
        n6.AddSpawn(new SpawnPoint(7, 3, SpawnType.THRONE));
        n6.AddSpawn(new SpawnPoint(7, 7, SpawnType.THRONE));
        n6.AddSpawn(new SpawnPoint(11, 3, SpawnType.THRONE));
        n6.AddSpawn(new SpawnPoint(11, 7, SpawnType.THRONE));

        NodeLayout n7 = new NodeLayout(7, 7, 12, 12, 20);
        n7.AddSpawn(new SpawnPoint(2, 1, SpawnType.PLAYER));
        n7.AddSpawn(new SpawnPoint(6, 1, SpawnType.PLAYER));
        n7.AddSpawn(new SpawnPoint(10, 3, SpawnType.PLAYER));
        n7.AddSpawn(new SpawnPoint(6, 5, SpawnType.PLAYER));
        n7.AddSpawn(new SpawnPoint(2, 7, SpawnType.PLAYER));
        n7.AddSpawn(new SpawnPoint(6, 9, SpawnType.PLAYER));
        n7.AddSpawn(new SpawnPoint(10, 9, SpawnType.PLAYER));
        n7.AddSpawn(new SpawnPoint(0, 2, SpawnType.THRONE));
        n7.AddSpawn(new SpawnPoint(0, 8, SpawnType.THRONE));
        n7.AddSpawn(new SpawnPoint(4, 1, SpawnType.THRONE));
        n7.AddSpawn(new SpawnPoint(4, 6, SpawnType.THRONE));
        n7.AddSpawn(new SpawnPoint(8, 4, SpawnType.THRONE));
        n7.AddSpawn(new SpawnPoint(6, 11, SpawnType.THRONE));
        n7.AddSpawn(new SpawnPoint(8, 9, SpawnType.THRONE));

        NodeLayout n8 = new NodeLayout(8, 8, 16, 8, 16);
        n8.AddSpawn(new SpawnPoint(1, 1, SpawnType.PLAYER));
        n8.AddSpawn(new SpawnPoint(1, 5, SpawnType.PLAYER));
        n8.AddSpawn(new SpawnPoint(5, 1, SpawnType.PLAYER));
        n8.AddSpawn(new SpawnPoint(5, 5, SpawnType.PLAYER));
        n8.AddSpawn(new SpawnPoint(9, 1, SpawnType.PLAYER));
        n8.AddSpawn(new SpawnPoint(9, 5, SpawnType.PLAYER));
        n8.AddSpawn(new SpawnPoint(13, 1, SpawnType.PLAYER));
        n8.AddSpawn(new SpawnPoint(13, 5, SpawnType.PLAYER));
        n8.AddSpawn(new SpawnPoint(3, 3, SpawnType.THRONE));
        n8.AddSpawn(new SpawnPoint(3, 7, SpawnType.THRONE));
        n8.AddSpawn(new SpawnPoint(7, 3, SpawnType.THRONE));
        n8.AddSpawn(new SpawnPoint(7, 7, SpawnType.THRONE));
        n8.AddSpawn(new SpawnPoint(11, 3, SpawnType.THRONE));
        n8.AddSpawn(new SpawnPoint(11, 7, SpawnType.THRONE));
        n8.AddSpawn(new SpawnPoint(15, 3, SpawnType.THRONE));
        n8.AddSpawn(new SpawnPoint(15, 7, SpawnType.THRONE));

        NodeLayout n9 = new NodeLayout(9, 9, 12, 12, 16);
        n9.AddSpawn(new SpawnPoint(1, 1, SpawnType.PLAYER));
        n9.AddSpawn(new SpawnPoint(1, 5, SpawnType.PLAYER));
        n9.AddSpawn(new SpawnPoint(1, 9, SpawnType.PLAYER));
        n9.AddSpawn(new SpawnPoint(5, 1, SpawnType.PLAYER));
        n9.AddSpawn(new SpawnPoint(5, 5, SpawnType.PLAYER));
        n9.AddSpawn(new SpawnPoint(5, 9, SpawnType.PLAYER));
        n9.AddSpawn(new SpawnPoint(9, 1, SpawnType.PLAYER));
        n9.AddSpawn(new SpawnPoint(9, 5, SpawnType.PLAYER));
        n9.AddSpawn(new SpawnPoint(9, 9, SpawnType.PLAYER));
        n9.AddSpawn(new SpawnPoint(3, 3, SpawnType.THRONE));
        n9.AddSpawn(new SpawnPoint(3, 7, SpawnType.THRONE));
        n9.AddSpawn(new SpawnPoint(3, 11, SpawnType.THRONE));
        n9.AddSpawn(new SpawnPoint(7, 3, SpawnType.THRONE));
        n9.AddSpawn(new SpawnPoint(7, 7, SpawnType.THRONE));
        n9.AddSpawn(new SpawnPoint(7, 11, SpawnType.THRONE));
        n9.AddSpawn(new SpawnPoint(11, 3, SpawnType.THRONE));
        n9.AddSpawn(new SpawnPoint(11, 7, SpawnType.THRONE));
        n9.AddSpawn(new SpawnPoint(11, 11, SpawnType.THRONE));

        NodeLayout n10 = new NodeLayout(10, 10, 15, 15, 22);
        n10.AddSpawn(new SpawnPoint(2, 2, SpawnType.PLAYER));
        n10.AddSpawn(new SpawnPoint(6, 2, SpawnType.PLAYER));
        n10.AddSpawn(new SpawnPoint(11, 1, SpawnType.PLAYER));
        n10.AddSpawn(new SpawnPoint(13, 5, SpawnType.PLAYER));
        n10.AddSpawn(new SpawnPoint(9, 6, SpawnType.PLAYER));
        n10.AddSpawn(new SpawnPoint(5, 8, SpawnType.PLAYER));
        n10.AddSpawn(new SpawnPoint(1, 9, SpawnType.PLAYER));
        n10.AddSpawn(new SpawnPoint(3, 13, SpawnType.PLAYER));
        n10.AddSpawn(new SpawnPoint(8, 12, SpawnType.PLAYER));
        n10.AddSpawn(new SpawnPoint(12, 12, SpawnType.PLAYER));
        n10.AddSpawn(new SpawnPoint(6, 0, SpawnType.THRONE));
        n10.AddSpawn(new SpawnPoint(13, 0, SpawnType.THRONE));
        n10.AddSpawn(new SpawnPoint(1, 4, SpawnType.THRONE));
        n10.AddSpawn(new SpawnPoint(6, 6, SpawnType.THRONE));
        n10.AddSpawn(new SpawnPoint(8, 8, SpawnType.THRONE));
        n10.AddSpawn(new SpawnPoint(13, 7, SpawnType.THRONE));
        n10.AddSpawn(new SpawnPoint(13, 10, SpawnType.THRONE));
        n10.AddSpawn(new SpawnPoint(1, 7, SpawnType.THRONE));
        n10.AddSpawn(new SpawnPoint(1, 14, SpawnType.THRONE));
        n10.AddSpawn(new SpawnPoint(8, 14, SpawnType.THRONE));

        NodeLayout n11 = new NodeLayout(11, 12, 16, 16, 23);
        n11.AddSpawn(new SpawnPoint(4, 1, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(5, 2, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(9, 1, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(8, 7, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(12, 5, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(14, 1, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(4, 9, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(2, 13, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(7, 13, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(12, 13, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(14, 9, SpawnType.PLAYER));
        n11.AddSpawn(new SpawnPoint(2, 1, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(7, 1, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(6, 4, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(10, 4, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(15, 3, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(4, 6, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(1, 11, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(6, 10, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(12, 8, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(10, 10, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(9, 13, SpawnType.THRONE));
        n11.AddSpawn(new SpawnPoint(14, 13, SpawnType.THRONE));

        NodeLayout n12 = new NodeLayout(12, 12, 16, 12, 16);
        n12.AddSpawn(new SpawnPoint(1, 1, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(1, 5, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(1, 9, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(5, 1, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(5, 5, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(5, 9, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(9, 1, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(9, 5, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(9, 9, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(13, 1, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(13, 5, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(13, 9, SpawnType.PLAYER));
        n12.AddSpawn(new SpawnPoint(3, 3, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(3, 7, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(3, 11, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(7, 3, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(7, 7, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(7, 11, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(11, 3, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(11, 7, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(11, 11, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(15, 3, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(15, 7, SpawnType.THRONE));
        n12.AddSpawn(new SpawnPoint(15, 11, SpawnType.THRONE));

        NodeLayout n13 = new NodeLayout(13, 12, 16, 16, 19);
        n13.AddSpawn(new SpawnPoint(2, 1, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(3, 5, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(6, 1, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(8, 7, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(10, 1, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(12, 5, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(13, 9, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(14, 13, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(14, 1, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(10, 13, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(6, 13, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(2, 13, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(4, 9, SpawnType.PLAYER));
        n13.AddSpawn(new SpawnPoint(4, 1, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(6, 4, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(9, 4, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(12, 1, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(15, 6, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(15, 4, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(10, 10, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(12, 13, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(7, 10, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(4, 13, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(1, 10, SpawnType.THRONE));
        n13.AddSpawn(new SpawnPoint(1, 8, SpawnType.THRONE));

        NodeLayout n14 = new NodeLayout(14, 14, 18, 18, 23);
        n14.AddSpawn(new SpawnPoint(4, 1, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(7, 6, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(8, 1, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(11, 9, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(12, 1, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(14, 5, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(16, 10, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(16, 1, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(14, 14, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(2, 5, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(4, 10, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(2, 14, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(6, 14, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(10, 14, SpawnType.PLAYER));
        n14.AddSpawn(new SpawnPoint(1, 2, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(2, 7, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(1, 11, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(3, 17, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(8, 9, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(7, 11, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(7, 16, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(11, 17, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(15, 16, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(10, 6, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(11, 4, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(16, 8, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(17, 13, SpawnType.THRONE));
        n14.AddSpawn(new SpawnPoint(17, 4, SpawnType.THRONE));

        NodeLayout n15 = new NodeLayout(15, 15, 20, 12, 16);
        n15.AddSpawn(new SpawnPoint(1, 1, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(1, 5, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(1, 9, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(5, 1, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(5, 5, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(5, 9, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(9, 1, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(9, 5, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(9, 9, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(13, 1, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(13, 5, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(13, 9, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(17, 1, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(17, 5, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(17, 9, SpawnType.PLAYER));
        n15.AddSpawn(new SpawnPoint(3, 3, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(3, 7, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(3, 11, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(7, 3, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(7, 7, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(7, 11, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(11, 3, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(11, 7, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(11, 11, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(15, 3, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(15, 7, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(15, 11, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(19, 3, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(19, 7, SpawnType.THRONE));
        n15.AddSpawn(new SpawnPoint(19, 11, SpawnType.THRONE));

        NodeLayout n16 = new NodeLayout(16, 16, 16, 16, 16);
        n16.AddSpawn(new SpawnPoint(1, 1, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(1, 5, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(1, 9, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(1, 13, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(5, 1, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(5, 5, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(5, 9, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(5, 13, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(9, 1, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(9, 5, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(9, 9, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(9, 13, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(13, 1, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(13, 5, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(13, 9, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(13, 13, SpawnType.PLAYER));
        n16.AddSpawn(new SpawnPoint(3, 3, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(3, 7, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(3, 11, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(3, 15, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(7, 3, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(7, 7, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(7, 11, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(7, 15, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(11, 3, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(11, 7, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(11, 11, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(11, 15, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(15, 3, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(15, 7, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(15, 11, SpawnType.THRONE));
        n16.AddSpawn(new SpawnPoint(15, 15, SpawnType.THRONE));

        NodeLayout n17 = new NodeLayout(17, 16, 20, 20, 23);
        n17.AddSpawn(new SpawnPoint(2, 3, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(6, 5, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(6, 1, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(10, 3, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(10, 9, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(14, 13, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(14, 6, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(14, 1, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(18, 15, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(18, 8, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(18, 2, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(2, 10, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(6, 12, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(10, 15, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(14, 17, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(6, 17, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(2, 16, SpawnType.PLAYER));
        n17.AddSpawn(new SpawnPoint(3, 0, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(9, 0, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(10, 6, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(14, 11, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(14, 8, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(14, 3, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(18, 11, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(19, 5, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(2, 7, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(6, 7, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(6, 10, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(6, 15, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(1, 13, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(10, 12, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(11, 18, SpawnType.THRONE));
        n17.AddSpawn(new SpawnPoint(17, 17, SpawnType.THRONE));

        NodeLayout n18 = new NodeLayout(18, 18, 20, 20, 22);
        n18.AddSpawn(new SpawnPoint(2, 1, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(6, 1, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(10, 2, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(14, 2, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(18, 1, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(7, 6, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(12, 7, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(13, 11, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(18, 6, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(18, 11, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(18, 16, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(2, 6, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(2, 11, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(2, 16, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(8, 10, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(6, 15, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(10, 15, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(14, 16, SpawnType.PLAYER));
        n18.AddSpawn(new SpawnPoint(5, 4, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(10, 4, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(15, 13, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(15, 8, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(15, 5, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(19, 9, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(19, 18, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(18, 3, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(1, 8, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(5, 9, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(5, 12, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(10, 13, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(2, 14, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(1, 19, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(5, 18, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(8, 18, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(12, 19, SpawnType.THRONE));
        n18.AddSpawn(new SpawnPoint(15, 19, SpawnType.THRONE));

        NodeLayout n19 = new NodeLayout(19, 19, 20, 20, 21);
        n19.AddSpawn(new SpawnPoint(2, 2, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(6, 2, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(10, 1, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(10, 9, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(12, 5, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(14, 11, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(14, 1, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(17, 6, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(18, 16, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(18, 11, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(18, 1, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(2, 7, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(6, 7, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(3, 12, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(8, 13, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(2, 17, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(6, 17, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(10, 17, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(14, 16, SpawnType.PLAYER));
        n19.AddSpawn(new SpawnPoint(5, 4, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(9, 6, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(9, 3, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(14, 8, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(15, 14, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(15, 4, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(18, 9, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(19, 14, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(1, 4, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(2, 9, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(6, 10, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(5, 14, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(11, 12, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(11, 15, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(2, 19, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(7, 19, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(13, 19, SpawnType.THRONE));
        n19.AddSpawn(new SpawnPoint(18, 19, SpawnType.THRONE));

        NodeLayout n20 = new NodeLayout(20, 20, 20, 16, 16);
        n20.AddSpawn(new SpawnPoint(1, 1, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(1, 5, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(1, 9, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(1, 13, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(5, 1, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(5, 5, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(5, 9, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(5, 13, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(9, 1, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(9, 5, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(9, 9, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(9, 13, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(13, 1, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(13, 5, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(13, 9, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(13, 13, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(17, 1, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(17, 5, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(17, 9, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(17, 13, SpawnType.PLAYER));
        n20.AddSpawn(new SpawnPoint(3, 3, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(3, 7, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(3, 11, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(3, 15, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(7, 3, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(7, 7, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(7, 11, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(7, 15, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(11, 3, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(11, 7, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(11, 11, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(11, 15, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(15, 3, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(15, 7, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(15, 11, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(15, 15, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(19, 3, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(19, 7, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(19, 11, SpawnType.THRONE));
        n20.AddSpawn(new SpawnPoint(19, 15, SpawnType.THRONE));

        m_layouts.Add(n2);
        m_layouts.Add(n3);
        m_layouts.Add(n4);
        m_layouts.Add(n5);
        m_layouts.Add(n6);
        m_layouts.Add(n7);
        m_layouts.Add(n8);
        m_layouts.Add(n9);
        m_layouts.Add(n10);
        m_layouts.Add(n11);
        m_layouts.Add(n12);
        m_layouts.Add(n13);
        m_layouts.Add(n14);
        m_layouts.Add(n15);
        m_layouts.Add(n16);
        m_layouts.Add(n17);
        m_layouts.Add(n18);
        m_layouts.Add(n19);
        m_layouts.Add(n20);
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
                    if (c.ConnectionType == ConnectionType.MOUNTAIN)
                    {
                        c.SetConnection(ConnectionType.MOUNTAINPASS);
                    }
                    else if (c.ConnectionType == ConnectionType.RIVER)
                    {
                        c.SetConnection(ConnectionType.SHALLOWRIVER);
                    }
                }
            }
            else // make sure there's no boxed in provinces
            {
                bool boxed = true;

                foreach (Connection c in n.Connections)
                {
                    if (c.ConnectionType != ConnectionType.RIVER && c.ConnectionType != ConnectionType.MOUNTAIN)
                    {
                        boxed = false;
                        break;
                    }
                }

                if (boxed)
                {
                    Connection c = n.Connections.GetRandom();

                    if (c.ConnectionType == ConnectionType.MOUNTAIN)
                    {
                        c.SetConnection(ConnectionType.MOUNTAINPASS);
                    }
                    else
                    {
                        c.SetConnection(ConnectionType.SHALLOWRIVER);
                    }
                }
            }
        }
    }

    static void generate_roads()
    {
        int num_seas = 0;

        foreach (NationData n in m_nations)
        {
            if (n.IsWater)
            {
                num_seas += m_layout.ProvsPerPlayer;
            }
        }

        List<Node> starts = m_nodes.Where(x => x.Untouched && !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater).ToList();
        
        if (starts.Count < 10)
        {
            starts.AddRange(m_nodes.Where(x => x.SemiUntouched && !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater));
        }

        starts.Shuffle();

        int total_provs = m_layout.TotalProvinces - num_seas;
        int max_roads = (int)(total_provs * 0.10f) - UnityEngine.Random.Range(0, 3); // todo: expose to user
        int count = 0;

        while (count < max_roads && starts.Any())
        {
            Node cur = starts[0];
            starts.Remove(cur);

            Connection con = cur.Connections.GetRandom();
            int limit = 0;

            while ((con.ConnectionType != ConnectionType.STANDARD || con.Node1.ProvinceData.IsWater || con.Node2.ProvinceData.IsWater) && limit < 10)
            {
                con = cur.Connections.GetRandom();
                limit++;
            }

            if (limit >= 10)
            {
                continue;
            }

            con.SetConnection(ConnectionType.ROAD);
            count++;

            if (UnityEngine.Random.Range(0, 12) == 0) // small chance to make a longer road
            {
                if (con.Node1 == cur)
                {
                    if (starts.Contains(con.Node2))
                    {
                        starts.Remove(con.Node2);
                    }

                    starts.Insert(0, con.Node2);
                }
                else
                {
                    if (starts.Contains(con.Node1))
                    {
                        starts.Remove(con.Node1);
                    }

                    starts.Insert(0, con.Node1);
                }
            }
        }
    }

    static void generate_cliffs()
    {
        int num_seas = 0;

        foreach (NationData n in m_nations)
        {
            if (n.IsWater)
            {
                num_seas += m_layout.ProvsPerPlayer;
            }
        }

        List<Node> starts = m_nodes.Where(x => x.Untouched && !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater).ToList();

        if (starts.Count < 10)
        {
            starts.AddRange(m_nodes.Where(x => x.SemiUntouched && !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater));
        }

        starts.Shuffle();

        int total_provs = m_layout.TotalProvinces - num_seas;
        int max_cliffs = (int)(total_provs * 0.28f) + UnityEngine.Random.Range(-4, 5); // todo: expose to user
        int count = 0;

        while (count < max_cliffs && starts.Any())
        {
            Node cur = starts[0];
            starts.Remove(cur);

            Connection con = cur.Connections.GetRandom();
            int limit = 0;

            while ((con.ConnectionType != ConnectionType.STANDARD || con.Node1.ProvinceData.IsWater || con.Node2.ProvinceData.IsWater) && limit < 10)
            {
                con = cur.Connections.GetRandom();
                limit++;
            }

            if (limit == 10 || con == null)
            {
                continue;
            }

            int len = UnityEngine.Random.Range(1, 4);
            int i = 0;
            List<Connection> done = new List<Connection>();

            while (i < len)
            {
                int rand = UnityEngine.Random.Range(0, 10);

                if (rand < 2)
                {
                    con.SetConnection(ConnectionType.MOUNTAINPASS);
                }
                else
                {
                    con.SetConnection(ConnectionType.MOUNTAIN);
                }

                if (con.Node1.HasNation || con.Node2.HasNation)
                {
                    i = len;
                }
                else
                {
                    done.Add(con);

                    limit = 0;
                    con = con.Adjacent.GetRandom();

                    while ((con.ConnectionType != ConnectionType.STANDARD || con.Node1.ProvinceData.IsWater || con.Node2.ProvinceData.IsWater) && limit < 10)
                    {
                        con = con.Adjacent.GetRandom();
                        limit++;
                    }

                    if (limit == 10)
                    {
                        i = len;
                    }
                }

                i++;
                count++;
            }

            Connection c1 = null;
            Connection c2 = null;

            foreach (Connection c in done) // chance to remove unnecessary forks
            {
                if (c1 == null)
                {
                    c1 = c;
                    continue;
                }
                if (c2 == null)
                {
                    c2 = c1;
                    c1 = c;
                    continue;
                }

                List<Node> shared = c.GetUniqueNodes(c1, c2);

                if (shared.Count == 3 && UnityEngine.Random.Range(0, 10) < 4)
                {
                    c1.SetConnection(ConnectionType.STANDARD);
                    count--;
                }

                c2 = c1;
                c1 = c;
            }
        }
    }

    static void generate_rivers()
    {
        int num_seas = 0;

        foreach (NationData n in m_nations)
        {
            if (n.IsWater)
            {
                num_seas += m_layout.ProvsPerPlayer;
            }
        }

        int total_provs = m_layout.TotalProvinces - num_seas;
        int max_rivers = (int) (total_provs * 0.5f) - UnityEngine.Random.Range(0, 9); // todo: expose to user?
        int count = 0;

        if (max_rivers <= 0)
        {
            return;
        }

        List<Connection> starts = new List<Connection>();
        List<Node> water = m_nodes.Where(x => x.ProvinceData.IsWaterSwamp).ToList();
        List<Connection> invalid = m_connections.Where(x => x.Node1.HasNation || x.Node2.HasNation || x.Node1.ProvinceData.IsWater || x.Node2.ProvinceData.IsWater).ToList();

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

                    if (n1.HasConnection(n2) && !n1.IsCapRing && !n2.IsCapRing && !n1.ProvinceData.IsWater && !n2.ProvinceData.IsWater)
                    {
                        Connection c = n1.GetConnectionTo(n2);
                        starts.Add(c);
                    }
                }
            }
        }

        starts.Shuffle();

        Connection cur = null;

        while (count < max_rivers)
        {
            if (starts.Count == 0)
            {
                break;
            }

            if (cur == null)
            {
                cur = starts[0];
                starts.RemoveAt(0);
            }

            int len = UnityEngine.Random.Range(1, 5);
            int i = 0;
            List<Connection> done = new List<Connection>();

            while (i < len)
            {
                if (UnityEngine.Random.Range(0, 10) < 6)
                {
                    cur.SetConnection(ConnectionType.SHALLOWRIVER);
                }
                else
                {
                    cur.SetConnection(ConnectionType.RIVER);
                }

                i++;
                count++;
                done.Add(cur);
                starts.Add(cur);
                starts.Shuffle();

                Connection adj = cur.Adjacent.GetRandom();
                int limit = 0;

                while ((invalid.Contains(adj) || done.Contains(adj) || adj.ConnectionType == ConnectionType.RIVER || adj.ConnectionType == ConnectionType.SHALLOWRIVER) && limit < 10)
                {
                    adj = cur.Adjacent.GetRandom();
                    limit++;
                }

                if (limit == 10)
                {
                    cur = null;
                    break;
                }
                else
                {
                    cur = adj;
                }
            }

            Connection c1 = null;
            Connection c2 = null;

            foreach (Connection c in done) // chance to remove unnecessary forks
            {
                if (c1 == null)
                {
                    c1 = c;
                    continue;
                }
                if (c2 == null)
                {
                    c2 = c1;
                    c1 = c;
                    continue;
                }

                List<Node> shared = c.GetUniqueNodes(c1, c2);

                if (shared.Count == 3 && UnityEngine.Random.Range(0,10) < 4)
                {
                    c1.SetConnection(ConnectionType.STANDARD);
                    count--;
                }

                c2 = c1;
                c1 = c;
            }

            cur = null;
        }
    }

    static void generate_seas()
    {
        foreach (Node n in m_starts)
        {
            if (n.Nation.IsWater)
            {
                int modifier = Mathf.RoundToInt(UnityEngine.Random.Range(0.0f, 0.15f) * m_layout.ProvsPerPlayer) + n.ConnectedNodes.Count;
                int count = 0;
                
                while (count < m_layout.ProvsPerPlayer - modifier)
                {
                    int iterations = UnityEngine.Random.Range(2, 7);
                    int i = 0;
                    Node cur = n.ConnectedNodes.GetRandom();

                    while (i < iterations && count < m_layout.ProvsPerPlayer - modifier)
                    {
                        if (!cur.HasNation && !cur.IsCapRing && !cur.ProvinceData.IsWater)
                        {
                            int rand = UnityEngine.Random.Range(0, 10);

                            cur.ProvinceData.SetTerrainFlags(Terrain.SEA);

                            if (rand == 0)
                            {
                                cur.ProvinceData.SetTerrainFlags(Terrain.DEEPSEA);
                            }
                            else if (rand < 2)
                            {
                                cur.ProvinceData.AddTerrainFlag(Terrain.CAVE);
                            }
                            else if (rand < 3)
                            {
                                cur.ProvinceData.AddTerrainFlag(Terrain.HIGHLAND);
                            }
                            else if (rand < 5)
                            {
                                cur.ProvinceData.AddTerrainFlag(Terrain.FOREST);
                            }

                            i++;
                            count++;
                        }

                        cur = cur.ConnectedNodes.GetRandom();
                    }
                }
            }
        }
    }

    static void generate_farms()
    {
        float num_farms = UnityEngine.Random.Range(0.06f, 0.10f); // 6-10% of the remaining provinces should be farmland - todo: expose this to the user

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && x.ProvinceData.Terrain == Terrain.PLAINS).ToList();
        valid.Shuffle();

        /*foreach (Node n in valid)
        {
            n.AssignScore(m_starts); // very crude province scoring
        }*/
        //var ordered = valid.OrderBy(x => x.FairnessScore).ToList();

        int max = Mathf.RoundToInt(num_farms * m_layout.TotalProvinces);
        int i = 0;
        
        while (i < max && valid.Count > 0)
        {
            Node n = valid[0];
            valid.Remove(n);

            n.ProvinceData.SetTerrainFlags(Terrain.FARM);

            if (UnityEngine.Random.Range(0, 12) == 0)
            {
                n.ProvinceData.AddTerrainFlag(Terrain.WARMER);
            }

            i++;
        }
    }

    static void generate_thrones()
    {
        foreach (Node n in m_nodes)
        {
            if (m_layout.Spawns.Any(x => x.X == n.X && n.Y == x.Y && x.SpawnType == SpawnType.THRONE))
            {
                n.ProvinceData.AddTerrainFlag(Terrain.THRONE);
            }
        }
    }

    static void generate_sized()
    {
        float num_large = UnityEngine.Random.Range(0.20f, 0.24f); // 20-24% of the provinces should be large - todo: expose this to the user? maybe not
        float num_small = UnityEngine.Random.Range(0.20f, 0.24f); // 20-24% of the provinces should be small 

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing).ToList();
        valid.Shuffle();

        Dictionary<NationData, List<Node>> dict = new Dictionary<NationData, List<Node>>();

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

            dict.Add(n.Nation, nodes);
        }

        int max = Mathf.RoundToInt(num_large * m_layout.TotalProvinces);
        int i = 0;

        while (i < max) // fairly assign large provinces to each nation in turn
        {
            foreach (KeyValuePair<NationData, List<Node>> pair in dict)
            {
                if (i >= max)
                {
                    break;
                }

                if (!pair.Value.Any())
                {
                    i++;

                    if (valid.Any())
                    {
                        Node alt = valid[0];
                        valid.Remove(alt);

                        alt.ProvinceData.AddTerrainFlag(Terrain.LARGEPROV);
                    }
                    else
                    {
                        Debug.Log("No provinces left to tag as large");
                    }

                    continue; 
                }

                Node n = pair.Value.GetRandom();
                pair.Value.Remove(n);

                n.ProvinceData.AddTerrainFlag(Terrain.LARGEPROV);

                i++;
            }
        }

        valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.Terrain.IsFlagSet(Terrain.LARGEPROV)).ToList();
        valid.Shuffle();

        max = Mathf.RoundToInt(num_small * m_layout.TotalProvinces);
        i = 0;

        while (i < max)
        {
            Node n = valid.GetRandom();
            valid.Remove(n);

            n.ProvinceData.AddTerrainFlag(Terrain.SMALLPROV);

            i++;
        }
    }

    static void generate_misc()
    {
        float num_highlands = UnityEngine.Random.Range(0.02f, 0.06f); // 2-6% of the provinces should be highlands - todo: expose these to the user
        float num_mountains = UnityEngine.Random.Range(0.02f, 0.06f); // 2-6% of the provinces should be mountainous 
        float num_forests = UnityEngine.Random.Range(0.08f, 0.12f); // 8-12% of the provinces should be forest 
        float num_caves = UnityEngine.Random.Range(0.02f, 0.06f); // 2-6% of the provinces should be caves
        float num_waste = UnityEngine.Random.Range(0.02f, 0.06f); // 2-6% of the provinces should be wastes

        Dictionary<Terrain, float> dict = new Dictionary<Terrain, float>();
        dict.Add(Terrain.HIGHLAND, num_highlands);
        dict.Add(Terrain.MOUNTAINS, num_mountains);
        dict.Add(Terrain.FOREST, num_forests);
        dict.Add(Terrain.CAVE, num_caves);
        dict.Add(Terrain.WASTE, num_waste);

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && x.ProvinceData.Terrain == Terrain.PLAINS).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.HasNation && x.ProvinceData.Terrain == Terrain.PLAINS).ToList();
        }

        valid.Shuffle();

        foreach (KeyValuePair<Terrain, float> pair in dict)
        {
            int max = Mathf.RoundToInt(pair.Value * m_layout.TotalProvinces);
            int i = 0;

            while (i < max && valid.Count > 0)
            {
                Node n = valid[0];
                valid.Remove(n);

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
                if ((pair.Key == Terrain.HIGHLAND || pair.Key == Terrain.MOUNTAINS) && rand == 0)
                {
                    n.ProvinceData.AddTerrainFlag(Terrain.COLDER);
                }

                i++;
            }
        }
    }

    static void generate_lakes_swamps()
    {
        float num_lakes = UnityEngine.Random.Range(0.04f, 0.08f); // 4-8% of the provinces should be watar - todo: expose these to the user
        float num_swamps = UnityEngine.Random.Range(0.04f, 0.08f); // 4-8% of the provinces should be swamp

        if (m_nations.Any(x => x.IsWater))
        {
            num_lakes = UnityEngine.Random.Range(0.01f, 0.05f); // if water nations are playing then reduce the random water provinces
        }

        List<Node> valid = m_nodes.Where(x => !x.HasNation && !x.IsCapRing && !x.ProvinceData.IsWater).ToList();

        if (!m_nat_starts)
        {
            valid = m_nodes.Where(x => !x.HasNation && !x.ProvinceData.IsWater).ToList();
        }

        valid.Shuffle();

        int lakes = Mathf.RoundToInt(num_lakes * m_layout.TotalProvinces);
        int swamps = Mathf.RoundToInt(num_swamps * m_layout.TotalProvinces);

        for (int i = 0; i < lakes; i++)
        {
            if (!valid.Any())
            {
                break;
            }

            Node n = valid[0];
            valid.Remove(n);

            n.ProvinceData.SetTerrainFlags(Terrain.SEA);

            int rand = UnityEngine.Random.Range(0, 10);

            if (rand == 0)
            {
                n.ProvinceData.SetTerrainFlags(Terrain.DEEPSEA);
            }
            else if (rand < 2)
            {
                n.ProvinceData.AddTerrainFlag(Terrain.CAVE);
            }
            else if (rand < 3)
            {
                n.ProvinceData.AddTerrainFlag(Terrain.HIGHLAND);
            }
            else if (rand < 5)
            {
                n.ProvinceData.AddTerrainFlag(Terrain.FOREST);
            }

            if (rand == 6) // small chance to create connected water province
            {
                Connection c = n.Connections.FirstOrDefault(x => !x.Node1.IsCapRing && !x.Node2.IsCapRing);

                if (c != null)
                {
                    if (c.Node1 == n)
                    {
                        valid.Insert(0, c.Node2);
                    }
                    else
                    {
                        valid.Insert(0, c.Node1);
                    }
                }
            }
        }

        for (int i = 0; i < swamps; i++)
        {
            if (!valid.Any())
            {
                break;
            }

            Node n = valid[0];
            valid.Remove(n);

            n.ProvinceData.SetTerrainFlags(Terrain.SWAMP);

            int temp = UnityEngine.Random.Range(0, 10);

            if (temp == 0)
            {
                n.ProvinceData.AddTerrainFlag(Terrain.COLDER);
            }
            else if (temp == 1)
            {
                n.ProvinceData.AddTerrainFlag(Terrain.WARMER);
            }
        }
    }

    static void generate_caprings()
    {
        foreach (Node n in m_nodes)
        {
            if (n.HasNation) // capring logic
            {
                n.Connections.Shuffle();

                int i = 0;

                foreach (Connection c in n.Connections)
                {
                    if (i >= n.Nation.TerrainData.Length)
                    {
                        break;
                    }

                    if (c.Node1 == n)
                    {
                        c.Node2.ProvinceData.SetTerrainFlags(n.Nation.TerrainData[i]);
                    }
                    else
                    {
                        c.Node1.ProvinceData.SetTerrainFlags(n.Nation.TerrainData[i]);
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

        /*//connect top left to bottom right all the time
        Node topleft = m_nodes.FirstOrDefault(n => n.X == 0 && n.Y == m_layout.Y - 1);
        Node botright = m_nodes.FirstOrDefault(n => n.X == m_layout.X - 1 && n.Y == 0);
        Connection con2 = new Connection(topleft, botright, ConnectionType.STANDARD);

        m_connections.Add(con2);
        topleft.AddConnection(con2);
        botright.AddConnection(con2);*/

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

        // capring connections
        foreach (Node n in m_nodes.Where(x => x.HasNation))
        {
            List<Node> diag = new List<Node>();
            diag.Add(get_node_with_wrap(n.X + 1, n.Y + 1));
            diag.Add(get_node_with_wrap(n.X + 1, n.Y - 1));
            diag.Add(get_node_with_wrap(n.X - 1, n.Y - 1));
            diag.Add(get_node_with_wrap(n.X - 1, n.Y + 1));
            diag.Shuffle();

            while (n.Connections.Count < n.Nation.CapRing)
            {
                Node next = diag[0];
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

    static void generate_nodes()
    {
        m_nodes = new List<Node>();
        m_starts = new List<Node>();
        m_connections = new List<Connection>();

        List<NationData> scrambled = new List<NationData>();

        if (m_teamplay)
        {
            int i = 0;
            int front = UnityEngine.Random.Range(0, 2);

            foreach (NationData d in m_nations)
            {
                if (i == 1)
                {
                    i = 0;
                    front = UnityEngine.Random.Range(0, 2);
                }
                else
                {
                    i++;
                }

                if (front == 0)
                {
                    scrambled.Add(d);
                }
                else
                {
                    scrambled.Insert(0, d);
                }
            }

            if (m_num_players == 6)
            {
                NationData d = scrambled[3];
                scrambled.RemoveAt(3);
                scrambled.Add(d);
            }
            else if (m_num_players == 10)
            {
                NationData d = scrambled[5];
                scrambled.RemoveAt(5);
                scrambled.Add(d);
            }
        }
        else
        {
            scrambled.AddRange(m_nations);
            scrambled.Shuffle();
        }

        NodeLayout nl = m_layouts.FirstOrDefault(x => x.NumPlayers == m_num_players);

        if (nl == null)
        {
            Debug.Log("No map layout made for " + m_num_players);
            return;
        }

        m_layout = nl;

        for (int x = 0; x < nl.X; x++)
        {
            for (int y = 0; y < nl.Y; y++)
            {
                if (nl.HasSpawn(x, y, SpawnType.PLAYER))
                {
                    NationData d = scrambled[0];
                    scrambled.RemoveAt(0);

                    Node node = new Node(x, y, new ProvinceData(d.CapTerrain | Terrain.START), d);

                    if ((x == 0 && y == 0) || (x == nl.X - 1 && y == nl.Y - 1))
                    {
                        node.SetWrapCorner(true);
                    }

                    m_nodes.Add(node);
                    m_starts.Add(node);
                }
                else
                {
                    Node node = new Node(x, y, new ProvinceData());

                    if ((x == 0 && y == 0) || (x == nl.X - 1 && y == nl.Y - 1))
                    {
                        node.SetWrapCorner(true);
                    }

                    m_nodes.Add(node);
                }
            }
        }
    }
}
