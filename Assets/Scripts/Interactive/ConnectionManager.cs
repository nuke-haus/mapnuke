using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager class that handles post-processing for connections.
/// Has a global singleton.
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager s_connection_manager;

    public AudioClip Audio;
    public AudioClip AudioClick;
    public AudioClip AudioApply;
    public Toggle Standard;
    public Toggle Road;
    public Toggle Mountain;
    public Toggle MountainPass;
    public Toggle DeepRiver;
    public Toggle ShallowRiver;

    public Toggle CaveStandard;
    public Toggle CaveRiver;

    private ConnectionMarker m_current;
    private NodeLayoutData m_layout;

    private void Awake()
    {
        s_connection_manager = this;
    }

    public void SetLayout(NodeLayoutData layout)
    {
        m_layout = layout;
    }

    public void SetConnectionType(int ct)
    {
        if (m_current == null)
        {
            return;
        }

        var c = (ConnectionType)ct;
        var cur = m_current.Connection.ConnectionType;

        if (c == cur)
        {
            return;
        }

        m_current.UpdateConnection(c);

        if (m_current.LinkedConnection != null)
        {
            m_current.LinkedConnection.UpdateConnection(c);
        }

        var provs = new List<ProvinceMarker>();
        var conns = new List<ConnectionMarker>();
        ProvinceMarker pm = null;

        if (m_current.Prov1.IsDummy)
        {
            pm = m_current.Prov2;
        }
        else
        {
            pm = m_current.Prov1;
        }

        provs.Add(pm);

        foreach (var m in pm.ConnectedProvinces)
        {
            if (m.IsDummy)
            {
                provs.AddRange(m.LinkedProvinces);
            }
            else
            {
                provs.Add(m);
            }
        }

        conns.AddRange(pm.Connections);

        foreach (var p in provs)
        {
            foreach (var m in p.Connections)
            {
                if (provs.Contains(m.Prov1) && provs.Contains(m.Prov2) && !conns.Contains(m))
                {
                    conns.Add(m);
                }
            }
        }

        GetComponent<AudioSource>().PlayOneShot(AudioApply);
        GenerationManager.s_generation_manager.RegenerateElements(provs, conns, m_layout);
    }

    public void SetCaveConnectionType(int ct)
    {
        if (m_current == null)
        {
            return;
        }

        var c = (ConnectionType)ct;
        var cur = m_current.Connection.CaveConnectionType;

        if (c == cur)
        {
            return;
        }

        m_current.SetSeason(GenerationManager.s_generation_manager.Season);
        m_current.UpdateCaveConnection(c);

        if (m_current.LinkedConnection != null)
        {
            m_current.LinkedConnection.UpdateCaveConnection(c);
        }

        var provs = new List<ProvinceMarker>();
        var conns = new List<ConnectionMarker>();
        ProvinceMarker pm = null;

        if (m_current.Prov1.IsDummy)
        {
            pm = m_current.Prov2;
        }
        else
        {
            pm = m_current.Prov1;
        }

        provs.Add(pm);

        foreach (var m in pm.ConnectedProvinces)
        {
            if (m.IsDummy)
            {
                provs.AddRange(m.LinkedProvinces);
            }
            else
            {
                provs.Add(m);
            }
        }

        conns.AddRange(pm.Connections);

        foreach (var p in provs)
        {
            foreach (var m in p.Connections)
            {
                if (provs.Contains(m.Prov1) && provs.Contains(m.Prov2) && !conns.Contains(m))
                {
                    conns.Add(m);
                }
            }
        }

        GetComponent<AudioSource>().PlayOneShot(AudioApply);
        GenerationManager.s_generation_manager.RegenerateElements(provs, conns, m_layout);
    }

    public void SetConnection(ConnectionMarker c)
    {
        ProvinceManager.s_province_manager.Deselect();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (m_current != null)
        {
            if (m_current == c)
            {
                GetComponent<AudioSource>().PlayOneShot(Audio);

                return;
            }

            m_current.SetSelected(false);
        }

        m_current = null;

        var connection = c.Connection.ConnectionType;
        var cave_connection = c.Connection.CaveConnectionType;
        update_checkbox(connection, cave_connection);

        m_current = c;

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

    private void update_checkbox(ConnectionType connection, ConnectionType cave_connection)
    {
        switch (connection)
        {
            case ConnectionType.STANDARD:
                Standard.isOn = true;
                break;
            case ConnectionType.ROAD:
                Road.isOn = true;
                break;
            case ConnectionType.MOUNTAIN:
                Mountain.isOn = true;
                break;
            case ConnectionType.MOUNTAINPASS:
                MountainPass.isOn = true;
                break;
            case ConnectionType.RIVER:
                DeepRiver.isOn = true;
                break;
            case ConnectionType.SHALLOWRIVER:
                ShallowRiver.isOn = true;
                break;
            default:
                Standard.isOn = true;
                break;
        }

        switch (cave_connection)
        {
            case ConnectionType.STANDARD:
                CaveStandard.isOn = true;
                break;
            case ConnectionType.RIVER:
                CaveRiver.isOn = true;
                break;
        }
    }
}
