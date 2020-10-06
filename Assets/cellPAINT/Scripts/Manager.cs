using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UIWidgets;
using UIWidgetsSamples;
using UnityEngine.EventSystems;
using UnityStandardAssets.Utility;
//using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;
using SimpleJSON;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Accord;
//TODO:
//-loading async
//-get rid of the sorting order and use -Z with camera rotation. this will normal depth sorting
//[ExecuteInEditMode]
public class Manager : MonoBehaviour {
    //need cleanup 
    //all boolean 
    public bool DEBUG = false;
    public string version = "v2.0";
    //public bool useECS=false;
    public bool HideInstance = true;

    public Camera current_camera;
    public GameObject current_prefab;
    public GameObject myPrefab;
    public GameObject root;
    public bool layer_number_draw = true;
    public int layer_number_options = 3;//three layer 
    public bool boundMode = false;
    public bool surfaceMode = false;//set geT ?
    public bool fiberMode = false;
    public bool drawMode = false;
    public bool dragMode = false;
    public bool eraseMode = false;
    public bool pinMode = false;
    public bool infoMode = false;
    public bool groupMode = false;
    public bool ghostMode = false;
    public bool measureMode = false;
    public bool allOff = false;
    public bool continuous = false;
    public bool bucketMode = false;
    public bool bindMode = false;
    private bool group_interact_mode = true;
    private bool moveWithMode = false;
    //public bool erase_collider_mode = false;
    //public bool pin_collider_mode = false;
    //public bool drag_collider_mode = false;

    #region mouse parameters
    public int layer_frequence = 3;
    public int nbInstancePerClick = 1;
    public float radiusPerClick = 1;
    public bool mask_ui = false;
    private Vector3 mousePos;
    private Vector3 prev_mousePos;
    private Vector3 startPos;    // Start position of line
    private Vector3 endPos;    // End position of line
    //private Vector3 lastEndPos;    // last End position of line

    private Vector3 lastPos;
    private Vector3 offsetPos;
    public float delta;
    public float delta_threshold = 5;
    public float sprayModulous;
    private bool even = true;

    //private CircleCollider2D mouseCollide;
    private bool mouseDown;
    public Vector2 mousePositionInViewPort;
    #endregion mouse parameters
    #region fiber parameters
    public float membrane_thickness_delta = 1.0f;
    public float membrane_thickness = 42.0f;
    public float fiber_length;
    public float fiber_closing_distance;
    //public float fiber_current_distance;
    public bool fiber_distanceJoint = false;
    public bool fiber_hingeJoint = false;
    public float fiber_distance = 0.05f;
    public int fiber_persistence = 1;
    public int fiber_count=0;
    private int fiber_nextNameNumber = 0;
    private bool fiber_init = false;
    public string fiber_init_compartment = "";
    public string fiber_compartment = "";
    public GameObject fiber_parent;
    public List<GameObject> fiber_parents;
    public List<List<GameObject>> fibers_instances;
    public GameObject fiber_attachto;

    #endregion fiber parameters
    #region UI parameters
    public Canvas _canvas;
    public Text version_label;
    public RectTransform uiHolder;
    public Text scale_bar_text;
    public Toggle ToggleBrush;
    public Toggle TogglePhysics;
    public GameObject message_panel;
    //need a progress bar for loading
    //private Progressbar pb;
    private Text currentLabel;

    #region UI descriptions parameters
    public Image tools_toggle_description_image;  //description panel protein image
    public GameObject Description_Holder;
    public GameObject Description_Holder_HSV;
    public Toggle show_membrane_bg;
    public Text textCommonName;
    public Text textCompartment;
    public Text textFunctionGroup;
    public Text textDescription;
    #endregion UI descriptions parameters
    #endregion UI parameters
    #region attachemnts 
    private List<PrefabProperties> attached_object;
    public List<GameObject> attached;
    public List<SpringJoint2D> attachments;
    public GameObject bound_lines_holder;
    public List<LineRenderer> bound_lines;
    public GameObject attach1;
    public GameObject attach2;
    public Vector3 attachPos1;
    public Vector3 attachPos2;
    #endregion attachemnts
    public float distance_attach = 0.8f;
    public float frequency_attach = 2.5f;
    public int type_attach = 0;
    public bool collision_attach = true;
    public float proteinArea;
    public float screenArea;
    public float percentFilled;

    public int totalNprotein = 0;
    //public GameObject listHolder;
    public string current_name_below;
    public string last_active_current_name_below;
    public GameObject current_objectparent_below;
    private string current_name;

    private LineRenderer line;
    private Collider2D otherSurf;
    public GameObject other;
    public GameObject last_other;
    

    private ErasePrefab eraser;
    
    private PrefabProperties current_properties;
    public Dictionary<string, int> proteins_count;
    private Dictionary<string, Text> proteins_ui_labels;
    public Dictionary<string, Material> prefab_materials;
    public Dictionary<string, GameObject> all_prefab = new Dictionary<string, GameObject>();

    private GameObject pushAway;
    public bool update_texture = false;
    private Texture2D compartment_texture;

    private bool stop = false;
    
    #region erase options
    private int count_removed = 0;
    #endregion erase options
    #region drag options
    private DragRigidbody2D dragger;
    public float frequency = 10.0f;
    public float damping = 1.0f;

    //const float k_Spring = 50.0f;
    //const float k_Damper = 20.0f;
    const float k_Drag = 10.0f;
    const float k_AngularDrag = 5.0f;
    //const float k_Distance = 0.2f;
    //const bool k_AttachToCenterOfMass = false;

    private SpringJoint2D m_SpringJoint;
    private RigidbodyType2D bodyType;
    #endregion drag options

    private GameObject below;
    private List<PrefabProperties> pinned_object;
    

    public GameObject thermometer;
    public GameObject mercury;
    public GameObject clockwise;

    public JSONNode AllIngredients;
    public JSONNode AllRecipes;
    public Dictionary<string, JSONNode> ingredient_node;
    //public List<string> ingredients_names;
    public Dictionary<string,int> ingredients_names = new Dictionary<string, int>();
    public Dictionary<int,string> ingredients_ids = new Dictionary<int, string>();
    public List<string> additional_ingredients_names = new List<string>();
    public List<string> additional_compartments_names = new List<string>();
    public Dictionary<int,string> sprites_names = new Dictionary<int, string>();
    public Dictionary<string,Texture2D> sprites_textures = new Dictionary<string, Texture2D>();
    //public List<GameObject> ingredients_prefab = new List<GameObject>();

    [HideInInspector]
    public List<Rigidbody2D> everything;

    [HideInInspector]
    public Rigidbody2D[] bounded;
    public List<GameObject> selectedobject;
    public int rbCount = 0;
    //public int rbSurfaceCount = 0;
    public int boundedCount = 0;
    public int MaxRigidBodies = 5000;
    //public int Ninstance;
    public float scale_force = 1.0f;
    public float timeScale = 1.0f;
    public float unit_scale = 1.0f;
    public List<GameObject> surface_objects;

    public List<string> selected_prefab;
    public GameObject selected_instance;
    private bool prefab_changing_lock = false;

    #region materials 
    //public Material spritedefault;
    public Material outline_material;
    public Material nucleicacid_material;
    public Material manager_prefab_material;
    public Material lineMat;
    #endregion materials

    public GameObject Background;

    public LineRenderer lines;
    public LineRenderer fiber_lines;

    public Camera secondCamera;
    public Shader secondShader;
    public RenderTexture secondRenderTexture;
    
    public RecipeUI recipeUI;
    public GameObject warningPanel;

    

    private int count_used = 0;
    private int fixedCount = 0;
    public int _Seed = 1;
    public int default_bucket_count = 10;

    private int count_update = 0;

    public Text timescale_label;
    //public bool show_time;
    public Text deltatime_labels;
    public float total_time;
    public float total_time_sec;
    public float dt;
    public float safety_deltaTime = 0.5f;
    public int safety_frame =0;
    public float scaling_fiber=1.0f;
    public float scaling_surface=1.0f;
    public float scaling_soluble=1.0f;

    public float zLevel = 0.00f;
    public float colorValue = 1.00f;
    public bool layerDirection = true;
    private float lerp_time = 0.0f;
    //public float bicycle_radius = 1.0f;//should be equal the size of the membrane


