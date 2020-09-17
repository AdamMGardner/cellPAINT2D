using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityStandardAssets.ImageEffects;
using System.Runtime.InteropServices;
using Crosstales.FB;
using System.Text;


public class QuitGame : MonoBehaviour
{
//#if UNITY_WEBGL

//        [DllImport("__Internal")]
//        private static extern void getFileFromBrowser(string objectName, string callbackFuncName);
        
//#endif


    string path;
    float lastTime;
    public string lastScreenshot = "";
    public GameObject ui;
    public GameObject manager;
    public GameObject backgroundImageManagerContainer;
    private BackgroundImageManager backgroundImageManager;
    public GUISkin skinWithListStyle;

    public Slider cs;
    //public MotionBlur mb;
    public Camera myCamera;
    public bool use_native_browser;
    private bool show_browser_save = false;
    private bool save_scene = false;
    private bool show_browser_load = false;
    private bool load_scene = false;
    private bool show_browser_save_image = false;
    private bool load_image = false;
    private bool save_image = false;
    private bool screen_grab = false;
    private string m_LabelContent;
    private bool isTransparent = false;

    protected string m_textPath;
    //protected FileBrowser m_fileBrowser;

    private byte[] current_bytes;
    private Texture2D current_screenShot;
    private Rect windowRect;// = new Rect(10, 10, 100, 100);
    
    [SerializeField]
    protected Texture2D m_directoryImage,
                        m_fileImage;


    protected void OnGUI()
    {
        
    }

    protected void Update()
    {

    }

    /* unity brwoser for linux */
    // Open a file browser to save and load files
    private void OpenFileBrowser()
    {
        GetComponent<GracesGames.SimpleFileBrowser.Scripts.DemoCaller > ().OpenFileBrowser((!load_scene), FileSelectedCallback);
    }

    protected void FileSelectedCallback(string path)
    {
        Debug.Log("You are in the callback with file path: " + path);
        m_textPath = path;
        if (save_scene) { SaveScene_cb(path); }
        if (load_scene) { LoadScene_cb(path); }
        if (save_image) { SaveImage_cb(path); }
        if (load_image) { LoadImage_cb(path); }
        save_scene = false;
        load_scene = false;
        load_image = false;
        save_image = false;
       // SceneManager.Instance.mask_ui = false;
    }

    void Start() {
        //path = Application.dataPath;
        //if (!mb) mb = Camera.main.GetComponent<MotionBlur>();
        backgroundImageManager = backgroundImageManagerContainer.GetComponent<BackgroundImageManager>();
    }

    public void Exit()
    {
        //Need a warning popup here
        Application.Quit();
    }

    public string ScreenShotName(int width, int height)
    {

        string strPath = "";

        strPath = string.Format("{0}/screen_{1}x{2}_{3}.png",
                             path,
                             width, height,
                                       System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        lastScreenshot = strPath;

        return strPath;
    }

    public void ToggleFullScreen(bool value)
    {
        Screen.fullScreen = value;
    }
    
    public void SaveImage_cb(string path) {
        //if (ui != null) { ui.SetActive(false); }
        if (path == null) return;
        TakeScreenShot();
        if (current_bytes == null) return;
        current_bytes = current_screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, current_bytes);
        Debug.Log(string.Format("Screenshot saved to: {0}", path));
        //if (ui != null) { ui.SetActive(true); }
        Application.OpenURL(path);//
        //System.Diagnostics.Process.Start(path);
    }
    
    public void LoadImage_cb(string path) 
    {
        if (path == null) return;
        current_bytes = File.ReadAllBytes(path);
        Texture2D backgroundImage = new Texture2D (2,2);
        backgroundImage.LoadImage(current_bytes);

        backgroundImageManager.backgroundImageContainer.GetComponent<Renderer>().material.mainTexture = backgroundImage;
        backgroundImageManager.backgroundImageOriginalResoution = new Vector2 (backgroundImage.width, backgroundImage.height);
        backgroundImageManager.backgroundImageContainer.transform.localScale = new Vector3 (backgroundImage.width/100, 1,  backgroundImage.height/100);

        backgroundImageManager.backgroundImageContainer.SetActive(true);
        backgroundImageManager.showBackgroundImageToggle.isOn = true;
    }

    public void OriginalScreenShot() {
        int resWidth = Screen.width * 2;
        int resHeight = Screen.height * 2;
        Debug.Log("You are in the take screenshot function");
        int resWidthN = resWidth;
        int resHeightN = resHeight;

        RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
        myCamera.targetTexture = rt;

        TextureFormat tFormat;
        if (isTransparent)
            tFormat = TextureFormat.ARGB32;
        else
            tFormat = TextureFormat.RGB24;

        current_screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);

        myCamera.Render();
        RenderTexture.active = rt;

