﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
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
    public static void GenerateText(string mapname, NodeLayout layout, ElementManager mgr, List<PlayerData> nations, Vector2 mins, Vector2 maxs, List<ProvinceMarker> provs, bool teamplay)
    {
        string data_folder = Application.dataPath;
        string folder = data_folder + "/Export/";
        string path = folder + mapname + ".map";

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        int x = Mathf.RoundToInt(layout.X * mgr.X * 100);
        int y = Mathf.RoundToInt(layout.Y * mgr.Y * 100);

        provs = provs.Where(p => !p.IsDummy).ToList();
        provs = provs.OrderBy(p => p.ProvinceNumber).ToList();

        using (FileStream fs = File.Create(path))
        {
            string map_stats = string.Format("Player count: {0}, Throne count: {1}, Province count: {2}", layout.NumPlayers, layout.NumThrones, layout.TotalProvinces);

            write(fs, "-- Basic Map Information");
            write(fs, "#dom2title " + mapname);
            write(fs, "#imagefile " + mapname + ".tga");
            write(fs, "#winterimagefile " + mapname + "_winter.tga");
            write(fs, "#mapsize " + x + " " + y);
            write(fs, "#wraparound");
            write(fs, "#maptextcol 0.2 0.0 0.0 1.0");
            write(fs, "#description \"[Generated by A.I.D.S]  " + map_stats + "\"");

            write(fs, "\n-- Start Location Data");

            foreach (PlayerData d in nations)
            {
                if (d.NationData.ID > -1)
                {
                    write(fs, "#allowedplayer " + d.NationData.ID);
                }
            }

            foreach (ProvinceMarker m in provs)
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

            foreach (ProvinceMarker m in provs)
            {
                Terrain terr = m.Node.ProvinceData.Terrain;

                if (m.Node.Connections.Any(c => c.ConnectionType == ConnectionType.RIVER || c.ConnectionType == ConnectionType.SHALLOWRIVER))
                {
                    terr = terr | Terrain.FRESHWATER;
                }

                if (m.Node.HasNation)
                {
                    terr = terr | Terrain.START;
                }

                write(fs, "#terrain " + m.ProvinceNumber + " " + (int) terr);
            }

            write(fs, "\n-- Province Neighbour Data");

            foreach (ProvinceMarker m in provs)
            {
                foreach (Connection c in m.Node.Connections)
                {
                    write(fs, "#neighbour " + c.Node1.ID + " " + c.Node2.ID);

                    if (c.ConnectionType != ConnectionType.STANDARD && c.ConnectionType != ConnectionType.SHALLOWRIVER)
                    {
                        write(fs, "#neighbourspec " + c.Node1.ID + " " + c.Node2.ID + " " + (int)c.ConnectionType);
                    }
                }
            }

            write(fs, "\n-- Province Border Data");

            List<PolyLineData> pd = calculate_poly_data();

            foreach (PolyLineData p in pd)
            {
                write(fs, "#pb " + p.X + " " + p.Y + " " + p.NumPixels + " " + p.ProvNum);
            }

            fs.Close();
        }
    }

    static RaycastHit do_ray_trace(Vector3 pt)
    {
        pt.z = -900;

        RaycastHit hit;
        Physics.Raycast(pt, Vector3.forward, out hit, 9000);

        return hit;
    }

    private static List<PolyLineData> calculate_poly_data()
    {
        Vector3 mins = MapBorder.s_map_border.Mins;
        Vector3 maxs = MapBorder.s_map_border.Maxs;
        Vector3 cur = mins;
        float pixel = 0.01f;
        List<PolyLineData> data = new List<PolyLineData>();
        PolyLineData cur_data = null;

        while (cur.y <= maxs.y)
        {
            while (cur.x <= maxs.x)
            {
                RaycastHit hit = do_ray_trace(cur);

                if (hit.collider != null)
                {
                    ProvinceMarker pm = hit.collider.gameObject.GetComponentInParent<ProvinceMarker>();
                    ProvinceWrapMarker pwm = hit.collider.gameObject.GetComponentInParent<ProvinceWrapMarker>();
                    Node n = null;
                    int prov_num = int.MaxValue;

                    if (pm != null)
                    {
                        n = pm.Node;
                        prov_num = pm.ProvinceNumber;
                    }
                    else if (pwm != null)
                    {
                        n = pwm.Parent.Node;
                        prov_num = pwm.Parent.ProvinceNumber;
                    }

                    if (n != null)
                    {
                        if (cur_data == null)
                        {
                            cur_data = new PolyLineData(n, cur - mins, prov_num, 1);
                        }
                        else
                        {
                            if (cur_data.Node == n)
                            {
                                cur_data.Increment();
                            }
                            else
                            {
                                data.Add(cur_data);
                                cur_data = new PolyLineData(n, cur - mins, prov_num, 1);
                            }
                        }
                    }
                }
                else
                {
                    if (cur_data != null)
                    {
                        cur_data.Increment();
                    }
                }

                cur.x += pixel;
            }

            if (cur_data != null)
            {
                cur_data.IsEdge = true;
                data.Add(cur_data);

                cur_data = null;
            }

            cur.y += pixel;
            cur.x = mins.x;
        }

        /*List<PolyLineData> bad = new List<PolyLineData>();

        foreach (PolyLineData p in data.Where(x => x.IsEdge))
        {
            PolyLineData left = data.FirstOrDefault(x => x.Y == p.Y && x != p && x.Node == p.Node);

            if (left != null)
            {
                p.NumPixels += left.NumPixels;
                bad.Add(left);
            }
        }

        data.RemoveAll(x => bad.Contains(x));*/

        return data;
    }

    private static void write(FileStream fs, string value)
    {
        value += "\n";
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }

    public static void GenerateImage(string mapname, RenderTexture tex, bool is_targa = true)
    {
        Texture2D t = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        string data_folder = Application.dataPath;
        string folder = data_folder + "/Export/";

        if (is_targa)
        {
            byte[] bytes = t.EncodeToTGA();
            string path = folder + mapname + ".tga";

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
            byte[] bytes = t.EncodeToPNG();
            string path = folder + mapname + ".png";

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
