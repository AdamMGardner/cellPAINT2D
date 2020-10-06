using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseIconManager : MonoBehaviour
{
    public Camera Camera;
    public Texture2D eraseMouseIcon;
    public Texture2D drawMouseIcon;
    public Texture2D pinMouseIcon;

    public Slider radiusSlider;
    public Slider InstanceNumberSlider;

    public GameObject mouseCursor;

    public bool defaultMouseActive = true;

    private bool drawMode = true;
    private GameObject radiusCursor;
    private GameObject pinCursor;
    private GameObject pinToCursor;
    private GameObject groupCursor;
    private GameObject lockCursor;
    private GameObject measureCursor;
    private GameObject eraseCursor;
    private GameObject dragCursor;
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;

        radiusCursor = mouseCursor.transform.Find("mouseCursorRadius").gameObject;
        pinCursor = mouseCursor.transform.Find("Pin_Icon").gameObject;
        pinToCursor = mouseCursor.transform.Find("PinTo_Icon").gameObject;
        groupCursor = mouseCursor.transform.Find("Group_Icon").gameObject;
        lockCursor = mouseCursor.transform.Find("Lock_Icon").gameObject;
        measureCursor = mouseCursor.transform.Find("Measure_Icon").gameObject;
        eraseCursor = mouseCursor.transform.Find("Erase_Icon").gameObject;
        dragCursor = mouseCursor.transform.Find("Drag_Icon").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mouseCursor.transform.position = Camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0.0f));
        ChangeCursorRadius();

        if (InstanceNumberSlider.value == 1 | drawMode == false)
        {
            radiusCursor.SetActive(false);
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
        if (InstanceNumberSlider.value > 1 & drawMode == true)
        {
            radiusCursor.SetActive(true);
            radiusCursor.transform.localScale = new Vector3 (radiusSlider.value,radiusSlider.value,radiusSlider.value);
        }       
    }
    public void ClearCursor()
    {
        drawMode = false;
        radiusCursor.SetActive(false);
        pinCursor.SetActive(false);
        pinToCursor.SetActive(false);
        groupCursor.SetActive(false);
        lockCursor.SetActive(false);
        measureCursor.SetActive(false);
        eraseCursor.SetActive(false);
        dragCursor.SetActive(false);
    }

    public void ToggleDrawCursor()
    {
        ClearCursor();
        drawMode = true;
    }
    
    public void TogglePinCursor()
    {
        ClearCursor();
        pinCursor.SetActive(true); 
    }

    public void TogglePinToCursor()
    {
        ClearCursor();
        pinToCursor.SetActive(true);   
    }

    public void ToggleGroupCursor()
    {
        ClearCursor();
        groupCursor.SetActive(true);    
    }

    public void ToggleLockCursor()
    {
        ClearCursor();
        lockCursor.SetActive(true);
    }
        
    public void ToggleMeasureCursor()
    {
        ClearCursor();
        measureCursor.SetActive(true);    
    }

    public void ToggleDragCursor()
    {
        ClearCursor();
        dragCursor.SetActive(true);   
    }

    public void ToggleEraseCursor()
    {
        ClearCursor();
        eraseCursor.SetActive(true);   
    }
}
