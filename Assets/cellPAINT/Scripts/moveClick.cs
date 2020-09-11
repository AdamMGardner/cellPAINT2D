using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UIWidgets;
using UIWidgetsSamples;
using UnityEngine.EventSystems;
using UnityStandardAssets.Utility;
using UnityEngine.UI;

public class moveClick : MonoBehaviour {
    public GameObject myPrefab;
    public Camera cam;
    public GameObject root;
    public Vector3 mousePos;
    public Vector3 startPos;    // Start position of line
    public Vector3 endPos;    // End position of line
    public Material shadowMatSprites;
    public bool use_shadow = true;
    public bool surfaceMode = false;
    public bool fiberMode = false;
    public bool dragMode = false;
    public bool eraseMode = false;
    public bool pinMode = false;
    public bool continuous = false;

    //public TreeViewSample ui;
    public Image tools_toggle_image;
    public GameObject current_prefab;

    public float proteinArea;
    public float protienArea;
    public float screenArea;
    public float percentFilled;

    public int totalNprotein = 0;

    public int layer_frequence;
    public int nbInstancePerClick;

    private CircleCollider2D mouseCollide;
    private Vector3 lastEndPos;    // last End position of line
    private LineRenderer line;
    private Collider2D otherSurf;
    private bool mouseDown;

    private DragRigidbody2D dragger;
    private ErasePrefab eraser;
    private DrawPhysicsLine fiber;
    private float circleArea;
    private float boxArea;
    private float circleRadius;
    private Progressbar pb;
    private Text currentLabel;
    private Dictionary<string, int> proteins_count;
    private Dictionary<string, Text> proteins_ui_labels;
    private GameObject pushAway;

    // Use this for initialization
    void Start () {
        //ui.OnSelect.AddListener(SwitchPrefab);
        dragger = cam.GetComponent<DragRigidbody2D>();
        eraser = GetComponent<ErasePrefab>();
        fiber = GetComponent<DrawPhysicsLine>();
        pb = GameObject.Find("Progressbar").GetComponent<Progressbar>();
        totalNprotein = 0;
        proteins_count = new Dictionary<string, int>();
        proteins_ui_labels = new Dictionary<string, Text>();
        pushAway = transform.GetChild(1).gameObject;
        pushAway.SetActive(false);
    }

    public void SwitchPrefab(int dropItem, ListViewItem component)
    {
        //this is called from the user interface, swicth to draw directly
        
        //TreeViewSampleComponent comp = component as TreeViewSampleComponent;        
        //var selected = ui.SelectedIndex;
        //TreeViewSampleComponent test = ui.DataSource[selected] as TreeViewSampleComponent;
        //currentLabel = comp.Text;
        //string name = currentLabel.text.Split(":".ToCharArray()[0])[0];
        if (!proteins_count.ContainsKey(name)) {
            proteins_count.Add(name, 0);
            proteins_ui_labels.Add(name, currentLabel);
        }
       
        myPrefab = Resources.Load("Prefabs/" + name) as GameObject;
        
        if (current_prefab) GameObject.Destroy(current_prefab);
        if (!myPrefab) return;
        
        current_prefab = Instantiate(myPrefab, transform.position, Quaternion.identity) as GameObject;
        current_prefab.transform.parent = transform;
        current_prefab.GetComponent<Rigidbody2D>().isKinematic = true;
        PrefabProperties props = current_prefab.GetComponent<PrefabProperties>();
        foreach (CircleCollider2D coll in current_prefab.GetComponents<CircleCollider2D>())
        {
            coll.isTrigger = true;
            coll.enabled = false;
        }
        foreach (CircleCollider2D coll in current_prefab.GetComponentsInChildren<CircleCollider2D>())
        {
            coll.isTrigger = true;
            coll.enabled = false;
        }
        foreach (Rigidbody2D coll in current_prefab.GetComponentsInChildren<Rigidbody2D>())
        {
            coll.isKinematic = true;
        }
       
        surfaceMode = props.is_surface;
        fiberMode = props.is_fiber;
        if (fiberMode)
        {
            foreach (var c in current_prefab.GetComponents<CircleCollider2D>())
                Destroy(c);
            Destroy(current_prefab.GetComponent<BoxCollider2D>());
            fiber.updatePrefab(myPrefab);
            fiber.draw = true;
            //activate the push away empty.
            pushAway.SetActive(true);
            pushAway.GetComponent<CircleCollider2D>().radius = props.circle_radius*3.0f;
        }
        else {
            if (!surfaceMode) current_prefab.GetComponent<AddForce>().enabled = false;
            fiber.updatePrefab(null);
            fiber.draw = false;
            pushAway.SetActive(false);
        }
        if (tools_toggle_image)
        {
            tools_toggle_image.sprite = current_prefab.GetComponent<SpriteRenderer>().sprite;
            //tools_toggle_image.fillMethod = Image.FillMethod.Radial360;
            //tools_toggle_image.SetNativeSize();
            //ools_toggle_image.transform.localScale = new Vector3(props.encapsulating_radius, props.encapsulating_radius, props.encapsulating_radius)*2/10.0f;
        }
        GameObject.Find("ToggleBrush").GetComponent<Toggle>().isOn = true;
        //ToggleContinuous(true);//toggle the ui ?
    }

