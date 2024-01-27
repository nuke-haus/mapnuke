using System.Collections.Generic;
using System.Linq;
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

    public RectTransform PanelTransform;

    // Standard province stuff
    public Toggle Plains;
    public Toggle Highland;
    public Toggle Mountain;
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
    public Toggle Throne;
    public Toggle Fort;

    // Cave stuff
    public Toggle HasCaveEntrance;
    public Toggle HasCaveProvince;
    public Toggle Cave;
    public Toggle DripCave;
    public Toggle CrystalCave;
    public Toggle ForestCave;
    public Toggle SeaCave;

    public InputField Name;
    public InputField CaveName;

    private ProvinceMarker m_current;
    private NodeLayoutData m_layout;

    private void Awake()
    {
        s_province_manager = this;
    }

    private void update_panel_position()
    {
        // If the user is using a small ass screen then move the panel to a nice spot
        if (Screen.height <= 900)
        {
            PanelTransform.anchoredPosition = new Vector3(PanelTransform.anchoredPosition.x, -5f, 0f);
        }
    }

    private void clear_checkboxes()
    {
        Plains.isOn = false;
        Highland.isOn = false;
        Mountain.isOn = false;
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
        Fort.isOn = false;
        Cave.isOn = false;
        DripCave.isOn = false;
        CrystalCave.isOn = false;
        ForestCave.isOn = false;
        SeaCave.isOn = false;
        HasCaveEntrance.isOn = false;
        HasCaveProvince.isOn = false;
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

        var cave_flags = Terrain.PLAINS;
        var flags = Terrain.PLAINS;

        // Standard province modifiers
        if (Highland.isOn)
        {
            flags |= Terrain.HIGHLAND;
        }
        if (Mountain.isOn)
        {
            flags |= Terrain.MOUNTAINS;
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

        // Size modifiers
        if (Large.isOn)
        {
            flags |= Terrain.LARGEPROV;
        }
        else if (Small.isOn)
        {
            flags |= Terrain.SMALLPROV;
        }

        // Hot and cold modifiers
        if (Colder.isOn)
        {
            flags |= Terrain.COLDER;
        }
        else if (Warmer.isOn)
        {
            flags |= Terrain.WARMER;
        }

        // Throne modifier
        if (Throne.isOn)
        {
            flags |= Terrain.THRONE;
        }

        // Cave terrain modifiers
        if (DripCave.isOn)
        {
            CrystalCave.isOn = false;
            ForestCave.isOn = false;
            cave_flags = Terrain.SWAMP;
        }
        if (CrystalCave.isOn)
        {
            DripCave.isOn = false;
            ForestCave.isOn = false;
            cave_flags = Terrain.HIGHLAND;
        }
        if (ForestCave.isOn)
        {
            CrystalCave.isOn = false;
            DripCave.isOn = false;
            cave_flags = Terrain.FOREST;
        }
        if (SeaCave.isOn)
        {
            // Sea swamp is not allowed for caves
            if (cave_flags == Terrain.SWAMP)
            {
                DripCave.isOn = false;
                cave_flags = Terrain.PLAINS;
            }

            cave_flags |= Terrain.SEA;
        }

        if (HasCaveEntrance.isOn)
        {
            HasCaveProvince.isOn = true;
        }

        if (Fort.isOn)
        {
            // We update fort type in case terrain changed at all
            m_current.Node.ProvinceData.SetFortType(FortHelper.GetFort(m_current.Node));
        }
        else
        {
            m_current.Node.ProvinceData.SetFortType(global::Fort.NONE);
        }

        m_current.Node.TempCaveProvinceData.SetHasCaveEntrance(HasCaveEntrance.isOn);
        m_current.Node.TempCaveProvinceData.SetIsCaveWall(!HasCaveProvince.isOn);
        m_current.Node.TempCaveProvinceData.SetCaveTerrainFlags(cave_flags);
        m_current.Node.ProvinceData.SetCaveTerrainFlags(cave_flags);
        m_current.Node.ProvinceData.SetTerrainFlags(flags);
        m_current.Node.ProvinceData.SetCustomName(Name.text);
        m_current.Node.ProvinceData.SetCaveCustomName(CaveName.text);
        m_current.Node.ProvinceData.SetHasCaveEntrance(HasCaveEntrance.isOn);
        m_current.Node.ProvinceData.SetIsCaveWall(!HasCaveProvince.isOn);
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

        update_panel_position();
        clear_checkboxes();
        update_checkboxes(p.Node.ProvinceData);
        update_name(p.Node.ProvinceData);

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

    private void update_name(ProvinceData data)
    {
        Name.text = data.CustomName;
        CaveName.text = data.CaveCustomName;
    }

    private void update_checkboxes(ProvinceData data)
    {
        var flags = data.Terrain;
        var cave_flags = data.CaveTerrain;
        var plains = true;

        // Standard province terrains
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
        if (data.Fort != global::Fort.NONE)
        {
            Fort.isOn = true;
        }

        // Size flags
        if (flags.IsFlagSet(Terrain.LARGEPROV))
        {
            Large.isOn = true;
        }
        else if (flags.IsFlagSet(Terrain.SMALLPROV))
        {
            Small.isOn = true;
        }

        // Hot and cold flags
        if (flags.IsFlagSet(Terrain.COLDER))
        {
            Colder.isOn = true;
        }
        else if (flags.IsFlagSet(Terrain.WARMER))
        {
            Warmer.isOn = true;
        }

        Plains.isOn = plains;

        // Cave flags
        if (cave_flags.IsFlagSet(Terrain.HIGHLAND))
        {
            CrystalCave.isOn = true;
        }
        else if (cave_flags.IsFlagSet(Terrain.SWAMP))
        {
            DripCave.isOn = true;
        }
        else if (cave_flags.IsFlagSet(Terrain.FOREST))
        {
            ForestCave.isOn = true;
        }
        else if (cave_flags.IsFlagSet(Terrain.SEA))
        {
            SeaCave.isOn = true;
        }
        else 
        {
            Cave.isOn = true;
        }

        HasCaveEntrance.isOn = data.HasCaveEntrance;
        HasCaveProvince.isOn = !data.IsCaveWall;
    }
}
