using UnityEngine;
using System.Collections;

public class proceduralGrid : MonoBehaviour {
    public GameObject prefab;
    public int Ninstance;
    public Camera cam;
    private HaltonSequence halton;
    
    private Rigidbody2D[] everything;

    public float scale_force = 1.0f;
    public float timeScale = 1.0f;

    public void change_scale(float val) {
        scale_force = val;
    }

    // Use this for initialization
    void Start() {
        GameObject root = GameObject.Find("root");
        float y = cam.orthographicSize * 2.0f; //Orthograpic size is half so it must be multiplied by 2.
        float x = y * cam.aspect; //multiplies X by the aspect ratio of the screen.
        halton = new HaltonSequence();
        float w = x;
        float h = y;
        float side = Mathf.Sqrt(Ninstance);
        everything = new Rigidbody2D[Ninstance];

        for (int i = 0; i < Ninstance; i++) 
        {            
            GameObject instance = GameObject.Instantiate(prefab) as GameObject;
            instance.transform.position = new Vector3(halton.m_CurrentPos.x*w-w/2.0f, halton.m_CurrentPos.y*h-h / 2.0f, 0.0f);
            instance.transform.parent = root.transform;
            instance.SetActive(true);
            halton.Increment();
            Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
            everything[i] = rb;
        }
    }
}
