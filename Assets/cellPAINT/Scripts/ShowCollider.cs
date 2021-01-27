using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShowCollider : MonoBehaviour
{
    public bool initialized = false;
    public int circle_segment = 20;
    public float line_width = 0.20f;
    CircleCollider2D[] all_circles;
    BoxCollider2D[] all_boxes;
    List<GameObject> renderers;
    // Start is called before the first frame update
    void Start()
    {
        renderers = new List<GameObject>();
    }

    public void GetColliders(){
        all_circles = GetComponents<CircleCollider2D>();
        all_boxes = GetComponents<BoxCollider2D>();        
    }
    void SetCirclePoints (int segments, float radius, Vector2 off, LineRenderer line)
    {
        float x;
        float y;
        float z = 0f;
        float angle = 20f;
        for (int i = 0; i < segments ; i++)
        {
            x = Mathf.Sin (Mathf.Deg2Rad * angle) * radius;
            y = Mathf.Cos (Mathf.Deg2Rad * angle) * radius;
            line.SetPosition (i,gameObject.transform.TransformPoint(new Vector3(off.x+x,off.y+y,z)) );
            angle += (360f / segments);
        }
    }

    public void Setup(){
        var layer = -1;
        var sr = transform.GetComponent<SpriteRenderer>();
        var mat = Manager.Instance.manager_prefab_material;
        if (sr) {
            layer = sr.sortingOrder;
            mat = sr.sharedMaterial;
        }
        foreach(BoxCollider2D box in all_boxes) {
            //create and add a line renderer?
            var l = new GameObject();
            l.transform.parent = transform;
            var line = l.AddComponent<LineRenderer>();
            line.positionCount = 4;
            //from local space to world space.
            Vector2 off = box.offset;
            Vector2 size = box.size;
            line.loop = true;
            line.sharedMaterial = mat;
            line.sortingOrder = layer;
            line.widthMultiplier = line_width;
            //line.startColor = Color.yellow;
            //line.endColor = Color.yellow;     
            line.useWorldSpace= false;       
            line.SetPosition(0, gameObject.transform.TransformPoint(new Vector3(off.x-size.x/2.0f,off.y-size.y/2.0f,0.0f)));
            line.SetPosition(1, gameObject.transform.TransformPoint(new Vector3(off.x-size.x/2.0f,off.y+size.y/2.0f,0.0f)));
            line.SetPosition(2, gameObject.transform.TransformPoint(new Vector3(off.x+size.x/2.0f,off.y+size.y/2.0f,0.0f)));
            line.SetPosition(3, gameObject.transform.TransformPoint(new Vector3(off.x+size.x/2.0f,off.y-size.y/2.0f,0.0f)));
            //line.SetPosition(0, gameObject.transform.TransformPoint(new Vector3(off.x-size.x/2.0f,off.y-size.y/2.0f,0.0f)));
            renderers.Add(l);
        }
        foreach(CircleCollider2D circle in all_circles) {
            //create and add a line renderer?
            var l = new GameObject();
            l.transform.parent = transform;
            var line = l.AddComponent<LineRenderer>();
            line.sharedMaterial = mat;
            line.sortingOrder = layer;
            line.widthMultiplier = line_width;            
            line.loop = true;
            line.positionCount = circle_segment;
            //line.startColor = Color.yellow;
            //line.endColor = Color.yellow;  
            line.useWorldSpace= false;          
            SetCirclePoints(circle_segment,circle.radius, circle.offset, line);
            renderers.Add(l);
        }
    }

    public void Toggle(bool value){
        if (renderers == null || renderers.Count == 0) return;
        foreach(GameObject o in renderers) {
            o.SetActive(value);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!initialized) {
             GetColliders();
             Setup();
             initialized = true;
        }
    }
}
