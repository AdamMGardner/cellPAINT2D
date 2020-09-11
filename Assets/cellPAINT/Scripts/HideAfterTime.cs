using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideAfterTime : MonoBehaviour
{
    public float time_to_disappear=30.0f;
    private float start_time;
    // Start is called before the first frame update
    void Start()
    {
        start_time = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        if ((Time.realtimeSinceStartup - start_time) > time_to_disappear)
        {
            gameObject.SetActive(false);
        }
    }
}
