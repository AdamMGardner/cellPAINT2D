using UnityEngine;
using System.Collections;

public class resizeQuad : MonoBehaviour
{
    private float x;
    private float y;
    public Camera cam;
    public GameObject Background_Quad;

    private float last_size=0;
    private float last_size_max = 0;

    //Resizes the background quad to the screen resolution on the start of the program.
    void Start()
    {
        CameraMove cm=cam.GetComponent<CameraMove>();
        cam.orthographicSize = cm.cameraZoomMax;
        y = cm.cameraZoomMax * 2.0f; //Orthograpic size is half so it must be multiplied by 2.
        x = y * cam.aspect; //multiplies X by the aspect ratio of the screen.
        Background_Quad.transform.localScale = new Vector3(x, y, 1);
        last_size = cm.cameraZoomMax;
        last_size_max = last_size;
        cam.orthographicSize = cm.cameraCurrentZoom;
    }

    void Update() {
        y = last_size_max * 2.0f; //Orthograpic size is half so it must be multiplied by 2.
        x = y * cam.aspect; //multiplies X by the aspect ratio of the screen.
        Background_Quad.transform.localScale = new Vector3(x*2, y*2, 1);
        last_size = cam.orthographicSize;
        if (last_size > last_size_max)
            last_size_max = last_size;
    }
}
