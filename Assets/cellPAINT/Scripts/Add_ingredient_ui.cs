using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Crosstales.FB;
using System.IO;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
//using OpenQA.Selenium.Firefox;
using System.Threading;
using System.Threading.Tasks;

public class Add_ingredient_ui : MonoBehaviour
{
    public InputField input_pdb_field;
    public InputField input_name_field;
    public InputField input_seletion_field;
    public InputField input_bu_field;
    public InputField input_model_field;
    public InputField input_pixel_ratio_field;
    public InputField input_offset_y_field;

    public int query_id;
    public Image theSprite;
    public Toggle surface;
    public Toggle fiber;
    public Image loader;
    public string query_answer="";
    public string query_answer_url="";
    public bool query_sent = false;
    public bool query_done = true;
    public bool redo_query = false;
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
    public float input_pixel_ratio;
    public float input_offset_y;    
    private Texture2D tmp_texture;
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
            Force = false;
            //try the getTexture
            StartCoroutine(GetText(query_answer_url));
        }
        if (query_done) {
            StartCoroutine(GetText(query_answer_url));
            query_done = false;
        }
    }

    public void CallIllustrate(){
        if (loader) loader.gameObject.SetActive(true);   
        if (input_name_field.text == "") input_name_field.text = input_pdb_field.text;
        input_name = input_name_field.text;
        input_pdb = input_pdb_field.text;
        input_bu = input_bu_field.text;
        input_seletion = input_seletion_field.text;
        input_model = input_model_field.text;
        input_name = input_name_field.text;
        input_pixel_ratio_field.text = "6.0";
        var q_id = (query_id==-1)? Mathf.CeilToInt(UnityEngine.Random.Range(0.0f, 1.0f)*1000000000.0f) : query_id;
        query_id = q_id;
        DoAsyncIllustrate();
    }

    public void DoCallIllustrate(){
        Debug.Log("DoCallIllustrate");
        var query="https://mesoscope.scripps.edu/beta/illustratecall.html?pdbid="+input_pdb;
        if (input_name !="") query += "&name="+input_name;
        if (input_bu!="") query += "&bu="+input_bu;
        if (input_seletion!="") query += "&selection="+input_bu;
        if (input_model!="") query += "&model="+input_bu;
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
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
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
        //call fileBrowser and pass it to load sprites
        string filePath = FileBrowser.OpenSingleFile("Open image file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "png", "jpg");
        var sprite = Manager.Instance.LoadNewSprite(filePath);
        theSprite.sprite = sprite;
        sprite_name = Path.GetFileName(filePath);
        sprite_path = Path.GetDirectoryName(filePath);
        PdbLoader.DataDirectories.Add(sprite_path);
        Manager.Instance.AddUserDirectory(sprite_path);
    }

    public void AddTheIngredient(){
        float scale2d = float.Parse (input_pixel_ratio_field.text);
        float yoffset = (input_offset_y_field.text!="")?float.Parse (input_offset_y_field.text):0.0f;
        Manager.Instance.recipeUI.AddOneIngredient(input_name_field.text, sprite_name, scale2d, yoffset, surface.isOn, fiber.isOn, 0);
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
                query_done = false;
                redo_query = false;
            }
            else
            {
                var filePath = PdbLoader.DownloadFile(input_name, "https://mesoscope.scripps.edu/data/tmp/ILL/"+query_id.ToString()+"/",  PdbLoader.DefaultDataDirectory + "/" + "images/", ".png");
                //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                //var filePath = PdbLoader.DefaultDataDirectory + "/" + "images/" + input_name.text+".png";
                //File.WriteAllText(filePath, webRequest.downloadHandler.text);
                query_done = false;
                redo_query = false;
                var sprite = Manager.Instance.LoadNewSprite(filePath);
                theSprite.sprite = sprite;
                driver.Close();
            }
        }
    }

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
    }
    
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
            }
            else
            {
                // Get downloaded asset bundle
                tmp_texture = DownloadHandlerTexture.GetContent(uwr);
                query_done = false;
                var mySprite = Sprite.Create(tmp_texture, new Rect(0.0f, 0.0f, tmp_texture.width, tmp_texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                theSprite.sprite = mySprite;
                var current_bytes = tmp_texture.EncodeToPNG();
                var filePath = PdbLoader.DefaultDataDirectory + "/" + "images/" + input_name+".png";
                System.IO.File.WriteAllBytes(filePath, current_bytes);    
                if (loader) loader.gameObject.SetActive(false);       
                driver.Close();     
                driver.Quit();           
            }
        }
    }
}
