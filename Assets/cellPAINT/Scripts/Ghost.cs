using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Accord;

public class Ghost : MonoBehaviour
{
    public int gid;
    public List<GameObject> locked_item = new List<GameObject>();
    public bool highlight = false;
    public bool highlight_setuped = false;
    public bool highligh_path = false;
    public float cluster_radius = 8.0f;
    private Rigidbody2D rb;
    private PolygonCollider2D polygon;
    private Bounds bounds;
    private GameObject lines_holder;
    private List<LineRenderer> lineRenderers =  new List<LineRenderer>();

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        polygon = GetComponent<PolygonCollider2D>();
        if (polygon == null) polygon = gameObject.AddComponent<PolygonCollider2D>();
    }

    void Setup(){
        if (rb == null)rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        if (polygon == null)polygon = GetComponent<PolygonCollider2D>();
        if (polygon == null) polygon = gameObject.AddComponent<PolygonCollider2D>();        
    }

    void ghostHierarchy(Transform parent,bool append = true){
        foreach (Transform ch in parent.transform)
        {
            if (Manager.Instance.fiber_parents.Contains(ch.gameObject))
            {
                ghostHierarchy(ch,false);
                if (append) locked_item.Add(ch.gameObject);
            }
            PrefabProperties p = ch.GetComponent<PrefabProperties>();
            if (p)
            {
                if (p.RB.simulated)
                {
                    p.ghost_id = gid;
                    p.RB.simulated = false;
                    if (append) locked_item.Add(ch.gameObject);
                }
            }
        }
    }

    void unghostHierarchy(Transform parent){
        foreach (Transform ch in parent.transform)
        {
            if (Manager.Instance.fiber_parents.Contains(ch.gameObject))
            {
                unghostHierarchy(ch);
            }
            PrefabProperties p = ch.GetComponent<PrefabProperties>();
            if (p)
            {
                p.ghost_id = -1;
                p.RB.simulated = true;
            }
        }
    }

    public void unGhost(){
        foreach (var o in locked_item){
            //test for attachements
            if (Manager.Instance.fiber_parents.Contains(o))
            {
                unghostHierarchy(o.transform);
            }
            else
            {
                var p = o.GetComponent<PrefabProperties>();
                var pg = o.GetComponentInParent<PrefabGroup>();
                if (p == null && pg == null)
                {
                    p = o.transform.parent.GetComponent<PrefabProperties>();
                    //o = o.transform.parent.gameObject;
                    pg = o.transform.parent.gameObject.GetComponentInParent<PrefabGroup>();
                }
                if (pg != null) {
                    p = pg.gameObject.GetComponent<PrefabProperties>();
                }
                if (p) {
                    if (pg || p.is_Group) {
                        //do allthe group selection
                        unghostHierarchy(o.transform);
                    }
                    else {
                        p.ghost_id = -1;
                        p.RB.simulated = true;
                    }
                }
            }            
        }
    }

    public void changeParent(Transform new_parent){
        foreach (var o in locked_item){
            //test for attachements
            if (Manager.Instance.fiber_parents.Contains(o))
            {
                o.transform.parent = new_parent;
            }
            else
            {
                var p = o.GetComponent<PrefabProperties>();
                var pg = o.GetComponentInParent<PrefabGroup>();
                if (p == null && pg == null)
                {
                    p = o.transform.parent.GetComponent<PrefabProperties>();
                    //o = o.transform.parent.gameObject;
                    pg = o.transform.parent.gameObject.GetComponentInParent<PrefabGroup>();
                }
                if (pg != null) {
                    p = pg.gameObject.GetComponent<PrefabProperties>();
                }
                if (p) {
                    if (pg || p.is_Group) {
                        //do allthe group selection
                        pg.transform.parent = new_parent;
                    }
                    else {
                        p.transform.parent = new_parent;
                    }
                }
            }            
        }        
    }

    public void SetupFromSelection(List<GameObject> selection){
        Setup();
        locked_item = new List<GameObject>();//selection);
        //filter out already locked object
        foreach (var o in selection)
        {
            //test for attachements
            if (Manager.Instance.fiber_parents.Contains(o))
            {
                ghostHierarchy(o.transform,false);
                locked_item.Add(o);
            }
            else
            {
                var p = o.GetComponent<PrefabProperties>();
                var pg = o.GetComponentInParent<PrefabGroup>();
                if (p == null && pg == null)
                {
                    p = o.transform.parent.GetComponent<PrefabProperties>();
                    //o = o.transform.parent.gameObject;
                    pg = o.transform.parent.gameObject.GetComponentInParent<PrefabGroup>();
                }
                if (pg != null) {
                    p = pg.gameObject.GetComponent<PrefabProperties>();
                }
                if (p) {
                    if (pg || p.is_Group) {
                        //do allthe group selection
                        ghostHierarchy(o.transform,false);
                        locked_item.Add(o);
                    }
                    else {
                        if (p.RB.simulated)
                        {
                            p.ghost_id = gid;
                            p.RB.simulated = false;
                            locked_item.Add(o);
                        }
                    }
                }
            }
        }        
    }

    List<Vector2> GatherVector2FromHierarchy(GameObject obj){
        List<Vector2> pts = new List<Vector2>();
        foreach (Transform ch in obj.transform){
            if (Manager.Instance.fiber_parents.Contains(ch.gameObject)){
                pts.AddRange(GatherVector2FromHierarchy(ch.gameObject));
            }
            else {
                var p = ch.GetComponent<PrefabProperties>();
                if (p&&!p.RB.simulated)
                {
                    pts.Add(new Vector2(Mathf.CeilToInt(p.RB.position.x),Mathf.CeilToInt(p.RB.position.y)));
                    bounds.Encapsulate(p.RB.position);
                }                
            }
        }
        return pts;
    }

    public List<Vector2> GatherVector2FromSelection(){
        List<Vector2> pts = new List<Vector2>();
        bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        foreach (var o in locked_item)
        {
            //test for attachements
            if (Manager.Instance.fiber_parents.Contains(o))
            {
                pts.AddRange(GatherVector2FromHierarchy(o));
            }
            else
            {
                var p = o.GetComponent<PrefabProperties>();
                var pg = o.GetComponentInParent<PrefabGroup>();
                if (p == null && pg == null)
                {
                    p = o.transform.parent.GetComponent<PrefabProperties>();
                    //o = o.transform.parent.gameObject;
                    pg = o.transform.parent.gameObject.GetComponentInParent<PrefabGroup>();
                }
                if (pg != null) {
                    p = pg.gameObject.GetComponent<PrefabProperties>();
                }                
                if (p)
                {
                    if (p.is_Group) {
                        //do allthe group selection
                        pts.AddRange(GatherVector2FromHierarchy(o));
                    }
                    else {
                        pts.Add(new Vector2(Mathf.CeilToInt(p.RB.position.x),Mathf.CeilToInt(p.RB.position.y)));
                        bounds.Encapsulate(p.RB.position);
                    }
                }
            }
        }
        return pts;
    }

    public void SetupGhostArea(){
        //gather list of points
        List<Vector2> pts = GatherVector2FromSelection();        
        transform.position = bounds.center;
        if (pts.Count == 0) {
            Debug.Log("didnt found any point");
            polygon.pathCount = 0;
        }
        else {
            //cluster
            List<int> labels = new List<int>();
            int cluster_count = Helper.ComputeCluster(pts,cluster_radius, ref labels);
            Debug.Log(labels);
            Debug.Log(cluster_count);
            List<List<IntPoint>> cl_points = new List<List<IntPoint>>();
            for (var i=0;i < cluster_count;i++){
                cl_points.Add(new List<IntPoint>());
            }
            for (var i=0;i < labels.Count;i++){
                var l = labels[i];
                cl_points[l].Add(new IntPoint(Mathf.CeilToInt(pts[i].x),Mathf.CeilToInt((pts[i].y))));
            }
            polygon.pathCount = cluster_count;
            int clid=0;
            for (var i=0;i < cluster_count;i++){
                if (cl_points[i].Count == 0){
                    continue;
                }
                List<Vector2> hull = Helper.ComputeConvexHull(cl_points[i],bounds.center,false);
                polygon.SetPath(clid,hull);
                clid++;
            }
            polygon.pathCount = clid;
            //box.offset = new Vector2(b.center.x,b.center.y);
            /*
            if we want a mesh
            var mesh = box.CreateMesh(false,false);
            mf.sharedMesh = mesh;
            */
        }
        //ghostArea.transform.localScale = new Vector3(1.1f,1.1f,1.1f);
    }

    public void ToggleHighlight(bool toggle){
        if (!highlight_setuped) SetupHighlight();
        lines_holder.SetActive(toggle);
    }

    public void SetupHighlight(){
         if (lines_holder == null){
             lines_holder = new GameObject(name+"_lines_holder");
             lines_holder.transform.parent = transform;
        }
        LineRenderer line;
        int nPath = polygon.pathCount;
        for (var i =0;i < nPath;i++){
            var path = polygon.GetPath(i);
            var g = new GameObject("lines_"+i.ToString());
            g.transform.parent = lines_holder.transform;
            line = g.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = path.Length;
            lineRenderers.Add(line);
            line.sharedMaterial = Manager.Instance.lineMat;
            //line.sortingOrder = jt.gameObject.GetComponent<SpriteRenderer>().sortingOrder+1;
            line.widthMultiplier = 0.3f;
            line.numCapVertices = 5;
            line.startColor = Color.yellow;
            line.endColor = Color.yellow;
            line.loop = true;
            for ( var j = 0; j<path.Length;j++ ){
                line.SetPosition(j,new Vector3(path[j].x+bounds.center.x,path[j].y+bounds.center.y,0.0f));
            }
        }
        highlight_setuped = true;
        lines_holder.SetActive(false);
    }
}