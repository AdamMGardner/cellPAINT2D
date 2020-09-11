using UnityEngine;
using System.Collections;

public class WheelUI : MonoBehaviour {
    public int Number_items=1;
    public float Radius = 1.0f;
    
    private GameObject item;
    private float last_radius;
    private int last_nbitems;

    //
    Vector3 getPosOnCircle(float angle) {
        return new Vector3(0.0f, Radius * Mathf.Sin(angle), Radius * Mathf.Cos(angle));
    }

	// Use this for initialization
	void Start () {
        item = transform.GetChild(0).gameObject;
        //generate the N instance of the object
        float Rincr = 360.0f / (float)Number_items;
        float currentR = Rincr;
        for (int i = 0; i < Number_items; i++) {
            GameObject instance = GameObject.Instantiate(item);//, getPosOnCircle(currentR), Quaternion.AngleAxis(currentR, Vector3.right)) as GameObject;
            instance.transform.parent = transform;
            instance.transform.localPosition = getPosOnCircle(Mathf.Deg2Rad*currentR);
            instance.transform.localRotation = Quaternion.AngleAxis(-currentR, Vector3.right);
            currentR += Rincr;
        }
        last_nbitems = Number_items;
        last_radius = Radius;
        item.SetActive(false);
        transform.position -= new Vector3(0, 0, Radius);
    }

    void Update_items()
    {
        float Rincr = 360.0f / (float)Number_items;
        float currentR = Rincr;
        for (int i = 0; i < Number_items; i++)
        {
            GameObject instance = transform.GetChild(i).gameObject;
            instance.transform.localPosition = getPosOnCircle(Mathf.Deg2Rad * currentR);
            instance.transform.localRotation = Quaternion.AngleAxis(-currentR, Vector3.right);
            currentR += Rincr;
        }
    }
	// Update is called once per frame
	void Update () {
        if (Radius != last_radius)
            Update_items();
        last_radius = Radius;
    }
}
