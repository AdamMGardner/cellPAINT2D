using UnityEngine;
using System.Collections;

public class resizeQuad : MonoBehaviour
{
    private float x;
    private float y;
    public Camera cam;
    public GameObject Background_Quad;

    public float last_size=0;
    private float last_size_max = 0;
    private GameObject duplicate;

    //Resizes the background quad to the screen resolution on the start of the program.
    
    void Start()
    {
        CameraMove cm=cam.GetComponent<CameraMove>();
        cam.orthographicSize = cm.cameraZoomMax;
        y = cm.cameraZoomMax * 20.0f; //Orthograpic size is half so it must be multiplied by 2.
        x = y * cam.aspect; //multiplies X by the aspect ratio of the screen.
        Background_Quad.transform.localScale = new Vector3(x, y, 1);
        last_size = cm.cameraZoomMax;
        last_size_max = last_size;
        cam.orthographicSize = cm.cameraCurrentZoom;
    }

    void Update() {
        return;
    }
}
