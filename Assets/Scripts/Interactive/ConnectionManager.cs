
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager class that handles post-processing for connections.
/// Has a global singleton.
/// </summary>
public class ConnectionManager: MonoBehaviour
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

    ConnectionMarker m_current;
    NodeLayout m_layout;

    void Awake()
    {
        s_connection_manager = this;
	}

    public void SetLayout(NodeLayout layout)
    {
        m_layout = layout;
    }

    /*public void UpdateConnection(ConnectionType ct)
    {
        if (m_current != null && m_current.Connection != null)
        {
            m_current.UpdateConnection(ct);
        }
    }*/

    public void SetConnectionType(int ct)
    {
        if (m_current == null)
        {
            return;
        }

        ConnectionType c = (ConnectionType)ct;
        ConnectionType cur = m_current.Connection.ConnectionType;

        if (c == cur)
        {
            return;
        }

        m_current.UpdateConnection(c);

        if (m_current.LinkedConnection != null)
        {
            m_current.LinkedConnection.UpdateConnection(c);
        }

        List<ProvinceMarker> provs = new List<ProvinceMarker>();
        List<ConnectionMarker> conns = new List<ConnectionMarker>();
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

        foreach (ProvinceMarker m in pm.ConnectedProvinces)
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

        foreach (ProvinceMarker p in provs)
        {
            foreach (ConnectionMarker m in p.Connections)
            {
                if (provs.Contains(m.Prov1) && provs.Contains(m.Prov2) && !conns.Contains(m))
                {
                    conns.Add(m);
                }
            }
        }

        /*if (m_current.Prov1.IsDummy)
        {
            provs.AddRange(m_current.Prov1.LinkedProvinces);
        }
        else
        {
            provs.Add(m_current.Prov1);

            foreach (ProvinceMarker pm in m_current.Prov1.ConnectedProvinces)
            {
                if (pm.IsDummy)
                {
                    foreach (ProvinceMarker pm2 in pm.LinkedProvinces)
                    {
                        if (!provs.Contains(pm2))
                        {
                            provs.Add(pm2);
                        }
                    }
                }
            }
        }

        if (m_current.Prov2.IsDummy)
        {
            provs.AddRange(m_current.Prov2.LinkedProvinces);
        }
        else
        {
            provs.Add(m_current.Prov2);

            foreach (ProvinceMarker pm in m_current.Prov2.ConnectedProvinces)
            {
                if (pm.IsDummy)
                {
                    foreach (ProvinceMarker pm2 in pm.LinkedProvinces)
                    {
                        if (!provs.Contains(pm2))
                        {
                            provs.Add(pm2);
                        }
                    }
                }
            }
        }*/

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

        ConnectionType ct = c.Connection.ConnectionType;
        update_checkbox(ct);

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

    void update_checkbox(ConnectionType ct)
    {
        switch (ct)
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
    }
}