    public void ToggleContinuous(bool toggle) {
        continuous = toggle;
        dragMode = false;
        if (fiberMode)
        {
            if (fiber.FiberPrefab) fiber.FiberPrefab.SetActive(true);
            fiber.draw = true;
        }
        else {
            pushAway.SetActive(false);
        }
    }

    public void TogglePen(bool toggle)
    {
        continuous = !toggle;
        dragMode = false;
        if (fiberMode)
        {
            if (fiber.FiberPrefab) fiber.FiberPrefab.SetActive(true);
            fiber.draw = true;
        }
        else {
            pushAway.SetActive(false);
        }
    }

    public void ToggleErase(bool toggle)
    {
        //this mode toggle if we draw or drag
        eraseMode = toggle;
        eraser.ToggleMode(toggle);
        if (current_prefab)
            current_prefab.SetActive(!eraseMode);
        if (fiberMode)
        {
            fiber.FiberPrefab.SetActive(!eraseMode);
            fiber.draw = !eraseMode;
        }
        pushAway.SetActive(false);
    }

    public void TogglePin(bool toggle)
    {
        //this mode toggle if we draw or drag
        pinMode = toggle;
        dragger.pinMode = pinMode;
        fiber.draw = !toggle;
        if (current_prefab)
            current_prefab.SetActive(!pinMode);
        dragger.togglePinOutline(toggle);
        pushAway.SetActive(false);
    }

    public void TogglePickDrag(bool toggle) {
        //this mode toggle if we draw or drag
        dragMode = toggle;
        dragger.dragMode = dragMode;
        fiber.draw = !toggle;
        GetComponent<CircleCollider2D>().enabled = !toggle;
        if (current_prefab)
            current_prefab.SetActive(!toggle);
        dragger.togglePinOutline(toggle);
        pushAway.SetActive(false);
    }

