using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawMeshContour : MonoBehaviour {

    //create a mesh and render it
    public List<Vector2> pos;
    public List<int> faces;
    public int[] indices;
    public Vector2[] uvs;
    public Material matToApply;
    public string iname;

    private MeshFilter meshFilter;
    public MeshRenderer mr;
    private bool running = false;
    void Start() {
        Setup();
        StartCoroutine(updateBackground());
    }

    void OnEnable() {
        Setup();
    }
    // Use this for initialization
    void Setup () {
        meshFilter = GetComponent<MeshFilter>();
        float offset = 0.5f;
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (mr == null)
        {
            mr = gameObject.AddComponent<MeshRenderer>();
        }
        /*
        if (!SceneManager.Instance.prefab_materials.ContainsKey(matToApply.name))
        {
            matToApply.color = gameObject.transform.GetChild(0).GetComponent<PrefabProperties>().Default_Sprite_Color;
        }

        if (matToApply != null)
        {
            if (!SceneManager.Instance.prefab_materials.ContainsKey(matToApply.name)) SceneManager.Instance.prefab_materials.Add(matToApply.name, matToApply);
        }
        */
        if ( matToApply!= null) mr.sharedMaterial = matToApply;

        //gather the  children
        int N = transform.childCount;
        pos = new List<Vector2>();
        faces = new List<int>();
        uvs = new Vector2[N];
        List<Vector3> vertices = new List<Vector3>();
        //int i = 0;
        foreach (Transform child in transform) {
            pos.Add(new Vector2 (child.localPosition.x, child.localPosition.y));
            if ((matToApply!=null)&&(matToApply.name == "HIVCAhex_bg"))
            {
                offset = 0.4f;
            }
            else
            {
                offset = 0.5f;
            }
            vertices.Add(new Vector3(child.localPosition.x, child.localPosition.y, child.localPosition.z+offset));
            //uvs[i] = new Vector2(child.position.x, child.position.z);
            //i++;
        }
        // Use the triangulator to get indices for creating triangles
        //Triangulate tr1 = new Triangulate();
        indices = Triangulate.Process(pos.ToArray());
        //Triangulator tr = new Triangulator(pos.ToArray());
        //indices = tr.Triangulate();

        //finish up
        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Bounds bounds = mesh.bounds;
        int i = 0;
        while (i < uvs.Length)
        {
            uvs[i] = new Vector2((vertices[i].x- bounds.min.x) / bounds.size.x, (vertices[i].y-bounds.min.y) / bounds.size.y);
            i++;
        }
        mesh.uv = uvs;
        meshFilter.sharedMesh = mesh;
        //mesh.Optimize();
    }

    IEnumerator updateBackground() {
        running = true;
        //gather the  children
        int N = transform.childCount;
        pos = new List<Vector2>();
        faces = new List<int>();
        uvs = new Vector2[N];
        List<Vector3> vertices = new List<Vector3>();
        //int i = 0;
        float offset = 0.5f;
        foreach (Transform child in transform)
        {
            pos.Add(new Vector2(child.localPosition.x, child.localPosition.y));
            if ((matToApply != null) && (matToApply.name == "HIVCAhex_bg"))
            {
                offset = 0.4f;
            }
            else
            {
                offset = 0.5f;
            }
            vertices.Add(new Vector3(child.localPosition.x, child.localPosition.y, child.localPosition.z + offset));
        }
        // Use the triangulator to get indices for creating triangles
        //Triangulate tr1 = new Triangulate();
        indices = Triangulate.Process(pos.ToArray());
        //Triangulator tr = new Triangulator(pos.ToArray());
        //indices = tr.Triangulate();

        //finish up

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            meshFilter.mesh = new Mesh();
            mesh = meshFilter.sharedMesh;
        }
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices;
        //mesh.RecalculateNormals();
        //mesh.RecalculateBounds();
        Bounds bounds = mesh.bounds;
        int i = 0;
        while (i < uvs.Length)
        {
            uvs[i] = new Vector2((vertices[i].x - bounds.min.x) / bounds.size.x, (vertices[i].y - bounds.min.y) / bounds.size.y);
            i++;
        }
        mesh.uv = uvs;
        yield return null;
        StartCoroutine(updateBackground());
    }

    // Update is called once per frame
    void Update () {
        //Setup();
        if (!running) StartCoroutine(updateBackground());
    }
}
