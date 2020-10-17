using System.Collections.Generic;
using UnityEngine;

public class ProvinceWidget : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public TextMesh Text;
    public List<TextMesh> TextOutlines;

    private ProvinceMarker m_parent;
    private bool m_selected = false;
    private float m_scale = 1.0f;
    private Dictionary<Terrain, Color> m_colors;

    public void SetParent(ProvinceMarker m)
    {
        m_parent = m;
    }

    public void SetSelected(bool b)
    {
        m_selected = b;
        m_scale = 1.0f;

        Renderer.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
    }

    public void SetNode(Node n)
    {
        if (m_colors == null)
        {
            m_colors = new Dictionary<Terrain, Color>
            {
                { Terrain.DEEPSEA, new Color(0.2f, 0.3f, 0.9f) },
                { Terrain.SEA, new Color(0.4f, 0.6f, 0.9f) },
                { Terrain.FARM, new Color(0.9f, 0.8f, 0.2f) },
                { Terrain.SWAMP, new Color(0.6f, 0.8f, 0.1f) },
                { Terrain.WASTE, new Color(0.6f, 0.4f, 0.3f) },
                { Terrain.MOUNTAINS, new Color(0.4f, 0.3f, 0.4f) },
                { Terrain.HIGHLAND, new Color(0.5f, 0.5f, 0.7f) },
                { Terrain.FOREST, new Color(0.1f, 0.4f, 0.1f) },
                { Terrain.CAVE, new Color(0.1f, 0.4f, 0.5f) }
            };
        }

        UpdateLabel(n);
        UpdateColor(n);
    }

    private Color get_node_color(Node n)
    {
        var t = n.ProvinceData.Terrain;

        foreach (var pair in m_colors)
        {
            if (t.IsFlagSet(pair.Key))
            {
                return pair.Value;
            }
        }

        return new Color(0.9f, 0.9f, 0.8f); //default is plains
    }

    public void UpdateColor(Node n)
    {
        Renderer.color = get_node_color(n);
    }

    public void UpdateLabel(Node n)
    {
        if (n.HasNation)
        {
            Text.gameObject.SetActive(true);
            Text.text = n.Nation.NationData.Name;
            Text.color = new Color(1.0f, 0.5f, 1.0f);
        }
        else if (n.ProvinceData.IsThrone)
        {
            Text.gameObject.SetActive(true);
            Text.text = "Throne";
            Text.color = new Color(1.0f, 0.3f, 0.3f);
        }
        else
        {
            Text.gameObject.SetActive(false);
            Text.text = string.Empty;
        }

        foreach (var m in TextOutlines)
        {
            if (Text.text == string.Empty)
            {
                m.gameObject.SetActive(false);
            }
            else
            {
                m.gameObject.SetActive(true);
            }

            m.text = Text.text;
        }
    }

    private void Update()
    {
        if (m_selected)
        {
            m_scale = 1.1f + 0.2f * Mathf.Sin(Time.time * 5.5f);
            Renderer.transform.localScale = new Vector3(m_scale, m_scale, 1.0f);
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ProvinceManager.s_province_manager.SetProvince(m_parent);

            m_parent.SetSelected(true);
        }
    }
}
