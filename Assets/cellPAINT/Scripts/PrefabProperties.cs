using UnityEngine;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;

[Serializable]
public class PrefabProperties : MonoBehaviour
{
    
    public string name;
    public int id;
    public string common_name;
    public string function_group;
    public string compartment;
    public string description;
    public int order=0;//0 front, 1 middle, 2 bottom
    public int layer_number = 3;
    public bool isMultiChain;
    public List<GameObject> chains;
    public bool isCircleCollider;
    public float circle_radius;
    public Vector3 m_ext;
    public Vector2 circle_offset;
    public bool isCapsuleCollider;
    public Vector2 capsule_size;
    public Vector2 capsule_offset;
    public bool isHorizontal;
    public bool is_surface;
    public bool surface_secondLayer = true;
    public float surface_offset = 0.0f;
    public float y_offset = 0.0f;
    public float y_length = 0.0f;
    public float scale2d = 1.0f;
    public float local_scale = 1.0f;
    public Vector3 pcpAxe = Vector3.up;
    public Vector3 offset = Vector3.zero;
    public bool is_fiber;
    public bool fiber_Middle;
    public bool fiber_Bottom;
    public bool light_fiber=false;
    public bool closing = true;
    public bool is_Group = false;
    public bool is_anchor;
    public bool is_bound;
    public string bound_to;
    public bool is_connected;
    public string connected_to;
    public bool connection_occupied = false;
    public bool nucleic_acid_depth = false;
    public bool sprite_random_switch;
    public bool prefab_random_switch;
    public bool sprite_ordered_switch;
    public bool sprite_tumble_switch = false;
    public bool sprite_icosahedronRotation_switch = false;
    public int persistence_length = 0;
    public float persistence_strength = -1.0f;
    public bool fiber_hingeJoint_limits = false;
    public float hingeJoint_UPPERlimit = 15;
    public float hingeJoint_LOWERlimit = -15;
    public int number_placed;
    public bool setuped = false;
    public bool outline;
    public float outline_width = 12.0f;

    public List<Sprite> sprites_asset;
    public List<GameObject> prefab_asset;
    public List<Vector4> pcp;

    public Color Default_Sprite_Color;
    [ColorUsage(true, true)]//, 0f, 8f, 0.125f, 3f)]
    public Color outline_color = Color.yellow;
    [ColorUsage(true, true)]//, 0f, 8f, 0.125f, 3f)]
    public Color pin_color = Color.red;

    [HideInInspector]
    public Vector3 entry;
    [HideInInspector]
    public Vector3 exit;

    public float area;
    public bool ispined = false;
    public bool enableCollision = false;
    public int spriteTumble_id = 0;
    public int spriteRotation_id = 0;
    int prefab_id = 0;

    private int spriteOrdered_id = 0;
    private int[,] array2DIcosChoices;
    private SpriteRenderer spriteRenderer;
    public LineRenderer bound_lines;
    public GLDebug gLDebug;
    public float fiber_length = -1.0f;
    public float fiber_scale = 1.0f;
    public int maxNumber = 0;
    public float maxLength = 0.0f;
    private bool placed;

    public int gap_between_bound;
    public bool iterate_bound;

    public bool draw_background = false;
    public Material background_mat;

    public float zangle;
    public bool initialized = false; //so when we deserialized we dont redo some of the setup
    public Rigidbody2D RB;
    public List<SpringJoint2D> attachments;
    public int ghost_id = -1;
    private GameObject bicycleUp;
    private CircleCollider2D CircleUp;
    private CircleCollider2D CircleUp2;
    private FixedJoint2D hjup;
    private FixedJoint2D hjup2;
    private Rigidbody2D  CircleUpRb;
    private GameObject bicycleDown;
    private CircleCollider2D CircleDown;
    private CircleCollider2D CircleDown2;
    private FixedJoint2D hjdown;
    private FixedJoint2D hjdown2;
    private Rigidbody2D  CircleDownRb;
    private bool _checked = false;
    public bool debug_show_colliders = false;

    private ShowCollider showCollider;

    void SpriteTumble()
    {
        if (sprite_tumble_switch)
        {
            int tumbleChoice = UnityEngine.Random.Range(1, 3);
            if (tumbleChoice == 1)
            {
                spriteTumble_id = spriteTumble_id + 1;

            }
            else if (tumbleChoice == 2)
            {
                spriteTumble_id = spriteTumble_id - 1;

            }
            else
            {
            }
            if (spriteTumble_id >= sprites_asset.Count - 1)
            {
                spriteTumble_id = 0;
            }
            if (spriteTumble_id < 0)
            {
                spriteTumble_id = sprites_asset.Count - 1;
            }
            spriteRenderer.sprite = sprites_asset[spriteTumble_id];
        }
    }
    public void switchSpriteInOrder()
    {
        if (spriteOrdered_id >= sprites_asset.Count)
        {
            spriteOrdered_id = 0;
        }
        GetComponent<SpriteRenderer>().sprite = sprites_asset[spriteOrdered_id];
        spriteOrdered_id = spriteOrdered_id + 1;

    }

    public void switchSpriteRandomly()
    {
        int sprite_id = UnityEngine.Random.Range(0, sprites_asset.Count - 1);
        GetComponent<SpriteRenderer>().sprite = sprites_asset[sprite_id];
    }

    public void switchSprite(int sprite_id)
    {
        GetComponent<SpriteRenderer>().sprite = sprites_asset[sprite_id];
    }

    public void updateFiberPrefab()
    {
        CircleCollider2D[] allc = GetComponents<CircleCollider2D>();
        if (allc.Length < 2) return;
        Vector2 pos1 = new Vector2(transform.position.x, transform.position.y) + allc[0].offset * fiber_scale;
        Vector2 pos2 = new Vector2(transform.position.x, transform.position.y) + allc[allc.Length - 1].offset * fiber_scale;
        fiber_length = Vector2.Distance(pos1, pos2);
    }

    void setupCollider() {
        return;
    }

