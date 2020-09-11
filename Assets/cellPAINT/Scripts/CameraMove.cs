using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {
    public float dragSpeed = 1;
    public float zoomDragSpeed = 0.15f;
    private Vector3 dragOrigin;
    public Camera Main_Camera;
    private float mapY;
    private float mapX;
    private float minX;
    private float maxX;
    private float minY;
    private float maxY;
    private float vertExtent;
    private float horzExtent;
    private Vector3 pos;
    public Vector3 cameraPos3D;
    private Vector3 mapYPre;
    private Vector3 mapXPre;
    public Vector3 move;
    public bool use_mouse;
    public float threshold_mouse = 0.1f;
    public Vector3 pmousePos;
    public Vector3 pmousePosWorld;
    public int cameraCurrentZoom = 20;
    public int cameraZoomMax = 20;
    public int cameraZoomMin = 5;

    void Start()
    {
        Main_Camera.orthographicSize = cameraZoomMax;
        vertExtent = (Main_Camera.ScreenToWorldPoint(new Vector3(0.0f, Main_Camera.pixelRect.height, 0.0f)).y);
        horzExtent = (Main_Camera.ScreenToWorldPoint(new Vector3(Main_Camera.pixelRect.width, 0.0f, 0.0f)).x);
        Main_Camera.orthographicSize = cameraCurrentZoom;
        ChildTransSize();
    }

    void Update()
    {
        //dragOrigin = new Vector3(Input.mousePosition.x, Input.mousePosition.y,-3000);
        if (Manager.Instance.mask_ui) return;
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // back
        {
            //if (cameraCurrentZoom < cameraZoomMax)
            //{

            Zoom(-1);
            ChildTransSize();
            cameraMapping();
            ZoomMove();
            PositionOutOfBounds();
            // }
        }

        else if (Input.GetAxis("Mouse ScrollWheel") < 0) // forward
        {
            // if (cameraCurrentZoom > cameraZoomMin)
            // {
            Zoom(1);
            ChildTransSize();
            cameraMapping();
            ZoomMove();
            PositionOutOfBounds();
            //  }
        }
        else if (Input.GetMouseButtonDown(2)||Input.GetMouseButtonDown(1)) {
            dragOrigin = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2)||Input.GetMouseButton(1))
        {
            cameraMapping();
            Move();
            PositionOutOfBounds();
        }
        else { }
        pmousePos = Input.mousePosition;
    }

    void cameraMapping()
    {
        cameraPos3D = Main_Camera.transform.position;
        mapYPre = Main_Camera.ScreenToWorldPoint(new Vector3(0.0f, Main_Camera.pixelRect.height, 0.0f));
        mapXPre = Main_Camera.ScreenToWorldPoint(new Vector3(Main_Camera.pixelRect.width, 0.0f, 0.0f));

        mapY = mapYPre.y - cameraPos3D.y;
        mapX = mapXPre.x - cameraPos3D.x;

        maxX = horzExtent - mapX;
        minX = mapX - horzExtent;
        maxY = vertExtent - mapY;
        minY = mapY - vertExtent;
    }

    void Move()
    {
        int mouseInv = (use_mouse) ? -1 : 1;
        Vector3 current_mouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -3000);
        Vector3 current_mouse_world = Main_Camera.ScreenToWorldPoint(current_mouse);
        Vector3 dragOrigin_world = Main_Camera.ScreenToWorldPoint(dragOrigin);
        pos = (current_mouse_world - dragOrigin_world) * dragSpeed * mouseInv;
        
        MoveOutOfBounds();

        move = new Vector3(pos.x, pos.y, 0.0f);

        Main_Camera.transform.Translate(move);
        dragOrigin = new Vector3(Input.mousePosition.x, Input.mousePosition.y,-3000);
        cameraPos3D = Main_Camera.transform.position;
    }

    void MoveOutOfBounds()
    {
        if (Main_Camera == null) return;

        if (cameraPos3D.x >= maxX)
        {
            if (pos.x > 0) { pos.x = 0.0f; cameraPos3D.x = maxX; };
        }
        if (cameraPos3D.x <= minX)
        {
            if (pos.x < 0) { pos.x = 0.0f; cameraPos3D.x = minX; };
        }
        if (cameraPos3D.y >= maxY)
        {
            if (pos.y > 0) { pos.y = 0.0f; cameraPos3D.y = maxY; };
        }
        if (cameraPos3D.y <= minY)
        {
            if (pos.y < 0) { pos.y = 0.0f; cameraPos3D.y = minY; };
        }
        Main_Camera.transform.position = cameraPos3D;
    }

    void Zoom(int zoomDir)
    {
        cameraCurrentZoom = (int)Mathf.Max(cameraZoomMin, Mathf.Min(cameraCurrentZoom + zoomDir, cameraZoomMax));
        Main_Camera.orthographicSize = cameraCurrentZoom;
    }

    void ZoomMove()
    {
        Vector3 current_mouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -3000);
        Vector3 current_mouse_world = Main_Camera.ScreenToWorldPoint(current_mouse);
        /*if (pmousePosWorld!= null && Vector3.Distance(Input.mousePosition,pmousePos) < threshold_mouse){
            current_mouse_world = pmousePosWorld;
        }
        else {
            pmousePosWorld = current_mouse_world;
        }*/
        //MoveOutOfBounds();
        if (Manager.Instance.selected_instance!=null){
            current_mouse_world = Manager.Instance.selected_instance.transform.position;
        }
        move = new Vector3(current_mouse_world.x, current_mouse_world.y, -3000.0f);
        float zoomDragMax = zoomDragSpeed *(cameraCurrentZoom/cameraZoomMin);
        Vector3 newPos = Vector3.MoveTowards (cameraPos3D, move, zoomDragMax);
        //newPos = newPos * (curren)
        newPos = new Vector3 (newPos.x, newPos.y, -3000.0f);
        Main_Camera.transform.position = newPos;
        //dragOrigin = new Vector3(Input.mousePosition.x, Input.mousePosition.y,-3000);
        cameraPos3D = Main_Camera.transform.position;
    }

    void PositionOutOfBounds()
    {
        if (Main_Camera == null) return;
        if (cameraPos3D.x > maxX) { cameraPos3D.x = maxX; };
        if (cameraPos3D.x < minX) { cameraPos3D.x = minX; };
        if (cameraPos3D.y > maxY) { cameraPos3D.y = maxY; };
        if (cameraPos3D.y < minY) { cameraPos3D.y = minY; };

        Main_Camera.transform.position = cameraPos3D;
    }

    void ChildTransSize()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<Camera>().orthographicSize = Main_Camera.orthographicSize;
        }
    }
    public void ToggleMouse()
    {
        if (use_mouse == false) use_mouse = true;
        else if (use_mouse == true) use_mouse = false;
        else return;
    }
}
