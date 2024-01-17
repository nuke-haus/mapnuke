using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager class that handles all user input and generally is the entry point for a lot of logic.
/// Has a global singleton.
/// </summary>
public class GenerationManager : MonoBehaviour
{
    public static GenerationManager s_generation_manager;

    public Camera CaptureCamera;
    public Toggle NatStarts;
    public AudioClip AcceptAudio;
    public AudioClip ClickAudio;
    public AudioClip DenyAudio;
    public GameObject OutputWindow;
    public GameObject LoadingScreen;
    public InputField MapName;
    public Dropdown LayoutDropdown;
    public GameObject NationPicker;
    public GameObject ScrollContent;
    public GameObject ScrollPanel;
    public GameObject Logo;
    public GameObject LogScreen;
    public GameObject LogContent;
    public GameObject[] HideableOptions;
    public GameObject[] HideableControls;
    public GameObject[] HideableButtons;
    public InputField[] OverlayFields;
    public InputField[] BorderFields;
    public InputField[] SeaBorderFields;
    public InputField LoggerText;
    public Image OverlayPreview;
    public Image BorderPreview;
    public Image SeaBorderPreview;
    public MeshRenderer province_id_mesh_prefab;

    private GameObject province_id_map_container;
    private Color m_border_color = new Color();
    private Color m_sea_border_color = new Color();
    private Color m_overlay_color = new Color();
    private bool m_generic_starts = false;
    private bool m_cluster_water = true;
    private bool m_cluster_islands = false;
    private bool m_teamplay = false;
    private bool m_is_for_dom6 = false;
    private int m_player_count = 9;
    private Age m_age = Age.EARLY;
    private Season m_season = Season.SUMMER;
    private List<GameObject> m_log_content;
    private List<GameObject> m_content;
    private List<PlayerData> m_nations;
    private NodeLayoutCollection m_layouts;
    private NodeLayoutData m_layout;
    private CustomNameDataCollection m_name_data;
    private CustomNameFormatCollection m_name_formats;
    private Terrain[] m_dom6_terrains =
        {
            Terrain.PLAINS,
            Terrain.SEA,
            Terrain.SEA | Terrain.FOREST,
            Terrain.HIGHLAND,
            Terrain.SWAMP,
            Terrain.WASTE,
            Terrain.FOREST,
            Terrain.FARM,
            Terrain.CAVE
        };

    public Color BorderColor => m_border_color;
    public Color SeaBorderColor => m_sea_border_color;
    public Color OverlayColor => m_overlay_color;

    public Season Season
    {
        get
        {
            return m_season;
        }
    }

    public List<PlayerData> NationData
    {
        get
        {
            return m_nations;
        }
    }

    private void Start()
    {
        AllNationData.Init();
        GeneratorSettings.Initialize();

        s_generation_manager = this;
        m_content = new List<GameObject>();
        m_log_content = new List<GameObject>();

        Application.logMessageReceived += handle_log;

        load_name_data();
        load_layouts();
        load_nation_data();
        update_nations();
        hide_controls();
        OnBorderColorUpdate();
        OnSeaBorderColorUpdate();
        OnOverlayColorUpdate();
    }

    private void Update()
    {
        Util.ResetFrameTime();
    }

    void handle_log(string log, string stack, LogType type)
    {
        if (type == LogType.Exception && !LogScreen.activeSelf)
        {
            LoggerText.text = log + "\n\n" + stack;
            LogScreen.SetActive(true);
            GetComponent<AudioSource>().PlayOneShot(DenyAudio);
        }
    }

    public void CloseLogScreen()
    {
        LogScreen.SetActive(false);
    }

    public void LogText(string text)
    {
        StartCoroutine(do_log(text));
    }

    private IEnumerator do_log(string text)
    {
        yield return null;

        var pos = m_log_content.Count + 1;

        var obj = GameObject.Instantiate(LogContent);
        var rt = obj.GetComponent<RectTransform>();
        var txt = obj.GetComponent<UnityEngine.UI.Text>();

        txt.text = text;
        rt.SetParent(LogScreen.GetComponent<RectTransform>());
        rt.localPosition = new Vector3(10, 10 - (pos * 20), 0);
        rt.sizeDelta = new Vector2(0, 20);
        rt.anchoredPosition = new Vector2(10, 10 - (pos * 20));

        m_log_content.Add(obj);

        yield return null;
    }

