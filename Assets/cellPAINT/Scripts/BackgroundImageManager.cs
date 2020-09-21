using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundImageManager : MonoBehaviour
{
    //public bool showBackgroundImage;
    //public bool showcellPAINTScaleBar;
    //public bool backgroundImageOnTop;

    public Image theSprite;
    public Toggle showBackgroundImageToggle;
    public Toggle backgroundImageOnTopToggle;
    public Toggle showScaleBarToggle;
    public Toggle interactive_mode_toggle;
    public Slider imageScaleSlider;
    public Slider imageOpacitySlider;
    public InputField imageScaleField;
    public InputField imageOpacityField;
    public InputField Zrotation_field;
    public Slider Zrotation_slider;
    public Text ui_log;
    public GameObject cellPAINTScaleBar;

    public float imageScale = 1.0f;
    public float imageOpacity = 1.0f;
    public Vector2 backgroundImageOriginalResoution= new Vector2();

    public bool interactive_mode = false;//if on mouse can move them around
    public GameObject backgroundImageContainer;
    //hold a list of background image that we could move around
    public int current_bg = 0;
    private GameObject bg_holder;
    private List<GameObject> allbg;
    private Renderer backgroundImageRenderer;
    private Vector3 start_pos;
    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        allbg = new List<GameObject>();
        bg_holder = new GameObject("Backgrounds");
        bg_holder.transform.position = Vector3.zero;
        /*if (!backgroundImageContainer)
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
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        if (interactive_mode) {
            if (!Manager.Instance.allOff) {
                interactive_mode = false;
                interactive_mode_toggle.isOn = false;
            }
            LayerMask layerMask = 1 << LayerMask.NameToLayer("background");
            RaycastHit2D hit = new RaycastHit2D();
            hit = Physics2D.Raycast(Manager.Instance.current_camera.ScreenPointToRay(Input.mousePosition).origin,
                                Manager.Instance.current_camera.ScreenPointToRay(Input.mousePosition).direction, 100, layerMask);
            //highlight
            ClearOutline();
            if (current_bg != -1) {
                UpdateOutline(current_bg, true);
                ui_log.text = "selected background "+current_bg.ToString();
                theSprite.sprite = allbg[current_bg].GetComponent<SpriteRenderer>().sprite;
            }
            else {
                ui_log.text = "no background selected";
                theSprite.sprite = null;
            }
            if (hit && hit.collider){
                var bg = hit.collider.gameObject;
                var bgid = allbg.IndexOf(bg);
                if (bgid != -1) {
                    ui_log.text = "over background "+bgid.ToString();
                    theSprite.sprite = allbg[bgid].GetComponent<SpriteRenderer>().sprite;
                    UpdateOutline(bgid, true);
                    //attach and drag ?
                    if (Input.GetMouseButtonDown(0)){
                        current_bg = bgid;
                        start_pos = Manager.Instance.transform.position;
                        offset = allbg[current_bg].transform.position - start_pos;
                        //attach to mouse
                        //allbg[current_bg].transform.position = Manager.Instance.transform.position;

                    }
                    else if (Input.GetMouseButtonUp(0)){}
                    else if (Input.GetMouseButton(0) && Manager.Instance.delta > 0){
                        //translate using mouse delta ?
                        //Vector3 displacement = Manager.Instance.transform.position - start_pos;
                        //allbg[current_bg].transform.Translate(displacement);
                        allbg[current_bg].transform.position = Manager.Instance.transform.position + offset;
                    }
                }
            } 
            else 
            {
                if (Input.GetMouseButtonDown(0) && !Manager.Instance.mask_ui){
                    current_bg = -1;
                    ui_log.text = "no background selected";
                    theSprite.sprite = null;
                }
            }
        }
    }

    public void SetBackgroundImageOpacity(float value){
        imageOpacityField.text = value.ToString();
        imageOpacity = value;
        BackgroundImageOpacity();
    }

    public void SetBackgroundImageOpacity(string value){
        imageOpacity = float.Parse(value);
        imageOpacitySlider.value = imageOpacity;
        BackgroundImageOpacity();
    }

    public void SetBackgroundImageScale(float value){
        imageScaleField.text = (1.0f/value).ToString();
        imageScale = (1.0f/value);
        ScaleBackgroundImage();
    }

    public void SetBackgroundImageScale(string value){
        imageScale = 1.0f/float.Parse(value);
        imageScaleSlider.value = imageScale;
        ScaleBackgroundImage();
    }

    public void ScaleBackgroundImage ()
    {
        if (imageScale == 0) imageScale = 0.00001f;
        //backgroundImageContainer.transform.localScale = new Vector3 (imageScale*(backgroundImageOriginalResoution.x/100), 0, imageScale*(backgroundImageOriginalResoution.y/100));
        if (current_bg!=-1) {
            //scale the current sprites
            var pixel_scale = (Manager.Instance.unit_scale * 10.0f) / 100.0f;
            var unity_scale2d = 1.0f / (Manager.Instance.unit_scale * 10.0f);
            var local_scale = 1.0f/(pixel_scale * imageScale);
            allbg[current_bg].transform.localScale = new Vector3(local_scale, local_scale, local_scale);
        }
    }

    public void BackgroundImageOnTopToggle(bool value) {
        if (current_bg!=-1) {
             var sr = allbg[current_bg].GetComponent<SpriteRenderer>();
             if (value) {
                 sr.sortingOrder = 2;
                 sr.material.renderQueue = 3000;

                 }
             else {
                 sr.sortingOrder = 0;
                 sr.material.renderQueue = 0;
             }
        }
    }

    public void BackgroundImageOnTopToggle_cb()
    {
        /*
        if (backgroundImageOnTopToggle.isOn)
        {
            backgroundImageRenderer.material.renderQueue = 10000;
        }
        else
        {
            backgroundImageRenderer.material.renderQueue = 1;
        }*/
    }

    public void BackgroundImageOpacity()
    {
        Vector4 color = new Vector4 (1, 1, 1, imageOpacity);
        //backgroundImageRenderer.material.SetColor("_Color", color);
        if (current_bg!=-1) {
            var spriteRenderer = allbg[current_bg].GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetColor("_Color",new Color(1,1,1,imageOpacity) );
            spriteRenderer.SetPropertyBlock(mpb);
        }
    }

    public void ShowcellPAINTScaleBar()
    {
        cellPAINTScaleBar.SetActive(showScaleBarToggle.isOn);
    }

    public void ToggleBackgroundImage(bool toggle)
    {
        //allimage
        //backgroundImageContainer.SetActive(toggle);
        bg_holder.SetActive(toggle);
    }

    public void ToggleInteractingMode(bool toggle) {
        interactive_mode = toggle;
        ClearOutline();
        if (toggle)
        {
            Manager.Instance.allToggleOff();
            if (Manager.Instance.current_prefab)
                Manager.Instance.current_prefab.SetActive(!toggle);
        }
    }

    public void setZrot(float number){
        if (current_bg!=-1) {
            Zrotation_field.text = number.ToString();
            allbg[current_bg].transform.rotation = Quaternion.Euler(0, 0, number);
        }
    }

    public void setZrot(string number){
       if (current_bg!=-1) {
            Zrotation_slider.value = float.Parse (number); 
            allbg[current_bg].transform.rotation = Quaternion.Euler(0, 0, float.Parse (number));
       }
    }

    public void AddBackgroundSprites(Texture2D SpriteTexture){
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
        var prefab2d = new GameObject("bg_"+allbg.Count.ToString());
        prefab2d.transform.parent = bg_holder.transform;
        prefab2d.transform.position = new Vector3(0, 0, 0);
        prefab2d.layer = LayerMask.NameToLayer("background");
        //prefab.transform.position = cam.ScreenToWorldPoint(new Vector3(screenShot.width, Screen.height / 2.0f, 20));
        SpriteRenderer sp = prefab2d.AddComponent<SpriteRenderer>();
        Material amat = new Material(Manager.Instance.outline_material.shader);
        amat.renderQueue = 0;
        sp.material = amat;
        sp.sprite = NewSprite;
        sp.sortingOrder = 0;
        allbg.Add(prefab2d);
        //the scale
        var pixel_scale = (Manager.Instance.unit_scale * 10.0f) / 100.0f; // angstrom to pixel
        var unity_scale2d = 1.0f / (Manager.Instance.unit_scale * 10.0f); // unity to pixel
        var local_scale = 1.0f/(pixel_scale * imageScale);                // local scale if image is different
        prefab2d.AddComponent<BoxCollider2D>();
        prefab2d.transform.localScale = new Vector3(local_scale, local_scale, local_scale);
        showBackgroundImageToggle.isOn = true;   
    }

    public void AddBackgroundImage(Texture2D backgroundImage) {
        /*.GetComponent<Renderer>().material.mainTexture = backgroundImage;
        backgroundImageOriginalResoution = new Vector2 (backgroundImage.width, backgroundImage.height);
        backgroundImageContainer.transform.localScale = new Vector3 (backgroundImage.width/100, 1,  backgroundImage.height/100);
        backgroundImageContainer.SetActive(true);
        showBackgroundImageToggle.isOn = true;     */   
    }

    public void ClearOutline(){
        for (var i=0;i<allbg.Count;i++)
        {
            UpdateOutline(i, false);
        }
    }

    public void UpdateOutline(int bgid, bool is_outline)
    {
        var spriteRenderer = allbg[bgid].GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        if (is_outline) mpb.SetColor("_Color", new Color(1,1,0.5f,imageOpacity));
        else mpb.SetColor("_Color",new Color(1,1,1,imageOpacity) );
        //mpb.SetFloat("_Outline", is_outline ? Manager.Instance.current_camera.orthographicSize : 0);
        //mpb.SetColor("_OutlineColor", Color.yellow);
        spriteRenderer.SetPropertyBlock(mpb);
    }
}
