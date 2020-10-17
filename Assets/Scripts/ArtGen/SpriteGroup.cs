using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteGroup : MonoBehaviour
{
    public MeshSpriteRenderer mesh_sprite_prefab;
    MeshSpriteRenderer mesh_sprites;
    List<GameObject> mesh_sprites_dupes = new List<GameObject>();
    MeshSpriteRenderer winter_mesh_sprites;
    List<GameObject> winter_mesh_sprites_dupes = new List<GameObject>();

    private void Awake()
    {
        mesh_sprites = Instantiate(mesh_sprite_prefab);
        mesh_sprites.transform.position = Vector3.zero;
        winter_mesh_sprites = Instantiate(mesh_sprite_prefab);
        winter_mesh_sprites.transform.position = Vector3.zero;
    }

    public void SetSeason(Season season)
    {
        if (mesh_sprites != null)
        {
            mesh_sprites.gameObject.SetActive(season == Season.SUMMER);
            foreach (var m in mesh_sprites_dupes) m.SetActive(season == Season.SUMMER);
            winter_mesh_sprites.gameObject.SetActive(season != Season.SUMMER);
            foreach (var m in winter_mesh_sprites_dupes) m.SetActive(season != Season.SUMMER);
        }
    }

    public List<Vector3> SpritePos()
    {
        return mesh_sprites.sprite_pos;
    }


    public void PlaceSprite(MapSprite ps, Vector3 pos, bool flipX=false)
    {
        if (ps.Sprite == null) return;
        if (ps.ValidColors.Count > 0) mesh_sprites.Add(ps.Sprite, pos, ps.ValidColors.GetRandom(), flipX);
        else mesh_sprites.Add(ps.Sprite, pos, flipX);
        if (ps.ValidColors.Count > 0) winter_mesh_sprites.Add(ps.WinterSprite, pos, ps.ValidWinterColors.GetRandom(), flipX);
        else winter_mesh_sprites.Add(ps.WinterSprite, pos, flipX);
    }

    public void Clear()
    {
        Destroy(mesh_sprites.gameObject);
        mesh_sprites = Instantiate(mesh_sprite_prefab, transform);
        mesh_sprites.transform.position = new Vector3();
        Destroy(winter_mesh_sprites.gameObject);
        winter_mesh_sprites = Instantiate(mesh_sprite_prefab, transform);
        winter_mesh_sprites.transform.position = new Vector3();
        SetSeason(GenerationManager.s_generation_manager?.Season ?? Season.SUMMER);
        foreach (var m in mesh_sprites_dupes) Destroy(m);
        mesh_sprites_dupes.Clear();
        foreach (var m in winter_mesh_sprites_dupes) Destroy(m);
        winter_mesh_sprites_dupes.Clear();
    }

    void Duplicate(Bounds b)
    {
        foreach (var m in mesh_sprites_dupes) Destroy(m);
        mesh_sprites_dupes.Clear();
        foreach (var m in winter_mesh_sprites_dupes) Destroy(m);
        winter_mesh_sprites_dupes.Clear();
        mesh_sprites_dupes.AddRange(BorderOverlap.Duplicate(mesh_sprites.gameObject, mesh_sprites.Bounds, b));
        winter_mesh_sprites_dupes.AddRange(BorderOverlap.Duplicate(winter_mesh_sprites.gameObject, winter_mesh_sprites.Bounds, b));
    }

    public void Build(Bounds b)
    {
        mesh_sprites.Build();
        winter_mesh_sprites.Build();
        Duplicate(b);
    }
}