    public void Click()
    {
        GetComponent<AudioSource>().PlayOneShot(ClickAudio);
    }

    public void ClearLog()
    {
        foreach (var g in m_log_content)
        {
            GameObject.Destroy(g);
        }

        m_log_content = new List<GameObject>();
    }

    public void OnGenerate()
    {
        var picks = new List<PlayerData>();

        foreach (var obj in m_content)
        {
            var np = obj.GetComponent<NationPicker>();
            var str = np.NationName;
            var data = AllNationData.AllNations.FirstOrDefault(x => x.Name == str);
            var pd = new PlayerData(data, np.TeamNum);

            if (data == null || (picks.Any(x => x.NationData.Name == data.Name && x.NationData.ID > -1) && !m_generic_starts))
            {
                GetComponent<AudioSource>().PlayOneShot(DenyAudio);
                return;
            }

            picks.Add(pd);
        }

        m_nations = picks;

        if (ElementManager.s_element_manager.GeneratedObjects.Any())
        {
            ElementManager.s_element_manager.WipeGeneratedObjects();
        }

        GetComponent<AudioSource>().PlayOneShot(AcceptAudio);
        var layout = m_layouts.Layouts.FirstOrDefault(x => x.Name == LayoutDropdown.options[LayoutDropdown.value].text && x.NumPlayers == m_player_count);
        MoveCameraForGeneration(layout);
        Resources.UnloadUnusedAssets();
        StartCoroutine(perform_async(() => do_generate(layout), true));
    }

    private void MoveCameraForGeneration(NodeLayoutData layout)
    {
        var main_cam = Camera.main;
        var p = main_cam.transform.position;
        p.x = layout.X;
        p.y = layout.Y * 0.5f;
        main_cam.transform.position = p;
    }

    private IEnumerator do_generate(NodeLayoutData layout) // pipeline for initial generation of all nodes and stuff
    {
        foreach (var obj in HideableButtons)
        {
            obj.SetActive(false);
        }
        
        if (layout == null)
        {
            layout = m_layouts.Layouts.FirstOrDefault(x => x.NumPlayers == m_player_count);
        }

        m_layout = layout;
        m_season = Season.SUMMER;

        // create the conceptual nodes and connections first
        WorldGenerator.GenerateWorld(m_teamplay, m_cluster_water, m_cluster_islands, NatStarts.isOn, m_nations, layout);
        var conns = WorldGenerator.GetConnections();
        var nodes = WorldGenerator.GetNodes();

        // generate the unity objects using the conceptual nodes
        var mgr = GetComponent<ElementManager>();

        // position and resize the cameras
        var campos = new Vector3(layout.X * 0.5f * mgr.X - mgr.X, layout.Y * 0.5f * mgr.Y - mgr.Y, -10);
        CaptureCamera.transform.position = campos;

        var ortho = (mgr.Y * layout.Y * 100) / 100f / 2f;
        CaptureCamera.orthographicSize = ortho;

        yield return StartCoroutine(mgr.GenerateElements(nodes, conns, layout));

        set_province_names(mgr.Provinces);

        ProvinceManager.s_province_manager.SetLayout(layout);
        ConnectionManager.s_connection_manager.SetLayout(layout);
        Camera.main.transform.position = campos + new Vector3(500f, 0f, 0f);

        foreach (var obj in HideableButtons)
        {
            obj.SetActive(true);
        }
    }

    private void set_province_names(List<ProvinceMarker> provinces)
    {
        var name_chance = GeneratorSettings.s_generator_settings.CustomNameFreq;

        foreach (var province in provinces)
        {
            if (!province.Node.HasNation && name_chance > UnityEngine.Random.Range(0f, 1f))
            {
                province.Node.ProvinceData.SetCustomName(generate_custom_name(provinces, province));
            }

            if (!province.Node.ProvinceData.IsCaveWall && name_chance > UnityEngine.Random.Range(0f, 1f))
            {
                province.Node.ProvinceData.SetCaveCustomName(generate_cave_name(provinces, province));
            }
        }
    }

