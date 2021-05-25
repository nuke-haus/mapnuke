using System.Collections.Generic;
using UnityEngine;

public class MeshSpriteRenderer : MonoBehaviour
{
    public class MeshBuilder
    {
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();
        private readonly List<Vector2> uv = new List<Vector2>();
        private readonly List<Color> colors = new List<Color>();

        public void Add(Sprite s, Vector3 pos, Color c, bool flip)
        {
            var n = vertices.Count;
            var min = s.bounds.min;
            var max = s.bounds.max;
            var dy = max.y - min.y;
            vertices.Add(new Vector3(min.x, min.y, 0) + pos);
            vertices.Add(new Vector3(max.x, min.y, 0) + pos);
            vertices.Add(new Vector3(min.x, max.y, -dy * 0.1f) + pos);
            vertices.Add(new Vector3(max.x, max.y, -dy * 0.1f) + pos);
            colors.Add(c);
            colors.Add(c);
            colors.Add(c);
            colors.Add(c);
            var factor = new Vector2(1.0f / s.texture.width, 1.0f / s.texture.height);
            var r = s.textureRect;

            if (flip)
            {
                uv.Add(new Vector2(r.xMax, r.yMin) * factor);
                uv.Add(new Vector2(r.xMin, r.yMin) * factor);
                uv.Add(new Vector2(r.xMax, r.yMax) * factor);
                uv.Add(new Vector2(r.xMin, r.yMax) * factor);
            }
            else
            {
                uv.Add(new Vector2(r.xMin, r.yMin) * factor);
                uv.Add(new Vector2(r.xMax, r.yMin) * factor);
                uv.Add(new Vector2(r.xMin, r.yMax) * factor);
                uv.Add(new Vector2(r.xMax, r.yMax) * factor);
            }

            triangles.Add(0 + n);
            triangles.Add(3 + n);
            triangles.Add(1 + n);

            triangles.Add(0 + n);
            triangles.Add(2 + n);
            triangles.Add(3 + n);
        }

        public Mesh Build()
        {
            var mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }
    }

    private readonly MeshBuilder builder = new MeshBuilder();
    private bool m_dirty = false;

    public List<Vector3> sprite_pos = new List<Vector3>();
   
    public void Add(Sprite s, Vector3 pos, bool flip = false)
    {
        Add(s, pos, Color.white, flip);
    }

    public void Add(Sprite s, Vector3 pos, Color c, bool flip = false)
    {
        if (s == null) return;
        InitMaterial(s.texture);
        sprite_pos.Add(pos);
        m_dirty = true;
        builder.Add(s, pos, c, flip);
    }

    public void SetMesh(Mesh m)
    {
        m_dirty = false;
        var mf = GetComponent<MeshFilter>();
        mf.mesh = m;
        var mr = GetComponent<MeshRenderer>();
        mr.material = m_mat;
    }

    private static bool INIT_ATLAS_MATERIAL = false;
    public Material atlas_material;
    private static Material m_mat;
    private static Texture2D m_set_texture;

    private void InitMaterial(Texture2D t)
    {
        // TODO(johan): This is a hack and only works when all sprites comes from the same atlas, make proper fix! 
        if (INIT_ATLAS_MATERIAL)
        {
            /*if (t != set_texture)
            {
                Debug.LogWarning("Trying to write wrong texture.");
            }*/
            return;
        }

        INIT_ATLAS_MATERIAL = true;
        m_set_texture = t;
        m_mat = Instantiate(atlas_material);
        m_mat.mainTexture = t;
    }

    public MeshRenderer Mesh => GetComponent<MeshRenderer>();

    public void Build()
    {
        if (m_dirty)
        {
            SetMesh(builder.Build());
        }

    }

    public Bounds Bounds => GetComponent<MeshRenderer>().bounds;

    // Update is called once per frame
    private void LateUpdate()
    {
        Build();
    }
}
