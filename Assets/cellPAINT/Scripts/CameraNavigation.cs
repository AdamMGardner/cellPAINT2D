using UnityEngine;
using System.Collections;


public class CameraNavigation : MonoBehaviour
{
    private Vector3 mousePos;
    public int cameraCurrentZoom = 20;
    public int cameraZoomMax = 20;
    public int cameraZoomMin = 5;




    void Start()
    {
        Camera.main.orthographicSize = cameraCurrentZoom;
        foreach (Transform child in transform)
        {
            child.GetComponent<Camera>().orthographicSize = cameraCurrentZoom;
        }
    }
    void Update()
    {
        mousePos = Input.mousePosition;

        {
            if (mousePos.x < Screen.width * 0.15f) return;
            if (mousePos.y > Screen.height - Screen.height * 0.05f) return;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
        {
            if (cameraCurrentZoom < cameraZoomMax)
            {
                cameraCurrentZoom += 1;
                Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize + 1);
                foreach (Transform child in transform)
                {
                    child.GetComponent<Camera>().orthographicSize = Mathf.Max(child.GetComponent<Camera>().orthographicSize + 1);
                }
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            if (cameraCurrentZoom > cameraZoomMin)
            {
                cameraCurrentZoom -= 1;
                Camera.main.orthographicSize = Mathf.Min(Camera.main.orthographicSize - 1);
                foreach (Transform child in transform)
                {
                    child.GetComponent<Camera>().orthographicSize = Mathf.Min(child.GetComponent<Camera>().orthographicSize - 1);
                }
            }
        }
    }
}