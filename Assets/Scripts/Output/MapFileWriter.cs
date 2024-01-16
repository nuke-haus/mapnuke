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
    public static void GenerateText(string mapname, NodeLayoutData layout, ElementManager mgr, List<PlayerData> nations, Vector2 mins, Vector2 maxs, List<ProvinceMarker> provs, bool teamplay, int[] province_ids, bool is_for_dom6)
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

        provs = provs.Where(p => !p.IsDummy).OrderBy(p => p.ProvinceNumber).ToList();

        using (var fs = File.Create(path))
        {
            var dom_col_r = (int)(GenerationManager.s_generation_manager.OverlayColor.r * 255f);
            var dom_col_g = (int)(GenerationManager.s_generation_manager.OverlayColor.g * 255f);
            var dom_col_b = (int)(GenerationManager.s_generation_manager.OverlayColor.b * 255f);
            var dom_col_a = (int)(GenerationManager.s_generation_manager.OverlayColor.a * 255f);

            var map_stats = string.Format("Player count: {0}, Throne count: {1}, Province count: {2}, Provinces per player: {3}", 
                layout.NumPlayers, 
                layout.NumThrones, 
                layout.TotalProvinces,
                (float)(layout.TotalProvinces / layout.NumPlayers));

            write(fs, "-- Basic Map Information");
            write(fs, "#dom2title " + mapname);
            write(fs, "#imagefile " + mapname + ".tga");
            write(fs, "#winterimagefile " + mapname + "_winter.tga");
            write(fs, "#mapsize " + x + " " + y);
            write(fs, "#wraparound");
            write(fs, "#maptextcol 0.2 0.0 0.0 1.0");
            write(fs, $"#mapdomcol {dom_col_r} {dom_col_g} {dom_col_b} {dom_col_a}");
            write(fs, is_for_dom6 ? "#domversion 575" : "#domversion 500");
            write(fs, "#description \"[Generated by MapNuke]  " + map_stats + "\"");

            if (is_for_dom6)
            {
                write(fs, "#nodeepcaves");
                write(fs, "\n-- Province Cave Entrance Data");

                foreach (var m in provs)
                {
                    if (m.Node.ProvinceData.HasCaveEntrance)
                    {
                        write(fs, "#gate " + m.ProvinceNumber + " " + m.ProvinceNumber);
                    }
                }
            }


            write(fs, "\n-- Province Custom Name Data");

            foreach (var m in provs)
            {
                if (m.Node.ProvinceData.CustomName != string.Empty)
                {
                    write(fs, "#landname " + m.ProvinceNumber + " \"" + m.Node.ProvinceData.CustomName + "\"");
                }
            }

            write(fs, "\n-- Player Start Location Data");

            if (!nations.Any(nat => nat.NationData.ID == -1)) 
            {
                foreach (var d in nations)
                {
                    write(fs, "#allowedplayer " + d.NationData.ID);
                }
            }

            foreach (var m in provs)
            {
                if (m.Node.HasNation)
                {
                    if (m.Node.Nation.NationData.ID == -1 && !m.Node.Nation.NationData.StartsUnderground)
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
                    else if (!m.Node.Nation.NationData.StartsUnderground)
                    {
                        write(fs, "#specstart " + m.Node.Nation.NationData.ID + " " + m.ProvinceNumber);
                    }
                }
            }

            write(fs, "\n-- Province Terrain Data");

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

                if (is_for_dom6)
                {
                    if (terr.IsFlagSet(Terrain.THRONE))
                    {
                        terr &= Terrain.THRONE; // in dom6 the START and THRONE flags got reversed. don't ask me why, idk.
                        terr |= Terrain.START;
                    }
                }

                if (m.Node.HasNation)
                {
                    if (is_for_dom6)
                    {
                        if (m.Node.Nation.NationData.ID == -1 && !m.Node.Nation.NationData.StartsUnderground)
                        {
                            terr |= Terrain.GENERICSTART;
                        }
                    } 
                    else
                    {
                        terr |= Terrain.START;
                    }
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

            fs.Close();
        }
    }

    public static void GenerateCaveLayerText(string mapname, NodeLayoutData layout, ElementManager mgr, List<PlayerData> nations, Vector2 mins, Vector2 maxs, List<ProvinceMarker> provs, bool teamplay, int[] province_ids)
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

        provs = provs.Where(p => !p.IsDummy).OrderBy(p => p.ProvinceNumber).ToList();

        using (var fs = File.Create(path))
        {
            var dom_col_r = (int)(GenerationManager.s_generation_manager.OverlayColor.r * 255f);
            var dom_col_g = (int)(GenerationManager.s_generation_manager.OverlayColor.g * 255f);
            var dom_col_b = (int)(GenerationManager.s_generation_manager.OverlayColor.b * 255f);
            var dom_col_a = (int)(GenerationManager.s_generation_manager.OverlayColor.a * 255f);

            var map_stats = string.Format("Player count: {0}, Throne count: {1}, Province count: {2}, Provinces per player: {3}",
                layout.NumPlayers,
                layout.NumThrones,
                layout.TotalProvinces,
                (float)(layout.TotalProvinces / layout.NumPlayers));

            write(fs, "-- Basic Map Information");
            write(fs, "#imagefile " + mapname + ".tga");
            write(fs, "#mapsize " + x + " " + y);
            write(fs, "#wraparound");
            write(fs, "#maptextcol 0.2 0.0 0.0 1.0");
            write(fs, $"#mapdomcol {dom_col_r} {dom_col_g} {dom_col_b} {dom_col_a}");
            write(fs, "#domversion 575");
            write(fs, "#planename Cave");
            write(fs, "#description \"[Generated by MapNuke]  " + map_stats + "\"");

            write(fs, "\n-- Province Cave Entrance Data");

            foreach (var m in provs)
            {
                if (m.Node.ProvinceData.HasCaveEntrance)
                {
                    write(fs, "#gate " + m.ProvinceNumber + " " + m.ProvinceNumber);
                }
            }

            write(fs, "\n-- Province Cave Custom Name Data");

            foreach (var m in provs)
            {
                if (m.Node.ProvinceData.CaveCustomName != string.Empty)
                {
                    write(fs, "#landname " + m.ProvinceNumber + " \"" + m.Node.ProvinceData.CaveCustomName + "\"");
                }
            }

            write(fs, "\n-- Player Cave Start Location Data");

            foreach (var m in provs)
            {
                if (m.Node.HasNation)
                {
                    if (m.Node.Nation.NationData.ID == -1 && m.Node.Nation.NationData.StartsUnderground)
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
                    else if (m.Node.Nation.NationData.StartsUnderground)
                    {
                        write(fs, "#specstart " + m.Node.Nation.NationData.ID + " " + m.ProvinceNumber);
                    }
                }
            }

            write(fs, "\n-- Province Terrain Data");

            foreach (var m in provs)
            {
                long terr = ProvinceData.Dom6Cave + ProvinceData.Dom6CaveWall; 

                if (!m.Node.ProvinceData.IsCaveWall)
                {
                    terr = ProvinceData.Dom6Cave + (long)m.Node.ProvinceData.CaveTerrain;
                }

                write(fs, "#terrain " + m.ProvinceNumber + " " + terr);
            }

            write(fs, "\n-- Province Neighbour Data");

            foreach (var m in provs)
            {
                foreach (var c in m.Node.Connections)
                {
                    write(fs, "#neighbour " + c.Node1.ID + " " + c.Node2.ID);

                    /*if (c.ConnectionType != ConnectionType.STANDARD && c.ConnectionType != ConnectionType.SHALLOWRIVER)
                    {
                        write(fs, "#neighbourspec " + c.Node1.ID + " " + c.Node2.ID + " " + (int)c.ConnectionType);
                    }*/
                }
            }

            write(fs, "\n-- Province Border Data");

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

    private static string get_output_dir()
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        return Application.persistentDataPath;
#else
        return Application.dataPath;
#endif
    }

    public static void open_folder_osx(string path)
    {
        var valid_folder = false;
        var mac_path = path.Replace("\\", "/"); 

        if (System.IO.Directory.Exists(mac_path)) 
        {
            valid_folder = true;
        }

        if (!mac_path.StartsWith("\""))
        {
            mac_path = "\"" + mac_path;
        }

        if (!mac_path.EndsWith("\""))
        {
            mac_path += "\"";
        }

        var arguments = (valid_folder ? "" : "-R ") + mac_path;
        System.Diagnostics.Process.Start("open", arguments);
    }

    public static void open_folder_win(string path)
    {
        var valid_folder = false;
        var winPath = path.Replace("/", "\\");

        if (System.IO.Directory.Exists(winPath)) 
        {
            valid_folder = true;
        }

        System.Diagnostics.Process.Start("explorer.exe", (valid_folder ? "/root," : "/select,") + winPath);
    }

    public static void open_folder(string path)
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        open_folder_osx(path);
#elif UNITY_STANDALONE_WIN
        open_folder_win(path);
#endif
    }

    public static void GenerateImage(string mapname, RenderTexture tex, bool is_targa = true)
    {
        var t = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        var data_folder = get_output_dir();
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

    public static void OpenFolder()
    {
        var data_folder = get_output_dir();
        var folder = data_folder + "/Export/";
        open_folder(folder);
    }
}
