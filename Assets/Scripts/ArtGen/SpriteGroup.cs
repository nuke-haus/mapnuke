using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteGroup: MonoBehaviour
{
    public MeshSpriteRenderer MeshSpritePrefab;

    private MeshSpriteRenderer m_mesh_sprites;
    private List<GameObject> m_mesh_sprites_dupes = new List<GameObject>();
    private MeshSpriteRenderer m_winter_mesh_sprites;
    private List<GameObject> m_winter_mesh_sprites_dupes = new List<GameObject>();

    private void Awake()
    {
        m_mesh_sprites = Instantiate(MeshSpritePrefab);
        m_mesh_sprites.transform.position = Vector3.zero;
        m_winter_mesh_sprites = Instantiate(MeshSpritePrefab);
        m_winter_mesh_sprites.transform.position = Vector3.zero;
    }

    public void SetSeason(Season season)
    {
        if (m_mesh_sprites != null)
        {
            m_mesh_sprites.gameObject.SetActive(season == Season.SUMMER);

            foreach (var m in m_mesh_sprites_dupes) 
            { 
                m.SetActive(season == Season.SUMMER); 
            }

            m_winter_mesh_sprites.gameObject.SetActive(season != Season.SUMMER);

            foreach (var m in m_winter_mesh_sprites_dupes)
            {
                m.SetActive(season != Season.SUMMER);
            }
        }
    }

    public List<Vector3> SpritePos()
    {
        return m_mesh_sprites.sprite_pos;
    }


    public void PlaceSprite(MapSprite ps, Vector3 pos, bool flipX=false)
    {
        if (ps.Sprite == null)
        {
            return;
        }

        if (ps.ValidColors.Count > 0)
        {
            m_mesh_sprites.Add(ps.Sprite, pos, ps.ValidColors.GetRandom(), flipX);
            m_winter_mesh_sprites.Add(ps.WinterSprite, pos, ps.ValidWinterColors.GetRandom(), flipX);
        }
        else
        {
            m_mesh_sprites.Add(ps.Sprite, pos, flipX);
            m_winter_mesh_sprites.Add(ps.WinterSprite, pos, flipX);
        }
    }

    public void Clear()
    {
        Destroy(m_mesh_sprites.gameObject);
        m_mesh_sprites = Instantiate(MeshSpritePrefab, transform);
        m_mesh_sprites.transform.position = new Vector3();

        Destroy(m_winter_mesh_sprites.gameObject);
        m_winter_mesh_sprites = Instantiate(MeshSpritePrefab, transform);
        m_winter_mesh_sprites.transform.position = new Vector3();

        SetSeason(GenerationManager.s_generation_manager?.Season ?? Season.SUMMER);

        foreach (var m in m_mesh_sprites_dupes)
        {
            Destroy(m);
        }

        m_mesh_sprites_dupes.Clear();

        foreach (var m in m_winter_mesh_sprites_dupes)
        {
            Destroy(m);
        }

        m_winter_mesh_sprites_dupes.Clear();
    }

    void Duplicate(Bounds b)
    {
        foreach (var m in m_mesh_sprites_dupes) 
        { 
            Destroy(m); 
        }

        m_mesh_sprites_dupes.Clear();

        foreach (var m in m_winter_mesh_sprites_dupes)
        {
            Destroy(m);
        }

        m_winter_mesh_sprites_dupes.Clear();
        m_mesh_sprites_dupes.AddRange(BorderOverlap.Duplicate(m_mesh_sprites.gameObject, m_mesh_sprites.Bounds, b));
        m_winter_mesh_sprites_dupes.AddRange(BorderOverlap.Duplicate(m_winter_mesh_sprites.gameObject, m_winter_mesh_sprites.Bounds, b));
    }

    public void Build(Bounds b)
    {
        m_mesh_sprites.Build();
        m_winter_mesh_sprites.Build();
        Duplicate(b);
    }
}
