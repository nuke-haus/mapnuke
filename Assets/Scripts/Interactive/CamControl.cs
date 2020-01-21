using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Behaviour class for main camera.
/// Controls how the user's mouse affects the camera.
/// </summary>
public class CamControl: MonoBehaviour
{
    public float MaxSize;
    public float MinSize;

    [SerializeField]
    private EventSystem eventSystem;

    const float INTERPOLATE_CAM = 0.48f;
    const float INTERPOLATE_POS = 0.01f;
    const float SENSITIVITY = 0.20f;

    bool m_toggle_sprites = false;
    float m_target = 5.0f;
    Camera m_cam;
    Vector2 m_lastpos;
    bool is_dragging;
    Vector2 drag_world_pos;

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
            if (!eventSystem.IsPointerOverGameObject()) scroll(Event.current.delta.y);
        }
        if (Event.current.type == EventType.MouseDown)
        {
            // Don't drag if we are over a UI element like a button.
            is_dragging = !eventSystem.IsPointerOverGameObject();
            drag_world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Event.current.type == EventType.MouseDrag)
        {
            if (is_dragging) mouse_drag();
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

    void mouse_drag()
    {
        m_lastpos = Camera.main.transform.position;

        Vector2 next_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 delta = drag_world_pos - next_pos;

        Vector3 pos = m_cam.transform.position;
        Vector3 newpos = pos + new Vector3(delta.x, delta.y, 0);

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
