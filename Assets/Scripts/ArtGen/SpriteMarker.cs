using System.Collections.Generic;
using UnityEngine;

public enum Season
{
    SUMMER,
    WINTER
}

/// <summary>
/// The behaviour class for sprite objects that are used to decorate provinces and connections.
/// </summary>
public class SpriteMarker : MonoBehaviour
{
    public GameObject MapSpritePrefab;
    private bool m_force_flip = false;
    private bool m_flip_x = false;

    public MapSprite MapSprite
    {
        get;
        private set;
    }

    public bool IsDummy
    {
        get;
        private set;
    }

    public bool ForceFlip
    {
        get
        {
            return m_force_flip;
        }
    }

    public int SortingOrder
    {
        get
        {
            return GetComponent<SpriteRenderer>().sortingOrder;
        }
    }

    public bool FlipX
    {
        get
        {
            return GetComponent<SpriteRenderer>().flipX;
        }
    }


    public void SetSeason(Season s)
    {
        if (s == Season.SUMMER)
        {
            if (MapSprite.CanFlip && UnityEngine.Random.Range(0, 2) == 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
            else
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }

            GetComponent<SpriteRenderer>().sprite = MapSprite.Sprite;

            if (MapSprite.ValidColors.Any())
            {
                GetComponent<SpriteRenderer>().color = MapSprite.ValidColors.GetRandom();
            }
            else
            {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
            }
        }
        else
        {
            if (MapSprite.CanFlipWinter && UnityEngine.Random.Range(0, 2) == 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
            else
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }

            GetComponent<SpriteRenderer>().sprite = MapSprite.WinterSprite;

            if (MapSprite.ValidWinterColors.Any())
            {
                GetComponent<SpriteRenderer>().color = MapSprite.ValidWinterColors.GetRandom();
            }
            else
            {
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
            }
        }

        if (m_force_flip)
        {
            GetComponent<SpriteRenderer>().flipX = m_flip_x;
        }
    }

    public void SetFlip(bool on, bool flip_x)
    {
        m_force_flip = on;
        m_flip_x = flip_x;

        if (m_force_flip)
        {
            GetComponent<SpriteRenderer>().flipX = m_flip_x;
        }
    }

    public void SetSprite(MapSprite ms)
    {
        MapSprite = ms;

        SetSeason(GenerationManager.s_generation_manager.Season);
    }

    public void CopyFrom(SpriteMarker m)
    {
        IsDummy = true;
        MapSprite = m.MapSprite;
        GetComponent<SpriteRenderer>().flipX = m.GetComponent<SpriteRenderer>().flipX;
        GetComponent<SpriteRenderer>().color = m.GetComponent<SpriteRenderer>().color;
    }

    public void SetOrder(int i)
    {
        GetComponent<SpriteRenderer>().sortingOrder = i;
    }

    public bool Overlaps(Vector3 pos)
    {
        return Vector3.Distance(transform.position, pos) <= MapSprite.Size;
    }

    public List<SpriteMarker> CreateMirrorSprites(Vector3 forced_max, Vector3 forced_min, bool force = false)
    {
        var result = new List<SpriteMarker>();
        var rend = GetComponent<SpriteRenderer>();
        var b = rend.bounds;
        var max = b.max;
        var min = b.min;

        if (force)
        {
            max = forced_max;
            min = forced_min;
        }

        var map_max = MapBorder.s_map_border.Maxs;
        var map_min = MapBorder.s_map_border.Mins;
        var size = map_max - map_min;
        var flip = FlipX;

        if (max.x > map_max.x)
        {
            result.Add(create_sprite(transform.position - new Vector3(size.x, 0, 0), flip));
        }
        else if (min.x < map_min.x)
        {
            result.Add(create_sprite(transform.position + new Vector3(size.x, 0, 0), flip));
        }

        if (max.y > map_max.y)
        {
            result.Add(create_sprite(transform.position - new Vector3(0, size.y, 0), flip));
        }
        else if (min.y < map_min.y)
        {
            result.Add(create_sprite(transform.position + new Vector3(0, size.y, 0), flip));
        }

        if (max.y > map_max.y && max.x > map_max.x)
        {
            result.Add(create_sprite(transform.position - new Vector3(size.x, size.y, 0), flip));
        }
        else if (min.y < map_min.y && min.x < map_min.x)
        {
            result.Add(create_sprite(transform.position + new Vector3(size.x, size.y, 0), flip));
        }

        return result;
    }

    private SpriteMarker create_sprite(Vector3 pos, bool flip)
    {
        var g = GameObject.Instantiate(MapSpritePrefab);
        var sm = g.GetComponent<SpriteMarker>();
        sm.CopyFrom(this);
        sm.transform.position = pos;

        var max_y = Mathf.RoundToInt((MapBorder.s_map_border.Maxs.y + 1.0f) * 100f);
        var y = max_y - Mathf.RoundToInt(pos.y * 100f);

        sm.SetOrder(y);
        sm.SetFlip(true, flip);

        return sm;
    }
}
