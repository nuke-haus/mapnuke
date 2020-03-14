using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureCam: MonoBehaviour
{
    public static CaptureCam s_capture_cam;

    public static Bounds Bounds(Vector3 offset = new Vector3())
    {
        var campos = CaptureCam.s_capture_cam.transform.position;
        var camera_h = CaptureCam.s_capture_cam.Camera.orthographicSize * 2;
        var camera_w = CaptureCam.s_capture_cam.Camera.aspect * camera_h;
        Bounds cam_bounds = new Bounds(campos + offset, new Vector3(camera_w, camera_h));

        return cam_bounds;
    }

    public Camera Camera
    {
        get
        {
            return m_cam;
        }
    }

    Camera m_cam;
    bool m_defer_render = false;

    void Awake()
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
