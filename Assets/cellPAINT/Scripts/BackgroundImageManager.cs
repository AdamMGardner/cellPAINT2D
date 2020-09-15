using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundImageManager : MonoBehaviour
{
    //public bool showBackgroundImage;
    //public bool showcellPAINTScaleBar;
    //public bool backgroundImageOnTop;

    public Toggle showBackgroundImageToggle;
    public Toggle backgroundImageOnTopToggle;
    public Toggle showScaleBarToggle;
    public Slider imageScaleSlider;
    public Slider imageOpacitySlider;

    public GameObject cellPAINTScaleBar;

    public float imageScale = 1.0f;
    public float imageOpacity = 1.0f;
    public Vector2 backgroundImageOriginalResoution= new Vector2();

    public GameObject backgroundImageContainer;
    private Renderer backgroundImageRenderer;

    // Start is called before the first frame update
    void Start()
    {
        if (!backgroundImageContainer)
        {
            backgroundImageContainer = GameObject.CreatePrimitive(PrimitiveType.Plane);
            backgroundImageContainer.name = "Background Image Container";
            DestroyImmediate(backgroundImageContainer.GetComponent<MeshCollider>());
            backgroundImageContainer.transform.rotation = Quaternion.Euler(90, 180, 0);
            backgroundImageContainer.transform.position = new Vector3 (0, 0,0);

            backgroundImageRenderer = backgroundImageContainer.GetComponent<Renderer>();
            
            backgroundImageRenderer.material.shader = Shader.Find("Sprites/Default");
            backgroundImageRenderer.material.renderQueue = 1;
            backgroundImageOnTopToggle.isOn = false;

            backgroundImageContainer.SetActive(false);
            showBackgroundImageToggle.isOn = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScaleBackgroundImage ()
    {
        imageScale = imageScaleSlider.value;
        if (imageScale == 0) imageScale = 0.00001f;
        backgroundImageContainer.transform.localScale = new Vector3 (imageScale*(backgroundImageOriginalResoution.x/100), 0, imageScale*(backgroundImageOriginalResoution.y/100));
    }

    public void BackgroundImageOnTopToggle()
    {
        

        if (backgroundImageOnTopToggle.isOn)
        {
            backgroundImageRenderer.material.renderQueue = 10000;
        }
        else
        {
            backgroundImageRenderer.material.renderQueue = 1;
        }
    }

    public void BackgroundImageOpacity()
    {
        imageOpacity = imageOpacitySlider.value;
        Vector4 color = new Vector4 (1, 1, 1, imageOpacity);
        backgroundImageRenderer.material.SetColor("_Color", color);
    }

    public void ShowcellPAINTScaleBar()
    {
        cellPAINTScaleBar.SetActive(showScaleBarToggle.isOn);
    }

    public void ToggleBackgroundImage()
    {
        backgroundImageContainer.SetActive(showBackgroundImageToggle.isOn);
    }

}