    // Following method creates line runtime using Line Renderer component
    void OnTriggerStay2D(Collider2D other)
    {
        if (surfaceMode)
        {
            //if other attached to membrane objects
            if (other.gameObject.tag == "membrane")
            {
                otherSurf = other;
                //Debug.Log(other.name);
                //Debug.Log(myPrefab.name);
                //Debug.DrawLine(other.transform.position, other.transform.position + other.transform.up, Color.red);
                current_prefab.transform.position = other.transform.position;
                current_prefab.transform.rotation = Quaternion.FromToRotation(Vector3.up, other.transform.up);
                /*if (mouseDown)
                {
                    if (!myPrefab) return;
                    if (!other.transform) return;
                    GameObject newObject = Instantiate(myPrefab, other.transform.position, Quaternion.FromToRotation(Vector3.up, other.transform.up)) as GameObject;
                    newObject.transform.parent = root.transform;
                    newObject.hideFlags = HideFlags.HideInHierarchy;
                    mouseDown = false;
                }*/
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        otherSurf = null;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        //Debug.Log("pointer Enter");

    }

    public void OneInstance(Vector3 objectPos) {
        float Zangle = Random.value * Mathf.PI * Mathf.Rad2Deg;
        Quaternion quat = Quaternion.AngleAxis(Zangle, Vector3.forward);
        GameObject newObject = Instantiate(myPrefab, objectPos, quat) as GameObject;
        newObject.transform.parent = root.transform;
        newObject.hideFlags = HideFlags.HideInHierarchy;
        totalNprotein++;
        string name = currentLabel.text.Split(":".ToCharArray()[0])[0];
        if (proteins_count.ContainsKey(name))
        {
            proteins_count[name]++;
        }
        int percentFilledInt = addToArea(newObject);
        pb.Value = percentFilledInt;

        float perc = (((float)proteins_count[name] * getArea(newObject)) / (float) cam.GetComponent<buildBoundary>().boundryArea);
        currentLabel.text = name + ": " + perc.ToString("P") + " ( " + proteins_count[name].ToString() + " ) ";

       Transform t = newObject.transform.GetChild(0);
        t.gameObject.SetActive(false);
        if ((totalNprotein % layer_frequence) == 0)
            t.gameObject.SetActive(true);
        if (newObject.transform.childCount >1 ) {
            t = newObject.transform.GetChild(1);
            t.gameObject.SetActive(false);
            if ((totalNprotein % layer_frequence*2) == 0)
                t.gameObject.SetActive(true);
        }
        //some of top lay get drawn on top of dna.
        if ((totalNprotein % layer_frequence * 10) == 0)
            newObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
        /*foreach (Transform child in newObject.transform)
        {
            child.gameObject.SetActive(false);
            if ((totalNprotein % layer_frequence) == 0)
            {
                child.gameObject.SetActive(true);
            }
        }*/
        //Transform t = newObject.transform.GetChild(1);
        //if (t) t.gameObject.SetActive(false);
    }

    public void DestroyInstance(GameObject toDestroy) {
        string name = toDestroy.name.Split("(".ToCharArray())[0];//.Split("".ToCharArray()[0])[0];
        if (!proteins_count.ContainsKey(name))
        {
            Destroy(toDestroy);
            return;
        }
        totalNprotein--;
        proteins_count[name]--;
        float area = getArea(toDestroy);
        //Debug.Log(area.ToString());
        proteinArea -= area;
        float screenArea = cam.GetComponent<buildBoundary>().boundryArea;
        int percentFilledInt = (int)((proteinArea / screenArea) * 100);
        pb.Value = percentFilledInt;
        float perc = (((float)proteins_count[name] * area) / (float)cam.GetComponent<buildBoundary>().boundryArea);
        proteins_ui_labels[name].text = name + ": " + perc.ToString("P") + " ( " + proteins_count[name].ToString() + " ) ";
        Destroy(toDestroy);
    }
    
    // Update is called once per frame
    void Update () {
        string ename = null;// = EventSystem.current.currentSelectedGameObject.name;
        if (EventSystem.current.currentSelectedGameObject)
        {
            ename = EventSystem.current.currentSelectedGameObject.name;
        }
        //Debug.Log(EventSystem.current.IsPointerOverGameObject(-1));
        if (!myPrefab) return;
        mouseDown = false;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mousePos.z + 30.0f;       // we want 2m away from the camera position
                                  //Debug.Log(mousePos);
        Vector3 objectPos = cam.ScreenToWorldPoint(mousePos);
        transform.position = objectPos;
        //Debug.Log(mousePos.ToString());
        //Debug.Log(Screen.width.ToString() + " " + Screen.height.ToString());
        if (((ename != "Canvas") && (ename != null))) return;
        if (fiberMode) return;
        if (dragMode) return;
        if (pinMode) return;
        if (eraseMode) return;
        /*if (ui.isActiveAndEnabled)
        {
            if (mousePos.x < Screen.width * 0.15f) return;
            if (mousePos.y > Screen.height - Screen.height * 0.05f) return;
        }*/
        bool input_event = false;
        if (continuous)
            input_event = Input.GetMouseButton(0);
        else
            input_event = Input.GetMouseButtonDown(0);

        if (input_event)
        //if (Input.GetButtonDown("Fire1"))
        {
            
            //Debug.Log(mousePos);
            //Debug.Log(objectPos);
            if (!surfaceMode)
            {
                for (int i = 0; i < nbInstancePerClick; i++)
                {
                    Vector3 offset = UnityEngine.Random.insideUnitCircle*(float)i;
                    OneInstance(objectPos+offset);
                }
            }
        }
        //Debug.Log(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;
            if (surfaceMode) {
                if (otherSurf) {
                    //GameObject newObject = Instantiate(myPrefab, otherSurf.transform.position, Quaternion.FromToRotation(Vector3.up, otherSurf.transform.up)) as GameObject;
                    //Debug.Log("Within surface mode loop 2.");
                    GameObject newObject = Instantiate(myPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    totalNprotein++;
                    /*
                    //Get the area of the box and or circle colliders of the top layer of the prefab.
                    Component[] circleColliders;
                    Component[] boxColliders;

                    circleColliders = newObject.GetComponentsInParent(typeof(CircleCollider2D));
                    Debug.Log(circleColliders);
                    boxColliders = newObject.GetComponentsInParent(typeof(BoxCollider2D));
                    Debug.Log(boxColliders);

                    if (circleColliders != null){
                        foreach (CircleCollider2D collider2D in circleColliders)
                            circleRadius = collider2D.radius;
                            circleArea = Mathf.PI * (circleRadius * circleRadius);
                            protienArea = circleArea + protienArea;
                    }
                    if (boxColliders != null)
                    {
                        foreach (BoxCollider2D collider2D in boxColliders)
                            boxArea = (collider2D.size.x) * (collider2D.size.y);
                            protienArea = boxArea + protienArea;
                    }

                    screenArea = cam.GetComponent<buildBoundary>().boundryArea;
                    percentFilled = (protienArea / screenArea) * 100;
                    int percentFilledInt = (int) percentFilled;
                    */
                    int percentFilledInt = addToArea(newObject);
                    pb.Value = percentFilledInt;

                    newObject.transform.position = current_prefab.transform.position;
                    newObject.transform.rotation = current_prefab.transform.rotation;
                    newObject.transform.parent = root.transform;
                    newObject.hideFlags = HideFlags.HideInHierarchy;
                }
            }
        }
    }

    public float getArea(GameObject newObject)
    {
        return newObject.GetComponent<PrefabProperties>().area;
    }

    public int addToArea(GameObject newObject) {
        proteinArea += getArea(newObject);
        screenArea = cam.GetComponent<buildBoundary>().boundryArea;
        percentFilled = (proteinArea / screenArea) * 100;
        int percentFilledInt = (int)percentFilled;
        return percentFilledInt;
    }

    public void Clear() {
        //go through all children of root
        foreach (Transform child in root.transform) {
            GameObject.Destroy(child.gameObject);
            int percentFilledInt = 0;
            protienArea = 0;
            totalNprotein = 0;
            GameObject.Find("Progressbar").GetComponent<Progressbar>().Value = percentFilledInt;
        }
        //clear the all the count
        foreach (var keyvalue in proteins_count) {
            proteins_count[keyvalue.Key] = 0;
            proteins_ui_labels[keyvalue.Key].text = keyvalue.Key;
        }
    }
}
