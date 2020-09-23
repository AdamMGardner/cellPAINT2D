using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Crosstales.FB;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
//using OpenQA.Selenium.Firefox;
using System.Threading;
using System.Threading.Tasks;

//TODO fix the sprites_name for illustrate. use _sprite
public class Add_ingredient_ui : MonoBehaviour
{
    public bool use_webDriver = false;
    public InputField input_pdb_field;
    public InputField input_name_field;
    public InputField input_seletion_field;
    public InputField input_bu_field;
    public InputField input_model_field;
    public InputField input_pixel_ratio_field;
    public InputField input_offset_y_field;
    public InputField Zrotation_field;
    public InputField fiber_length_field;
    public InputField comp_name_field;
    public Text log_label;
    public Button Load;
    public Button Illustrate;
    public Button Create;
    public int query_id;
    public Image theSprite;
    public Image theSpriteMb;
    public Image theSpriteAxis;
    public Image theSpriteFiberLeft;
    public Image theSpriteFiberRight;
    public Toggle surface;
    public Toggle fiber;
    public Image loader;
    public Image browser_image;
    public string query_answer="";
    public string query_answer_url="";
    public bool do_screen_capture = false;
    public bool query_sent = false;
    public bool query_done = true;
    public bool redo_query = false;
    public bool illustrated = false;
    public string sprite_name="";
    public string sprite_path="";
    public bool Force = false;
    private static Add_ingredient_ui _instance = null;
    private IWebDriver driver;
    private Task ill_task;
    public string input_pdb;
    public string input_name;
    public string input_seletion;
    public string input_bu;
    public string input_model;
    public float input_pixel_ratio = 1.0f;
    public float input_offset_y;   
    public float Zrotation = 0.0f; 
    public float fiber_length = 0.0f; 
    private Texture2D tmp_texture;
    private Texture2D browser_texture;
    private string webdriverspath;
    public static Add_ingredient_ui Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<Add_ingredient_ui>();
            if (_instance == null)
            {
                var go = GameObject.Find("Add_ingredient_ui");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("Add_ingredient_ui"); //{ hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<Add_ingredient_ui>();
            }
            return _instance;
        }
    }
    // Start is called before the first frame update
    
    void Start()
    {
        browser_texture = new Texture2D(2, 2);
        webdriverspath = UnityEngine.Application.dataPath + "/../Data/webdrivers/win/";
#if UNITY_STANDALONE_OSX
        webdriverspath = UnityEngine.Application.dataPath + "/../Data/webdrivers/mac/";
#elif UNITY_EDITOR_OSX
        webdriverspath = UnityEngine.Application.dataPath + "/../Data/webdrivers/mac/";
#elif UNITY_STANDALONE_WIN
        webdriverspath = UnityEngine.Application.dataPath + "/../Data/webdrivers/win/";
#elif UNITY_EDITOR_WIN
        webdriverspath = UnityEngine.Application.dataPath + "/../Data/webdrivers/win/";
#elif UNITY_STANDALONE_LINUX
        webdriverspath = UnityEngine.Application.dataPath + "/../Data/webdrivers/linux/";
#else 
        webdriverspath = UnityEngine.Application.dataPath + "/../Data/webdrivers/win/";
#endif        
    }

    // Update is called once per frame
    void Update()
    {
        query_answer_url ="https://mesoscope.scripps.edu/data/tmp/ILL/"+query_id.ToString()+"/"+input_name_field.text+".png";
        if (query_sent) {
            //check if result available
            
            //test of url exist
            if (!query_done && redo_query) {
                redo_query = false;
                StartCoroutine(GetText(query_answer_url));
            }
            else {

            }
        }
        if (Force){
            var filePath = PdbLoader.DownloadFile(input_name_field.text, "https://mesoscope.scripps.edu/data/tmp/ILL/"+query_id.ToString()+"/",  PdbLoader.DefaultDataDirectory + "/" + "images/", ".png");
            var sprite = Manager.Instance.LoadNewSprite(filePath);
            theSprite.sprite = sprite;
            var ratio =(float) theSprite.sprite.texture.width/(float)theSprite.sprite.texture.height;
            var h = 210;//w/ratio;//(snode.data.thumbnail)?snode.data.thumbnail.height:150;
            var w = h*ratio;
            theSprite.rectTransform.sizeDelta = new Vector2(w,(int)h);
            Force = false;
            //try the getTexture
            StartCoroutine(GetText(query_answer_url));
        }
        if (query_done) {
            StartCoroutine(GetText(query_answer_url));
            query_done = false;
        }
        if (!query_done &&(driver!=null)&&do_screen_capture){
            Screenshot image = ((ITakesScreenshot)driver).GetScreenshot();
            //Save the screenshot
            //AsByteArray
            browser_texture.LoadImage(image.AsByteArray);
            if (browser_image) {
                browser_image.sprite = Sprite.Create(browser_texture, new Rect(0, 0, browser_texture.width, browser_texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            //image.SaveAsFile("C:/temp/Screenshot.png");
        }
    }

    public void setScale(float number){
        input_pixel_ratio_field.text = number.ToString();
        input_pixel_ratio = number;
    }

    public void setScale(string number){
        input_pixel_ratio = float.Parse (number);
    }

    public void setYoffset(string number){
        setYoffset_cb(float.Parse (number));
    }

    public void setYoffset(float number){
        input_offset_y_field.text = number.ToString();
        setYoffset_cb(number);
    }

    public void setZrot(float number){
        Zrotation_field.text = number.ToString();
        theSprite.rectTransform.rotation = Quaternion.Euler(0, 0, number);
        Zrotation = number; 
        if (fiber.isOn) setFiberLength_cb();
    }

    public void setZrot(string number){
        theSprite.rectTransform.rotation = Quaternion.Euler(0, 0, float.Parse (number));
        Zrotation = float.Parse (number); 
        if (fiber.isOn) setFiberLength_cb();
    }

    public void setYoffset_cb(float number){
        input_pixel_ratio = float.Parse (input_pixel_ratio_field.text ); 
        input_offset_y_field.text = number.ToString();
        input_offset_y = number;
        //update on the sprite
        float w = (float)theSprite.rectTransform.rect.width;
        float h = (float)theSprite.rectTransform.rect.height;
        Debug.Log(w.ToString()+" "+h.ToString()+" "+theSprite.sprite.texture.width.ToString());
        var canvas_scale = w/theSprite.sprite.texture.width;
        Debug.Log(canvas_scale);
        var sc2d = input_pixel_ratio*canvas_scale;//*canvas_scale;
        Debug.Log(sc2d);
        var offy = input_offset_y*sc2d;//sc2d is angstrom to pixels
        Debug.Log(offy);
        var p = theSpriteMb.rectTransform.localPosition;
        theSpriteMb.rectTransform.localPosition = new Vector3(p.x,offy,p.z);
        var thickness = 42.0f*sc2d;//angstrom
        theSpriteMb.rectTransform.sizeDelta = new Vector2((int)w,thickness);
        //theSpriteMb.rectTransform.localPosition = new Vector3(p.x,h/2.0f-offy,p.z);
    }

    public void ToggleMainImage(bool toggle){
        //if fiber disable the image
        theSprite.enabled = ! toggle;
    }
    
    public void setFiberLength(float number){
        fiber_length_field.text = number.ToString();
        //theSprite.rectTransform.rotation = Quaternion.Euler(0, 0, number);
        fiber_length = number; 
        setFiberLength_cb();
    }

    public void setFiberLength(string number){
        //theSprite.rectTransform.rotation = Quaternion.Euler(0, 0, float.Parse (number));
        fiber_length = float.Parse (number);
        setFiberLength_cb(); 
    }

    public void setFiberLength_cb(){
        input_pixel_ratio = float.Parse (input_pixel_ratio_field.text ); 
        //update on the sprite
        float w = (float)theSprite.rectTransform.rect.width;
        float h = (float)theSprite.rectTransform.rect.height;
        var canvas_scale = w/theSprite.sprite.texture.width;
        var sc2d = input_pixel_ratio*canvas_scale;//*canvas_scale;
        var pixel_length = fiber_length*sc2d;//sc2d is angstrom to pixels
        var scaling = pixel_length/154.0f;
        //theSpriteMb.rectTransform.localPosition = new Vector3(p.x,h/2.0f-offy,p.z);
        //the line is 154pixel long in the image. scale it to accomodate the fiber length.
        //theSpriteAxis.rectTransform.sizeDelta = new Vector2(scaling,1.0f);
        var p = theSprite.rectTransform.position;
        theSpriteAxis.transform.localScale = new Vector3(scaling,scaling,1.0f);
        //this doesnt work when rotate the parent.
        theSpriteFiberLeft.rectTransform.position = new Vector3(p.x-pixel_length/2.0f,p.y,p.z);
        theSpriteFiberRight.rectTransform.position = new Vector3(p.x+pixel_length/2.0f,p.y,p.z);
    }

    public void CallIllustrate(){
        if (loader) loader.gameObject.SetActive(true);   
        if (input_name_field.text == "") input_name_field.text = input_pdb_field.text;
        input_name = input_name_field.text;
        input_pdb = input_pdb_field.text;
        input_bu = input_bu_field.text;
        input_seletion = input_seletion_field.text;
        input_model = input_model_field.text;
        input_pixel_ratio_field.text = "6.0";
        var q_id = (query_id==-1)? Mathf.CeilToInt(UnityEngine.Random.Range(0.0f, 1.0f)*1000000000.0f) : query_id;
        query_id = q_id;
        illustrated = true;
        if (use_webDriver) {
            DoAsyncIllustrate();
        }
        else {
            var query="https://mesoscope.scripps.edu/beta//cgi-bin/hILL.py?pdbid="+input_pdb;
            if (input_name !="") query += "&name="+input_name;
            if (input_bu!="") query += "&bu="+input_bu;
            if (input_seletion!="") query += "&selection="+input_seletion;
            if (input_model!="") query += "&model="+input_model;
            query+="&qid="+query_id.ToString();
            StartCoroutine(GetRequest(query));
        }
    }

    public void DoCallIllustrate(){
        Debug.Log("DoCallIllustrate");
        var query="https://mesoscope.scripps.edu/beta/illustratecall.html?pdbid="+input_pdb;
        if (input_name !="") query += "&name="+input_name;
        if (input_bu!="") query += "&bu="+input_bu;
        if (input_seletion!="") query += "&selection="+input_seletion;
        if (input_model!="") query += "&model="+input_model;
        query+="&qid="+query_id.ToString();
        sprite_name = input_name+".png";
        var options = new ChromeOptions();
        options.AddArguments("--headless");
        var chromeDriverService = ChromeDriverService.CreateDefaultService(webdriverspath);
        chromeDriverService.HideCommandPromptWindow = true;
        driver = new ChromeDriver(chromeDriverService,options);
        driver.Url = query;
        
        //Debug.Log(driver.PageSource);
        // wait for the results to appear
        //IWebElement firstResult = driver.WaitForElement(By.Id("image_url"));
        WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, 1, 120));//TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(0.1));
        try {
            IWebElement firstResult = wait.Until(e => {
                return e.FindElement(By.Id("image_url"));
                });
            Debug.Log(firstResult.Text);
            query_answer_url = firstResult.Text;
            redo_query = false;       
            query_sent = true; 
            query_done = true;
        }
        catch {
            //do nothing
            Debug.Log(driver.PageSource);
            if (loader) loader.gameObject.SetActive(false); 
        }
        //StartCoroutine(GetText(query_answer_url)); 
    }

    public async void DoAsyncIllustrate()
    {
        ill_task = AsyncIllustrate();
        await ill_task;
        if (ill_task.IsCompleted)
        {    
            Debug.Log(ill_task.Status);
        }
    }

    private Task AsyncIllustrate()
    {
        return Task.Factory.StartNew(() =>
            {
                DoCallIllustrate();
            });
    }

    public void LoadASprite(){
        illustrated = false;
        //call fileBrowser and pass it to load sprites
        string filePath = FileBrowser.OpenSingleFile("Open image file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "png", "jpg");
        var sprite = Manager.Instance.LoadNewSprite(filePath);
        theSprite.sprite = sprite;
        theSpriteFiberLeft.sprite = sprite;
        theSpriteFiberRight.sprite = sprite;
        var ratio =(float) theSprite.sprite.texture.width/(float)theSprite.sprite.texture.height;
        var h = 210;//w/ratio;//(snode.data.thumbnail)?snode.data.thumbnail.height:150;
        var w = h*ratio;
        theSprite.rectTransform.sizeDelta = new Vector2(w,(int)h);
        sprite_name = Path.GetFileName(filePath);
        sprite_path = Path.GetDirectoryName(filePath);
        if (input_name_field.text == "") input_name_field.text =  Path.GetFileNameWithoutExtension(filePath);
        Manager.Instance.AddUserDirectory(sprite_path);
    }

    public Texture2D RotateImage(Texture2D image, float angle){
        float pi2 = Mathf.PI / 2.0f;
        int oldWidth = image.width;
        int oldHeight = image.height;
        float theta = angle;//* Math.PI / 180.0 
        float locked_theta = theta;
        if (locked_theta < 0.0) locked_theta += 2.0f * Mathf.PI;
        float newWidth;
        float newHeight;
        int nWidth;
        int nHeight;
        float adjacentTop;
        float oppositeTop;
        float adjacentBottom;
        float oppositeBottom;
        if ((locked_theta >= 0.0f && locked_theta < pi2) ||
             (locked_theta >= Mathf.PI && locked_theta < (Mathf.PI + pi2)))
        {
            adjacentTop = Mathf.Abs(Mathf.Cos(locked_theta)) * oldWidth;
            oppositeTop = Mathf.Abs(Mathf.Sin(locked_theta)) * oldWidth;

            adjacentBottom = Mathf.Abs(Mathf.Cos(locked_theta)) * oldHeight;
            oppositeBottom = Mathf.Abs(Mathf.Sin(locked_theta)) * oldHeight;
        }
        else
        {
            adjacentTop = Mathf.Abs(Mathf.Sin(locked_theta)) * oldHeight;
            oppositeTop = Mathf.Abs(Mathf.Cos(locked_theta)) * oldHeight;

            adjacentBottom = Mathf.Abs(Mathf.Sin(locked_theta)) * oldWidth;
            oppositeBottom = Mathf.Abs(Mathf.Cos(locked_theta)) * oldWidth;
        }
        newWidth = adjacentTop + oppositeBottom;
        newHeight = adjacentBottom + oppositeTop;
        Vector2 center = new Vector2(oldWidth/2,oldHeight/2);
        var rotation = Quaternion.Euler(0, 0, angle);
        Bounds b = new Bounds();
        Vector2 upleft = rotation*(new Vector2(0,oldHeight) - center);
        b.Encapsulate(new Vector3(upleft.x,upleft.y,0));
        Vector2 upright = rotation*(new Vector2(oldWidth,oldHeight) - center);
        b.Encapsulate(new Vector3(upright.x,upright.y,0));
        //Vector2 botleft = rotation*(new Vector2(0,0) - center);
        //b.Encapsulate(new Vector3(botleft.x,botleft.y,0));
        //Vector2 botright = rotation*(new Vector2(oldWidth,0) - center);
        //b.Encapsulate(new Vector3(botright.x,botright.y,0));
        nWidth = Mathf.CeilToInt(b.size.x*2);
        nHeight = Mathf.CeilToInt(b.size.y*2);

        //nWidth = Mathf.CeilToInt(newWidth);
        //nHeight = Mathf.CeilToInt(newHeight);
        
        Texture2D rotatedBmp = new Texture2D(nWidth,nHeight);
        for (int i = 0; i <nWidth; i++)
        {
            for (int j = 0; j < nHeight; j++)
            {
                rotatedBmp.SetPixel(i,j,Color.clear);
            }
        }
        rotatedBmp.Apply();
        
        Vector2 center2 = new Vector2(nWidth/2,nHeight/2);
        Vector2 offset = new Vector2(Mathf.Abs(nWidth-oldWidth)/2,Mathf.Abs(nHeight-oldHeight)/2);
        //pass the rotated data to the new texture ?
        //Color[] pixels = image.GetPixels();
        //Now rotate your original image around its center but add an offset to the coordinates before putting them into the new array. 
        //The offset would be half the difference in width and height between both arrays.
        for (int i = 0; i <oldWidth; i++)
        {
            for (int j = 0; j < oldHeight; j++)
            {
                Vector2 new_coord = rotation*(new Vector2(i,j) - center);
                var pix = image.GetPixel(i, j);
                var newi = Mathf.RoundToInt(new_coord.x+center2.x+ 0.5f);//use 0.5 ?
                var newj = Mathf.RoundToInt(new_coord.y+center2.y+ 0.5f);
                rotatedBmp.SetPixel(newi,newj,pix);
            }
        }
        rotatedBmp.Apply();
        return rotatedBmp;
    }

    public Texture2D RotateImageReverse(Texture2D image, float angle){
        float pi2 = Mathf.PI / 2.0f;
        int oldWidth = image.width;
        int oldHeight = image.height;
        Vector2 center = new Vector2(oldWidth/2,oldHeight/2);
        var rotation = Quaternion.Euler(0, 0, angle);
        Bounds b = new Bounds();
        Vector2 upleft = rotation*(new Vector2(0,oldHeight) - center);
        b.Encapsulate(upleft);
        Vector2 upright = rotation*(new Vector2(oldWidth,oldHeight) - center);
        b.Encapsulate(upright);
        Vector2 botleft = rotation*(new Vector2(0,0) - center);
        b.Encapsulate(botleft);
        Vector2 botright = rotation*(new Vector2(oldWidth,0) - center);
        b.Encapsulate(botright);
        int nWidth = Mathf.CeilToInt(b.size.x);
        int nHeight = Mathf.CeilToInt(b.size.y);
        Vector2 center2 = new Vector2(nWidth/2,nHeight/2);
        Texture2D rotatedBmp = new Texture2D(nWidth,nHeight);
        for (int i = 0; i <nWidth; i++)
        {
            for (int j = 0; j < nHeight; j++)
            {
                //inverse rotation to get the original pixel
                Vector2 new_coord =  Quaternion.Inverse(rotation) * (new Vector2(i,j) - center2);
                var newi = Mathf.RoundToInt(new_coord.x+center.x);
                var newj = Mathf.RoundToInt(new_coord.y+center.y);
                if (newi >= 0 && newi < oldWidth && newj >=0 && newj < oldHeight)
                {   
                    var pix = image.GetPixel(newi, newj);
                    rotatedBmp.SetPixel(i,j,pix);
                }
                else {
                    rotatedBmp.SetPixel(i,j,Color.clear);
                }
            }
        }
        rotatedBmp.Apply();
        return rotatedBmp;
    }

    public void AddTheIngredient(){
        //make sure we have the values
        input_pixel_ratio = float.Parse (input_pixel_ratio_field.text );
        if (illustrated || Zrotation!= 0.0f) {
            if (Zrotation!= 0.0f) {
                //apply the rotation to the pixels.
                 var texture = RotateImageReverse(theSprite.sprite.texture,Zrotation);
                 theSprite.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                 Zrotation = 0.0f;
            }
            var current_bytes = theSprite.sprite.texture.EncodeToPNG();
            var filePath = PdbLoader.DefaultDataDirectory + "/" + "images/" + input_name_field.text+"_sprite_ill.png";
            System.IO.File.WriteAllBytes(filePath, current_bytes);
            sprite_name = input_name_field.text+"_sprite_ill.png";
        }
        Manager.Instance.recipeUI.AddOneIngredient(input_name_field.text, sprite_name, input_pixel_ratio, 
                                                    -input_offset_y, fiber_length, surface.isOn, fiber.isOn, "");
        input_name_field.text = "";
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError || webRequest.isHttpError )
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                var sprite = Resources.Load<Sprite>("Recipie/error");
                theSprite.sprite = sprite;
                theSpriteFiberLeft.sprite = sprite;
                theSpriteFiberRight.sprite = sprite;
                Load.interactable = true;
                Illustrate.interactable = true;
                Create.interactable = true;  
                if (loader) loader.gameObject.SetActive(false); 
                query_done = false;
                redo_query = false;
                log_label.text = webRequest.error;
            }
            else
            {
                //var filePath = PdbLoader.DownloadFile(input_name, "https://mesoscope.scripps.edu/data/tmp/ILL/"+query_id.ToString()+"/",  PdbLoader.DefaultDataDirectory + "/" + "images/", ".png");
                //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                //var filePath = PdbLoader.DefaultDataDirectory + "/" + "images/" + input_name.text+".png";
                //File.WriteAllText(filePath, webRequest.downloadHandler.text);
                query_done = true;
                redo_query = false;
                query_sent = true; 
                log_label.text = "";
                //var sprite = Manager.Instance.LoadNewSprite(filePath);
                //theSprite.sprite = sprite;
            }
        }
    }

