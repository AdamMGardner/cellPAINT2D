using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public InputField widthInput;
    public InputField heightInput;
    public GameObject topBoundry;
    public GameObject bottomBoundry;
    public GameObject rightBoundry;
    public GameObject leftBoundry;

    public Vector3 topc;
    public Vector3 bottomc;
    public Vector3 rightc;
    public Vector3 leftc;

    //private float last_width;
    //private float last_height;
    private float colliderWidthScreen;

    public float colliderWidth;
    public float canvasWidth = 1500;
    public float canvasHeight = 1500;
    public float boundryArea;
    public float offset_left=0.0f;//346
    private float last_size;
    private float last_size_max=0;
    private float prev_W=0.0f;
    private float prev_H=0.0f;
    private CameraMove cm;

    public float newW;
    public float newH;
    public bool boundsCreated;
    // Use this for initialization
    void Start()
    {
      
        cam = GetComponent<Camera>();
        cm=cam.GetComponent<CameraMove>();
        cam.orthographicSize = cm.cameraZoomMax - 100;
        //use user preference ?
        //last_width = maxWidthCanvas;//,Screen.width);// should it be a  defined size 
        //last_height = maxHeightCanvas;//,Screen.height);// should it be a  defined size 

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
        //last_size_max = last_size = cm.cameraZoomMax;

        changeBoundary();
        cam.orthographicSize = cm.cameraCurrentZoom;
    }

    void CreateABound(Vector3 size, Vector3 pos, string name){
        var bo = GameObject.Instantiate(quadPrefab);
        bo.name = name;
        bo.transform.parent = boundary.transform;
        bo.transform.position = pos;
        bo.transform.localScale = size;
        bo.AddComponent<BoxCollider2D>(); 

        if (name == "Top Bounds")
        {
            topBoundry = bo;
        }
        else if (name == "Bottom Bounds")
        {
            bottomBoundry = bo;
        }
        else if (name == "Right Bounds")
        {
            rightBoundry = bo;
        }
        else if (name == "Left Bounds")
        {
            leftBoundry = bo;
        }
        else
        {
            Debug.Log("Entered the CreateABound() function without a name");
        } 

    }
    public void SetNewWidth()
    {  
        newW = float.Parse(widthInput.text);
    }

    public void SetNewHeight()
    {
        newH = float.Parse(heightInput.text);
    }

    public void CheckNewWidthAndHeight()
    {
        if (newW <= canvasWidth) 
        {
            newW = canvasWidth;
        }

        if (newH <= canvasHeight) 
        {
            newH = canvasHeight;
        }

        cm=cam.GetComponent<CameraMove>();
        cam.transform.position = new Vector3 (0,0,-3000);
        cam.orthographicSize = cm.cameraZoomMax - 100; // Might need to come up with a clever way to rescale max zoom.
        //last_size_max = last_size = cm.cameraZoomMax;

        canvasWidth = newW;//,Screen.width);// should it be a  defined size 
        canvasHeight = newH;//,Screen.height);// should it be a  defined size 

        changeBoundary();
        cam.orthographicSize = cm.cameraCurrentZoom;
    }

    void changeBoundary()
    {
        Vector2 xa = cam.WorldToScreenPoint(new Vector2((-colliderWidth / 2), 0));
        Vector2 xb = cam.WorldToScreenPoint(new Vector2((colliderWidth / 2), 0));
        colliderWidthScreen = Vector2.Distance(xa, xb);

        topc = cam.ScreenToWorldPoint(new Vector2(Screen.width/2, (Screen.height/2)+(canvasHeight + colliderWidthScreen) / 2.0f));
        bottomc = cam.ScreenToWorldPoint(new Vector2(Screen.width/2, (Screen.height/2)-(canvasHeight + colliderWidthScreen) / 2.0f));
        rightc = cam.ScreenToWorldPoint(new Vector2((Screen.width/2)+((canvasWidth+ colliderWidthScreen) / 2.0f), Screen.height/2));
        leftc = cam.ScreenToWorldPoint(new Vector2((Screen.width/2)-((canvasWidth+ colliderWidthScreen) / 2.0f), Screen.height/2));

        float W = Vector2.Distance(rightc, leftc) + colliderWidth;
        float H = Vector2.Distance(topc, bottomc) + colliderWidth;

        boundryArea = (W - colliderWidth*2) * (H - colliderWidth*2);
        if (use_quad) 
        {
            if (!boundsCreated)
            {
                CreateABound(new Vector3(W, colliderWidth,0), new Vector3(topc.x, topc.y,1), "Top Bounds");
                CreateABound(new Vector3(W, colliderWidth,0), new Vector3(bottomc.x, bottomc.y,1), "Bottom Bounds");
                CreateABound(new Vector3(colliderWidth, H,0), new Vector3(rightc.x, rightc.y,1), "Right Bounds");
                CreateABound(new Vector3(colliderWidth, H,0), new Vector3(leftc.x, leftc.y,1), "Left Bounds");
            }
            else
            {
                //If the bounds are already created and do not need to be recreated.
                topBoundry.SetActive(false);
                bottomBoundry.SetActive(false);
                rightBoundry.SetActive(false);
                leftBoundry.SetActive(false);

                topBoundry.transform.position = new Vector3(topc.x,topc.y,1);
                topBoundry.transform.localScale =  new Vector3(W, colliderWidth,0);

                bottomBoundry.transform.position = new Vector3(bottomc.x ,bottomc.y,1);
                bottomBoundry.transform.localScale = new Vector3(W, colliderWidth,0);

                rightBoundry.transform.position = new Vector3(rightc.x, rightc.y,1);
                rightBoundry.transform.localScale = new Vector3(colliderWidth, H,0);

                leftBoundry.transform.position = new Vector3(leftc.x, leftc.y,1); 
                leftBoundry.transform.localScale = new Vector3(colliderWidth, H,0);  

                topBoundry.SetActive(true);
                bottomBoundry.SetActive(true);
                rightBoundry.SetActive(true);
                leftBoundry.SetActive(true);            
            }

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

        boundsCreated = true;

    }
}