using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using UnityStandardAssets.ImageEffects;
using System.Runtime.InteropServices;
using Crosstales.FB;
using SimpleJSON;


public class QuitGame : MonoBehaviour
{
    public bool use_coroutine;
    public int frequency = 100;
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
    private string current_loaded_text;
    private Texture2D current_loaded_texture;
    protected string m_textPath;
    //protected FileBrowser m_fileBrowser;

    private byte[] current_bytes;
    private Texture2D current_screenShot;
    private Rect windowRect;// = new Rect(10, 10, 100, 100);
    private Task loading_task;

#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
    [DllImport("__Internal")]
    private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

#endif

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
        GetComponent<GracesGames.SimpleFileBrowser.Scripts.DemoCaller > ().OpenFileBrowser(save_scene, FileSelectedCallback);
    }

    private IEnumerator OutputRoutineText(string url) {
        var ext = Path.GetExtension(url);
        var loader = new WWW(url);
        yield return loader;
        current_loaded_text = loader.text;
        Debug.Log(url);
        Debug.Log(ext);
        if (ext == ".txt")
        {
            var current_name = "";
            if (Manager.Instance.myPrefab)
                current_name = Manager.Instance.myPrefab.name;
            if (!Manager.Instance.recipeUI.merge_upon_loading) Manager.Instance.Clear();
            Debug.Log("should load from " + url);
            LoadFromString(current_loaded_text);
        }
        else if (ext == ".json")
        {
            //load json recipe
            var resultData = JSONNode.Parse(current_loaded_text);
            if (Manager.Instance.recipeUI.merge_upon_loading) {
                Manager.Instance.recipeUI.MergeRecipe_cb(resultData);
            } else {
                Manager.Instance.Clear();
                Manager.Instance.recipeUI.Clear();
                Manager.Instance.recipeUI.LoadRecipe_cb(resultData);
            }
        }
        else {
            //not supported
            Debug.Log("OutputRoutineText not supported "+url);
            Debug.Log(ext);
        }
    }

    private IEnumerator OutputRoutineTexture(string url) {
        var loader = new WWW(url);
        yield return loader;
        current_loaded_texture = loader.texture;
        BackgroundImageManager.Get.AddBackgroundSprites(current_loaded_texture,url,Vector3.zero,1.0f,0.0f);
    }

    private IEnumerator OutputRoutineZip(string url) {
        var loader = new WWW(url);
        yield return loader;
        var zipdata = loader.bytes;
        if (use_coroutine) StartCoroutine(LoadFromZipDataCR(zipdata));
        else LoadFromZipData(zipdata);
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
    public void OnFileUpload(string url) {
        if (load_scene) { StartCoroutine(OutputRoutineZip(url));}
        if (load_image) { StartCoroutine(OutputRoutineTexture(url));}
        save_scene = false;
        load_scene = false;
        load_image = false;
        save_image = false;        
    }

    public void OnFileDownload() {}

    void Start() {
        //path = Application.dataPath;
        //if (!mb) mb = Camera.main.GetComponent<MotionBlur>();
        backgroundImageManager = backgroundImageManagerContainer.GetComponent<BackgroundImageManager>();
        Manager.Instance.recipeUI.use_coroutine = use_coroutine;
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
        //if (current_bytes == null) return;
        current_bytes = current_screenShot.EncodeToPNG();
#if UNITY_WEBGL && !UNITY_EDITOR
#else        
        System.IO.File.WriteAllBytes(path, current_bytes);
        Debug.Log(string.Format("Screenshot saved to: {0}", path));
        //if (ui != null) { ui.SetActive(true); }
        Application.OpenURL(path);//
        //System.Diagnostics.Process.Start(path);
#endif
    }
    
    public void LoadImage_cb(string path) 
    {
        if (path == null) return;
        BackgroundImageManager.Get.AddBackgroundSprites(path,Vector3.zero,1.0f,0.0f);
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
        if (use_coroutine) StartCoroutine(LoadFromLines_CR(line));
        else LoadFromLines(line);
    }

    IEnumerator LoadFromZipDataCR(byte[] data){
        //create a memorystream from the data
        string recipe_data_string="";
        string recipe_json_data="";
        using (var memoryStream = new MemoryStream())
        {
            memoryStream.Write(data, 0 , data.Length);
            // Set the position to the beginning of the stream.
            memoryStream.Seek(0, SeekOrigin.Begin);

            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read)){
                int counter = 0;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fname = entry.Name;
                    if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && entry.Name!="README.txt")
                    {
                        //recipe model
                        using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream)) {
                                    recipe_data_string = reader.ReadToEnd();
                                }                        
                    }
                    else if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) 
                    {
                        //recipe json
                        using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream)) {
                                    recipe_json_data = reader.ReadToEnd();
                                }                               
                    }
                    else {
                        //sprites images
                        using (var stream = entry.Open()){
                            var mem = new MemoryStream();
                            stream.CopyTo(mem);
                            byte[] truncated = mem.ToArray();//or .GetBuffer(); for no copy
                            Texture2D aTexture = new Texture2D (2,2);
                            aTexture.LoadImage(truncated);
                            Debug.Log("Adding a texture "+fname);
                            Debug.Log("Adding a texture exist ? "+Manager.Instance.sprites_textures.ContainsKey(fname).ToString());
                            if (Manager.Instance.sprites_textures.ContainsKey(fname)){
                                //update the sprite?
                                Debug.Log("what is the texture");
                                Debug.Log(Manager.Instance.sprites_textures[fname]);
                                //Manager.Instance.sprites_textures[entry.Name] = aTexture;
                                Debug.Log("OK");
                            }
                            else {
                                Debug.Log("add the texture "+fname);
                                Debug.Log(Manager.Instance.sprites_textures.Count.ToString());
                                Manager.Instance.sprites_textures.Add(fname,aTexture);
                                Debug.Log("OK");
                            }
                        }
                    }
                    UI_manager.Get.UpdatePB((float)counter/(float)(archive.Entries.Count),"loading archive entry "+entry.Name);       
                    yield return null;
                }
            }
        }
        if (recipe_json_data!="") {
            //load json recipe
            var resultData = JSONNode.Parse(recipe_json_data);
            if (Manager.Instance.recipeUI.merge_upon_loading) {
                if (use_coroutine) StartCoroutine(Manager.Instance.recipeUI.MergeRecipe_cb_CR(resultData));
                else Manager.Instance.recipeUI.MergeRecipe_cb(resultData);
            } else {
                Manager.Instance.Clear();
                Manager.Instance.recipeUI.Clear();
                if (use_coroutine) StartCoroutine(Manager.Instance.recipeUI.LoadRecipe_cb_CR(resultData));
                else Manager.Instance.recipeUI.LoadRecipe_cb(resultData);
            }            
        }        
        if (recipe_data_string!="") {
            if (!Manager.Instance.recipeUI.merge_upon_loading) {
                Manager.Instance.Clear();
            }
            Debug.Log("LoadFromZipDataCR LoadFromString");
            LoadFromString(recipe_data_string);
        }
    }
    void LoadFromZipData(byte[] data) {
        //create a memorystream from the data
        string recipe_data_string="";
        string recipe_json_data="";
        using (var memoryStream = new MemoryStream())
        {
            memoryStream.Write(data, 0 , data.Length);
            // Set the position to the beginning of the stream.
            memoryStream.Seek(0, SeekOrigin.Begin);

            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read)){
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fname = entry.Name;
                    if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && entry.Name!="README.txt")
                    {
                        //recipe model
                        using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream)) {
                                    recipe_data_string = reader.ReadToEnd();
                                }                        
                    }
                    else if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) 
                    {
                        //recipe json
                        using (var stream = entry.Open())
                                using (var reader = new StreamReader(stream)) {
                                    recipe_json_data = reader.ReadToEnd();
                                }                               
                    }
                    else {
                        //sprites images
                        using (var stream = entry.Open()){
                            var mem = new MemoryStream();
                            stream.CopyTo(mem);
                            byte[] truncated = mem.ToArray();//or .GetBuffer(); for no copy
                            Texture2D aTexture = new Texture2D (2,2);
                            aTexture.LoadImage(truncated);
                            Debug.Log("Adding a texture "+fname);
                            Debug.Log("Adding a texture exist ? "+Manager.Instance.sprites_textures.ContainsKey(fname).ToString());
                            if (Manager.Instance.sprites_textures.ContainsKey(fname)){
                                //update the sprite?
                                Debug.Log("what is the texture");
                                Debug.Log(Manager.Instance.sprites_textures[fname]);
                                //Manager.Instance.sprites_textures[entry.Name] = aTexture;
                                Debug.Log("OK");
                            }
                            else {
                                Debug.Log("add the texture "+fname);
                                Debug.Log(Manager.Instance.sprites_textures.Count.ToString());
                                Manager.Instance.sprites_textures.Add(fname,aTexture);
                                Debug.Log("OK");
                            }
                        }
                    } 
                }
            }
        }
        if (recipe_data_string!="") {
            if (!Manager.Instance.recipeUI.merge_upon_loading) {
                Manager.Instance.Clear();
            }
            Debug.Log("LoadFromZipData LoadFromString");
            LoadFromString(recipe_data_string);
        }
        else if (recipe_json_data!="") {
            //load json recipe
            var resultData = JSONNode.Parse(recipe_json_data);
            if (Manager.Instance.recipeUI.merge_upon_loading) {
                Manager.Instance.recipeUI.MergeRecipe_cb(resultData);
            } else {
                Manager.Instance.Clear();
                Manager.Instance.recipeUI.Clear();
                Manager.Instance.recipeUI.LoadRecipe_cb(resultData);
            }            
        }
    }
    byte[] SaveAsZipData(){
        //build the savefile + all the extra PNG files i one Zip
        var towrite = SaveScene_cb("",false);
        using (var memoryStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var demoFile = archive.CreateEntry("moel_file.txt");
                using (var entryStream = demoFile.Open())
                {
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(towrite);
                    }
                }
                //for every additional ingredient add the png file, and the background images ?
                int extra_ingredient = Manager.Instance.additional_ingredients_names.Count;
                for (var i=0;i<extra_ingredient;i++){
                    var name = Manager.Instance.additional_ingredients_names[i];
                    var ind = Manager.Instance.ingredients_names[name];
                    var sprite_name = Manager.Instance.sprites_names[ind];
                    var texture = Manager.Instance.sprites_textures[sprite_name];
                    var current_bytes = texture.EncodeToPNG();
                    var imgFile = archive.CreateEntry(sprite_name);
                    using (var entryStream = imgFile.Open())
                    {
                        entryStream.Write(current_bytes,0,current_bytes.Length);
                    }
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            //get the data 
            byte[] truncated = memoryStream.ToArray();
            return truncated;
        }
    }

    public void SaveScene()
    {
        int extra_ingredient = Manager.Instance.additional_ingredients_names.Count;
        load_scene = false;
        save_image = false;
        load_image = false;
        save_scene = true;
        if (!use_native_browser) OpenFileBrowser();// GetImage.GetImageFromUserAsync(gameObject.name, "LoadFromString");
        else {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (extra_ingredient!=0) {
                var bytes = SaveAsZipData();
                DownloadFile(gameObject.name, "OnFileDownload", "MySaveArchive.zip", bytes, bytes.Length);
            }
#else         
            if (extra_ingredient!=0) {
                string filePath = FileBrowser.SaveFile("Save archive", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MySaveArchive", "zip");
                var bytes = SaveAsZipData();
                System.IO.File.WriteAllBytes(filePath, bytes);
            }       
            else {
                string filePath = FileBrowser.SaveFile("Save file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MySaveFile", "txt");
                FileSelectedCallback(filePath);
            }
#endif  
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
#if UNITY_WEBGL && !UNITY_EDITOR
            UploadFile(gameObject.name, "OnFileUpload", ".png, .jpeg, .jpg, .tiff, .bmp", false);
#else                      
            string filePath = FileBrowser.OpenSingleFile("Open single file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "png","jpeg","jpg","tiff","bmp");
            FileSelectedCallback(filePath);
#endif                
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
#if UNITY_WEBGL && !UNITY_EDITOR
            //should only support zip file here, so we can load txt+png or json+png
            UploadFile(gameObject.name, "OnFileUpload", ".zip", false);
#else            
            string filePath = FileBrowser.OpenSingleFile("Open single file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "txt", "json","zip");
            FileSelectedCallback(filePath);
#endif            
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
#if UNITY_WEBGL && !UNITY_EDITOR
            SaveImage_cb("");
            DownloadFile(gameObject.name, "OnFileDownload", "image.png", current_bytes, current_bytes.Length);
#else    
            string filePath = FileBrowser.SaveFile("Save file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Image", "png");
            FileSelectedCallback(filePath);
#endif
        }
    }
    //should save a zip with additional ing
    public string SaveScene_cb(string filename, bool write = true)
    {
        if (filename == "" && write) return "";
        string sep = ",";
        Debug.Log("You are in the saveScene_cb with filename: " + filename);
        if (filename == null) return "";
        string text = "";
        string towrite = "";
        Debug.Log("should save in " + filename);
        //first loop over all material and check if default color or not
        /* background image */
        int nbg_images = BackgroundImageManager.Get.bg_Images.Count;
        towrite += nbg_images.ToString()+ "\r\n";
        for (var i=0;i<nbg_images;i++){
            var path  = BackgroundImageManager.Get.bg_Images[i];
            var scale2d = BackgroundImageManager.Get.GetScale(i);
            var pos = BackgroundImageManager.Get.GetPosition(i);
            var rot = BackgroundImageManager.Get.GetRotation(i);
            towrite += path+sep+scale2d.ToString()+sep+rot.ToString()+sep+pos.x.ToString()+sep+pos.y.ToString()+sep+pos.z.ToString()+ "\r\n";
        }
        text = "";
        /* Extra compartments */
        int extra_compartments = Manager.Instance.additional_compartments_names.Count;
        towrite += extra_compartments.ToString()+ "\r\n";
        for (var i=0;i<extra_compartments;i++){
            var name = Manager.Instance.additional_compartments_names[i];
            towrite += name+ "\r\n";
        }
        //Manager.Instance.additional_compartments_names.Clear();
        /*add extra ingredients data e.g. scale2d and offsetY
        upon loading add the ingredient if not there already
        */
        int extra_ingredient = Manager.Instance.additional_ingredients_names.Count;
        towrite += extra_ingredient.ToString()+ "\r\n";
        //name,spritename,scale2d,yoffset,issurf,isfiber,comp
        for (var i=0;i<extra_ingredient;i++){
            var name = Manager.Instance.additional_ingredients_names[i];
            var ind = Manager.Instance.ingredients_names[name];
            var sprite_name = Manager.Instance.sprites_names[ind];
            var prefab = Manager.Instance.all_prefab[name];
            var props = prefab.GetComponent<PrefabProperties>();
            var issurf = (props.is_surface)?"1":"0";
            var isfiber = (props.is_fiber)?"1":"0";
            var fiber_length = props.y_length;
            var comp = props.compartment;//.IndexOf(props.compartment);
            name = name.Split('.')[2];
            towrite +=name+sep+sprite_name+sep+props.scale2d.ToString()+sep+props.y_offset.ToString()+sep+fiber_length.ToString()+sep; 
            towrite +=issurf+sep+isfiber+sep+comp+ "\r\n";
        }
        int mat_count = 0;
        /*Background Color*/
        var bgcolor = Manager.Instance.Background.GetComponent<Renderer>().material.color;
        text ="background"+sep+bgcolor.r.ToString() +sep+bgcolor.g.ToString() + sep + bgcolor.b.ToString()+"\r\n";
        mat_count++;
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
            text += (player.bodyType==RigidbodyType2D.Static)? "1"+ sep : "0" + sep;//is it pinned
            //retrieve the ghost ID ?
            text += props.ghost_id.ToString();//is it ghosted 
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
                var props = player.gameObject.GetComponent<PrefabProperties>();
                if (ch.GetComponent<PrefabProperties>().is_bound)
                {
                    towrite += player.gameObject.GetComponent<PrefabProperties>().name+sep;
                }
                else { towrite += "0"+sep; }
                towrite += (player.bodyType == RigidbodyType2D.Static) ? "1" + sep : "0" + sep;//is it pinned
                towrite += props.ghost_id.ToString();//is it ghosted
                towrite += "\r\n";
            }
        }
        //surface object
        for (int i = 0; i < Manager.Instance.surface_objects.Count; i++)
        {
            Rigidbody2D player = Manager.Instance.surface_objects[i].GetComponent<Rigidbody2D>();
            var props = Manager.Instance.surface_objects[i].GetComponent<PrefabProperties>();
            towrite += player.position.x.ToString() + sep + player.position.y.ToString() + sep + Manager.Instance.surface_objects[i].transform.position.z + sep;
            towrite += player.rotation.ToString() + sep;
            towrite += props.name + sep;
            towrite += (player.bodyType == RigidbodyType2D.Static) ? "1"+ sep : "0" + sep;
            towrite += props.ghost_id.ToString();//is it ghosted
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
        if (write) System.IO.File.WriteAllText(filename, towrite);
        return towrite;
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

    public void LoadFromLines(string[] lines, bool as_task=false) {
        string sep = ",";
        var current_name = "";
        if (Manager.Instance.myPrefab)
            current_name = Manager.Instance.myPrefab.name;
        int lineCounter = 0;
        //split with space and get x y r name
        //first line is nb of material color to overwrote
        string[] elems = lines[lineCounter].Split(sep[0]);
        int nbg_images =int.Parse(elems[0]);
        Debug.Log("found nbg_images "+nbg_images.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + nbg_images; i++)
        {
            elems = lines[i].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            var path  = elems[0];
            var scale2d = float.Parse(elems[1]);
            var rot = float.Parse(elems[2]);
            var x = float.Parse(elems[3]);//Debug.Log(x);
            var y = float.Parse(elems[4]);//Debug.Log(y);
            var z = float.Parse(elems[5]);//Debug.Log(z);          
            BackgroundImageManager.Get.AddBackgroundSprites(path,new Vector3(x,y,z),scale2d,rot);
            if (as_task) UI_manager.Get.UpdatePB((float)lineCounter/(float)(lineCounter + nbg_images),"loading background images");
        }
        lineCounter += nbg_images;
        elems = lines[lineCounter].Split(sep[0]);
        int extra_compartments = int.Parse(elems[0]);
        Debug.Log("found extra_compartments "+extra_compartments.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + extra_compartments; i++)
        {
            elems = lines[i].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            Manager.Instance.recipeUI.AddOneCompartment(elems[0],false);
            if (as_task) UI_manager.Get.UpdatePB((float)lineCounter/(float)(lineCounter + extra_compartments),"loading extra compartments");
        }
        lineCounter += extra_compartments;
        elems = lines[lineCounter].Split(sep[0]);
        int extra_ingredient = int.Parse(elems[0]);
        Debug.Log("found extra_ingredient "+extra_ingredient.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + extra_ingredient; i++)
        {
            elems = lines[i].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            var name = elems[0];
            var sprite_name = elems[1];
            var scale2d = float.Parse(elems[2]);
            var yoffset = float.Parse(elems[3]);
            var fiber_length = float.Parse(elems[4]);
            var issurf = (elems[5] == "0")? false : true;
            var isfiber = (elems[6] == "0")? false : true;
            var comp = elems[7];//int.Parse(elems[6] );
            var prefix = (issurf)?"surface":"interior";
            var iname = comp+"."+prefix+"."+name;
            //actual name is compname+"."+prefix+"."+name
            if (!Manager.Instance.ingredients_names.ContainsKey(iname)) {
                Debug.Log("AddOneIngredient "+name+" "+sprite_name+" "+comp);
                Manager.Instance.recipeUI.AddOneIngredient(name, sprite_name, scale2d, yoffset, fiber_length, issurf, isfiber, comp);
            }
            if (as_task) UI_manager.Get.UpdatePB((float)lineCounter/(float)(lineCounter + extra_ingredient),"loading extra ingredient");
        }
        lineCounter += extra_ingredient;
        elems = lines[lineCounter].Split(sep[0]);
        int n_mat = int.Parse(elems[0]);
        Debug.Log("found n_mat "+n_mat.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + n_mat; i++)
        {
            elems = lines[i].Replace("\n", "").Replace("\r", "").Split(sep[0]);
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
            if (name == "background") {
                Manager.Instance.changeColorBackground( new Color(r, g, b) );
                continue;
            }
            if (!Manager.Instance.prefab_materials.ContainsKey(name))
            {
                Manager.Instance.prefab_materials.Add(name, Manager.Instance.createNewSpriteMaterial(name));
            }//else build the material ?
            Manager.Instance.prefab_materials[name].color = new Color(r, g, b);
            if (as_task) UI_manager.Get.UpdatePB((float)lineCounter/(float)(lineCounter + n_mat),"loading materials");
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
            elems = lines[i].Replace("\n", "").Replace("\r", "").Split(sep[0]);
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
            var newObject = Manager.Instance.restoreOneInstance(name, new Vector3(x, y, z), zangle, order, false, (kinematic == 1),false);
            GroupManager.Get.UpdateGroupFromObject(newObject, group_name, group_id);
            if (ghost != -1) {
                GhostManager.Get.UpdateFromObject(newObject, ghost, group_name, group_id);
            }    
            if (as_task) UI_manager.Get.UpdatePB((float)lineCounter/(float)(lineCounter + nObj),"loading soluble proteins");
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
            elems = lines[lineCounter].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            lineCounter++;
            var name = elems[0];
            int nPoints = int.Parse(elems[1]);
            var prefabName = checkFiberName(name);
            if (prefabName == "DrawDNA") prefabName = "Draw DNA";
            Debug.Log(name + " fiber  ??  " + prefabName+" "+nPoints.ToString());
            GameObject fp = Manager.Instance.AddFiberParent(prefabName);
            var group_name = elems[2];
            var group_id = int.Parse(elems[3]);            
            bool closed = name.EndsWith("_Closed");
            var do_ghost = -1;
            for (int j = 0; j < nPoints; j++)
            {
                //-38.75513,-14.91165,0,158.7505,0,0,1
                elems = lines[lineCounter].Replace("\n", "").Replace("\r", "").Split(sep[0]);
                if (elems.Length != 7) {
                    Debug.Log("problem fiber line "+j.ToString()+ " "+lines[lineCounter]);
                }
                var x = float.Parse(elems[0]);
                var y = float.Parse(elems[1]);
                var z = float.Parse(elems[2]);
                var zangle = float.Parse(elems[3]);
                var bounded = elems[4];
                var kinematic = int.Parse(elems[5]);
                do_ghost = int.Parse(elems[6]); 
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
            GroupManager.Get.UpdateGroupFromObject(fp, group_name, group_id);
            if (do_ghost != -1){
                GhostManager.Get.UpdateFromObject(fp, do_ghost, group_name, group_id);
            }
            if (as_task) UI_manager.Get.UpdatePB((float)i/(float)(nFiber),"loading fiber proteins");
        }
        Debug.Log("found nSurfaceOfFiber " + nSurfaceOfFiber.ToString());
        for (int i = 0; i < nSurfaceOfFiber; i++)
        {
            elems = lines[lineCounter].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            var x = float.Parse(elems[0]);
            var y = float.Parse(elems[1]);
            var z = float.Parse(elems[2]);
            var zangle = float.Parse(elems[3]);
            var name = elems[4];
            var kinematic = int.Parse(elems[5]);
            var ghost = int.Parse(elems[6]); 
            var group_name = elems[7];
            var group_id = int.Parse(elems[8]);
            var newObject = Manager.Instance.restoreOneInstance(name, new Vector3(x, y, z), zangle, 0, true, (kinematic == 1), false);
            GroupManager.Get.UpdateGroupFromObject(newObject, group_name, group_id); 
            if (ghost != -1) {
                GhostManager.Get.UpdateFromObject(newObject, ghost, group_name, group_id);
            }   
            if (as_task) UI_manager.Get.UpdatePB((float)i/(float)(nSurfaceOfFiber),"loading surface proteins");            
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
            elems = lines[lineCounter].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            var x1 = float.Parse(elems[0]);
            var y1 = float.Parse(elems[1]);
            var z1 = float.Parse(elems[2]);
            var id1 = elems[3];
            var x2 = float.Parse(elems[4]);
            var y2 = float.Parse(elems[5]);
            var z2 = float.Parse(elems[6]);
            var id2 = elems[7];
            Manager.Instance.restoreOneBond(new Vector3(x1, y1, z1),id1, new Vector3(x2, y2, z2),id2);
            if (as_task) UI_manager.Get.UpdatePB((float)i/(float)(nLink),"loading bonds");      
            lineCounter++;
        }
        //Manager.Instance.UpdateGhostArea();   
        //if (current_name!="") Manager.Instance.SwitchPrefabFromName(current_name);
        //restore groups
        GroupManager.Get.RestoreGroups();
        GhostManager.Get.RestoreGhost();
        Manager.Instance.recipeUI.SetTofirstIngredient();
    }

    IEnumerator LoadFromLines_CR(string[] lines) {
        int lcounter = 0;
        if (Manager.Instance.TogglePhysics) Manager.Instance.TogglePhysics.isOn = false;
        Physics2D.autoSimulation = false;        
        UI_manager.Get.progress_bar_holder.SetActive(true);    
        UI_manager.Get.UpdatePB(0.0f,"loading");        
        string sep = ",";
        var current_name = "";
        if (Manager.Instance.myPrefab)
            current_name = Manager.Instance.myPrefab.name;
        int lineCounter = 0;
        //split with space and get x y r name
        //first line is nb of material color to overwrote
        string[] elems = lines[lineCounter].Split(sep[0]);
        int nbg_images =int.Parse(elems[0]);
        Debug.Log("found nbg_images "+nbg_images.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + nbg_images; i++)
        {
            elems = lines[i].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            var path  = elems[0];
            var scale2d = float.Parse(elems[1]);
            var rot = float.Parse(elems[2]);
            var x = float.Parse(elems[3]);//Debug.Log(x);
            var y = float.Parse(elems[4]);//Debug.Log(y);
            var z = float.Parse(elems[5]);//Debug.Log(z);          
            BackgroundImageManager.Get.AddBackgroundSprites(path,new Vector3(x,y,z),scale2d,rot);
            if (lcounter%frequency == 0) {
                yield return null;
                UI_manager.Get.UpdatePB((float)lcounter/(float)(nbg_images),"loading background images");
            }
            lcounter++;
            //
        }
        lcounter = 0;
        lineCounter += nbg_images;
        elems = lines[lineCounter].Split(sep[0]);
        int extra_compartments = int.Parse(elems[0]);
        Debug.Log("found extra_compartments "+extra_compartments.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + extra_compartments; i++)
        {
            elems = lines[i].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            Debug.Log("AddOneCompartment "+elems[0]);
            Manager.Instance.recipeUI.AddOneCompartment(elems[0],false);
            if (lcounter%frequency == 0) {
                UI_manager.Get.UpdatePB((float)lineCounter/(float)(lineCounter + extra_compartments),"loading extra compartments");
                yield return null;
            }
            lcounter++;
        }
        lcounter = 0;
        lineCounter += extra_compartments;
        elems = lines[lineCounter].Split(sep[0]);
        int extra_ingredient = int.Parse(elems[0]);
        Debug.Log("found extra_ingredient "+extra_ingredient.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + extra_ingredient; i++)
        {
            elems = lines[i].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            var name = elems[0];
            var sprite_name = elems[1];
            var scale2d = float.Parse(elems[2]);
            var yoffset = float.Parse(elems[3]);
            var fiber_length = float.Parse(elems[4]);
            var issurf = (elems[5] == "0")? false : true;
            var isfiber = (elems[6] == "0")? false : true;
            var comp = elems[7];//int.Parse(elems[6] );
            var prefix = (issurf)?"surface":"interior";
            var iname = comp+"."+prefix+"."+name;
            //actual name is compname+"."+prefix+"."+name
            if (!Manager.Instance.ingredients_names.ContainsKey(iname)) {
                Debug.Log("AddOneIngredient "+name+" "+sprite_name+" "+comp);
                Manager.Instance.recipeUI.AddOneIngredient(name, sprite_name, scale2d, yoffset, fiber_length, issurf, isfiber, comp);
            }
            if (lcounter%frequency == 0) {
                UI_manager.Get.UpdatePB((float)lcounter/(float)(extra_ingredient),"loading extra ingredient");
                yield return null;
            }
            lcounter++;        
        }
        lcounter = 0;
        lineCounter += extra_ingredient;
        elems = lines[lineCounter].Split(sep[0]);
        int n_mat = int.Parse(elems[0]);
        Debug.Log("found n_mat "+n_mat.ToString());
        lineCounter++;
        for (int i = lineCounter; i < lineCounter + n_mat; i++)
        {
            elems = lines[i].Replace("\n", "").Replace("\r", "").Split(sep[0]);
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
            if (name == "background") {
                Manager.Instance.changeColorBackground( new Color(r, g, b) );
                continue;
            }
            if (!Manager.Instance.prefab_materials.ContainsKey(name))
            {
                Manager.Instance.prefab_materials.Add(name, Manager.Instance.createNewSpriteMaterial(name));
            }//else build the material ?
            Manager.Instance.prefab_materials[name].color = new Color(r, g, b);
            if (lcounter%frequency == 0) {
                UI_manager.Get.UpdatePB((float)lcounter/(float)(n_mat),"loading materials");                
                yield return null;
            }
            lcounter++;
        }
        lcounter = 0;
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
            elems = lines[i].Replace("\n", "").Replace("\r", "").Split(sep[0]);
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
            var newObject = Manager.Instance.restoreOneInstance(name, new Vector3(x, y, z), zangle, order, false, (kinematic == 1),false);
            GroupManager.Get.UpdateGroupFromObject(newObject, group_name, group_id);
            if (ghost != -1) {
                GhostManager.Get.UpdateFromObject(newObject, ghost, group_name, group_id);
            }    
            if (lcounter%frequency == 0) {
                UI_manager.Get.UpdatePB((float)lcounter/(float)(nObj),"loading soluble proteins");
                yield return null;
            }
            lcounter++;
        }
        lcounter = 0;
        //in case of fiber need to do the random choice of sprite id, or save it
        lineCounter += nObj;
        GameObject attached = null;
        GameObject fiber = null;
        Debug.Log("found nFiber " + nFiber.ToString());
        for (int i = 0; i < nFiber; i++)
        {
            UI_manager.Get.UpdatePB(0.0f,"loading fiber ");
            attached = null;
            fiber = null;
            Debug.Log(lines[lineCounter]);
            elems = lines[lineCounter].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            lineCounter++;
            var name = elems[0];
            int nPoints = int.Parse(elems[1]);
            var prefabName = checkFiberName(name);
            if (prefabName == "DrawDNA") prefabName = "Draw DNA";
            Debug.Log(name + " fiber  ??  " + prefabName+" "+nPoints.ToString());
            GameObject fp = Manager.Instance.AddFiberParent(prefabName);
            var group_name = elems[2];
            var group_id = int.Parse(elems[3]);            
            bool closed = name.EndsWith("_Closed");
            var do_ghost = -1;
            lcounter = 0;
            for (int j = 0; j < nPoints; j++)
            {
                //-38.75513,-14.91165,0,158.7505,0,0,1
                elems = lines[lineCounter].Replace("\n", "").Replace("\r", "").Split(sep[0]);
                if (elems.Length != 7) {
                    Debug.Log("problem fiber line "+j.ToString()+ " "+lines[lineCounter]);
                }
                var x = float.Parse(elems[0]);
                var y = float.Parse(elems[1]);
                var z = float.Parse(elems[2]);
                var zangle = float.Parse(elems[3]);
                var bounded = elems[4];
                var kinematic = int.Parse(elems[5]);
                do_ghost = int.Parse(elems[6]); 
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
                if (lcounter%frequency == 0) {
                    UI_manager.Get.UpdatePB((float)j/(float)(nPoints),"loading fiber "+fp.name);
                    yield return null;
                }
                lcounter++;                
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
            if (do_ghost != -1){
                GhostManager.Get.UpdateFromObject(fp, do_ghost, group_name, group_id);
            }
        }
        lcounter = 0;
        Debug.Log("found nSurfaceOfFiber " + nSurfaceOfFiber.ToString());
        for (int i = 0; i < nSurfaceOfFiber; i++)
        {
            elems = lines[lineCounter].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            var x = float.Parse(elems[0]);
            var y = float.Parse(elems[1]);
            var z = float.Parse(elems[2]);
            var zangle = float.Parse(elems[3]);
            var name = elems[4];
            var kinematic = int.Parse(elems[5]);
            var ghost = int.Parse(elems[6]); 
            var group_name = elems[7];
            var group_id = int.Parse(elems[8]);
            var newObject = Manager.Instance.restoreOneInstance(name, new Vector3(x, y, z), zangle, 0, true, (kinematic == 1), false);
            GroupManager.Get.UpdateGroupFromObject(newObject, group_name, group_id); 
            if (ghost != -1) {
                GhostManager.Get.UpdateFromObject(newObject, ghost, group_name, group_id);
            }   
            if (lcounter%frequency == 0) {
                UI_manager.Get.UpdatePB((float)i/(float)(nSurfaceOfFiber),"loading surface proteins");            
                yield return null;
            }
            lcounter++;
            lineCounter++;
        }
        lcounter = 0;
        //reset ui and manager
        elems = lines[lineCounter].Split(sep[0]);
        int nLink = int.Parse(elems[0]);
        lineCounter++;
        Debug.Log("found nLink " + nLink.ToString());
        for (int i = 0; i < nLink; i++)
        {
            //create a joints
            elems = lines[lineCounter].Replace("\n", "").Replace("\r", "").Split(sep[0]);
            var x1 = float.Parse(elems[0]);
            var y1 = float.Parse(elems[1]);
            var z1 = float.Parse(elems[2]);
            var id1 = elems[3];
            var x2 = float.Parse(elems[4]);
            var y2 = float.Parse(elems[5]);
            var z2 = float.Parse(elems[6]);
            var id2 = elems[7];
            Manager.Instance.restoreOneBond(new Vector3(x1, y1, z1),id1, new Vector3(x2, y2, z2),id2);
            if (lcounter%frequency == 0) {
                UI_manager.Get.UpdatePB((float)i/(float)(nLink),"loading bonds");      
                yield return null;
            }
            lcounter++;
            lineCounter++;
        }
        lcounter = 0;
        //Manager.Instance.UpdateGhostArea();   
        //if (current_name!="") Manager.Instance.SwitchPrefabFromName(current_name);
        //restore groups
        GroupManager.Get.RestoreGroups();
        GhostManager.Get.RestoreGhost();
        if (Manager.Instance.TogglePhysics) Manager.Instance.TogglePhysics.isOn = true;
        Physics2D.autoSimulation = true;  
        UI_manager.Get.progress_bar_holder.SetActive(false);   
        Manager.Instance.recipeUI.SetTofirstIngredient();          
    }

    //should be able to load a zip
    public void LoadScene_cb(string filename)
    {
        Manager.Instance.CheckDir();
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
            //LoadFromLines(lines);
            //DoAsyncLoadFromLines(lines);
            if (use_coroutine) StartCoroutine(LoadFromLines_CR(lines));
            else LoadFromLines(lines);
        }
        else if (ext == ".json")
        {
            //load json recipe

            if (!Manager.Instance.recipeUI.merge_upon_loading) Manager.Instance.Clear();
            Manager.Instance.recipeUI.LoadRecipe(filename);
        }
        else if (ext == ".zip") 
        {
            byte[] zipbytes = File.ReadAllBytes(filename);
            if (use_coroutine) StartCoroutine(LoadFromZipDataCR(zipbytes));
            else LoadFromZipData(zipbytes);
        }
        else {
            //not supported
            Debug.Log("not supported");
        }
    }

    public void OpenURL_mesoscope(){
        var url = "https://mesoscope.scripps.edu/beta/";
        Application.OpenURL(url);
    }

    public void OpenURL_ccsb(){
        var url = "https://ccsb.scripps.edu/";
        Application.OpenURL(url);
    }

    public void OpenURL(string url){
        Application.OpenURL(url);
    }

    public void SendLogEmail()
    {
        //%USERPROFILE%\AppData\LocalLow\CompanyName\ProductName\output_log.txt
        /*System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + @"\Unity\Editor\Editor.log";
        string email = "autin@scripps.edu";
        string subject = "LOGFile";
        string body = "";//atach the log file
        Application.OpenURL ("mailto:" + email + "?subject=" + subject + "&body=" + body);
        //System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(attachmentPath);
        */
    }  
}
//5oxv,6b8h, 1sm1,5jhm