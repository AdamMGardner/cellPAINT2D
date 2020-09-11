using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class spriteShadow : MonoBehaviour {
    //public bool castShadow;
    //public bool receiveShadow;

    private Renderer arenderer;

    // Use this for initialization
    void Start () {
        arenderer = gameObject.GetComponent<Renderer>();
        arenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        arenderer.receiveShadows =true;
    }
	
	// Update is called once per frame
	void Update () {


    }
}
