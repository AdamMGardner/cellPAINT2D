﻿using UnityEngine;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;

[Serializable]
public class PrefabProperties : MonoBehaviour
{
    public string name;
    public string common_name;
    public string function_group;
    public string compartment;
    public string description;
    public int order=0;//0 front, 1 middle, 2 bottom
    public int layer_number = 3;
    public bool isCircleCollider;
    public float circle_radius;
    public Vector2 circle_offset;
    public bool isCapsuleCollider;
    public Vector2 capsule_size;
    public Vector2 capsule_offset;
    public bool isHorizontal;
    public bool is_surface;
    public bool surface_secondLayer = true;
    public float surface_offset = 0.0f;
    public float scale2d = 1.0f;
    public float local_scale = 1.0f;
    public Vector3 pcpAxe = Vector3.up;
    public Vector3 offset = Vector3.zero;
    public bool is_fiber;
    public bool fiber_Middle;
    public bool fiber_Bottom;
    public bool light_fiber=false;
    public bool closing = true;
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
    [ColorUsage(true, true, 0f, 8f, 0.125f, 3f)]
    public Color outline_color = Color.yellow;
    [ColorUsage(true, true, 0f, 8f, 0.125f, 3f)]
    public Color pin_color = Color.red;

    [HideInInspector]
    public Vector3 entry;
    [HideInInspector]
    public Vector3 exit;

    public float area;
    public bool ispined = false;
    public bool enableCollision = false;
    //public float timeToGo;
    public int spriteTumble_id = 0;
    public int spriteRotation_id = 0;
    int prefab_id = 0;