/*
    private IEnumerator onResponse(UnityWebRequest req)
    {
        yield return req.SendWebRequest();
        if (req.isNetworkError)
            Debug.Log("Network error has occured: " + req.GetResponseHeader(""));
        else{
            Debug.Log("isDone "+req.isDone );
            Debug.Log("Success "+req.downloadHandler.text );
            query_answer = req.downloadHandler.text;
            if (query_answer.Contains("https://mesoscope.scripps.edu/data/tmp/ILL")){
                var start = query_answer.IndexOf("https://mesoscope");
                var end = query_answer.IndexOf(".png");
                var length = (end+4)-start;
                Debug.Log(start);
                Debug.Log(end);
                query_answer_url = query_answer.Substring(start,length);
                StartCoroutine(GetRequest(query_answer_url));
            }
            else 
            {
                query_done = false;
                redo_query = true;       
                query_sent = true;         
            }
        }
    }*/
    
    IEnumerator GetText(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError )
            {
                Debug.Log(": Error: " + uwr.error);
                query_done = false;
                redo_query = false;
                var sprite = Resources.Load<Sprite>("Recipie/error");
                theSprite.sprite = sprite;
                theSpriteFiberLeft.sprite = sprite;
                theSpriteFiberRight.sprite = sprite;
                Load.interactable = true;
                Illustrate.interactable = true;
                Create.interactable = true;  
                if (loader) loader.gameObject.SetActive(false); 
                log_label.text = uwr.error;           
            }
            else
            {
                // Get downloaded asset bundle
                tmp_texture = DownloadHandlerTexture.GetContent(uwr);
                query_done = false;
                var mySprite = Sprite.Create(tmp_texture, new Rect(0.0f, 0.0f, tmp_texture.width, tmp_texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                theSprite.sprite = mySprite;
                theSpriteFiberLeft.sprite = mySprite;
                theSpriteFiberRight.sprite = mySprite;
                var ratio =(float) theSprite.sprite.texture.width/(float)theSprite.sprite.texture.height;
                //var w = 150;//(snode.data.thumbnail)?snode.data.thumbnail.width:150;
                var h = 210;//w/ratio;//(snode.data.thumbnail)?snode.data.thumbnail.height:150;
                var w = h*ratio;
                theSprite.rectTransform.sizeDelta = new Vector2(w,(int)h);    
                if (loader) loader.gameObject.SetActive(false);                  
                //driver.Close();     
                //driver.Quit();      
                log_label.text = "";     
            }
            Load.interactable = true;
            Illustrate.interactable = true;
            Create.interactable = true;
            if (use_webDriver) {
                 driver.Close();
                driver.Quit();
            }
        }
    }

    public void AddCompartment(){
        //when creating a compartment give a name and create the membrane prefab for it.
        //check if name exist already
        var cname = comp_name_field.text;
        if (Manager.Instance.recipeUI.CompartmentsNames.ContainsKey(cname)) {
            log_label.text = cname+" already exist in the list of compartment. Choose a different name.";
            return;
        }
        Manager.Instance.recipeUI.AddOneCompartment(cname);
        log_label.text = "";
        gameObject.SetActive(false);
        Manager.Instance.mask_ui = false;
    }

    void OnApplicationQuit(){
        if (driver != null && use_webDriver) {
            driver.Close();
            driver.Quit();
        }
    }
}
