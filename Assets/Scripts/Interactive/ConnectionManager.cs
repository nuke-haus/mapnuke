
using System;
using System.Collections.Generic;
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

        List<ProvinceMarker> provs = new List<ProvinceMarker> { m_current.Prov1, m_current.Prov2 };
        List<ConnectionMarker> conns = new List<ConnectionMarker> { m_current };

        GetComponent<AudioSource>().PlayOneShot(AudioApply);

        GenerationManager.s_generation_manager.RegenerateElements(provs, conns, m_layout);
    }

    public void SetConnection(ConnectionMarker c)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (m_current != null)
        {
            if (m_current == c)
            {
                return;
            }

            m_current.SetSelected(false);
        }

        ProvinceManager.s_province_manager.Deselect();

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
