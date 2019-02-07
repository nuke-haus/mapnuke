using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager class that handles post-processing for provinces.
/// Has a global singleton.
/// </summary>
public class ProvinceManager: MonoBehaviour
{
    public static ProvinceManager s_province_manager;

    public AudioClip AudioApply;
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
    public Toggle Throne;

    ProvinceMarker m_current;
    NodeLayout m_layout;

    void Awake()
    {
        s_province_manager = this;
    }

    public void SetLayout(NodeLayout layout)
    {
        m_layout = layout;
    }

    public void UpdateProvince()
    {
        if (m_current == null)
        {
            return;
        }

        Terrain flags = Terrain.PLAINS;

        if (Highland.isOn)
        {
            flags = flags | Terrain.HIGHLAND;
        }
        if (Mountain.isOn)
        {
            flags = flags | Terrain.MOUNTAINS;
        }
        if (Cave.isOn)
        {
            flags = flags | Terrain.CAVE;
        }
        if (Swamp.isOn)
        {
            flags = flags | Terrain.SWAMP;
        }
        if (Waste.isOn)
        {
            flags = flags | Terrain.WASTE;
        }
        if (Forest.isOn)
        {
            flags = flags | Terrain.FOREST;
        }
        if (Farm.isOn)
        {
            flags = flags | Terrain.FARM;
        }
        if (Sea.isOn)
        {
            flags = flags | Terrain.SEA;
        }
        if (DeepSea.isOn)
        {
            flags = flags | Terrain.DEEPSEA | Terrain.SEA;
        }
        if (Throne.isOn)
        {
            flags = flags | Terrain.THRONE;
        }

        if (Large.isOn)
        {
            flags = flags | Terrain.LARGEPROV;
        }
        else if (Small.isOn)
        {
            flags = flags | Terrain.SMALLPROV;
        }

        if (Colder.isOn)
        {
            flags = flags | Terrain.COLDER;
        }
        else if (Warmer.isOn)
        {
            flags = flags | Terrain.WARMER;
        }

        if (m_current.Node.HasNation)
        {
            flags = flags | Terrain.START;
        }

        m_current.Node.ProvinceData.SetTerrainFlags(flags);
        m_current.UpdateColor();

        List<ProvinceMarker> provs = new List<ProvinceMarker> { m_current };
        provs.AddRange(m_current.ConnectedProvinces);

        List<ConnectionMarker> conns = new List<ConnectionMarker>();
        conns.AddRange(m_current.Connections);

        foreach (ProvinceMarker pm in provs)
        {
            foreach (ConnectionMarker m in pm.Connections)
            {
                if (provs.Contains(m.Prov1) && provs.Contains(m.Prov2) && !conns.Contains(m))
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

    public void SetProvince(ProvinceMarker p)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (m_current != null)
        {
            m_current.SetSelected(false);
        }

        ConnectionManager.s_connection_manager.Deselect();

        m_current = p;

        clear_checkboxes();
        update_checkboxes(p.Node.ProvinceData.Terrain);

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

    void clear_checkboxes()
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
    }

    void update_checkboxes(Terrain flags)
    {
        bool plains = true;

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
    }
}
