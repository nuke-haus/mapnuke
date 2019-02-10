using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager class that handles all user input and generally is the entry point for a lot of logic.
/// Has a global singleton.
/// </summary>
public class GenerationManager : MonoBehaviour
{
    public static GenerationManager s_generation_manager;

    public Camera CaptureCam;
    public Toggle NatStarts;
    public AudioClip AcceptAudio;
    public AudioClip DenyAudio;
    public GameObject OutputWindow;
    public GameObject LoadingScreen;
    public InputField MapName;
    public GameObject NationPicker;
    public GameObject ScrollContent;
    public GameObject Logo;
    public GameObject LogScreen;
    public GameObject LogContent;
    public GameObject[] HideableOptions;
    public GameObject[] HideableControls;

    bool m_cluster_water = true;
    bool m_teamplay = false;
    int m_players = 9;
    Age m_age = Age.EARLY;
    Season m_season = Season.SUMMER;
    List<GameObject> m_log_content;
    List<GameObject> m_content;
    List<NationData> m_nations;

    public Season Season
    {
        get
        {
            return m_season;
        }
    }

    public List<NationData> NationData
    {
        get
        {
            return m_nations;
        }
    }

    void Start()
    {
        AllNationData.Init();

        s_generation_manager = this;
        m_content = new List<GameObject>();
        m_log_content = new List<GameObject>();

        update_nations();
        hide_controls();
    }

    void Update()
    {

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
        if (m_teamplay && m_content.Count % 2 == 1)
        {
            GetComponent<AudioSource>().PlayOneShot(DenyAudio);
            return;
        }

        if (Logo != null && Logo.activeSelf)
        {
            Logo.SetActive(false);
        }

        List<NationData> picks = new List<NationData>();

        foreach (GameObject obj in m_content)
        {
            Dropdown d = obj.GetComponent<Dropdown>();
            string str = d.options[d.value].text;
/*#if DEBUG
            // nvm
#else
            if (str == "CHOOSE NATION")
            {
                return;
            }
#endif*/
            NationData data = AllNationData.AllNations.FirstOrDefault(x => x.Name == str);

            if (data == null || picks.Contains(data))
            {
                Debug.Log("Bad nation name: " + str);
                GetComponent<AudioSource>().PlayOneShot(DenyAudio);
                return;
            }

            picks.Add(data);
        }

        m_nations = picks;

        GetComponent<AudioSource>().PlayOneShot(AcceptAudio);

        StartCoroutine(perform_async(() => do_generate(), true));
    }

    void do_generate() // pipeline for initial generation of all nodes and stuff
    {
        m_season = Season.SUMMER;

        // create the conceptual nodes and connections first
        WorldGenerator.GenerateWorld(m_teamplay, m_cluster_water, NatStarts.isOn, m_nations);
        List<Connection> conns = WorldGenerator.GetConnections();
        List<Node> nodes = WorldGenerator.GetNodes();
        NodeLayout layout = WorldGenerator.GetLayout();

        // generate the unity objects using the conceptual nodes
        ElementManager mgr = GetComponent<ElementManager>();
        mgr.GenerateElements(nodes, conns, layout);

        ProvinceManager.s_province_manager.SetLayout(layout);
        ConnectionManager.s_connection_manager.SetLayout(layout);

        // position and resize the cameras
        Vector3 campos = new Vector3(layout.X * 0.5f * mgr.X - mgr.X, layout.Y * 0.5f * mgr.Y - mgr.Y, -10);
        Camera.main.transform.position = campos;
        CaptureCam.transform.position = campos;

        float ortho = (mgr.Y * layout.Y * 100) / 100f / 2f;
        CaptureCam.orthographicSize = ortho;
    }

    void do_regen(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout) 
    {
        ArtManager.s_art_manager.RegenerateElements(provs, conns, layout);
    }

    public void RegenerateElements(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout)
    {
        StartCoroutine(perform_async(() => do_regen(provs, conns, layout)));
    }

    IEnumerator perform_async(System.Action function, bool show_log = false)
    {
        LoadingScreen.SetActive(true);

        if (show_log)
        {
            //LogScreen.SetActive(true); // commented out cuz it doesn't work well
            //ClearLog();
        }

        yield return null; 
        yield return null; 

        if (function != null)
        {
            function();
        }

        LoadingScreen.SetActive(false);
        //LogScreen.SetActive(false);
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

        GetComponent<AudioSource>().PlayOneShot(AcceptAudio);

        StartCoroutine(perform_async(() => do_season_change()));
    }

    void do_season_change()
    {
        ArtManager.s_art_manager.ChangeSeason(m_season);
    }

