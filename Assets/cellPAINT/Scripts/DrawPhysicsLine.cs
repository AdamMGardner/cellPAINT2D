using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UIWidgetsSamples;

public class DrawPhysicsLine : MonoBehaviour
{
    public Camera cam;
    public GameObject root;
    public GameObject FiberPrefab;
    public float lengthPrefab;
    public bool quad = true;
    public int objectCounter;
    public int persistence = 1;
    public bool distanceJoint = false;
    public bool hingeJoint = false;
    public float distance = 0.05f;
    public float closing_distance = 2.0f;
    public bool membraneMode = false;
    public bool draw = false;
    public float layer_change_distance = 20.0f;
    public float total_length = 20.0f;
    //public TreeViewSample ui;

    private LineRenderer line; 
    private GameObject quadOb;
    private float nextNameNumber = 0;
    private int randomText = 0;
    private Vector3 mousePos;
    private Vector3 startPos;
    private Vector3 endPos;
    private bool init = false;
    private GameObject parent;
    private moveClick manager;

    void Start() {
        objectCounter = 0;
        if (FiberPrefab != null)
        {
            //a fiber should have at lease two circle collider at each extermity
            CircleCollider2D[] allc = FiberPrefab.GetComponents<CircleCollider2D>();
            Vector2 pos1 = new Vector2(FiberPrefab.transform.position.x, FiberPrefab.transform.position.y) + allc[0].offset;
            Vector2 pos2 = new Vector2(FiberPrefab.transform.position.x, FiberPrefab.transform.position.y) + allc[allc.Length - 1].offset;
            lengthPrefab = Vector2.Distance(pos1, pos2);
            closing_distance = lengthPrefab * 3.0f;
        }
        init = false;
        manager = GetComponent<moveClick>();
    }

    public void updatePrefab(GameObject prefab) {
        //if (FiberPrefab) GameObject.Destroy(FiberPrefab);
        FiberPrefab = prefab;

        if (!FiberPrefab)
        {
            GameObject.Destroy(FiberPrefab);
            return;
        }
        CircleCollider2D[] allc = FiberPrefab.GetComponents<CircleCollider2D>();
        Vector2 pos1 = new Vector2(FiberPrefab.transform.position.x, FiberPrefab.transform.position.y) + allc[0].offset;
        Vector2 pos2 = new Vector2(FiberPrefab.transform.position.x, FiberPrefab.transform.position.y) + allc[allc.Length - 1].offset;
        lengthPrefab = Vector2.Distance(pos1, pos2);
        closing_distance = lengthPrefab * 3.0f;
        FiberPrefab.SetActive(true);
    }

    public void checkCloseChains(Vector3 mousePos) {

    }

