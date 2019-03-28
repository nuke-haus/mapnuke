using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureCam: MonoBehaviour
{
    public static CaptureCam s_capture_cam;

    public Camera Camera
    {
        get
        {
            return m_cam;
        }
    }

    Camera m_cam;
    bool m_defer_render = false;

    void Start()
    {
        s_capture_cam = this;

        m_cam = GetComponent<Camera>();
        m_cam.enabled = false;
    }

    void Update()
    { 
        if (m_defer_render)
        {
            m_cam.Render();
            m_defer_render = false;
        }
    }

    public void Render()
    {
        m_defer_render = true;
    }
}
