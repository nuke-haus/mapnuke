using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager class that handles post-processing for provinces.
/// Has a global singleton.
/// </summary>
public class ProvinceManager : MonoBehaviour
{
    public static ProvinceManager s_province_manager;

    public AudioClip AudioApply;
    public AudioClip AudioClick;
    public AudioClip Audio;
    public Toggle Plains;
    public Toggle Highland;
    public Toggle Mountain;
    public Toggle Cave;
    public Toggle Swamp;
    public Toggle Waste;
    public Toggle Forest;
    public Toggle Farm;
    public Toggle Sea;
    public Toggle DeepSea;
    public Toggle Large;
    public Toggle Small;
    public Toggle Colder;
    public Toggle Warmer;
    public Toggle MoreSites;
    public Toggle HasCaveEntrance;
    public Toggle Throne;
    public InputField Name;
    private ProvinceMarker m_current;
    private NodeLayoutData m_layout;

    private void Awake()
    {
        s_province_manager = this;
    }

    public void SetLayout(NodeLayoutData layout)
    {
        m_layout = layout;
    }

    public void UpdateProvince()
    {
        if (m_current == null)
        {
            return;
        }

        var flags = Terrain.PLAINS;

        if (Highland.isOn)
        {
            flags |= Terrain.HIGHLAND;
        }
        if (Mountain.isOn)
        {
            flags |= Terrain.MOUNTAINS;
        }
        if (Cave.isOn)
        {
            flags |= Terrain.CAVE;
        }
        if (Swamp.isOn)
        {
            flags |= Terrain.SWAMP;
        }
        if (Waste.isOn)
        {
            flags |= Terrain.WASTE;
        }
        if (Forest.isOn)
        {
            flags |= Terrain.FOREST;
        }
        if (Farm.isOn)
        {
            flags |= Terrain.FARM;
        }
        if (Sea.isOn)
        {
            flags |= Terrain.SEA;
        }
        if (DeepSea.isOn)
        {
            flags = flags | Terrain.DEEPSEA | Terrain.SEA;
        }
        if (MoreSites.isOn)
        {
            flags |= Terrain.MANYSITES;
        }

        if (Large.isOn)
        {
            flags |= Terrain.LARGEPROV;
        }
        else if (Small.isOn)
        {
            flags |= Terrain.SMALLPROV;
        }

        if (Colder.isOn)
        {
            flags |= Terrain.COLDER;
        }
        else if (Warmer.isOn)
        {
            flags |= Terrain.WARMER;
        }

        if (Throne.isOn)
        {
            flags |= Terrain.THRONE;
        }

        m_current.Node.ProvinceData.SetTerrainFlags(flags);
        m_current.Node.ProvinceData.SetCustomName(Name.text);
        m_current.Node.ProvinceData.SetHasCaveEntrance(HasCaveEntrance.isOn);
        m_current.UpdateColor();
        m_current.UpdateSprite();
        m_current.UpdateLinked();
        m_current.ValidateConnections();

        var provs = new List<ProvinceMarker>();
        var dummies = new List<ProvinceMarker>();

        foreach (var pm in m_current.ConnectedProvinces)
        {
            if (pm.IsDummy)
            {
                dummies.Add(pm);
                provs.AddRange(pm.LinkedProvinces);

                if (pm.LinkedProvinces[0].LinkedProvinces.Count > 1)
                {
                    foreach (var linked in pm.LinkedProvinces[0].LinkedProvinces) // 3 dummies
                    {
                        dummies.Add(linked);
                        provs.AddRange(linked.ConnectedProvinces);
                    }
                }
                else
                {
                    provs.AddRange(pm.LinkedProvinces[0].ConnectedProvinces);
                }
            }
            else
            {
                provs.Add(pm);

                if (pm.LinkedProvinces != null && pm.LinkedProvinces.Any())
                {
                    foreach (var linked in pm.LinkedProvinces)
                    {
                        provs.AddRange(linked.ConnectedProvinces);
                        dummies.Add(linked);
                    }
                }

                // very specific case where the province is indirectly connected to the 0,0 corner province
                var corner_dummy = pm.ConnectedProvinces.Find(x => x.IsDummy && x.Node.X == 0 && x.Node.Y == 0);
                if (corner_dummy != null && !provs.Contains(corner_dummy))
                {
                    dummies.Add(corner_dummy);

                    if (corner_dummy.LinkedProvinces != null && corner_dummy.LinkedProvinces.Any())
                    {
                        foreach (var linked in corner_dummy.LinkedProvinces)
                        {
                            provs.Add(linked);
                        }
                    }
                }
            }
        }

        provs.Add(m_current);

        if (m_current.LinkedProvinces != null && m_current.LinkedProvinces.Any())
        {
            foreach (var linked in m_current.LinkedProvinces)
            {
                provs.AddRange(linked.ConnectedProvinces);
                dummies.Add(linked);
            }
        }

        var conns = new List<ConnectionMarker>();

        foreach (var pm in provs)
        {
            foreach (var m in pm.Connections)
            {
                if (!conns.Contains(m) && ((provs.Contains(m.Prov1) || dummies.Contains(m.Prov1)) && (provs.Contains(m.Prov2) || dummies.Contains(m.Prov2))))
                {
                    conns.Add(m);
                }
            }
        }

        GenerationManager.s_generation_manager.RegenerateElements(provs, conns, m_layout);
    }

