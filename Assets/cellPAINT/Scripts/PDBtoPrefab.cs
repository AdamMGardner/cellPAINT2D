using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
using System.IO;
//using UnityStandardAssets.ImageEffects;

public class PDBtoPrefab : MonoBehaviour {

    public string PDBid;
    public GameObject atom_prefab;
    public GameObject root;
    public GameObject ui;
    public Texture2D screenShot;
    public string path;
    public string lastScreenshot;
    public float scale;
    public Material sprite_material;
    public moveClick manager;

    private List<Atom> atoms_data;
    private Camera cam;
    private GameObject prefab;
    private Bounds bb;
    /*
    // Use this for initialization
    void Start () {
        if (!root) root = new GameObject("root");
        cam = GetComponent<Camera>();
        path = Application.dataPath+ "/Resources/Sprites/";
        //cam.GetComponent<MouseCameraControl>().m_look_target = root.transform;
    }

    public void setPDBid(string val) {
        PDBid = val;
    }

    public void switchEffectCamera(bool toggle) {
        cam.GetComponent<EdgeDetection>().enabled = toggle;
    }

    public string ScreenShotName(int width, int height)
    {

        string strPath = "";

        strPath = string.Format("{0}/texture_{1}.png",
                             path,
                             PDBid);
        lastScreenshot = strPath;

        return strPath;
    }

    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite = new Sprite();
        Texture2D SpriteTexture = LoadTexture(FilePath);
        NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);

        return NewSprite;
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
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public void Clear() {
        GameObject.Destroy(prefab);
    }

    public void TakeScreenShot()
    {
        Debug.Log(path);
        //deactivate the ui
        if (ui) ui.SetActive(false);

        Camera myCamera = Camera.main;
        bool isTransparent = true;

        int resWidth = 512;// Screen.width * 4;
        int resHeight = 512;// Screen.height * 4;

        int resWidthN = resWidth;
        int resHeightN = resHeight;
        RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
        myCamera.targetTexture = rt;

        TextureFormat tFormat;
        if (isTransparent)
            tFormat = TextureFormat.ARGB32;
        else
            tFormat = TextureFormat.RGB24;

        screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);
        myCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
        myCamera.targetTexture = null;
        RenderTexture.active = null;
        screenShot.alphaIsTransparency = true;
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(resWidthN, resHeightN);

        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
        screenShot = new Texture2D(2, 2);           // Create new "empty" texture
        screenShot.LoadImage(bytes);           // Load the imagedata into the texture (size is set automatically)
        //Application.OpenURL(filename);
        screenShot.name = "screenshot";
        if (ui) ui.SetActive(true);
        AssetDatabase.Refresh();
        

        //AssetDatabase.ImportAsset("Textures/texture_" + PDBid, ImportAssetOptions.ForceUpdate);
        //makeThePrefab
        prefab = new GameObject("prefab_"+PDBid);
        prefab.transform.position = cam.ScreenToWorldPoint(new Vector3(screenShot.width, Screen.height/2.0f, 20));
        SpriteRenderer sp = prefab.AddComponent<SpriteRenderer>();
        sp.material = sprite_material;
        sp.material.color = Camera.main.backgroundColor;
        Rigidbody2D rb2d = prefab.AddComponent<Rigidbody2D>();
        rb2d.drag = 10.0f;
        rb2d.angularDrag = 5.0f;
        CircleCollider2D coll = prefab.AddComponent<CircleCollider2D>();
        coll.radius = 2.0f;
        PrefabProperties props = prefab.AddComponent<PrefabProperties>();
        props.encapsulating_radius = bb.size.x;
        AddForce f = prefab.AddComponent<AddForce>();

        StartCoroutine ("GetSprite","Textures/texture_" + PDBid);
    }

    IEnumerator GetSprite( string name)
    {
        SpriteRenderer sp = prefab.GetComponent<SpriteRenderer>();
        Sprite sprite = new Sprite();
        sprite = Resources.Load<Sprite>("Sprites/texture_" + PDBid);
        sp.sprite = sprite;
        while (!sprite)
        {
            sprite = Resources.Load<Sprite>("Sprites/texture_" + PDBid);
            sp.sprite = sprite;
            Debug.Log(sprite);
            yield return new WaitForSeconds(1);
        }
        PrefabUtility.CreatePrefab("Assets/Prefabs/" + PDBid + ".prefab", prefab);
        manager.SwitchPrefab_cb(prefab);
        //also switch mode to draw
        //hide main object
        foreach (Transform child in root.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        switchEffectCamera(false);
    }

    public void doit() {
        foreach (Transform child in root.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        GameObject.Destroy(prefab);
        manager.SwitchPrefab_cb(null);
        manager.Clear();
        switchEffectCamera(true);
        atoms_data = PdbLoader.LoadAtomDataFull(PDBid);
        AtomHelper.CenterAtoms(ref atoms_data);
        bb = AtomHelper.ComputeBounds(atoms_data);
        //align camera to the bb
        transform.position = bb.center;
        transform.Translate(new Vector3(0, 0, -(bb.size.z)));
        cam.orthographicSize = bb.size.y;
        Debug.Log(bb.size.ToString());
        //generate the object to display
        foreach (Atom at in atoms_data) {
            GameObject instance = GameObject.Instantiate(atom_prefab, at.position, Quaternion.identity) as GameObject;
            instance.transform.parent = root.transform;
            instance.transform.localScale = new Vector3(at.radius*scale, at.radius * scale, at.radius * scale);
        }
    }

	// Update is called once per frame
	void Update () {
	
	}
    */
}
