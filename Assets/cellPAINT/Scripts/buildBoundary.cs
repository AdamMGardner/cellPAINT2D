using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buildBoundary : MonoBehaviour
{
    public bool use_quad;
    public GameObject quadPrefab;
    private Camera cam;
    private GameObject boundary;
    public BoxCollider2D top;
    public BoxCollider2D bottom;
    public BoxCollider2D right;
    public BoxCollider2D left;

    public Vector3 topc;
    public Vector3 bottomc;
    public Vector3 rightc;
    public Vector3 leftc;

    private float last_width;
    private float last_height;
    private float colliderWidthScreen;

    public float colliderWidth;
    public int maxWidthCanvas = 2000;
    public int maxHeightCanvas = 2000;
    public float boundryArea;
    public float offset_left=0.0f;//346
    private float last_size;
    private float last_size_max=0;
    private float prev_W=0.0f;
    private float prev_H=0.0f;
    private CameraMove cm;
    // Use this for initialization
    void Start()
    {
      
        cam = GetComponent<Camera>();
        cm=cam.GetComponent<CameraMove>();
        cam.orthographicSize = cm.cameraZoomMax - 100;
        //use user preference ?
        last_width = Mathf.Max(maxWidthCanvas,Screen.width);// should it be a  defined size 
        last_height = Mathf.Max(maxHeightCanvas,Screen.height);// should it be a  defined size 

        //build the box collider surrounding the view port
        boundary = new GameObject("boundary");
        boundary.transform.position = new Vector2(0, 0);
        if (use_quad) {

        }
        else
        {
            top = boundary.AddComponent<BoxCollider2D>();
            bottom = boundary.AddComponent<BoxCollider2D>();
            right = boundary.AddComponent<BoxCollider2D>();
            left = boundary.AddComponent<BoxCollider2D>();
        }
        last_size_max = last_size = cm.cameraZoomMax; //orthographicSize;

        changeBoundary();
        cam.orthographicSize = cm.cameraCurrentZoom;
    }

    void CreateABound(Vector3 size, Vector3 pos){
        var bo = GameObject.Instantiate(quadPrefab);
        bo.transform.parent = boundary.transform;
        bo.transform.position = pos;
        bo.transform.localScale = size;
        bo.AddComponent<BoxCollider2D>();        
    }

    void changeBoundary()
    {
        Vector2 xa = cam.WorldToScreenPoint(new Vector2((-colliderWidth / 2), 0));
        Vector2 xb = cam.WorldToScreenPoint(new Vector2((colliderWidth / 2), 0));
        colliderWidthScreen = Vector2.Distance(xa, xb);

        topc = cam.ScreenToWorldPoint(new Vector2((last_width / 2.0f), last_height + (colliderWidthScreen / 2.0f)));
        bottomc = cam.ScreenToWorldPoint(new Vector2(last_width / 2.0f, (-colliderWidthScreen / 2.0f)));
        rightc = cam.ScreenToWorldPoint(new Vector2(last_width + (colliderWidthScreen / 2.0f), last_height / 2.0f));
        leftc = cam.ScreenToWorldPoint(new Vector2((-colliderWidthScreen / 2.0f)+offset_left, last_height  / 2.0f));

        float W = Vector2.Distance(rightc, leftc) + colliderWidth;
        float H = Vector2.Distance(topc, bottomc) + colliderWidth;

        boundryArea = (W - colliderWidth*2) * (H - colliderWidth*2);
        if (use_quad) {
            CreateABound(new Vector3(W, colliderWidth,0), new Vector3(topc.x, topc.y,1));
            CreateABound(new Vector3(W, colliderWidth,0), new Vector3(bottomc.x, bottomc.y,1));
            CreateABound(new Vector3(colliderWidth, H,0), new Vector3(rightc.x, rightc.y,1));
            CreateABound(new Vector3(colliderWidth, H,0), new Vector3(leftc.x, leftc.y,1));

        }
        else {
            top.offset = topc;
            top.size = new Vector2(W, colliderWidth);

            bottom.offset = bottomc;
            bottom.size = new Vector2(W, colliderWidth);

            right.offset = rightc;
            right.size = new Vector2(colliderWidth, H);

            left.offset = leftc;
            left.size = new Vector2(colliderWidth, H);
        }

        prev_W = W;
        prev_H = H;
    }

    void Update() {
        return;
        //check if aspect ratio change or window size changed 
        //is that what we want ?
        /*last_width = Screen.width;//cam.pixelRect.width;
        last_height = Screen.height;//cam.pixelRect.height;
        var top1 = cam.ScreenToWorldPoint(new Vector2((last_width / 2.0f), last_height + (colliderWidthScreen / 2.0f)));
        var bottom1 = cam.ScreenToWorldPoint(new Vector2(last_width / 2.0f, (-colliderWidthScreen / 2.0f)));
        var right1 = cam.ScreenToWorldPoint(new Vector2(last_width + (colliderWidthScreen / 2.0f), last_height / 2.0f));
        var left1 = cam.ScreenToWorldPoint(new Vector2((-colliderWidthScreen / 2.0f)+offset_left, last_height  / 2.0f));
        if (top1 != topc || bottom1!=bottomc || right1!= rightc || left1 != leftc){
            //keep the biggest one
            float W = Vector2.Distance(right1, left1) + colliderWidth;
            float H = Vector2.Distance(top1, bottom1) + colliderWidth;
            if ( W >= prev_W || H >= prev_H || cam.orthographicSize == cm.cameraZoomMax ){
                changeBoundary();
            }
        }*/
        /*
        if (cam.orthographicSize - last_size_max > 0)
            changeBoundary();
        last_size = cam.orthographicSize;
        if (last_size > last_size_max)
            last_size_max = last_size;
        */
    }
}