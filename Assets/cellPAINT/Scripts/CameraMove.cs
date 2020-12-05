using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {
    public bool use_Viewport;
    public bool zoomToMouse;
    public float dragSpeed = 1;
    public float zoomDragSpeed = 0.15f;
    public float rotation_speed = 2.0f;
    public float elapsedTime;
    public float last_elapasedTime;
    private Vector3 dragOrigin;
    private float ZrotOrigin;
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
    public int zoomvalue = 2;
    public Vector3 pmousePos;
    public Vector3 pmousePosWorld;
    public Vector3 current_mouse_world;
    public int cameraCurrentZoom = 20;
    public int cameraZoomMax = 20;
    public int cameraZoomMin = 5;

    public bool wheel_start = false;
    public float wheel_start_time = 0;
    public float wheel_start_time_threshold = 0.1f;
    void Start()
    {
        Main_Camera.orthographicSize = cameraZoomMax;
        vertExtent = (Main_Camera.ScreenToWorldPoint(new Vector3(0.0f, Main_Camera.pixelRect.height, 0.0f)).y);
        horzExtent = (Main_Camera.ScreenToWorldPoint(new Vector3(Main_Camera.pixelRect.width, 0.0f, 0.0f)).x);
        Main_Camera.orthographicSize = cameraCurrentZoom;
        ChildTransSize();
        if (use_Viewport) Main_Camera.rect = new Rect (0.25f,1,0,1);
        ZrotOrigin = 0.0f;
        elapsedTime = Time.realtimeSinceStartup;
    }
    
    void OnGUI()
    {
        Event e = Event.current;
        if (e.isMouse)
        {
            if ( e.rawType == EventType.ScrollWheel ) {
                Debug.Log("mouse delta ScrollWheel is " + e.delta.ToString());
            }
        }
    }

    void Update()
    {
        if (cameraCurrentZoom != Main_Camera.orthographicSize) {
            cameraCurrentZoom = (int) Main_Camera.orthographicSize;
        }
        //dragOrigin = new Vector3(Input.mousePosition.x, Input.mousePosition.y,-3000);
        if (Manager.Instance.mask_ui) return;
        if (Input.GetAxis("Mouse ScrollWheel")!=0 && !wheel_start) {
            wheel_start = true;
            wheel_start_time = Time.realtimeSinceStartup;
            Vector3 current_mouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -3000);
            current_mouse_world = Main_Camera.ScreenToWorldPoint(current_mouse);          
            if (Manager.Instance.selected_instance!=null)
            {
                current_mouse_world = Manager.Instance.selected_instance.transform.position;
            }              
        }
        if (Input.GetAxis("Mouse ScrollWheel")==0 && wheel_start) {
            if ((Time.realtimeSinceStartup - wheel_start_time) > wheel_start_time_threshold) {
                Debug.Log((Time.realtimeSinceStartup - wheel_start_time).ToString());
                wheel_start = false;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // back
        {
            //if (cameraCurrentZoom < cameraZoomMax)
            //{

            Zoom(-zoomvalue);
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
            Zoom(zoomvalue);
            ChildTransSize();
            cameraMapping();
            ZoomMove();
            PositionOutOfBounds();
            //  }
        }
        else if (Input.GetMouseButtonDown(2)){
            //ZrotOrigin = Main_Camera.transform.eulerAngles.z;
            dragOrigin = new Vector3(Input.mousePosition.x, Input.mousePosition.y,0);
            Vector3 target_position = Vector3.zero;
            if (Manager.Instance.selected_instance!=null){
                target_position = Manager.Instance.selected_instance.transform.position;
                Main_Camera.transform.position = target_position;
                Main_Camera.transform.position = new Vector3(Main_Camera.transform.position.x,Main_Camera.transform.position.y,-3000);
                //Main_Camera.transform.LookAt(target_position, Vector3.up);                
            }
        }
        else if (Input.GetMouseButtonDown(1)) {
            dragOrigin = new Vector3(Input.mousePosition.x, Input.mousePosition.y,-3000);
        }
        else if (Input.GetMouseButton(1))
        {
            //cameraMapping();
            Move();
            //PositionOutOfBounds();
        }
        else if (Input.GetMouseButton(2)){
            Vector3 target_position = Vector3.zero;
            if (Manager.Instance.selected_instance!=null){
                target_position = Manager.Instance.selected_instance.transform.position;
            }
            //rotate around Z by smooth increment
            //Main_Camera.transform.eulerAngles += Vector3.forward * (rotation_speed * Time.deltaTime);
            Vector3 current_mouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            //Vector3 current_mouse_world = Main_Camera.ScreenToWorldPoint(current_mouse);
            //Vector3 dragOrigin_world = Main_Camera.ScreenToWorldPoint(dragOrigin);
            Vector3 d = (current_mouse - dragOrigin);
            float sign = (d.x < 0.0f )? -1.0f : 1.0f;
            float amount  = d.magnitude*rotation_speed*sign ;
            ZrotOrigin+=amount;
            //Main_Camera.transform.eulerAngles += Vector3.forward * (rotation_speed * ZrotOrigin) * sign;
            Main_Camera.transform.rotation = Quaternion.AngleAxis(ZrotOrigin, Vector3.forward);
            dragOrigin = current_mouse;
            //Main_Camera.orthographicSize = cameraZoomMax;
            //vertExtent = Main_Camera.transform.InverseTransformVector(Main_Camera.ScreenToWorldPoint(new Vector3(0.0f, Main_Camera.pixelRect.height, 0.0f))).y;
            //horzExtent = Main_Camera.transform.InverseTransformVector(Main_Camera.ScreenToWorldPoint(new Vector3(Main_Camera.pixelRect.width, 0.0f, 0.0f))).x;
            //Main_Camera.orthographicSize = cameraCurrentZoom;
        }
        else { }
        if (use_Viewport) {
            Main_Camera.rect = new Rect (0.25f,1,0,1);
        }
        pmousePos = Input.mousePosition;
    }

    void cameraMapping()
    {
        cameraPos3D = Main_Camera.transform.position;
        mapYPre = Main_Camera.transform.InverseTransformVector(Main_Camera.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, 0.0f)));
        mapXPre = Main_Camera.transform.InverseTransformVector(Main_Camera.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, 0.0f)));

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
        Vector3 cmouse_world = Main_Camera.ScreenToWorldPoint(current_mouse);
        Vector3 dragOrigin_world = Main_Camera.ScreenToWorldPoint(dragOrigin);
        pos = (cmouse_world - dragOrigin_world) * dragSpeed * mouseInv;
        
        //MoveOutOfBounds();

        move = new Vector3(pos.x, pos.y, 0.0f);
        Main_Camera.transform.Translate(Main_Camera.transform.InverseTransformVector(move));
        //Main_Camera.transform.Translate(move);
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
        UI_manager.Get.pixelscale_slider.value = cameraCurrentZoom;
    }

    void ZoomMove()
    {
        Vector3 current_mouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -3000);
        //Vector3 current_mouse_world = Main_Camera.ScreenToWorldPoint(current_mouse);
        //if (pmousePosWorld!= null && Vector3.Distance(Input.mousePosition,pmousePos) < threshold_mouse){
        //    current_mouse_world = pmousePosWorld;
        // }
        // else {
        //    pmousePosWorld = current_mouse_world;
        //}
        //MoveOutOfBounds();

        if (!zoomToMouse)
        {
            if (Manager.Instance.selected_instance!=null)
            {
                current_mouse_world = Manager.Instance.selected_instance.transform.position;
            }
            else
            {
                current_mouse_world = Main_Camera.transform.position;
            }
        }     
        else {
            if (wheel_start) {
                //current_mouse_world = Main_Camera.ScreenToWorldPoint(pmousePos);        
            }
        }  
        move = new Vector3(current_mouse_world.x, current_mouse_world.y, -3000.0f);
        float zoomDragMax = zoomDragSpeed *(cameraCurrentZoom/cameraZoomMin);
        Vector3 newPos = Vector3.MoveTowards (cameraPos3D, move, zoomDragMax);
        newPos = new Vector3 (newPos.x, newPos.y, -3000.0f);
        Main_Camera.transform.position = newPos;
        cameraPos3D = Main_Camera.transform.position;

        /*
        if (!zoomToMouse & Manager.Instance.selected_instance!=null)
        {
            current_mouse_world = Manager.Instance.selected_instance.transform.position;
        }
        else if (zoomToMouse)
        {
            move = new Vector3(current_mouse_world.x, current_mouse_world.y, -3000.0f);
            float zoomDragMax = zoomDragSpeed *(cameraCurrentZoom/cameraZoomMin);
            Vector3 newPos = Vector3.MoveTowards (cameraPos3D, move, zoomDragMax);
            //newPos = newPos * (curren)
            newPos = new Vector3 (newPos.x, newPos.y, -3000.0f);
            Main_Camera.transform.position = newPos;
            //dragOrigin = new Vector3(Input.mousePosition.x, Input.mousePosition.y,-3000);
            cameraPos3D = Main_Camera.transform.position;
        }
        else
        {
            current_mouse_world = Vector3.zero;
            move = new Vector3(current_mouse_world.x, current_mouse_world.y, -3000.0f);
            float zoomDragMax = zoomDragSpeed *(cameraCurrentZoom/cameraZoomMin);
            Vector3 newPos = Vector3.MoveTowards (cameraPos3D, move, zoomDragMax);
            //newPos = newPos * (curren)
            newPos = new Vector3 (newPos.x, newPos.y, -3000.0f);
            Main_Camera.transform.position = newPos;
            //dragOrigin = new Vector3(Input.mousePosition.x, Input.mousePosition.y,-3000);
            cameraPos3D = Main_Camera.transform.position;
        }
        */
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
