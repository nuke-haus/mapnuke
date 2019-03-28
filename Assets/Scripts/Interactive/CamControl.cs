using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behaviour class for main camera.
/// Controls how the user's mouse affects the camera.
/// </summary>
public class CamControl: MonoBehaviour
{
    public float MaxSize;
    public float MinSize;

    const float INTERPOLATE_CAM = 0.48f;
    const float INTERPOLATE_POS = 0.01f;
    const float INTERPOLATE_DRAG = 0.03f;
    const float SENSITIVITY = 0.20f;

    bool m_toggle_sprites = false;
    float m_target = 5.0f;
    Camera m_cam;
    Vector2 m_lastpos;

	void Start()
    {
        m_cam = Camera.main;
        m_lastpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

	void Update()
    {
        if (Mathf.Abs(m_cam.orthographicSize - m_target) < 0.001f)
        {
            return;
        }

        float size = m_cam.orthographicSize;
        m_cam.orthographicSize = Mathf.SmoothStep(m_cam.orthographicSize, m_target, INTERPOLATE_CAM);

        Vector3 pos = m_cam.transform.position;
        Vector3 newpos = Vector3.Lerp(pos, m_lastpos, INTERPOLATE_POS);

        m_cam.transform.position = newpos;
    }

    public void OnGUI()
    {
        if (Event.current.type == EventType.ScrollWheel)
        {
            scroll(Event.current.delta.y);
        }

        if (Event.current.type == EventType.MouseDrag)
        {
            mouse_drag(Event.current.delta);
        }
    }

    public void ToggleSprites()
    {
        if (m_toggle_sprites)
        {
            m_cam.cullingMask = -1;
        }
        else
        {
            m_cam.cullingMask = 823;
        }

        m_toggle_sprites = !m_toggle_sprites;
    }

    void mouse_drag(Vector2 delta)
    {
        m_lastpos = Camera.main.transform.position;

        Vector3 pos = m_cam.transform.position;
        Vector3 newpos = pos + new Vector3(delta.x * -INTERPOLATE_DRAG, delta.y * INTERPOLATE_DRAG, 0);

        m_cam.transform.position = newpos;
    }

    void scroll(float delta)
    {
        m_target = m_target + delta * SENSITIVITY;
        m_lastpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (m_target > MaxSize)
        {
            m_target = MaxSize;
        }

        if (m_target < MinSize)
        {
            m_target = MinSize;
        }
    }
}