    private string generate_cave_name(List<ProvinceMarker> all, ProvinceMarker marker)
    {
        var format = m_name_formats.GetRandom(Terrain.CAVE);
        var name = string.Empty;

        foreach (var id in format.Strings)
        {
            var str = m_name_data.GetRandomCaveString(id, marker.Node.ProvinceData.CaveTerrain);

            if (str == null)
            {
                Debug.LogError($"Unable to find custom name data with id: {id}, terrain: {marker.Node.ProvinceData.CaveTerrain}");
            }
            else
            {
                if (id == "SPACE")
                {
                    name += " ";
                }
                else
                {
                    name += str;
                }
            }
        }

        name = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name);
        name = name.Replace(" Of ", " of ");

        if (all.Any(x => x.Node.ProvinceData.CaveCustomName != string.Empty && (x.Node.ProvinceData.CaveCustomName.Contains(name) || name.Contains(x.Node.ProvinceData.CaveCustomName))))
        {
            return string.Empty;
        }

        return name;
    }

    private string generate_custom_name(List<ProvinceMarker> all, ProvinceMarker marker, bool is_cave = false)
    {
        var format = m_name_formats.GetRandom(marker.Node.ProvinceData.Terrain);
        var name = string.Empty;
        
        foreach (var id in format.Strings)
        {
            var str = m_name_data.GetRandomString(id, marker.Node.ProvinceData.Terrain, marker.Node.ProvinceData.IsPlains);

            if (str == null)
            {
                Debug.LogError($"Unable to find custom name data with id: {id}, terrain: {marker.Node.ProvinceData.Terrain}");
            } 
            else
            {
                if (id == "SPACE")
                {
                    name += " ";
                }
                else
                {
                    name += str;
                }
            }
        }

        name = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name);
        name = name.Replace(" Of ", " of ");

        if (all.Any(x => x.Node.ProvinceData.CustomName != string.Empty && (x.Node.ProvinceData.CustomName.Contains(name) || name.Contains(x.Node.ProvinceData.CustomName))))
        {
            return string.Empty;
        }

        return name;
    }

    private void do_regen(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayoutData layout)
    {
        ArtManager.s_art_manager.RegenerateElements(provs, conns, layout);
    }

    public void RegenerateElements(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayoutData layout)
    {
        Resources.UnloadUnusedAssets();
        StartCoroutine(perform_async(() => do_regen(provs, conns, layout)));
    }

    private IEnumerator perform_async(System.Func<IEnumerator> function, bool show_log = false)
    {
        LoadingScreen.SetActive(true);

        yield return null;
        yield return new WaitUntil(() => LoadingScreen.activeInHierarchy);
        var start_time = Time.realtimeSinceStartup;
        if (function != null)
        {
            yield return StartCoroutine(function());
        }
        var total_time = Time.realtimeSinceStartup - start_time;
        Debug.LogFormat("Generation time: {0}", total_time);

        LoadingScreen.SetActive(false);
    }

    private IEnumerator perform_async(System.Action function, bool show_log = false)
    {
        LoadingScreen.SetActive(true);

        yield return null;
        yield return new WaitUntil(() => LoadingScreen.activeInHierarchy);

        if (function != null)
        {
            function();
        }

        LoadingScreen.SetActive(false);
    }

    public void OnSeasonChanged()
    {
        GetComponent<AudioSource>().PlayOneShot(ClickAudio);

        StartCoroutine(perform_async(() => do_season_change()));
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }

    private void do_season_change()
    {
        m_season = m_season == Season.SUMMER
            ? Season.WINTER
            : Season.SUMMER;

        ArtManager.s_art_manager.ChangeSeason(m_season);
    }

    public void ShowOutputWindow(bool is_for_dom6)
    {
        m_is_for_dom6 = is_for_dom6;
        GetComponent<AudioSource>().PlayOneShot(ClickAudio);

        if (!OutputWindow.activeSelf)
        {
            OutputWindow.SetActive(true);
        }
    }

    public void GenerateOutput()
    {
        var str = MapName.text;

        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        OutputWindow.SetActive(false);
        GetComponent<AudioSource>().PlayOneShot(ClickAudio);

        StartCoroutine(output_async(m_is_for_dom6, str));
    }

    private void MakeProvinceIdTexture(ProvinceMarker pm, Vector3 offset, Transform t)
    {
        var cam_bounds = CaptureCam.Bounds(offset);

        var pn = pm.ProvinceNumber;
        Color c = new Color32(0, (byte)(pn / 256), (byte)(pn % 256), 255);
        Debug.Assert(pn != 0);
        {
            var province_mesh = Instantiate(province_id_mesh_prefab);
            province_mesh.GetComponent<MeshFilter>().mesh = pm.MeshFilter.sharedMesh;
            var new_mat = Instantiate(province_id_mesh_prefab.sharedMaterial);
            new_mat.color = c;
            province_mesh.GetComponent<MeshRenderer>().material = new_mat;
            province_mesh.transform.position = offset;
            var bounds = province_mesh.bounds;
            province_mesh.transform.SetParent(t);
            BorderOverlap.Duplicate(province_mesh.gameObject, province_mesh.bounds, cam_bounds);
        }
    }

    public int[] GetProvinceIdVals(Vector3 offset)
    {
        var mgr = GetComponent<ElementManager>();
        province_id_map_container = new GameObject("ProvinceIdMapContainer");
        var tr = province_id_map_container.transform;

        foreach (var prov in mgr.Provinces)
        {
            MakeProvinceIdTexture(prov, offset, tr);
        }

        CaptureCam.s_capture_cam.transform.position += offset;
        CaptureCam.s_capture_cam.Camera.Render();
        CaptureCam.s_capture_cam.transform.position -= offset;

        var tex = mgr.Texture;
        var t = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        var pixels = t.GetPixels32();

        Destroy(t);
        Destroy(province_id_map_container);

        var province_ids = new int[pixels.Length];

        for (var i = 0; i < pixels.Length; ++i)
        {
            var p = pixels[i];
            province_ids[i] = p.b + p.g * 256;
        }

        return province_ids;
    }

    // Dom6 expects a certain filename for each terrain
    private string get_terrain_name(Terrain terrain)
    {
        switch (terrain)
        {
            case Terrain.CAVE:
                return "plane2"; // we need to use this name for cave plane
            case Terrain.PLAINS:
                return "plain";
            case Terrain.FARM:
                return "farm";
            case Terrain.FOREST:
                return "forest";
            case Terrain.HIGHLAND:
                return "highland";
            case Terrain.SWAMP:
                return "swamp";
            case Terrain.WASTE:
                return "waste";
            case Terrain.SEA:
                return "sea";
            default:
                return "kelp"; // sea and forest
        }
    }

    private IEnumerator output_async(bool is_for_dom6, string str, bool show_log = false)
    {
        LoadingScreen.SetActive(true);

        yield return null;
        yield return new WaitUntil(() => LoadingScreen.activeInHierarchy);

        var mgr = GetComponent<ElementManager>();
        var layout = WorldGenerator.GetLayout();
        var province_ids = GetProvinceIdVals(new Vector3(0, 60, 0));

        MapFileWriter.GenerateText(str, layout, mgr, m_nations, new Vector2(-mgr.X, -mgr.Y), new Vector2(mgr.X * (layout.X - 1), mgr.Y * (layout.Y - 1)), mgr.Provinces, m_teamplay, province_ids, is_for_dom6);

        yield return null;

        MapFileWriter.GenerateImage(str, str, mgr.Texture); // summer image

        mgr.ShowLabels(true);

        yield return null;

        MapFileWriter.GenerateImage(str, str + "_with_labels", mgr.Texture, false); // labeled image

        mgr.ShowLabels(false);

        do_season_change();

        yield return new WaitUntil(() => ArtManager.s_art_manager.JustChangedSeason);
        yield return new WaitForEndOfFrame();
        yield return null;

        ArtManager.s_art_manager.CaptureCam.Render(); 

        yield return new WaitForEndOfFrame();
        yield return null;

        MapFileWriter.GenerateImage(str, str + "_winter", mgr.Texture); // winter image

        do_season_change();

        yield return new WaitUntil(() => ArtManager.s_art_manager.JustChangedSeason);
        yield return new WaitForEndOfFrame();
        yield return null;

        if (is_for_dom6)
        {
            // Dom6 requires we generate a lot more files for each province type
            // We don't want province shapes to change while we generate all of these different outputs
            ArtManager.s_art_manager.LockProvinceShapes(true);
            mgr.LockMapData(true);

            foreach (Terrain t in m_dom6_terrains)
            {
                mgr.OverrideAllProvinceTerrain(t);
                ArtManager.s_art_manager.OnOverrideProvinceTerrain(t == Terrain.CAVE);

                Resources.UnloadUnusedAssets();
                yield return new WaitForEndOfFrame();
                yield return null;
                yield return StartCoroutine(perform_async(() => do_regen(mgr.Provinces, mgr.Connections, m_layout)));
                yield return new WaitForEndOfFrame();
                yield return null;

                string enum_name = get_terrain_name(t);

                MapFileWriter.GenerateImage(str, str + "_" + enum_name, mgr.Texture); // summer image

                if (t != Terrain.CAVE) // underworld layer does not need winter sprites
                {
                    do_season_change();

                    yield return new WaitUntil(() => ArtManager.s_art_manager.JustChangedSeason);
                    yield return new WaitForEndOfFrame();
                    yield return null;

                    MapFileWriter.GenerateImage(str, str + "_" + enum_name + "w", mgr.Texture); // winter image

                    do_season_change();

                    yield return new WaitUntil(() => ArtManager.s_art_manager.JustChangedSeason);
                    yield return new WaitForEndOfFrame();
                    yield return null;
                }
            }

            ArtManager.s_art_manager.OnOverrideProvinceTerrain(false);
            mgr.LockMapData(false);

            Resources.UnloadUnusedAssets();
            yield return StartCoroutine(perform_async(() => do_regen(mgr.Provinces, mgr.Connections, m_layout)));
            yield return new WaitForEndOfFrame();
            yield return null;

            ArtManager.s_art_manager.LockProvinceShapes(false);

            MapFileWriter.GenerateCaveLayerText(str, str + "_plane2", layout, mgr, m_nations, new Vector2(-mgr.X, -mgr.Y), new Vector2(mgr.X * (layout.X - 1), mgr.Y * (layout.Y - 1)), mgr.Provinces, m_teamplay, province_ids);

            yield return null;
        }

        GetComponent<AudioSource>().PlayOneShot(AcceptAudio);

        LoadingScreen.SetActive(false);

        MapFileWriter.OpenFolder();
    }

    public void UpdateColors(Color overlay, Color land_border, Color sea_border)
    {
        m_overlay_color = overlay;
        m_border_color = land_border;
        m_sea_border_color = sea_border;

        OverlayPreview.color = m_overlay_color;
        BorderPreview.color = m_border_color;
        SeaBorderPreview.color = m_sea_border_color;

        OverlayFields[0].text = Mathf.RoundToInt(m_overlay_color.r * 255f).ToString();
        OverlayFields[1].text = Mathf.RoundToInt(m_overlay_color.g * 255f).ToString();
        OverlayFields[2].text = Mathf.RoundToInt(m_overlay_color.b * 255f).ToString();
        OverlayFields[3].text = Mathf.RoundToInt(m_overlay_color.a * 255f).ToString();

        BorderFields[0].text = Mathf.RoundToInt(m_border_color.r * 255f).ToString();
        BorderFields[1].text = Mathf.RoundToInt(m_border_color.g * 255f).ToString();
        BorderFields[2].text = Mathf.RoundToInt(m_border_color.b * 255f).ToString();
        BorderFields[3].text = Mathf.RoundToInt(m_border_color.a * 255f).ToString();

        SeaBorderFields[0].text = Mathf.RoundToInt(m_sea_border_color.r * 255f).ToString();
        SeaBorderFields[1].text = Mathf.RoundToInt(m_sea_border_color.g * 255f).ToString();
        SeaBorderFields[2].text = Mathf.RoundToInt(m_sea_border_color.b * 255f).ToString();
        SeaBorderFields[3].text = Mathf.RoundToInt(m_sea_border_color.a * 255f).ToString();
    }

    public void OnOverlayColorUpdate()
    {
        var red = 255f;
        var green = 0f;
        var blue = 0f;
        var a = 55f;

        float.TryParse(OverlayFields[0].text, out red);
        float.TryParse(OverlayFields[1].text, out green);
        float.TryParse(OverlayFields[2].text, out blue);
        float.TryParse(OverlayFields[3].text, out a);

        red = Mathf.Clamp(red, 0f, 255f) / 255f;
        green = Mathf.Clamp(green, 0f, 255f) / 255f;
        blue = Mathf.Clamp(blue, 0f, 255f) / 255f;
        a = Mathf.Clamp(a, 0f, 255f) / 255f;

        m_overlay_color = new Color(red, green, blue, a);

        OverlayPreview.color = m_overlay_color;
    }

    public void OnBorderColorUpdate()
    {
        var red = 0f;
        var green = 0f;
        var blue = 0f;
        var a = 55f;

        float.TryParse(BorderFields[0].text, out red);
        float.TryParse(BorderFields[1].text, out green);
        float.TryParse(BorderFields[2].text, out blue);
        float.TryParse(BorderFields[3].text, out a);

        red = Mathf.Clamp(red, 0f, 255f) / 255f;
        green = Mathf.Clamp(green, 0f, 255f) / 255f;
        blue = Mathf.Clamp(blue, 0f, 255f) / 255f;
        a = Mathf.Clamp(a, 0f, 255f) / 255f;

        m_border_color = new Color(red, green, blue, a);

        BorderPreview.color = m_border_color;
    }

    public void OnSeaBorderColorUpdate()
    {
        var red = 0f;
        var green = 0f;
        var blue = 0f;
        var a = 55f;

        float.TryParse(SeaBorderFields[0].text, out red);
        float.TryParse(SeaBorderFields[1].text, out green);
        float.TryParse(SeaBorderFields[2].text, out blue);
        float.TryParse(SeaBorderFields[3].text, out a);

        red = Mathf.Clamp(red, 0f, 255f) / 255f;
        green = Mathf.Clamp(green, 0f, 255f) / 255f;
        blue = Mathf.Clamp(blue, 0f, 255f) / 255f;
        a = Mathf.Clamp(a, 0f, 255f) / 255f;

        m_sea_border_color = new Color(red, green, blue, a);

        SeaBorderPreview.color = m_sea_border_color;
    }

    public void OnCluster()
    {
        m_cluster_water = !m_cluster_water;
    }

    public void OnClusterIslands()
    {
        m_cluster_islands = !m_cluster_islands;
    }

    public void OnGeneric()
    {
        m_generic_starts = !m_generic_starts;

        update_nations();
    }

    public void OnTeamplay()
    {
        m_teamplay = !m_teamplay;

        update_nations();
    }

    public void OnHideOptions()
    {
        var enabled = HideableOptions[0].activeSelf;

        foreach (var obj in HideableOptions)
        {
            obj.SetActive(!enabled);
        }

        GetComponent<AudioSource>().PlayOneShot(ClickAudio);
    }

    private void hide_controls()
    {
        foreach (var obj in HideableControls)
        {
            obj.SetActive(false);
        }
    }

    public void OnPlayerCountChanged(Dropdown d)
    {
        var str = d.captionText.text;
        var trim = str.Replace(" Players", string.Empty);
        var players = 2;
        int.TryParse(trim, out players);

        m_player_count = players;

        update_nations();
    }

    public void OnAgeChanged(Dropdown d)
    {
        var str = d.captionText.text;

        if (str == "Middle Ages")
        {
            m_age = Age.MIDDLE;
        }
        else if (str == "Early Ages")
        {
            m_age = Age.EARLY;
        }
        else if (str == "Late Ages")
        {
            m_age = Age.LATE;
        }
        else
        {
            m_age = Age.ALL;
        }

        foreach (var obj in m_content)
        {
            GameObject.Destroy(obj);
        }

        m_content = new List<GameObject>();

        update_nations();
    }

    private void populate_nations(Dropdown d, int i)
    {
        var list = AllNationData.AllNations.Where(x => (x.Age == m_age || m_age == Age.ALL) || x.ID == -1);

        if (m_generic_starts)
        {
            list = AllNationData.AllNations.Where(x => x.ID == -1);
            i = 0;
        }

        d.options.Clear();

        foreach (var nd in list)
        {
            d.options.Add(new Dropdown.OptionData(nd.Name));
        }

        d.value = -1; // hard reset the value with this trick
        d.value = i;
    }

    private void update_nations()
    {
        var list = new List<Dropdown.OptionData>();

        foreach (var layout in m_layouts.Layouts.Where(x => x.NumPlayers == m_player_count))
        {
            var od = new Dropdown.OptionData(layout.Name);
            list.Add(od);
        }

        LayoutDropdown.ClearOptions();
        LayoutDropdown.AddOptions(list);
        LayoutDropdown.value = 0;

        while (m_content.Count > m_player_count)
        {
            var obj = m_content[m_content.Count - 1];
            m_content.RemoveAt(m_content.Count - 1);

            GameObject.Destroy(obj);
        }

        var tf = ScrollPanel.GetComponent<RectTransform>();
        tf.sizeDelta = new Vector2(290f, 2f + m_player_count * 34f);

        for (var i = 0; i < m_player_count; i++)
        {
            if (m_content.Count > i)
            {
                var obj = m_content[i];
                var rt = obj.GetComponent<RectTransform>();
                var np = obj.GetComponent<NationPicker>();
                np.Initialize();
                np.SetTeamplay(m_teamplay);

                populate_nations(np.NationDropdown, 0);

                rt.localPosition = new Vector3(0, -17 - (i * 34), 0);
                rt.sizeDelta = new Vector2(0, 34);
                rt.anchoredPosition = new Vector2(0, -17 - (i * 34));
            }
            else
            {
                var cnt = GameObject.Instantiate(NationPicker);
                var rt = cnt.GetComponent<RectTransform>();
                var np = cnt.GetComponent<NationPicker>();
                np.Initialize();
                np.SetTeamplay(m_teamplay);

                populate_nations(np.NationDropdown, 0);

                rt.SetParent(tf);
                rt.localPosition = new Vector3(0, -17 - (i * 34), 0);
                rt.sizeDelta = new Vector2(0, 34);
                rt.anchoredPosition = new Vector2(0, -17 - (i * 34));

                m_content.Add(cnt);
            }
        }
    }

    private void load_nation_data()
    {
        var folder = Application.dataPath + "/Nations/";

        foreach (var file in Directory.GetFiles(folder))
        {
            if (file.Contains(".meta"))
            {
                continue;
            }

            var contents = File.ReadAllText(file);
            var serializer = new XmlSerializer(typeof(NationCollection));
            NationCollection result;

            using (TextReader reader = new StringReader(contents))
            {
                result = (NationCollection)serializer.Deserialize(reader);
            }

            AllNationData.AddNations(result);
        }

        AllNationData.SortNations();
    }

    private void load_name_data()
    {
        m_name_data = new CustomNameDataCollection();
        m_name_formats = new CustomNameFormatCollection();

        var data_folder = Application.dataPath;
        var folder = data_folder + "/NameData/";

        foreach (var file in Directory.GetFiles(folder))
        {
            if (file.Contains(".meta"))
            {
                continue;
            }

            var contents = File.ReadAllText(file);
            var serializer = new XmlSerializer(typeof(CustomNameDataCollection));
            CustomNameDataCollection result;

            using (TextReader reader = new StringReader(contents))
            {
                result = (CustomNameDataCollection)serializer.Deserialize(reader);
            }

            m_name_data.Add(result);
        }

        folder = data_folder + "/NameFormats/";

        foreach (var file in Directory.GetFiles(folder))
        {
            if (file.Contains(".meta"))
            {
                continue;
            }

            var contents = File.ReadAllText(file);
            var serializer = new XmlSerializer(typeof(CustomNameFormatCollection));
            CustomNameFormatCollection result;

            using (TextReader reader = new StringReader(contents))
            {
                result = (CustomNameFormatCollection)serializer.Deserialize(reader);
            }

            m_name_formats.Add(result);
        }
    }

    private void load_layouts()
    {
        m_layouts = new NodeLayoutCollection();

        var data_folder = Application.dataPath;
        var folder = data_folder + "/Layouts/";

        foreach (var file in Directory.GetFiles(folder))
        {
            if (file.Contains(".meta"))
            {
                continue;
            }

            var contents = File.ReadAllText(file);
            var serializer = new XmlSerializer(typeof(NodeLayoutCollection));
            NodeLayoutCollection result;

            using (TextReader reader = new StringReader(contents))
            {
                result = (NodeLayoutCollection)serializer.Deserialize(reader);
            }

            m_layouts.Add(result);
        }
    }
}
