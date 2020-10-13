using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseIconManager : MonoBehaviour
{
    public Camera Camera;
    //public Texture2D eraseMouseIcon;
    //public Texture2D drawMouseIcon;
    //public Texture2D pinMouseIcon;

    public Slider radiusSlider;
    public Slider InstanceNumberSlider;

    public GameObject mouseCursor;

    public bool defaultMouseActive = true;

    public bool drawMode = true;
    public bool cursorsLoaded;
    private GameObject radiusCursor;
    private GameObject pinCursor;
    private GameObject pinToCursor;
    private GameObject groupCursor;
    private GameObject lockCursor;
    private GameObject measureCursor;
    private GameObject eraseCursor;
    private GameObject dragCursor;
    /* use actual cursor API */
    public bool change_mouse_cursor = false;
    public bool use_mouse_gameobject = false;
    [SerializeField]
    public List<Texture2D> cursors_texture;
    [SerializeField]
    public List<Vector2> cursors_offset;
    [SerializeField]
    public List<string> cursors_name;//correspond to the mode
    public CursorMode cursorMode = CursorMode.Auto;
    public Texture2D current_texture;
    public Vector2 current_offset;
    public string current_mode;

    public bool force_change = false;
    // Start is called before the first frame update
    private static MouseIconManager _instance = null;
    public static MouseIconManager Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<MouseIconManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("MouseIconManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("MouseIconManager");
                _instance = go.AddComponent<MouseIconManager>();
                //_instance.hideFlags = HideFlags.HideInInspector;
            }
            return _instance;
        }
    }

    void Start()
    {
        //Cursor.visible = false;
        drawMode = true;
        radiusCursor = mouseCursor.transform.Find("mouseCursorRadius").gameObject;
        pinCursor = mouseCursor.transform.Find("Pin_Icon").gameObject;
        pinToCursor = mouseCursor.transform.Find("PinTo_Icon").gameObject;
        groupCursor = mouseCursor.transform.Find("Group_Icon").gameObject;
        lockCursor = mouseCursor.transform.Find("Lock_Icon").gameObject;
        measureCursor = mouseCursor.transform.Find("Measure_Icon").gameObject;
        eraseCursor = mouseCursor.transform.Find("Erase_Icon").gameObject;
        dragCursor = mouseCursor.transform.Find("Drag_Icon").gameObject;
        cursorsLoaded = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mouseCursor.transform.position = Camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0.0f));
        //scale ?
        ChangeCursorRadius();

        if (UI_manager.Get.ninstance_click.value == 1 || current_mode != "drawMode")
        {
            radiusCursor.SetActive(false);
        }
        if (force_change) {
            Cursor.SetCursor(current_texture, current_offset, cursorMode);
            force_change = false;
        }
    }

    public void UpdateMouseCursor(){
        current_mode = Manager.Instance.GetCurrentMode();
        var cursor_id = cursors_name.IndexOf(current_mode);
        current_texture = cursors_texture[cursor_id];
        current_offset = cursors_offset[cursor_id];
    }
    
    public void ChangeMouseCursor(bool value){
        if (!change_mouse_cursor) return;
        current_mode = Manager.Instance.GetCurrentMode();
        if (use_mouse_gameobject) {
            ToggleIconObject(current_mode);
        }
        else {
            var cursor_id = cursors_name.IndexOf(current_mode);
            current_texture = cursors_texture[cursor_id];
            current_offset = cursors_offset[cursor_id];
            Cursor.SetCursor(current_texture, current_offset, cursorMode);
        }
    }

    public void ToggleMouseCursorCustomUI(bool value) {
        if (!change_mouse_cursor) return;
        if (use_mouse_gameobject) {
            Cursor.visible = value;
        }
        else {
            Debug.Log("ToggleMouseCursorCustom "+value.ToString()+" "+current_mode);
            if (value) {
                Cursor.SetCursor(current_texture, current_offset, cursorMode);
                //Cursor.visible = ! (current_mode == "drawMode");
            } else {
               // Cursor.visible = true;
                Cursor.SetCursor(null, Vector2.zero, cursorMode);
            }   
        }
    }

    public void ToggleMouseCursorCustom(string name,bool value) {
        if (!change_mouse_cursor) return;
        var cursor_id = cursors_name.IndexOf(name);
        current_texture = cursors_texture[cursor_id];
        current_offset = cursors_offset[cursor_id];        
        if (value) {
            Cursor.SetCursor(current_texture, current_offset, cursorMode);
        } else {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
    }

    public void ChangeCursorColor()
    {
        //Draw = Green
        //Erase = Red

    }

    public void ChangeCursorRadius()
    {
        //Need to know if in draw mode and if is soluble.
        if (UI_manager.Get.ninstance_click.value > 1 && current_mode == "drawMode" && !Manager.Instance.fiberMode && !Manager.Instance.surfaceMode)
        {
            radiusCursor.SetActive(true);
            radiusCursor.transform.localScale = new Vector3 (Manager.Instance.radiusPerClick/4 , Manager.Instance.radiusPerClick/4, Manager.Instance.radiusPerClick/4);
        }       
    }

    public void ClearCursor()
    {
        if (!cursorsLoaded) Start();
        drawMode = false;
        radiusCursor.SetActive(false);
        if (use_mouse_gameobject) {
            pinCursor.SetActive(false);
            pinToCursor.SetActive(false);
            groupCursor.SetActive(false);
            lockCursor.SetActive(false);
            measureCursor.SetActive(false);
            eraseCursor.SetActive(false);
            dragCursor.SetActive(false);
        }
    }

    public void ToggleIconObject(string mode){
        ClearCursor();
        if (mode == "drawMode") {}
        if (mode == "pinMode") pinCursor.SetActive(true); 
        if (mode == "bindMode") pinToCursor.SetActive(true); 
        if (mode == "groupMode") groupCursor.SetActive(true); 
        if (mode == "ghostMode") lockCursor.SetActive(true); 
        if (mode == "measureMode") measureCursor.SetActive(true); 
        if (mode == "dragMode") dragCursor.SetActive(true); 
        if (mode == "eraseMode") eraseCursor.SetActive(true); 
    }

    public void ToggleDrawCursor()
    {
        ClearCursor();
        drawMode = true;
        if (use_mouse_gameobject) {}
        else ToggleMouseCursorCustom("drawMode",true);        
    }
    
    public void TogglePinCursor()
    {
        ClearCursor();
        if (use_mouse_gameobject) pinCursor.SetActive(true); 
        else ToggleMouseCursorCustom("pinMode",true);
    }

    public void TogglePinToCursor()
    {
        ClearCursor();
        if (use_mouse_gameobject) pinToCursor.SetActive(true);  
        else ToggleMouseCursorCustom("bindMode",true); 
    }

    public void ToggleGroupCursor()
    {
        ClearCursor();
        if (use_mouse_gameobject) groupCursor.SetActive(true);   
        else ToggleMouseCursorCustom("groupMode",true); 
    }

    public void ToggleLockCursor()
    {
        ClearCursor();
        if (use_mouse_gameobject) lockCursor.SetActive(true);
        else ToggleMouseCursorCustom("ghostMode",true);
    }
        
    public void ToggleMeasureCursor()
    {
        ClearCursor();
        if (use_mouse_gameobject) measureCursor.SetActive(true);  
        else ToggleMouseCursorCustom("measureMode",true);  
    }

    public void ToggleDragCursor()
    {
        ClearCursor();
        if (use_mouse_gameobject) dragCursor.SetActive(true); 
        else ToggleMouseCursorCustom("dragMode",true);  
    }

    public void ToggleEraseCursor()
    {
        ClearCursor();
        if (use_mouse_gameobject) eraseCursor.SetActive(true);
        else ToggleMouseCursorCustom("eraseMode",true);   
    }
}