    public void ShowOutputWindow()
    {
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
        GetComponent<AudioSource>().PlayOneShot(AcceptAudio);

        StartCoroutine(output_async(str));
    }

    IEnumerator output_async(string str, bool show_log = false)
    {
        LoadingScreen.SetActive(true);

        if (show_log)
        {
            //LogScreen.SetActive(true); // commented out cuz it doesn't work well
            //ClearLog();
        }

        yield return null;

        ElementManager mgr = GetComponent<ElementManager>();
        NodeLayout layout = WorldGenerator.GetLayout();

        MapFileWriter.GenerateText(str, layout, mgr, m_nations, new Vector2(-mgr.X, -mgr.Y), new Vector2(mgr.X * (layout.X - 1), mgr.Y * (layout.Y - 1)), mgr.Provinces);

        yield return null;

        MapFileWriter.GenerateImage(str, mgr.Texture); // summer

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

        MapFileWriter.GenerateImage(str + "winter", mgr.Texture); // winter

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

    public void OnCluster()
    {
        m_cluster_water = !m_cluster_water;
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

        m_players = players;

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
        var list = AllNationData.AllNations.Where(x => x.Age == m_age || m_age == Age.ALL || x.ID == -1);

        foreach (NationData nd in list)
        {
            d.options.Add(new Dropdown.OptionData(nd.Name));
        }

        d.value = i;
    }

    void update_nations()
    {
        while (m_content.Count > m_players)
        {
            GameObject obj = m_content[m_content.Count - 1];
            m_content.RemoveAt(m_content.Count - 1);

            GameObject.Destroy(obj);
        }

        int counter = 1;

        for (int i = 0; i < m_players; i++)
        {
            if (m_content.Count > i)
            {
                GameObject obj = m_content[i];
                RectTransform rt = obj.GetComponent<RectTransform>();
                Dropdown d = obj.GetComponent<Dropdown>();

                rt.localPosition = new Vector3(0, -15 - (i * 30), 0);
                rt.sizeDelta = new Vector2(0, 30);
                rt.anchoredPosition = new Vector2(0, -15 - (i * 30));

                if (m_teamplay)
                {
                    if (counter < 3)
                    {
                        ColorBlock block = d.colors;
                        block.normalColor = new Color(0.9f, 0.7f, 0.7f);
                        block.highlightedColor = new Color(0.9f, 0.7f, 0.8f);
                        d.colors = block;
                    }
                    else if (counter < 5)
                    {
                        ColorBlock block = d.colors;
                        block.normalColor = new Color(0.7f, 0.7f, 0.9f);
                        block.highlightedColor = new Color(0.7f, 0.8f, 0.9f);
                        d.colors = block;
                    }

                    counter++;

                    if (counter >= 5)
                    {
                        counter = 1;
                    }
                }
                else
                {
                    ColorBlock block = d.colors;
                    block.normalColor = new Color(0.7f, 0.7f, 0.9f);
                    block.highlightedColor = new Color(0.7f, 0.8f, 0.9f);
                    d.colors = block;
                }
            }
            else
            {
                GameObject cnt = GameObject.Instantiate(NationPicker);
                RectTransform rt = cnt.GetComponent<RectTransform>();
                Dropdown d = cnt.GetComponent<Dropdown>();

                populate_nations(d, i);

                rt.SetParent(ScrollContent.GetComponent<RectTransform>());
                rt.localPosition = new Vector3(0, -15 - (i * 30), 0);
                rt.sizeDelta = new Vector2(0, 30);
                rt.anchoredPosition = new Vector2(0, -15 - (i * 30));

                if (m_teamplay)
                {
                    if (counter < 3)
                    {
                        ColorBlock block = d.colors;
                        block.normalColor = new Color(0.9f, 0.7f, 0.7f);
                        block.highlightedColor = new Color(0.9f, 0.7f, 0.8f);
                        d.colors = block;
                    }
                    else if (counter < 5)
                    {
                        ColorBlock block = d.colors;
                        block.normalColor = new Color(0.7f, 0.7f, 0.9f);
                        block.highlightedColor = new Color(0.7f, 0.8f, 0.9f);
                        d.colors = block;
                    }

                    counter++;

                    if (counter >= 5)
                    {
                        counter = 1;
                    }
                }
                else
                {
                    ColorBlock block = d.colors;
                    block.normalColor = new Color(0.7f, 0.7f, 0.9f);
                    block.highlightedColor = new Color(0.7f, 0.8f, 0.9f);
                    d.colors = block;
                }

                m_content.Add(cnt);
            }
        }
    }
}
