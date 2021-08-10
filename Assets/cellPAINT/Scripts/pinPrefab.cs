using UnityEngine;
using System;
using System.Collections;
using UIWidgets;
using UIWidgetsSamples;
using UnityEngine.EventSystems;


public class pinPrefab : MonoBehaviour
{
    public bool pinMode = false;
    public GameObject pinIcon;
    public bool collider_mode = false;

    private GameObject toPin;

    public void ToggleMode(bool toggle)
    {
        pinMode = toggle;
        pinIcon.GetComponent<SpriteRenderer>().enabled = toggle;
    }

    // Use this for initialization
    void Start()
    {
        if (!pinIcon)
        {
            pinIcon = transform.GetChild(0).gameObject;
            pinIcon.GetComponent<SpriteRenderer>().enabled = false;
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collider_mode)
        {
            toPin = other.gameObject;
        }
    }

    GameObject erase_raycast()
    {
        var mainCamera = FindCamera();
        // We need to actually hit an object
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
    void Update()
    {
        if (!pinMode)
        {
            pinIcon.GetComponent<SpriteRenderer>().enabled = false;
            return;
        }

        if (!collider_mode)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
            {
                toPin = erase_raycast();
            }
        }

        if (toPin)
        {
            Debug.Log(toPin.name);
            if (toPin == gameObject) return;
            toPin.GetComponent<Rigidbody2D>().isKinematic = true;
            Debug.Log("Pinned?");
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