    public void Initialized() {
        //happen after instnatiate
        /* all the follwoing should be in the prefabProperties start*/
        if (initialized) return;
        gameObject.name = name + " (Top)";
        gameObject.transform.parent = Manager.Instance.root.transform;
        gameObject.layer = LayerMask.NameToLayer("Top Layer");
        if (gameObject.GetComponent<Renderer>().sharedMaterial == null)
        {
            gameObject.GetComponent<Renderer>().sharedMaterial = Manager.Instance.prefab_materials[name];
        }

        //Puts appropriate collider and settings based on prefab properties.
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            rb.angularDrag = 20.0f;
            rb.drag = 20.0f;
            rb.sleepMode = RigidbodySleepMode2D.StartAsleep;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            //Add Rigidbody2D to loop and count.
            if (!Manager.Instance.everything.Contains(rb))
            {
                Manager.Instance.everything.Add(rb);//[Manager.Instance.rbCount] = rb;
                Manager.Instance.rbCount++;
                Manager.Instance.UpdateCountAndLabel(name, gameObject);
            }
            RB = rb;
        }

        setupCollider();

        initialized = true;
    }

    public void TestPcpalAxis() { }

    public Bounds TestPcpalAxis2D_surface(GameObject bicycle, Vector2[] pts, bool do_colliders = true)
    {
        var gameotouse = bicycle;
        if ( gameotouse == null) {
            gameotouse = gameObject;
        }
        float width = 0.0f;
        //get the max
        int imax = -1;
        float maximum = 0.0f;
        List<int> order = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            if (pcp[i].w > maximum)
            {
                maximum = pcp[i].w;
                imax = i;
                order.Insert(0, imax);
            }
            else
                order.Add(i);
        }
        Quaternion mrot = new Quaternion(pcp[2].x, pcp[2].y, pcp[2].z, pcp[2].w);
        Vector3 center = (pcp[0] + pcp[1]) * 0.5f;
        Vector3 position = mrot * center;
        m_ext =  (pcp[0] - pcp[1]);//align to the pcpal Axis//should be absolute!
        var sr = GetComponent<SpriteRenderer>();
        bool flip = (sr.size.x > sr.size.y);
        if (flip) m_ext = new Vector2(m_ext.y,m_ext.x);

        var unity_scale2d = 1.0f / (Manager.Instance.unit_scale * 10.0f);
        float delta = Manager.Instance.membrane_thickness_delta;
        float mthickness = Manager.Instance.membrane_thickness;
        float thickness = (delta*2.0f + mthickness) * unity_scale2d * 1.0f/local_scale;//(23.0f) 
        //surface offset is +/-
        float up_height = 0.0f;
        float down_height = 0.0f;
        float e = Mathf.Abs(m_ext.y)/2.0f;
        float s = Mathf.Abs(surface_offset);
        float y1 = 0.0f;
        float y2 = 0.0f;
        if (surface_offset <= 0) {
            up_height = Mathf.Abs(e + (s - thickness));  y1 = -s + thickness + up_height/2.0f;
            down_height = Mathf.Abs(e - (s + thickness));y2 = -s - thickness - down_height/2.0f;
        }
        else {
            down_height = Mathf.Abs(e + (s - thickness)); y2 = s - thickness - down_height/2.0f;
            up_height = Mathf.Abs(e - (s + thickness));     y1 = s + thickness + up_height/2.0f;
        }
        if (Mathf.Abs(up_height) < 0.1f) up_height = 0.0f;
        if (Mathf.Abs(down_height) < 0.1f) down_height = 0.0f;
        Vector2 box1 = new Vector2(m_ext.x,Mathf.Abs(up_height));//pos will be thickness + height/2
        Vector2 box2 = new Vector2(m_ext.x,Mathf.Abs(down_height));
        center = mrot * center;
        Bounds bup = new Bounds(new Vector3(0, surface_offset+thickness+up_height/2.0f,0),
                              new Vector3(Mathf.Abs(box1.x), up_height,10)  );
        Bounds bdown = new Bounds(new Vector3(0, surface_offset-thickness-down_height/2.0f,0),
                              new Vector3(Mathf.Abs(box2.x), down_height,10)  );
        Bounds bup2 = new Bounds();
        Bounds bdown2 = new Bounds();
        Bounds mid = new Bounds();
        float widthUp = 0.0f;
        float widthDown = 0.0f;
        float widthMiddle = 0.0f;
        foreach (var p in pts) {
            var dtoce = Mathf.Abs(p.x)*2.0f; 
            if (bup.Contains(p)){
                widthUp = Mathf.Max(dtoce,widthUp);
                bup2.Encapsulate(p);
            }
            else if (bdown.Contains(p)){
                widthDown = Mathf.Max(dtoce,widthDown);
                bdown2.Encapsulate(p);
            } else {
                widthMiddle = Mathf.Max(dtoce,widthMiddle);
                mid.Encapsulate(p);
            }
        }

        if (!do_colliders) return mid;
        if (up_height > 0.1f) {
            if (Mathf.Abs(box1.x - box1.y) < 1.15f) {
                CircleCollider2D Circle = gameotouse.AddComponent<CircleCollider2D>();
                Circle.radius = widthUp / 2.0f;
                Circle.offset = new Vector2(bup2.center.x, surface_offset+thickness+up_height/2.0f);
            }
            else {
                BoxCollider2D box = gameotouse.AddComponent<BoxCollider2D>();
                box.size = new Vector2(widthUp, up_height);
                box.offset = new Vector2(bup2.center.x, surface_offset+thickness+up_height/2.0f);
            }
        }
        if (down_height > 0.1f){ 
            if (Mathf.Abs(box2.x - box2.y) < 1.15f) {
                CircleCollider2D Circle = gameotouse.AddComponent<CircleCollider2D>();
                Circle.radius = widthDown / 2.0f;
                Circle.offset = new Vector2(bdown2.center.x, surface_offset-thickness-down_height/2.0f);
            }
            else {
                BoxCollider2D box = gameotouse.AddComponent<BoxCollider2D>();
                box.size = new Vector2(widthDown, down_height);
                box.offset = new Vector2(bdown2.center.x, surface_offset-thickness-down_height/2.0f);
            }
        }
        if (Mathf.Abs(m_ext.x - m_ext.y) < 1.15f) {
            width = m_ext.x/ 2.0f;
        }
        else
        {
            width = Mathf.Abs(m_ext.x)/2.0f;
        }
        circle_radius = width;
        return mid;
    }

    public float TestPcpalAxis2D()
    {
        float width = 0.0f;
        //get the max
        int imax = -1;
        float maximum = 0.0f;
        List<int> order = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            if (pcp[i].w > maximum)
            {
                maximum = pcp[i].w;
                imax = i;
                order.Insert(0, imax);
            }
            else
                order.Add(i);
        }
        Quaternion mrot = new Quaternion(pcp[2].x, pcp[2].y, pcp[2].z, pcp[2].w);
        Vector3 center = (pcp[0] + pcp[1]) * 0.5f;
        Vector3 position = mrot * center;
        m_ext =  (pcp[0] - pcp[1]); //align to the pcpal Axis
        Debug.Log(m_ext.ToString());
        Debug.Log((Quaternion.Inverse(mrot) * m_ext).ToString());
        Debug.Log((mrot * m_ext).ToString());
        var sr = GetComponent<SpriteRenderer>();
        bool flip = (sr.size.x > sr.size.y);

        center = mrot * center;
        if (Mathf.Abs(m_ext.x - m_ext.y) < 1.15f) {
            CircleCollider2D Circle = gameObject.AddComponent<CircleCollider2D>();
            Circle.radius = m_ext.x/ 2.0f;
            Circle.offset = new Vector2(center.x, center.y);
            width = Circle.radius;
        }
        else
        {
            if (flip) m_ext = new Vector2(m_ext.y,m_ext.x);
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(Mathf.Abs(m_ext.x), Mathf.Abs(m_ext.y));
            box.offset = new Vector2(center.x, center.y);
            width =  box.size.x/2.0f;
        }
        circle_radius = width;
        return width;
    }

    public void UpdateScale2D(float new_value){
        scale2d = new_value;

    }

    public void SetupBicycle(){
        Vector3 center = (pcp[0] + pcp[1]) * 0.5f;
        var unity_scale2d = 1.0f / (Manager.Instance.unit_scale * 10.0f);
        //instead of 2 circle up and down, try 2 box up and down. bow width == ingredient radius
        float thickness = (84.0f / 4.0f) * unity_scale2d;//42 angstrom
        float width = TestPcpalAxis2D();
        float bicycle_radius = width*1.0f/local_scale;
        bicycleUp = new GameObject("up");
        bicycleUp.layer = 15;
        bicycleUp.transform.parent = gameObject.transform;
        bicycleUp.transform.localPosition = new Vector3(center.y, center.z + thickness + bicycle_radius + surface_offset, 0.0f);
        CircleUp = bicycleUp.AddComponent<CircleCollider2D>();
        CircleUp.radius = bicycle_radius;
        CircleUp.offset = new Vector2(bicycle_radius,0);
        CircleUp2 = bicycleUp.AddComponent<CircleCollider2D>();
        CircleUp2.radius = bicycle_radius;
        CircleUp2.offset = new Vector2(-bicycle_radius,0);
        CircleUpRb = bicycleUp.AddComponent<Rigidbody2D>();
        //add the joint FixedJoint2D or Hinge
        hjup = bicycleUp.AddComponent<FixedJoint2D>();
        hjup.autoConfigureConnectedAnchor = false;
        hjup.connectedBody = RB;
        hjup.anchor = new Vector2(-bicycle_radius, 0);
        hjup.connectedAnchor = new Vector2(-bicycle_radius, center.z + thickness + bicycle_radius + surface_offset);
        hjup2 = bicycleUp.AddComponent<FixedJoint2D>();
        hjup2.autoConfigureConnectedAnchor = false;
        hjup2.connectedBody = RB;
        hjup2.anchor = new Vector2(bicycle_radius, 0);
        hjup2.connectedAnchor = new Vector2(bicycle_radius, center.z + thickness + bicycle_radius+ surface_offset);
        bicycleDown = new GameObject("down");
        bicycleDown.layer = 15;
        bicycleDown.transform.parent = gameObject.transform;
        bicycleDown.transform.localPosition = new Vector3(center.y, center.z - thickness - bicycle_radius + surface_offset, 0.0f);
        CircleDown = bicycleDown.AddComponent<CircleCollider2D>();
        CircleDown.radius = bicycle_radius;
        CircleDown.offset = new Vector2(bicycle_radius,0);
        CircleDown2 = bicycleDown.AddComponent<CircleCollider2D>();
        CircleDown2.radius = bicycle_radius;
        CircleDown2.offset = new Vector2(-bicycle_radius,0);
        CircleDownRb = bicycleDown.AddComponent<Rigidbody2D>();
        //add the joint
        hjdown = bicycleDown.AddComponent<FixedJoint2D>();
        hjdown.autoConfigureConnectedAnchor = false;
        hjdown.connectedBody = RB;
        hjdown.anchor = new Vector2(-bicycle_radius, 0);
        hjdown.connectedAnchor = new Vector2(-bicycle_radius, center.z - thickness -bicycle_radius + surface_offset);
        hjdown2 = bicycleDown.AddComponent<FixedJoint2D>();
        hjdown2.autoConfigureConnectedAnchor = false;
        hjdown2.connectedBody = RB;
        hjdown2.anchor = new Vector2(bicycle_radius, 0);
        hjdown2.connectedAnchor = new Vector2(bicycle_radius, center.z - thickness -bicycle_radius + surface_offset);
    }


    public void SetupBicycleNew(Vector2[] pts){
        float delta = Manager.Instance.membrane_thickness_delta;
        float mthickness = Manager.Instance.membrane_thickness;
        Vector3 center = (pcp[0] + pcp[1]) * 0.5f;
        var unity_scale2d = 1.0f / (Manager.Instance.unit_scale * 10.0f);
        //instead of 2 circle up and down, try 2 box up and down. bow width == ingredient radius
        float thickness = (delta + mthickness/2.0f) * unity_scale2d * 1.0f/local_scale;//(23.0f) 
        //the width should come from the extend of the membrane area

        bicycleUp = new GameObject("bicycle");
        bicycleUp.layer = 15;
        bicycleUp.transform.parent = gameObject.transform;
        bicycleUp.transform.localPosition = new Vector3(0,0, 0);
        
        Bounds mid = TestPcpalAxis2D_surface(bicycleUp,pts,true);
        float width = mid.extents.x/2.0f;
        float mx = 0;
        float bicycle_radius = thickness;

        CircleUp = bicycleUp.AddComponent<CircleCollider2D>();
        CircleUp.radius = thickness;
        CircleUp.offset = new Vector2(mx+width,  thickness + thickness + surface_offset);//-thickness
        CircleUp2 = bicycleUp.AddComponent<CircleCollider2D>();
        CircleUp2.radius = bicycle_radius;
        CircleUp2.offset = new Vector2(mx-width,thickness + thickness + surface_offset);//+thickness
        
        CircleDown = bicycleUp.AddComponent<CircleCollider2D>();
        CircleDown.radius = bicycle_radius;
        CircleDown.offset = new Vector2(mx+width, - thickness - thickness + surface_offset);//-thickness
        CircleDown2 = bicycleUp.AddComponent<CircleCollider2D>();
        CircleDown2.radius = bicycle_radius;
        CircleDown2.offset = new Vector2(mx-width, - thickness - thickness + surface_offset);
        TestPcpalAxis2D();
    }

    public void SetupBoxCycle(){
        Vector3 center = (pcp[0] + pcp[1]) * 0.5f;
        var unity_scale2d = 1.0f / (Manager.Instance.unit_scale * 10.0f);
        //instead of 2 circle up and down, try 2 box up and down. bow width == ingredient radius
        float thickness = (23.0f) * unity_scale2d;
        float width = TestPcpalAxis2D();
        float height = thickness;//bradius*1.0f/local_scale;
        bicycleUp = new GameObject("up");
        bicycleUp.layer = 15;
        bicycleUp.transform.parent = gameObject.transform;

        bicycleUp.transform.localPosition = new Vector3(center.y, center.z + thickness + height + surface_offset, 0.0f);
        BoxCollider2D box_up = bicycleUp.AddComponent<BoxCollider2D>();
        box_up.size = new Vector2(width*2.0f, height*2.0f);
        box_up.offset = new Vector2(0, 0);
        hjup = bicycleUp.AddComponent<FixedJoint2D>();
        hjup.autoConfigureConnectedAnchor = false;
        hjup.connectedBody = RB;
        hjup.anchor = new Vector2(-width, 0);
        hjup.connectedAnchor = new Vector2(-width, center.z + thickness + height + surface_offset);
        hjup2 = bicycleUp.AddComponent<FixedJoint2D>();
        hjup2.autoConfigureConnectedAnchor = false;
        hjup2.connectedBody = RB;
        hjup2.anchor = new Vector2(width, 0);
        hjup2.connectedAnchor = new Vector2(width, center.z + thickness + height+ surface_offset);

        bicycleDown = new GameObject("down");
        bicycleDown.layer = 15;
        bicycleDown.transform.parent = gameObject.transform;
        bicycleDown.transform.localPosition = new Vector3(center.y, center.z - thickness - height + surface_offset, 0.0f);
        BoxCollider2D box_down = bicycleDown.AddComponent<BoxCollider2D>();
        box_down.size = new Vector2(width*2.0f, height*2.0f);
        box_down.offset = new Vector2(0, 0);
        hjdown = bicycleDown.AddComponent<FixedJoint2D>();
        hjdown.autoConfigureConnectedAnchor = false;
        hjdown.connectedBody = RB;
        hjdown.anchor = new Vector2(-width, 0);
        hjdown.connectedAnchor = new Vector2(-width, center.z - thickness -height + surface_offset);
        hjdown2 = bicycleDown.AddComponent<FixedJoint2D>();
        hjdown2.autoConfigureConnectedAnchor = false;
        hjdown2.connectedBody = RB;
        hjdown2.anchor = new Vector2(width, 0);
        hjdown2.connectedAnchor = new Vector2(width, center.z - thickness -height + surface_offset);        
    }

    public void SetupBoxCycleNew(){
        Vector3 center = (pcp[0] + pcp[1]) * 0.5f;
        var unity_scale2d = 1.0f / (Manager.Instance.unit_scale * 10.0f);
        //instead of 2 circle up and down, try 2 box up and down. bow width == ingredient radius
        float thickness = (23.0f) * unity_scale2d * 1.0f/local_scale;
        float width = TestPcpalAxis2D();
        float height = thickness;
        bicycleUp = new GameObject("up");
        bicycleUp.layer = 15;
        bicycleUp.transform.parent = gameObject.transform;
        bicycleUp.transform.localPosition = new Vector3(0,0, 0);
        BoxCollider2D box_up = bicycleUp.AddComponent<BoxCollider2D>();
        box_up.size = new Vector2(width*2.0f, height*2.0f);
        box_up.offset = new Vector2(0,  thickness + height + surface_offset);
        BoxCollider2D box_down = bicycleUp.AddComponent<BoxCollider2D>();
        box_down.size = new Vector2(width*2.0f, height*2.0f);
        box_down.offset = new Vector2(0,  - thickness - height + surface_offset);
    }

    public void SetupFromNode() {
        //setup color ?
        if (setuped) return;
        if (!Manager.Instance.ingredient_node.ContainsKey(name)) return;
        var rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        RB=rb;
        JSONNode node = Manager.Instance.ingredient_node[name];
        is_fiber = (node["Type"].Value == "Grow");
        is_surface = (node["surface"].Value == "surface") ? true : false;
        Collider2D[] coll = GetComponents<Collider2D>();
        foreach (var col in coll) DestroyImmediate(col);
        PolygonCollider2D box = gameObject.GetComponent<PolygonCollider2D>();
        if (box == null) box = gameObject.AddComponent<PolygonCollider2D>();
        Vector2[] pts = box.points;//is this getting all points ?
        List<Vector2> plist = new List<Vector2>();
        for (int i=0; i < box.pathCount;i++) {
            plist.AddRange(new List<Vector2>(box.GetPath(i)));
        }
        DestroyImmediate(box);
        pcp = Helper.BuildOBB2D(plist.ToArray(), 0.0f);
        Debug.Log("pcp"); Debug.Log(pcp);
        //any sprite to unity sprite scale is 
        var pixel_scale = (Manager.Instance.unit_scale * 10.0f) / 100.0f;
        var unity_scale2d = 1.0f / (Manager.Instance.unit_scale * 10.0f);
        //1 unit scale is 3.5nm e.g. Manager.Instance.unit_scale. Divide by (uscale*10.0) to get unity unit.
        //sprite is scale2d 
        scale2d = 1.0f;
        y_offset = 0.0f;
        y_length = -1.0f;
        if (node.HasKey("sprite"))
        {
            scale2d = float.Parse(node["sprite"]["scale2d"]);//1 angstrom in the image is scale2d pixel
            //check if its 0.0
            if (scale2d == 0.0) scale2d = 1.0f;
            y_offset = float.Parse(node["sprite"]["offsety"]);
            if (node["sprite"].HasKey("lengthy")) {
                y_length= float.Parse(node["sprite"]["lengthy"]) ;
                if (y_length == 0.0f) y_length = -1.0f;
            }
        }
        surface_offset = -y_offset * unity_scale2d;
        local_scale = 1.0f/(pixel_scale * scale2d);
        surface_offset=(surface_offset/local_scale);
        Vector3 center = (pcp[0] + pcp[1]) * 0.5f;
        Vector3 m_ext = (pcp[0] - pcp[1]);
        Quaternion mrot = new Quaternion(pcp[2].x, pcp[2].y, pcp[2].z, pcp[2].w);
        if (!is_fiber && is_surface)
        {
            //split the point between up and down
            //use the split point BB to create the colliders
            //also need to split the collider to not overlap with the membrane.
            gameObject.layer = 12;
            SetupBicycleNew(pts);
        }
        else if (is_fiber) {
            //persistence length ?
            persistence_length = 1;//nb of spring
            persistence_strength = 5.0f;
            fiber_scale = local_scale;
            //setup the collider
            center = mrot * center;
            float radius = (Mathf.Abs(m_ext.y) / 2.0f)/2.0f;
            m_ext = Quaternion.Inverse(mrot) * new Vector2(Mathf.Abs(m_ext.x), Mathf.Abs(m_ext.y));
            BoxCollider2D abox = gameObject.AddComponent<BoxCollider2D>();
            abox.size = new Vector2(Mathf.Abs(m_ext.x), Mathf.Abs(m_ext.y));
            abox.offset = new Vector2(center.x, center.y);
            
            float lengthy = (y_length==-1.0f)?Mathf.Abs(m_ext.x):y_length* unity_scale2d/local_scale;
            if (y_length==-1.0f) y_length = Mathf.Abs(m_ext.x)/(unity_scale2d/local_scale);
            //anchor collider, on X axis.
            CircleCollider2D CircleLeft = gameObject.AddComponent<CircleCollider2D>();
            CircleLeft.radius = radius;
            CircleLeft.offset = new Vector2(-lengthy/2.0f , center.y);
            CircleCollider2D CircleRight = gameObject.AddComponent<CircleCollider2D>();
            CircleRight.radius = radius;
            CircleRight.offset = new Vector2(lengthy / 2.0f , center.y);
            gameObject.layer = 13; //DNA. should we use nucleic acid depth ?
            if (node.HasKey("closed")) 
            {
                closing = bool.Parse(node["closed"].Value);
            }
        }
        else
        {
            TestPcpalAxis2D();
        }
        
        transform.localScale = new Vector3(local_scale, local_scale, local_scale);
        setuped = true;
        surface_offset=surface_offset*local_scale;

    }

    public void SetupFromValues(bool issurface, bool isfiber, float ascale2d, float offsety, float alength, bool isclosing = false ) {
        //setup color ?
        Debug.Log("SetupFromValues "+ascale2d.ToString()+" "+offsety.ToString());
        if (setuped) return;
        var rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        RB=rb;
        is_fiber = isfiber;
        is_surface = issurface;
        Collider2D[] coll = GetComponents<Collider2D>();
        foreach (var col in coll) DestroyImmediate(col);
        PolygonCollider2D box = gameObject.GetComponent<PolygonCollider2D>();
        if (box == null) box = gameObject.AddComponent<PolygonCollider2D>();
        Vector2[] pts = box.points;
        List<Vector2> plist = new List<Vector2>();
        for (int i=0; i < box.pathCount;i++) {
            plist.AddRange(new List<Vector2>(box.GetPath(i)));
        }
        DestroyImmediate(box);
        pcp = Helper.BuildOBB2D(plist.ToArray(), 0.0f);
        Debug.Log("pcp"); Debug.Log(pcp);
        var pixel_scale = (Manager.Instance.unit_scale * 10.0f) / 100.0f;
        var unity_scale2d = 1.0f / (Manager.Instance.unit_scale * 10.0f);
        //1 unit scale is 3.5nm e.g. Manager.Instance.unit_scale. Divide by (uscale*10.0) to get unity unit.
        //sprite is scale2d 
        y_offset = offsety;
        scale2d = ascale2d;
        y_length = alength;
        surface_offset = -y_offset * unity_scale2d;
        local_scale = 1.0f/(pixel_scale * scale2d);
        surface_offset=(surface_offset/local_scale);
        Vector3 center = (pcp[0] + pcp[1]) * 0.5f;
        Vector3 m_ext = (pcp[0] - pcp[1]);
        Quaternion mrot = new Quaternion(pcp[2].x, pcp[2].y, pcp[2].z, pcp[2].w);
        if (!is_fiber && is_surface)
        {
            gameObject.layer = 12;
            SetupBicycleNew(pts);
        }
        else if (is_fiber) {
            //persistence length ?
            persistence_length = 1;//nb of spring
            persistence_strength = 5.0f;
            fiber_scale = local_scale;
            //setup the collider
            center = mrot * center;
            float radius = (Mathf.Abs(m_ext.y) / 2.0f)/2.0f;
            m_ext = Quaternion.Inverse(mrot) * new Vector2(Mathf.Abs(m_ext.x), Mathf.Abs(m_ext.y));
            BoxCollider2D abox = gameObject.AddComponent<BoxCollider2D>();
            abox.size = new Vector2(Mathf.Abs(m_ext.x), Mathf.Abs(m_ext.y));
            abox.offset = new Vector2(center.x, center.y);
            //anchor collider, on X axis. depends on fiber_length
            float lengthy = y_length * unity_scale2d * 1.0f/local_scale;
            CircleCollider2D CircleLeft = gameObject.AddComponent<CircleCollider2D>();
            CircleLeft.radius = radius;
            CircleLeft.offset = new Vector2(-lengthy/2.0f, center.y);
            CircleCollider2D CircleRight = gameObject.AddComponent<CircleCollider2D>();
            CircleRight.radius = radius;
            CircleRight.offset = new Vector2(lengthy/2.0f, center.y);
            gameObject.layer = 13; //DNA. should we use nucleic acid depth ?
            closing = isclosing;
        }
        else
        {
            TestPcpalAxis2D();
        }
        
        transform.localScale = new Vector3(local_scale, local_scale, local_scale);
        setuped = true;
        surface_offset=surface_offset*local_scale;
    }

    public void Setup(bool special = false) {
        if (is_Group) return;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer.sprite == null && sprites_asset.Count!=0) {
            spriteRenderer.sprite = sprites_asset[0];
        }
        SetupFromNode();
        area = getArea();
        UpdateOutline(false);
        Debug.Log("update " + name);
        var p = gameObject.transform.parent;
        if ((name == null) || (name == ""))
        {
            if (gameObject.name.Contains("Clone"))
            {
                order = 0;
                name = gameObject.name.Replace("(Clone)", "");
            }
            else if (gameObject.name.Contains("Middle"))
            {
                order = 1;
                if (p!=null)
                    name = gameObject.transform.parent.name.Replace("(Clone)", "");
            }
            else if (gameObject.name.Contains("Bottom"))
            {
                order = 2;
                if (gameObject.transform.parent.childCount < 2)
                    order = 1;
                if (p != null) name = gameObject.transform.parent.name.Replace("(Clone)", "");
            }
            else {
                if (special)
                {
                    order = 0;
                    name = gameObject.name;
                }
                else {
                    order = gameObject.transform.GetSiblingIndex() + 1;
                    if (p != null) name = gameObject.transform.parent.name.Replace("(Clone)", "");
                }
            }
        }
        if (is_bound)
        {
            Collider2D[] coll = GetComponents<Collider2D>();
        }
        if (is_fiber)
        {
            updateFiberPrefab();
        }
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        UpdateOutline(false);
    }

    // Use this for initialization
    void Start()
    {

        int[,] array2DIcosChoices = new int[12, 5] { { 1, 2, 7, 8, 9 }, { 0, 2, 7, 10, 11 }, { 0, 1, 3, 8, 11 }, { 2, 4, 5, 8, 11 }, { 3, 5, 6, 9, 10 }, { 3, 4, 6, 10, 11 }, { 4, 5, 7, 9, 10 }, { 0, 1, 6, 9, 10 }, { 0, 2, 3, 4, 9 }, { 0, 4, 6, 7, 8 }, { 1, 5, 6, 7, 11 }, { 1, 2, 3, 5, 10 } };
        area = getArea();
        if (is_fiber)
        {
            updateFiberPrefab();
        }
        Setup();
        _checked = false;
        attachments = new List<SpringJoint2D>();
        var rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        RB=rb;
    }

    public void setCheck(bool value) {
        _checked = value;
    }

    public void checkFiberConnection() {
        //should be done on undo not creation
        if (_checked) return;
        //reconect to preivous child if not
        if (is_fiber)
        {
            //if no parent return
            if (transform.parent == null) return;
            if (gameObject.GetComponentInParent<Manager>() != null) return;
            //check if hinge and spring along persitence
            int ipos = transform.GetSiblingIndex();
            var pos = transform.position;
            var rotation = transform.rotation;
            int n = transform.parent.childCount;
            if (ipos > 0)
            {
                //chack that previous is connected to himself
                var previous = transform.parent.GetChild(ipos - 1);
                var hinge = previous.GetComponent<HingeJoint2D>();
                if (hinge.connectedBody != gameObject.GetComponent<Rigidbody2D>())
                {
                    hinge.connectedBody = gameObject.GetComponent<Rigidbody2D>();
                    CircleCollider2D[] other_allc = gameObject.GetComponents<CircleCollider2D>();
                    CircleCollider2D[] current_allc = previous.GetComponents<CircleCollider2D>();
                    hinge.anchor = other_allc[1].offset;
                    hinge.connectedAnchor = current_allc[0].offset;
                }
            }
            else
            {
                //check if parent closed
                var previous = transform.parent.GetChild(n - 1);
                var hinge = previous.GetComponent<HingeJoint2D>();
                if (transform.parent.name.Contains("_Closed"))
                {
                    if (hinge.connectedBody != gameObject.GetComponent<Rigidbody2D>())
                    {
                        hinge.connectedBody = gameObject.GetComponent<Rigidbody2D>();
                        CircleCollider2D[] other_allc = gameObject.GetComponents<CircleCollider2D>();
                        CircleCollider2D[] current_allc = previous.GetComponents<CircleCollider2D>();
                        hinge.anchor = other_allc[1].offset;
                        hinge.connectedAnchor = current_allc[0].offset;
                    }
                }
            }
            
            SpringJoint2D[] sjts = transform.GetComponents<SpringJoint2D>();//only one ?
            for (int j = 1; j <= sjts.Length; j++)
            {
                Transform ch;
                if ((ipos + j) >= n)//last
                {
                    if (transform.parent.name.Contains("_Closed"))
                    {
                        if ((n - (ipos +j)) < 0) continue;
                        ch = transform.parent.GetChild(n - (ipos + j));
                    }
                    else continue;
                }
                else {
                    ch = transform.parent.GetChild(ipos + j);
                }
                SpringJoint2D spring = sjts[j - 1];
                spring.connectedBody = ch.GetComponent<Rigidbody2D>();
                spring.enableCollision = enableCollision;
                spring.autoConfigureDistance = false;
                spring.distance = fiber_length * (j);
                spring.anchor = Vector2.zero;
                spring.connectedAnchor = Vector2.zero;
                spring.frequency = (persistence_strength != -1.0f)? persistence_strength :  10.0f / ((j + 2) / 2.0f);
            spring.dampingRatio = 0.5f;
            }
            transform.position = pos;
            transform.rotation = rotation;
        }
        _checked = true;
    }

    public float getArea()
    {
        Component[] circleColliders;
        Component[] boxColliders;
        circleColliders = GetComponents(typeof(CircleCollider2D));
        boxColliders = GetComponents(typeof(BoxCollider2D));
        var R = 0.0f;
        if (circleColliders != null)
        {
            foreach (CircleCollider2D collider2D in circleColliders)
            {
                area += Mathf.PI * (collider2D.radius * collider2D.radius);
                R += collider2D.radius;
            }
        }
        if (boxColliders != null)
        {
            foreach (BoxCollider2D collider2D in boxColliders)
                area += (collider2D.size.x) * (collider2D.size.y);
        }
        if (circle_radius == 0.0f)
            circle_radius = Mathf.Sqrt(area / Mathf.PI);
        return area;
    }

    public void UpdateOutline(bool is_outline)
    {
        //if (ispined) return;
        if (spriteRenderer == null) return;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_Glow", is_outline ? 1f : 0f);
        spriteRenderer.SetPropertyBlock(mpb);
    }

    public void UpdateOutlinePin(bool toggle)
    {
        ispined = toggle;
        if (spriteRenderer == null) return;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_Outline", toggle ? outline_width : 0);
        mpb.SetColor("_OutlineColor", pin_color);
        spriteRenderer.SetPropertyBlock(mpb);
    }

    public void alignFiber(GameObject start, GameObject end)
    {
        Vector3 entry_to_align = transform.TransformDirection(entry);
        Vector3 exit_to_align = transform.TransformDirection(exit);
        //start align to entry

        Quaternion q = Quaternion.FromToRotation(Vector3.right, entry_to_align);
        start.GetComponent<Rigidbody2D>().rotation = q.eulerAngles.z;
        //end align to exit
        Quaternion q1 = Quaternion.FromToRotation(Vector3.right, exit_to_align);
        end.GetComponent<Rigidbody2D>().rotation = -q.eulerAngles.z;
    }

    private void fixPersistenseOnRemove(int elemRemoved, GameObject chain_parent, int persistence_length)
    {
        //clean the persitence spring according the reordering after deleting one element.
        //need to go back ward and remove the overlapping spring
        for (int i = 0; i < persistence_length; i++)
        {
            //remove the joins that goes over the binding ?
            GameObject elem = chain_parent.transform.GetChild(elemRemoved - i).gameObject;
            foreach (SpringJoint2D sjt in elem.GetComponents<SpringJoint2D>())
            {
                if (sjt.connectedBody.transform.GetSiblingIndex() > elemRemoved)
                {
                    Destroy(sjt);
                }
            }
        }
    }

    public void AttachToPartnerFiber(GameObject partner)
    {


        //get the partner parent which is the chain
        GameObject chain_parent = partner.transform.parent.gameObject;
        int chain_index = partner.transform.GetSiblingIndex();

        if (iterate_bound)
        {
            IterateAlongChain(partner, chain_parent, chain_index);
        }

        //find the closet chain index number to the gap length of the prefab.
        int searchScope = chain_index + gap_between_bound;
        if (gap_between_bound != 0 && !iterate_bound)
        {
            IterateSingleAttachment(chain_index, chain_parent, searchScope);
            GameObject test = chain_parent.transform.GetChild(chain_index - 1).gameObject;
            Rigidbody2D testRigidbody = test.GetComponent<HingeJoint2D>().connectedBody;
            string testName = testRigidbody.transform.name;

            if (testName == "HIV_NC_1f6u(Clone)")
            {
                searchScope = searchScope + gap_between_bound;
                IterateSingleAttachment(chain_index, chain_parent, searchScope);
            }
        }
    }

    public void ConnectToPartnerFiber(GameObject partner)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        PrefabProperties props = partner.GetComponent<PrefabProperties>();

    }

    public void ConnectToPartnerFiber(GameObject partner, GameObject chain_parent,int chain_index)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        PrefabProperties props = partner.GetComponent<PrefabProperties>();

        //Get the first collider which are the anchor
        CircleCollider2D[] coll = GetComponents<CircleCollider2D>();
        CircleCollider2D[] coll_fiber = partner.GetComponents<CircleCollider2D>();

        //Get Neighboors and change the partner gameobject to the modulous=0 positioning.
        partner = chain_parent.transform.GetChild(chain_index).gameObject;
        GameObject start = chain_parent.transform.GetChild(chain_index - 1).gameObject;
        GameObject end = chain_parent.transform.GetChild(chain_index + 1).gameObject;
        //we remove the current chain_index +/- size ?
        //should destroy any spring joints connected to it

        //Use properties of the start object (color value, order in layer, and z Depth) to align and color bound object correctly.
        Vector3 ChainZ = start.transform.position;
        int startLayerOrder = start.GetComponent<SpriteRenderer>().sortingOrder;

        GetComponent<SpriteRenderer>().sortingOrder = startLayerOrder + 1;
        transform.position = new Vector3(transform.position.x, transform.position.y, start.transform.position.z);
        float zLevel = start.transform.position.z;

        if (zLevel <= 0.083f)
        {
            gameObject.layer = 8;
        }
        else if (zLevel > 0.083f && zLevel <= 0.16f)
        {
            gameObject.layer = 9;
        }
        else
        {
            gameObject.layer = 10;
        }

        foreach (var jts in chain_parent.GetComponentsInChildren<SpringJoint2D>())
        {
            if (jts.connectedBody == partner.GetComponent<Rigidbody2D>())
                Destroy(jts);
        }

        fixPersistenseOnRemove(chain_index, chain_parent, props.persistence_length);

        Destroy(partner);

        HingeJoint2D jt = start.GetComponent<HingeJoint2D>();
        if (!jt)
        {
            jt = start.AddComponent<HingeJoint2D>();
            jt.enableCollision = false;
            jt.autoConfigureConnectedAnchor = false;
            jt.useLimits = true;
        }
        jt.connectedAnchor = coll[1].offset * fiber_scale;
        JointAngleLimits2D limits = jt.limits;
        limits.min = -15.0f;
        limits.max = 15.0f;
        jt.limits = limits;
        jt.connectedBody = rb;
        JointMotor2D jtmotor = new JointMotor2D();
        jtmotor.motorSpeed = -1000.0f;
        jtmotor.maxMotorTorque = 1000.0f;
        jt.motor = jtmotor;

        HingeJoint2D jt1 = gameObject.AddComponent<HingeJoint2D>();
        jt1.autoConfigureConnectedAnchor = false;
        jt1.connectedBody = end.GetComponent<Rigidbody2D>();
        jt1.anchor = coll[0].offset * fiber_scale;
        jt1.connectedAnchor = coll_fiber[0].offset * fiber_scale;


        Vector2 pos = ((coll[0].offset + (coll[2].offset - coll[0].offset))*fiber_scale) / 2.0f;
        for (int i = 0; i < props.persistence_length; i++)
        {
            GameObject elem = chain_parent.transform.GetChild(chain_index + i).gameObject;
            SpringJoint2D spring1 = elem.AddComponent<SpringJoint2D>();
            spring1.enableCollision = false;
            spring1.autoConfigureDistance = false;
            spring1.distance = fiber_length * (i + 1) - 1;
            spring1.connectedAnchor = pos;
            spring1.connectedBody = rb;
            spring1.frequency = (persistence_strength != -1.0f) ? persistence_strength : 10.0f / ((i + 2) / 2.0f); ;
        }
    }

    public void IterateSingleAttachment(int chain_index, GameObject chain_parent, int searchScope)
    {
        if (chain_index % gap_between_bound == 0) return;
        if (gap_between_bound != 0) return;
        
        for (int i = chain_index; i < searchScope; i++)
        {
            if (i % gap_between_bound == 0)
            {
                chain_index = i;
            }
        }
    }

    public void IterateAlongChain(GameObject partner, GameObject chain_parent, int chain_index)
    {
        int childcount = chain_parent.transform.childCount;
        for (int i = gap_between_bound; i <(childcount/gap_between_bound); i++ )
        {
            chain_index = i * gap_between_bound;
            ConnectToPartnerFiber(partner, chain_parent, chain_index);
        }
    }

    public void ConnectToPartner(GameObject partner)
    {
        if (!is_connected) return;
        PrefabProperties props = partner.GetComponent<PrefabProperties>();
        bool partner_is_fiber = props.is_fiber;
        bool partner_connection_occupied = props.connection_occupied;
        if (is_fiber && !connection_occupied)
            ConnectToPartnerFiber(partner);
    }

    public void AttachToPartner(GameObject partner)
    {
        if (!is_bound) return;
        bool is_fiber = partner.GetComponent<PrefabProperties>().is_fiber;
        if (is_fiber)
            AttachToPartnerFiber(partner);
    }

    public void ActorCount(int number_placed, bool placed)
    {
        //Not used yet, it might be better to use a global array in the SceneManager.
        if (placed)
            number_placed++;
        if (!placed)
            number_placed--;
    }

    void ShowAttachmentsGL(){
        if (attachments.Count == 0)
        {
             if (gLDebug!= null) {gLDebug.Clear();gLDebug.displayLines = false;}
             return;
        }
        else {
            if (Manager.Instance.bindMode || Manager.Instance.dragMode){
                if (gLDebug== null){
                    gLDebug = gameObject.AddComponent<GLDebug>();
                }
                gLDebug.Clear();
                gLDebug.displayLines = true;
                for(int i=0;i< attachments.Count; i++){
                    var jt = attachments[i];
                    gLDebug.DrawLine(transform.TransformPoint(jt.anchor),
                                     jt.connectedBody.transform.TransformPoint(jt.connectedAnchor),
                                     spriteRenderer.sharedMaterial.color,
                                     10000,
                                     false
                                    );
                }

            }
            else 
            {
                if (gLDebug!= null) {gLDebug.Clear();gLDebug.displayLines = false;}
            }
        }
    }

    void ShowAttachmentsLineRendererOld(){
        if (attachments.Count == 0){
             if (bound_lines != null) bound_lines.enabled = false;
             return;
        }
        else {
            if (Manager.Instance.bindMode || Manager.Instance.dragMode){
                if (bound_lines == null){
                        bound_lines = gameObject.AddComponent<LineRenderer>();
                        bound_lines.sharedMaterial = Manager.Instance.lineMat;
                        bound_lines.sortingOrder = spriteRenderer.sortingOrder+1;
                        bound_lines.widthMultiplier = 1.0f;
                        bound_lines.numCapVertices = 5;
                        bound_lines.startColor = spriteRenderer.sharedMaterial.color;
                        bound_lines.endColor =spriteRenderer.sharedMaterial.color;
                }
                bound_lines.enabled = true;
                bound_lines.positionCount = attachments.Count*2;
                Gradient gradient = new Gradient();
                gradient.mode = GradientMode.Fixed;
                GradientColorKey[] colorKey=new GradientColorKey[bound_lines.positionCount];
                GradientAlphaKey[] alphaKey=new GradientAlphaKey[bound_lines.positionCount*2];
                for(int i=0;i< attachments.Count; i++){
                    var jt = attachments[i];
                    bound_lines.SetPosition(i*2, transform.TransformPoint(jt.anchor));
                    bound_lines.SetPosition(i*2+1, jt.connectedBody.transform.TransformPoint(jt.connectedAnchor));
                    float id0=((float)(i*2)/(float)bound_lines.positionCount)-0.01f;
                    if (i>0) {
                        alphaKey[i*4].alpha = 0.0f; 
                        alphaKey[i*4].time = id0;
                        float id1=(float)(i*2)/(float)bound_lines.positionCount;
                        colorKey[i*2].color = spriteRenderer.sharedMaterial.color;
                        colorKey[i*2].time = id1;
                        alphaKey[i*4+1].alpha = 1.0f; 
                        alphaKey[i*4+1].time = id1;
                    }
                    else {
                        colorKey[i*2].color = spriteRenderer.sharedMaterial.color;
                        colorKey[i*2].time = id0; 
                        alphaKey[i*4].alpha = 1.0f; 
                        alphaKey[i*4].time = 0;                        
                    }
                    float id2=(float)(i*2+1)/(float)bound_lines.positionCount;
                    colorKey[i*2+1].color = jt.connectedBody.GetComponent<SpriteRenderer>().sharedMaterial.color; 
                    colorKey[i*2+1].time = id2; 
                    alphaKey[i*4+2].alpha = 1.0f; 
                    alphaKey[i*4+2].time = id2;
                    float id3=((float)(i*2+1)/(float)bound_lines.positionCount)+0.01f;
                    if (i<attachments.Count-1){
                        alphaKey[i*4+3].alpha = 0.0f; 
                        alphaKey[i*4+3].time = id3;
                    }
                    else {
                        alphaKey[i*4+3].alpha = 1.0f; 
                        alphaKey[i*4+3].time = 1.0f;
                    }
                }
                gradient.SetKeys(colorKey, alphaKey);
                bound_lines.colorGradient = gradient;
            }
            else {
                if (bound_lines != null) bound_lines.enabled = false;
            }
        }
    }

    void Update() 
    {
        checkFiberConnection();
    }
}