        current_screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
        myCamera.targetTexture = null;
        RenderTexture.active = null;
        //if (ui != null) { ui.SetActive(true); }
        screen_grab = true;
    }

    public void TakeScreenShot()
    {
        //  
        //myCamera.gameObject.SetActive(true);

        //super lame sloution to f-actin bug
        //GameObject tempObject = SceneManager.Instance.myPrefab;

        Debug.Log("You are in the take screenshot function");
       // if (ui != null) { ui.SetActive(false); }
        //var mask = myCamera.cullingMask;
        //myCamera.cullingMask = LayerMask.NameToLayer("Everything");
        
        int resWidth = Screen.width*2;
        int resHeight = Screen.height*2;

        int resWidthN = resWidth;
        int resHeightN = resHeight;

        //RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
        RenderTexture rt = RenderTexture.GetTemporary(resWidthN, resHeightN, 24);
        myCamera.targetTexture = rt;

        TextureFormat tFormat;
        tFormat = TextureFormat.ARGB32;
        if (isTransparent)
            tFormat = TextureFormat.ARGB32;
        else
            tFormat = TextureFormat.RGB24;
        
        current_screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);

        myCamera.Render();
        current_screenShot.Apply();
        var temp = RenderTexture.active;
        RenderTexture.active = rt;
        current_screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        RenderTexture.active = temp;
        current_bytes = current_screenShot.EncodeToPNG();
        // RenderTexture.active = null;
        //myCamera.cullingMask = LayerMask.NameToLayer("renderCameras");
        myCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);
        //rt = null;
        //Debug.Log("The culling mask is: " + myCamera.cullingMask);
        //if (ui != null) { ui.SetActive(true); }
        screen_grab = true;
        //myCamera.gameObject.SetActive(false);

        //super lame sloution to f-actin bug
        //SceneManager.Instance.myPrefab = tempObject;

        // if (ui) ui.SetActive(true);
       // myCamera.cullingMask = mask;
    }

    void LoadFromString(string lines)
    {
        string[] line = lines.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        LoadFromLines(line);
    }


    public void SaveScene()
    {
        load_scene = false;
        save_image = false;
        load_image = false;
        save_scene = true;
        if (!use_native_browser) OpenFileBrowser();// GetImage.GetImageFromUserAsync(gameObject.name, "LoadFromString");
        else {
            string filePath = FileBrowser.SaveFile("Save file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MySaveFile", "txt");
            FileSelectedCallback(filePath);
            /*FileBrowser.SaveFilePanel("Save Scene File", "Save file to....", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "File Name", new string[] { "txt" }, null, (bool canceled, string filePath) =>
            {
                if (canceled)
                {
                    m_LabelContent = "[Save File] Canceled";
                    Debug.Log("[Save File] Canceled");
                    return;
                }

                m_LabelContent = "[Save File] You can now save the file to the path: " + filePath;
                Debug.Log("[Save File] You can now save the file to the path: " + filePath);

                FileSelectedCallback(filePath);
            });*/
        }
    }

    public void LoadImage()
    {
        load_scene = false;
        load_image = true;
        save_image = false;
        save_scene = false;
        //#if UNITY_WEBGL
        if (!use_native_browser) OpenFileBrowser();//// GetImage.GetImageFromUserAsync(gameObject.name, "LoadFromString");
        //getFileFromBrowser(gameObject.name, "LoadFromLines");
        //#else
        else {
            string filePath = FileBrowser.OpenSingleFile("Open single file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "png");
            /*FileBrowser.OpenFilePanel("Open file Title", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), new string[] { "txt","json" }, null, (bool canceled, string filePath) => {
                if (canceled)
                {
                    m_LabelContent = "[Open File] Canceled";
                    Debug.Log("[Open File] Canceled");
                    return;
                }

                m_LabelContent = "[Open File] Selected file: " + filePath;
                Debug.Log("[Open File] Selected file: " + filePath);
                FileSelectedCallback(filePath);
            });*/
            FileSelectedCallback(filePath);
        }
    }

    public void LoadScene()
    {
        load_scene = true;
        load_image = false;
        save_image = false;
        save_scene = false;
        //#if UNITY_WEBGL
        if (!use_native_browser) OpenFileBrowser();//// GetImage.GetImageFromUserAsync(gameObject.name, "LoadFromString");
        //getFileFromBrowser(gameObject.name, "LoadFromLines");
        //#else
        else {
            string filePath = FileBrowser.OpenSingleFile("Open single file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "txt", "json");
            /*FileBrowser.OpenFilePanel("Open file Title", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), new string[] { "txt","json" }, null, (bool canceled, string filePath) => {
                if (canceled)
                {
                    m_LabelContent = "[Open File] Canceled";
                    Debug.Log("[Open File] Canceled");
                    return;
                }

                m_LabelContent = "[Open File] Selected file: " + filePath;
                Debug.Log("[Open File] Selected file: " + filePath);
                FileSelectedCallback(filePath);
            });*/
            FileSelectedCallback(filePath);
        }
    }

    public void SaveImage()
    {
        save_scene = false;
        load_scene = false;
        
        //SceneManager.Instance.mask_ui = true;
        load_image = false;
        save_image = true;
        if (!use_native_browser) OpenFileBrowser();// GetImage.GetImageFromUserAsync(gameObject.name, "LoadFromString");
        //getFileFromBrowser(gameObject.name, "LoadFromLines");
        else {
            string filePath = FileBrowser.SaveFile("Save file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Image", "png");
            FileSelectedCallback(filePath);
            /*FileBrowser.SaveFilePanel("Save Screenshot", "Save image to....", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Image Name", new string[] { "png" }, null, (bool canceled, string filePath) =>
            {
                if (canceled)
                {
                    m_LabelContent = "[Save File] Canceled";
                    Debug.Log("[Save File] Canceled");
                    return;
                }

                m_LabelContent = "[Save File] You can now save the file to the path: " + filePath;
                Debug.Log("[Save File] You can now save the file to the path: " + filePath);
                FileSelectedCallback(filePath);
            });*/
        }
    }
    public void SaveScene_cb(string filename)
    {
        string sep = ",";
        Debug.Log("You are in the saveScene_cb with filename: " + filename);
        if (filename == null) return;
        string text = "";
        string towrite = "";
        Debug.Log("should save in " + filename);
        //first loop over all material and check if default color or not
        text = "";
        /*add extra ingredients data e.g. scale2d and offsetY
        upon loading add the ingredient if not there already
        */
        int extra_ingredient = Manager.Instance.additional_ingredients_names.Count;
        towrite += extra_ingredient.ToString()+ "\r\n";
        //name,spritename,scale2d,yoffset,issurf,isfiber,comp
        for (var i=0;i<extra_ingredient;i++){
            var name = Manager.Instance.additional_ingredients_names[0];
            var ind = Manager.Instance.ingredients_names.IndexOf(name);
            var sprite_name = Manager.Instance.sprites_names[ind];
            var prefab = Manager.Instance.all_prefab[name];
            var props = prefab.GetComponent<PrefabProperties>();
            var issurf = (props.is_surface)?"1":"0";
            var isfiber = (props.is_fiber)?"1":"0";
            var comp = Manager.Instance.recipeUI.Compartments.IndexOf(props.compartment);
            towrite +=name+sep+sprite_name+sep+props.scale2d.ToString()+sep+props.y_offset.ToString()+sep; 
            towrite +=issurf+sep+isfiber+sep+comp+ "\r\n";
        }
        int mat_count = 0;
        /*Write the material colors values*/
        foreach ( var keyvalue in Manager.Instance.prefab_materials)
        {
            if (keyvalue.Value == null) continue;
            if (!Manager.Instance.all_prefab.ContainsKey(keyvalue.Key)) continue;
            PrefabProperties props = Manager.Instance.all_prefab[keyvalue.Key].GetComponent<PrefabProperties>();
            if (keyvalue.Value.color != props.Default_Sprite_Color) {
                text += keyvalue.Key+sep+ keyvalue.Value.color.r.ToString() + sep + keyvalue.Value.color.g.ToString() + sep + keyvalue.Value.color.b.ToString()+"\r\n";
                mat_count++;
            }
        }
        towrite += mat_count.ToString()+ "\r\n";
        towrite += text;
        /*loop over every instance and save xyz,r,name.*/
        int count = 0;
        float angle = 0.0F;
        Vector3 axis = Vector3.zero;
        text = "";
        for (int i = 0; i < Manager.Instance.everything.Count; i++)//Manager.Instance.rbCount
        {
            Rigidbody2D player = Manager.Instance.everything[i];
            if (player == null) continue;
            PrefabProperties props = player.gameObject.GetComponent<PrefabProperties>();
            text += player.position.x.ToString()+sep+ player.position.y.ToString()+sep+ player.gameObject.transform.position.z+sep;
            text += player.rotation.ToString() + sep;
            text += props.name + sep;
            text += props.order + sep;
            text += (player.bodyType==RigidbodyType2D.Static)? "1" : "0" + sep;//is it pinned
            text += (player.simulated)? "1" : "0";//is it ghosted
            //text += (player.isKinematic)? "1" : "0";//is it pinned or should we use the ispin ?
            string g = sep+"n"+ sep+"0";
            PrefabGroup pg = player.gameObject.GetComponentInParent<PrefabGroup>();
            if (pg != null){
                g = sep+pg.name;
                g += sep+pg.instance_id.ToString();
            }
            text += g;
            text += "\r\n";
            count++;
        }
        //write rb count, fiber chain count, surface object count
        towrite += count.ToString() + sep + 
            Manager.Instance.fiber_parents.Count.ToString() + sep +
            Manager.Instance.surface_objects.Count.ToString()+ "\r\n";
        towrite += text;
        
        //Handle the fiber. loop over every chain parent
        for (int i = 0; i < Manager.Instance.fiber_parents.Count;i++) {
            //firs line is name and nbPoints
            //is it closed ?
            towrite += Manager.Instance.fiber_parents[i].name+sep+
                Manager.Instance.fiber_parents[i].transform.childCount.ToString();
            string g = sep+"n"+ sep+"0";
            PrefabGroup pg = Manager.Instance.fiber_parents[i].gameObject.GetComponentInParent<PrefabGroup>();
            if (pg != null){
                g = sep+pg.name;
                g += sep+pg.instance_id.ToString();
            }
            towrite += g;
            towrite +="\r\n";
            //chain segments or bound object e.g. nucleocapside
            //fiberinstance order instead
            //for (int j = 0; j < Manager.Instance.fiber_parents[i].transform.childCount; j++)
            for (int j=0;j < Manager.Instance.fibers_instances[i].Count ;j++)
            {
                GameObject ch = Manager.Instance.fibers_instances[i][j];
                //write down posxyz, r, bind 0/1
                //GameObject ch = Manager.Instance.fiber_parents[i].transform.GetChild(j).gameObject;
                Rigidbody2D player = ch.GetComponent<Rigidbody2D>();
                towrite += player.position.x.ToString() + sep + player.position.y.ToString() +sep+ ch.transform.position.z+sep;
                towrite += player.rotation.ToString() + sep;
                //player.gameObject.transform.rotation.ToAngleAxis(out angle, out axis);
                //towrite += angle.ToString() + " ";
                if (ch.GetComponent<PrefabProperties>().is_bound)
                {
                    towrite += player.gameObject.GetComponent<PrefabProperties>().name+sep;
                }
                else { towrite += "0"+sep; }
                towrite += (player.bodyType == RigidbodyType2D.Static) ? "1" : "0" + sep;//is it pinned
                towrite += (player.simulated)? "1" : "0";//is it ghosted
                towrite += "\r\n";
            }
        }
        //surface object
        for (int i = 0; i < Manager.Instance.surface_objects.Count; i++)
        {
            Rigidbody2D player = Manager.Instance.surface_objects[i].GetComponent<Rigidbody2D>();
            towrite += player.position.x.ToString() + sep + player.position.y.ToString() + sep + Manager.Instance.surface_objects[i].transform.position.z + sep;
            towrite += player.rotation.ToString() + sep;
            towrite += Manager.Instance.surface_objects[i].GetComponent<PrefabProperties>().name + sep;
            towrite += (player.bodyType == RigidbodyType2D.Static) ? "1" : "0" + sep;
            towrite += (player.simulated)? "1" : "0";//is it ghosted
            string g = sep+"n"+ sep+"0";
            PrefabGroup pg = player.gameObject.GetComponentInParent<PrefabGroup>();
            if (pg != null){
                g = sep+pg.name;
                g += sep+pg.instance_id.ToString();
            }
            towrite += g;
            towrite += "\r\n";
        }

        int nLink = Manager.Instance.attached.Count/2;
        towrite += nLink.ToString() + "\r\n";
        for (int i = 0; i < nLink; i++) {
            //write xyzid
            SpringJoint2D jt1 = Manager.Instance.attached[i*2].GetComponent<SpringJoint2D>();//HighlightManager.Instance.pinned_to_bonds[i];
            Vector3 p1 = jt1.anchor;
            Vector3 p2 = jt1.connectedAnchor;
            Debug.Log(i.ToString()+" retrieve for 1 " + jt1.gameObject.name);
            string id1 = Manager.Instance.FindIdString(jt1.gameObject);
            Debug.Log(i.ToString() + " retrieve for 2 " + jt1.connectedBody.gameObject.name);
            string id2 = Manager.Instance.FindIdString(jt1.connectedBody.gameObject);
            if (id1 == "" || id2 == "") Debug.Log("problem with jt " + i.ToString());
            else Debug.Log(id1+" "+ id2);
            towrite += p1.x.ToString() + sep + p1.y.ToString() + sep + p1.z.ToString() + sep + id1 + sep;
            towrite += p2.x.ToString() + sep + p2.y.ToString() + sep + p2.z.ToString() + sep + id2 + "\r\n";
        }
        System.IO.File.WriteAllText(filename, towrite);
    }

    public void SaveScene_original_cb(string filename)
    {
        //what about group and new ingredient, look at the code from the 3D branch.
        string sep = ",";
        Debug.Log("You are in the saveScene_cb with filename: " + filename);
        if (filename == null) return;
        string text = "";
        string towrite = "";
        Debug.Log("should save in " + filename);
        //first loop over all material and check if default color or not
        int mat_count = 0;

        /*Write the material colors values*/
        foreach ( var keyvalue in Manager.Instance.prefab_materials)
        {
            Debug.Log(keyvalue.Key);
            Debug.Log(keyvalue.Value);
            if (keyvalue.Value == null) continue;
            if (!Manager.Instance.all_prefab.ContainsKey(keyvalue.Key)) continue;
            PrefabProperties props = Manager.Instance.all_prefab[keyvalue.Key].GetComponent<PrefabProperties>();
            if (keyvalue.Value.color != props.Default_Sprite_Color) {
                text += keyvalue.Key+sep+ keyvalue.Value.color.r.ToString() + sep + keyvalue.Value.color.g.ToString() + sep + keyvalue.Value.color.b.ToString()+"\r\n";
                mat_count++;
            }
        }
        towrite += mat_count.ToString()+ "\r\n";
        towrite += text;
        /*loop over every instance and save xyz,r,name.*/
        int count = 0;
        float angle = 0.0F;
        Vector3 axis = Vector3.zero;

        text = "";
        for (int i = 0; i < Manager.Instance.everything.Count; i++)
        {
            Rigidbody2D player = Manager.Instance.everything[i];
            if (player == null) continue;
            PrefabProperties props = player.gameObject.GetComponent<PrefabProperties>();
            //Debug.Log(player.position.ToString() + " " + player.rotation.ToString() + " " + player.gameObject.name);
            text += player.position.x.ToString()+sep+ player.position.y.ToString()+sep+ player.gameObject.transform.position.z+sep;
            //player.gameObject.transform.rotation.ToAngleAxis(out angle, out axis);
            //text += angle.ToString()+ " ";
            text += player.rotation.ToString() + sep;
            text += props.name + sep;
            text += props.order + sep;
            text += (player.bodyType==RigidbodyType2D.Static)? "1" : "0";//is it pinned
            text += "\r\n";
            count++;
        }
        //write rb count, fiber chain count, surface object count
        towrite += count.ToString() + sep + 
            Manager.Instance.fiber_parents.Count.ToString() + sep +
            Manager.Instance.surface_objects.Count.ToString()+ "\r\n";
        towrite += text;
        
        //Handle the fiber. loop over every chain parent
        for (int i = 0; i < Manager.Instance.fiber_parents.Count;i++) {
            //firs line is name and nbPoints
            //is it closed ?
            towrite += Manager.Instance.fiber_parents[i].name+sep+
                Manager.Instance.fiber_parents[i].transform.childCount.ToString()+ 
                "\r\n";
            //chain segments or bound object e.g. nucleocapside
            for (int j = 0; j < Manager.Instance.fiber_parents[i].transform.childCount; j++)
            {
                //write down posxyz, r, bind 0/1
                GameObject ch = Manager.Instance.fiber_parents[i].transform.GetChild(j).gameObject;
                Rigidbody2D player = ch.GetComponent<Rigidbody2D>();
                towrite += player.position.x.ToString() + sep + player.position.y.ToString() +sep+ ch.transform.position.z+sep;
                towrite += player.rotation.ToString() + sep;
                //player.gameObject.transform.rotation.ToAngleAxis(out angle, out axis);
                //towrite += angle.ToString() + " ";

                if (ch.GetComponent<PrefabProperties>().is_bound)
                {
                    towrite += player.gameObject.GetComponent<PrefabProperties>().name+sep;
                }
                else { towrite += "0"+sep; }
                towrite += (player.bodyType == RigidbodyType2D.Static) ? "1" : "0";//is it pinned
                towrite += "\r\n";
            }
        }
        //surface object
        for (int i = 0; i < Manager.Instance.surface_objects.Count; i++)
        {
            Rigidbody2D player = Manager.Instance.surface_objects[i].GetComponent<Rigidbody2D>();
            towrite += player.position.x.ToString() + sep + player.position.y.ToString() + sep + Manager.Instance.surface_objects[i].transform.position.z + sep;
            towrite += player.rotation.ToString() + sep;
            towrite += Manager.Instance.surface_objects[i].GetComponent<PrefabProperties>().name + sep;
            towrite += (player.bodyType == RigidbodyType2D.Static) ? "1" : "0";
            towrite += "\r\n";
        }
        int nLink = Manager.Instance.attached.Count/2;
        towrite += nLink.ToString() + "\r\n";
        for (int i = 0; i < nLink; i++) {
            //write xyzid
            SpringJoint2D jt1 = Manager.Instance.attached[i*2].GetComponent<SpringJoint2D>();//HighlightManager.Instance.pinned_to_bonds[i];
            Vector3 p1 = jt1.anchor;
            Vector3 p2 = jt1.connectedAnchor;
            Debug.Log(i.ToString()+" retrieve for 1 " + jt1.gameObject.name);
            string id1 = Manager.Instance.FindIdString(jt1.gameObject);
            Debug.Log(i.ToString() + " retrieve for 2 " + jt1.connectedBody.gameObject.name);
            string id2 = Manager.Instance.FindIdString(jt1.connectedBody.gameObject);
            if (id1 == "" || id2 == "") Debug.Log("problem with jt " + i.ToString());
            else Debug.Log(id1+" "+ id2);
            towrite += p1.x.ToString() + sep + p1.y.ToString() + sep + p1.z.ToString() + sep + id1 + sep;
            towrite += p2.x.ToString() + sep + p2.y.ToString() + sep + p2.z.ToString() + sep + id2 + "\r\n";
        }
        //gather if close or not
        //gather children elem in order and their Zdepth (e.g.NucleicAcidDepth)
        //if binder, try to insert it proper position in order
        System.IO.File.WriteAllText(filename, towrite);
    }

    public string checkFiberName(string name) {
        //example HIV_CAhex_chain_1 _Closed 24
        var elems = name.Split("_"[0]);
        //should  be 2 or 3 preffabname_chain_n_close
        bool closed = name.EndsWith("_Closed");
        if (closed) {
            if (elems.Length >= 5) {
                return elems[0] + "_" + elems[1];
            }
        }
        else {
            if (elems.Length >= 4)
            {
                return elems[0] + "_" + elems[1];
            }
        }
        return elems[0];
    }

    public void LoadFromLines_1(string[] lines) {
        var current_name = "";
        if (Manager.Instance.myPrefab)
            current_name = Manager.Instance.myPrefab.name;
        int lineCounter = 0;
        //split with space and get x y r name
        //first line is nb of material color to overwrote
        string[] elems = lines[lineCounter].Split(" "[0]);
        int n_mat = int.Parse(elems[0]);
        Debug.Log("found n_mat "+n_mat.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + n_mat; i++)
        {
            elems = lines[i].Split(" "[0]);
            var name = elems[0];
            if (name == "Draw") {
                name = "Draw DNA";
                elems[1] = elems[2];
                elems[2] = elems[3];
                elems[3] = elems[4];
            }
            var r = float.Parse(elems[1]);
            var g = float.Parse(elems[2]);
            var b = float.Parse(elems[3]);
            if (!Manager.Instance.prefab_materials.ContainsKey(name))
            {
                Manager.Instance.prefab_materials.Add(name, Manager.Instance.createNewSpriteMaterial(name));
            }//else build the material ?
            Manager.Instance.prefab_materials[name].color = new Color(r, g, b);
        }
        lineCounter += n_mat;
        //first line is nb rb, and nb fiber
        elems = lines[lineCounter].Split(" "[0]);
        int nObj = int.Parse(elems[0]);             //nb instances
        int nFiber = int.Parse(elems[1]);           //nb chains fiber
        int nSurfaceOfFiber = int.Parse(elems[2]);  //nb surface objects
        Debug.Log("found nObj " + nObj.ToString() + " " + nFiber.ToString() + " " + nSurfaceOfFiber.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + nObj; i++)
        {
            elems = lines[i].Split(" "[0]);
            var x = float.Parse(elems[0]);
            var y = float.Parse(elems[1]);
            var z = float.Parse(elems[2]);
            var zangle = float.Parse(elems[3]);
            var name = elems[4];
            var order = int.Parse(elems[5]);
            var kinematic = int.Parse(elems[6]);
            Manager.Instance.restoreOneInstance(name, new Vector3(x, y, z), zangle, order, false, (kinematic == 1));
        }
        //in case of fiber need to do the random choice of sprite id, or save it
        lineCounter += nObj;
        GameObject attached = null;
        GameObject fiber = null;
        for (int i = 0; i < nFiber; i++)
        {
            attached = null;
            fiber = null;
            Debug.Log(lines[lineCounter]);
            elems = lines[lineCounter].Split(" "[0]);
            lineCounter++;
            var name = elems[0];
            int nPoints = int.Parse(elems[1]);
            var prefabName = checkFiberName(name);
            if (prefabName == "DrawDNA") prefabName = "Draw DNA";
            Debug.Log(name + " fiber  ??  " + prefabName);
            Manager.Instance.AddFiberParent(prefabName);
            bool closed = name.EndsWith("_Closed");
            for (int j = 0; j < nPoints; j++)
            {
                elems = lines[lineCounter].Split(" "[0]);
                var x = float.Parse(elems[0]);
                var y = float.Parse(elems[1]);
                var z = float.Parse(elems[2]);
                var zangle = float.Parse(elems[3]);
                var bounded = elems[4];
                var kinematic = int.Parse(elems[5]);
                if (bounded == "0")
                {
                    if (attached != null)
                    {
                        fiber = Manager.Instance.restoreAttachFiber(new Vector3(x, y, z),
                            zangle, attached);
                        attached = null;
                    }
                    else
                    {
                        //make sure pinned will be at the proper position
                        fiber = Manager.Instance.oneInstanceFiberPos(new Vector3(x, y, z), zangle);
                        Quaternion rotation = Quaternion.AngleAxis(zangle, Vector3.forward);
                        fiber.transform.position = new Vector3(x, y, z);
                        fiber.transform.rotation = rotation;
                    }

                    if (kinematic == 1)
                        Manager.Instance.pin_object(fiber, kinematic == 1); //SceneManager.Instance.pinInstance(fiber);
                    if ( fiber.tag != "membrane" ) Manager.Instance.changeColorAndOrderFromDepth(z, fiber);
                    else fiber.GetComponent<SpriteRenderer>().sortingOrder = 0;
                }
                else {
                    attached = Manager.Instance.restoreAttachments(bounded, new Vector3(x, y, z), zangle, fiber);
                    Debug.Log("attached to fiber " + attached.name + " " + kinematic.ToString());
                    if (kinematic == 1)
                        Manager.Instance.pin_object(attached, kinematic == 1); //SceneManager.Instance.pinInstance(attached);
                }
                lineCounter++;
            }
            if (closed)
            {
                Manager.Instance.closePersistence();
            }

            if (Manager.Instance.myPrefab.GetComponent<PrefabProperties>().light_fiber)
            {
                foreach (Transform child in Manager.Instance.fiber_parent.transform)
                {
                    Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
                    rb.mass = 5.0f;
                    rb.drag = 1.0f;
                    rb.angularDrag = 0.05f;
                }
            }
        }
        for (int i = 0; i < nSurfaceOfFiber; i++)
        {
            elems = lines[lineCounter].Split(" "[0]);
            var x = float.Parse(elems[0]);
            var y = float.Parse(elems[1]);
            var z = float.Parse(elems[2]);
            var zangle = float.Parse(elems[3]);
            var name = elems[4];
            var kinematic = int.Parse(elems[5]);
            Manager.Instance.restoreOneInstance(name, new Vector3(x, y, z), zangle, 0, true, (kinematic == 1));
            lineCounter++;
        }
        //reset ui and manager
        if (current_name!="") Manager.Instance.SwitchPrefabFromName(current_name);
    }

    public void LoadFromLines(string[] lines) {
        string sep = ",";
        var current_name = "";
        if (Manager.Instance.myPrefab)
            current_name = Manager.Instance.myPrefab.name;
        int lineCounter = 0;
        //split with space and get x y r name
        //first line is nb of material color to overwrote
        string[] elems = lines[lineCounter].Split(sep[0]);
        int extra_ingredient = int.Parse(elems[0]);
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + extra_ingredient; i++)
        {
            elems = lines[i].Split(sep[0]);
            var name = elems[0];
            var sprite_name = elems[1];
            var scale2d = float.Parse(elems[2]);
            var yoffset = float.Parse(elems[3]);
            var issurf = (elems[4] == "0")? false : true;
            var isfiber = (elems[5] == "0")? false : true;
            var comp = int.Parse(elems[6] );
            if (!Manager.Instance.ingredients_names.Contains(name)) {
                Manager.Instance.recipeUI.AddOneIngredient(name, sprite_name, scale2d, -yoffset, issurf, isfiber, comp);
            }
        }
        lineCounter += extra_ingredient;
        elems = lines[lineCounter].Split(sep[0]);
        int n_mat = int.Parse(elems[0]);
        Debug.Log("found n_mat "+n_mat.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + n_mat; i++)
        {
            elems = lines[i].Split(sep[0]);
            var name = elems[0];
            if (name == "Draw") {
                name = "Draw DNA";
                elems[1] = elems[2];
                elems[2] = elems[3];
                elems[3] = elems[4];
            }
            var r = float.Parse(elems[1]);
            var g = float.Parse(elems[2]);
            var b = float.Parse(elems[3]);
            if (!Manager.Instance.prefab_materials.ContainsKey(name))
            {
                Manager.Instance.prefab_materials.Add(name, Manager.Instance.createNewSpriteMaterial(name));
            }//else build the material ?
            Manager.Instance.prefab_materials[name].color = new Color(r, g, b);
        }
        lineCounter += n_mat;
        //first line is nb rb, and nb fiber
        elems = lines[lineCounter].Split(sep[0]);
        int nObj = int.Parse(elems[0]);             //nb instances
        int nFiber = int.Parse(elems[1]);           //nb chains fiber
        int nSurfaceOfFiber = int.Parse(elems[2]);  //nb surface objects
        Dictionary<string,Group> groups = new Dictionary<string,Group>();
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + nObj; i++)
        {
            // 9.848226,21.78014,0.0004882813,197.4075,LDL,0,0
            elems = lines[i].Split(sep[0]);
            var x = float.Parse(elems[0]);//Debug.Log(x);
            var y = float.Parse(elems[1]);//Debug.Log(y);
            var z = float.Parse(elems[2]);//Debug.Log(z);
            var zangle = float.Parse(elems[3]);//Debug.Log(zangle);
            var name = elems[4];//Debug.Log(name);
            var order = int.Parse(elems[5]);//Debug.Log(order);
            var kinematic = int.Parse(elems[6]);//Debug.Log(kinematic);
            var ghost = int.Parse(elems[7]); 
            var group_name = elems[8];
            var group_id = int.Parse(elems[9]);           
            var newObject = Manager.Instance.restoreOneInstance(name, new Vector3(x, y, z), zangle, order, false, (kinematic == 1), (ghost == 0));
            GroupManager.Get.UpdateGroupFromObject(newObject, group_name, group_id);
        }
        //in case of fiber need to do the random choice of sprite id, or save it
        lineCounter += nObj;
        GameObject attached = null;
        GameObject fiber = null;
        Debug.Log("found nFiber " + nFiber.ToString());
        for (int i = 0; i < nFiber; i++)
        {
            attached = null;
            fiber = null;
            Debug.Log(lines[lineCounter]);
            elems = lines[lineCounter].Split(sep[0]);
            lineCounter++;
            var name = elems[0];
            int nPoints = int.Parse(elems[1]);
            var prefabName = checkFiberName(name);
            if (prefabName == "DrawDNA") prefabName = "Draw DNA";
            Debug.Log(name + " fiber  ??  " + prefabName);
            GameObject fp = Manager.Instance.AddFiberParent(prefabName);
            var group_name = elems[2];
            var group_id = int.Parse(elems[3]);            
            bool closed = name.EndsWith("_Closed");
            for (int j = 0; j < nPoints; j++)
            {
                elems = lines[lineCounter].Split(sep[0]);
                var x = float.Parse(elems[0]);
                var y = float.Parse(elems[1]);
                var z = float.Parse(elems[2]);
                var zangle = float.Parse(elems[3]);
                var bounded = elems[4];
                var kinematic = int.Parse(elems[5]);
                var ghost = int.Parse(elems[6]); 
                if (bounded == "0")
                {
                    if (attached != null)
                    {
                        fiber = Manager.Instance.restoreAttachFiber(new Vector3(x, y, z),
                            zangle, attached);
                        attached = null;
                    }
                    else
                    {
                        //make sure pinned will be at the proper position
                        fiber = Manager.Instance.oneInstanceFiberPos(new Vector3(x, y, z), zangle);
                        Quaternion rotation = Quaternion.AngleAxis(zangle, Vector3.forward);
                        fiber.transform.position = new Vector3(x, y, z);
                        fiber.transform.rotation = rotation;
                    }

                    if (kinematic == 1)
                        Manager.Instance.pin_object(fiber, kinematic == 1); //SceneManager.Instance.pinInstance(fiber);
                    if ( fiber.tag != "membrane" ) Manager.Instance.changeColorAndOrderFromDepth(z, fiber);
                    else fiber.GetComponent<SpriteRenderer>().sortingOrder = 0;
                }
                else {
                    attached = Manager.Instance.restoreAttachments(bounded, new Vector3(x, y, z), zangle, fiber);
                    Debug.Log("attached to fiber " + attached.name + " " + kinematic.ToString());
                    if (kinematic == 1)
                        Manager.Instance.pin_object(attached, kinematic == 1); //SceneManager.Instance.pinInstance(attached);
                }
                if (ghost==0){
                    fiber.GetComponent<Rigidbody2D>().simulated = false;
                }
                lineCounter++;
            }
            if (closed)
            {
                Manager.Instance.closePersistence();
            }

            if (Manager.Instance.myPrefab.GetComponent<PrefabProperties>().light_fiber)
            {
                foreach (Transform child in Manager.Instance.fiber_parent.transform)
                {
                    Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
                    rb.mass = 5.0f;
                    rb.drag = 1.0f;
                    rb.angularDrag = 0.05f;
                }
            }
            GroupManager.Get.UpdateGroupFromObject(fp, group_name, group_id);
        }
        Debug.Log("found nSurfaceOfFiber " + nSurfaceOfFiber.ToString());
        for (int i = 0; i < nSurfaceOfFiber; i++)
        {
            elems = lines[lineCounter].Split(sep[0]);
            var x = float.Parse(elems[0]);
            var y = float.Parse(elems[1]);
            var z = float.Parse(elems[2]);
            var zangle = float.Parse(elems[3]);
            var name = elems[4];
            var kinematic = int.Parse(elems[5]);
            var ghost = int.Parse(elems[6]); 
            var group_name = elems[7];
            var group_id = int.Parse(elems[8]);
            var newObject = Manager.Instance.restoreOneInstance(name, new Vector3(x, y, z), zangle, 0, true, (kinematic == 1), (ghost == 0));
            GroupManager.Get.UpdateGroupFromObject(newObject, group_name, group_id); 
            lineCounter++;
        }
        //reset ui and manager
        elems = lines[lineCounter].Split(sep[0]);
        int nLink = int.Parse(elems[0]);
        lineCounter++;
        Debug.Log("found nLink " + nLink.ToString());
        for (int i = 0; i < nLink; i++)
        {
            //create a joints
            elems = lines[lineCounter].Split(sep[0]);
            var x1 = float.Parse(elems[0]);
            var y1 = float.Parse(elems[1]);
            var z1 = float.Parse(elems[2]);
            var id1 = elems[3];
            var x2 = float.Parse(elems[4]);
            var y2 = float.Parse(elems[5]);
            var z2 = float.Parse(elems[6]);
            var id2 = elems[7];
            Manager.Instance.restoreOneBond(new Vector3(x1, y1, z1),id1, new Vector3(x2, y2, z2),id2);
            lineCounter++;
        }
        Manager.Instance.UpdateGhostArea();   
        if (current_name!="") Manager.Instance.SwitchPrefabFromName(current_name);
        //restore groups
        GroupManager.Get.RestoreGroups();
    }

    public void LoadScene_cb(string filename)
    {
        Debug.Log("You are in the loadScene_cb with filename: " + filename);
        var ext = Path.GetExtension(filename);
        if (ext == ".txt")
        {
            var current_name = "";
            if (Manager.Instance.myPrefab)
                current_name = Manager.Instance.myPrefab.name;
            if (filename == null) return;
            if (!Manager.Instance.recipeUI.merge_upon_loading) Manager.Instance.Clear();
            Debug.Log("should load from " + filename);
            string[] lines = System.IO.File.ReadAllLines(filename);
            Debug.Log("should load from this many lines " + lines.Length.ToString());
            LoadFromLines(lines);
        }
        else if (ext == ".json")
        {
            //load json recipe

            if (!Manager.Instance.recipeUI.merge_upon_loading) Manager.Instance.Clear();
            Manager.Instance.recipeUI.LoadRecipe(filename);
        }
        else {
            //not supported
            Debug.Log("not supported");
        }
    }
}
