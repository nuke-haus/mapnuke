using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager class that handles all things art-related.
/// Has a global singleton.
/// </summary>
public class ArtManager : MonoBehaviour
{
    public static ArtManager s_art_manager;

    public CaptureCam CaptureCam;
    public GameObject RenderPlane;
    public GameObject EditorPlane;
    public Material RenderMat;
    public Dropdown ArtStyleDropdown;
    public ArtConfiguration CurrentArtConfiguration;
    public List<ArtConfiguration> ArtConfigurations;
    private ArtStyle m_art;
    private RenderTexture m_render_texture;

    public RenderTexture Texture
    {
        get
        {
            CaptureCam.Render();
            RenderTexture.active = m_render_texture;

            return m_render_texture;
        }
    }

    public void OnArtStyleDropdownValueChanged(Dropdown d)
    {
        var str = d.captionText.text;
        var art_config = ArtConfigurations.FirstOrDefault(config => config.ArtConfigurationName == str);

        if (art_config != null)
        {
            CurrentArtConfiguration = art_config;
        }
    }

    private void Awake()
    {
        s_art_manager = this;
        m_art = new DefaultArtStyle();

        foreach (var art_config in ArtConfigurations)
        {
            var data = new Dropdown.OptionData(art_config.ArtConfigurationName);
            ArtStyleDropdown.options.Add(data);
        }

        ArtStyleDropdown.value = 0;
    }

    public bool JustChangedSeason
    {
        get
        {
            return m_art.JustChangedSeason;
        }
    }

    public void ChangeSeason(Season s)
    {
        m_art.ChangeSeason(s);
    }

    public ProvinceSprite GetProvinceSprite(Terrain flags)
    {
        return CurrentArtConfiguration.GetMapSprite(flags);
    }

    public MapSpriteSet GetMapSpriteSet(Terrain flags)
    {
        return CurrentArtConfiguration.GetMapSpriteSet(flags);
    }

    public ConnectionSprite GetConnectionSprite(ConnectionType flags)
    {
        return CurrentArtConfiguration.GetConnectionSprite(flags);
    }

    public ConnectionSprite GetMountainSpecSprite()
    {
        return CurrentArtConfiguration.GetMountainSpecSprite();
    }

    public IEnumerator GenerateElements(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout, float x, float y)
    {
        setup_cam(x, y);

        return m_art.Generate(provs, conns, layout);// todo: allow the user to pick which art style?
    }

    public void RegenerateElements(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout) // totally regen these provinces and their connections
    {
        m_art.Regenerate(provs, conns, layout);
    }

    private void setup_cam(float x, float y)
    {
        var tx = Mathf.RoundToInt(x * 100);
        var ty = Mathf.RoundToInt(y * 100);

        m_render_texture = new RenderTexture(tx, ty, 24);

        var mins = MapBorder.s_map_border.Mins;
        var maxs = MapBorder.s_map_border.Maxs;
        maxs.y = mins.y;

        var maxs2 = MapBorder.s_map_border.Maxs;
        maxs2.x = mins.x;

        var dist = Vector3.Distance(mins, maxs);
        var vert_dist = Vector3.Distance(mins, maxs2);
        var side_dist = 500f;
        var sep_dist = 5f;

        EditorPlane.transform.position = new Vector3(mins.x + side_dist + (dist * 0.5f), mins.y + (vert_dist * 0.5f), 0);
        EditorPlane.transform.localScale = new Vector3(x * 0.1f, RenderPlane.transform.localScale.y, y * 0.1f);

        RenderPlane.transform.position = EditorPlane.transform.position + new Vector3(dist + sep_dist, 0f, 0f);
        RenderPlane.transform.localScale = new Vector3(x * 0.1f, RenderPlane.transform.localScale.y, y * 0.1f);

        RenderMat.mainTexture = m_render_texture;
        CaptureCam.Camera.targetTexture = m_render_texture;
    }
}
