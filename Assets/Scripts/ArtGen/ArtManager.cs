using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager class that handles all things art-related.
/// Has a global singleton.
/// </summary>
public class ArtManager: MonoBehaviour
{
    public static ArtManager s_art_manager;

    public Camera CaptureCam;
    public List<RenderTexture> RenderTextures;
    public GameObject RenderPlane;
    public Material RenderMat;
    public SpriteSetCollection SpriteSetCollection;

    RenderTexture m_current;
    ArtStyle m_art;

    public RenderTexture Texture
    {
        get
        {
            CaptureCam.Render();
            RenderTexture.active = m_current;

            return m_current;
        }
    }

    void Awake()
    {
        s_art_manager = this;
        m_art = new DefaultArtStyle();
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
        return SpriteSetCollection.GetMapSprite(flags);
    }

    public MapSpriteSet GetMapSpriteSet(Terrain flags)
    {
        return SpriteSetCollection.GetMapSpriteSet(flags);
    }

    public ConnectionSprite GetConnectionSprite(ConnectionType flags)
    {
        return SpriteSetCollection.GetConnectionSprite(flags);
    }

    public List<GameObject> GenerateElements(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout, float x, float y)
    {
        setup_cam(x, y);

        return m_art.Generate(provs, conns, layout);// todo: allow the user to pick which art style?
    }

    public void RegenerateElements(List<ProvinceMarker> provs, List<ConnectionMarker> conns, NodeLayout layout) // totally regen these provinces and their connections
    {
        m_art.Regenerate(provs, conns, layout);
    }

    void setup_cam(float x, float y)
    {
        int tx = Mathf.RoundToInt(x * 100);
        int ty = Mathf.RoundToInt(y * 100);
        RenderTexture rend = RenderTextures.FirstOrDefault(rt => rt.width == tx && rt.height == ty && rt.name.Contains(GenerationManager.s_generation_manager.NationData.Count.ToString()));

        //RenderPlane.transform.localScale.Set(x, RenderPlane.transform.localScale.y, y);
      
        Vector3 mins = MapBorder.s_map_border.Mins;
        Vector3 maxs = MapBorder.s_map_border.Maxs;
        maxs.y = mins.y;

        Vector3 maxs2 = MapBorder.s_map_border.Maxs;
        maxs2.x = mins.x;

        float dist = Vector3.Distance(mins, maxs);
        float vert_dist = Vector3.Distance(mins, maxs2);

        RenderPlane.transform.position = new Vector3(mins.x + 1f + (dist * 1.5f), mins.y + (vert_dist * 0.5f), 0);
        RenderPlane.transform.localScale = new Vector3(x * 0.1f, RenderPlane.transform.localScale.y, y * 0.1f);

        if (rend == null)
        {
            rend = RenderTextures[0];
        }

        RenderMat.mainTexture = rend;
        CaptureCam.targetTexture = rend;
        m_current = rend;
    }
}