    public void PlaySound()
    {
        GetComponent<AudioSource>().PlayOneShot(AudioApply);
    }

    public void Click()
    {
        GetComponent<AudioSource>().PlayOneShot(AudioClick);
    }

    public void SetProvince(ProvinceMarker p)
    {
        ConnectionManager.s_connection_manager.Deselect();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (m_current != null)
        {
            m_current.SetSelected(false);
        }

        m_current = p;

        clear_checkboxes();
        update_checkboxes(p.Node.ProvinceData.Terrain, p.Node.ProvinceData.HasCaveEntrance);
        update_name(p.Node.ProvinceData.CustomName);

        GetComponent<AudioSource>().PlayOneShot(Audio);
    }

    public void Deselect()
    {
        if (m_current != null)
        {
            m_current.SetSelected(false);
        }

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private void update_name(string name)
    {
        Name.text = name;
    }

    private void clear_checkboxes()
    {
        Highland.isOn = false;
        Mountain.isOn = false;
        Cave.isOn = false;
        Swamp.isOn = false;
        Waste.isOn = false;
        Farm.isOn = false;
        Forest.isOn = false;
        Sea.isOn = false;
        DeepSea.isOn = false;
        Throne.isOn = false;
        Large.isOn = false;
        Small.isOn = false;
        Colder.isOn = false;
        Warmer.isOn = false;
        HasCaveEntrance.isOn = false;
    }

    private void update_checkboxes(Terrain flags, bool has_cave_entrance)
    {
        var plains = true;

        if (flags.IsFlagSet(Terrain.HIGHLAND))
        {
            Highland.isOn = true;
            plains = false;
        }
        if (flags.IsFlagSet(Terrain.MOUNTAINS))
        {
            Mountain.isOn = true;
            plains = false;
        }
        if (flags.IsFlagSet(Terrain.CAVE))
        {
            Cave.isOn = true;
            plains = false;
        }
        if (flags.IsFlagSet(Terrain.SWAMP))
        {
            Swamp.isOn = true;
            plains = false;
        }
        if (flags.IsFlagSet(Terrain.WASTE))
        {
            Waste.isOn = true;
            plains = false;
        }
        if (flags.IsFlagSet(Terrain.FOREST))
        {
            Forest.isOn = true;
            plains = false;
        }
        if (flags.IsFlagSet(Terrain.FARM))
        {
            Farm.isOn = true;
            plains = false;
        }
        if (flags.IsFlagSet(Terrain.SEA))
        {
            Sea.isOn = true;
            plains = false;
        }
        if (flags.IsFlagSet(Terrain.DEEPSEA))
        {
            DeepSea.isOn = true;
            plains = false;
        }
        if (flags.IsFlagSet(Terrain.THRONE))
        {
            Throne.isOn = true;
        }

        Plains.isOn = plains;

        if (flags.IsFlagSet(Terrain.LARGEPROV))
        {
            Large.isOn = true;
        }
        else if (flags.IsFlagSet(Terrain.SMALLPROV))
        {
            Small.isOn = true;
        }

        if (flags.IsFlagSet(Terrain.COLDER))
        {
            Colder.isOn = true;
        }
        else if (flags.IsFlagSet(Terrain.WARMER))
        {
            Warmer.isOn = true;
        }

        HasCaveEntrance.isOn = has_cave_entrance;
    }
}
