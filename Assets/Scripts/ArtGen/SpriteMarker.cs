using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Season
{
    SUMMER,
    WINTER
}

/// <summary>
/// The behaviour class for sprite objects that are used to decorate provinces and connections.
/// </summary>
public class SpriteMarker: MonoBehaviour
{
    public GameObject MapSpritePrefab;

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

    public void SetSeason(Season s)
    {
        if (s == Season.SUMMER)
        {
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
    }

    public void SetSprite(MapSprite ms)
    {
        MapSprite = ms;

        SetSeason(GenerationManager.s_generation_manager.Season);

        if (ms.CanFlip && UnityEngine.Random.Range(0, 2) == 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    public void CopyFrom(SpriteMarker m)
    {
        this.IsDummy = true;
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

    public List<SpriteMarker> CreateMirrorSprites()
    {
        List<SpriteMarker> result = new List<SpriteMarker>();
        SpriteRenderer rend = GetComponent<SpriteRenderer>();
        Bounds b = rend.bounds;
        Vector3 max = b.max;
        Vector3 min = b.min;
        //min.y = max.y;

        Vector3 map_max = MapBorder.s_map_border.Maxs;
        Vector3 map_min = MapBorder.s_map_border.Mins;

        Vector3 size = map_max - map_min;

        if (max.x > map_max.x)
        {
            result.Add(create_sprite(transform.position - new Vector3(size.x, 0, 0)));
        }
        else if (min.x < map_min.x)
        {
            result.Add(create_sprite(transform.position + new Vector3(size.x, 0, 0)));
        }

        if (max.y > map_max.y)
        {
            result.Add(create_sprite(transform.position - new Vector3(0, size.y, 0)));
        }
        else if (min.y < map_min.y)
        {
            result.Add(create_sprite(transform.position + new Vector3(0, size.y, 0)));
        }

        if (max.y > map_max.y && max.x > map_max.x)
        {
            result.Add(create_sprite(transform.position - new Vector3(size.x, size.y, 0)));
        }
        else if (min.y < map_min.y && min.x < map_min.x)
        {
            result.Add(create_sprite(transform.position + new Vector3(size.x, size.y, 0)));
        }

        return result;
    }

    SpriteMarker create_sprite(Vector3 pos)
    {
        GameObject g = GameObject.Instantiate(MapSpritePrefab);
        SpriteMarker sm = g.GetComponent<SpriteMarker>();
        sm.CopyFrom(this);
        sm.transform.position = pos;

        return sm;
    }
}