    private int spriteOrdered_id = 0;
    private int[,] array2DIcosChoices;
    private SpriteRenderer spriteRenderer;
    public LineRenderer bound_lines;
    public GLDebug gLDebug;
    public float fiber_length = 0.0f;
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
    public List<SpringJoint2D> attachments;
    private bool _checked = false;
    //private Rigidbody2D rigidbody;
    void IcosahedronRotation()
    {
        int[,] array2DIcosChoices = new int[12, 5] { { 1, 2, 7, 8, 9 }, { 0, 2, 7, 10, 11 }, { 0, 1, 3, 8, 11 }, { 2, 4, 5, 8, 11 }, { 3, 5, 6, 9, 10 }, { 3, 4, 6, 10, 11 }, { 4, 5, 7, 9, 10 }, { 0, 1, 6, 9, 10 }, { 0, 2, 3, 4, 9 }, { 0, 4, 6, 7, 8 }, { 1, 5, 6, 7, 11 }, { 1, 2, 3, 5, 10 } };
        /*if (sprites_asset.Count != 12)
        {
            Debug.Log("Twelve sprites are needed for this to work correctly");
            return;
        }*/
        if (sprite_icosahedronRotation_switch)
        {
            spriteRotation_id = array2DIcosChoices[spriteRotation_id, UnityEngine.Random.Range(1, 5)];
            //Debug.Log(array2DIcosChoices);
            spriteRenderer.sprite = sprites_asset[spriteRotation_id];
        }
        //timeToGo = Time.fixedTime + Random.Range(0.0f, 1.0f);
    }

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
            //spriteTumble_id = Random.Range(0, sprites_asset.Count - 1);
            spriteRenderer.sprite = sprites_asset[spriteTumble_id];
        }
        //timeToGo = Time.fixedTime + Random.Range(0.0f, 1.0f);
    }

    /*
    void FixedUpdate()
    {
        if (Time.fixedTime >= timeToGo)
        {
            SpriteTumble();
            IcosahedronRotation();
        }
    }
    */
    public void switchSpriteInOrder()
    {
        if (spriteOrdered_id >= sprites_asset.Count)
        {
            spriteOrdered_id = 0;
        }
        //if (spriteOrdered_id >= sprites_asset.Count) return;
        //Debug.Log(spriteOrdered_id);
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
        //Debug.Log(gameObject.name);
        CircleCollider2D[] allc = GetComponents<CircleCollider2D>();
        if (allc.Length < 2) return;
        Vector2 pos1 = new Vector2(transform.position.x, transform.position.y) + allc[0].offset * fiber_scale;
        Vector2 pos2 = new Vector2(transform.position.x, transform.position.y) + allc[allc.Length - 1].offset * fiber_scale;
        fiber_length = Vector2.Distance(pos1, pos2);
    }

    void setupCollider() {
        return;
        /*if (isCircleCollider == true)
        {
            CircleCollider2D Circle = gameObject.GetComponent<CircleCollider2D>();
            if (Circle == null) Circle = gameObject.AddComponent<CircleCollider2D>();
            if (circle_radius != 0.0f)
            {
                Circle.radius = circle_radius;
            }
            else {
                circle_radius = Circle.radius;
            }
            Circle.offset = circle_offset;
        }

        else if (isCapsuleCollider == true)
        {
            CapsuleCollider2D Capsule = gameObject.GetComponent<CapsuleCollider2D>();
            if (Capsule == null) Capsule = gameObject.AddComponent<CapsuleCollider2D>();
            if (capsule_size != new Vector2(0.0f, 0.0f))
            {
                if (isHorizontal)
                {
                    Capsule.direction = CapsuleDirection2D.Horizontal;
                }
                else
                {
                    Capsule.direction = CapsuleDirection2D.Vertical;
                }
                Capsule.size = capsule_size;
                Capsule.offset = capsule_offset;
            }
        }
        else {
            CircleCollider2D Circle = gameObject.GetComponent<CircleCollider2D>();
            if (Circle == null) Circle = gameObject.AddComponent<CircleCollider2D>();
            if (circle_radius != 0.0f)
            {
                Circle.radius = circle_radius;
            }
            else {
                circle_radius = Circle.radius;
            }
            Circle.offset = circle_offset;
        }*/
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
            // topRB = rb;
        }

        setupCollider();

        //This loop creates the other layers from the top layer
        /*
        if (layer_number == 2)
        {
            GameObject twoLayerBottom = Instantiate(newObject, objectPosBottom, quat) as GameObject;
            twoLayerBottom.transform.name = Props.name + " (Bottom)";
            twoLayerBottom.layer = LayerMask.NameToLayer("Bottom Layer");
            twoLayerBottom.transform.parent = root.transform;

            //Add Rigidbody2D to loop and count.
            if (bottomRB != null)
            {
                bottomRB.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            }
            Rigidbody2D rb = twoLayerBottom.GetComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            everything[rbCount] = rb;
            rbCount++;
            string name = instancePrefab.name;
            UpdateCountAndLabel(name, newObject);
            bottomRB = rb;
        }
        else
        {
            GameObject threeLayerMiddle = Instantiate(newObject, objectPosMiddle, quat) as GameObject;
            GameObject threeLayerBottom = Instantiate(newObject, objectPosBottom, quat) as GameObject;

            threeLayerMiddle.transform.name = Props.name + " (Middle)";
            threeLayerBottom.transform.name = Props.name + " (Bottom)";

            threeLayerMiddle.layer = LayerMask.NameToLayer("Middle Layer");
            threeLayerBottom.layer = LayerMask.NameToLayer("Bottom Layer");

            threeLayerMiddle.transform.parent = root.transform;
            threeLayerBottom.transform.parent = root.transform;

            //Add Rigidbody2D to loop and count.
            Rigidbody2D rb = threeLayerMiddle.GetComponent<Rigidbody2D>();
            if (middleRB != null)
            {
                middleRB.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            }
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            everything[rbCount] = rb;
            rbCount++;
            string name = instancePrefab.name;
            UpdateCountAndLabel(name, newObject);
            middleRB = rb;

            Rigidbody2D rb2 = threeLayerBottom.GetComponent<Rigidbody2D>();
            if (bottomRB != null)
            {
                bottomRB.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            }
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            everything[rbCount] = rb2;
            rbCount++;
            string name2 = instancePrefab.name;
            UpdateCountAndLabel(name2, newObject);
            bottomRB = rb2;
        }*/
        initialized = true;
    }

    public void TestPcpalAxis() { }


    public void TestPcpalAxis2D()
    {
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
       // Debug.Log((mrot * Vector3.up).ToString());
       // Debug.Log((mrot * Vector3.up).ToString());
        Vector3 m_ext =  (pcp[0] - pcp[1]);//align to the pcpal Axis//should be absolute!
        Debug.Log(m_ext.ToString());
        Debug.Log((Quaternion.Inverse(mrot) * m_ext).ToString());
        Debug.Log((mrot * m_ext).ToString());
        /*GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = transform;
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localRotation = mrot;
        cube.transform.localScale = m_ext;
        DestroyImmediate(cube.GetComponent<BoxCollider>());
        cube.AddComponent<BoxCollider2D>();
        cube.GetComponent<MeshRenderer>().enabled = false;
        */
        //m_ext = Quaternion.Inverse(mrot) * m_ext;
        var sr = GetComponent<SpriteRenderer>();
        bool flip = (sr.size.x > sr.size.y);

        center = mrot * center;
        if (Mathf.Abs(m_ext.x - m_ext.y) < 0.25f) {
            CircleCollider2D Circle = gameObject.AddComponent<CircleCollider2D>();
            Circle.radius = ((Mathf.Abs(m_ext.x) + Mathf.Abs(m_ext.y)) / 2.0f) / 2.0f;
            Circle.offset = new Vector2(center.x, center.y);
        }
        else
        {
            if (flip) m_ext = new Vector2(m_ext.y,m_ext.x);//Quaternion.Inverse(mrot) * new Vector2(Mathf.Abs(m_ext.x), Mathf.Abs(m_ext.y));
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(Mathf.Abs(m_ext.x), Mathf.Abs(m_ext.y));
            box.offset = new Vector2(center.x, center.y);
        }
        circle_radius = (((Mathf.Abs(m_ext.x) + Mathf.Abs(m_ext.y)) / 2.0f) / 2.0f) * (1.0f/local_scale);//unity unit
        return;
        //check the extend
        //GameObject ob;
        float cutoff = 60.0f;
        float pcutoff = 0.70f;//30%
        float radius = (m_ext.y + m_ext.z) / 2.0f;
        float rmax = Mathf.Max(Mathf.Max(m_ext.x, m_ext.y), m_ext.z);//this is 100%
        float dx = m_ext.x / rmax;
        float dy = m_ext.y / rmax;
        float dz = m_ext.z / rmax;
        bool isSpherical = false;
        bool isCapsule = false;
        Vector3 ascale = Vector3.one;
        if (rmax == m_ext.x)//check y and z, dx = 1
        {
            Debug.Log("X");
            Debug.Log(Mathf.Abs(Mathf.Min(m_ext.x, m_ext.y) / Mathf.Max(m_ext.x, m_ext.y)));
            if ((dz >= pcutoff) && (dy >= pcutoff)) isSpherical = true;
            else if (Mathf.Abs(Mathf.Min(m_ext.z, m_ext.y) / Mathf.Max(m_ext.z, m_ext.y)) >= pcutoff)
            {
                ascale = new Vector3(m_ext.y, rmax, m_ext.z);
                isCapsule = true;
            }
        }
        else if (rmax == m_ext.y)//check y and z,dy = 1
        {
            Debug.Log("Y");
            Debug.Log(Mathf.Abs(Mathf.Min(m_ext.x, m_ext.y) / Mathf.Max(m_ext.x, m_ext.y)));
            if ((dx >= pcutoff) && (dy >= pcutoff)) isSpherical = true;
            else if (Mathf.Abs(Mathf.Min(m_ext.y, m_ext.x) / Mathf.Max(m_ext.y, m_ext.x)) >= pcutoff)
            {
                ascale = new Vector3(m_ext.y, rmax, m_ext.z);
                isCapsule = true;
            }
        }
        else if (rmax == m_ext.z)//check y and z,dz = 1
        {
            Debug.Log("Z");
            Debug.Log(Mathf.Abs(Mathf.Min(m_ext.z, m_ext.y) / Mathf.Max(m_ext.z, m_ext.y)));
            if ((dz >= pcutoff) && (dy >= pcutoff)) isSpherical = true;
            else if (Mathf.Abs(Mathf.Min(m_ext.z, m_ext.y) / Mathf.Max(m_ext.z, m_ext.y)) >= pcutoff)
            {
                ascale = new Vector3(m_ext.z, rmax, m_ext.y);
                isCapsule = true;
            }
        }
        
        //float score = (Mathf.Abs(m_ext.x - m_ext.y) + Mathf.Abs(m_ext.x - m_ext.z) + Mathf.Abs(m_ext.z - m_ext.y)) / 3.0f;
        if (isSpherical)//(Mathf.Abs(m_ext.x - radius) < cutoff) && (Mathf.Abs(m_ext.y - radius) < cutoff) && (Mathf.Abs(m_ext.z - radius) < cutoff) )
        {
            //assume a sphere of radius max or min or avg
            Debug.Log("isSpherical " + radius.ToString());
            CircleCollider2D Circle = gameObject.AddComponent<CircleCollider2D>();
            Circle.radius = radius/2.0f;
            //Circle.offset = new Vector2(center.y, center.z);
            /*ob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ob.transform.parent = transform;
            ob.transform.localPosition = Vector3.zero;
            ob.transform.localRotation = Quaternion.identity;
            ob.transform.localScale = new Vector3(radius, radius, radius) * 0.01f;*/
        }
        else if (isCapsule)
        {
            Debug.Log("isCapsule " + m_ext.ToString());
            CapsuleCollider2D Capsule = gameObject.AddComponent<CapsuleCollider2D>();
            Capsule.direction = CapsuleDirection2D.Horizontal;
            //Capsule.direction = CapsuleDirection2D.Vertical;
            Capsule.size = new Vector2(m_ext.z, m_ext.y);
            //Capsule.offset = new Vector2(center.z, center.y);
            //Capsule.offset = capsule_offset;
            //assume a sphere of radius max or min or avg            
            /*ob = GameObject.CreatePrimitive(PrimitiveType.Capsule);//default direction is Y
            ob.transform.parent = transform;
            ob.transform.localPosition = Vector3.zero;// center * 0.01f;
            ob.transform.localRotation = mrot;
            ob.transform.localScale = (m_ext) * 0.01f;
            CapsuleCollider cc = ob.GetComponent<CapsuleCollider>();
            cc.direction = 2;
            cc.radius = 0.5f;
            cc.height = 1.0f;*/
        }
        else
        {
            Debug.Log("Cube " + m_ext.ToString());
            BoxCollider2D box2 = gameObject.AddComponent<BoxCollider2D>();
            box2.size = new Vector2(m_ext.z, m_ext.y);
            //box.offset = new Vector2(center.z, center.y);
            /*ob = GameObject.CreatePrimitive(PrimitiveType.Cube); //this create a box collider automatically
                                                                 //GameObject cube = new GameObject("cube");//empty
            ob.transform.parent = transform;
            ob.transform.localPosition = Vector3.zero;//center * 0.01f;// Vector3.zero;//*0.01 ?
            ob.transform.localRotation = mrot;// Quaternion.Inverse(Quaternion.FromToRotation(Vector3.right, pcp[imax]));
            ob.transform.localScale = (m_ext) * 0.01f;// Vector3.one;// new Vector3(pcp[order[0]].w, pcp[order[1]].w, pcp[order[2]].w) * 0.01f;
            */
        }
        //if (is_surface)
        //    ob.layer = 19;//transmb2
        //ob.GetComponent<MeshRenderer>().enabled = false;
    }

    public void SetupFromNode() {
        //setup color ?
        if (setuped) return;
        if (!Manager.Instance.ingredient_node.ContainsKey(name)) return;
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        JSONNode node = Manager.Instance.ingredient_node[name];
        is_fiber = (node["Type"].Value == "Grow");
        is_surface = (node["surface"].Value == "surface") ? true : false;
        Collider2D[] coll = GetComponents<Collider2D>();
        foreach (var col in coll) DestroyImmediate(col);
        PolygonCollider2D box = gameObject.GetComponent<PolygonCollider2D>();
        if (box == null) box = gameObject.AddComponent<PolygonCollider2D>();
        Vector2[] pts = box.points;
        DestroyImmediate(box);
        pcp = Helper.BuildOBB2D(pts, 0.0f); //TestEigenTest(cluster);
        Debug.Log("pcp"); Debug.Log(pcp);
        //use the polygon collider to get the path and compute the simple collider
        /*
        Vector3 v = gameObject.GetComponent < SpriteRenderer >().bounds.size;
        BoxCollider2D box = gameObject.GetComponent<BoxCollider2D>();
        if (box == null) box = gameObject.AddComponent<BoxCollider2D>();
        box.size = v;
        */
        //what is pixel-angstrom ratio in unity
        //1pixel = 1.0/100.0funit = 0.01 u = 0.35Ang * scale2d
        //any sprite to unity sprite scale is 
        var pixel_scale = (Manager.Instance.unit_scale * 10.0f) / 100.0f;
        var unity_scale2d = 1.0f / (Manager.Instance.unit_scale * 10.0f);
        //1 unit scale is 3.5nm e.g. Manager.Instance.unit_scale. Divide by (uscale*10.0) to get unity unit.
        //sprite is scale2d 
        scale2d = 1.0f;
        surface_offset = 0.0f;
        if (node.HasKey("sprite"))
        {
            scale2d = float.Parse(node["sprite"]["scale2d"]);//1 angstrom in the image is scale2d pixel
            surface_offset = -float.Parse(node["sprite"]["offsety"]) * unity_scale2d;// offset.magnitude / (Manager.Instance.unit_scale * 10.0f);
        }
        local_scale = 1.0f/(pixel_scale * scale2d);
        surface_offset=(surface_offset/local_scale);
        Vector3 center = (pcp[0] + pcp[1]) * 0.5f;
        Vector3 m_ext = (pcp[0] - pcp[1]);
        Quaternion mrot = new Quaternion(pcp[2].x, pcp[2].y, pcp[2].z, pcp[2].w);
        if (!is_fiber && is_surface)
        {
            gameObject.layer = 12;
            float thickness = (84.0f / 2.0f) * unity_scale2d;//42 angstrom
            float radius = (m_ext.y + m_ext.z) / 4.0f;
            //C0ircleCollider2D c = gameObject.AddComponent<CircleCollider2D>();
            //c.radius = thickness;
            //c.offset = new Vector2(center.y, center.z + surface_offset);
            float bradius = 2.0f;
            float bicycle_radius = bradius*1.0f/local_scale;
            TestPcpalAxis2D();
            GameObject bicycleUp = new GameObject("up");
            bicycleUp.layer = 15;
            bicycleUp.transform.parent = gameObject.transform;
            bicycleUp.transform.localPosition = new Vector3(center.y, center.z + thickness + bicycle_radius + surface_offset, 0.0f);
            CircleCollider2D CircleUp = bicycleUp.AddComponent<CircleCollider2D>();
            CircleUp.radius = bicycle_radius;
            CircleUp.offset = new Vector2(bicycle_radius,0);
            CircleCollider2D CircleUp2 = bicycleUp.AddComponent<CircleCollider2D>();
            CircleUp2.radius = bicycle_radius;
            CircleUp2.offset = new Vector2(-bicycle_radius,0);
            Rigidbody2D CircleUpRb = bicycleUp.AddComponent<Rigidbody2D>();
            //CircleUpRb.bodyType = RigidbodyType2D.Kinematic;
            //add the joint FixedJoint2D or Hinge
            FixedJoint2D hjup = bicycleUp.AddComponent<FixedJoint2D>();
            hjup.autoConfigureConnectedAnchor = false;
            hjup.connectedBody = rb;
            hjup.anchor = new Vector2(-bicycle_radius, 0);
            hjup.connectedAnchor = new Vector2(-bicycle_radius, center.z + thickness + bicycle_radius + surface_offset);
            //hjup.frequency = 1.0f;
            hjup = bicycleUp.AddComponent<FixedJoint2D>();
            hjup.autoConfigureConnectedAnchor = false;
            hjup.connectedBody = rb;
            hjup.anchor = new Vector2(bicycle_radius, 0);
            hjup.connectedAnchor = new Vector2(bicycle_radius, center.z + thickness + bicycle_radius+ surface_offset);
            //hjup.frequency = 1.0f;
            GameObject bicycleDown = new GameObject("down");
            bicycleDown.layer = 15;
            bicycleDown.transform.parent = gameObject.transform;
            bicycleDown.transform.localPosition = new Vector3(center.y, center.z - thickness - bicycle_radius + surface_offset, 0.0f);
            CircleCollider2D CircleDown = bicycleDown.AddComponent<CircleCollider2D>();
            CircleDown.radius = bicycle_radius;
            CircleDown.offset = new Vector2(bicycle_radius,0);
            CircleCollider2D CircleDown2 = bicycleDown.AddComponent<CircleCollider2D>();
            CircleDown2.radius = bicycle_radius;
            CircleDown2.offset = new Vector2(-bicycle_radius,0);
            Rigidbody2D CircleDownRb = bicycleDown.AddComponent<Rigidbody2D>();
            //CircleDownRb.bodyType = RigidbodyType2D.Kinematic;
            //add the joint
            FixedJoint2D hjdown = bicycleDown.AddComponent<FixedJoint2D>();
            hjdown.autoConfigureConnectedAnchor = false;
            hjdown.connectedBody = rb;
            hjdown.anchor = new Vector2(-bicycle_radius, 0);
            hjdown.connectedAnchor = new Vector2(-bicycle_radius, center.z - thickness -bicycle_radius + surface_offset);
            //hjdown.frequency = 1.0f;
            hjdown = bicycleDown.AddComponent<FixedJoint2D>();
            hjdown.autoConfigureConnectedAnchor = false;
            hjdown.connectedBody = rb;
            hjdown.anchor = new Vector2(bicycle_radius, 0);
            hjdown.connectedAnchor = new Vector2(bicycle_radius, center.z - thickness -bicycle_radius + surface_offset);
            //hjdown.frequency = 1.0f;
        }
        else if (is_fiber) {
            //persistence length ?
            persistence_length = 1;//nb of spring
            persistence_strength = 5.0f;
            fiber_scale = local_scale;
            //setup the collider
            center = mrot * center;
            float radius = (Mathf.Abs(m_ext.y) / 2.0f)/2.0f;
            //main collider first
            if (Mathf.Abs(m_ext.x - m_ext.y) < 0.25f)
            {
                CircleCollider2D Circle = gameObject.AddComponent<CircleCollider2D>();
                Circle.radius = ((Mathf.Abs(m_ext.x) + Mathf.Abs(m_ext.y)) / 2.0f) / 2.0f;
                Circle.offset = new Vector2(center.x, center.y);
            }
            else
            {
                m_ext = Quaternion.Inverse(mrot) * new Vector2(Mathf.Abs(m_ext.x), Mathf.Abs(m_ext.y));
                BoxCollider2D abox = gameObject.AddComponent<BoxCollider2D>();
                abox.size = new Vector2(Mathf.Abs(m_ext.x), Mathf.Abs(m_ext.y));
                abox.offset = new Vector2(center.x, center.y);
            }
            //anchor collider, on X axis.
            CircleCollider2D CircleLeft = gameObject.AddComponent<CircleCollider2D>();
            CircleLeft.radius = radius;
            CircleLeft.offset = new Vector2(-Mathf.Abs(m_ext.x)/2.0f + radius/2.0f, center.y);
            CircleCollider2D CircleRight = gameObject.AddComponent<CircleCollider2D>();
            CircleRight.radius = radius;
            CircleRight.offset = new Vector2(Mathf.Abs(m_ext.x) / 2.0f - radius / 2.0f, center.y);
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

    public void Setup(bool special = false) {
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
            entry = (coll[3].offset - coll[1].offset).normalized;
            exit = (coll[0].offset - coll[2].offset).normalized;
        }
        if (is_fiber)
        {
            //Debug.Log("OnEnable " + gameObject.name);
            updateFiberPrefab();
        }
    }

    void OnEnable()
    {
        //Setup();
    }

    void OnDisable()
    {
        UpdateOutline(false);
    }

    // Use this for initialization
    void Start()
    {
        int[,] array2DIcosChoices = new int[12, 5] { { 1, 2, 7, 8, 9 }, { 0, 2, 7, 10, 11 }, { 0, 1, 3, 8, 11 }, { 2, 4, 5, 8, 11 }, { 3, 5, 6, 9, 10 }, { 3, 4, 6, 10, 11 }, { 4, 5, 7, 9, 10 }, { 0, 1, 6, 9, 10 }, { 0, 2, 3, 4, 9 }, { 0, 4, 6, 7, 8 }, { 1, 5, 6, 7, 11 }, { 1, 2, 3, 5, 10 } };
        //timeToGo = Time.fixedTime + Random.Range(0.0f, 1.0f);
        area = getArea();
        if (is_fiber)
        {
            //Debug.Log("Start " + gameObject.name);
            updateFiberPrefab();
        }
        Setup();
        _checked = false;
        attachments = new List<SpringJoint2D>();
    }

    public void setCheck(bool value) {
        _checked = value;
    }

    public void checkFiberConnection() {
        //should be done on undo not creation
        // Debug.Log("checkFiberConnection " + _checked.ToString());
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
            //foreach (var aj in sjts) Destroy(aj);
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
                /*
                if ((ipos - j) < 0)
                {
                    if (transform.parent.name.Contains("_Closed"))
                    {
                        if ((n - j) < 0) continue;
                        ch = transform.parent.GetChild(n - j);
                    }
                    else continue;
                }
                else {
                    ch = transform.parent.GetChild(ipos - j);
                }
                */
                SpringJoint2D spring = sjts[j - 1];// ch.gameObject.AddComponent<SpringJoint2D>();
                spring.connectedBody = ch.GetComponent<Rigidbody2D>();
                spring.enableCollision = enableCollision;
                spring.autoConfigureDistance = false;
                spring.distance = fiber_length * (j);// + UnityEngine.Random.Range(0.0f, fiber_length / 10.0f);
                spring.anchor = Vector2.zero;// allc[1].offset;
                spring.connectedAnchor = Vector2.zero;//allc[0].offset;
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
        mpb.SetFloat("_Outline", is_outline ? outline_width : 0);
        mpb.SetColor("_OutlineColor", outline_color);
        spriteRenderer.SetPropertyBlock(mpb);
    }

    public void UpdateOutlinePin(bool toggle)
    {
        //Debug.Log("UpdateOutlinePin");
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
        //Vector3 other_orientation = start.transform.TransformDirection(Vector3.right);

        Quaternion q = Quaternion.FromToRotation(Vector3.right, entry_to_align);
        start.GetComponent<Rigidbody2D>().rotation = q.eulerAngles.z;
        //start.transform.rotation = q;
        //end align to exit
        //her_orientation = end.transform.TransformDirection(Vector3.right);
        Quaternion q1 = Quaternion.FromToRotation(Vector3.right, exit_to_align);
        end.GetComponent<Rigidbody2D>().rotation = -q.eulerAngles.z;
        //end.transform.rotation = q1; 
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
        /*GameObject start = chain_parent.transform.GetChild(elemRemoved - 1).gameObject;
        GameObject end = chain_parent.transform.GetChild(elemRemoved + 1).gameObject;
        foreach (var jts in start.GetComponentsInChildren<SpringJoint2D>())
        {
            Destroy(jts);
        }*/
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
            //Debug.Log(testName);

            if (testName == "HIV_NC_1f6u(Clone)")
            {
                searchScope = searchScope + gap_between_bound;
                //Debug.Log(searchScope);
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
        //Color prefabRGB = start.GetComponent<SpriteRenderer>().color;
        //float hue, S, V;
        //Color.RGBToHSV(prefabRGB, out hue, out S, out V);

        //Set Properties of bound object.
       // Color boundRGB = GetComponent<SpriteRenderer>().color;
        //float hue2, S2, V2;
        //Color.RGBToHSV(boundRGB, out hue2, out S2, out V2);
        //Color newBoundColor = Color.HSVToRGB(hue2, S2, V);

        //GetComponent<SpriteRenderer>().color = newBoundColor;
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

        //reorient start and end to aligne to the binder
        alignFiber(start, end);

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
        //jt.useMotor = false;

        HingeJoint2D jt1 = gameObject.AddComponent<HingeJoint2D>();
        jt1.autoConfigureConnectedAnchor = false;
        jt1.connectedBody = end.GetComponent<Rigidbody2D>();
        jt1.anchor = coll[0].offset * fiber_scale;
        jt1.connectedAnchor = coll_fiber[0].offset * fiber_scale;
        /*
        JointAngleLimits2D limits1 = jt.limits;
        limits1.min = -5.0f-150.0f;
        limits1.max = 5.0f-90.0f;
        jt1.limits = limits1;
        jtmotor.motorSpeed = -1000.0f;
        jt1.motor = jtmotor;
        //jt1.useMotor = false;
        */

        Vector2 pos = ((coll[0].offset + (coll[2].offset - coll[0].offset))*fiber_scale) / 2.0f;
        for (int i = 0; i < props.persistence_length; i++)
        {
            //attach elem+i to obj
            GameObject elem = chain_parent.transform.GetChild(chain_index + i).gameObject;
            SpringJoint2D spring1 = elem.AddComponent<SpringJoint2D>();
            spring1.enableCollision = false;
            spring1.autoConfigureDistance = false;
            spring1.distance = fiber_length * (i + 1) - 1;
            //spring1.distance = spring1.distance * fiber_scale;
            //spring1.anchor = coll[1].offset;
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
        if (attachments.Count == 0){
             //if (bound_lines != null) bound_lines.enabled = false;
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
                /*
                if (bound_lines == null){
                        bound_lines = gameObject.AddComponent<LineRenderer>();
                        bound_lines.sharedMaterial = Manager.Instance.lineMat;
                        bound_lines.sortingOrder = spriteRenderer.sortingOrder+1;
                        bound_lines.widthMultiplier = 0.3f;
                        bound_lines.numCapVertices = 5;
                        //bound_lines.startColor = spriteRenderer.sharedMaterial.color;
                        //bound_lines.endColor = jt.connectedBody.GetComponent<SpriteRenderer>().sharedMaterial.color;
                }
                bound_lines.enabled = true;
                for(int i=0;i< attachments.Count; i++){
                    var jt = attachments[i];
                    bound_lines.SetPosition(i*2, transform.TransformPoint(jt.anchor));
                    bound_lines.SetPosition(i*2+1, jt.connectedBody.transform.TransformPoint(jt.connectedAnchor));                   
                }*/
            }
            else {
                //if (bound_lines != null) bound_lines.enabled = false;
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
                        //bound_lines.startColor = spriteRenderer.sharedMaterial.color;
                        //bound_lines.endColor = jt.connectedBody.GetComponent<SpriteRenderer>().sharedMaterial.color;
                }
                bound_lines.enabled = true;
                bound_lines.positionCount = attachments.Count*2;
                Gradient gradient = new Gradient();
                gradient.mode = GradientMode.Fixed;
                GradientColorKey[] colorKey=new GradientColorKey[bound_lines.positionCount];
                GradientAlphaKey[] alphaKey=new GradientAlphaKey[bound_lines.positionCount*2];
                //AnimationCurve curve = new AnimationCurve();
                //curve.AddKey(0.0f, 0.3f);
                for(int i=0;i< attachments.Count; i++){
                    var jt = attachments[i];
                    bound_lines.SetPosition(i*2, transform.TransformPoint(jt.anchor));
                    bound_lines.SetPosition(i*2+1, jt.connectedBody.transform.TransformPoint(jt.connectedAnchor));
                    float id0=((float)(i*2)/(float)bound_lines.positionCount)-0.01f;
                    if (i>0) {
                        //colorKey[i*4].color = Color.white; 
                        //colorKey[i*4].time = id0; 
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
                        //colorKey[i*4+3].color = Color.white; 
                        //colorKey[i*4+3].time = id3; 
                        alphaKey[i*4+3].alpha = 0.0f; 
                        alphaKey[i*4+3].time = id3;
                    }
                    else {
                        alphaKey[i*4+3].alpha = 1.0f; 
                        alphaKey[i*4+3].time = 1.0f;
                    }
                    //betwee 2-3, 4-5 etc...
                   // if (i>0)curve.AddKey(((float)(i*2)/(float)bound_lines.positionCount)-0.01f, 0.0f);
                   // curve.AddKey((float)(i*2)/(float)bound_lines.positionCount, 0.3f);                
                   // curve.AddKey((float)(i*2+1)/(float)bound_lines.positionCount, 0.3f);
                   // if (i<attachments.Count-1) curve.AddKey(((float)(i*2+1)/(float)bound_lines.positionCount)+0.01f, 0.0f);
                }
                gradient.SetKeys(colorKey, alphaKey);
                bound_lines.colorGradient = gradient;
                //curve.AddKey(1.0f, 0.3f);
                //Debug.Log(curve);
               //bound_lines.widthCurve = curve;
            }
            else {
                if (bound_lines != null) bound_lines.enabled = false;
                //if (gLDebug!= null) {gLDebug.Clear();gLDebug.displayLines = false;}
            }
        }
    }

    void Update() {
        checkFiberConnection();
        //ShowAttachmentsLineRenderer();
        /*SpringJoint2D jt = gameObject.GetComponent<SpringJoint2D>();
        if( !is_fiber )
        {
            if (jt!= null){
                if (bound_lines == null){
                        bound_lines = gameObject.AddComponent<LineRenderer>();
                        bound_lines.sharedMaterial = Manager.Instance.lineMat;
                        bound_lines.sortingOrder = spriteRenderer.sortingOrder+1;
                        bound_lines.widthMultiplier = 0.3f;
                        bound_lines.numCapVertices = 5;
                        bound_lines.startColor = spriteRenderer.sharedMaterial.color;
                        bound_lines.endColor = jt.connectedBody.GetComponent<SpriteRenderer>().sharedMaterial.color;
                }
            }
            if (Manager.Instance.bindMode || Manager.Instance.dragMode){        
                if (jt!= null){
                    bound_lines.enabled = true;
                    bound_lines.SetPosition(0, transform.TransformPoint(jt.anchor));
                    bound_lines.SetPosition(1, jt.connectedBody.transform.TransformPoint(jt.connectedAnchor));
                }
            }
            else {
                if (jt!= null) bound_lines.enabled = false;
            }
        }
        */
        /*
        zangle = transform.rotation.eulerAngles.z;
        Debug.Log(transform.rotation.eulerAngles.ToString());
        Debug.Log(gameObject.GetComponent<Rigidbody2D>().rotation.ToString());
        Vector3 axis;
        gameObject.transform.rotation.ToAngleAxis(out zangle, out axis);
        Debug.Log(zangle);
        */
    }
}

