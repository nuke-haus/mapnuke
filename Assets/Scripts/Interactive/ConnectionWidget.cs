using System.Collections.Generic;
using UnityEngine;

public class ConnectionWidget : MonoBehaviour
{
    public GameObject SpriteObj;
    private ConnectionMarker m_parent;
    private Vector3 m_midpt;
    private Dictionary<ConnectionType, Color> m_colors;
    private bool m_selected = false;
    private float m_scale = 1.0f;

    public void SetParent(ConnectionMarker m)
    {
        m_parent = m;

        var p1 = m.Endpoint1;
        var p2 = m.Endpoint2;
        var offset = new Vector3(500f, 0f, 0f);

        m_midpt = (p1 + p2) / 2;
        var dir = (p1 - p2).normalized;

        p1.z = -4.0f;
        p2.z = -4.0f;

        var rend = GetComponent<LineRenderer>();
        rend.SetPositions(new Vector3[] { p1 + offset, p2 + offset });

        //SetConnection(m.Connection);
    }

    public void SetConnection(Connection c)
    {
        if (m_colors == null)
        {
            m_colors = new Dictionary<ConnectionType, Color>
            {
                { ConnectionType.STANDARD, new Color(1.0f, 1.0f, 0.4f) },
                { ConnectionType.SHALLOWRIVER, new Color(0.4f, 0.5f, 0.9f) },
                { ConnectionType.ROAD, new Color(0.8f, 0.5f, 0.1f) },
                { ConnectionType.MOUNTAINPASS, new Color(0.6f, 0.3f, 0.8f) },
                { ConnectionType.MOUNTAIN, new Color(0.5f, 0.1f, 0.2f) },
                { ConnectionType.RIVER, new Color(0.2f, 0.2f, 0.9f) }
            };
        }

        var col = m_colors[c.ConnectionType];

        var rend = GetComponent<LineRenderer>();
        rend.startColor = col;
        rend.endColor = col;

        var rend2 = SpriteObj.GetComponent<SpriteRenderer>();
        rend2.color = col;
    }

    private void Update()
    {
        if (m_selected)
        {
            m_scale = 1.0f + 0.2f * Mathf.Sin(Time.time * 5.5f);
            SpriteObj.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
        }
    }

    private void OnMouseOver()
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
