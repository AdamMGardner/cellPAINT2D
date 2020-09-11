using UnityEngine;
using System;
using System.Collections;
using UIWidgets;
using UIWidgetsSamples;
using UnityEngine.EventSystems;


public class ErasePrefab : MonoBehaviour {
    public bool eraseMode = false;
    public GameObject eraseIcon;
    public bool collider_mode = false;

    private GameObject toDestroy;
    private moveClick manager;

    public void ToggleMode(bool toggle) {
        eraseMode = toggle;
        eraseIcon.GetComponent<SpriteRenderer>().enabled = toggle;
        //if (eraseMode&& collider_mode) {
        //    gameObject.layer = 0; //default ... what about himself
        //}
        //else gameObject.layer = 11; //camera collider
    }

	// Use this for initialization
	void Start () {
        manager = GetComponent<moveClick>();
        if (!eraseIcon)
        {
            eraseIcon = transform.GetChild(0).gameObject;
            eraseIcon.GetComponent<SpriteRenderer>().enabled = false;
        }

	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collider_mode)
        {
            toDestroy = other.gameObject;
        }
    }

    GameObject erase_raycast() {
        var mainCamera = FindCamera();
        // We need to actually hit an object
        //LayerMask layerMask = ~(1 << LayerMask.NameToLayer("CameraCollider"));//ignore camera collider
        LayerMask layerMask = ~(1 << LayerMask.NameToLayer("CameraCollider") | 1 << LayerMask.NameToLayer("FiberPushAway")); // ignore both layerX and layerY

        RaycastHit2D hit = new RaycastHit2D();
        hit = Physics2D.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition).origin, mainCamera.ScreenPointToRay(Input.mousePosition).direction, 100, layerMask);

        if (!hit)
        {
            return null;
        }
        return hit.collider.gameObject;
    }

    // Update is called once per frame
    void Update() {
        if (!eraseMode)
        {
            eraseIcon.GetComponent<SpriteRenderer>().enabled = false;
            return;
        }

        if (!collider_mode) { 
            if (Input.GetMouseButton(0)|| Input.GetMouseButtonDown(0))
            {
                toDestroy = erase_raycast();
            }
        }
        
        if (toDestroy)
        {
            Debug.Log(toDestroy.name);
            if (toDestroy == gameObject) return;
            manager.DestroyInstance(toDestroy);
            Debug.Log("Destroyed?");
        }
    }

    private Camera FindCamera()
    {
        if (GetComponent<Camera>())
        {
            return GetComponent<Camera>();
        }

        return Camera.main;
    }
}
