using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;
using System;

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
    public Image OverlayPreview;
    public Image BorderPreview;

    public MeshRenderer province_id_mesh_prefab;
    GameObject province_id_map_container;

    Color m_border_color = new Color();
    Color m_overlay_color = new Color();
    bool m_generic_starts = false;
    bool m_cluster_water = true;
    bool m_teamplay = false;
    int m_player_count = 9;
    Age m_age = Age.EARLY;
    Season m_season = Season.SUMMER;
    List<GameObject> m_log_content;
    List<GameObject> m_content;
    List<PlayerData> m_nations;
    NodeLayoutCollection m_layouts;

    public Color BorderColor => m_border_color;
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

    void Start()
    {
        AllNationData.Init();
        GeneratorSettings.Initialize();

        s_generation_manager = this;
        m_content = new List<GameObject>();
        m_log_content = new List<GameObject>();

        load_layouts();
        load_nation_data();
        update_nations();
        hide_controls();
        OnBorderColorUpdate();
        OnOverlayColorUpdate();
    }

    void Update()
    {
        Util.ResetFrameTime();
    }

    public void LogText(string text)
    {
        StartCoroutine(do_log(text));
    }

    IEnumerator do_log(string text)
    {
        yield return null;

        int pos = m_log_content.Count + 1;

        GameObject obj = GameObject.Instantiate(LogContent);
        RectTransform rt = obj.GetComponent<RectTransform>();
        UnityEngine.UI.Text txt = obj.GetComponent<UnityEngine.UI.Text>();

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
        foreach (GameObject g in m_log_content)
        {
            GameObject.Destroy(g);
        }

        m_log_content = new List<GameObject>();
    }

    public void OnGenerate()
    {
        if (ElementManager.s_element_manager.GeneratedObjects.Any())
        {
            ElementManager.s_element_manager.WipeGeneratedObjects();
        }

        List<PlayerData> picks = new List<PlayerData>();

        foreach (GameObject obj in m_content)
        {
            NationPicker np = obj.GetComponent<NationPicker>();
            string str = np.NationName;

            NationData data = AllNationData.AllNations.FirstOrDefault(x => x.Name == str);
            PlayerData pd = new PlayerData(data, np.TeamNum);

            if (data == null || (picks.Any(x => x.NationData.Name == data.Name) && !m_generic_starts))
            {
                GetComponent<AudioSource>().PlayOneShot(DenyAudio);
                return;
            }

            picks.Add(pd);
        }

        m_nations = picks;

        GetComponent<AudioSource>().PlayOneShot(AcceptAudio);
        NodeLayout layout = m_layouts.Layouts.FirstOrDefault(x => x.Name == LayoutDropdown.options[LayoutDropdown.value].text && x.NumPlayers == m_player_count);
        MoveCameraForGeneration(layout);
        Resources.UnloadUnusedAssets();
        StartCoroutine(perform_async(() => do_generate(layout), true));
    }

    void MoveCameraForGeneration(NodeLayout layout)
    {
        var main_cam = Camera.main;
        var p = main_cam.transform.position;
        p.x = layout.X;
        p.y = layout.Y * 0.5f;
        main_cam.transform.position = p;
    }

   IEnumerator do_generate(NodeLayout layout) // pipeline for initial generation of all nodes and stuff
    {
        foreach (GameObject obj in HideableButtons)
        {
            obj.SetActive(false);
        }

        if (layout == null)
        {
            layout = m_layouts.Layouts.FirstOrDefault(x => x.NumPlayers == m_player_count);
        }

        m_season = Season.SUMMER;

        // create the conceptual nodes and connections first
        WorldGenerator.GenerateWorld(m_teamplay, m_cluster_water, NatStarts.isOn, m_nations, layout);
        List<Connection> conns = WorldGenerator.GetConnections();
        List<Node> nodes = WorldGenerator.GetNodes();

        // generate the unity objects using the conceptual nodes
        ElementManager mgr = GetComponent<ElementManager>();

        // position and resize the cameras
        Vector3 campos = new Vector3(layout.X * 0.5f * mgr.X - mgr.X, layout.Y * 0.5f * mgr.Y - mgr.Y, -10);
        CaptureCamera.transform.position = campos;

        float ortho = (mgr.Y * layout.Y * 100) / 100f / 2f;
        CaptureCamera.orthographicSize = ortho;

        yield return StartCoroutine(mgr.GenerateElements(nodes, conns, layout));

        ProvinceManager.s_province_manager.SetLayout(layout);
        ConnectionManager.s_connection_manager.SetLayout(layout);
        Camera.main.transform.position = campos + new Vector3(500f, 0f, 0f);

        foreach (GameObject obj in HideableButtons)
        {
            obj.SetActive(true);
        }
    }

    void do_regen(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout) 
    {
        ArtManager.s_art_manager.RegenerateElements(provs, conns, layout);
    }

    public void RegenerateElements(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout)
    {
        Resources.UnloadUnusedAssets();
        StartCoroutine(perform_async(() => do_regen(provs, conns, layout)));
    }

   IEnumerator perform_async(System.Func<IEnumerator> function, bool show_log = false) {
      LoadingScreen.SetActive(true);

      if (show_log) {
         //LogScreen.SetActive(true); 
         //ClearLog();
      }

      yield return null;
      yield return new WaitUntil(() => LoadingScreen.activeInHierarchy);
      float start_time = Time.realtimeSinceStartup;
      if (function != null) {
         yield return StartCoroutine(function());
      }
      float total_time = Time.realtimeSinceStartup - start_time;
      Debug.LogFormat("Generation time: {0}", total_time);

      LoadingScreen.SetActive(false);
      //LogScreen.SetActive(false);
   }

   IEnumerator perform_async(System.Action function, bool show_log = false)
    {
        LoadingScreen.SetActive(true);

        if (show_log)
        {
            //LogScreen.SetActive(true); 
            //ClearLog();
        }

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
        if (m_season == Season.SUMMER)
        {
            m_season = Season.WINTER;
        }
        else
        {
            m_season = Season.SUMMER;
        }

        GetComponent<AudioSource>().PlayOneShot(ClickAudio);

        StartCoroutine(perform_async(() => do_season_change()));
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }

    void do_season_change()
    {
        ArtManager.s_art_manager.ChangeSeason(m_season);
    }

    public void ShowOutputWindow()
    {
        GetComponent<AudioSource>().PlayOneShot(ClickAudio);

        if (!OutputWindow.activeSelf)
        {
            OutputWindow.SetActive(true);
        }
    }

    public void GenerateOutput()
    {
        string str = MapName.text;

        if (string.IsNullOrEmpty(str))
        {
            return;
        }

        OutputWindow.SetActive(false);
        GetComponent<AudioSource>().PlayOneShot(ClickAudio);

        StartCoroutine(output_async(str));
    }

    void MakeProvinceIdTexture(ProvinceMarker pm, Vector3 offset, Transform t)
    {
        Bounds cam_bounds = CaptureCam.Bounds(offset);

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
        ElementManager mgr = GetComponent<ElementManager>();
        province_id_map_container = new GameObject("ProvinceIdMapContainer");
        Transform tr = province_id_map_container.transform;
        foreach (var prov in mgr.m_provinces)
        {
            MakeProvinceIdTexture(prov, offset, tr);
        }

        CaptureCam.s_capture_cam.transform.position += offset;

        CaptureCam.s_capture_cam.Camera.Render();

        CaptureCam.s_capture_cam.transform.position -= offset;
        var tex = mgr.Texture;
        Texture2D t = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);


        Color32[] pixels = t.GetPixels32();
        Destroy(t);
        Destroy(province_id_map_container);

        int[] province_ids = new int[pixels.Length];
        for (int i = 0; i < pixels.Length; ++i)
        {
            Color32 p = pixels[i];
            province_ids[i] = p.b + p.g * 256;
        }
        return province_ids;
    }

    IEnumerator output_async(string str, bool show_log = false)
    {
        LoadingScreen.SetActive(true);

        if (show_log)
        {
            //LogScreen.SetActive(true); 
            //ClearLog();
        }

        yield return null;
        yield return new WaitUntil(() => LoadingScreen.activeInHierarchy);

        ElementManager mgr = GetComponent<ElementManager>();
        NodeLayout layout = WorldGenerator.GetLayout();
        int[] province_ids = GetProvinceIdVals(new Vector3(0, 60, 0));

        MapFileWriter.GenerateText(str, layout, mgr, m_nations, new Vector2(-mgr.X, -mgr.Y), new Vector2(mgr.X * (layout.X - 1), mgr.Y * (layout.Y - 1)), mgr.Provinces, m_teamplay, province_ids);

        yield return null;

        MapFileWriter.GenerateImage(str, mgr.Texture); // summer

        mgr.ShowLabels(true);

        yield return null;

        MapFileWriter.GenerateImage(str + "_with_labels", mgr.Texture, false); // labeled image

        mgr.ShowLabels(false);

        if (m_season == Season.SUMMER)
        {
            m_season = Season.WINTER;
        }
        else
        {
            m_season = Season.SUMMER;
        }

        do_season_change();

        yield return new WaitUntil(() => ArtManager.s_art_manager.JustChangedSeason);
        yield return new WaitForEndOfFrame(); // possibly not needed

        ArtManager.s_art_manager.CaptureCam.Render(); // possibly not needed

        yield return new WaitForEndOfFrame(); // possibly not needed

        MapFileWriter.GenerateImage(str + "_winter", mgr.Texture); // winter

        if (m_season == Season.SUMMER)
        {
            m_season = Season.WINTER;
        }
        else
        {
            m_season = Season.SUMMER;
        }

        do_season_change();

        GetComponent<AudioSource>().PlayOneShot(AcceptAudio);

        LoadingScreen.SetActive(false);
        //LogScreen.SetActive(false);
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
        var red = 255f;
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

    public void OnCluster()
    {
        m_cluster_water = !m_cluster_water;
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
        GameObject o = HideableOptions[0];

        if (o.activeSelf)
        {
            foreach (GameObject obj in HideableOptions)
            {
                obj.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject obj in HideableOptions)
            {
                obj.SetActive(true);
            }
        }

        GetComponent<AudioSource>().PlayOneShot(ClickAudio);
    }

    void hide_controls()
    {
        GameObject o = HideableControls[0];

        if (o.activeSelf)
        {
            foreach (GameObject obj in HideableControls)
            {
                obj.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject obj in HideableControls)
            {
                obj.SetActive(true);
            }
        }
    }

    public void OnPlayerCountChanged(Dropdown d)
    {
        string str = d.captionText.text;
        string trim = str.Replace(" Players", string.Empty);
        int players = 2;
        int.TryParse(trim, out players);

        m_player_count = players;

        update_nations();
    }

    public void OnAgeChanged(Dropdown d)
    {
        string str = d.captionText.text;

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

        foreach (GameObject obj in m_content)
        {
            GameObject.Destroy(obj);
        }

        m_content = new List<GameObject>();

        update_nations();
    }

    void populate_nations(Dropdown d, int i)
    {
        var list = AllNationData.AllNations.Where(x => (x.Age == m_age || m_age == Age.ALL) && x.ID != -1);

        if (m_generic_starts)
        {
            list = AllNationData.AllNations.Where(x => x.ID == -1);
            i = 0;
        }

        d.options.Clear();

        foreach (NationData nd in list)
        {
            d.options.Add(new Dropdown.OptionData(nd.Name));
        }

        d.value = -1; // hard reset the value with this trick
        d.value = i;
    }

    void update_nations()
    {
        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();

        foreach (NodeLayout layout in m_layouts.Layouts.Where(x => x.NumPlayers == m_player_count))
        {
            Dropdown.OptionData od = new Dropdown.OptionData(layout.Name);
            list.Add(od);
        }

        LayoutDropdown.ClearOptions();
        LayoutDropdown.AddOptions(list);
        LayoutDropdown.value = 0;

        while (m_content.Count > m_player_count)
        {
            GameObject obj = m_content[m_content.Count - 1];
            m_content.RemoveAt(m_content.Count - 1);

            GameObject.Destroy(obj);
        }

        RectTransform tf = ScrollPanel.GetComponent<RectTransform>();
        tf.sizeDelta = new Vector2(247f, 2f + m_player_count * 34f);

        for (int i = 0; i < m_player_count; i++)
        {
            if (m_content.Count > i)
            {
                GameObject obj = m_content[i];
                RectTransform rt = obj.GetComponent<RectTransform>();
                NationPicker np = obj.GetComponent<NationPicker>();
                np.Initialize();
                np.SetTeamplay(m_teamplay);

                populate_nations(np.NationDropdown, i);

                rt.localPosition = new Vector3(0, -17 - (i * 34), 0);
                rt.sizeDelta = new Vector2(0, 34);
                rt.anchoredPosition = new Vector2(0, -17 - (i * 34));
            }
            else
            {
                GameObject cnt = GameObject.Instantiate(NationPicker);
                RectTransform rt = cnt.GetComponent<RectTransform>();
                NationPicker np = cnt.GetComponent<NationPicker>();
                np.Initialize();
                np.SetTeamplay(m_teamplay);

                populate_nations(np.NationDropdown, i);

                rt.SetParent(tf);
                rt.localPosition = new Vector3(0, -17 - (i * 34), 0);
                rt.sizeDelta = new Vector2(0, 34);
                rt.anchoredPosition = new Vector2(0, -17 - (i * 34));

                m_content.Add(cnt);
            }
        }
    }

    void load_nation_data()
    {
        string data_folder = Application.dataPath;
        string folder = data_folder + "/Nations/";

        foreach (string file in Directory.GetFiles(folder))
        {
            if (file.Contains(".meta"))
            {
                continue;
            }

            string contents = File.ReadAllText(file);
            var serializer = new XmlSerializer(typeof(NationCollection));
            NationCollection result;

            using (TextReader reader = new StringReader(contents))
            {
                result = (NationCollection)serializer.Deserialize(reader);
            }

            AllNationData.AddNations(result);
        }
    }

    void load_layouts()
    {
        m_layouts = new NodeLayoutCollection();

        string data_folder = Application.dataPath;
        string folder = data_folder + "/Layouts/";

        foreach (string file in Directory.GetFiles(folder))
        {
            if (file.Contains(".meta"))
            {
                continue;
            }

            string contents = File.ReadAllText(file);
            var serializer = new XmlSerializer(typeof(NodeLayoutCollection));
            NodeLayoutCollection result;

            using (TextReader reader = new StringReader(contents))
            {
                result = (NodeLayoutCollection) serializer.Deserialize(reader);
            }

            m_layouts.Add(result);
        }
    }
}
