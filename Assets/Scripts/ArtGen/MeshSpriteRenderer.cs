using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSpriteRenderer : MonoBehaviour
{
    public class MeshBuilder
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        List<Color> colors = new List<Color>();

        public void Add(Sprite s, Vector3 pos, Color c, bool flip)
        {
            int n = vertices.Count;
            var min = s.bounds.min;
            var max = s.bounds.max;
            float dy = max.y - min.y;
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

            } else
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
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }
    }

    MeshBuilder builder = new MeshBuilder();

    public List<Vector3> sprite_pos = new List<Vector3>();


    bool dirty = false;
    public void Add(Sprite s, Vector3 pos, bool flip = false)
    {
        Add(s, pos, Color.white, flip);
    }

    public void Add(Sprite s, Vector3 pos, Color c, bool flip = false)
    {
        if (s == null) return;
        InitMaterial(s.texture);
        sprite_pos.Add(pos);
        dirty = true;
        builder.Add(s, pos, c, flip);
    }

    public void SetMesh(Mesh m)
    {
        dirty = false;
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = m;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material = mat;
    }
    static bool init_atlas_material = false;
    public Material atlas_material;

    static Material mat;
    static Texture2D set_texture;

    void InitMaterial(Texture2D t)
    {
        // TODO(johan): This is a hack and only works when all sprites comes from the same atlas, make proper fix! 
        if (init_atlas_material)
        {
            if (t != set_texture)
            {
                Debug.LogWarning("Trying to write wrong texture.");
            }
            return;
        }
        init_atlas_material = true;
        set_texture = t;
        mat = Instantiate(atlas_material);
        mat.mainTexture = t;
    }

    public MeshRenderer mesh => GetComponent<MeshRenderer>();

    public void Build ()
    {
        if (dirty)
        {
            SetMesh(builder.Build());
        }

    }

    public Bounds bounds => GetComponent<MeshRenderer>().bounds;

    // Update is called once per frame
    void LateUpdate()
    {
        Build();
    }
}
