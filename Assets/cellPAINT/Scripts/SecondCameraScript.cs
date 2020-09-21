using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SecondCameraScript : MonoBehaviour {

    public Camera mainCamera;
    public int down_sample;
    public Dictionary<float,string> mapping;
    private Color originalColor;
    private Camera cam;
    private Color[] color_back;
    private GameObject[] membranes;
    private RenderTexture compartment_texture;

    private static SecondCameraScript _instance;
    public static SecondCameraScript Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<SecondCameraScript>();
            if (_instance == null)
            {
                var go = GameObject.Find("Camera_compartment");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("Camera_compartment"); //{ hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<SecondCameraScript>();
            }
            return _instance;
        }
    }

    // Use this for initialization
    void Start () {
        cam = GetComponent<Camera>();
        color_back = new Color[500];
        mapping = new Dictionary<float, string>();
        compartment_texture = new RenderTexture(Screen.width / down_sample, Screen.height / down_sample,1);
        cam.targetTexture = compartment_texture;
        Manager.Instance.secondRenderTexture = compartment_texture;
    }

    void OnPreRender()
    {
        float r = 0.0f;
        int count = 0;
        membranes = GameObject.FindGameObjectsWithTag("MembraneChain");
        foreach (GameObject o in membranes) {
            Renderer ren = o.GetComponent<Renderer>();
            if (ren != null)
            {
                if (!mapping.ContainsKey(r))
                    mapping.Add(r,o.name);
                color_back[count] = ren.material.color;
                ren.material.color = new Color(r, 1, 1, 1);
                r += 0.01f;
                count++;
            }
        }
    }

    void OnPostRender()
    {
        int count = 0;
        foreach (GameObject o in membranes)
        {
            Renderer r = o.GetComponent<Renderer>();

            if (r != null)
            {
                r.material.color = color_back[count];
                count++;
            }
        }
    }
    
	// Update is called once per frame
	void Update () {
        if (mainCamera != null)
        {
            cam.orthographicSize = mainCamera.orthographicSize;
        }
	}
}
