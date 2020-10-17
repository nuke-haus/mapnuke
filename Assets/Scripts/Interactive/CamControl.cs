using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Behaviour class for main camera.
/// Controls how the user's mouse manipulations affect the camera.
/// </summary>
public class CamControl : MonoBehaviour
{
    public float MaxSize;
    public float MinSize;

    public EventSystem eventSystem;
    private const float INTERPOLATE_CAM = 0.48f;
    private const float SENSITIVITY = 0.20f;
    private bool m_toggle_sprites = false;
    private float m_target = 5.0f;
    private Camera m_cam;
    private Vector2 m_lastpos;
    private bool m_is_dragging;
    private Vector2 m_drag_world_pos;
    private Vector2 m_prev_camera_pos;

    private void Start()
    {
        m_cam = Camera.main;
        m_lastpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void Update()
    {
        if (Mathf.Abs(m_cam.orthographicSize - m_target) < 0.001f)
        {
            return;
        }

        var size = m_cam.orthographicSize;
        m_cam.orthographicSize = Mathf.SmoothStep(m_cam.orthographicSize, m_target, INTERPOLATE_CAM);
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
            m_is_dragging = !eventSystem.IsPointerOverGameObject();
            m_drag_world_pos = m_cam.ScreenToWorldPoint(Input.mousePosition);
            m_prev_camera_pos = m_cam.transform.position;
        }

        if (Event.current.type == EventType.MouseDrag)
        {
            if (m_is_dragging) mouse_drag();
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

    private void mouse_drag()
    {
        m_lastpos = Camera.main.transform.position;

        if (m_prev_camera_pos != m_lastpos)
        {
            m_drag_world_pos += m_lastpos - m_prev_camera_pos;
        }

        Vector2 next_pos = m_cam.ScreenToWorldPoint(Input.mousePosition);
        var delta = m_drag_world_pos - next_pos;
        var pos = m_cam.transform.position;
        var newpos = pos + new Vector3(delta.x, delta.y, 0);

        m_cam.transform.position = newpos;
        m_prev_camera_pos = m_cam.transform.position;
    }

    private void scroll(float delta)
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