    /*image effect*/
    //private UnityStandardAssets.ImageEffects.Bloom bloomEffect;
    //private UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration vignetteEffect;
    //private UnityStandardAssets.ImageEffects.BlurOptimized blurEffect;
    //private UnityStandardAssets.ImageEffects.Grayscale grayscaleEffect;
    private static Manager _instance = null;
    public static Manager Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<Manager>();
            if (_instance == null)
            {
                var go = GameObject.Find("manager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("manager"); //{ hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<Manager>();
            }
            return _instance;
        }
    }

    public void ToggleLayerNumberDraw(bool toggle) {
        layer_number_draw = toggle;
    }

    public void TogglePhysicsSimulation(bool toggle) {
        Physics2D.autoSimulation = toggle;
    }

    public string getDescription(string iname)
    {
        //return description text given an ingredient name
        if (AllIngredients.GetAllKeys().Contains(iname)) {
            return AllIngredients[iname]["description"];
        }
        var sname = iname.Split('.');
        if ( sname.Length != 0) {
            iname = sname[2];
            Debug.Log("getDescriptionName Split "+iname);
            if (AllIngredients.GetAllKeys().Contains(iname)) {
            if (AllIngredients[iname].GetAllKeys().Contains("description")) return AllIngredients[iname]["description"];
            }
        } 
        if (iname.Contains("_membrane")){
            iname = sname[0];
            Debug.Log("getDescriptionName Split "+iname);
            if (AllIngredients.GetAllKeys().Contains(iname)) {
            if (AllIngredients[iname].GetAllKeys().Contains("description")) return AllIngredients[iname]["description"];
            }
        }
        return "";
    }

    public string getDescriptionName(string iname)
    {
        Debug.Log("getDescriptionName "+iname);
        //return description text given an ingredient name
        if (AllIngredients.GetAllKeys().Contains(iname)) {
            if (AllIngredients[iname].GetAllKeys().Contains("name")) return AllIngredients[iname]["name"];
        }
        var sname = iname.Split('.');
        if ( sname.Length != 0) {
            iname = iname.Split('.')[2];
            Debug.Log("getDescriptionName Split "+iname);
            if (AllIngredients.GetAllKeys().Contains(iname)) {
            if (AllIngredients[iname].GetAllKeys().Contains("name")) return AllIngredients[iname]["name"];
            }
        } 
        return "";
    }

    public void ToggleBackgroundMembrane(bool toggle) {
        if (all_prefab.ContainsKey(current_name_below))
        {
            var p = all_prefab[current_name_below].GetComponent<PrefabProperties>();
            if (p!=null && p.draw_background){
                //loop over all fiber parent
                foreach(var fp in fiber_parents)
                {
                    if (fp.name.StartsWith(current_name_below) && fp.name.EndsWith("_Closed")){
                        var contour = fp.GetComponent<DrawMeshContour>();
                        if (contour) contour.ToggleDisplay(toggle);
                    }
                }
            }
        }
        /*var touse = below;
        if (below == null) {
            touse = last_other;
        };
        var props = touse.GetComponent<PrefabProperties>();
        if (props.is_fiber && props.draw_background){
            var parent = touse.transform.parent;
            var dmc = parent.GetComponent<DrawMeshContour>();
            if (dmc) dmc.ToggleDisplay(toggle);
        }*/
    }

    public void changeDescription(GameObject another, SpriteRenderer otherImage) {

        var props = another.GetComponent<PrefabProperties>();
        Debug.Log("changeDescription props name "+props.name+" "+props.is_fiber.ToString());
        var n = props.name.Replace("_"," ");
        var sn = getDescriptionName(props.name);
        if (sn != "") {
            n = sn;
        }
        else if ((props.common_name != "") && (props.common_name != null)) 
        {
            n = props.common_name;
        }
        Debug.Log("changeDescription "+n);
        current_name_below = props.name;
        //if (props.is_surface) n += " transmembranar";
        textCommonName.text = "<b>NAME:</b> " + n;
        textCompartment.text = "";
        textFunctionGroup.text = "";
        textDescription.text = "";
        if ((props.compartment != "") && (props.compartment != null)) 
        {
            textCompartment.gameObject.SetActive(true);
            textCompartment.text = "<b>COMPARTMENT:</b> " + props.compartment;
        }
        else {
            textCompartment.gameObject.SetActive(false);
        }
        if ((props.function_group != "") && (props.function_group != null))
        {
            textFunctionGroup.gameObject.SetActive(true);
            textFunctionGroup.text = "<b>FUNCTION GROUP:</b> " + props.function_group;
        }
        else 
        {
            textFunctionGroup.gameObject.SetActive(false);
        }
        if ((props.description == null) || (props.description == ""))
        {
            props.description=getDescription(props.name);
        }
        if ((props.description != null) && (props.description != ""))
        {
            textDescription.text = "<b>DESCRIPTION:</b> " + props.description;
        }

        //Debug.Log("The iname for the prefab find is" + iname);
        if (props.is_fiber && props.draw_background)
        {
            //check if membrane
            textDescription.text += " (note: draw clockwise)";
            if (show_membrane_bg != null) {
                show_membrane_bg.gameObject.SetActive(true);
            }
        }
        else {
            if (show_membrane_bg != null) {
                show_membrane_bg.gameObject.SetActive(false);
            }            
        }
        if (otherImage) {
            tools_toggle_description_image.sprite = otherImage.sprite;
            tools_toggle_description_image.material = otherImage.sharedMaterial;
            if (otherImage.sharedMaterial != null) {
                Description_Holder_HSV.GetComponent<ColorPicker>().Color = otherImage.sharedMaterial.color;
            }
            var pw = tools_toggle_description_image.transform.GetComponentInParent<RectTransform>().rect.width;
            var ratio =(float) otherImage.sprite.texture.width/(float)otherImage.sprite.texture.height;
            //var w = 150;//(snode.data.thumbnail)?snode.data.thumbnail.width:150;
            var h = 210;//w/ratio;//(snode.data.thumbnail)?snode.data.thumbnail.height:150;
            var w = h*ratio;
            //if w > max change the w
            if (w > pw) {
                w = pw;
                h = (int) ((float)w / ratio);
            }
            tools_toggle_description_image.rectTransform.sizeDelta = new Vector2(w,(int)h);   
            float cscale = Manager.Instance._canvas.transform.localScale.x;
            float scale2d  = props.scale2d;
            //float w = (float)tools_toggle_description_image.rectTransform.rect.width;
            //float h = (float)tools_toggle_description_image.rectTransform.rect.height;
            var canvas_scale = w/tools_toggle_description_image.sprite.texture.width; 
            var sc2d = scale2d*canvas_scale*cscale;           
            if (props.is_surface){
                UI_manager.Get.IngredientSpriteMb.gameObject.SetActive(true);
                var offy = -props.y_offset*sc2d/cscale;
                var p = tools_toggle_description_image.rectTransform.localPosition;
                UI_manager.Get.IngredientSpriteMb.rectTransform.localPosition = new Vector3(p.x,offy,p.z);
                var thickness = membrane_thickness*sc2d/cscale;//angstrom
                //theSpriteMb.rectTransform.sizeDelta = new Vector2((int)w,42.0f);

                UI_manager.Get.IngredientSpriteMb.rectTransform.sizeDelta = new Vector2((int)pw,thickness);
                Debug.Log("props for "+props.name+" "+props.y_offset.ToString()+" "+props.scale2d.ToString());
            }
            else {
                UI_manager.Get.IngredientSpriteMb.gameObject.SetActive(false);
            }
            if (props.is_fiber && !props.draw_background){
                var pixel_length = props.y_length*sc2d;
                UI_manager.Get.IngredientSpriteFiberLeft.gameObject.SetActive(true);
                UI_manager.Get.IngredientSpriteFiberRight.gameObject.SetActive(true);
                var p = tools_toggle_description_image.rectTransform.position;
                UI_manager.Get.IngredientSpriteFiberLeft.sprite = otherImage.sprite;
                UI_manager.Get.IngredientSpriteFiberRight.sprite = otherImage.sprite;
                UI_manager.Get.IngredientSpriteFiberLeft.material = otherImage.sharedMaterial;
                UI_manager.Get.IngredientSpriteFiberRight.material = otherImage.sharedMaterial;
                UI_manager.Get.IngredientSpriteFiberLeft.rectTransform.position = new Vector3(p.x-pixel_length/2.0f,p.y,p.z);
                UI_manager.Get.IngredientSpriteFiberRight.rectTransform.position = new Vector3(p.x+pixel_length/2.0f,p.y,p.z);
                tools_toggle_description_image.enabled = false;
                Debug.Log("props for "+props.name+" "+props.y_length.ToString()+" "+props.scale2d.ToString());
            }
            else {
                UI_manager.Get.IngredientSpriteFiberLeft.gameObject.SetActive(false);
                UI_manager.Get.IngredientSpriteFiberRight.gameObject.SetActive(false);
                tools_toggle_description_image.enabled = true;
            }
        } else {
             tools_toggle_description_image.sprite = null;
        }

        //this call changeColor
        //this should change the current name below
        

        //Text name = Description_Holder.transform.GetChild(0).GetChild(0).GetChild(1).GetComponentInChildren<Text>();
        //name.text = iname;
    }

    /*public void changeDescription(GameObject other, Image otherImage)
    {
        Text description = textDescription;
        var props = other.GetComponent<PrefabProperties>();
        if (props.description == null)
        {
            description.text = getDescription(props.name);
        }
        else { description.text = props.description; }

        //Debug.Log("The iname for the prefab find is" + iname);
        //tools_toggle_description_image.sprite = other.sprite;
        //tools_toggle_description_image.material = other.material;
        //Description_Holder.transform.GetChild(0).GetComponent<ColorPicker>().Color = other.material.color;
        //Text name = Description_Holder.transform.GetChild(0).GetChild(0).GetChild(1).GetComponentInChildren<Text>();
        //name.text = iname;
    }*/

    public void updateFiberPrefab(GameObject aPrefab)
    {
        //CircleCollider2D[] allc = aPrefab.GetComponents<CircleCollider2D>();
        //float scale_factor = aPrefab.GetComponent<PrefabProperties>().fiber_scale;
        //Debug.Log("updateFiberPrefab " + aPrefab.name + " " + allc.Length.ToString());
        //Vector2 pos1 = new Vector2(aPrefab.transform.position.x, aPrefab.transform.position.y) + (allc[0].offset * scale_factor);
        //Vector2 pos2 = new Vector2(aPrefab.transform.position.x, aPrefab.transform.position.y) + (allc[allc.Length - 1].offset * scale_factor);
        fiber_length = aPrefab.GetComponent<PrefabProperties>().fiber_length;// Vector2.Distance(pos1, pos2);
        fiber_closing_distance = fiber_length * 3.0f;
        //myPrefab.SetActive(true);
    }

    public void SwitchPrefab(int dropItem, ListViewItem component)
    {
        //this is called from the user interface, swicth to draw directly
        //TreeViewSampleComponent comp = component as TreeViewSampleComponent;
        //var selected = ui.SelectedIndex;
        //currentLabel = comp.Text;
        //string name = currentLabel.text.Split(":".ToCharArray()[0])[0];
        //SwitchPrefabFromName(name);

    }

    public void checkMaterial(GameObject toCheck, PrefabProperties props = null) {
        PrefabProperties p = toCheck.GetComponent<PrefabProperties>();
        if ((props == null) || (p != null))
            props = p;
        if (props)
            props.Setup(true);
        else {
            return;
        }
        SpriteRenderer sr = toCheck.GetComponent<SpriteRenderer>();
        if (sr != null) {
            string name = props.name;
            if (!prefab_materials.ContainsKey(name)) {
                prefab_materials.Add(name, createNewSpriteMaterial(name));
            }
            sr.sharedMaterial = prefab_materials[name];
            if (!toCheck.activeInHierarchy) toCheck.SetActive(true);
        }
        foreach (Transform ch in toCheck.transform) {
            checkMaterial(ch.gameObject, props);
        }
    }

    public void changeColorBackground(Color32 color)
    {
        RenderSettings.fogColor = color;
        if (Background) Background.GetComponent<Renderer>().material.color = color;
        
    }

    public void changeColorMaterial(Color32 color)
    {
        changeColorMaterial(current_name_below, color);
        if (current_prefab!=null){
            if (drawMode) {
                var sr = current_prefab.GetComponent<SpriteRenderer>();
                if (sr) current_prefab.GetComponent<SpriteRenderer>().sharedMaterial.color = color;
            }
        }
        //for closed chains with a background
        //check if name below is a fiber that draw a backgroundfiber_parents
        if (all_prefab.ContainsKey(current_name_below))
        {
            var p = all_prefab[current_name_below].GetComponent<PrefabProperties>();
            if (p!=null && p.draw_background){
                //loop over all fiber parent
                foreach(var fp in fiber_parents)
                {
                    if (fp.name.StartsWith(current_name_below) && fp.name.EndsWith("_Closed")){
                        var contour =fp.GetComponent<DrawMeshContour>();
                        if (contour) contour.mr.sharedMaterial.color = color;
                    }
                }
            }
        }
    }

    public void changeColorMaterial(string name, Color32 color) {
        if (prefab_materials.ContainsKey(name))
            prefab_materials[name].color = color;//
    }

    public Material createNewSpriteMaterial(string name) {
        /*GameObject aPrefab;
        if (!all_prefab.ContainsKey(name))
        {
            aPrefab = Resources.Load("Prefabs/" + name) as GameObject;
            if (aPrefab == null)
            {
                Debug.Log(name + " was not found");
                return null;
            }
            all_prefab.Add(name, aPrefab);
        }
        else
            aPrefab =  all_prefab[name];
       
        //if (myPrefab.GetComponent<SpriteRenderer>().sharedMaterial != null) return myPrefab.GetComponent<SpriteRenderer>().sharedMaterial;
        Shader shader;
        if (aPrefab == null)
        {
            Debug.Log(name + " was not found");
            return null;
        }
        if (aPrefab.tag == "NA")
        {
            shader = nucleicacid_material.shader;
        }
        else
        {
            shader = outline_material.shader;
        }
         */
        Shader shader;
        if (name.Contains("Draw DNA") || name.Contains("RNA"))
        {
            shader = nucleicacid_material.shader;
        }
        else
        {
            shader = outline_material.shader;
        }
        //Shader shader = outline_material.shader;
        Material amat = new Material(shader);
        if (name.Contains("Draw DNA") || name.Contains("RNA"))
            amat.SetFloat("_Nucleic", 1.0f);
        //gets the default color for the ingredient (if there is one) and applies it to the material.
        //Debug.Log("The prefab for material creation is " + myPrefab.name);
        //Adam Flag
        //Color defaultColor = aPrefab.GetComponent<PrefabProperties>().Default_Sprite_Color;
        //if (defaultColor != new Color(0, 0, 0, 0) && useDefaultColor)
        //{
        //    amat.color = defaultColor;
        //}
        //else
        //{
        //    amat.color = Color.white;
        //}
        amat.color = Color.white;
        amat.name = name;
        if (ingredient_node.ContainsKey(name)){
            var node = ingredient_node[name];
            if (node!=null && node.HasKey("color")) 
            {
                amat.color = new Color(  node["color"].AsArray[0].AsFloat,
                                        node["color"].AsArray[1].AsFloat,
                                        node["color"].AsArray[2].AsFloat);
            }
        }
        //Debug.Log("The ingrediant name is " + name);
        //Debug.Log("The material Name is " + amat.name);
        return amat;
    }

    public void BuildPrefab(GameObject P, string path, string name)
    {
#if UNITY_EDITOR
        string aname = P.name;
        P.name = name;
        PrefabUtility.SaveAsPrefabAsset(P,path);
        //PrefabUtility.CreatePrefab(path, P, ReplacePrefabOptions.ReplaceNameBased);
        P.name = aname;
#endif
    }

    public void BuildPrefab2D(string aname = null)
    {
        string prefab_name = aname;
        string img_name = aname;
        int inde = ingredients_names[aname];// ingredients_names.IndexOf(aname);
        if (sprites_names.ContainsKey(inde)) img_name = sprites_names[inde];
        var sprite = Resources.Load<Sprite>("Recipie/" + img_name);
        var prefab2d = new GameObject(prefab_name);
        //prefab.transform.position = cam.ScreenToWorldPoint(new Vector3(screenShot.width, Screen.height / 2.0f, 20));
        SpriteRenderer sp = prefab2d.AddComponent<SpriteRenderer>();
        Material amat = new Material(outline_material.shader);
        sp.material = amat;
        PrefabProperties props = prefab2d.AddComponent<PrefabProperties>();
        //automatic change of the properties based on type of element
        //get the properties from ingredient_node[aname];
        props.name = prefab_name;
        //props.pdb_source = pdbid;
        sp.sprite = sprite;
        string path = "Assets/Resources/Prefabs/" + prefab_name + ".prefab";
        //prefab2d.AddComponent<CircleCollider2D>();
        BuildPrefab(prefab2d, path, prefab_name);
        Destroy(prefab2d);
    }

    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))         // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        Debug.Log(FilePath);
        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f), PixelsPerUnit);
        return NewSprite;
    }

    public Sprite GetSprite(string img_name)
    {
        Debug.Log("GetSprite "+img_name);
        var sprite = Resources.Load<Sprite>("Recipie/" + Path.GetFileNameWithoutExtension(img_name));
        if (sprite == null)
        {
            Debug.Log("GetSprite "+sprites_textures.ContainsKey(img_name));
            //check in the dictionary
            if (sprites_textures.ContainsKey(img_name)){
                var SpriteTexture = sprites_textures[img_name];
                sprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            else {
                var ext = Path.GetExtension(img_name);
                var iname = Path.GetFileNameWithoutExtension(img_name);
                var image_path = PdbLoader.GetAFile(iname, "images", ext);
                if ( image_path == null ) sprite = Resources.Load<Sprite>("Recipie/error");
                else sprite = LoadNewSprite(image_path);
            }
        }
        Debug.Log("Return Sprite "+sprite.name);
        return sprite;
    }

    public GameObject Build(string aname = null,string img_name = null) {
        string prefab_name = aname;
        if (img_name == null) img_name = aname;
        int inde = ingredients_names[aname];
        if (inde != -1 && sprites_names.ContainsKey(inde)) {
            img_name = sprites_names[inde];
        }
        else
        {
            Debug.Log("Build didnt found " + aname + " " + inde.ToString());
            return null;
        }
        if (img_name.Contains("DNA") && img_name.Contains("Draw"))
        {
            img_name = "DNA_full_Turn";
        }
        Debug.Log("Build " + aname + " with sprite " + img_name);
        var sprite = GetSprite(img_name);
        var prefab2d = new GameObject(prefab_name);
        //prefab.transform.position = cam.ScreenToWorldPoint(new Vector3(screenShot.width, Screen.height / 2.0f, 20));
        SpriteRenderer sp = prefab2d.AddComponent<SpriteRenderer>();
        Material amat = new Material(outline_material.shader);
        sp.material = amat;
        PrefabProperties props = prefab2d.AddComponent<PrefabProperties>();
        props.name = prefab_name;
        //props.pdb_source = pdbid;
        sp.sprite = sprite;
        prefab2d.transform.position = new Vector3(10000, 10000, 10000);
        if (img_name.Contains("DNA")) {
            props.nucleic_acid_depth = true;
            sp.material = new Material(nucleicacid_material.shader);
        }
        prefab2d.SetActive(false);
        return prefab2d;
    }

    public void SwitchPrefabFromName(string name)
    {
        Manager.Instance.update_texture = true;
        //myPrefab = Resources.Load("Prefabs/" + name) as GameObject;
        if (!all_prefab.ContainsKey(name))
        {
            Debug.Log(name + " not found in all_prefab");
            myPrefab = Resources.Load("Prefabs/" + name) as GameObject;
            if (myPrefab==null)
                myPrefab = Build(name);
            myPrefab.SetActive(true);
            all_prefab.Add(name, myPrefab);
        }
        else
            myPrefab = all_prefab[name];
        //myPrefab = Instantiate(Resources.Load("Prefabs/" + name, typeof(GameObject))) as GameObject;
        //myPrefab.hideFlags = HideFlags.HideInHierarchy;

        if (current_prefab) GameObject.Destroy(current_prefab);
        if (!myPrefab) return;

        if (!all_prefab.ContainsKey(name)) all_prefab.Add(name, myPrefab);
        checkMaterial(myPrefab);

        prefab_changing_lock = true;
        if (!proteins_count.ContainsKey(name))
        {
            proteins_count.Add(name, 0);
            if (!proteins_ui_labels.ContainsKey(name)) proteins_ui_labels.Add(name, currentLabel);
        }
        //Debug.Log("description for " + name);
        string D = getDescription(name);
        //Debug.Log(D);
        current_name = name;
        //myPrefab.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.StartAsleep;

        current_prefab = Instantiate(myPrefab, transform.position, Quaternion.identity) as GameObject;
        current_prefab.transform.parent = transform;
        current_prefab.SetActive(true);
        //current_prefab.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        PrefabProperties props = current_prefab.GetComponent<PrefabProperties>();
        current_properties = props;
        fiber_persistence = props.persistence_length;
        SpriteRenderer sr = current_prefab.GetComponent<SpriteRenderer>();
        if (sr) sr.sharedMaterial = manager_prefab_material;
        if (name.Contains("Draw DNA") || name.Contains("RNA")) {
            props.nucleic_acid_depth = true;
        }
        foreach (CircleCollider2D coll in current_prefab.GetComponents<CircleCollider2D>())
        {
            coll.isTrigger = true;
            coll.enabled = false;
        }
        foreach (CircleCollider2D coll in current_prefab.GetComponentsInChildren<CircleCollider2D>())
        {
            coll.isTrigger = true;
            coll.enabled = false;
        }
        foreach (CapsuleCollider2D coll in current_prefab.GetComponents<CapsuleCollider2D>())
        {
            coll.isTrigger = true;
            coll.enabled = false;
        }
        foreach (CapsuleCollider2D coll in current_prefab.GetComponentsInChildren<CapsuleCollider2D>())
        {
            coll.isTrigger = true;
            coll.enabled = false;
        }
        foreach (BoxCollider2D coll in current_prefab.GetComponents<BoxCollider2D>())
        {
            coll.isTrigger = true;
            coll.enabled = false;
        }
        foreach (BoxCollider2D coll in current_prefab.GetComponentsInChildren<BoxCollider2D>())
        {
            coll.isTrigger = true;
            coll.enabled = false;
        }
        foreach (Rigidbody2D coll in current_prefab.GetComponentsInChildren<Rigidbody2D>())
        {
            coll.bodyType = RigidbodyType2D.Static;
        }
        boundMode = props.is_bound;
        surfaceMode = props.is_surface;
        fiberMode = props.is_fiber;
        //should use a prefab property about closing availability
        clockwise.SetActive(((props.name.Contains("Membrane")|| props.name.Contains("Capsid"))));
        if (sr)
        {
            if (surfaceMode)
            {
                sr.sharedMaterial.color = new Color(0.4f, 0.4f, 0.4f);
            }
            else {
                sr.sharedMaterial.color = prefab_materials[props.name].color;// props.Default_Sprite_Color;//doest his overwitte the color specified by user ?
            }
        }

        if (fiberMode)
        {
            foreach (var c in current_prefab.GetComponents<CircleCollider2D>())
                Destroy(c);
            Destroy(current_prefab.GetComponent<BoxCollider2D>());
            updateFiberPrefab(myPrefab);
            //activate the push away empty.
            //pushAway.SetActive(true);
            //pushAway.GetComponent<CircleCollider2D>().radius = props.circle_radius * 3.0f;
            drawMode = false;
            fiber_init = false;
            //enable the clockwise icon
            clockwise.GetComponent<SpriteRenderer>().sharedMaterial.color = prefab_materials[props.name].color;// props.Default_Sprite_Color;
            //clockwise is only for membrane
        }
        else {
            //if (!surfaceMode && !boundMode) current_prefab.GetComponent<AddForce>().enabled = false;
            pushAway.SetActive(false);
            if (!surfaceMode && !boundMode) drawMode = true;
            else drawMode = false;
        }
        //if (tools_toggle_image)
        //{
            //tools_toggle_image.sprite = current_prefab.GetComponent<SpriteRenderer>().sprite;
            //tools_toggle_description_image.sprite = current_prefab.GetComponent<SpriteRenderer>().sprite;
            //tools_toggle_image.fillMethod = Image.FillMethod.Radial360;
            //tools_toggle_image.SetNativeSize();
            //ools_toggle_image.transform.localScale = new Vector3(props.encapsulating_radius, props.encapsulating_radius, props.encapsulating_radius)*2/10.0f;
        //}
        if (bucketMode)
            drawMode = false;
        else
        {
            //ToggleContinuous(true);
            ToggleBrush.isOn = true;
        }
        other = null;
        below = null;
        last_other = null;
        //ToggleContinuous(true);//toggle the ui ?
    }

    public void UpdateCountAndLabel(string name, GameObject newO)
    {
        if (proteins_count.ContainsKey(name))
        {
            proteins_count[name]++;
        }
        else {
            proteins_count.Add(name, 1);
        }
        //int percentFilledInt = addToArea(newO);
        //pb.Value = percentFilledInt;
        //float parea = newO.GetComponent<PrefabProperties>().area;
        //float perc = (((float)proteins_count[name] * parea) / (float)current_camera.GetComponent<buildBoundary>().boundryArea);

        if (currentLabel) currentLabel.text = name + ": " + " ( " + proteins_count[name].ToString() + " ) ";
    }

    public void pin_object(GameObject newObject, bool pinn) {
        PrefabProperties p = newObject.GetComponent<PrefabProperties>();
        //newObject.GetComponent<Rigidbody2D>().isKinematic = pinn;
        if (pinn) newObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        else newObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        p.ispined = pinn;
        //p.UpdateOutlinePin(p.ispined);
        if (p.ispined)
            pinned_object.Add(p);
        else
            pinned_object.Remove(p);
    }

    public void restoreOneBond(Vector3 p1, string id1, Vector3 p2, string id2) {
        //FindObjectFromIdString
        var o1 = FindObjectFromIdString(id1);
        var o2 = FindObjectFromIdString(id2);
        if ((o1 != null) && (o2 != null))
        {
            //attach both object
            Rigidbody2D rb1 = o1.GetComponent<Rigidbody2D>();
            Rigidbody2D rb2 = o2.GetComponent<Rigidbody2D>();
            SpringJoint2D jt1 = o1.AddComponent<SpringJoint2D>();
            jt1.autoConfigureConnectedAnchor = false;
            jt1.anchor = attachPos1;
            jt1.enableCollision = true;
            jt1.connectedBody = rb2;
            jt1.connectedAnchor = attachPos2;
            jt1.frequency = 2.5f;
            jt1.autoConfigureDistance = false;
            jt1.distance = 0.8f;
            //Do we need this list now?
            attached.Add(o1);
            attached.Add(o2);
            attachments.Add(jt1);
            o1.GetComponent<PrefabProperties>().UpdateOutline(false);
            o2.GetComponent<PrefabProperties>().UpdateOutline(false);
        }
    }

    public GameObject restoreOneInstance(string name, Vector3 pos, float Zangle, int zorder,
        bool surface = false, bool pinned = false, bool ghost = false)
    {
        GameObject Prefab;
        if (!all_prefab.ContainsKey(name))
        {
            Debug.Log(name + " not found in all_prefab");
            Prefab = Resources.Load("Prefabs/" + name) as GameObject;
            if (Prefab == null)
                Prefab = Build(name);
            Prefab.SetActive(true);
            all_prefab.Add(name, Prefab);
        }
        else
            Prefab = all_prefab[name];
        var Props = Prefab.GetComponent<PrefabProperties>();
        if (Prefab == null) return null;
        checkMaterial(Prefab);
        //Prefab.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.StartAsleep;
        Quaternion quat = Quaternion.AngleAxis(Zangle, Vector3.forward);

        GameObject newObject = Instantiate(Prefab) as GameObject;//, new Vector3(pos.x, pos.y, 0.0f), quat) as GameObject;
        //extract the proper one

        if (!surface)
        {
            /*if (zorder > 0)
            {
                //var Object = newObject.transform.GetChild(zorder - 1).gameObject;
                //Object.transform.parent = root.transform;
                var allCh = newObject.GetComponentsInChildren<Transform>();
                foreach (Transform tr in allCh)
                {
                    Destroy(tr.gameObject);
                }
                Destroy(newObject);
                //newObject = Object;
            }
            else {
                foreach (Transform childTransform in newObject.transform)
                    Destroy(childTransform.gameObject);
                newObject.transform.parent = root.transform;
            }*/
            //Add RigidBody to array for force calculation later.
            
            if (pos.z == 0.25f)
            {
                newObject.layer = LayerMask.NameToLayer("Bottom Layer");
                //newObject.name = newObject.name + " (Bottom)"; 
            }
            else if (pos.z == 0.125f)
            {
                newObject.layer = LayerMask.NameToLayer("Middle Layer");
                //newObject.name = newObject.name + " (Middle)";
            }
            else
            {
                newObject.layer = LayerMask.NameToLayer("Top Layer");
                //newObject.name = newObject.name;// + " (Top)";
            }

            Rigidbody2D rb = newObject.GetComponent<Rigidbody2D>();
            if (rb == null) rb = newObject.AddComponent<Rigidbody2D>();
            rb.angularDrag = 20.0f;
            rb.drag = 20.0f;
            rb.sleepMode = RigidbodySleepMode2D.StartAsleep;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            everything.Add(rb);
            rbCount = rbCount + 1;
        }
        else {
            newObject.transform.parent = root.transform;
            SpriteRenderer asr = newObject.GetComponent<SpriteRenderer>();
            if (asr) {
                if (pos.z ==0.125f) asr.sortingOrder = 0;
                else asr.sortingOrder = 1;
            }
            surface_objects.Add(newObject);
        }
        if (pinned) {
            pin_object(newObject, pinned);
        }
        if (ghost) {
            newObject.GetComponent<Rigidbody2D>().simulated = false;
        }
        newObject.transform.position = pos;
        newObject.transform.rotation = quat;
        newObject.transform.parent = root.transform;
        if (HideInstance) newObject.hideFlags = HideFlags.HideInHierarchy;
        totalNprotein++;
        //remove the children
        return newObject;
    }

    public void InstanceAndCreatePrefab(GameObject instancePrefab, Vector3 objectPos)
    {
        if (rbCount >= MaxRigidBodies) warningPanel.SetActive(true);
        if (rbCount >= MaxRigidBodies) return;
        if (instancePrefab.GetComponent<PrefabProperties>() == null) return;
        var pname = instancePrefab.GetComponent<PrefabProperties>().name;
        Vector3 objectPosMiddle = new Vector3(objectPos.x, objectPos.y, 0.125f);
        Vector3 objectPosBottom = new Vector3(objectPos.x, objectPos.y, 0.250f);
        var layer = LayerMask.NameToLayer("Top Layer");
        var order = 0;
        if (layer_number_options == 1) {
            layer = LayerMask.NameToLayer("Middle Layer");
        }
        if (layer_number_options == 2) {
            layer = LayerMask.NameToLayer("Bottom Layer");
        }
        float Zangle = UnityEngine.Random.value * Mathf.PI * Mathf.Rad2Deg;
        Quaternion quat = Quaternion.AngleAxis(Zangle, Vector3.forward);
        
        GameObject newObject = Instantiate(instancePrefab, objectPos, quat) as GameObject;
        newObject.GetComponent<SpriteRenderer>().sortingOrder = order;
        /* all the follwoing should be in the prefabProperties start*/
        var Props = newObject.GetComponent<PrefabProperties>();
        newObject.transform.name = Props.name;// + " (Top)";
        newObject.transform.parent = root.transform;
        newObject.layer = layer;
        SetupLayer(newObject);
        if (Props.is_Group) {
            newObject.layer = 22;//group
            newObject.transform.name = newObject.GetComponent<PrefabGroup>().name;
            //add all the child to the list

        }
        else {
            if (newObject.GetComponent<Renderer>().sharedMaterial==null)
            {
                newObject.GetComponent<Renderer>().sharedMaterial = prefab_materials[Props.name];
            }
        }
       
        //Puts appropriate collider and settings based on prefab properties.
        Rigidbody2D toprb = newObject.GetComponent<Rigidbody2D>();
        if (toprb == null) 
        {
            toprb = newObject.AddComponent<Rigidbody2D>();
        }
        Props.RB = toprb;
        toprb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;// Continuous;
        toprb.angularDrag = 20.0f;
        toprb.drag = 20.0f;
        toprb.sleepMode = RigidbodySleepMode2D.StartAsleep;
        toprb.interpolation = RigidbodyInterpolation2D.Interpolate;
        everything.Add(toprb);

        rbCount++;
        string name = instancePrefab.name;
        UpdateCountAndLabel(name, newObject);
        if (Props.is_Group) return;
        //This loop creates the other layers from the top layer
        if (layer_number_options!=3) return;
        if (Props.layer_number == 2)
        {
            GameObject twoLayerBottom = Instantiate(newObject, objectPosBottom, quat) as GameObject;
            twoLayerBottom.transform.name = Props.name + " (Bottom)";
            twoLayerBottom.layer = LayerMask.NameToLayer("Bottom Layer");
            twoLayerBottom.transform.parent = root.transform;
            twoLayerBottom.GetComponent<SpriteRenderer>().sortingOrder = -1;
            //Add Rigidbody2D to loop and count.
            Rigidbody2D rb = twoLayerBottom.GetComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;//.Continuous;
            //everything[rbCount] = rb;
            everything.Add(rb);

            rbCount++;
            name = instancePrefab.name;
            UpdateCountAndLabel(name, newObject);
            //bottomRB = rb;
        }
        else
        {
            GameObject threeLayerMiddle = Instantiate(newObject, objectPosMiddle, quat) as GameObject;
            GameObject threeLayerBottom = Instantiate(newObject, objectPosBottom, quat) as GameObject;

            threeLayerMiddle.transform.name = Props.name;// + " (Middle)";
            threeLayerBottom.transform.name = Props.name;// + " (Bottom)";
            threeLayerMiddle.GetComponent<SpriteRenderer>().sortingOrder = -1;
            threeLayerBottom.GetComponent<SpriteRenderer>().sortingOrder = -2;

            threeLayerMiddle.layer = LayerMask.NameToLayer("Middle Layer");
            threeLayerBottom.layer = LayerMask.NameToLayer("Bottom Layer");

            threeLayerMiddle.transform.parent = root.transform;
            threeLayerBottom.transform.parent = root.transform;

            //Add Rigidbody2D to loop and count.
            Rigidbody2D rb = threeLayerMiddle.GetComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;// Continuous;
            //everything[rbCount] = rb;
            everything.Add(rb);

            rbCount++;
            name = instancePrefab.name;
            UpdateCountAndLabel(name, newObject);
            //middleRB = rb;

            Rigidbody2D rb2 = threeLayerBottom.GetComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;//.Continuous;
            //everything[rbCount] = rb;
            everything.Add(rb2);

            rbCount++;
            string name2 = instancePrefab.name;
            UpdateCountAndLabel(name2, newObject);
            //bottomRB = rb2;
        }
    }

    public void OneInstance(Vector3 objectPos)
    {
        if (stop) return;
        float Zangle = UnityEngine.Random.value * Mathf.PI * Mathf.Rad2Deg;
        Quaternion quat = Quaternion.AngleAxis(Zangle, Vector3.forward);
        GameObject newObject = Instantiate(myPrefab, objectPos, quat) as GameObject;

        //Add RigidBody to array for force calculation later.
        Rigidbody2D rb = newObject.GetComponent<Rigidbody2D>();
        //everything[rbCount] = rb;
        everything.Add(rb);

        rbCount = rbCount + 1;

        newObject.transform.parent = root.transform;
        if (HideInstance) newObject.hideFlags = HideFlags.HideInHierarchy;
        totalNprotein++;
        string name = myPrefab.name;// currentLabel.text.Split(":".ToCharArray()[0])[0];
        UpdateCountAndLabel(name, newObject);
        //some of top lay get drawn on top of dna.
        if ((totalNprotein % layer_frequence * 10) == 0)
            newObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
        int Nchild = newObject.transform.childCount;
        if (Nchild == 0) return;
        Transform t = newObject.transform.GetChild(0);
        t.gameObject.SetActive(false);//??
        rb = t.GetComponent<Rigidbody2D>();
        //everything[rbCount] = rb;
        everything.Add(rb);

        rbCount = rbCount + 1;
        t.gameObject.SetActive(false);

        if ((totalNprotein % layer_frequence) == 0)
        {
            t.gameObject.SetActive(true);
            //t.gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
        if (Nchild > 1)
        {
            Transform t2 = newObject.transform.GetChild(1);
            t2.gameObject.SetActive(true);
            rb = t2.GetComponent<Rigidbody2D>();
            //everything[rbCount] = rb;
            everything.Add(rb);

            rbCount = rbCount + 1;
            if ((totalNprotein % layer_frequence * 2) == 0)
            {
                t.gameObject.SetActive(true);
                //t.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }
        List<Transform> trs = new List<Transform>();
        var allChildren = newObject.GetComponentsInChildren<Transform>();
        foreach (Transform tr in allChildren)
        {
            if (tr.gameObject.activeInHierarchy)
            {
                tr.parent = root.transform;
                if (HideInstance)
                    tr.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        /*foreach (Transform child in newObject.transform)
            {
                child.gameObject.SetActive(false);
                if ((totalNprotein % layer_frequence) == 0)
                {
                    child.gameObject.SetActive(true);
                }
            }*/
        //Transform t = newObject.transform.GetChild(1);
        //if (t) t.gameObject.SetActive(false);
    }

    public GameObject oneInstanceFiber(Vector3 start, Vector3 end, GameObject other = null)
    {
        //Debug.Log(myPrefab.GetComponent<PrefabProperties>().sprite_ordered_switch);
        fiber_init = true;
        Vector3 midPoint = (start + end) / 2;
        Vector3 v2 = (end - start).normalized;
        float a = Vector3.Angle(Vector3.right, v2);
        float sign = (v2.y < Vector3.right.y) ? -1.0f : 1.0f;
        return oneInstanceFiberPos(end, a * sign, other);
    }

    public GameObject oneInstanceFiberPos(Vector3 pos, float angle,
        GameObject other = null, GameObject aprefab = null)
    {
        if (aprefab == null) aprefab = myPrefab;
        PrefabProperties props = aprefab.GetComponent<PrefabProperties>();
        if (props.sprite_random_switch)
        {
            props.switchSpriteRandomly();
        }

        if (props.sprite_ordered_switch)
        {
            props.switchSpriteInOrder();
        }
        //This if loop should change the prefab based on an array and switch in PrefabProperties.cs
        if (props.prefab_random_switch)
        {
            int prefab_id = UnityEngine.Random.Range(0, props.prefab_asset.Count);
            aprefab = props.prefab_asset[prefab_id];
            string prefabName = aprefab.GetComponent<PrefabProperties>().name;
            SpriteRenderer sr = aprefab.GetComponent<SpriteRenderer>();
            sr.sharedMaterial = prefab_materials[prefabName];
        }
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        GameObject fiber_quadOb = Instantiate(aprefab, pos, rotation) as GameObject;
        SpriteRenderer sr1 = fiber_quadOb.GetComponent<SpriteRenderer>();
        if (sr1.sharedMaterial == null)sr1.sharedMaterial = prefab_materials[props.name];

        fiber_quadOb.name = "ob" + fiber_nextNameNumber;
        var RigidbodyAnchor = fiber_quadOb.gameObject.GetComponent<Rigidbody2D>();
        //anchors the first instance in the chain.
        if (fiber_nextNameNumber == 0 && !other)
        {
            RigidbodyAnchor.bodyType = RigidbodyType2D.Dynamic;
            fiber_quadOb.GetComponent<PrefabProperties>().ispined = false;
            if (fiber_quadOb.tag != "NA")
            {
                RigidbodyAnchor.bodyType = RigidbodyType2D.Static;
                fiber_quadOb.GetComponent<PrefabProperties>().ispined = true;
                pinned_object.Add(fiber_quadOb.GetComponent<PrefabProperties>());
            }
        }
        RigidbodyAnchor.drag = 20;
        RigidbodyAnchor.angularDrag = 20;
        fiber_quadOb.GetComponent<PrefabProperties>().setCheck(true); //no need to check
        fiber_nextNameNumber++;
        int nchild = fiber_parent.transform.childCount;
        fiber_quadOb.transform.parent = fiber_parent.transform;
        fiber_quadOb.transform.SetSiblingIndex(fibers_instances[fiber_parents.Count-1].Count);
        fibers_instances[fiber_parents.Count-1].Add(fiber_quadOb);
        //int st = 0;
        int indice = 0;
        if (other == null)
        {
            if (fiber_parent.transform.childCount > 1)
                other = fiber_parent.transform.GetChild(nchild - 1).gameObject;
        }
        else {
            indice = other.transform.GetSiblingIndex();
            other.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }

        if ((fiber_hingeJoint) && (other != null))
        {
            HingeJoint2D hinge = other.AddComponent<HingeJoint2D>();
            hinge.enabled = true;

            hinge.autoConfigureConnectedAnchor = false;
            hinge.enableCollision = props.enableCollision;

            CircleCollider2D[] other_allc = other.GetComponents<CircleCollider2D>();
            CircleCollider2D[] current_allc = fiber_quadOb.GetComponents<CircleCollider2D>();
            hinge.anchor = other_allc[1].offset;
            hinge.connectedAnchor = current_allc[0].offset;

            JointAngleLimits2D limits = hinge.limits;
            //should depends on the persistence length
            limits.min = props.hingeJoint_LOWERlimit;
            limits.max = props.hingeJoint_UPPERlimit;
            hinge.limits = limits;
            hinge.useLimits = props.fiber_hingeJoint_limits;
            //st = 1;

            /*
            hinge.useMotor = true;
            JointMotor2D mo = new JointMotor2D();
            mo.motorSpeed = 200.0f;
            hinge.motor = mo;
            */
            
            //before connected the Body realign it
            fiber_quadOb.transform.rotation = other.transform.rotation;
            hinge.connectedBody = fiber_quadOb.GetComponent<Rigidbody2D>();
        }

        for (int i = 0; i < fiber_persistence; i++)
        {
            //we go backward
            if (nchild < i) continue;
            if (nchild - (i + 1) < 0) continue;
            var ch = fiber_parent.transform.GetChild(nchild - (i + 1));
            SpringJoint2D spring = ch.gameObject.AddComponent<SpringJoint2D>();
            spring.connectedBody = fiber_quadOb.GetComponent<Rigidbody2D>();
            spring.enableCollision = aprefab.GetComponent<PrefabProperties>().enableCollision;
            spring.autoConfigureDistance = false;
            CircleCollider2D[] allc = aprefab.GetComponents<CircleCollider2D>();
            spring.distance = fiber_length * (i + 1);// + UnityEngine.Random.Range(0.0f, fiber_length / 10.0f);
            spring.anchor = Vector2.zero;// allc[1].offset;
            spring.connectedAnchor = Vector2.zero;//allc[0].offset;
            spring.frequency = (props.persistence_strength != -1.0f)? props.persistence_strength :  10.0f / ((i + 2) / 2.0f);
            if (i == 0 ) spring.frequency = 50.0f;
            spring.dampingRatio = 0.5f;
        }
        Vector3 objectPos = pos;
        //var layer = LayerMask.NameToLayer("Top Layer");
        SetupLayer(fiber_quadOb);
        if (layer_number_options == 3){
            fiber_quadOb.transform.position = new Vector3(objectPos.x, objectPos.y, 0.0f);
            PrefabProperties pr = fiber_quadOb.GetComponent<PrefabProperties>();
            //if (pr.nucleic_acid_depth)
            //{
            if (!props.draw_background) {
                NucleicAcidDepthLerp(fiber_quadOb);
                fiber_quadOb.transform.position = new Vector3 (objectPos.x, objectPos.y, zLevel);
            }
        }
        fiber_quadOb.transform.rotation = rotation;
        return fiber_quadOb;
    }

    public void closePersistence()
    {
        if (fiber_parent.transform.childCount <= fiber_persistence) return;
        Transform first = fiber_parent.transform.GetChild(0);
        Transform last = fiber_parent.transform.GetChild(fiber_parent.transform.childCount - 1);
        /*
        if ((fiber_distanceJoint) && (fiber_parent.transform.childCount > 1))
        {
            DistanceJoint2D joint = last.gameObject.AddComponent<DistanceJoint2D>();
            joint.enabled = true;
            joint.connectedBody = first.gameObject.GetComponent<Rigidbody2D>();
            joint.autoConfigureConnectedAnchor = false;
            joint.autoConfigureDistance = false;
            joint.distance = fiber_distance;
            joint.enableCollision = myPrefab.GetComponent<PrefabProperties>().enableCollision;
            CircleCollider2D[] allc = myPrefab.GetComponents<CircleCollider2D>();
            joint.anchor = allc[1].offset;
            joint.connectedAnchor = allc[0].offset;
        }
        */
        if ((fiber_hingeJoint) && (fiber_parent.transform.childCount > 1))
        {
            HingeJoint2D hinge = last.gameObject.AddComponent<HingeJoint2D>();
            hinge.enabled = true;
            hinge.autoConfigureConnectedAnchor = false;
            //hinge.autoConfigureDistance = false;
            //hinge.distance = distance;
            hinge.enableCollision = myPrefab.GetComponent<PrefabProperties>().enableCollision;
            CircleCollider2D[] allc = myPrefab.GetComponents<CircleCollider2D>();
            hinge.anchor = allc[1].offset;
            hinge.connectedAnchor = allc[0].offset;

            JointAngleLimits2D limits = hinge.limits;
            limits.min = -15.0f;
            limits.max = 15.0f;
            hinge.limits = limits;
            hinge.useLimits = false;

            first.transform.rotation = last.transform.rotation;
            hinge.connectedBody = first.gameObject.GetComponent<Rigidbody2D>();
        }
        /*
        int start = 0;
        if ((fiber_distanceJoint) && (fiber_parent.transform.childCount > 1))
        {
            start = 1;
        }
        if ((fiber_hingeJoint) && (fiber_parent.transform.childCount > 1))
        {
            start = 1;
        }
        */
        int nchild = fiber_parent.transform.childCount;
        int st = 0;
        int end = nchild - 1;
        bool enableCollision = myPrefab.GetComponent<PrefabProperties>().enableCollision;
        PrefabProperties prop = myPrefab.GetComponent<PrefabProperties>();
        for (int l = 0; l < fiber_persistence; l++)
        {
            int i = 0;//i<l+1
            for (int k = l; k >= 0; k--)
            {
                //if (i < i + 1) continue;
                //Debug.Log((st + i).ToString()+" attached to "+ (end - k).ToString());
                var ch1 = fiber_parent.transform.GetChild(st + i);
                var ch2 = fiber_parent.transform.GetChild(end - k);
                SpringJoint2D spring = ch2.gameObject.AddComponent<SpringJoint2D>();
                spring.connectedBody = ch1.gameObject.GetComponent<Rigidbody2D>();
                ch1.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                ch2.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                spring.enableCollision = enableCollision;
                spring.autoConfigureDistance = false;
                CircleCollider2D[] allc = myPrefab.GetComponents<CircleCollider2D>();
                spring.distance = fiber_length * (l + 1);// + UnityEngine.Random.Range(-fiber_length / 10.0f, fiber_length / 10.0f);
                spring.anchor = Vector2.zero;// allc[1].offset;
                spring.connectedAnchor = Vector2.zero;//allc[0].offset;
                spring.frequency = (prop.persistence_strength != -1.0f) ? prop.persistence_strength : 10.0f / ((l + 2) / 2.0f);
                spring.dampingRatio = 0.5f;
                i++;
            }
        }

        pin_object(first.gameObject, false);
        pin_object(last.gameObject, false);

        if (myPrefab.GetComponent<PrefabProperties>().draw_background)
        {
            DrawMeshContour dmc = fiber_parent.AddComponent<DrawMeshContour>();
            //dmc.matToApply = myPrefab.GetComponent<PrefabProperties>().background_mat;
            Material m = myPrefab.GetComponent<PrefabProperties>().background_mat;
            dmc.matToApply = new Material(m);
            dmc.iname = myPrefab.name;
            dmc.matToApply.color = prefab_materials[myPrefab.name].color;
            fiber_parent.layer = 20; ////separate fiber in compartment layer (20)
            update_texture = true;
        }
        fiber_parent.name = fiber_parent.name + "_Closed";
    }

    public void DestroyInstance(GameObject toDestroy)
    {
        string name = toDestroy.name.Split(" (".ToCharArray())[0];//.Split("".ToCharArray()[0])[0];
        Rigidbody2D player = toDestroy.GetComponent<Rigidbody2D>();
        if (everything.Contains(player))
        {
            everything.Remove(player);
        }
        if (surface_objects.Contains(toDestroy)) {
            surface_objects.Remove(toDestroy);
        }
        if (attached.Contains(toDestroy)){
            var foundJT = new List<SpringJoint2D>();
            var foundIndexes = new List<int>();
            for (int i = 0; i < attached.Count; i++)
            {
                if (attached[i] == toDestroy) {
                    if ((i%2)==0) {
                        foundJT.Add(attachments[i/2]);
                        foundIndexes.Add(i);
                    }
                    else {
                        foundJT.Add(attachments[(i-1)/2]);
                        foundIndexes.Add(i-1);
                    }
                }
            }
            foreach(int indice in foundIndexes.OrderByDescending(v => v))
            {
                attached.RemoveAt(indice+1);
                attached.RemoveAt(indice);
            }
            for (int i = 0; i < foundIndexes.Count; i++)
            {
                attachments.Remove(foundJT[i]);
            }
        }
        for (int i=0;i<fibers_instances.Count;i++)
        { 
            if (fibers_instances[i].Contains(toDestroy)){
                fibers_instances[i].Remove(toDestroy);
            }
        }
        if (!proteins_count.ContainsKey(name))
        {
            PrefabProperties p = toDestroy.GetComponent<PrefabProperties>();
            if (p != null)
            {
                if (toDestroy.GetComponent<PrefabProperties>().is_fiber)
                {
                    foreach (var jts in toDestroy.transform.parent.GetComponentsInChildren<SpringJoint2D>())
                    {
                        if (jts.connectedBody == toDestroy.GetComponent<Rigidbody2D>())
                            DestroyImmediate(jts);
                    }
                    fixPersistenseOnRemove(toDestroy.transform.GetSiblingIndex(), toDestroy.transform.parent.gameObject,
                        toDestroy.GetComponent<PrefabProperties>().persistence_length);
                }
            }
            GameObject.DestroyImmediate(toDestroy);
            return;
        }
        totalNprotein--;
        
        proteins_count[name]--;
        Debug.Log("Deleted protein name: " + name);
        rbCount--;
        float area = toDestroy.GetComponent<PrefabProperties>().area; ;
        //Debug.Log(area.ToString());
        proteinArea -= area;
        float screenArea = current_camera.GetComponent<buildBoundary>().boundryArea;
        int percentFilledInt = (int)((proteinArea / screenArea) * 100);
        //pb.Value = percentFilledInt;
        float perc = (((float)proteins_count[name] * area) / (float)current_camera.GetComponent<buildBoundary>().boundryArea);
        if (proteins_ui_labels[name]) proteins_ui_labels[name].text = name + ": " + perc.ToString("P") + " ( " + proteins_count[name].ToString() + " ) ";
        GameObject.DestroyImmediate(toDestroy);
    }

    public int addToArea(GameObject newObject)
    {
        proteinArea += newObject.GetComponent<PrefabProperties>().area;
        screenArea = current_camera.GetComponent<buildBoundary>().boundryArea;
        percentFilled = (proteinArea / screenArea) * 100;
        int percentFilledInt = (int)percentFilled;
        return percentFilledInt;
    }

    void OnEnable()
    {
        proteins_ui_labels = new Dictionary<string, Text>();
        everything = new List<Rigidbody2D>();//[MaxRigidBodies];
        bounded = new Rigidbody2D[MaxRigidBodies];
        prefab_materials = new Dictionary<string, Material>();
        proteins_count = new Dictionary<string, int>();
        pinned_object = new List<PrefabProperties>();
        attached_object = new List<PrefabProperties>();
        fiber_parents = new List<GameObject>();
        fibers_instances = new List<List<GameObject>>();
        surface_objects = new List<GameObject>();
        all_prefab = new Dictionary<string, GameObject>();
        attached = new List<GameObject>();
        attachments = new List<SpringJoint2D>();
    }

    public void AddUserDirectory(string path) 
    {
        var current_paths = "";
        if (PlayerPrefs.HasKey("UserDirectories")) {
            current_paths = PlayerPrefs.GetString("UserDirectories");
        }
        if (!current_paths.Contains(path)) {
            PdbLoader.DataDirectories.Add(path);
            current_paths+=path+";";
            PlayerPrefs.SetString("UserDirectories",path);
        }
        PlayerPrefs.Save();
        UI_manager.Get.UpdatePanelUserDirectory();
    }

    public void CheckDir(){
        Debug.Log("CheckDir");
        if (PlayerPrefs.HasKey("UserDirectories")) {
            var dirs = PlayerPrefs.GetString("UserDirectories");
            Debug.Log("CheckDir "+dirs);
            string[] alldir = dirs.Split(new string[] { ";" }, StringSplitOptions.None);
            foreach (var d in alldir) {
                Debug.Log(d);
                if (d!="" && !PdbLoader.DataDirectories.Contains(d)) {
                    PdbLoader.DataDirectories.Add(d);
                    Debug.Log(d+" added");
                }
            }
        }
    }

    public void ShowDataDirectory(){
        //show a panel with all the directory with option to remove and add.

    }

    public void ClearCacheDirectory(){
        PdbLoader.DataDirectories.Clear();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetString("version",version); 
        PlayerPrefs.Save();       
    } 

    // Use this for initialization
    void Start() {
        _canvas = FindObjectOfType<Canvas>();
        System.IO.Directory.CreateDirectory(PdbLoader.DefaultDataDirectory);
        System.IO.Directory.CreateDirectory(Path.Combine(PdbLoader.DefaultDataDirectory,"proteins"));
        System.IO.Directory.CreateDirectory(Path.Combine(PdbLoader.DefaultDataDirectory,"images"));
        /*image effect check*/
        /*bloomEffect = current_camera.GetComponent<Bloom>();
        if (bloomEffect == null) bloomEffect = current_camera.gameObject.AddComponent<Bloom>();
        vignetteEffect = current_camera.GetComponent<VignetteAndChromaticAberration>();
        if (vignetteEffect == null) vignetteEffect = current_camera.gameObject.AddComponent<VignetteAndChromaticAberration>();
        blurEffect = current_camera.GetComponent<BlurOptimized>();
        if (blurEffect == null) blurEffect = current_camera.gameObject.AddComponent<BlurOptimized>();
        grayscaleEffect = current_camera.GetComponent<Grayscale>();
        if (grayscaleEffect == null) grayscaleEffect = current_camera.gameObject.AddComponent<Grayscale>();*/
        //apply default value
        GlowActive();
        VignetteActive();
        DepthBlurActive();
        GreyScaleActive();
        //check for any dataDirectories in PlayerPref
        if (PlayerPrefs.HasKey("version")){
            var v = PlayerPrefs.GetString("version");
            if (v != version) {
                //clear 
                Debug.Log("Clear PlayerPrefs");
                PlayerPrefs.DeleteAll();
                PlayerPrefs.SetString("version",version);
            }
        }
        else {
            PlayerPrefs.SetString("version",version);
        }
        version_label.text = "cellPAINT\n"+version;
        CheckDir();
        //PdbLoader.DataDirectories.Add(Application.dataPath + "/../Data/images/");
        UnityEngine.Random.InitState(_Seed);
        selected_prefab = new List<string>();
        selectedobject = new List<GameObject>();
        proteins_count = new Dictionary<string, int>();
        //ui.OnSelect.AddListener(SwitchPrefab);
        dragger = current_camera.GetComponent<DragRigidbody2D>();
        eraser = GetComponent<ErasePrefab>();
        //pb = GameObject.Find("Progressbar").GetComponent<Progressbar>();
        totalNprotein = 0;

        pushAway = transform.GetChild(1).gameObject;
        pushAway.SetActive(false);
        proteins_ui_labels = new Dictionary<string, Text>();
        //everything = new Rigidbody2D[MaxRigidBodies];
        everything = new List<Rigidbody2D>();
        bounded = new Rigidbody2D[MaxRigidBodies];
        //prefab_materials = new Dictionary<string, Material>();
        
        pinned_object = new List<PrefabProperties>();
        attached_object= new List<PrefabProperties>();
        fiber_parents = new List<GameObject>();
        fibers_instances = new List<List<GameObject>>();
        surface_objects = new List<GameObject>();
        sprites_textures.Clear();
        //all_prefab = new Dictionary<string, GameObject>();
        secondCamera.SetReplacementShader(secondShader, "RenderType"); ////Replace shader

        StartCoroutine(DiffuseRBandSurfaceRoutine());
        //Camera.current.nearClipPlane = 5.0f;
        recipeUI.LoadRecipe();
    }

    void drawInstance() {
        //delta is in pixel
        if (Input.GetMouseButton(0)){
            //transform.position
            if (HitMembrane()) return;
        }
        var props = current_prefab.GetComponent<PrefabProperties>();
        delta_threshold = props.circle_radius*props.local_scale*2.0f;//m_ext.x/2.0f;//circle_radius;
        if (props.is_Group) {
            delta_threshold = current_prefab.GetComponent<PrefabGroup>().getRadius();
        }
        float delta_mouse = delta*unit_scale;// Vector3.Distance(prev_mousePos, transform.position);
        bool input_event = (Input.GetMouseButton(0) && delta_mouse > 0) || Input.GetMouseButtonDown(0);
        if (Input.GetMouseButtonDown(0)) endPos = startPos = transform.position;
        //Debug.Log(delta_mouse);
        /*stop = false;
        int nb = myPrefab.GetComponent<PrefabProperties>().number_placed;
        int max = myPrefab.GetComponent<PrefabProperties>().maxNumber;other
        if (max != 0){ 
            if (nb >= max)
            stop = true;
        } */
        float delta_capped = delta;
        if (delta_capped >= 100) delta_capped = 100;
        sprayModulous = Mathf.Round(1/delta * 18);
        if (sprayModulous <= 0) sprayModulous = 0;
        if (sprayModulous >= 18) sprayModulous = 18;
        bool test = true;//Input.GetMouseButtonDown(0) ? true : (fixedCount % sprayModulous == 0);//this is base on number of frame ?
        var cname = props.compartment;//expected compartment
        if ((input_event)&&test)
        {
            endPos = transform.position;
            float lineLength = Vector3.Distance(startPos, endPos);
            test = (lineLength >= delta_threshold) || Input.GetMouseButtonDown(0);
            //do it only if moving, otherwise only one
            if (test)
            {
                for (int i = 0; i < nbInstancePerClick; i++)
                {
                    if (delta < 1f && !Input.GetMouseButtonDown(0)) continue;

                    Vector3 offset = UnityEngine.Random.insideUnitCircle * radiusPerClick;
                    if (nbInstancePerClick == 1) offset = Vector3.zero;
                    //OneInstance(transform.position + offset);
                    if (props.is_Group)
                    {
                        GroupManager.Get.CreateInstanceGroup(myPrefab,  transform.position + offset,Quaternion.identity, root.transform);
                        //if (vr_input) VR_InputManager.Get.VR_Haptic_Right(); //Adam Mac Build
                    }
                    else 
                    {
                        InstanceAndCreatePrefab(myPrefab, transform.position + offset);
                    }
                    input_event = (Input.GetMouseButton(0) && delta_mouse > 0) || Input.GetMouseButtonDown(0);
                }
                startPos = endPos;
            }
        }
    }

    void align_to_mouse(GameObject ob){
        //Vector2 cur_pos;
        var drag = ob.GetComponent<RectTransform>();
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, Input.mousePosition, _canvas.worldCamera, out cur_pos);
        //drag.position = _canvas.transform.TransformPoint(cur_pos);       
        CanvasScaler scaler = _canvas.GetComponentInParent<CanvasScaler>();
        //drag.anchoredPosition = new Vector2(Input.mousePosition.x * scaler.referenceResolution.x / Screen.width, Input.mousePosition.y * scaler.referenceResolution.y / Screen.height);
        drag.position = new Vector2( (mousePositionInViewPort.x) * Screen.width  , mousePositionInViewPort.y * Screen.height );
        //drag.position = mousePositionInViewPort;
    }

    void drawInstanceSurface() {
        //float delta = Vector3.Distance(prev_mousePos, transform.position);
        var props = current_prefab.GetComponent<PrefabProperties>();
        delta_threshold = props.circle_radius*props.local_scale*2.0f;//circle_radius;
        if (props.is_Group) {
            delta_threshold = current_prefab.GetComponent<PrefabGroup>().getRadius();
        }
        float delta_mouse = delta*unit_scale;//
        bool input_event = (Input.GetMouseButton(0) && delta_mouse > delta_threshold) || Input.GetMouseButtonDown(0);
        if (Input.GetMouseButtonDown(0)) endPos = startPos = transform.position;
        float delta_capped = delta;
        if (delta_capped >= 100) delta_capped = 100;
        sprayModulous = Mathf.Round(1 / delta * 18);
        if (sprayModulous <= 0) sprayModulous = 0;
        if (sprayModulous >= 18) sprayModulous = 18;
        bool test = Input.GetMouseButtonDown(0) ? true : (fixedCount % sprayModulous == 0);
        if ((input_event) && test)
        {
            endPos = transform.position;
            float lineLength = Vector3.Distance(startPos, endPos);
            mouseDown = true;
            if (otherSurf)
            {
                test = (lineLength >= delta_threshold) || Input.GetMouseButtonDown(0);
                //do it only if moving, otherwise only one
                if (test)
                {
                    if (props.prefab_random_switch)
                    {
                        int prefab_id = UnityEngine.Random.Range(0, props.prefab_asset.Count);
                        var aprefab = props.prefab_asset[prefab_id];
                        string prefabName = aprefab.GetComponent<PrefabProperties>().name;
                        SpriteRenderer sr = aprefab.GetComponent<SpriteRenderer>();
                        sr.sharedMaterial = prefab_materials[prefabName];
                        myPrefab = aprefab;
                        Debug.Log("completed Surface switch!");
                    }
                    
                    GameObject newObject = Instantiate(myPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    totalNprotein++;
                    int percentFilledInt = addToArea(newObject);
                    //pb.Value = percentFilledInt;
                    SpriteRenderer asr = newObject.GetComponent<SpriteRenderer>();
                    if (asr) {
                        string prefabName = newObject.GetComponent<PrefabProperties>().name;
                        asr.sortingOrder = 1;
                        if (asr.sharedMaterial==null)  asr.sharedMaterial = prefab_materials[prefabName];
                    }
                    Vector3 objectPos = current_prefab.transform.position;
                    Vector3 objectPosMiddle = new Vector3(objectPos.x, objectPos.y, 0.125f);
                    Vector3 objectPosBottom = new Vector3(objectPos.x, objectPos.y, 0.250f);
                    //var layer = LayerMask.NameToLayer("Top Layer");
                    if (layer_number_options == 1) {
                        objectPos = objectPosMiddle;
                        //layer = LayerMask.NameToLayer("Middle Layer");
                    }
                    if (layer_number_options == 2) {
                        objectPos = objectPosBottom;
                        //layer = LayerMask.NameToLayer("Bottom Layer");
                    }

                    newObject.transform.rotation = current_prefab.transform.rotation;
                    newObject.transform.position = objectPos;// new Vector3(current_prefab.transform.position.x, current_prefab.transform.position.y, 0.0f) + newpos;
                    newObject.transform.parent = root.transform;
                    if (HideInstance) newObject.hideFlags = HideFlags.HideInHierarchy;

                    Rigidbody2D rb = newObject.GetComponent<Rigidbody2D>();
                    if (rb==null)
                        rb = newObject.AddComponent<Rigidbody2D>();
                    rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
                    rb.angularDrag = 20.0f;
                    rb.drag = 20.0f;
                    //rb.sleepMode = RigidbodySleepMode2D.StartAsleep;
                    rb.interpolation = RigidbodyInterpolation2D.Interpolate;

                    //cant put them in everything otherwise need to change the save/load function
                    //everything[rbCount] = rb;
                    //rbCount++;

                    UpdateCountAndLabel(props.name, newObject);
                    surface_objects.Add(newObject);

                    if (layer_number_options == 3)//(layer_number_draw)//props.surface_secondLayer)
                    {
                            // create the same instance a layer bellow.
                            GameObject newObject2 = Instantiate(myPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                            totalNprotein++;
                            //int percentFilledInt = addToArea(newObject2);
                            newObject2.layer = 21; //transmb2
                            SpriteRenderer asr2 = newObject2.GetComponent<SpriteRenderer>();
                            if (asr2) {
                                string prefabName = newObject2.GetComponent<PrefabProperties>().name;
                                asr2.sortingOrder = 0;
                                if (asr2.sharedMaterial==null)  asr2.sharedMaterial = prefab_materials[prefabName];
                            }
                            //newObject2.GetComponent<SpriteRenderer>().sortingOrder = 0;
                            //newObject2.transform.GetChild(1).gameObject.layer = 15;//onlymembrane
                            newObject2.transform.rotation = current_prefab.transform.rotation;
                            newObject2.transform.position = new Vector3 (current_prefab.transform.position.x, current_prefab.transform.position.y, 0.125f);
                            newObject2.transform.parent = root.transform;
                            if (HideInstance) newObject2.hideFlags = HideFlags.HideInHierarchy;
                            Rigidbody2D rb2 = newObject2.GetComponent<Rigidbody2D>();
                            if (rb2==null) rb2 = newObject2.AddComponent<Rigidbody2D>();
                            rb2.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
                            rb2.interpolation = RigidbodyInterpolation2D.Interpolate;
                            rb2.angularDrag = 20.0f;
                            rb2.drag = 20.0f;
                            UpdateCountAndLabel(props.name, newObject2);
                            surface_objects.Add(newObject2);
                    }
                    startPos = endPos;
                }
            }
            else {
                if (Input.GetMouseButtonDown(0))
                {
                    message_panel.SetActive(true);
                    align_to_mouse(message_panel);
                    message_panel.GetComponentInChildren<Text>().text = "Membrane protein can only be drawn on top of a membrane. Draw a membrane first.";
                }
            }
        }
    }

    void SetupLayer(GameObject o){
        var p = o.transform.position;
        var sr = o.GetComponent<SpriteRenderer>();
        switch (layer_number_options) {
            case 0:
                o.transform.position = new Vector3(p.x, p.y, 0.0f);
                sr.sortingOrder = 0;
                break;
            case 1:
                o.transform.position = new Vector3(p.x, p.y, 0.125f);
                sr.sortingOrder = -1;
                break;
            case 2:
                o.transform.position = new Vector3(p.x, p.y, 0.250f);
                sr.sortingOrder = -2;
                break;
            case 3://all layer
                o.transform.position = new Vector3(p.x, p.y, 0.0f);
                sr.sortingOrder = 0;
                break;
            default:
                o.transform.position = new Vector3(p.x, p.y, 0.0f);
                sr.sortingOrder = 0; 
                break;                           
        }
    }

    void drawInstanceBoundToFiber() {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;
            if (otherSurf)
            {
                PrefabProperties props = myPrefab.GetComponent<PrefabProperties>();
                if (props.iterate_bound) {
                    IterateAlongChain(otherSurf.gameObject);
                }
                else {
                    GameObject newObject = Instantiate(myPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    totalNprotein++;
                    UpdateCountAndLabel(myPrefab.name, newObject);
                    newObject.transform.position = current_prefab.transform.position;
                    newObject.transform.rotation = current_prefab.transform.rotation;
                    newObject.transform.parent = root.transform;
                    SetupLayer(newObject);
                    if (HideInstance) newObject.hideFlags = HideFlags.HideInHierarchy;
                    //attach to other
                    AttachToPartner(newObject, otherSurf.gameObject);
                    //newObject.GetComponent<PrefabProperties>().AttachToPartner(otherSurf.gameObject);
                }
            }
        }
    }

    public void AttachToPartner(GameObject instance_to_attach, GameObject partner)
    {
        Debug.Log("AttachToPartner");
        PrefabProperties props = instance_to_attach.GetComponent<PrefabProperties>();
        if (!props.is_bound) return;
        bool is_fiber = partner.GetComponent<PrefabProperties>().is_fiber;
        Debug.Log("partner fiber ?"+is_fiber.ToString());
        AttachToPartnerSimple(instance_to_attach, partner);
        //if (is_fiber)
        //    AttachToPartnerFiber(instance_to_attach, partner);
        //else 
        //    AttachToPartnerSimple(instance_to_attach, partner);
    }

    public void AttachToPartnerSimple(GameObject instance_to_attach, GameObject partner)
    {
        FixedJoint2D fixedjt = instance_to_attach.AddComponent<FixedJoint2D>();
        fixedjt.connectedBody = partner.GetComponent<Rigidbody2D>();
        fixedjt.autoConfigureConnectedAnchor = false;
        fixedjt.anchor = Vector2.zero;
        fixedjt.connectedAnchor = Vector2.zero;
        fixedjt.enableCollision = false;
    }

    public void AttachToPartnerFiber(GameObject instance_to_attach, GameObject partner)
    {
        //get the partner parent which is the chain
        PrefabProperties props = instance_to_attach.GetComponent<PrefabProperties>();
        GameObject chain_parent = partner.transform.parent.gameObject;
        int chain_index = partner.transform.GetSiblingIndex();

        /*if (props.iterate_bound)
        {
            //shouldnt get there anyway
            return;// IterateAlongChain(instance_to_attach,partner, chain_parent, chain_index);
        }*/

        //find the closet chain index number to the gap length of the prefab.
        int searchScope = chain_index + props.gap_between_bound;
        if (props.gap_between_bound == 0 && !props.iterate_bound)
        {
            chain_index = IterateSingleAttachment(chain_index, chain_parent, searchScope, props.gap_between_bound);
            GameObject test = chain_parent.transform.GetChild(chain_index - 1).gameObject;
            Rigidbody2D testRigidbody = test.GetComponent<HingeJoint2D>().connectedBody;
            string testName = testRigidbody.transform.name;
            Debug.Log(testName + " AttachToPartnerFiber");

            if (testName == "Nucleocapsid")
            {
                searchScope = searchScope + props.gap_between_bound;
                Debug.Log(searchScope);
                chain_index = IterateSingleAttachment(chain_index, chain_parent, searchScope, props.gap_between_bound);
            }
            ConnectToPartnerFiber(instance_to_attach, partner, chain_parent, chain_index);
        }
    }

    public int IterateSingleAttachment(int chain_index, GameObject chain_parent, int searchScope, int gap_between_bound)
    {
        if (chain_index % gap_between_bound == 0) return chain_index;
        if (gap_between_bound != 0) return chain_index;

        for (int i = chain_index; i < searchScope; i++)
        {
            if (i % gap_between_bound == 0)
            {
                chain_index = i;
            }
        }
        return chain_index;
    }

    public void IterateAlongChain(GameObject partner)
    {
        PrefabProperties props = myPrefab.GetComponent<PrefabProperties>();
        GameObject chain_parent = partner.transform.parent.gameObject;
        int childcount = chain_parent.transform.childCount;
        int chain_index = 0;
        Debug.Log((childcount / props.gap_between_bound).ToString());
        for (int i = 0; i < (childcount / props.gap_between_bound); i++)
        {
            chain_index = (i * props.gap_between_bound) + count_removed;
            if (chain_index >= chain_parent.transform.childCount) continue;
            GameObject newObject = Instantiate(myPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            totalNprotein++;
            UpdateCountAndLabel(myPrefab.name, newObject);
            if (HideInstance) newObject.hideFlags = HideFlags.HideInHierarchy;
            GameObject apartner = chain_parent.transform.GetChild(chain_index).gameObject;
            newObject.transform.position = apartner.transform.position;
            newObject.transform.rotation = apartner.transform.rotation;
            newObject.transform.parent = root.transform;
            ConnectToPartnerFiber(newObject, partner, chain_parent, chain_index);
        }
    }

    public void alignFiber(PrefabProperties props, GameObject start, GameObject end)
    {
        Vector3 entry_to_align = transform.TransformDirection(props.entry);
        Vector3 exit_to_align = transform.TransformDirection(props.exit);
        //start align to entry
        //Vector3 other_orientation = start.transform.TransformDirection(Vector3.right);

        if (start)
        {
            Quaternion q = Quaternion.FromToRotation(Vector3.right, entry_to_align);
            start.GetComponent<Rigidbody2D>().rotation = q.eulerAngles.z;
        }
        //start.transform.rotation = q;
        //end align to exit
        //her_orientation = end.transform.TransformDirection(Vector3.right);

        if (end)
        {
            Quaternion q1 = Quaternion.FromToRotation(Vector3.right, exit_to_align);
            end.GetComponent<Rigidbody2D>().rotation = -q1.eulerAngles.z;
        }

        //end.transform.rotation = q1; 
    }

    private void fixPersistenseOnRemove(int elemRemoved, GameObject chain_parent, int persistence_length)
    {
        //clean the persitence spring according the reordering after deleting one element.
        //need to go back ward and remove the overlapping spring
        for (int i = 0; i < persistence_length; i++)
        {
            if ((elemRemoved - i) < 0) continue;
            //remove the joins that goes over the binding ?
            GameObject elem = chain_parent.transform.GetChild(elemRemoved - i).gameObject;
            foreach (SpringJoint2D sjt in elem.GetComponents<SpringJoint2D>())
            {
                if (sjt.connectedBody.transform.GetSiblingIndex() > elemRemoved)
                {
                    DestroyImmediate(sjt);
                }
            }
        }
        /*GameObject start = chain_parent.transform.GetChild(elemRemoved - 1).gameObject;
        GameObject end = chain_parent.transform.GetChild(elemRemoved + 1).gameObject;
        foreach (var jts in start.GetComponentsInChildren<SpringJoint2D>())
        {
            Destroy(jts);
        }*/
    }

    public GameObject AddFiberParent(string name) {
        Debug.Log("load ressource Prefabs/" + name);
        //myPrefab = Resources.Load("Prefabs/" + name) as GameObject;
        if (!all_prefab.ContainsKey(name))
        {
            Debug.Log(name + " not found in all_prefab");
            myPrefab = Resources.Load("Prefabs/" + name) as GameObject;
            if (myPrefab == null)
                myPrefab = Build(name);
            myPrefab.SetActive(true);
            all_prefab.Add(name, myPrefab);
        }
        else
            myPrefab = all_prefab[name];

        if (myPrefab == null)
        {
            Debug.Log(name + " null prefab");
            return null;
        }
        Debug.Log(name + " loaded as "+ myPrefab.name);
        checkMaterial(myPrefab);
        //myPrefab.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.StartAsleep;
        Debug.Log(name + " checked as " + myPrefab.name);
        updateFiberPrefab(myPrefab);
        fiber_persistence = myPrefab.GetComponent<PrefabProperties>().persistence_length;
        fiber_parent = new GameObject();
        fiber_parent.name = name + "_chain_" + fiber_count.ToString();
        fiber_parent.transform.parent = root.transform;
        fiber_count++;
        fiber_parents.Add(fiber_parent);
        fibers_instances.Add(new List<GameObject>());
        fiber_nextNameNumber = 0;
        colorValue = 1.0f;
        count_removed = 0;
        return fiber_parent;
    }

    public void ConnectToPartnerFiber(GameObject instance_to_attach, GameObject partner, GameObject chain_parent, int chain_index)
    {
        Debug.Log("ConnectToPartnerFiber " + chain_index.ToString());
        Rigidbody2D rb = instance_to_attach.GetComponent<Rigidbody2D>();
        PrefabProperties props = partner.GetComponent<PrefabProperties>();
        SpriteRenderer sr = instance_to_attach.GetComponent<SpriteRenderer>();

        //add instance to attatch to rigidbody array for jitter calculation.
        bounded[boundedCount] = rb;
        boundedCount = boundedCount + 1;


        //Get the first collider which are the anchor
        CircleCollider2D collStart = new CircleCollider2D();
        CircleCollider2D collEnd = new CircleCollider2D();

        // If the ingredient has no colliders and it should create new ones.

        GameObject startAttach = new GameObject();
        collStart = startAttach.AddComponent<CircleCollider2D>();
        startAttach.transform.parent = instance_to_attach.transform;
        startAttach.name = "Start Attach";

        collStart.transform.localPosition = new Vector3(0.01f,0,0);
        collStart.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);


        GameObject endAttach = new GameObject();
        collEnd = endAttach.AddComponent<CircleCollider2D>();
        endAttach.transform.parent = instance_to_attach.transform;
        endAttach.name = "End Attach";
        endAttach.AddComponent<CircleCollider2D>();

        collEnd.transform.localPosition = new Vector3(-0.01f, 0, 0);
        collEnd.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);


        CircleCollider2D[] coll_fiber = partner.GetComponents<CircleCollider2D>();

        //Get Neighboors and change the partner gameobject to the modulous=0 positioning.
        partner = chain_parent.transform.GetChild(chain_index).gameObject;
        Debug.Log(chain_index.ToString() + " " + partner.name);
        GameObject start = null;
        GameObject end = null;
        Vector3 ChainZ = Vector3.zero;
        int startLayerOrder = 0;
        Color prefabRGB = Color.white;
        if (chain_index > 0)//first eleme
        {
            start = chain_parent.transform.GetChild(chain_index - 1).gameObject;
            ChainZ = start.transform.position;
            startLayerOrder = start.GetComponent<SpriteRenderer>().sortingOrder;
            prefabRGB = start.GetComponent<SpriteRenderer>().color;
        }
        if (chain_index < chain_parent.transform.childCount - 1)
        {
            end = chain_parent.transform.GetChild(chain_index + 1).gameObject;
            ChainZ = end.transform.position;
            startLayerOrder = end.GetComponent<SpriteRenderer>().sortingOrder;
            prefabRGB = end.GetComponent<SpriteRenderer>().color;
        }
        //we remove the current chain_index +/- size ?
        //should destroy any spring joints connected to it
        float hue, S, V;
        Color.RGBToHSV(prefabRGB, out hue, out S, out V);

        //Set Properties of bound object.
        Color boundRGB = sr.color;
        float hue2, S2, V2;
        Color.RGBToHSV(boundRGB, out hue2, out S2, out V2);
        Color newBoundColor = Color.HSVToRGB(hue2, S2, V);

        sr.color = newBoundColor;
        sr.sortingOrder = startLayerOrder + 1;

        instance_to_attach.transform.position = new Vector3(instance_to_attach.transform.position.x, instance_to_attach.transform.position.y, ChainZ.z);
        float zLevel = ChainZ.z;

        if (zLevel <= 0.083f)
        {
            instance_to_attach.layer = 8;
        }
        else if (zLevel > 0.083f && zLevel <= 0.16f)
        {
            instance_to_attach.layer = 9;
        }
        else
        {
            instance_to_attach.layer = 10;
        }

        foreach (var jts in chain_parent.GetComponentsInChildren<SpringJoint2D>())
        {
            if (jts.connectedBody == partner.GetComponent<Rigidbody2D>())
                Destroy(jts);
        }

        fixPersistenseOnRemove(chain_index, chain_parent, props.persistence_length);

        Destroy(partner);
        count_removed++;
        //reorient start and end to aligne to the binder
        alignFiber(props, start, end);

        if (start)
        {
            HingeJoint2D jt = start.GetComponent<HingeJoint2D>();
            if (!jt)
            {
                jt = start.AddComponent<HingeJoint2D>();
                jt.enableCollision = false;
                jt.autoConfigureConnectedAnchor = false;
                jt.useLimits = true;
            }
            jt.connectedAnchor = collEnd.offset;
            JointAngleLimits2D limits = jt.limits;
            limits.min = -15.0f;
            limits.max = 15.0f;
            jt.limits = limits;
            jt.connectedBody = rb;
            JointMotor2D jtmotor = new JointMotor2D();
            jtmotor.motorSpeed = -1000.0f;
            jtmotor.maxMotorTorque = 1000.0f;
            jt.motor = jtmotor;
            //jt.useMotor = false;
        }
        if (end)
        {
            HingeJoint2D jt1 = instance_to_attach.AddComponent<HingeJoint2D>();
            jt1.autoConfigureConnectedAnchor = false;
            jt1.connectedBody = end.GetComponent<Rigidbody2D>();
            jt1.anchor = collStart.offset;
            jt1.connectedAnchor = coll_fiber[0].offset;
        }
        /*
        JointAngleLimits2D limits1 = jt.limits;
        limits1.min = -5.0f-150.0f;
        limits1.max = 5.0f-90.0f;
        jt1.limits = limits1;
        jtmotor.motorSpeed = -1000.0f;
        jt1.motor = jtmotor;
        //jt1.useMotor = false;
        */

        Vector2 pos = collStart.offset + (collEnd.offset - collStart.offset) / 2.0f;
        for (int i = 0; i < props.persistence_length; i++)
        {
            //attach elem+i to obj
            GameObject elem = chain_parent.transform.GetChild(chain_index + i).gameObject;
            SpringJoint2D spring1 = elem.AddComponent<SpringJoint2D>();
            spring1.enableCollision = false;
            spring1.autoConfigureDistance = false;
            spring1.distance = fiber_length * (i + 1) - 1;
            //spring1.anchor = coll[1].offset;
            spring1.connectedAnchor = pos;
            spring1.connectedBody = rb;
            spring1.frequency = (props.persistence_strength != -1.0f) ? props.persistence_strength : 10.0f / ((i + 2) / 2.0f);
        }
        instance_to_attach.transform.parent = chain_parent.transform;
        instance_to_attach.transform.SetSiblingIndex(chain_index);
        //chain_index += count_removed;
    }

    public GameObject restoreAttachments(string name, Vector3 pos, float Zangle,
                    GameObject chain_elem)
    {
        var Prefab = Resources.Load("Prefabs/" + name) as GameObject;
        if (Prefab == null) return null;
        checkMaterial(Prefab);
        Prefab.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.StartAsleep;
        Quaternion quat = Quaternion.AngleAxis(Zangle, Vector3.forward);
        GameObject instance_to_attach = Instantiate(Prefab) as GameObject;//, new Vector3(pos.x, pos.y, 0.0f), quat) as GameObject;
        instance_to_attach.transform.parent = fiber_parent.transform;
        instance_to_attach.transform.position = pos;
        instance_to_attach.transform.rotation = quat;
        Rigidbody2D rb = instance_to_attach.GetComponent<Rigidbody2D>();
        bounded[boundedCount] = rb;
        boundedCount = boundedCount + 1;

        if (chain_elem == null) return instance_to_attach;
        CircleCollider2D[] coll = instance_to_attach.GetComponents<CircleCollider2D>();
        CircleCollider2D[] coll_fiber = chain_elem.GetComponents<CircleCollider2D>();

        Rigidbody2D chain_rb = chain_elem.GetComponent<Rigidbody2D>();
        PrefabProperties props = chain_elem.GetComponent<PrefabProperties>();
        SpriteRenderer sr = instance_to_attach.GetComponent<SpriteRenderer>();
        SpriteRenderer chain_sr = chain_elem.GetComponent<SpriteRenderer>();

        HingeJoint2D jt = chain_elem.GetComponent<HingeJoint2D>();
        if (!jt)
        {
            jt = chain_elem.AddComponent<HingeJoint2D>();
            jt.enableCollision = false;
            jt.autoConfigureConnectedAnchor = false;
            jt.useLimits = true;
        }
        jt.connectedAnchor = coll[1].offset;
        JointAngleLimits2D limits = jt.limits;
        limits.min = -15.0f;
        limits.max = 15.0f;
        jt.limits = limits;
        jt.connectedBody = rb;
        JointMotor2D jtmotor = new JointMotor2D();
        jtmotor.motorSpeed = -1000.0f;
        jtmotor.maxMotorTorque = 1000.0f;
        jt.motor = jtmotor;

        //change color sprite
        int startLayerOrder = chain_sr.sortingOrder;
        Color prefabRGB = chain_sr.color;
        float hue, S, V;
        Color.RGBToHSV(prefabRGB, out hue, out S, out V);

        //Set Properties of bound object.
        Color boundRGB = sr.color;
        float hue2, S2, V2;
        Color.RGBToHSV(boundRGB, out hue2, out S2, out V2);
        Color newBoundColor = Color.HSVToRGB(hue2, S2, V);

        sr.color = newBoundColor;
        sr.sortingOrder = startLayerOrder + 1;

        //change layer
        float zLevel = instance_to_attach.transform.position.z;
        if (zLevel <= 0.083f)
        {
            instance_to_attach.layer = 8;
        }
        else if (zLevel > 0.083f && zLevel <= 0.16f)
        {
            instance_to_attach.layer = 9;
        }
        else
        {
            instance_to_attach.layer = 10;
        }
        return instance_to_attach;
    }

    public GameObject restoreAttachFiber(Vector3 pos, float Zangle,
                                        GameObject instance_to_attach) {
        PrefabProperties props = myPrefab.GetComponent<PrefabProperties>();
        Quaternion rotation = Quaternion.AngleAxis(Zangle, Vector3.forward);
        GameObject fiber_quadOb = Instantiate(myPrefab, pos, rotation) as GameObject;
        fiber_quadOb.name = "ob" + fiber_nextNameNumber;
        fiber_quadOb.transform.parent = fiber_parent.transform;
        CircleCollider2D[] coll = instance_to_attach.GetComponents<CircleCollider2D>();
        CircleCollider2D[] coll_fiber = fiber_quadOb.GetComponents<CircleCollider2D>();
        HingeJoint2D jt1 = instance_to_attach.AddComponent<HingeJoint2D>();
        jt1.autoConfigureConnectedAnchor = false;
        jt1.connectedBody = fiber_quadOb.GetComponent<Rigidbody2D>();
        jt1.anchor = coll[0].offset;
        jt1.connectedAnchor = coll_fiber[0].offset;
        fiber_nextNameNumber++;
        return fiber_quadOb;
    }

    void linkFiberObjects(Transform a, Transform b) {
        if (fiber_distanceJoint)
        {
            DistanceJoint2D joint = a.gameObject.AddComponent<DistanceJoint2D>();
            joint.enabled = true;
            joint.connectedBody = b.gameObject.GetComponent<Rigidbody2D>();
            joint.autoConfigureConnectedAnchor = false;
            joint.autoConfigureDistance = false;
            joint.distance = fiber_distance;
            joint.enableCollision = myPrefab.GetComponent<PrefabProperties>().enableCollision;
            CircleCollider2D[] allc = myPrefab.GetComponents<CircleCollider2D>();
            joint.anchor = allc[1].offset;
            joint.connectedAnchor = allc[0].offset;
        }
        if (fiber_hingeJoint)
        {
            HingeJoint2D hinge = a.gameObject.AddComponent<HingeJoint2D>();
            hinge.enabled = true;
            hinge.connectedBody = b.gameObject.GetComponent<Rigidbody2D>();
            hinge.autoConfigureConnectedAnchor = false;
            //hinge.autoConfigureDistance = false;
            //hinge.distance = distance;
            hinge.enableCollision = myPrefab.GetComponent<PrefabProperties>().enableCollision;
            CircleCollider2D[] allc = myPrefab.GetComponents<CircleCollider2D>();
            hinge.anchor = allc[1].offset;
            hinge.connectedAnchor = allc[0].offset;

            JointAngleLimits2D limits = hinge.limits;
            limits.min = -15.0f;
            limits.max = 15.0f;
            hinge.limits = limits;
            hinge.useLimits = false;
        }
        //closePersistence();
        b.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
    }

    bool HitMembrane(){
        RaycastHit2D hit = raycast();
        if (hit) {
            if (//hit.collider.gameObject.layer == 13 || //DNA
                hit.collider.gameObject.layer == 11 || //Membrane
                hit.collider.gameObject.layer == 20 || //compartment
                hit.collider.gameObject.layer == 24)   //membraneBot
                {
                    //Debug.Log("skip "+hit.collider.gameObject.layer.ToString());
                    return true;
                }
            else {
                return false;
                //Debug.Log("didnt skip  "+hit.collider.gameObject.layer.ToString());
            }
        } else {
                //Debug.Log("no HIT");
                return false;
        }
    }

    void drawInstanceFiber() {
        //did we reach maxNb or maxLength
        bool stop = false;
        if (Input.GetMouseButton(0)){
            //transform.position
            if (HitMembrane()) {
                //if self pass below or above ?
                return;
            }
        }
        var props = myPrefab.GetComponent<PrefabProperties>();
        if (fiber_init)
        {
            int nb = fiber_parent.transform.childCount;
            //var props = myPrefab.GetComponent<PrefabProperties>();
            float L = props.fiber_length * nb;
            int max = props.maxNumber;
            float maxL = props.maxLength;
            if (max != 0)
                if (nb >= max)
                    stop = true;
            if (maxL != 0.0f)
                if (L >= maxL)
                    stop = true;
        }

        if (Input.GetMouseButtonDown(0))//on click
        {
            if (mask_ui) return;
            if (fiber_init == false)
            {
                fiber_init_compartment = GetCurrentCompartmentBelow(mousePositionInViewPort.x,mousePositionInViewPort.y);
                //what the compartment below mouse               
                fiber_init = true;
                //check if close to another chain start/end 
                //Debug.Log("attach to "+fiber_attachto.ToString());
                if ((fiber_attachto != null) && (!fiber_attachto.gameObject.transform.parent.name.Contains(myPrefab.name))) { fiber_attachto = null; }
                if (fiber_attachto != null)
                {
                    //make sure it is the same type!
                    fiber_parent = fiber_attachto.transform.parent.gameObject;
                    fiber_attachto.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    endPos = transform.position;
                    startPos = fiber_attachto.transform.position;
                    var fiber = oneInstanceFiber(startPos, endPos, fiber_attachto);
                    startPos = endPos;
                    fiber_attachto = null;
                }
                else {
                    //mousePos = cam.ScreenToWorldPoint(mousePosx);
                    endPos = startPos = transform.position;
                    fiber_parent = new GameObject();
                    if (props.fiber_Middle)
                    {
                        fiber_parent.transform.position = new Vector3 (fiber_parent.transform.position.x,fiber_parent.transform.position.y,0.125f);
                    }
                    else if (props.fiber_Bottom)
                    {
                        fiber_parent.transform.position = new Vector3 (fiber_parent.transform.position.x,fiber_parent.transform.position.y,0.25f);
                    }
                    else 
                    {
                        fiber_parent.transform.position = new Vector3 (fiber_parent.transform.position.x,fiber_parent.transform.position.y,0.0f);
                    }

                    fiber_parent.name = myPrefab.name + "_chain_" + fiber_count.ToString();
                    fiber_parent.transform.parent = root.transform;
                    fiber_count++;
                    if ((startPos == null) || (startPos.magnitude == 0.0f))
                    {
                        endPos = startPos = mousePos;
                    }
                    fiber_parent.tag = "MembraneChain";
                    fiber_parents.Add(fiber_parent);
                    fibers_instances.Add(new List<GameObject>());
                    count_removed = 0;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0)) //release click, finish the fiber, and close it 
        {
            fiber_compartment = fiber_init_compartment = "";
            fiber_lines.enabled = false;
            fiber_attachto = null;
            count_removed = 0;
            zLevel = 0.00f;
            lerp_time = 0.0f;
            colorValue = 1.0f;
            if (fiber_init == false) return;
            fiber_init = false;
            if (fiber_parent.transform.childCount == 0) return;
            Transform first = fiber_parent.transform.GetChild(0);
            Transform last = fiber_parent.transform.GetChild(fiber_parent.transform.childCount - 1);

            fiber_nextNameNumber = 0; //Reset the chain number after Button up.

            float D = Vector3.Distance(first.position, transform.position);// last.position);
            //fiber_current_distance = D;
            if (D < fiber_closing_distance && props.closing)
            {
                //Debug.Log(first.name);
                //Debug.Log(last.name);
                closePersistence();
            }
            else {
                if (last.tag == "NA") return;
                var RigidbodyAnchor = last.gameObject.GetComponent<Rigidbody2D>();
                RigidbodyAnchor.bodyType = RigidbodyType2D.Static;
                last.gameObject.GetComponent<PrefabProperties>().ispined = true;
                pinned_object.Add(last.gameObject.GetComponent<PrefabProperties>());
            }
            fiber_init = false;
            stop = false;
            startPos = Vector3.zero;
            endPos = Vector3.zero;

            if (myPrefab.GetComponent<PrefabProperties>().light_fiber)
            {
                foreach (Transform child in fiber_parent.transform)
                {
                    Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
                    rb.mass = 5.0f;
                    rb.drag = 1.0f;
                    rb.angularDrag = 0.05f;
                }
            }
            return;
        }
        else if (Input.GetMouseButton(0))
        {   
            if (mask_ui) return;
            if (fiber_init == false) return;
            if (stop) return;
            fiber_compartment = GetCurrentCompartmentBelow(mousePositionInViewPort.x,mousePositionInViewPort.y);
            if (fiber_compartment != fiber_init_compartment) return;
            endPos = transform.position;
            if (props.closing) drawFiberToClose();
            float lineLength = Vector3.Distance(startPos, endPos);
            //check if passing a membrane
            if (lineLength >= fiber_length)
            {
                Vector3 cStart = startPos;
                Vector3 cEnd = cStart + (endPos - startPos).normalized * fiber_length;
                int n = Mathf.RoundToInt(lineLength / fiber_length);//Mathf.Min(2,Mathf.RoundToInt(lineLength / fiber_length));//Mathf.RoundToInt(lineLength / fiber_length);
                for (int i = 0; i < n; i++)
                {
                    var spx = current_camera.WorldToViewportPoint(cEnd);
                    fiber_compartment = GetCurrentCompartmentBelow(spx.x,spx.y);
                    if (fiber_compartment != fiber_init_compartment) continue;
                    //check for self crossing?
                    RaycastHit2D hit = raycast_from(cStart);
                    if (hit && hit.collider.gameObject.transform.parent == fiber_parent && hit.collider.gameObject.tag == "membrane") continue;
                    var fiber = oneInstanceFiber(cStart, cEnd);
                    cStart = cEnd;
                    cEnd = cStart + (endPos - startPos).normalized * fiber_length;
                }
                startPos = endPos;
            }
        }
    }

    void drawFiberToClose() {// Transform current, GameObject fiber_parent) {
        //first test current fiber_parents
        //test against pther parents chains
        if (fiber_parent.transform.childCount < 3) return;
        Transform first = fiber_parent.transform.GetChild(0);
        Transform last = fiber_parent.transform.GetChild(fiber_parent.transform.childCount - 1);
        float D = Vector3.Distance(first.position, transform.position);
        //fiber_current_distance = D;
        if (D < fiber_closing_distance)
        { //show something{
            fiber_lines.enabled = true;
            DrawLineFiberClose(first.transform.position, last.transform.position);//
        }
        else {
            fiber_lines.enabled = false;
        }
    }

    bool checkFiberDistance(Transform first, Transform last) {
        float D = Vector3.Distance(first.position, transform.position);// last.position);
        return D < fiber_closing_distance;
    }

    Texture2D GetRenderTexturePixels(RenderTexture rt)
    {
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, true);
        Debug.Log(tex.width.ToString() + "," + tex.height.ToString());
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
        RenderTexture.active = currentActiveRT;
        tex.Apply(true);
        return tex;
    }

    double[] haltonSequence(int n, int p1, int p2, int start)
    {
        float p, u, v, ip;
        int k, kk, pos, a;
        double[] result = new double[n * 2];
        int kstart = start;
        //kstart = UnityEngine.Random.Range(1, 5000);
        for (k = kstart, pos = 0; k < (n + kstart); k++)
        {
            u = 0;
            ip = (float)1.0 / p1;                           // recipical of p1
            for (p = ip, kk = k; kk != 0; p *= ip, kk /= p1) // kk = (int)(kk/p1)
            {
                if ((a = kk % p1) != 0)
                {
                    u += a * p;
                }
            }
            v = 0;
            ip = (float)1.0 / p2;                           // recipical of p2
            for (p = ip, kk = k; kk != 0; p *= ip, kk /= p2)  // kk = (int)(kk/p2)
            {
                if ((a = kk % p2) != 0)
                {
                    v += a * p;
                }
            }

            result[pos++] = u;
            result[pos++] = v;
        }
        return result;
    }

    string GetCurrentCompartmentBelow(float x, float y){
        if (update_texture || (compartment_texture == null)) {
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = secondRenderTexture;
            compartment_texture = new Texture2D(secondRenderTexture.width, secondRenderTexture.height, TextureFormat.ARGB32, true);
            compartment_texture.ReadPixels(new Rect(0, 0, compartment_texture.width, compartment_texture.height), 0, 0, false);
            RenderTexture.active = currentActiveRT;
            compartment_texture.Apply(true);
            update_texture = false;
        }
        int xLocation = Mathf.RoundToInt(x * compartment_texture.width);
        int yLocation = Mathf.RoundToInt(y * compartment_texture.height);
        Color color = compartment_texture.GetPixel(xLocation, yLocation);//compartment color
        //Debug.Log("GetCurrentCompartmentBelow "+color.r.ToString()+" "+xLocation.ToString()+" "+yLocation.ToString());
        if (color == RenderSettings.fogColor) return "exterior";
        else if (SecondCameraScript.Get.mapping.ContainsKey(color.r))
            return SecondCameraScript.Get.mapping[color.r];
        else {
            return "not_found";
        }
    }

    void fillCompartments()
    {
        float delta = Vector3.Distance(prev_mousePos, transform.position);
        bool input_event = Input.GetMouseButtonDown(0);//(Input.GetMouseButton(0) && delta > 0.005f) || Input.GetMouseButtonDown(0);
        if (input_event)
        {
            //get current compartment
            Texture2D textTure2D = GetRenderTexturePixels(secondRenderTexture);
            int xLocation = Mathf.RoundToInt(mousePositionInViewPort.x * textTure2D.width);
            int yLocation = Mathf.RoundToInt(mousePositionInViewPort.y * textTure2D.height);
            Color color = textTure2D.GetPixel(xLocation, yLocation);//compartment color

            List<GameObject> selected_pref = new List<GameObject>();
            int Ninstance = 0;
            int Nobject = selected_prefab.Count;
            List<int> nb_per_obejct = new List<int>();
            foreach (var name in selected_prefab)
            {
                GameObject prefab = all_prefab.ContainsKey(name)? all_prefab[name]: Resources.Load("Prefabs/" + name) as GameObject;
                if (prefab == null)
                    prefab = Build(name);
                selected_pref.Add(prefab);
                checkMaterial(prefab);
                //prefab.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.StartAsleep;
                Ninstance += default_bucket_count;
                nb_per_obejct.Add(default_bucket_count);
            }

            count_used = UnityEngine.Random.Range(1, 50000);
            double[] data = haltonSequence(Ninstance, 2, 3, count_used + _Seed);// rbCount + _Seed);

            for (int i = 0; i < Ninstance; i++)
            {
                //pick a random object
                int obji = UnityEngine.Random.Range(0, Nobject);

                double x = data[i * 2];
                double y = data[i * 2 + 1];

                int hx = Mathf.RoundToInt((float)System.Math.Round(x, 4) * secondRenderTexture.width);
                int hy = Mathf.RoundToInt((float)System.Math.Round(y, 4) * secondRenderTexture.height);
                Color tempColor = textTure2D.GetPixel(hx, hy);

                if (color == tempColor)
                {
                    Vector3 point = new Vector3((float)System.Math.Round(x, 4), (float)System.Math.Round(y, 4));
                    Vector3 wPoint = current_camera.ViewportToWorldPoint(point);
                    wPoint.z = 0.0f;
                    float Zangle = UnityEngine.Random.value * Mathf.PI * Mathf.Rad2Deg;
                    Quaternion quat = Quaternion.AngleAxis(Zangle, Vector3.forward);
                    InstanceAndCreatePrefab(selected_pref[obji], wPoint);

                    //newObject.transform.parent = root.transform;
                    //newObject.SetActive(true);

                    //add new object and children to everything for "brownian" motion calculation.
                    //Rigidbody2D rb = newObject.GetComponent<Rigidbody2D>();
                    //everything[rbCount] = rb;
                    //rbCount = rbCount + 1;

                    /*int Nchild = newObject.transform.childCount;
                    if (Nchild == 0) continue;

                    Transform t = newObject.transform.GetChild(0);
                    t.gameObject.SetActive(false);
                    rb = t.GetComponent<Rigidbody2D>();
                    everything[rbCount] = rb;
                    rbCount = rbCount + 1;
                    t.gameObject.SetActive(true);

                    if (Nchild > 1)
                    {
                        Transform t2 = newObject.transform.GetChild(1);
                        t2.gameObject.SetActive(true);
                        rb = t2.GetComponent<Rigidbody2D>();
                        everything[rbCount] = rb;
                        rbCount = rbCount + 1;
                    }
                    */

                    nb_per_obejct[obji]--;
                    if (nb_per_obejct[obji] <= 0)
                    {
                        Nobject--;
                        selected_pref.RemoveAt(obji);
                    }
                    if (Nobject <= 0)
                        break;
                }
            }
        }
    }

    void highLightGroupType(PrefabGroup pg, bool toggle) {
        var family =
                from o in Manager.Instance.root.transform.GetComponentsInChildren<PrefabGroup>()
                where o.name == pg.name
                select o.transform;
        foreach (var o in family) {
            highLightHierarchy(o,toggle);
        }
    }

    void highLightProteinType(string name, bool toggle) {

        var family =
                from o in Manager.Instance.root.transform.GetComponentsInChildren<Transform>()
                where o.name.Replace("(Clone)","") == name
                select o;
        foreach (var o in family) {
            PrefabProperties p = o.GetComponent<PrefabProperties>();
            if (p && p.ghost_id == -1)
            {
                p.outline_width = current_camera.orthographicSize;
                p.UpdateOutline(toggle);
            }
        }
    }

    public void DestroyHierarchyFamily(string name)
    {
        var family =
                       from o in Manager.Instance.root.transform.GetComponentsInChildren<Transform>()
                       where o!=null && o.name.Replace("(Clone)","") == name
                       select o;
        foreach (var o in family)
        {
            //GameObject.Destroy(o.gameObject);
            DestroyInstance(o.gameObject);
        }
    }

    public void DestroyHierarchyPrefabFamily(string name)
    {
        //need to deal with Ghost
        if (!all_prefab.ContainsKey(name)) return;
        var prefab = all_prefab[name];
        var props = prefab.GetComponent<PrefabProperties>();
        //first remove from group ?
        //and remove from ghost ?
        if (props.is_fiber) {
            var aname = props.name+"_chain_";
            var family =
                       from o in Manager.Instance.root.transform.GetComponentsInChildren<Transform>()
                       where o.name.StartsWith(aname)
                       select o;            
            
            foreach (var o in family)
            {
                //GameObject.Destroy(o.gameObject);
                DestroyHierarchy(o);
            }
        }
        else if (props.is_Group) {
            //select everything in the group
            var family =
                from o in Manager.Instance.root.transform.GetComponentsInChildren<PrefabGroup>()
                where o.name == name
                select o.gameObject;
            foreach (var o in family)
            {
                foreach ( Transform child in o.transform) {
                    if (fiber_parents.Contains(child.gameObject)) {
                        DestroyHierarchy(child);
                    }
                    else 
                    {
                        DestroyInstance(child.gameObject);
                    }
                }
                GameObject.Destroy(o);
            }            
        }
        else {
            DestroyHierarchyFamily(name);
        }
        /*var family =
                       from o in Manager.Instance.root.transform.GetComponentsInChildren<PrefabProperties>()
                       where o.name.Replace("(Clone)","") == name
                       select o;
        foreach (var o in family)
        {
            //GameObject.Destroy(o.gameObject);
            DestroyInstance(o.gameObject);
        }*/
    }

    public void highLightHierarchy(Transform parent, bool toggle) {
        if (parent == null) return;
        foreach (Transform ch in parent.transform)
        {
            if (fiber_parents.Contains(ch.gameObject)) {
                highLightHierarchy(ch,toggle);
            }
            PrefabProperties p = ch.GetComponent<PrefabProperties>();
            if (p)
            {
                if (p.is_Group) {
                    highLightHierarchy(ch,toggle);
                }
                else {
                    p.outline_width = current_camera.orthographicSize;
                    p.UpdateOutline(toggle);
                }
            }
        }
    }

    public void DestroyHierarchy(Transform parent)
    {
        if (!parent) return;
        //DestroyInstance(other);?
        foreach (Transform ch in parent.transform)
        {
            //GameObject.Destroy(ch.gameObject);
            DestroyInstance(ch.gameObject);
        }
        if (fiber_parents.Contains(parent.gameObject))
        {
            int index = fiber_parents.IndexOf(parent.gameObject);
            fibers_instances.RemoveAt(index);
            fiber_parents.Remove(parent.gameObject);
        }
        //GameObject.Destroy(parent.gameObject);
        GameObject.DestroyImmediate(parent.gameObject);
    }

    void HighLightAttached1(){
         if (attach1 != null)
         {
            //highligh
            var pr = attach1.GetComponent<PrefabProperties>();
            if (pr)
            {
                pr.outline_width = current_camera.orthographicSize;
                pr.UpdateOutline(true);
            }
        }       
    }
    public void attachTwoObjects() {
        RaycastHit2D hit = raycast();
        Transform parent = null;
        if (!hit && Input.GetMouseButton(0)) selected_instance = null;
        if ((!hit) || (Input.GetMouseButtonUp(0))) {
            CleanOutline();
            below=null;
            HighLightAttached1();
            return;
        }
        else
        {
            print(hit);
            print(hit.collider);
            print("attach two objects");
            //bool _shift_up = Input.GetKeyUp(KeyCode.LeftShift);
        
            if (other) {
                CleanOutline();
                below=null;
                }
            //print("Found an object - distance: " + hit.distance + " " + hit.collider.gameObject.name);
            other = hit.collider.gameObject;
            Vector3 otherHitPos = hit.point;
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                HighLightAttached1();
                return;
            }

            PrefabProperties p;
            p = other.GetComponent<PrefabProperties>();
            if (p == null)
            {
                p = other.transform.parent.GetComponent<PrefabProperties>();
                other = other.transform.parent.gameObject;
            }
            //Debug.Log("found " + other.name+" "+p.name);
            if (!Input.GetMouseButton(0))
            {
                //mouse over event only
                //CleanOutline(null);
                below = other;
                p = below.GetComponent<PrefabProperties>();
                parent = below.transform.parent;
                if (p)
                {
                    p.outline_width = current_camera.orthographicSize;
                    p.UpdateOutline(true);
                }
                else if (parent != null)
                {
                    //check the parent
                    p = parent.GetComponent<PrefabProperties>();
                    if (p != null)
                    {
                        //p.outline_width = current_camera.orthographicSize;
                        p.outline_width = current_camera.orthographicSize;
                        p.UpdateOutline(true);
                        parent = parent.transform.parent;
                    }
                    else
                    {
                        below = null;
                    }
                }
                else {
                    below = null;
                }
                HighLightAttached1();
            }
            if (!Input.GetMouseButtonDown(0)) return;//!Input.GetMouseButtonDown(0)) return;
            selected_instance = other;
            last_other = other;
            if (other == null) return;
            var Props = other.GetComponent<PrefabProperties>();
            if (Props)
            {
                current_name_below = Props.name;
                last_active_current_name_below = Props.name;
                current_objectparent_below = other.transform.parent.gameObject;
                if (attach1 == null)
                {
                    attach1 = other;
                    attachPos1 = attach1.transform.InverseTransformPoint(otherHitPos);
                    Props.UpdateOutline(true);
                    other = null;
                }
                else if ((attach1 != null) && (attach2 == null))
                {
                    if (attach1 != other)
                    {
                        attach2 = other;
                        attachPos2 = attach2.transform.InverseTransformPoint(otherHitPos);
                        Props.UpdateOutline(true);
                        other = null;
                    }
                }
                if ((attach1 != null) && (attach2 != null))
                {
                    //attach both object
                    Rigidbody2D rb1 = attach1.GetComponent<Rigidbody2D>();
                    Rigidbody2D rb2 = attach2.GetComponent<Rigidbody2D>();
                    SpringJoint2D jt1 = attach1.AddComponent<SpringJoint2D>();
                    jt1.autoConfigureConnectedAnchor = false;
                    jt1.anchor = attachPos1;
                    jt1.enableCollision = collision_attach;//if one is a fiber reesult will not work because of the neighboors
                    jt1.connectedBody = rb2;
                    jt1.connectedAnchor = attachPos2;
                    jt1.frequency = frequency_attach;//2.5
                    jt1.autoConfigureDistance = false;
                    jt1.distance = distance_attach;

                    //Do we need this list now?
                    attached.Add(attach1);
                    attached.Add(attach2);
                    attachments.Add(jt1);
                    //Add objects to the highlight manager's lists.
                   /* HighlightManager.Instance.pinned_to_bonds.Add(jt1);
                    if (!HighlightManager.Instance.pinned_to_objects.Contains(attach1)) HighlightManager.Instance.pinned_to_objects.Add(attach1);
                    if (!HighlightManager.Instance.pinned_to_objects.Contains(attach2)) HighlightManager.Instance.pinned_to_objects.Add(attach2);
                    if (!HighlightManager.Instance.pinnedTo_Highlighted || !HighlightManager.Instance.pinnedTo_toHighlight)
                    {
                        //HighlightManager.Instance.pinnedTo_toHighlight = true;
                        //HighlightManager.Instance.HighlightDecision();
                    }
                    HighlightManager.Instance.UpdatePinToBondPositions_cb();
                    */

                    attach1.GetComponent<PrefabProperties>().UpdateOutline(false);
                    attach2.GetComponent<PrefabProperties>().UpdateOutline(false);
                    attach1 = null;
                    attach2 = null;
                }
            }
        }
    }


    void SelectAndGroupInstance()
    {
        RaycastHit2D hit = raycast();
        bool _shift = Input.GetKey(KeyCode.LeftShift);
        bool _enter = Input.GetKeyDown(KeyCode.Return);
        CleanOutline();
        if ((!hit))
        {
            /*if (other && !GroupManager.Get.current_selections.Contains(other))
            {
                if (group_interact_mode) highLightHierarchy(other.transform, false);
                else {
                    var props = other.GetComponent<PrefabProperties>();
                    if (props ) 
                    {
                        if (props.is_fiber) highLightHierarchy(other.transform.parent, false);
                        else if (props.is_Group&& group_interact_mode) highLightHierarchy(other.transform, false);
                        else {
                            props.outline_width = current_camera.orthographicSize;
                            props.UpdateOutline(true);
                        }
                    }
                }
            }*/
        }
        else {
            OverHighLight(hit.collider.gameObject, false);
            PrefabProperties p = other.GetComponent<PrefabProperties>();
            PrefabGroup pg = other.GetComponentInParent<PrefabGroup>();            
            below = other;
            if (Input.GetMouseButtonDown(0))
            {
                if (_shift) {
                    if (p.is_fiber && p.ghost_id == -1)
                    {
                        if (!GroupManager.Get.current_selections.Contains(other.transform.parent.gameObject))
                            GroupManager.Get.current_selections.Add(other.transform.parent.gameObject);
                    }
                    //else if (pg || p.is_Group){
                    //    if (!GroupManager.Get.current_selections.Contains(other))
                    //            GroupManager.Get.current_selections.Add(other);                           
                    //}
                    else {
                        var family =
                            from o in Manager.Instance.root.transform.GetComponentsInChildren<Transform>()
                            where o.name.Replace("(Clone)","") == p.name && o.GetComponent<Rigidbody2D>().simulated
                            select o.gameObject;
                        foreach (var o in family) {
                            if (!GroupManager.Get.current_selections.Contains(o))
                                GroupManager.Get.current_selections.Add(o);                            
                        }
                    }
                }
                else {
                //add to current_selection and keep highlighted
                    if (p.is_fiber&& p.ghost_id == -1)
                    {
                        if (!GroupManager.Get.current_selections.Contains(other.transform.parent.gameObject))
                            GroupManager.Get.current_selections.Add(other.transform.parent.gameObject);
                    }
                    else if ((pg || p.is_Group)&& group_interact_mode&& p.ghost_id == -1){
                        if (!GroupManager.Get.current_selections.Contains(other))
                                GroupManager.Get.current_selections.Add(other);                    
                    }
                    else
                    {
                        if (!GroupManager.Get.current_selections.Contains(other))
                                GroupManager.Get.current_selections.Add(other);
                    }
                }
                other = null;
                below = null;
            }
        }
        if (_enter)
        {
            CreateAGroup();
        }
    }
    
    public void CreateAGroup(){
        string gname = UI_manager.Get.group_name.text;
        gname = GroupManager.Get.CreateGroup(gname);
        //toggle to drawMode and select the new prefab
        if (gname != "")
        {
            ToggleGroup(false);
            ToggleContinuous(true);
            SwitchPrefabFromName(gname);
        }
    }

    void SelectAndGhostInstance()
    {
        RaycastHit2D hit = raycast();
        bool _shift = Input.GetKey(KeyCode.LeftShift);
        bool _enter = Input.GetKeyDown(KeyCode.Return);

        CleanOutline();
        if (hit) {
            if (other!=null && other.name.StartsWith("ghost_")) {
                //highlight path
                other.GetComponent<Ghost>().ToggleHighlight(false);
            }
            var ob = hit.collider.gameObject;
            OverHighLight(ob, true);
            if (ob.name.StartsWith("ghost_")) {
                //highlight path
                ob.GetComponent<Ghost>().ToggleHighlight(true);
                below = other = ob;
                if (Input.GetMouseButtonDown(0)){
                    //this is dangerous.
                    //need to build mutliple one....
                    GhostManager.Get.RemoveGhost(ob);
                    //ghostInstance(other);//toggle off everything 
                }
                return;
            };
            PrefabProperties p = other.GetComponent<PrefabProperties>();
            PrefabGroup pg = other.GetComponentInParent<PrefabGroup>();
            /*
            PrefabGroup pgc = hit.collider.gameObject.GetComponentInParent<PrefabGroup>();
            var collider = hit.collider.gameObject;
            if (group_interact_mode && pgc != null){
                collider = pgc.gameObject;
            }
            if (other && other != collider) {
                if (!GroupManager.Get.current_selections.Contains(other))
                {
                    if (group_interact_mode) highLightHierarchy(other.transform, false);
                    else {
                        var props = other.GetComponent<PrefabProperties>();
                        if (props ) {
                            if (props.is_fiber) highLightHierarchy(other.transform.parent, false);
                            else if (props.is_Group&& group_interact_mode) highLightHierarchy(other.transform, false);
                            else {
                                props.outline_width = current_camera.orthographicSize;
                                props.UpdateOutline(true);
                            }
                        }
                    }
                }
                if (other.name.StartsWith("ghost_")) {
                //highlight path
                    other.GetComponent<Ghost>().ToggleHighlight(false);
                }

            }
            other = collider;//hit.collider.gameObject;
            if (other.name.StartsWith("ghost_")) {
                //highlight path
                other.GetComponent<Ghost>().ToggleHighlight(true);
                if (Input.GetMouseButtonDown(0)){
                    //this is dangerous.
                    //need to build mutliple one....
                    GhostManager.Get.RemoveGhost(other);
                    //ghostInstance(other);//toggle off everything 
                }
                return;
            };
            
            PrefabProperties p = other.GetComponent<PrefabProperties>();
            PrefabGroup pg = other.GetComponentInParent<PrefabGroup>();
            //Debug.Log("hitting something in the update " + other.name+" "+(pb==null).ToString()+" "+(p==null).ToString());//cube ?
            if (pg == null && p == null)
            {
                if (other.transform.parent)
                {
                    p = other.transform.parent.GetComponent<PrefabProperties>();
                    other = other.transform.parent.gameObject;
                    pg = other.GetComponentInParent<PrefabGroup>();
                }
            }
            if (pg!=null) {
                other = pg.gameObject;
                p = other.GetComponent<PrefabProperties>();
            }
            if (p == null && pg == null) {
                CleanOutline();
                Debug.Log("no PrefabProperties and no PrefabGroup " + other.name);
                return;
            }
            if (p)
            {
                p.UpdateOutline(true);
                if (p.is_fiber) {
                    highLightHierarchy(other.transform.parent, true);
                }
                if (p.is_Group && group_interact_mode) highLightHierarchy(other.transform, true);
                if (_shift) {
                    if (p.is_fiber)
                    {
                        //highlight the all chain parent
                        highLightHierarchy(other.transform.parent,true);
                    }
                    else if (pg || p.is_Group){
                        //hightlight of group of this type
                        highLightGroupType(pg, true);
                    }
                    else {
                        //find everyother object of same type
                        highLightProteinType(p.name,true);
                    }                    
                }
            }
            if (pg && group_interact_mode ){
                highLightHierarchy(pg.transform, true);
            }*/
            below = other;
            if (Input.GetMouseButtonDown(0))
            {
                if (_shift) {
                    if (p && p.is_fiber)
                    {
                        if (!GroupManager.Get.current_selections.Contains(other.transform.parent.gameObject))
                            GroupManager.Get.current_selections.Add(other.transform.parent.gameObject);
                    }
                    else if (pg || (p &&p.is_Group)){
                        var family =
                            from o in Manager.Instance.root.transform.GetComponentsInChildren<PrefabGroup>()
                            where o.name == pg.name
                            select o.gameObject;
                        foreach (var o in family) {
                            if (!GroupManager.Get.current_selections.Contains(o))
                                GroupManager.Get.current_selections.Add(o);                            
                        }                  
                    }
                    else {
                        var family =
                            from o in Manager.Instance.root.transform.GetComponentsInChildren<Transform>()
                            where o.name.Replace("(Clone)","") == p.name && o.GetComponent<Rigidbody2D>().simulated
                            select o.gameObject;
                        foreach (var o in family) {
                            if (!GroupManager.Get.current_selections.Contains(o))
                                GroupManager.Get.current_selections.Add(o);                            
                        }
                    }
                }
                else {
                //add to current_selection and keep highlighted
                    if (p && p.is_fiber)
                    {
                        if (!GroupManager.Get.current_selections.Contains(other.transform.parent.gameObject))
                            GroupManager.Get.current_selections.Add(other.transform.parent.gameObject);
                    }
                    else if ((pg || (p &&p.is_Group))&& group_interact_mode){
                        if (!GroupManager.Get.current_selections.Contains(other))
                                GroupManager.Get.current_selections.Add(other);                    
                    }
                    else
                    {
                        if (!GroupManager.Get.current_selections.Contains(other))
                                GroupManager.Get.current_selections.Add(other);
                    }
                }
                other = null;
                below = null;
            }
        }
        if (_enter)//(vr_input && VR_InputManager.Get.VR_Right_TwoAxis_ClickDown())
        {
            GhostManager.Get.AddGhost(GroupManager.Get.current_selections);
            togglePinOutline(true);
            GroupManager.Get.current_selections.Clear();
            highLightHierarchy(root.transform, false);
            //ghostGselection();
            //togglePinOutline(true);
            //UpdateGhostArea();//first time doesnt work ?
        }
    }
    
    public void UpdateInfo()
    {
        var touse = below;
        if (below == null) {
            touse = last_other;
        };
        if (touse==null) return;
        var p=touse.GetComponent<PrefabProperties>();
        if (p==null) return;
        //current_name_below = p.name;
        //current_objectparent_below = touse.transform.parent.gameObject;
        if (Description_Holder.activeInHierarchy)
        {
            changeDescription(touse, touse.GetComponent<SpriteRenderer>());
        }
        //last_active_current_name_below = current_name_below;
    }

    void OverHighLight(GameObject object_hit, bool group_interact){
        //one function for all hit highlight
        bool _shift = Input.GetKey(KeyCode.LeftShift);
        CleanOutline();
        PrefabGroup pg = object_hit.gameObject.GetComponentInParent<PrefabGroup>();
        if (group_interact && pg != null){
            object_hit = pg.gameObject;
        }    
        if (other && other != object_hit) {
            if (other.name.StartsWith("ghost_")) {
                    other.GetComponent<Ghost>().ToggleHighlight(false);
            }
            var props = other.GetComponent<PrefabProperties>();
            var group = other.GetComponentInParent<PrefabGroup>();
            if (props && props.is_fiber) {
                group = other.transform.parent.gameObject.GetComponentInParent<PrefabGroup>();
            }
            if ( group ) highLightHierarchy(group.transform, false);
            if ( props ) {
                if (props.is_fiber) highLightHierarchy(other.transform.parent, false);
                else if (props.is_Group && group_interact) highLightHierarchy(other.transform, false);
                else {
                    props.outline_width = current_camera.orthographicSize;
                    props.UpdateOutline(false);
                }
            }
        }
        other = object_hit;
        var parent = other.transform.parent;
        PrefabProperties p = other.GetComponent<PrefabProperties>();
        if (pg == null && p == null && parent != null && group_interact)
        {
            p = parent.GetComponent<PrefabProperties>();
            other = parent.gameObject;
            pg = other.GetComponentInParent<PrefabGroup>();
        }
        if (pg!=null && group_interact) {
            other = pg.gameObject;
            p = other.GetComponent<PrefabProperties>();
        }
        if (p == null && pg == null) {
            Debug.Log("no PrefabProperties and no PrefabGroup " + other.name);
            return;
        }
        if (p)
        {
            p.UpdateOutline(true);
            if (p.is_fiber && (group_interact||groupMode)) {
                highLightHierarchy(other.transform.parent, true);
            }
            if (p.is_Group && group_interact) highLightHierarchy(other.transform, true);
            if (_shift) {
                if ((group_interact)&&(pg || p.is_Group)){
                    highLightGroupType(pg, true);
                }
                else if (!groupMode && (pg || p.is_Group)){
                    highLightHierarchy(pg.transform, true);
                }
                else if (p.is_fiber)
                {
                    //highlight the all chain parent
                    highLightHierarchy(other.transform.parent,true);
                }
                else {
                    //find everyother object of same type
                    highLightProteinType(p.name,true);
                }             
            }
        }
        below = other;
    }

    void pindragInstance() {
        //if (pin_collider_mode) return;
        RaycastHit2D hit = raycast();
        //check the shift keyEvent
        //if (Input.GetKeyUp(KeyCode.LeftShift)) CleanOutline();
        bool _shift = Input.GetKey(KeyCode.LeftShift);
        bool _ctrl = Input.GetKey(KeyCode.LeftControl);
        group_interact_mode = Input.GetKey(KeyCode.LeftControl);
        //bool _shift_up = Input.GetKeyUp(KeyCode.LeftShift);
        if (!hit && Input.GetMouseButton(0)) {
            selected_instance = null;

        }
        else if (Input.GetMouseButtonUp(0) && dragMode && (_shift||moveWithMode)) {
            PrefabProperties p = other.GetComponent<PrefabProperties>();
            PrefabGroup pg = other.GetComponentInParent<PrefabGroup>();  
            Transform parent = other.transform.parent;
            if (_shift) {
                if (other.name.StartsWith("ghost_")) {
                    //reparent all the ghost object
                    other.GetComponent<Ghost>().changeParent(root.transform);
                    other.transform.parent = GhostManager.Get.transform;
                }
                else if ((pg || p.is_Group) && group_interact_mode){
                    pg.transform.parent = current_objectparent_below.transform;
                }
                else if (pg || p.is_Group){
                    pg.transform.parent = current_objectparent_below.transform;
                }
                else if (p.is_fiber)
                {
                    parent.transform.parent = current_objectparent_below.transform;
                }
                else {
                    var family =
                        from o in transform.GetComponentsInChildren<Transform>()
                        where o.name.Replace("(Clone)","") == other.name
                        select o;
                    foreach (var o in family) {
                        o.transform.parent = current_objectparent_below.transform;
                    }
                }
            }
            else {
                if  ((pg || p.is_Group) && group_interact_mode) {
                    pg.transform.parent = current_objectparent_below.transform;
                }
                else {
                    other.transform.parent = current_objectparent_below.transform;                                
                }
            }
            moveWithMode = false;
        }
        else
        {
            if ((!moveWithMode) && (!hit)) {
                CleanOutline();
                return;
            }
            //if (!hit.collider.gameObject.GetComponent<Rigidbody2D>())
            //{
            //    CleanOutline();
            //    return;
            //}
            //this assigne the gameobject other
            if (hit && !moveWithMode) {
                OverHighLight(hit.collider.gameObject, false);
                if (dragMode&&_shift){
                    if (hit.collider.gameObject.name.StartsWith("ghost_")) {
                        //highlight path
                        hit.collider.gameObject.GetComponent<Ghost>().ToggleHighlight(true);
                        below = other = hit.collider.gameObject;
                    }
                }
            }

            PrefabProperties p = other.GetComponent<PrefabProperties>();
            PrefabGroup pg = other.GetComponentInParent<PrefabGroup>();  
            Transform parent = other.transform.parent;
            
            selected_instance = other;
            last_other = other;
            var props = other.GetComponent<PrefabProperties>();
            current_properties = props;
            if (props) {
                current_name_below = props.name;
                last_active_current_name_below =props.name;
            }
            if (infoMode)
            {
                if (!Input.GetMouseButtonDown(0)) return;
                if (other == null) return;
                Description_Holder.SetActive(true);
                Description_Holder_HSV.SetActive(true);
                Description_Holder_HSV.GetComponent<ColorPicker>().active = true;
                //Description_Holder.transform.GetChild(0).GetComponent<ColorPicker>().active = true;
                /**/
                if (props == null) other = other.transform.parent.gameObject;
                current_name_below = other.GetComponent<PrefabProperties>().name;
                current_objectparent_below = other.transform.parent.gameObject;

                //Debug.Log("The name of other in SceneManeger is " + other);
                changeDescription(other, other.GetComponent<SpriteRenderer>());
            }
            else if (pinMode)
            {
                if (!Input.GetMouseButtonDown(0)) return;
                if (_shift) {
                    if ((pg || p.is_Group) && group_interact_mode){
                        pinGroupType(pg);
                    }
                    else if (pg || p.is_Group){
                        pinHierarchy(pg.transform);
                    }
                    else if (p.is_fiber)
                    {
                        pinHierarchy(parent);
                    }
                    else {
                        pinProteinType(p.name);
                    }
                }
                else {
                    if  ((pg || p.is_Group) && group_interact_mode) {
                       pinHierarchy(pg.transform);
                    }
                    else pinInstance(other);
                }
                togglePinOutline(true);
            }
            else if (bindMode)
            {
                //select two objects to be attach on click position
            }
            else if (dragMode) { 
                if (!_shift && !_ctrl) 
                {
                    if (Input.GetMouseButtonDown(0)) dragInstance(other, hit.point); 
                }
                else 
                {
                    if (Input.GetMouseButtonDown(0)){
                        moveWithMode = true;
                        //put selection under transform
                        if (other.name.StartsWith("ghost_")) {
                            //translate all ghost holding object
                            other.GetComponent<Ghost>().changeParent(transform);
                            other.transform.parent = transform;
                            //current_objectparent_below = other.transform.parent.gameObject;
                            //other.transform.parent = transform;
                        }
                        else if (_shift) {
                            if ((pg || p.is_Group) && group_interact_mode){
                                current_objectparent_below = pg.transform.parent.gameObject;
                                pg.transform.parent = transform;
                            }
                            else if (pg || p.is_Group){
                                current_objectparent_below = pg.transform.parent.gameObject;
                                pg.transform.parent = transform;
                            }
                            else if (p.is_fiber)
                            {
                                current_objectparent_below = parent.parent.gameObject;
                                parent.transform.parent = transform;
                            }
                            else {
                                current_objectparent_below = other.transform.parent.gameObject;
                                var family =
                                    from o in root.transform.GetComponentsInChildren<Transform>()
                                    where o.name.Replace("(Clone)","") == other.name
                                    select o;
                                foreach (var o in family) {
                                    o.transform.parent = transform;
                                }
                            }
                        }
                        else {
                            if  ((pg || p.is_Group) && group_interact_mode) {
                                current_objectparent_below = pg.transform.parent.gameObject;
                                pg.transform.parent = transform;
                            }
                            else {
                                current_objectparent_below = other.transform.parent.gameObject;
                                other.transform.parent = transform;                                
                            }
                        }
                    }
                }
                togglePinOutline(true); 
            }
            else if (eraseMode)
            {
                if (!Input.GetMouseButtonDown(0)) return;
                if (other == gameObject) return;
                if (props == null) other = other.transform.parent.gameObject;
                //shift code
                if (_shift)
                {
                    if (p.is_fiber)
                    {
                        //highlight the all chain parent
                        DestroyHierarchy(parent);
                    }
                    else {
                        //find everyother object of same type
                        DestroyHierarchyFamily(other.GetComponent<PrefabProperties>().name);
                    }
                }
                else DestroyInstance(other);

            }
        }
    }

    void pinHierarchy(Transform parent){
        foreach (Transform ch in parent.transform)
        {
            if (fiber_parents.Contains(ch.gameObject)){
                pinHierarchy(ch);
            }
            else {
                PrefabProperties p = ch.GetComponent<PrefabProperties>();
                if (p)
                {
                    p.ispined = !p.ispined;
                    p.outline_width = current_camera.orthographicSize;
                    if (p.ispined)
                        {p.RB.bodyType = RigidbodyType2D.Static;
                        pinned_object.Add(p);}
                    else
                        {p.RB.bodyType = RigidbodyType2D.Dynamic;
                        pinned_object.Remove(p);}
                }
            }
        }
    }

    void pinProteinType(string name) {
        var family =
                from o in Manager.Instance.root.transform.GetComponentsInChildren<Transform>()
                where o.name.Replace("(Clone)","") == name
                select o;
        foreach (var o in family) {
            PrefabProperties p = o.GetComponent<PrefabProperties>();
            if (p)
            {
                p.ispined = !p.ispined;
                p.outline_width = current_camera.orthographicSize;
                if (p.ispined)
                    {p.RB.bodyType = RigidbodyType2D.Static;
                    pinned_object.Add(p);}
                else
                    {p.RB.bodyType = RigidbodyType2D.Dynamic;
                    pinned_object.Remove(p);}
            }
        }
    }

    void pinGroupType(PrefabGroup pg) {
        var family =
            from o in Manager.Instance.root.transform.GetComponentsInChildren<PrefabGroup>()
            where o.name == pg.name
            select o.gameObject;
        foreach (var o in family) {
            if (fiber_parents.Contains(o)){
                pinHierarchy(o.transform);
            }
            else {
                PrefabProperties p = o.GetComponent<PrefabProperties>();
                if (p)
                {
                    p.ispined = !p.ispined;
                    p.outline_width = current_camera.orthographicSize;
                    if (p.ispined)
                        {p.RB.bodyType = RigidbodyType2D.Static;
                        pinned_object.Add(p);}
                    else
                        {p.RB.bodyType = RigidbodyType2D.Dynamic;
                        pinned_object.Remove(p);}
                }
            }
        }
    }

    public void pinInstance(GameObject other) {

        if (!below) return;
        //Debug.Log(below.name);
        PrefabProperties p = below.GetComponent<PrefabProperties>();
    
        if (p.ispined == false)
        {
            p.RB.bodyType = RigidbodyType2D.Static;
            //Debug.Log(below + " is pinned");
        }
        else
        {
            p.RB.bodyType = RigidbodyType2D.Dynamic;
            //Debug.Log(below + " is unpinned");
        }


        p.ispined = !p.ispined;
        p.UpdateOutlinePin(p.ispined);
        if (p.ispined)
            pinned_object.Add(p);
        else
            pinned_object.Remove(p);
    }

    void ghostHierarchy(Transform parent){
        foreach (Transform ch in parent.transform)
        {
            PrefabProperties p = ch.GetComponent<PrefabProperties>();
            if (p)
            {
                p.RB.simulated = !p.RB.simulated;
            }
        }
    }

    void ghostProteinType(string name) {
        var family =
                from o in Manager.Instance.root.transform.GetComponentsInChildren<Transform>()
                where o.name.Replace("(Clone)","") == name
                select o;
        foreach (var o in family) {
            PrefabProperties p = o.GetComponent<PrefabProperties>();
            if (p)
            {
                p.RB.simulated = !p.RB.simulated;
            }
        }
    }

    public void ghostInstance(GameObject other) 
    {
        if (other.name == "ghostArea"){
            //unGhost
            for (int i = 0; i < everything.Count; i++)
            {
                Rigidbody2D player = everything[i];
                if (player == null) continue;
                if (!player.simulated) {
                    player.simulated = true;
                }
            }
            for (int i = 0; i < surface_objects.Count; i++)
            {
                Rigidbody2D player = surface_objects[i].GetComponent<Rigidbody2D>();
                if (player == null) continue;
                if (!player.simulated) {
                    player.simulated = true;
                }
            }
            for (int i=0;i< fibers_instances.Count;i++ ){
                for (int j=0;j<fibers_instances[i].Count;j++ ){
                    Rigidbody2D player = fibers_instances[i][j].GetComponent<Rigidbody2D>();
                    if (player == null) continue;
                    if (!player.simulated) {
                        player.simulated = true;
                    }
                }
            }
        }
        else 
        {
            PrefabProperties p = other.GetComponent<PrefabProperties>();
            if (p) p.RB.simulated = !p.RB.simulated;
        }
    }

    public void ghostGselection(){
        foreach (var o in GroupManager.Get.current_selections)
        {
            //test for attachements
            if (fiber_parents.Contains(o))
            {
                ghostHierarchy(o.transform);
            }
            else
            {
                var p = o.GetComponent<PrefabProperties>();
                if (p)
                {
                    if (p.is_Group) {
                        //do allthe group selection
                        ghostHierarchy(o.transform);
                    }
                    else p.RB.simulated = false;
                }
            }
        }
    }

    public void ghostHighlight(bool toggle){
        var family =
                from o in root.transform.GetComponentsInChildren<Rigidbody2D>()
                where (o.simulated == false)
                select o;
        foreach (var o in family) {
            PrefabProperties p = o.GetComponent<PrefabProperties>();
            if (p)
            {
                p.outline_width = current_camera.orthographicSize;
                p.UpdateOutline(toggle);
            }
        }
    }

    /*public void UpdateGhostArea(){
        return;
        //we can grow our canvas and make this region collider.
        //circle or box or polygon ?
        if (ghostArea == null) {
            ghostArea = new GameObject("ghostArea");
        }
        ghostArea.transform.localScale = new Vector3(1,1,1);
        //if we want a mesh 
        Rigidbody2D rb = ghostArea.GetComponent<Rigidbody2D>();
        if (rb == null) rb = ghostArea.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        //update the collider
        //ghostArea.GetComponents<Collider2D>();
        PolygonCollider2D box = ghostArea.GetComponent<PolygonCollider2D>();
        if (box) DestroyImmediate(box);
        box = ghostArea.AddComponent<PolygonCollider2D>();
        //gather list of points
        Bounds b = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        //gather the points
        //List<IntPoint> pts = new List<IntPoint>();
        List<Vector2> pts = new List<Vector2>();
        for (int i = 0; i < everything.Count; i++)
        {
            Rigidbody2D player = everything[i];
            if (player == null) continue;
            if (!player.simulated) {
                //pts.Add(new IntPoint(Mathf.CeilToInt(player.position.x),Mathf.CeilToInt(player.position.y)));
                pts.Add(new Vector2(Mathf.CeilToInt(player.position.x),Mathf.CeilToInt(player.position.y)));
                b.Encapsulate(player.position);
            }
        }
        for (int i = 0; i < surface_objects.Count; i++)
        {
            Rigidbody2D player = surface_objects[i].GetComponent<Rigidbody2D>();
            if (player == null) continue;
            if (!player.simulated) {
                //pts.Add(new IntPoint(Mathf.CeilToInt(player.position.x),Mathf.CeilToInt(player.position.y)));
                pts.Add(new Vector2(Mathf.CeilToInt(player.position.x),Mathf.CeilToInt(player.position.y)));
                b.Encapsulate(player.position);
            }
        }
        for (int i=0;i< fibers_instances.Count;i++ ){
            for (int j=0;j<fibers_instances[i].Count;j++ ){
                Rigidbody2D player = fibers_instances[i][j].GetComponent<Rigidbody2D>();
                if (player == null) continue;
                if (!player.simulated) {
                    //pts.Add(new IntPoint(Mathf.CeilToInt(player.position.x),Mathf.CeilToInt(player.position.y)));
                    pts.Add(new Vector2(Mathf.CeilToInt(player.position.x),Mathf.CeilToInt(player.position.y)));
                    b.Encapsulate(player.position);
                }
            }
        }
        ghostArea.transform.position = b.center;
        if (pts.Count == 0) {
            Debug.Log("didnt found any point");
            box.pathCount = 0;
        }
        else {
            //cluster
            List<int> labels = new List<int>();
            int cluster_count = Helper.ComputeCluster(pts,cluster_radius, ref labels);
            Debug.Log(labels);
            Debug.Log(cluster_count);
            List<List<IntPoint>> cl_points = new List<List<IntPoint>>();
            for (var i=0;i < cluster_count;i++){
                cl_points.Add(new List<IntPoint>());
            }
            for (var i=0;i < labels.Count;i++){
                var l = labels[i];
                cl_points[l].Add(new IntPoint(Mathf.CeilToInt(pts[i].x),Mathf.CeilToInt((pts[i].y))));
            }
            box.pathCount = cluster_count;
            for (var i=0;i < cluster_count;i++){
                List<Vector2> hull = Helper.ComputeConvexHull(cl_points[i],b.center,false);
                box.SetPath(i,hull);
            }
            //box.offset = new Vector2(b.center.x,b.center.y);
        }
        //ghostArea.transform.localScale = new Vector3(1.1f,1.1f,1.1f);
    }*/

    private void DrawLine(Vector3 start, Vector3 end) {
        lines.SetPosition(0, new Vector3(start.x, start.y, -10));
        lines.SetPosition(1, new Vector3(end.x, end.y, -10));
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0.0f), new GradientColorKey(Color.yellow, 0.5f), new GradientColorKey(Color.red, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        lines.colorGradient = gradient;
        //lines.sharedMaterial.color = color;
    }

    private void DrawLineFiberClose(Vector3 start, Vector3 end)
    {

        fiber_lines.SetPosition(0, new Vector3(start.x, start.y, -10));
        fiber_lines.SetPosition(1, new Vector3(end.x, end.y, -10));
        /*
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.mode = GradientMode.Fixed;
        
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.25f), new GradientColorKey(Color.blue, 0.5f), new GradientColorKey(Color.cyan, 0.75f), new GradientColorKey(Color.blue, 1.0f)},
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 1.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        fiber_lines.colorGradient = gradient;
        //lines.sharedMaterial.color = color;
        */
    }

    void drawFiberToCloseOther(GameObject fiber_element)
    {
        if (fiber_parent.name.EndsWith("_Closed")) return;
        Transform first = fiber_parent.transform.GetChild(0);
        Transform last = fiber_parent.transform.GetChild(fiber_parent.transform.childCount - 1);
        //check if last or first
        int rank = fiber_element.transform.GetSiblingIndex();
        if ((rank == 0) || (rank == fiber_parent.transform.childCount- 1)) {
            float D = Vector3.Distance(first.transform.position, last.transform.position);
            //fiber_current_distance = D;
            if (D < fiber_closing_distance)
            {
                fiber_lines.enabled = true;
                //change the color to the prefab?
                DrawLineFiberClose(first.transform.position, last.transform.position);//
            }
            else {
                fiber_lines.enabled = false;
            }
        }
        else
            fiber_lines.enabled = false;
        //check if other identical fiber type ?

    }

    bool connectFiberToCloseOther(GameObject fiber_element) {
        //is it already closed ?
        bool closed = false;
        if (fiber_parent.name.EndsWith("_Closed")) return closed;//already closed
        Transform first = fiber_parent.transform.GetChild(0);
        Transform last = fiber_parent.transform.GetChild(fiber_parent.transform.childCount - 1);
        //check if last or first
        int rank = fiber_element.transform.GetSiblingIndex();
        if ((rank == 0) || (rank == fiber_parent.transform.childCount - 1))
        {
            float D = Vector3.Distance(first.transform.position, last.transform.position);
            //fiber_current_distance = D;
            if (D < fiber_closing_distance)
            {
                closePersistence();
                closed = true;
            }
        }
        fiber_lines.enabled = false;
        return closed;
    }

    private IEnumerator DragObject()
    {
        //check the connected body
        bool fmode = (fiber_parent != null);
        bool closed = false;
        bool closing = current_properties.closing;
        //Debug.Log(m_SpringJoint.connectedBody);
        var oldDrag = m_SpringJoint.connectedBody.drag;
        var oldAngularDrag = m_SpringJoint.connectedBody.angularDrag;
        m_SpringJoint.connectedBody.drag = k_Drag;
        m_SpringJoint.connectedBody.angularDrag = k_AngularDrag;

        m_SpringJoint.dampingRatio = damping;
        m_SpringJoint.frequency = frequency;
        lines.enabled = true;
        var mainCamera = current_camera;
        
        while (Input.GetMouseButton(0))
        {
            //var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            //Input.mousePosition
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10.0f;       // we want 2m away from the camera position
                                      //Debug.Log(mousePos);
            m_SpringJoint.transform.position = mainCamera.ScreenToWorldPoint(mousePos);// ray.GetPoint(distance);
            //Debug.DrawLine(m_SpringJoint.connectedBody.transform.position,m_SpringJoint.transform.position, Color.yellow);
            DrawLine(m_SpringJoint.connectedBody.gameObject.transform.TransformPoint(m_SpringJoint.connectedAnchor), m_SpringJoint.transform.position);//,0.25f,20.0f,1,false);
            //if fiber should check for closing
            if (fmode && closing) {
                drawFiberToCloseOther(m_SpringJoint.connectedBody.gameObject);
            }
            yield return null;
        }
        if (fmode && closing)
        {
            closed = connectFiberToCloseOther(m_SpringJoint.connectedBody.gameObject);
        }
        if (m_SpringJoint.connectedBody)
        {
            m_SpringJoint.connectedBody.drag = oldDrag;
            m_SpringJoint.connectedBody.angularDrag = oldAngularDrag;
            if (!closed) m_SpringJoint.connectedBody.bodyType = bodyType;
            m_SpringJoint.connectedBody = null;
        }
        //clean outline
        CleanOutline();
        lines.enabled = false;
    }

    void dragInstance(GameObject other, Vector3 point)
    {
        if (!m_SpringJoint)
        {
            var go = new GameObject("Rigidbody dragger");
            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            m_SpringJoint = go.AddComponent<SpringJoint2D>();
            body.bodyType = RigidbodyType2D.Static;
        }
        //Debug.Log(m_SpringJoint);
        //m_SpringJoint.transform.position = point;
        m_SpringJoint.autoConfigureDistance = false;
        m_SpringJoint.anchor = Vector3.zero;
        m_SpringJoint.connectedAnchor = other.transform.InverseTransformPoint(point);
        
        //m_SpringJoint.spring = k_Spring;
        //m_SpringJoint.damper = k_Damper;
        //m_SpringJoint.maxDistance = k_Distance;
        m_SpringJoint.connectedBody = other.GetComponent<Rigidbody2D>();
        fiber_parent = null;
        if (other.GetComponent<PrefabProperties>() != null)
        {
            current_properties = other.GetComponent<PrefabProperties>();
           // other.GetComponent<PrefabProperties>().UpdateOutline(true);
            if (other.GetComponent<PrefabProperties>().is_fiber)
            {
                fiber_parent = other.transform.parent.gameObject;
            }
        }
      /*  else {
            //check if the parent has the outline
            if (other.transform.parent.GetComponent<PrefabProperties>() != null) {
                other.transform.parent.GetComponent<PrefabProperties>().UpdateOutline(true);
            }
        }
      */
        bodyType = m_SpringJoint.connectedBody.bodyType;
        m_SpringJoint.connectedBody.bodyType = RigidbodyType2D.Dynamic;
        //Debug.Log(m_SpringJoint.connectedBody);
        m_SpringJoint.distance = 0.0f;
        StartCoroutine("DragObject");
    }

    void eraseInstance() {
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = raycast();
            if (hit)
            {
                if (hit.collider.gameObject == gameObject) return;
                DestroyInstance(hit.collider.gameObject);
            }
        }
    }

    void ShowAttachmentsLineRenderer(){
        if (attached.Count == 0){
             if (bound_lines_holder != null) bound_lines_holder.SetActive(false);
             return;
        }
        else {
            if (bindMode || dragMode || pinMode || eraseMode){
                if (bound_lines_holder == null){
                        bound_lines_holder = new GameObject("lines_holder");
                        bound_lines = new List<LineRenderer>();
                        //bound_lines.startColor = spriteRenderer.sharedMaterial.color;
                        //bound_lines.endColor = jt.connectedBody.GetComponent<SpriteRenderer>().sharedMaterial.color;
                }
                bound_lines_holder.SetActive(true);
                int nLines = bound_lines.Count;
                if (nLines > attachments.Count){
                    for (int i=attachments.Count;i<nLines;i++)
                    {
                        bound_lines[i].gameObject.SetActive(false);
                    }
                }
                for(int i=0;i< attachments.Count; i++){
                    var jt = attachments[i];
                    if (jt.gameObject.GetComponent<PrefabProperties>().is_fiber) continue;
                    LineRenderer line;
                    if (i < nLines)
                    {
                        line = bound_lines[i];
                        line.gameObject.SetActive(true);
                    }
                    else {
                        var g = new GameObject("lines_"+i.ToString());
                        g.transform.parent = bound_lines_holder.transform;
                        line = g.AddComponent<LineRenderer>();
                        line.positionCount = 2;
                        bound_lines.Add(line);
                        line.sharedMaterial = Manager.Instance.lineMat;
                        line.sortingOrder = jt.gameObject.GetComponent<SpriteRenderer>().sortingOrder+1;
                        line.widthMultiplier = 0.3f;
                        line.numCapVertices = 5;
                    }
                    line.startColor = jt.gameObject.GetComponent<SpriteRenderer>().sharedMaterial.color;
                    line.endColor = jt.connectedBody.GetComponent<SpriteRenderer>().sharedMaterial.color;       
                    line.SetPosition(0, jt.gameObject.transform.TransformPoint(jt.anchor));
                    line.SetPosition(1, jt.connectedBody.transform.TransformPoint(jt.connectedAnchor));              
                }
            }
            else {
                if (bound_lines_holder != null) bound_lines_holder.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        dt = Time.deltaTime;
        if (dt > safety_deltaTime) {
            safety_frame ++;
            if (safety_frame > 2) {
                if (TogglePhysics) TogglePhysics.isOn = false;
                Physics2D.autoSimulation = false;
                //pop up warning
                if (message_panel) {
                    message_panel.SetActive(true);
                    message_panel.GetComponentInChildren<Text>().text = "Physics became unstable and was turned off; erase or move the faulty object before turning it back on";
                    UI_manager.Get.ToggleSettings.isOn = true;
                }
            }
            //find the toggle ?
        } else {
            safety_frame =0;
        }
        //JitterEverything();
        //DiffuseEverything();
        //DiffuseRBandSurface();
        ShowAttachmentsLineRenderer();
        if (!myPrefab) return;
        mouseDown = false;
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mousePos.z + (-current_camera.transform.position.z);// + zLevel;       // place the object at 0.00z position + whatever zLevel it wants to be at.
        Vector3 objectPos = current_camera.ScreenToWorldPoint(mousePos);
        mousePositionInViewPort = current_camera.ScreenToViewportPoint(mousePos); ////mouse location in view port
        transform.position = objectPos;
        clockwise.SetActive(fiberMode);
        //if mouse over ui return
        //this doesnt work with the Canvas Scaler in the build.
        //if (uiHolder.rect.Contains(Input.mousePosition) && sideBarExpanded) return;
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(uiHolder, Input.mousePosition, null, out lp);
        if (uiHolder.rect.Contains(lp)) return;
        //if (mask_ui) return;//problem then it doesnt catch the buttonUp
        if (fiberMode)
        {
            selected_instance = null;
            drawInstanceFiber();
        }
        else if (dragMode || pinMode || infoMode || eraseMode) {
            if (mask_ui) return;
            pindragInstance();
            UpdateInfo();
            //attached
        }
        else if (groupMode) {
            if (mask_ui) return;
            SelectAndGroupInstance();
        }
        else if (ghostMode) {
            if (mask_ui) return;
            SelectAndGhostInstance();
        }
        else if (bindMode)         
        {
            if (mask_ui) return;
            //HighlightManager.Instance.TogglePinToBond(true);
            //if (!HighlightManager.Instance.pinnedTo_Highlighted) HighlightManager.Instance.HighlightPinnedTo();
            attachTwoObjects();
            UpdateInfo();
        }
        //else if (eraseMode) pindragInstance();// eraseInstance();
        else if (surfaceMode) {
            if (mask_ui) return;
            selected_instance = null;
            UdateSurfacePrefab();
            drawInstanceSurface();
        }
        else if (boundMode) {
            if (mask_ui) return;
            selected_instance = null;
            UdateBoundToPrefab();
            drawInstanceBoundToFiber();
        }
        else if (drawMode)
        {
            if (mask_ui) return;
            selected_instance = null;
            drawInstance();
        }
        else if (bucketMode) {
            if (mask_ui) return;
            fillCompartments();
        }
        else if (measureMode){
            if (mask_ui) return;
            MeasureManager.Get.DoMeasure();
        }
        else { }
        prev_mousePos = objectPos;

        //handle the delta mouse position
        if (Input.GetMouseButtonDown(0))
        {
            lastPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            delta = (Input.mousePosition - lastPos).magnitude;
            lastPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0)) {
            delta = 0;
        }
        updateScale();
    }

    void FixedUpdate()
    {
        //DiffuseEverything();
        fixedCount++;
    }

    IEnumerator jitterCoroutine() {
        Rigidbody2D[] allrb = root.GetComponentsInChildren<Rigidbody2D>();
        float reduce = 1.0f;
        if (scale_force == 0) yield return null;
        for (int i = 0; i < allrb.Length; i++) {
            if (allrb[i].GetComponent<PrefabProperties>().is_fiber) { reduce = 1.0f / 100.0f; }
            if (allrb[i].GetComponent<PrefabProperties>().is_surface) { reduce = 1.0f / 10.0f; }
            else reduce = 1.0f;
            allrb[i].AddTorque( UnityEngine.Random.Range(-(timeScale), (timeScale)) * (scale_force / 2), 0);
            allrb[i].AddForce(UnityEngine.Random.insideUnitCircle * scale_force * reduce);
            yield return null;
        }
        //yield return new WaitForEndOfFrame();
        StartCoroutine(jitterCoroutine());
        //yield return null;
    }

    public void SetTimeScale(float value) {
        timeScale = value;
        timescale_label.text = timeScale.ToString();
    }

    //public GameObject times_labels;
    //public Text deltatime_labels;

    void DiffuseEverything()
    {
        //time is in seconds
        //how to scale in nanoseconds. use timeScale, if 1 1s = 1nsec
        //1 units is 0.025 angstrom = 0.0025nm
        //so if we want to move by 1nm => dx * 1/0.0025
        var dTime = Time.deltaTime * timeScale;

        var temperature = scale_force;//in K 0-500, room temperature 298K
        var uScale = unit_scale;// 1.0f/0.0025f;
        if ((count_update % 2) == 0)
        {
            count_update++;
            return;
        }
        count_update++;
        Debug.Log("dTime " + dTime.ToString());
        if (Input.GetMouseButton(0)) return;
        even = !even;
        //even = (count_update % 2)==0;

        //handle the jitter through the arrary of rigid bodies.
        if (temperature == 0) return;

        Rigidbody2D[] allrb = root.GetComponentsInChildren<Rigidbody2D>();
        
        for (int i = 0; i < allrb.Length; i++)
        {
            PrefabProperties p = allrb[i].GetComponent<PrefabProperties>();
            Vector2 direction_random = UnityEngine.Random.insideUnitCircle;
            if (p == null) continue;
            var R = p.circle_radius*uScale;//angstrom
            var dtheta = 0.0f;
            if ( UnityEngine.Random.value < 0.5f)
                dtheta = 1;
            else
                dtheta = -1;
            if (p.is_fiber) {
                direction_random = direction_random * Mathf.Sqrt((4.0f * 8.0e-5f * dTime));//in nm
                dtheta = 0;//in rad
                continue;
            }
            else if (p.is_surface) {
                direction_random = direction_random * Mathf.Sqrt((4.0f * 8.0e-5f * dTime));//in nm
                dtheta = 0;// dtheta * Mathf.Sqrt((2.0f * 0.184f * dTime) / (R * R * R * R));//in rad
            }
            else {
                //regular rigid body
                direction_random = direction_random * Mathf.Sqrt((6.0f * 0.245f * dTime)/R);//in nm
                dtheta = dtheta * Mathf.Sqrt((2.0f * 0.184f * dTime) / (R * R * R));//in rad
                Debug.Log(Mathf.Sqrt((6.0f * 0.245f * dTime) / R) * 1.0f / uScale);
            }
            //Lerp?
            allrb[i].MovePosition(allrb[i].position + direction_random * Mathf.Sqrt(temperature / 298.0f) * 1.0f/uScale);
            allrb[i].MoveRotation(allrb[i].rotation + Mathf.Rad2Deg*dtheta * Mathf.Sqrt(temperature / 298.0f) );
            //allrb[i].AddTorque(Random.Range(-(timeScale), (timeScale)) * (scale_force / 2), 0);
            //allrb[i].AddForce(UnityEngine.Random.insideUnitCircle * scale_force * reduce);
        }
    }


    void DiffuseRBandSurface()
    {
        var dTime = Time.deltaTime * timeScale;
        var temperature = scale_force;//in K 0-500, room temperature 298K
        var uScale = unit_scale;// 1.0f/0.0025f;

        if ((count_update % 2) == 0)
        {
            count_update++;
            return;
        }
        count_update++;
        if (Input.GetMouseButton(0)) return;
        even = !even;
        //even = (count_update % 2)==0;

        //handle the jitter through the arrary of rigid bodies.
        if (scale_force == 0) return;

        for (int i = 0; i < rbCount; i++)
        {
            Rigidbody2D player = everything[i];
            PrefabProperties p = player.GetComponent<PrefabProperties>();
            Vector2 direction_random = UnityEngine.Random.insideUnitCircle;
            if (p == null) continue;
            var R = p.circle_radius * uScale;//angstrom
            var dtheta = 0.0f;
            if (  UnityEngine.Random.value < 0.5f)
                dtheta = 1;
            else
                dtheta = -1;
            //regular rigid body
            direction_random = direction_random * Mathf.Sqrt((6.0f * 0.245f * dTime) / R);//in nm
            dtheta = dtheta * Mathf.Sqrt((2.0f * 0.184f * dTime) / (R * R * R));//in rad
            //Lerp?
            player.MovePosition(player.position + direction_random * Mathf.Sqrt(temperature / 298.0f) * 1.0f / uScale);
            player.MoveRotation(player.rotation + Mathf.Rad2Deg * dtheta * Mathf.Sqrt(temperature / 298.0f));
        }
        for (int i = 0; i < surface_objects.Count; i++)
        {
            Rigidbody2D player = surface_objects[i].GetComponent<Rigidbody2D>();
            if (player == null) continue;
            //player.bodyType = RigidbodyType2D.Dynamic;
            //player.collisionDetectionMode = CollisionDetectionMode2D.Discrete; //this is slow, put it in the on mouse up!
            Vector2 direction_random = UnityEngine.Random.insideUnitCircle * Mathf.Sqrt((4.0f * 8.0e-5f * dTime));//in nm
            player.MovePosition(player.position + direction_random * Mathf.Sqrt(temperature / 298.0f) * 1.0f / uScale);
            //player.MoveRotation(player.rotation + Mathf.Rad2Deg * dtheta * Mathf.Sqrt(temperature / 298.0f));
        }
    }

    IEnumerator DiffuseEverythingRoutine()
    {
        //time is in seconds
        //how to scale in nanoseconds. use timeScale, if 1 1s = 1nsec
        //1 units is 0.025 angstrom = 0.0025nm
        //so if we want to move by 1nm => dx * 1/0.0025
        var dTime = Time.deltaTime * timeScale;
        var temperature = scale_force;//in K 0-500, room temperature 298K
        var uScale = unit_scale;// 1.0f/0.0025f;
        if ((count_update % 2) == 0)
        {
            count_update++;
            yield return null;
        }
        count_update++;
        Debug.Log("dTime " + dTime.ToString());
        if (Input.GetMouseButton(0)) yield return null;
        even = !even;
        //even = (count_update % 2)==0;

        //handle the jitter through the arrary of rigid bodies.
        if (temperature == 0) yield return null;
        Rigidbody2D[] allrb = root.GetComponentsInChildren<Rigidbody2D>();

        for (int i = 0; i < allrb.Length; i++)
        {
            Debug.Log(allrb[i].gameObject.name);
            PrefabProperties p = allrb[i].GetComponent<PrefabProperties>();
            Vector2 direction_random = UnityEngine.Random.insideUnitCircle;
            if (p == null) continue;
            var R = p.circle_radius * uScale;//angstrom
            var dtheta = 0.0f;
            if ( UnityEngine.Random.value < 0.5f)
                dtheta = 1;
            else
                dtheta = -1;
            if (p.is_fiber)
            {

                direction_random = direction_random * Mathf.Sqrt((4.0f * 8.0e-5f * dTime)/ R) * scaling_fiber;//in nm
                dtheta = 0;//in rad
                //continue;
            }
            else if (p.is_surface)
            {
                float x = ( UnityEngine.Random.value < 0.5f) ? -1.0f : 1.0f;
                Vector2 d = new Vector2(x,0.0f);
                d = allrb[i].transform.TransformDirection(d);
                direction_random = d * Mathf.Sqrt((4.0f * 8.0e-5f * dTime)/ R) * scaling_surface;//in nm
                dtheta = 0;// dtheta * Mathf.Sqrt((2.0f * 0.184f * dTime) / (R * R * R * R));//in rad
            }
            else {
                //regular rigid body
                direction_random = direction_random * Mathf.Sqrt((6.0f * 0.245f * dTime) / R) * scaling_soluble;//in nm
                dtheta = dtheta * Mathf.Sqrt((2.0f * 0.184f * dTime) / (R * R * R));//in rad
                //Debug.Log(Mathf.Sqrt((6.0f * 0.245f * dTime) / R) * 1.0f / uScale);
            }
            //Lerp?
            allrb[i].MovePosition(allrb[i].position + direction_random * Mathf.Sqrt(temperature / 298.0f) * 1.0f / uScale);
            allrb[i].MoveRotation(allrb[i].rotation + Mathf.Rad2Deg * dtheta * Mathf.Sqrt(temperature / 298.0f));
            //allrb[i].AddTorque(Random.Range(-(timeScale), (timeScale)) * (scale_force / 2), 0);
            //allrb[i].AddForce(UnityEngine.Random.insideUnitCircle * scale_force * reduce);
        }
        yield return new WaitForEndOfFrame();
        StartCoroutine(DiffuseEverythingRoutine());
    }

    IEnumerator DiffuseRBandSurfaceRoutine()
    {
        var dTime = Time.deltaTime * timeScale;
        var temperature = scale_force;//in K 0-500, room temperature 298K
        var uScale = unit_scale;// 1.0f/0.0025f;

        if ((count_update % 2) == 0)
        {
            count_update++;
            yield return null;
        }
        count_update++;
        if (Input.GetMouseButton(0)) temperature = 0;
        even = !even;
        //even = (count_update % 2)==0;
        if (temperature == 0)
        {
            count_update++;
            yield return null;
        }
        //handle the jitter through the arrary of rigid bodies.
        //if (temperature == 0) yield return null; 
        if (temperature != 0)
        {
            total_time += dTime;
            total_time_sec += Time.unscaledDeltaTime;
        }
        if (deltatime_labels.gameObject.activeSelf)
        {
            deltatime_labels.text = "dT(ns): " + dTime.ToString() + "\ncT(ns):  " + total_time.ToString() + " "+ total_time_sec.ToString()+"\n";
        }

        for (int i = 0; i < everything.Count; i++)
        {
            Rigidbody2D player = everything[i];
            PrefabProperties p = player.GetComponent<PrefabProperties>();
            if (p.ispined) continue;
            if (player.bodyType == RigidbodyType2D.Static) continue;
            Vector2 direction_random = UnityEngine.Random.insideUnitCircle;
            if (p == null) continue;
            var R = p.circle_radius * uScale;//nm
            var dtheta = 0.0f;
            if ( UnityEngine.Random.value < 0.5f)
                dtheta = 1;
            else
                dtheta = -1;
            //regular rigid body
            direction_random = direction_random * Mathf.Sqrt((6.0f * 0.245f * dTime) / R)* scaling_soluble;//in nm
            dtheta = dtheta * Mathf.Sqrt((2.0f * 0.184f * dTime) / (R * R * R));//in rad
            //Lerp?
            Vector2 pp =player.position + direction_random * Mathf.Sqrt(temperature / 298.0f) * 1.0f / uScale ;
            player.MovePosition(pp);
            player.MoveRotation(player.rotation + Mathf.Rad2Deg * dtheta * Mathf.Sqrt(temperature / 298.0f));
        }
        for (int i = 0; i < surface_objects.Count; i++)
        {
            Rigidbody2D player = surface_objects[i].GetComponent<Rigidbody2D>();
            if (player == null) continue;
            if (player.bodyType == RigidbodyType2D.Static) continue;
            //player.bodyType = RigidbodyType2D.Dynamic;
            //player.collisionDetectionMode = CollisionDetectionMode2D.Discrete; //this is slow, put it in the on mouse up!
            float x = ( UnityEngine.Random.value < 0.5f) ? -1.0f : 1.0f;
            Vector2 d = new Vector2(x,0.0f);
            d = player.transform.TransformDirection(d);
            Vector2 direction_random = d * Mathf.Sqrt((4.0f * 8.0e-5f * dTime)) * scaling_surface;//in nm
            //Vector2 direction_random = UnityEngine.Random.insideUnitCircle * Mathf.Sqrt((4.0f * 8.0e-5f * dTime));//in nm
            player.MovePosition(player.position + direction_random * Mathf.Sqrt(temperature / 298.0f) * 1.0f / uScale);
            //or addForce
            //player.MoveRotation(player.rotation + Mathf.Rad2Deg * dtheta * Mathf.Sqrt(temperature / 298.0f));
        }
        for (int i=0;i< fibers_instances.Count;i++ ){
            for (int j=0;j<fibers_instances[i].Count;j++ ){
                Rigidbody2D player = fibers_instances[i][j].GetComponent<Rigidbody2D>();
                if (player == null) continue;
                if (player.bodyType == RigidbodyType2D.Static) continue;
                PrefabProperties p = player.GetComponent<PrefabProperties>();
                Vector2 direction_random = UnityEngine.Random.insideUnitCircle;
                if (p == null) continue;
                var R = p.circle_radius * uScale;//nm
                var dtheta = 0.0f;
                if ( UnityEngine.Random.value < 0.5f)
                    dtheta = 1;
                else
                    dtheta = -1;
                //regular rigid body
                direction_random = direction_random * Mathf.Sqrt((6.0f * 0.245f * dTime) / R) * scaling_fiber;//in nm
                dtheta = dtheta * Mathf.Sqrt((2.0f * 0.184f * dTime) / (R * R * R))* scaling_fiber;//in rad
                //Lerp?
                player.MovePosition(player.position + direction_random * Mathf.Sqrt(temperature / 298.0f) * 1.0f / uScale);
                player.MoveRotation(player.rotation + Mathf.Rad2Deg * dtheta * Mathf.Sqrt(temperature / 298.0f));
            }
        }
        //yield return null;
        //yield return new WaitForEndOfFrame();
        yield return null;
        StartCoroutine(DiffuseRBandSurfaceRoutine());
    }

    void OnTriggerExit2D(Collider2D other) {
        fiber_attachto = null;
        otherSurf = null;
        if (current_prefab)
        {
            current_prefab.transform.position = transform.position;
            current_prefab.transform.rotation = transform.rotation;
            if (surfaceMode) current_prefab.GetComponent<SpriteRenderer>().sharedMaterial.color = new Color(0.4f, 0.4f, 0.4f);
            else current_prefab.GetComponent<SpriteRenderer>().sharedMaterial.color = prefab_materials[current_prefab.GetComponent<PrefabProperties>().name].color;
        }
    }
    /**
    http://www.sunshine2k.de/coding/java/PointOnLine/PointOnLine.html
    * Get projected point P' of P on line e1. Faster version.
    * @return projected point p.
    */
    public Vector3 getProjectedPointOnLineFast(Vector3 p, Vector3 v1,Vector3 v2)
    {
        // get dot product of e1, e2
        Vector2 e1 = new Vector2(v2.x - v1.x, v2.y - v1.y);
        Vector2 e2 = new Vector2(p.x - v1.x, p.y - v1.y);
        float val = Vector2.Dot(e1, e2);
        // get squared length of e1
        float len2 = e1.x * e1.x + e1.y * e1.y;
        Vector3 pp = new Vector3((int)(v1.x + (val * e1.x) / len2),
                            (int)(v1.y + (val * e1.y) / len2),0.0f);
        return pp;
    }

    void TransormToSurfacePos(GameObject other){
        //actually project on other xy plane
        float surfOffset = current_prefab.GetComponent<PrefabProperties>().surface_offset;
        current_prefab.transform.rotation = Quaternion.FromToRotation(Vector3.up, other.transform.up);
        var f = other.GetComponent<PrefabProperties>().fiber_length;
        Vector3 v1 = current_prefab.transform.rotation * new Vector3(-f,0,0)+other.transform.position;
        Vector3 v2 = current_prefab.transform.rotation * new Vector3(f,0,0)+other.transform.position;
        Vector3 proj = other.transform.position;//getProjectedPointOnLineFast(transform.position, v1, v2);//other.transform.position.x, other.transform.position.y
        Vector3 newpos = current_prefab.transform.rotation * new Vector3(0, -surfOffset, 0);
        current_prefab.transform.position = new Vector3(proj.x, proj.y, 0.0f) + newpos;   
    }

    void OnTriggerEnter2D(Collider2D other) {
        fiber_attachto = null;
        if (!other) return;
        Debug.Log(" enter other is " + other.name);// + " " + other.transform.parent.gameObject.name);
        if (surfaceMode)
        {
            //use bound to ?

            if (other.gameObject.tag == "membrane" || other.gameObject.tag == "DNA")
            {
                otherSurf = other;
                TransormToSurfacePos(other.gameObject);
                if ( prefab_materials.ContainsKey(current_prefab.GetComponent<PrefabProperties>().name)) current_prefab.GetComponent<SpriteRenderer>().sharedMaterial.color = prefab_materials[current_prefab.GetComponent<PrefabProperties>().name].color; //current_properties.Default_Sprite_Color;
            }
        }
        if (boundMode)
        {
            //use bound to ?
            Debug.Log("current_properties.bound_to "+current_properties.bound_to);
            Debug.Log(other.name);
            Debug.Log(other.transform.parent.gameObject.name);
            if (other.transform.parent.gameObject.name.Contains(current_properties.bound_to))
            {
                otherSurf = other;
                current_prefab.transform.position = other.transform.position;
                current_prefab.transform.rotation = Quaternion.FromToRotation(Vector3.up, other.transform.up);
            }
        }
        if (fiberMode)
        {
            if (!other) return;
            PrefabProperties props = other.gameObject.GetComponent<PrefabProperties>();
            if (props && props.is_fiber)
            {
                if (other.gameObject.transform.parent == null) return;
                if (myPrefab == null) return;
                if (other.gameObject.transform.parent.name.Contains(myPrefab.name))
                {
                    //check if first or last of previous fiber
                    int i = other.gameObject.transform.GetSiblingIndex();
                    if (i == 0 || i == other.gameObject.transform.parent.childCount - 1)
                    {
                        //attach
                        fiber_attachto = other.gameObject;
                        //Debug.Log("attach to"+fiber_attachto.name);
                        return;
                    }
                }
            }
            //fiber_attachto = null;
        }
    }
  
    void OnTriggerStay2D(Collider2D other)
    {

        if (!other) return;
        Debug.Log(" stay other is " + other.name+" "+ other.transform.parent.gameObject.name);
        if (surfaceMode)
        {
            //use bound to ?
            
            if (other.gameObject.tag == "membrane" || other.gameObject.tag == "DNA")
            {
                otherSurf = other;
                TransormToSurfacePos(other.gameObject);
                //current_prefab.transform.position = other.transform.position;
                //current_prefab.transform.rotation = Quaternion.FromToRotation(Vector3.up, other.transform.up);
            }
        }
        if (boundMode)
        {
            //use bound to ?
            if (other.transform.parent.gameObject.name.Contains( current_properties.bound_to) )
            {
                otherSurf = other;
                current_prefab.transform.position = other.transform.position;
                current_prefab.transform.rotation = Quaternion.FromToRotation(Vector3.up, other.transform.up);
            }
        }
        /*
        if (fiberMode)
        {
            if (other.gameObject.GetComponent<PrefabProperties>().is_fiber) {
                if (other.gameObject.transform.parent.name.Contains(myPrefab.name)) {
                    //check if first or last of previous fiber
                    int i = other.gameObject.transform.GetSiblingIndex();
                    if (i == 0 || i == other.gameObject.transform.parent.childCount - 1) {
                        //attach
                        fiber_attachto = other.gameObject;
                        Debug.Log("attach to "+fiber_attachto.name);
                        return;
                    }
                }
            }
            //fiber_attachto = null;
        }*/
    }

    void UdateSurfacePrefab(){
        LayerMask layerMask = LayerMask.GetMask("Membrane");//LayerMask.NameToLayer("Membrane");//LayerMask.GetMask("Wall");
        RaycastHit2D hit = Physics2D.Raycast(current_camera.ScreenPointToRay(Input.mousePosition).origin,current_camera.ScreenPointToRay(Input.mousePosition).direction, 100, layerMask);  
        //Physics2D.CircleCast(Vector2 origin, float radius, Vector2 direction, float distance = Mathf.Infinity, int layerMask = DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity);
        //RaycastHit2D hit = Physics2D.CircleCast(current_camera.ScreenPointToRay(Input.mousePosition).origin,2.0f,current_camera.ScreenPointToRay(Input.mousePosition).direction,100, layerMask);
        Debug.Log("UdateSurfacePrefab "+layerMask.ToString());
        Debug.Log(hit);
        if (hit){
            if (hit.collider){
                Debug.Log("HIT "+hit.collider.gameObject.name);
                var mb = hit.collider.gameObject;
                if (mb.tag == "membrane" || mb.tag == "DNA")
                {
                    otherSurf = hit.collider;
                    TransormToSurfacePos(mb);
                    if ( prefab_materials.ContainsKey(current_prefab.GetComponent<PrefabProperties>().name)) current_prefab.GetComponent<SpriteRenderer>().sharedMaterial.color = prefab_materials[current_prefab.GetComponent<PrefabProperties>().name].color; //current_properties.Default_Sprite_Color;
                }                
            }
        }
        else {
            fiber_attachto = null;
            otherSurf = null;
            if (current_prefab)
            {
                current_prefab.transform.position = transform.position;
                current_prefab.transform.rotation = transform.rotation;
                if (surfaceMode) current_prefab.GetComponent<SpriteRenderer>().sharedMaterial.color = new Color(0.4f, 0.4f, 0.4f);
                else current_prefab.GetComponent<SpriteRenderer>().sharedMaterial.color = prefab_materials[current_prefab.GetComponent<PrefabProperties>().name].color;
            }
        }
    }
    
    void UdateBoundToPrefab(){
        LayerMask layerMask = LayerMask.GetMask("Membrane")|LayerMask.GetMask("DNA");//LayerMask.NameToLayer("Membrane");//LayerMask.GetMask("Wall");
        RaycastHit2D hit = Physics2D.Raycast(current_camera.ScreenPointToRay(Input.mousePosition).origin,current_camera.ScreenPointToRay(Input.mousePosition).direction, 100, layerMask);  
        //Physics2D.CircleCast(Vector2 origin, float radius, Vector2 direction, float distance = Mathf.Infinity, int layerMask = DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float maxDepth = Mathf.Infinity);
        //RaycastHit2D hit = Physics2D.CircleCast(current_camera.ScreenPointToRay(Input.mousePosition).origin,2.0f,current_camera.ScreenPointToRay(Input.mousePosition).direction,100, layerMask);
        Debug.Log("UdateBoundToPrefab "+layerMask.ToString());
        Debug.Log("current_properties.bound_to "+current_properties.bound_to);
        Debug.Log(hit);
        if (hit){
            if (hit.collider){
                Debug.Log("HIT "+hit.collider.gameObject.name);
                var f = hit.collider.gameObject;
                if (f.transform.parent.gameObject.name.Contains(current_properties.bound_to))
                {
                    otherSurf = hit.collider;
                    TransormToSurfacePos(f);
                    if ( prefab_materials.ContainsKey(current_prefab.GetComponent<PrefabProperties>().name)) current_prefab.GetComponent<SpriteRenderer>().sharedMaterial.color = prefab_materials[current_prefab.GetComponent<PrefabProperties>().name].color; //current_properties.Default_Sprite_Color;
                }                
            }
        }
        else {
            fiber_attachto = null;
            otherSurf = null;
            if (current_prefab)
            {
                current_prefab.transform.position = transform.position;
                current_prefab.transform.rotation = transform.rotation;
                if (boundMode) current_prefab.GetComponent<SpriteRenderer>().sharedMaterial.color = new Color(0.4f, 0.4f, 0.4f);
                else current_prefab.GetComponent<SpriteRenderer>().sharedMaterial.color = prefab_materials[current_prefab.GetComponent<PrefabProperties>().name].color;
            }
        }
    }

    RaycastHit2D raycast()
    {
        //ignore UI as well
        LayerMask layerMask = ~(  1 << LayerMask.NameToLayer("CameraCollider") 
                                | 1 << LayerMask.NameToLayer("FiberPushAway") 
                                | 1 << LayerMask.NameToLayer("OnlyMembrane") 
                                | 1 << LayerMask.NameToLayer("UI") 
                                | 1 << LayerMask.NameToLayer("background")
                                | 1 << LayerMask.NameToLayer("bg_image")
                                | 1 << LayerMask.NameToLayer("MembraneBot")); // ignore both layerX and layerY
        RaycastHit2D hit = new RaycastHit2D();
        hit = Physics2D.Raycast(current_camera.ScreenPointToRay(Input.mousePosition).origin,
                                current_camera.ScreenPointToRay(Input.mousePosition).direction, 100, layerMask);
        return hit;
    }

    RaycastHit2D raycast_from(Vector3 pos)
    {
        //ignore UI as well
        LayerMask layerMask = ~(  1 << LayerMask.NameToLayer("CameraCollider") 
                                | 1 << LayerMask.NameToLayer("FiberPushAway") 
                                | 1 << LayerMask.NameToLayer("OnlyMembrane") 
                                | 1 << LayerMask.NameToLayer("UI") 
                                | 1 << LayerMask.NameToLayer("background")
                                | 1 << LayerMask.NameToLayer("bg_image")
                                | 1 << LayerMask.NameToLayer("MembraneBot")); // ignore both layerX and layerY
        RaycastHit2D hit = new RaycastHit2D();
        hit = Physics2D.Raycast(pos,
                                Vector3.back, 100, layerMask);
        return hit;
    }

    private void CleanOutline()
    {
        if (below && below != attach1)
        {
            //if (below.name == "ghostArea") {
            //    ghostHighlight(false);
            //    return;
            //}
            var parent = below.transform.parent;
            //if (m_SpringJoint && m_SpringJoint.connectedBody != null)
            //    return;
            if (below.GetComponent<PrefabProperties>()!= null)
                below.GetComponent<PrefabProperties>().UpdateOutline(false);
            if (below.transform.parent&&below.transform.parent.GetComponent<PrefabProperties>() != null)
            {
                below.transform.parent.GetComponent<PrefabProperties>().UpdateOutline(false);
                parent = below.transform.parent.transform.parent;
            }
            highLightHierarchy(parent, false);
            if (below.GetComponent<PrefabProperties>()!=null)
                highLightProteinType(below.GetComponent<PrefabProperties>().name, false);
            if (below!=null && below.name.StartsWith("ghost_")) {
                //highlight path
                below.GetComponent<Ghost>().ToggleHighlight(false);
            }
            if (other!=null && other.name.StartsWith("ghost_")) {
                //highlight path
                other.GetComponent<Ghost>().ToggleHighlight(false);
            }
            below = null;
        }
        togglePinOutline(pinMode);
    }

    public void togglePinOutline(bool toggle)
    {
        foreach (PrefabProperties properties in pinned_object)
        {
            properties.UpdateOutlinePin(toggle);
        }
    }

    public void allToggleOff() {
        boundMode = false;
        surfaceMode = false;//set geT ?
        fiberMode = false;
        drawMode = false;
        dragMode = false;
        eraseMode = false;
        pinMode = false;
        continuous = false;
        infoMode = false;
        bindMode = false;
        groupMode = false;
        ghostMode = false;
        measureMode = false;
        allOff = true;
    }

    public void ToggleContinuous(bool toggle)
    {
        allToggleOff();
        if (myPrefab)
        {
            PrefabProperties props = myPrefab.GetComponent<PrefabProperties>();
            fiberMode = props.is_fiber;
            surfaceMode = props.is_surface;
            boundMode = props.is_bound;
        }
        if (!fiberMode)
        {
            drawMode = toggle;
            continuous = toggle;
        }
        if (fiberMode || drawMode)
        {
            //if (myPrefab) myPrefab.SetActive(true);
        }
        if (fiberMode)
            pushAway.SetActive(true);
        else {
            pushAway.SetActive(false);
        }
        if (surfaceMode) GetComponent<CircleCollider2D>().enabled = true;
        if (toggle) allOff = false;
    }

    public void TogglePen(bool toggle)
    {
        allToggleOff();
        if (toggle) allOff = false;
        if (myPrefab)
        {
            PrefabProperties props = myPrefab.GetComponent<PrefabProperties>();
            fiberMode = props.is_fiber;
            surfaceMode = props.is_surface;
            boundMode = props.is_bound;
        }
        if (!fiberMode)
        {
            drawMode = toggle;
            continuous = !toggle;
        }
        if (fiberMode || drawMode)
        {
            //if (myPrefab) myPrefab.SetActive(true);
        }
        if (fiberMode)
            pushAway.SetActive(true);
        else {
            pushAway.SetActive(false);
        }

        ////when toggle remove all selected prefab as this only used in bucket mode
        if (fiberMode || drawMode) selected_prefab.Clear();
    }

    public void ToggleBucket(bool toggle)
    {
        ////bool f = fiberMode;
        allToggleOff();
        if (toggle) allOff = false;
        bucketMode = toggle;
        ////fiberMode = f;

        if (current_prefab)
            current_prefab.SetActive(!bucketMode);
        if (myPrefab)
        {
            myPrefab.SetActive(!bucketMode);
        }

        if (bucketMode)
        {
            if (myPrefab) myPrefab.SetActive(true);
        }
        else {
            pushAway.SetActive(false);
        }
    }
    
    public void ToggleErase(bool toggle)
    {
        allToggleOff();
        if (toggle) allOff = false;
        eraseMode = toggle;
        if (current_prefab)
            current_prefab.SetActive(!eraseMode);
        if (myPrefab)
        {
            myPrefab.SetActive(!eraseMode);
        }
        pushAway.SetActive(false);
    }

    public void ToggleMeasure(bool toggle)
    {
        Debug.Log("ToggleMeasure");
        allToggleOff();
        if (toggle) allOff = false;
        measureMode = toggle;
        if (current_prefab)
            current_prefab.SetActive(!measureMode);
        if (myPrefab)
        {
            myPrefab.SetActive(!measureMode);
        }
        pushAway.SetActive(false);
        Debug.Log("ToggleLineAndLAbel");
        MeasureManager.Get.ToggleLineAndLAbel(toggle);
        Debug.Log("ToggleLineAndLAbel OK");
    }

    public void TogglePin(bool toggle)
    {
        allToggleOff();
        if (toggle) allOff = false;
        pinMode = toggle;
        if (current_prefab)
            current_prefab.SetActive(!pinMode);
        if (myPrefab)
        {
            myPrefab.SetActive(!pinMode);
        }
        togglePinOutline(toggle);
        pushAway.SetActive(false);
        //if (toggle) UpdateGhostArea();
    }

    public void ToggleGroup(bool toggle)
    {
        allToggleOff();
        if (toggle) allOff = false;
        groupMode = toggle;
        if (current_prefab)
            current_prefab.SetActive(!groupMode);
        if (myPrefab)
        {
            myPrefab.SetActive(!groupMode);
        }
        togglePinOutline(toggle);
        pushAway.SetActive(false);
        if (groupMode) {
            GroupManager.Get.current_selections.Clear();
        }
    }

    public void ToggleGhost(bool toggle)
    {
        allToggleOff();
        if (toggle) allOff = false;
        ghostMode = toggle;
        if (current_prefab)
            current_prefab.SetActive(!ghostMode);
        if (myPrefab)
        {
            myPrefab.SetActive(!ghostMode);
        }
        togglePinOutline(toggle);
        pushAway.SetActive(false);
        if (ghostMode) {
            GroupManager.Get.current_selections.Clear();
        }
    }

    public void ToggleBind(bool toggle)
    {
        if (bindMode==false){
            if (attach1!=null)
            {
                attach1.GetComponent<PrefabProperties>().UpdateOutline(false);
                attach1 = null;
            }
            if (attach2!=null)
            {
                attach2.GetComponent<PrefabProperties>().UpdateOutline(false);
                attach2 = null;        
            }
        }
        allToggleOff();
        if (toggle) allOff = false;
        bindMode = toggle;
        if (current_prefab)
            current_prefab.SetActive(!bindMode);
        if (myPrefab)
        {
            myPrefab.SetActive(!bindMode);
        }
        togglePinOutline(toggle);
        pushAway.SetActive(false);
    }

    public void TogglePickDrag(bool toggle)
    {
        allToggleOff();
        if (toggle) allOff = false;
        dragMode = toggle;
        GetComponent<CircleCollider2D>().enabled = !toggle;
        if (current_prefab)
            current_prefab.SetActive(!toggle);
        togglePinOutline(toggle);
        pushAway.SetActive(false);
    }

    public void ToggleInfo(bool toggle)
    {
        //Description_Holder.SetActive(toggle);
        allToggleOff();
        if (toggle) allOff = false;
        infoMode = toggle;
        GetComponent<CircleCollider2D>().enabled = !toggle;
        if (current_prefab)
            current_prefab.SetActive(!toggle);
        togglePinOutline(toggle);
        pushAway.SetActive(false);
    }

    public void mercuryRising()
    {
        float mercuryMax = thermometer.GetComponent<Slider>().maxValue;
        float mercuryCurrent = thermometer.GetComponent<Slider>().value;
        mercury.GetComponent<Image>().fillAmount= mercuryCurrent/mercuryMax;
    }

    public void GlowActive()
    {
        /*
        //UnityStandardAssets.ImageEffects.Bloom bloomEffect = current_camera.GetComponent<Bloom>();
        if (bloom_active.GetComponent<Toggle>().isOn == false) bloomEffect.enabled = false;
        if (bloom_active.GetComponent<Toggle>().isOn == true) bloomEffect.enabled = true;
        if (bloom_active.GetComponent<Toggle>().isOn)
        {
            bloomEffect.bloomThreshold = bloom_threshold.GetComponent<Slider>().value;
            bloomEffect.bloomIntensity = bloom_amount.GetComponent<Slider>().value;
        }
        */
    }

    public void VignetteActive()
    {
        /*
        //UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration vignetteEffect = current_camera.GetComponent<VignetteAndChromaticAberration>();
        float vignetteSliderValue = vignette_slider.GetComponent<Slider>().value;
        if (vignetteSliderValue == 0) vignetteEffect.enabled = false;
        if (vignetteSliderValue != 0)
        {
            vignetteEffect.enabled = true;
            vignetteEffect.chromaticAberration = 03.5f * vignetteSliderValue;
            vignetteEffect.blurSpread = 0.75f * vignetteSliderValue;
            vignetteEffect.blur = 0.325f * vignetteSliderValue;
            vignetteEffect.intensity = 0.325f * vignetteSliderValue;
        }
        */
    }

    public void DepthBlurActive()
    {
        /*
        if (blur_active.GetComponent<Toggle>().isOn == false) blurEffect.enabled = false;
        if (blur_active.GetComponent<Toggle>().isOn == true) blurEffect.enabled = true;
        foreach (Transform child in current_camera.transform)
        {
            if (child.GetComponent<BlurOptimized>())
            {
                UnityStandardAssets.ImageEffects.BlurOptimized blurEffect = child.GetComponent<BlurOptimized>();
                if (blur_active.GetComponent<Toggle>().isOn == false) blurEffect.enabled = false;
                if (blur_active.GetComponent<Toggle>().isOn == true) blurEffect.enabled = true;
            }
        }*/
    }

    public void GreyScaleActive()
    {
        /*
        UnityStandardAssets.ImageEffects.Grayscale grayscaleEffect = current_camera.GetComponent<Grayscale>();
        if (greyscale_active.GetComponent<Toggle>().isOn == false) grayscaleEffect.enabled = false;
        if (greyscale_active.GetComponent<Toggle>().isOn == true) grayscaleEffect.enabled = true;
        */
    }

    public void SepiaToneActive()
    {

    }

    public void NoiseActive()
    {

    }
    public void ToggleMembraneMembrane_collision(bool toggle)
    {
        //11 and 24
        Physics2D.IgnoreLayerCollision(11, 11, !toggle);
        Physics2D.IgnoreLayerCollision(11, 24, !toggle);
        Physics2D.IgnoreLayerCollision(24, 24, !toggle);
    }

    public void ToggleProtein_Protein_layer1_collision(bool toggle)
    {
        Physics2D.IgnoreLayerCollision(8, 8, !toggle);
        Physics2D.IgnoreLayerCollision(8, 12, !toggle);
        Physics2D.IgnoreLayerCollision(12, 12, !toggle);
    }
    public void ToggleProtein_Protein_layer2_collision(bool toggle)
    {
        Physics2D.IgnoreLayerCollision(9, 9, !toggle);
        Physics2D.IgnoreLayerCollision(9, 21, !toggle);
        Physics2D.IgnoreLayerCollision(21, 21, !toggle);
    }
    public void ToggleProtein_Protein_layer3_collision(bool toggle)
    {
        Physics2D.IgnoreLayerCollision(10, 10, !toggle);
    }
    public void ToggleDNA_DNA_collision(bool toggle)
    {
        Physics2D.IgnoreLayerCollision(13, 13, !toggle);
    }
    public void ToggleDNA_protein_collision(bool toggle)
    {
        Physics2D.IgnoreLayerCollision(13, 8, !toggle);
        Physics2D.IgnoreLayerCollision(13, 12, !toggle);
    }

    public void Clear()
    {
        //all_prefab
        //go through all children of root
        foreach (Transform child in root.transform)
        {
            GameObject.Destroy(child.gameObject);
            //GameObject.Find("Progressbar").GetComponent<Progressbar>().Value = percentFilledInt;
        }
        //int percentFilledInt = 0;
        proteinArea = 0;
        totalNprotein = 0;        
        //clear the all the count
        /*foreach (var keyvalue in proteins_count)
        {
            proteins_count[keyvalue.Key] = 0;
            if (proteins_ui_labels.ContainsKey(keyvalue.Key)) proteins_ui_labels[keyvalue.Key].text = keyvalue.Key;
        }*/
        fiber_init = false;
        fiber_parent = null;
        fiber_parents.Clear();
        fibers_instances.Clear();
        surface_objects.Clear();
        pinned_object.Clear();
        attached_object.Clear();
        selectedobject.Clear();
        attachments.Clear();
        attached.Clear();
        if (proteins_count!=null) proteins_count.Clear();
        fiber_count = 0;
        fiber_nextNameNumber = 0;
        rbCount = 0;
        boundedCount = 0;
        count_removed = 0;
        everything.Clear();// = new List<Rigidbody2D>();//[MaxRigidBodies];
        bounded = new Rigidbody2D[MaxRigidBodies];
        total_time = 0.0f;
        total_time_sec = 0.0f;
        GroupManager.Get.Clear();
        GhostManager.Get.Clear();
        update_texture = true;
    }

    public void countProteins(ListViewItem component, GameObject addedObject)
    {
        //foreach (Rigidbody2D rb in everything);
        //closed lipids calculate circumference and subtract area from screen size
        //
        string compartment= addedObject.transform.tag;

        if (compartment != "BloodPlasma" || compartment != "HIV" || compartment != "Cytoplasme") return;

        if (compartment == "BloodPlasma")
        {
            //TreeViewSampleData.j
            string name = addedObject.name;
            //UIWidgetsSamples.TreeViewSampleItemCountry item = new TreeViewSampleItemCountry(name); 
            //UIWidgetsSamples.TreeViewSampleItemContinent node = new TreeNode<item>;
        }

        if (compartment == "HIV")
        {
            //ui.DataSource.Add(addedObject);
            string name = addedObject.name;
        }
        if (compartment == "Cytoplasme")
        {
            //ui.DataSource.Add(addedObject);
            string name = addedObject.name;
        }
    }
    public void updateScale()
    {
        var pixel_scale = (unit_scale * 10.0f) / 100.0f;
        var unity_scale2d = 1.0f / (unit_scale * 10.0f);
        //scale bar is 300pix and it has a width of 128px
        //var imageScale =  1.0f/300.0f;
        //var local_scale = 1.0f/(pixel_scale * imageScale);
        var p1 = current_camera.ScreenToWorldPoint(Vector3.zero);
        var p2 = current_camera.ScreenToWorldPoint(new Vector3(128.0f,0,0));//300px
        var Distance = Vector3.Magnitude(p2-p1)*unit_scale; // in unity world coordinates
        scale_bar_text.text = Mathf.Round(Distance).ToString()+ "nm";
        float cscale = 1.0f/_canvas.transform.localScale.x;
        scale_bar_text.transform.parent.localScale = new Vector3(cscale,cscale,cscale);// 1.0/canvas_scale ?
        /*
        float cameraScale = current_camera.orthographicSize;
        int children = current_camera.transform.childCount;       
        var screenpt = current_camera.ScreenToViewportPoint(Vector3.zero);
        var screenpt1 = current_camera.ScreenToViewportPoint(new Vector3(1,0,0));//unity scale
        var DistanceOnScreen = Vector3.Magnitude(screenpt1-screenpt)/unity_scale2d;       
        float documentScale = Mathf.Round((cameraScale/1.5f));
        scale_bar_text.text =  documentScale.ToString() + "nm";*/
    }

    public void changeColorAndOrderFromDepth(float zLevel, GameObject aprefab=null)
    {
        if (aprefab == null) aprefab = myPrefab;
        SpriteRenderer prefabSR = aprefab.GetComponent<SpriteRenderer>();
        float hue, S, V;
        Color.RGBToHSV(prefabSR.color, out hue, out S, out V);
        //ADAM : given a Z value can you find the colorValue ?
        //0.0083Z = 0.16fV
        //1Z = 0.16/
        //V = Z*0.0083/0.016
        //colorValue = (zLevel <= 0.0215f)? (colorValue - 0.016f): (colorValue + 0.016f);
        //V = 1.0f-(zLevel / 0.0083f )* 0.016f;
        //prefabSR.color = Color.HSVToRGB(hue, S, V);
        if (zLevel == 0.0004882813f)//need a way to detect the membrane and put it a sortingOrder 0
        {
            prefabSR.sortingOrder = 0; //
        }
        else if (zLevel <= 0.0215f)
        {
            prefabSR.sortingOrder = 1; 
        }
        else if (zLevel > 0.0215f && zLevel <= 0.083f)
        {
            prefabSR.sortingOrder = 0;
        }
        else if (zLevel > 0.083f && zLevel <= 0.16f)
        {
            prefabSR.sortingOrder = -1;
        }
        else
        {
            prefabSR.sortingOrder = -2;
        }
    }

    public void NucleicAcidDepth(GameObject toapplyto=null)
    {
        if (!fiberMode) return;
        //Debug.Log("Nucleic Acid Depth Active");

        //layerDirection decides if the next fiber prefab is going towards or away from 
        //the camera or staying at the same level.
        var layerDirectionf = UnityEngine.Random.Range(0, 3);

        //This portion of the code gets the color value of the prefab in 
        //HSV color space (We only modify the "V" or value in the following code).
        //prefabRGB = myPrefab.GetComponent<SpriteRenderer>().color;
        //float hue, S, V;
        //Color.RGBToHSV(prefabRGB, out hue, out S, out V);

        //Depending on the integer layerDirection provides this if loop changes the properties of the prefab to reflect the correct color value and layer properties (assuming 30 steps from top layer to bottom layer). 
        if (layerDirectionf == 0)
        {
            if (zLevel <= 0.26f)
            {
                zLevel = (zLevel + 0.0083f);
            }
            /*if (colorValue >= 0.5f)
            {
                colorValue = (colorValue - 0.016f);
            }*/
        }

        else if (layerDirectionf == 2)
        {
            if (zLevel >= 0.00f)
            {
                zLevel = (zLevel - 0.0083f);
            }
            /*if (colorValue <= 1.0f)
            {
                colorValue = (colorValue + 0.016f);
            }*/
        }

        else {

        };

        //Sets the color of the prefab. We need to go back from the HSV color space to the RGB color space.
        //Adam set RNA material.
        /*string prefabName = myPrefab.GetComponent<PrefabProperties>().name;
        SpriteRenderer sr = myPrefab.GetComponent<SpriteRenderer>();
        Debug.Log("The prefab name (For material application) is: " + prefabName);
        Debug.Log(" The shared material name is: " + prefab_materials[prefabName]);
        sr.sharedMaterial = prefab_materials[prefabName];*/

        //V = colorValue;
        //newRGB = Color.HSVToRGB(hue, S, V);
        //myPrefab.GetComponent<SpriteRenderer>().color = newRGB;

        //Sets the the Z-Axis position based on the new position.
        //myPrefab.transform.position= new Vector3 (transform.position.x, transform.position.y, zLevel);
        if (toapplyto){
            //Sets the new order in layer based on the Z-Axis Position.
            if (zLevel <= 0.0215f)
            {
                toapplyto.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }
            else if (zLevel > 0.0215f && zLevel <= 0.083f)
            {
                toapplyto.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }
            else if (zLevel >  0.083f && zLevel <= 0.16f)
            {
                toapplyto.GetComponent<SpriteRenderer>().sortingOrder = -1;
            }
            else
            {
                toapplyto.GetComponent<SpriteRenderer>().sortingOrder = -2;
            }           
        }
        /*if (current_prefab) //use to be myPrefab
        {
            current_prefab.transform.position= new Vector3 (transform.position.x, transform.position.y, zLevel);
            //Sets the new order in layer based on the Z-Axis Position.
            if (zLevel <= 0.0215f)
            {
                current_prefab.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }
            else if (zLevel > 0.0215f && zLevel <= 0.083f)
            {
                current_prefab.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }
            else if (zLevel >  0.083f && zLevel <= 0.16f)
            {
                current_prefab.GetComponent<SpriteRenderer>().sortingOrder = -1;
            }
            else
            {
                current_prefab.GetComponent<SpriteRenderer>().sortingOrder = -2;
            }
        }*/
    }

    public void NucleicAcidDepthLerp(GameObject toapplyto=null)
    {
        if (!fiberMode) return;
        //Debug.Log("Nucleic Acid Depth Active");

        //layerDirection decides if the next fiber prefab is going towards or away from 
        //the camera or staying at the same level.
        //roll a dice to change speed of interpolation?
        if (layerDirection) {
            zLevel = Mathf.Lerp(0.00f, 0.26f, lerp_time);
        }
        else zLevel = Mathf.Lerp(0.26f, 0.00f, lerp_time);
        lerp_time += 0.5f * Time.deltaTime;
        if (lerp_time > 1.0f)
        {
            lerp_time = 0.0f;
            layerDirection = !layerDirection;
        }
        if (toapplyto){
            //Sets the new order in layer based on the Z-Axis Position.
            if (zLevel <= 0.0215f)
            {
                toapplyto.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }
            else if (zLevel > 0.0215f && zLevel <= 0.083f)
            {
                toapplyto.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }
            else if (zLevel >  0.083f && zLevel <= 0.16f)
            {
                toapplyto.GetComponent<SpriteRenderer>().sortingOrder = -1;
            }
            else
            {
                toapplyto.GetComponent<SpriteRenderer>().sortingOrder = -2;
            }           
        }
    }


    public void change_scale(float val)
    {
        scale_force = val;
    }

    public string FindIdString(GameObject toFind) {
        Rigidbody2D rb = toFind.GetComponent<Rigidbody2D>();
        string prefix = "B";
        int id =  everything.IndexOf(rb);
        if (id == -1) {
            prefix = "S";
            id =  surface_objects.IndexOf(toFind);
            if (id == -1) {
                bool found = false;
                for (int i = 0; i <  fiber_parents.Count; i++)
                {
                    prefix = "F" + i.ToString() + "_";
                    //use Manager.Instance.fibers_instances
                    id = fibers_instances[i].IndexOf(toFind);
                    if (id != -1) {
                        found = true;
                        break;
                    }
                    if (found) break;
                }
            }
        }
        if (id == -1)
        {
            Debug.Log("NOT FOUND !");
            Debug.Log(toFind.name);
            Debug.Log("return empty String");
            return "";
        }
        else return prefix + id.ToString();
    }

    public GameObject FindObjectFromIdString(string toFind) {
        var elems = toFind.Split('_');
        //string prefix;
        int id;
        //GameObject ob;
        if (elems.Length == 2) {
            //prefix = elems[0].Substring(0,1);
            int fiberparentid = int.Parse(elems[0].Split('F')[1]);
            id = int.Parse(elems[1]);
            return fiber_parents[fiberparentid].transform.GetChild(id).gameObject;
        }
        else {
            
            if (toFind.StartsWith("B")) {
                id = int.Parse(toFind.Split('B')[1]);
                return everything[id].gameObject;
            }
            else if (toFind.StartsWith("S")) {
                id = int.Parse(toFind.Split('S')[1]);
                return surface_objects[id];
            }
        }
        return null;
    }
}
