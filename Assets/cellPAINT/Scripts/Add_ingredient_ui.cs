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
//using OpenQA.Selenium.Firefox;

public class Add_ingredient_ui : MonoBehaviour
{
    public InputField input_pdb;
    public InputField input_name;
    public InputField input_seletion;
    public InputField input_bu;
    public InputField input_model;
    public InputField input_pixel_ratio;
    public InputField input_offset_y;

    public int query_id;
    public Image theSprite;
    public Toggle surface;
    public Toggle fiber;
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
        
    }

    // Update is called once per frame
    void Update()
    {
        query_answer_url ="https://mesoscope.scripps.edu/data/tmp/ILL/"+query_id.ToString()+"/"+input_name.text+".png";
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
            var filePath = PdbLoader.DownloadFile(input_name.text, "https://mesoscope.scripps.edu/data/tmp/ILL/"+query_id.ToString()+"/",  PdbLoader.DefaultDataDirectory + "/" + "images/", ".png");
            var sprite = Manager.Instance.LoadNewSprite(filePath);
            theSprite.sprite = sprite;
            Force = false;
            //try the getTexture
            StartCoroutine(GetText(query_answer_url));
        }
    }

    public void CallIllustrate(){
        if (input_name.text == "") input_name.text = input_pdb.text;
        var q_id = (query_id==-1)? Mathf.CeilToInt(UnityEngine.Random.Range(0.0f, 1.0f)*1000000000.0f) : query_id;
        query_id = q_id;
        var query="https://mesoscope.scripps.edu/beta/illustratecall.html?pdbid="+input_pdb.text;
        if (input_name.text!="") query += "&name="+input_name.text;
        if (input_bu.text!="") query += "&bu="+input_bu.text;
        if (input_seletion.text!="") query += "&selection="+input_bu.text;
        if (input_model.text!="") query += "&model="+input_bu.text;
        query+="&qid="+q_id.ToString();
        //UnityWebRequest www = UnityWebRequest.Get(query);
        //StartCoroutine(onResponse(www));
        Debug.Log(query_answer);
        input_pixel_ratio.text = "6.0";
        sprite_name = input_name.text+".png";
#if UNITY_STANDALONE_OSX
        driver = new ChromeDriver(UnityEngine.Application.dataPath + "/../Data/webdrivers/mac/");
#elif UNITY_EDITOR_OSX
        driver = new ChromeDriver(UnityEngine.Application.dataPath + "/../Data/webdrivers/mac/");
#elif UNITY_STANDALONE_WIN
        driver = new ChromeDriver(UnityEngine.Application.dataPath + "/../Data/webdrivers/win/");
#elif UNITY_EDITOR_WIN
        driver = new ChromeDriver(UnityEngine.Application.dataPath + "/../Data/webdrivers/win/");
#elif UNITY_STANDALONE_LINUX
        driver = new ChromeDriver(UnityEngine.Application.dataPath + "/../Data/webdrivers/linux/");
#else 
        driver = new ChromeDriver(UnityEngine.Application.dataPath + "/../Data/webdrivers/win/");
#endif
        //driver = new FirefoxDriver(UnityEngine.Application.dataPath + "/../Data/webdrivers");
        //query_answer = wb.Document.DomDocument.ToString();
        driver.Url = query;
        query_answer_url = driver.PageSource;
        Debug.Log(driver.Title.ToString());
        query_answer_url = driver.PageSource;
        query_done = false;
        redo_query = true;       
        query_sent = true;  

    }

    public void LoadASprite(){
        //call fileBrowser and pass it to load sprites
        string filePath = FileBrowser.OpenSingleFile("Open image file", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "png", "jpg");
        var sprite = Manager.Instance.LoadNewSprite(filePath);
        theSprite.sprite = sprite;
        sprite_name = Path.GetFileName(filePath);
        sprite_path = Path.GetDirectoryName(filePath);
        PdbLoader.DataDirectories.Add(sprite_path);
    }

    public void AddTheIngredient(){
        float scale2d = float.Parse (input_pixel_ratio.text);
        float yoffset = (input_offset_y.text!="")?float.Parse (input_offset_y.text):0.0f;
        Manager.Instance.recipeUI.AddOneIngredient(input_name.text, sprite_name, scale2d, yoffset, surface.isOn, fiber.isOn, 0);
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
                redo_query = true;
            }
            else
            {
                var filePath = PdbLoader.DownloadFile(input_name.text, "https://mesoscope.scripps.edu/data/tmp/ILL/"+query_id.ToString()+"/",  PdbLoader.DefaultDataDirectory + "/" + "images/", ".png");
                //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                //var filePath = PdbLoader.DefaultDataDirectory + "/" + "images/" + input_name.text+".png";
                //File.WriteAllText(filePath, webRequest.downloadHandler.text);
                query_done = true;
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
                redo_query = true;
            }
            else
            {
                // Get downloaded asset bundle
                var texture = DownloadHandlerTexture.GetContent(uwr);
                var mySprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                theSprite.sprite = mySprite;
                var current_bytes = texture.EncodeToPNG();
                var filePath = PdbLoader.DefaultDataDirectory + "/" + "images/" + input_name.text+".png";
                System.IO.File.WriteAllBytes(filePath, current_bytes);
                query_done = true;
                redo_query = false;
                driver.Close();     
                driver.Quit();           
            }
        }
    }
}