    void Update()
    {
        string ename = null;// = EventSystem.current.currentSelectedGameObject.name;
        if (EventSystem.current.currentSelectedGameObject)
        {
            ename = EventSystem.current.currentSelectedGameObject.name;
        }
        //Debug.Log(((ename != "Canvas") && (ename != null))); Debug.Log(ename);
        if (!FiberPrefab) return;
        if (!draw) return;
        if (((ename != "Canvas") && (ename != null))) return;
        if (((ename == "Button"))) return;
        Vector3 mousePosx = Input.mousePosition;
        mousePosx.z = 15.0f;
        /*if (ui.isActiveAndEnabled)
        {
            if (mousePosx.x < Screen.width * 0.15f) return;
            if (mousePosx.y > Screen.height - Screen.height * 0.05f) return;
        }*/
        if (Input.GetMouseButtonDown(0))//on click
        {
            if (init == false)
            {
                //check if close to another chain start/end 
                init = true;
                mousePos = cam.ScreenToWorldPoint(mousePosx);
                endPos = startPos = mousePos;
                parent = new GameObject();
                parent.name = "chain_" + objectCounter.ToString();
                parent.transform.parent = root.transform;
                objectCounter++;
                if ((startPos == null)||(startPos.magnitude==0.0f))
                {
                    endPos = startPos = mousePos;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Transform first = parent.transform.GetChild(0);
            Transform last = parent.transform.GetChild(parent.transform.childCount-1);

            nextNameNumber = 0; //Reset the chain number after Button up.

            float D = Vector3.Distance(first.position, last.position);

            if (D < closing_distance)
            {
                if ((distanceJoint) && (parent.transform.childCount > 1))
                {
                    DistanceJoint2D joint = last.gameObject.AddComponent<DistanceJoint2D>();
                    joint.enabled = true;
                    joint.connectedBody = first.gameObject.GetComponent<Rigidbody2D>();
                    joint.autoConfigureConnectedAnchor = false;
                    joint.autoConfigureDistance = false;
                    joint.distance = distance;
                    joint.enableCollision = false;
                    CircleCollider2D[] allc = FiberPrefab.GetComponents<CircleCollider2D>();
                    joint.anchor = allc[1].offset;
                    joint.connectedAnchor = allc[0].offset;
                }
                /*else
                {
                    DistanceJoint2D joint = last.gameObject.AddComponent<DistanceJoint2D>();
                    var RigidbodyAnchor = last.gameObject.GetComponent<Rigidbody2D>();
                    RigidbodyAnchor.isKinematic = true;
                    joint.enabled = true;
                    joint.connectedBody = first.gameObject.GetComponent<Rigidbody2D>();
                    joint.autoConfigureConnectedAnchor = false;
                    joint.autoConfigureDistance = false;
                    joint.distance = distance;
                    joint.enableCollision = false;
                    Debug.Log("You are in the non-closing loop");
                    nextNameNumber = 0;
                }*/

                if ((hingeJoint) && (parent.transform.childCount > 1))
                {
                    HingeJoint2D hinge = last.gameObject.AddComponent<HingeJoint2D>();
                    hinge.enabled = true;
                    hinge.connectedBody = first.gameObject.GetComponent<Rigidbody2D>();
                    hinge.autoConfigureConnectedAnchor = false;
                    //hinge.autoConfigureDistance = false;
                    //hinge.distance = distance;
                    hinge.enableCollision = false;
                    CircleCollider2D[] allc = FiberPrefab.GetComponents<CircleCollider2D>();
                    hinge.anchor = allc[1].offset;
                    hinge.connectedAnchor = allc[0].offset;
                    hinge.useLimits = false;
                    JointAngleLimits2D limits = hinge.limits;
                    limits.min = -30.0f;
                    limits.max = 30.0f;
                    hinge.limits = limits;
                }
                //Debug.Log(first.name);
                //Debug.Log(last.name);
                closePersistence();
                first.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            }
            else {
                var RigidbodyAnchor = last.gameObject.GetComponent<Rigidbody2D>();
                RigidbodyAnchor.isKinematic = true;
            }
            init = false;
            startPos = Vector3.zero;
            endPos = Vector3.zero;
            return;
        }
        else if (Input.GetMouseButton(0))
        {
            mousePos = cam.ScreenToWorldPoint(mousePosx);
            endPos = mousePos;
            
                float lineLength = Vector3.Distance(startPos, endPos);
                if (lineLength >= lengthPrefab)
                {
                    Vector3 cStart = startPos;
                    Vector3 cEnd = cStart + (endPos - startPos).normalized * lengthPrefab;
                    int n = Mathf.RoundToInt(lineLength / lengthPrefab);
                    for (int i = 0; i < n; i++)
                    {
                        createQuadPR(cStart, cEnd);
                        cStart = cEnd;
                        cEnd = cStart + (endPos - startPos).normalized * lengthPrefab;
                    }
                    startPos = endPos;
            }
               
        }

    }
  
    private void createLine()
    {
        GameObject ob = Instantiate(FiberPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        ob.name = "ob" + nextNameNumber;
        nextNameNumber++;
        line = ob.GetComponent<LineRenderer>();
        line.SetVertexCount(2);
        line.SetWidth(0.12f, 0.12f);
        line.SetColors(Color.green, Color.green);
        line.useWorldSpace = true;
    }
    
    private void closePersistence() {
        int start = 0;
        int nchild = parent.transform.childCount;
        if ((distanceJoint) && (parent.transform.childCount > 1))
        {
            start = 1;
        }
        if ((hingeJoint) && (parent.transform.childCount > 1))
        {
            start = 1;
        }
        int st = 0;
        int end = nchild - 1;
        for (int l = start; l < persistence; l++)
        {
            int i = 0;
            for (int k = l; k >= 0; k--) {
                if (i < i + 1) continue;
                var ch1= parent.transform.GetChild(st + i);
                var ch2 = parent.transform.GetChild(end - i);
                SpringJoint2D spring = ch2.gameObject.AddComponent<SpringJoint2D>();
                spring.connectedBody = ch1.gameObject.GetComponent<Rigidbody2D>();
                ch1.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                ch2.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                spring.enableCollision = true;
                spring.autoConfigureDistance = false;
                spring.distance = 0.01f;
                spring.anchor = new Vector2(-0.12f, 0.0f);
                spring.connectedAnchor = new Vector2(0.12f, 0.0f);
                i++;
            }
        }
    }

    private void createQuad()
    {
        init = true;
        Vector3 midPoint = (startPos + endPos) / 2;
        quadOb = Instantiate(FiberPrefab, midPoint, Quaternion.identity) as GameObject;
        quadOb.name = "ob" + nextNameNumber;
        int count = 0;
        nextNameNumber++;
        int nchild = parent.transform.childCount;
        quadOb.transform.parent = parent.transform;
        quadOb.GetComponent<DistanceJoint2D>().enabled = distanceJoint;
        quadOb.GetComponent<HingeJoint2D>().enabled = hingeJoint;

        int start = 0;
        if ((distanceJoint)&&(parent.transform.childCount > 1))
        { 
            DistanceJoint2D joint = parent.transform.GetChild(nchild - 1).gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = quadOb.GetComponent<Rigidbody2D>();
            start = 1;
        }
        if ((hingeJoint)&&(parent.transform.childCount > 1))
        {
           HingeJoint2D hinge = parent.transform.GetChild(nchild - 1).gameObject.AddComponent<HingeJoint2D>();
           hinge.connectedBody = quadOb.GetComponent<Rigidbody2D>();
            start = 1;
        }

        for (int i = start; i < persistence;i++) {
            if (parent.transform.childCount < i) continue;
            var ch = parent.transform.GetChild(nchild-(i+1));
            SpringJoint2D spring = ch.gameObject.AddComponent<SpringJoint2D>();
            spring.connectedBody = quadOb.GetComponent<Rigidbody2D>();
            spring.enableCollision = false;
            spring.autoConfigureDistance = true;
            spring.distance = 0.01f;
            spring.anchor = new Vector2(0.12f,0.0f);
            spring.connectedAnchor = new Vector2(-0.12f, 0.0f);
        } 
    }

    private void createQuadPR(Vector3 start, Vector3 end)
    {

        Debug.Log(FiberPrefab.GetComponent<PrefabProperties>().sprite_ordered_switch);
        if (FiberPrefab.GetComponent<PrefabProperties>().sprite_random_switch)
        {
            FiberPrefab.GetComponent<PrefabProperties>().switchSpriteRandomly();
        }

        //This if loop should change the prefab based on an array and switch in PrefabProperties.cs
        if (FiberPrefab.GetComponent<PrefabProperties>().prefab_random_switch)
        {
            int prefab_id = Random.Range(0, FiberPrefab.GetComponent<PrefabProperties>().prefab_asset.Count);
            FiberPrefab = FiberPrefab.GetComponent<PrefabProperties>().prefab_asset[prefab_id-1];
            Debug.Log("Prefab Randomize is True " + "The prefab_id is " + prefab_id);
        }

        if (FiberPrefab.GetComponent<PrefabProperties>().sprite_ordered_switch)
        {
            FiberPrefab.GetComponent<PrefabProperties>().switchSpriteInOrder();
        }

        init = true;
        Vector3 midPoint = (start + end) / 2;
        Vector3 v2 = (end - start).normalized;
        float a = Vector3.Angle(Vector3.right, v2);
        float sign = (v2.y < Vector3.right.y) ? -1.0f : 1.0f;

        Quaternion rotation = Quaternion.AngleAxis(a*sign,Vector3.forward);

        quadOb = Instantiate(FiberPrefab, midPoint, rotation) as GameObject;
        quadOb.name = "ob" + nextNameNumber;
        
        //check the overall length toward 


        //anchors the first instance in the chain.
        if (quadOb.name == "ob0")
        {
            var RigidbodyAnchor = quadOb.gameObject.GetComponent<Rigidbody2D>();
            RigidbodyAnchor.isKinematic = true;
        }

        int count = 0;
        nextNameNumber++;
        int nchild = parent.transform.childCount;
        quadOb.transform.parent = parent.transform;
        //quadOb.GetComponent<DistanceJoint2D>().enabled = false;// distanceJoint;
        //quadOb.GetComponent<HingeJoint2D>().enabled = false;// hingeJoint;

        //Changes the texture of the membrane sprite instance randomly.
        //membraneMode = FiberPrefab.GetComponent<PrefabProperties>().sprite_random_switch;
        //Debug.Log("membraneMode is " + membraneMode);

        int st = 0;
        if ((distanceJoint) && (parent.transform.childCount > 1))
        {
            DistanceJoint2D joint = parent.transform.GetChild(nchild - 1).gameObject.AddComponent<DistanceJoint2D>();
            joint.enabled = true;
            joint.connectedBody = quadOb.GetComponent<Rigidbody2D>();
            joint.autoConfigureConnectedAnchor = false;
            joint.autoConfigureDistance = false;
            joint.distance = distance;
            joint.enableCollision = false;
            CircleCollider2D[] allc = FiberPrefab.GetComponents<CircleCollider2D>();
            joint.anchor = allc[1].offset;
            joint.connectedAnchor = allc[0].offset;
            st = 1;
        }
        else {
            //quadOb.GetComponent<DistanceJoint2D>().enabled = false;
        }
        if ((hingeJoint) && (parent.transform.childCount > 1))
        {
            HingeJoint2D hinge = parent.transform.GetChild(nchild - 1).gameObject.AddComponent<HingeJoint2D>();
            hinge.enabled = true;
            hinge.connectedBody = quadOb.GetComponent<Rigidbody2D>();
            hinge.autoConfigureConnectedAnchor = false;
            //hinge.autoConfigureDistance = false;
            //hinge.distance = distance;
            hinge.enableCollision = false;
            CircleCollider2D[] allc = FiberPrefab.GetComponents<CircleCollider2D>();
            hinge.anchor = allc[1].offset;
            hinge.connectedAnchor = allc[0].offset;
            hinge.useLimits = true;
            JointAngleLimits2D limits = hinge.limits;
            limits.min = -10.0f;
            limits.max = 10.0f;
            hinge.limits = limits;
            st = 1;
        }
        else {
           // quadOb.GetComponent<HingeJoint2D>().enabled = false;
        }

        for (int i = 0; i < persistence; i++)
        {
            if (nchild < i) continue;
            if (nchild - (i + 1) < 0) continue;
            var ch = parent.transform.GetChild(nchild - (i + 1));
            SpringJoint2D spring = ch.gameObject.AddComponent<SpringJoint2D>();
            spring.connectedBody = quadOb.GetComponent<Rigidbody2D>();
            spring.enableCollision = false;
            spring.autoConfigureDistance = false;
            CircleCollider2D[] allc = FiberPrefab.GetComponents<CircleCollider2D>();
            spring.distance = lengthPrefab * (i+1);
            spring.anchor = allc[1].offset;
            spring.connectedAnchor = allc[1].offset;
        }
    }

    private void moveQuad()
    {
        float lineLength = Vector3.Distance(startPos, endPos);
        quadOb.transform.localScale = new Vector3(lengthPrefab, quadOb.transform.localScale.y, quadOb.transform.localScale.z);
        Vector3 midPoint = (startPos + endPos) / 2;
        quadOb.transform.position = midPoint; 
        float angle = (Mathf.Abs(startPos.y - endPos.y) / Mathf.Abs(startPos.x - endPos.x));
  

        if ((startPos.y < endPos.y && startPos.x > endPos.x) || (endPos.y < startPos.y && endPos.x > startPos.x))
        {
            angle *= -1;
        }
        
        angle = Mathf.Rad2Deg * Mathf.Atan(angle);

        quadOb.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

    }

    private void addColliderToLine()
    {
        BoxCollider2D col = line.gameObject.transform.GetChild(0).GetComponent<BoxCollider2D>();
        float lineLength = Vector3.Distance(startPos, endPos); 
        col.size = new Vector3(lineLength, 0.1f, 1f); 
        Vector3 midPoint = (startPos + endPos) / 2;
        col.transform.position = midPoint; 
        float angle = (Mathf.Abs(startPos.y - endPos.y) / Mathf.Abs(startPos.x - endPos.x));

        if ((startPos.y < endPos.y && startPos.x > endPos.x) || (endPos.y < startPos.y && endPos.x > startPos.x))
        {
            angle *= -1;
        }

        angle = Mathf.Rad2Deg * Mathf.Atan(angle);
        col.transform.Rotate(0, 0, angle);

    }
}