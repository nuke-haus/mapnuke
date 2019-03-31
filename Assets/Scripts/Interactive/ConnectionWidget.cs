using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionWidget: MonoBehaviour
{
    public GameObject SpriteObj;

    ConnectionMarker m_parent;
    Vector3 m_midpt;
    Dictionary<ConnectionType, Color> m_colors;
    bool m_selected = false;
    float m_scale = 1.0f;

    public void SetParent(ConnectionMarker m)
    {
        m_parent = m;

        Vector3 p1 = m.Endpoint1;
        Vector3 p2 = m.Endpoint2;
        Vector3 offset = new Vector3(500f, 0f, 0f);

        m_midpt = (p1 + p2) / 2;
        Vector3 dir = (p1 - p2).normalized;

        p1.z = -4.0f;
        p2.z = -4.0f;

        LineRenderer rend = GetComponent<LineRenderer>();
        rend.SetPositions(new Vector3[] { p1 + offset, p2 + offset });

        //SetConnection(m.Connection);
    }

    public void SetConnection(Connection c)
    {
        if (m_colors == null)
        {
            Dictionary<ConnectionType, Color> dict = new Dictionary<ConnectionType, Color>();
            dict.Add(ConnectionType.STANDARD, new Color(1.0f, 1.0f, 0.4f));
            dict.Add(ConnectionType.SHALLOWRIVER, new Color(0.4f, 0.5f, 0.9f));
            dict.Add(ConnectionType.ROAD, new Color(0.8f, 0.5f, 0.1f));
            dict.Add(ConnectionType.MOUNTAINPASS, new Color(0.6f, 0.3f, 0.8f));
            dict.Add(ConnectionType.MOUNTAIN, new Color(0.5f, 0.1f, 0.2f));
            dict.Add(ConnectionType.RIVER, new Color(0.2f, 0.2f, 0.9f));

            m_colors = dict;
        }

        Color col = m_colors[c.ConnectionType];

        LineRenderer rend = GetComponent<LineRenderer>();
        rend.startColor = col;
        rend.endColor = col;

        SpriteRenderer rend2 = SpriteObj.GetComponent<SpriteRenderer>();
        rend2.color = col;
    }

    void Update()
    {
        if (m_selected)
        {
            m_scale = 1.0f + 0.2f * Mathf.Sin(Time.time * 5.5f);
            SpriteObj.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ConnectionManager.s_connection_manager.SetConnection(m_parent);

            m_parent.SetSelected(true);
        }
    }

    public void SetSelected(bool b)
    {
        m_selected = b;
        m_scale = 1.0f;
        SpriteObj.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
    }
}
