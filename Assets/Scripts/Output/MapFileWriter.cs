﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Data structure used for #pb lines in .map file.
/// </summary>
public class PolyLineData
{
    public Node Node;
    public int NumPixels;
    public int ProvNum;
    public Vector3 Start;
    public bool IsEdge = false;

    public int X
    {
        get
        {
            return Mathf.RoundToInt(Start.x * 100f);
        }
    }

    public int Y
    {
        get
        {
            return Mathf.RoundToInt(Start.y * 100f);
        }
    }

    public PolyLineData(Node n, Vector3 start, int prov_num, int num_pixels)
    {
        Node = n;
        NumPixels = num_pixels;
        ProvNum = prov_num;
        Start = start;
    }

    public void Increment()
    {
        NumPixels++;
    }
}

/// <summary>
/// Handles output of .map file as well as image output.
/// </summary>
public static class MapFileWriter
{
    public static void GenerateText(string mapname, NodeLayout layout, ElementManager mgr, List<PlayerData> nations, Vector2 mins, Vector2 maxs, List<ProvinceMarker> provs, bool teamplay, int[] province_ids)
    {
        var data_folder = Application.dataPath;
        var folder = data_folder + "/Export/";
        var path = folder + mapname + ".map";

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var x = Mathf.RoundToInt(layout.X * mgr.X * 100);
        var y = Mathf.RoundToInt(layout.Y * mgr.Y * 100);

        provs = provs.Where(p => !p.IsDummy).ToList();
        provs = provs.OrderBy(p => p.ProvinceNumber).ToList();

        using (var fs = File.Create(path))
        {
            var dom_col_r = (int)(GenerationManager.s_generation_manager.OverlayColor.r * 255f);
            var dom_col_g = (int)(GenerationManager.s_generation_manager.OverlayColor.g * 255f);
            var dom_col_b = (int)(GenerationManager.s_generation_manager.OverlayColor.b * 255f);
            var dom_col_a = (int)(GenerationManager.s_generation_manager.OverlayColor.a * 255f);

            var map_stats = string.Format("Player count: {0}, Throne count: {1}, Province count: {2}", layout.NumPlayers, layout.NumThrones, layout.TotalProvinces);

            write(fs, "-- Basic Map Information");
            write(fs, "#dom2title " + mapname);
            write(fs, "#imagefile " + mapname + ".tga");
            write(fs, "#winterimagefile " + mapname + "_winter.tga");
            write(fs, "#mapsize " + x + " " + y);
            write(fs, "#wraparound");
            write(fs, "#maptextcol 0.2 0.0 0.0 1.0");
            write(fs, $"#mapdomcol {dom_col_r} {dom_col_g} {dom_col_b} {dom_col_a}");
            write(fs, "#description \"[Generated by MapNuke]  " + map_stats + "\"");

            write(fs, "\n-- Start Location Data");

            foreach (var d in nations)
            {
                if (d.NationData.ID > -1)
                {
                    write(fs, "#allowedplayer " + d.NationData.ID);
                }
            }

            foreach (var m in provs)
            {
                if (m.Node.HasNation)
                {
                    if (m.Node.Nation.NationData.ID == -1)
                    {
                        if (teamplay)
                        {
                            write(fs, "#teamstart " + m.ProvinceNumber + " " + m.Node.Nation.TeamNum);
                        }
                        else
                        {
                            write(fs, "#start " + m.ProvinceNumber);
                        }
                    }
                    else
                    {
                        write(fs, "#specstart " + m.Node.Nation.NationData.ID + " " + m.ProvinceNumber);
                    }
                }
            }

            write(fs, "\n-- Terrain Data");

            foreach (var m in provs)
            {
                var terr = m.Node.ProvinceData.Terrain;

                if (GeneratorSettings.s_generator_settings.UseClassicMountains && !terr.IsFlagSet(Terrain.MOUNTAINS) && mountain_count(m) >= 1f)
                {
                    terr |= Terrain.MOUNTAINS;
                }

                if (m.Node.Connections.Any(c => c.ConnectionType == ConnectionType.RIVER || c.ConnectionType == ConnectionType.SHALLOWRIVER))
                {
                    terr |= Terrain.FRESHWATER;
                }

                if (m.Node.HasNation)
                {
                    terr |= Terrain.START;
                }

                write(fs, "#terrain " + m.ProvinceNumber + " " + (int)terr);
            }

            write(fs, "\n-- Province Neighbour Data");

            foreach (var m in provs)
            {
                foreach (var c in m.Node.Connections)
                {
                    write(fs, "#neighbour " + c.Node1.ID + " " + c.Node2.ID);

                    if (c.ConnectionType != ConnectionType.STANDARD && c.ConnectionType != ConnectionType.SHALLOWRIVER)
                    {
                        write(fs, "#neighbourspec " + c.Node1.ID + " " + c.Node2.ID + " " + (int)c.ConnectionType);
                    }
                }
            }

            write(fs, "\n-- Province Border Data");

            {
                var cur = 0;
                for (var cur_y = 0; cur_y < y; ++cur_y)
                {
                    var cur_id = 0;
                    var cur_px = 0;
                    var cur_x = 0;
                    for (; cur_x < x; ++cur_x)
                    {
                        var nex_id = province_ids[cur];
                        if (nex_id != cur_id)
                        {
                            if (cur_id != 0)
                            {
                                write(fs, "#pb " + (cur_x - cur_px) + " " + cur_y + " " + cur_px + " " + cur_id);
                            }
                            cur_px = 0;
                        }
                        cur_id = nex_id;
                        cur_px++;
                        cur++;
                    }
                    if (cur_id != 0) write(fs, "#pb " + (cur_x - cur_px) + " " + cur_y + " " + cur_px + " " + cur_id);
                }
            }

            fs.Close();
        }
    }

    private static float mountain_count(ProvinceMarker m)
    {
        var res = 0f;

        foreach (var c in m.Node.Connections)
        {
            if (c.ConnectionType == ConnectionType.MOUNTAIN)
            {
                res += 1f;
            }
            else if (c.ConnectionType == ConnectionType.MOUNTAINPASS)
            {
                res += 0.5f;
            }
        }

        return res;
    }

    private static void write(FileStream fs, string value)
    {
        value += "\n";
        var info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }

    public static void GenerateImage(string mapname, RenderTexture tex, bool is_targa = true)
    {
        var t = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        var data_folder = Application.dataPath;
        var folder = data_folder + "/Export/";

        if (is_targa)
        {
            var bytes = t.EncodeToTGA();
            var path = folder + mapname + ".tga";

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllBytes(path, bytes);
        }
        else
        {
            var bytes = t.EncodeToPNG();
            var path = folder + mapname + ".png";

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllBytes(path, bytes);
        }
    }
}
