using UnityEngine;
//using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using HexGrid;
using System.Runtime;
using System.Runtime.InteropServices;

//[ExecuteInEditMode]
public class RecipeUI : MonoBehaviour {
    public TextAsset recipe;
    public TextAsset ingredients_description;
    public GameObject item_prefab;
    public GameObject item_list_prefab;
    public bool is_gridView = true; //Grid is the starting state.
    public Dropdown recipe_list;
    public Text current_recipe_label;
    public Text current_compartment_label;
    public int Number_items = 1;
    public float Radius = 1.0f;
    public int recipe_id = 0;
    //public static JSONNode resultData;
    public int map_radius = 1;
    public List<TextAsset> recipes;
    public int current_id;
    public string current_recipe_file;
    public GameObject parent;
    public GameObject delete_panel;
    private GameObject item;

    private float last_radius;
    private int last_nbitems;
    private List<Hex> hex_grid;
    private List<GameObject> hex_instance;
    private List<GameObject> hex_list_instance;
    public List<int> Compartments;
    public Dictionary<int,string> CompartmentsIDS;
    public Dictionary<string,int> CompartmentsNames;
    public bool use_coroutine=false;
    public int current_cid = 0;
    public string current_cname = "";
    private Dictionary<int, List<int>> CompartmentsIngredients_ids;//migrate to keep track of ingredients id to support merge
    private bool isGrouped = true; ////3/17/17
    public string current_selection;
    public List<int> selection = new List<int>();
    public bool merge_upon_loading = false;
    private System.Random random_uid = new System.Random();

    void OnEnable() {
        Manager.Instance.recipeUI = this;
        Manager.Instance.ingredient_node = new Dictionary<string, JSONNode>();
        Compartments = new List<int>();
        CompartmentsIDS = new Dictionary<int, string>();
        CompartmentsNames = new Dictionary<string, int>();
        CompartmentsIngredients_ids = new Dictionary<int, List<int>>();
        hex_instance = new List<GameObject>();
        hex_list_instance = new List<GameObject>();
        hex_grid = new List<Hex>();

        if (Manager.Instance.AllIngredients == null)
            GetAllIngredientsInfo();

        /*if (Manager.Instance.AllRecipes == null)
            Manager.Instance.AllRecipes = Helper.GetAllRecipeInfo();
        if (Manager.Instance.AllIngredients == null)
            Manager.Instance.AllIngredients = Helper.GetAllIngredientsInfo();

        recipe_list.ClearOptions();
        List<string> recipes_names = new List<string>();
        for (int i = 0; i < Manager.Instance.AllRecipes.Count; i++)
        {
            recipes_names.Add(Manager.Instance.AllRecipes.GetKey(i));
        }
        recipe_list.AddOptions(recipes_names);
        recipe_list.value = recipe_id;

        string url = Manager.Instance.AllRecipes[recipe_id][0]["resultfile"];
        //Debug.Log(SceneManager.Instance.AllRecipes.GetKey(recipe_id));
        //Debug.Log("load recipe at " + url);
        url = url.Replace("autoPACKserver", "https://raw.githubusercontent.com/mesoscope/cellPACK_data/master/cellPACK_database_1.1.0/");
        //fetch the results file from the server
        //var path = Helper.GetResultsFile(url);
        */
        //LoadRecipe();
        //current_compartment_label.text = Compartments[current_compartments];
        //current_recipe_label.text = SceneManager.Instance.AllRecipes.GetKey(recipe_id);
    }

    public void ChangeRecipe(int id) {
        recipe_id = id;
        string url = Manager.Instance.AllRecipes[recipe_id][0]["resultfile"];
        //Debug.Log(SceneManager.Instance.AllRecipes.GetKey(recipe_id));
        //Debug.Log("load recipe at " + url);
        //url = url.Replace("autoPACKserver", "https://raw.githubusercontent.com/mesoscope/cellPACK_data/master/cellPACK_database_1.1.0/");
        //fetch the results file from the server
        //var path = Helper.GetResultsFile(url);
        LoadRecipe();
        //current_compartments = 0;
        //current_compartment_label.text = Compartments[current_compartments];
        //current_recipe_label.text = SceneManager.Instance.AllRecipes.GetKey(recipe_id);
        //StartCoroutine(populateHexGridFromCenterSpiral());
    }

    // Use this for initialization
    void Start() {
        //StartCoroutine(populateHexGridFromCenterSpiral());
        Manager.Instance.recipeUI = this;
        ///populateHexGridFromCenterSpiral();
    }

    public void updateHexInstance()
    {
        //int start_id = (int)CompartmentsIngredients[current_compartments].x;
        //int total_counts = (int)CompartmentsIngredients[current_compartments].y; //total number of sprite to display
        var cid = Compartments[current_cid];
        int total_counts = CompartmentsIngredients_ids[cid].Count;
        for (int count = 0; count < total_counts; count++)
        {
            var ig_id = CompartmentsIngredients_ids[cid][count];
            if (is_gridView)
            {
                if (Manager.Instance.ingredients_ids.ContainsKey(ig_id))// ig_id < Manager.Instance.ingredients_names.Count)
                {
                    string iname = Manager.Instance.ingredients_ids[ig_id];
                    if (count < hex_instance.Count)
                    {
                        if (Manager.Instance.bucketMode)
                            hex_instance[count].GetComponent<Toggle>().group = null;
                        else
                            hex_instance[count].GetComponent<Toggle>().group = GetComponent<ToggleGroup>();

                        if (Manager.Instance.selected_prefab.Contains(iname))
                            hex_instance[count].GetComponent<Toggle>().isOn = true;
                        else
                            hex_instance[count].GetComponent<Toggle>().isOn = false;
                    }
                }
            }
            else
            {
                if (Manager.Instance.ingredients_ids.ContainsKey(ig_id))// ig_id < Manager.Instance.ingredients_names.Count)
                {
                    string iname = Manager.Instance.ingredients_ids[ig_id];
                    if (count < hex_list_instance.Count)
                    {
                        if (Manager.Instance.bucketMode)
                            hex_list_instance[count].GetComponent<Toggle>().group = null;
                        else
                            hex_list_instance[count].GetComponent<Toggle>().group = GetComponent<ToggleGroup>();

                        if (Manager.Instance.selected_prefab.Contains(iname))
                            hex_list_instance[count].GetComponent<Toggle>().isOn = true;
                        else
                            hex_list_instance[count].GetComponent<Toggle>().isOn = false;
                    }
                }
            }
        }
    }

    void Update()
    {
    }

    public static string LoadResourceJsonTextfile(string path)
    {

        string filePath =  path.Replace(".json", "");
        Debug.Log("try loading ressource filePath "+filePath);
        TextAsset targetFile = Resources.Load<TextAsset>(filePath);
        if (targetFile == null){
            Debug.Log("problem loading ressource filePath "+filePath);
        }
        return targetFile.text;
    }

    void GetAllIngredientsInfo() {
        JSONNode resultData;
        if (ingredients_description == null)
        {
            //look into ressource folder
            var fileContents = LoadResourceJsonTextfile("/Recipes/Blood_HIV_TCell_cellPAINT.json");
            resultData = JSONNode.Parse(fileContents);
        }
        else {
            resultData = JSONNode.Parse(ingredients_description.text);
        }

        //filter the name
        for (int i = 0; i < resultData.Count; i++)
        {
            if (resultData[i]["file"].Value.Contains("HIV"))
            {
                string key = resultData.GetKey(i);
                if (key.Contains("NC"))
                    resultData.ChangeKey(key, "HIV_" + key.Split('_')[1] + "_" + key.Split('_')[2]);
                else if (key.Contains("P6_VPR"))
                    resultData.ChangeKey(key, "HIV_" + key.Split('_')[1] + "_" + key.Split('_')[2]);
                else
                    resultData.ChangeKey(key, "HIV_" + key.Split('_')[1]);
                Debug.Log("new key is " + "HIV_" + key.Split('_')[1] + " " + key);
            }
        }
        Manager.Instance.AllIngredients = resultData;
    }

    public void Clear(){
        Manager.Instance.Clear();
        Manager.Instance.ingredient_node.Clear();
        Compartments.Clear();
        CompartmentsIDS.Clear();
        CompartmentsNames.Clear();
        CompartmentsIngredients_ids.Clear();
        //Manager.Instance.ingredients_prefab.Clear();
        Manager.Instance.all_prefab.Clear();
        Manager.Instance.ingredients_names.Clear();
        Manager.Instance.ingredients_ids.Clear();
        Manager.Instance.additional_ingredients_names.Clear();
        Manager.Instance.additional_compartments_names.Clear();
        Manager.Instance.sprites_names.Clear();
        Manager.Instance.sprites_textures.Clear();
        Manager.Instance.prefab_materials = new Dictionary<string, Material>();             
    }

    public void LoadRessourceRecipe(int id){
        current_id = id;
        JSONNode resultData = JSONNode.Parse(recipes[id].text);
        if (merge_upon_loading){
            MergeRecipe_cb(resultData);
            return;
        }
        Manager.Instance.Clear();
        Manager.Instance.ingredient_node.Clear();
        Compartments.Clear();
        CompartmentsIDS.Clear();
        CompartmentsNames.Clear();
        CompartmentsIngredients_ids.Clear();
        //Manager.Instance.ingredients_prefab.Clear();
        Manager.Instance.all_prefab.Clear();
        Manager.Instance.ingredients_names.Clear();
        Manager.Instance.ingredients_ids.Clear();
        Manager.Instance.additional_ingredients_names.Clear();
        Manager.Instance.additional_compartments_names.Clear();
        Manager.Instance.sprites_names.Clear();
        Manager.Instance.prefab_materials = new Dictionary<string, Material>();        
        Debug.Log("try loading ressource "+id.ToString());
        LoadRecipe_cb(resultData);
    }
    //need to add the membrane
    
    public void LoadRecipe(string filename=null) {
        //Debug.Log("*****");
        //Debug.Log("Loading scene: " + recipePath);
        current_recipe_file = filename;
        if (merge_upon_loading){
            MergeRecipe(filename);
            return;
        }
        Manager.Instance.ingredient_node.Clear();
        Compartments.Clear();
        CompartmentsIDS.Clear();
        CompartmentsNames.Clear();
        CompartmentsIngredients_ids.Clear();
        //Manager.Instance.ingredients_prefab.Clear();
        //Destroy all prefab ?
        foreach (var KeyVal in Manager.Instance.all_prefab)
        {
            GameObject.Destroy(KeyVal.Value);
        }
        Manager.Instance.all_prefab.Clear();
        Manager.Instance.ingredients_names.Clear();
        Manager.Instance.ingredients_ids.Clear();
        Manager.Instance.additional_ingredients_names.Clear();
        Manager.Instance.additional_compartments_names.Clear();
        Manager.Instance.sprites_names.Clear();
        Manager.Instance.prefab_materials = new Dictionary<string, Material>();
        JSONNode resultData;

        if (filename == null)
        {
            if (recipe == null)
            {
                var fileContents = LoadResourceJsonTextfile("/Recipes/Blood_HIV_TCell_cellPAINT.json");
                resultData = JSONNode.Parse(fileContents);
            }
            else
            {
                resultData = JSONNode.Parse(recipe.text);
            }
        }
        else {
            //add the filename directory as a search directory for data
            //PdbLoader.DataDirectories.Add(Path.GetDirectoryName(filename));
            Manager.Instance.AddUserDirectory(Path.GetDirectoryName(filename));
            resultData = JSONNode.Parse(File.ReadAllText(filename)); //JSONNode.LoadFromFile(filename);
        }
        if (use_coroutine) StartCoroutine(LoadRecipe_cb_CR(resultData));
        else LoadRecipe_cb(resultData);
    }

    public void LoadRecipe_cb(JSONNode resultData) {
        
        int nCompartemnts = 0;
        int nIngredients = 0;
        int nC = 0;
        if (resultData["cytoplasme"] != null)
        {
            var cid = random_uid.Next();
            Compartments.Add(cid);//"Exterior");//"Blood Plasma");
            nIngredients += resultData["cytoplasme"]["ingredients"].Count;
            //CompartmentsIngredients.Add(nC, new Vector2(0, nIngredients));
            CompartmentsIngredients_ids.Add(cid, new List<int>());
            CompartmentsIDS.Add(cid,"Exterior");
            CompartmentsNames.Add("Exterior",cid);
            Debug.Log("found 0 " + nIngredients.ToString());
            nCompartemnts += 1;
            nC++;
        }
        //for each compartment should add a Cell_Membrane
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            var cid = random_uid.Next();
            Compartments.Add(cid);//.Add(resultData["compartments"].GetKey(i));
            int total = resultData["compartments"][i]["interior"]["ingredients"].Count +
                resultData["compartments"][i]["surface"]["ingredients"].Count + 1;
            //CompartmentsIngredients.Add(nC, new Vector2(nIngredients, total));
            CompartmentsIngredients_ids.Add(cid, new List<int>());
            CompartmentsIDS.Add(cid,resultData["compartments"].GetKey(i));
            CompartmentsNames.Add(resultData["compartments"].GetKey(i),cid);
            Debug.Log("found " + nIngredients.ToString() + " " + total.ToString()+" "+i.ToString());
            nIngredients += total;
            nCompartemnts += 2;
            nC++;
        }
        if (nCompartemnts < 2)
            nCompartemnts = 2;
        DateTime start = DateTime.Now;
        nC = 0;
        if (resultData["cytoplasme"] != null)
        {
            var cid = CompartmentsNames["Exterior"];
            AddRecipeIngredients(cid, resultData["cytoplasme"]["ingredients"], "interior",false);
            nC++;
        }

        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            //add the membrane
            //SceneManager.Instance.ingredients_names.Add("DrawMembrane");
            var cid = CompartmentsNames[resultData["compartments"].GetKey(i)];
            AddRecipeIngredientMembrane(cid,null, resultData["compartments"][i]);//"Cell_Membrane"
            AddRecipeIngredients(cid, resultData["compartments"][i]["surface"]["ingredients"], "surface" + i.ToString(),true);
            AddRecipeIngredients(cid, resultData["compartments"][i]["interior"]["ingredients"], "interior" + i.ToString(),false);
            nC++;
        }
        //this is comment as I manually change a lot of name of the recipe to a better reading for the NSF competition. 
        //buildHierarchy (resultData);
        current_cid = -1;
        loadNextCompartments();
        //load first ingredient of first compartments
        int ingid = CompartmentsIngredients_ids[Compartments[current_cid]][0];
        string iname = Manager.Instance.ingredients_ids[ingid];
        Manager.Instance.SwitchPrefabFromName(iname);
        var prefab = Manager.Instance.all_prefab[iname];
        Manager.Instance.changeDescription(prefab, prefab.GetComponent<SpriteRenderer>());
        if (is_gridView)
        {
            hex_instance[0].GetComponent<Toggle>().isOn = true;
        }
        else
        {
            hex_list_instance[0].GetComponent<Toggle>().isOn = true;
        }

    }
    
    public void SetTofirstIngredient(){
        //load first ingredient of first compartments
        int ingid = CompartmentsIngredients_ids[Compartments[current_cid]][0];
        string iname = Manager.Instance.ingredients_ids[ingid];
        Manager.Instance.SwitchPrefabFromName(iname);        
    }

    public IEnumerator LoadRecipe_cb_CR(JSONNode resultData) {
        
        int nCompartemnts = 0;
        int nIngredients = 0;
        int nC = 0;
        if (resultData["cytoplasme"] != null)
        {
            var cid = random_uid.Next();
            Compartments.Add(cid);//"Exterior");//"Blood Plasma");
            nIngredients += resultData["cytoplasme"]["ingredients"].Count;
            //CompartmentsIngredients.Add(nC, new Vector2(0, nIngredients));
            CompartmentsIngredients_ids.Add(cid, new List<int>());
            CompartmentsIDS.Add(cid,"Exterior");
            CompartmentsNames.Add("Exterior",cid);
            Debug.Log("found 0 " + nIngredients.ToString());
            nCompartemnts += 1;
            nC++;
        }
        //for each compartment should add a Cell_Membrane
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            var cid = random_uid.Next();
            Compartments.Add(cid);//.Add(resultData["compartments"].GetKey(i));
            int total = resultData["compartments"][i]["interior"]["ingredients"].Count +
                resultData["compartments"][i]["surface"]["ingredients"].Count + 1;
            //CompartmentsIngredients.Add(nC, new Vector2(nIngredients, total));
            CompartmentsIngredients_ids.Add(cid, new List<int>());
            CompartmentsIDS.Add(cid,resultData["compartments"].GetKey(i));
            CompartmentsNames.Add(resultData["compartments"].GetKey(i),cid);
            Debug.Log("found " + nIngredients.ToString() + " " + total.ToString()+" "+i.ToString());
            nIngredients += total;
            nCompartemnts += 2;
            nC++;
        }
        if (nCompartemnts < 2)
            nCompartemnts = 2;
        DateTime start = DateTime.Now;
        nC = 0;
        if (resultData["cytoplasme"] != null)
        {
            UI_manager.Get.UpdatePB(0.5f,"loading cytoplasme ingredients");       
            yield return null;
            var cid = CompartmentsNames["Exterior"];
            AddRecipeIngredients(cid, resultData["cytoplasme"]["ingredients"], "interior",false);
            nC++;
        }

        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            UI_manager.Get.UpdatePB((float)i/(float)(resultData["compartments"].Count),"loading "+resultData["compartments"].GetKey(i)+" ingredients");       
            yield return null;            
            //add the membrane
            //SceneManager.Instance.ingredients_names.Add("DrawMembrane");
            var cid = CompartmentsNames[resultData["compartments"].GetKey(i)];
            AddRecipeIngredientMembrane(cid,null, resultData["compartments"][i]);//"Cell_Membrane"
            AddRecipeIngredients(cid, resultData["compartments"][i]["surface"]["ingredients"], "surface" + i.ToString(),true);
            AddRecipeIngredients(cid, resultData["compartments"][i]["interior"]["ingredients"], "interior" + i.ToString(),false);
            nC++;
        }
        //this is comment as I manually change a lot of name of the recipe to a better reading for the NSF competition. 
        //buildHierarchy (resultData);
        current_cid = -1;
        loadNextCompartments();
        //load first ingredient of first compartments
        int ingid = CompartmentsIngredients_ids[Compartments[current_cid]][0];
        string iname = Manager.Instance.ingredients_ids[ingid];
        Manager.Instance.SwitchPrefabFromName(iname);
        var prefab = Manager.Instance.all_prefab[iname];
        Manager.Instance.changeDescription(prefab, prefab.GetComponent<SpriteRenderer>());
        if (is_gridView)
        {
            hex_instance[0].GetComponent<Toggle>().isOn = true;
        }
        else
        {
            hex_list_instance[0].GetComponent<Toggle>().isOn = true;
        }

    }

    public void MergeRecipe(string filename = null) 
    {
        JSONNode resultData;
        if (filename == null)
        {
            if (recipe == null)
            {
                var fileContents = LoadResourceJsonTextfile("/Recipes/Blood_HIV_TCell_cellPAINT.json");
                resultData = JSONNode.Parse(fileContents);
            }
            else
            {
                resultData = JSONNode.Parse(recipe.text);
            }
        }
        else {
            //add the filename directory as a search directory for data
            //PdbLoader.DataDirectories.Add(Path.GetDirectoryName(filename));
            Manager.Instance.AddUserDirectory(Path.GetDirectoryName(filename));
            resultData = JSONNode.Parse(File.ReadAllText(filename)); //JSONNode.LoadFromFile(filename);
        }
        if (use_coroutine) StartCoroutine(MergeRecipe_cb_CR(resultData));
        else MergeRecipe_cb(resultData);
    }

    public void MergeRecipe_cb(JSONNode resultData)
    {
        int nCompartemnts = Compartments.Count;
        int nIngredients = Manager.Instance.ingredients_names.Count;
        int nC = nCompartemnts;
        //for each compartment should add a Cell_Membrane
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            //if (Compartments.Contains(resultData["compartments"].GetKey(i))) continue;
            if (CompartmentsNames.ContainsKey(resultData["compartments"].GetKey(i))) continue;
            Compartments.Add(nC);//.Add(resultData["compartments"].GetKey(i));
            //CompartmentsIngredients.Add(nC, new Vector2(nIngredients, total));
            CompartmentsIDS.Add(nC,resultData["compartments"].GetKey(i));
            CompartmentsNames.Add(resultData["compartments"].GetKey(i),nC);
            CompartmentsIngredients_ids.Add(nC, new List<int>());
            nC++;
        }
        DateTime start = DateTime.Now;
        if (resultData["cytoplasme"] != null)
        {
            nC =  CompartmentsNames["Exterior"];//("Blood Plasma");
            AddRecipeIngredients(nC,resultData["cytoplasme"]["ingredients"], "interior",false);
        }
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            //add the membrane
            //SceneManager.Instance.ingredients_names.Add("DrawMembrane");
            nC =  CompartmentsNames[resultData["compartments"].GetKey(i)];//Compartments.IndexOf(resultData["compartments"].GetKey(i));
            if (!Manager.Instance.ingredients_names.ContainsKey(resultData["compartments"][i]["name"].Value)) 
            {
                AddRecipeIngredientMembrane(nC,null,resultData["compartments"][i]);//"Cell_Membrane"
            }
            AddRecipeIngredients(nC,resultData["compartments"][i]["surface"]["ingredients"], "surface" + i.ToString(),true);
            AddRecipeIngredients(nC,resultData["compartments"][i]["interior"]["ingredients"], "interior" + i.ToString(),false);
        }
        //this is comment as I manually change a lot of name of the recipe to a better reading for the NSF competition. 
        //buildHierarchy (resultData);
        current_cid = 0;
        loadNextCompartments();
        //load first ingredient of first compartments
        int ingid = CompartmentsIngredients_ids[Compartments[current_cid]][0];
        string iname = Manager.Instance.ingredients_ids[ingid];
        Manager.Instance.SwitchPrefabFromName(iname);
        var prefab = Manager.Instance.all_prefab[iname];
        Manager.Instance.changeDescription(prefab, prefab.GetComponent<SpriteRenderer>());
        if (is_gridView)
        {
            hex_instance[0].GetComponent<Toggle>().isOn = true;
        }
        else
        {
            hex_list_instance[0].GetComponent<Toggle>().isOn = true;
        }
    }

    public IEnumerator MergeRecipe_cb_CR(JSONNode resultData)
    {
        int nCompartemnts = Compartments.Count;
        int nIngredients = Manager.Instance.ingredients_names.Count;
        int nC = nCompartemnts;
        //for each compartment should add a Cell_Membrane
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            //if (Compartments.Contains(resultData["compartments"].GetKey(i))) continue;
            if (CompartmentsNames.ContainsKey(resultData["compartments"].GetKey(i))) continue;
            Compartments.Add(nC);//.Add(resultData["compartments"].GetKey(i));
            //CompartmentsIngredients.Add(nC, new Vector2(nIngredients, total));
            CompartmentsIDS.Add(nC,resultData["compartments"].GetKey(i));
            CompartmentsNames.Add(resultData["compartments"].GetKey(i),nC);
            CompartmentsIngredients_ids.Add(nC, new List<int>());
            nC++;
        }
        DateTime start = DateTime.Now;
        if (resultData["cytoplasme"] != null)
        {
            UI_manager.Get.UpdatePB(0.5f,"loading exterior ingredients");       
            yield return null;  
            nC =  CompartmentsNames["Exterior"];//("Blood Plasma");
            AddRecipeIngredients(nC,resultData["cytoplasme"]["ingredients"], "interior",false);
        }
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            UI_manager.Get.UpdatePB((float)i/(float)(resultData["compartments"].Count),"loading "+resultData["compartments"].GetKey(i)+" ingredients");       
            yield return null;  
            //add the membrane
            //SceneManager.Instance.ingredients_names.Add("DrawMembrane");
            nC =  CompartmentsNames[resultData["compartments"].GetKey(i)];//Compartments.IndexOf(resultData["compartments"].GetKey(i));
            if (!Manager.Instance.ingredients_names.ContainsKey(resultData["compartments"][i]["name"].Value)) 
            {
                AddRecipeIngredientMembrane(nC,null,resultData["compartments"][i]);//"Cell_Membrane"
            }
            AddRecipeIngredients(nC,resultData["compartments"][i]["surface"]["ingredients"], "surface" + i.ToString(),true);
            AddRecipeIngredients(nC,resultData["compartments"][i]["interior"]["ingredients"], "interior" + i.ToString(),false);
        }
        //this is comment as I manually change a lot of name of the recipe to a better reading for the NSF competition. 
        //buildHierarchy (resultData);
        current_cid = 0;
        loadNextCompartments();
        //load first ingredient of first compartments
        int ingid = CompartmentsIngredients_ids[Compartments[current_cid]][0];
        string iname = Manager.Instance.ingredients_ids[ingid];
        Manager.Instance.SwitchPrefabFromName(iname);
        var prefab = Manager.Instance.all_prefab[iname];
        Manager.Instance.changeDescription(prefab, prefab.GetComponent<SpriteRenderer>());
        if (is_gridView)
        {
            hex_instance[0].GetComponent<Toggle>().isOn = true;
        }
        else
        {
            hex_list_instance[0].GetComponent<Toggle>().isOn = true;
        }
    }


    //need the path in the name so we keep info on compartment
    public void AddRecipeIngredients(int cid, JSONNode recipeDictionary, string prefix,bool surface)
    {
        for (int j = 0; j < recipeDictionary.Count; j++)
        {
            string iname = recipeDictionary[j]["name"].Value;
            if (iname.Contains("DNA")) {
                iname = "Draw DNA_"+cid.ToString();
            }
            if (iname.Contains("hu"))
            {
                iname = "HU";
            }
            //iname = CompartmentsIDS[cid]+"."+iname;
            //if (Manager.Instance.ingredients_names.ContainsKey( iname )) continue;
            var ig_id = AddProteinIngredient(cid, recipeDictionary[j], prefix,surface);
            //var ig_id = Manager.Instance.ingredients_names[iname];
            if (ig_id == -1) {
                Debug.Log("didnt found "+iname);
            }
            CompartmentsIngredients_ids[cid].Add(ig_id);
        }
    }

    public void AddRecipeIngredientMembrane(int cid, string iname = null, JSONNode compDictionary=null)
    {
        Debug.Log("AddRecipeIngredientMembrane "+iname);
        int n = random_uid.Next();// Manager.Instance.ingredients_names.Count;
        if (iname==null && compDictionary!=null) {
            var cname = compDictionary["name"].Value;
            iname = cname+".surface."+cname+"_membrane";
        }
        string prefab_name = "Cell_Membrane";
        Debug.Log("AddRecipeIngredientMembrane "+iname);
        CompartmentsIngredients_ids[cid].Add(n);
        Manager.Instance.ingredient_node.Add(iname, compDictionary);
        Manager.Instance.ingredients_names.Add(iname,n);
        Debug.Log("Manager.Instance.ingredients_names "+Manager.Instance.ingredients_names[iname].ToString());
        Manager.Instance.ingredients_ids.Add(n,iname);
        Manager.Instance.sprites_names.Add(n,prefab_name);
        if (!Manager.Instance.prefab_materials.ContainsKey(iname))
        {
            Material mat = Manager.Instance.createNewSpriteMaterial(iname);
            //parse the color that is in the dictionary
            //if (compDictionary.HasKey("color")) Debug.Log(compDictionary["color"].Value);
            if (compDictionary!=null && compDictionary.HasKey("color")) {
                Debug.Log("color is for "+ iname);
                mat.color = new Color(  compDictionary["color"].AsArray[0].AsFloat,
                                        compDictionary["color"].AsArray[1].AsFloat,
                                        compDictionary["color"].AsArray[2].AsFloat);
                Debug.Log(mat.color);
            }
            Manager.Instance.prefab_materials.Add(iname, mat);
        }
        //GameObject myPrefab = Resources.Load("Prefabs/" + prefab_name) as GameObject;
        GameObject myPrefab = Instantiate(Resources.Load("Prefabs/" + prefab_name, typeof(GameObject))) as GameObject;
        myPrefab.name = iname;
        myPrefab.SetActive(true);
        myPrefab.transform.position = new Vector3(10000, 10000, 10000);
        Debug.Log("Load Membrane! "+iname);
        Debug.Log(myPrefab);
        PrefabProperties p = myPrefab.GetComponent<PrefabProperties>();
        p.name = iname;
        //myPrefab.name = iname;
        Manager.Instance.all_prefab.Add(iname, myPrefab);
        Debug.Log(iname +" added to all prefab "+ Manager.Instance.all_prefab.ContainsKey(iname).ToString());
        //List<string> keyList = new List<string>(Manager.Instance.all_prefab.Keys);
        //Debug.Log("keys " + keyList.Count.ToString());
        //foreach (var k in keyList) { Debug.Log("keys is " + k); }
        //Manager.Instance.ingredients_prefab.Add(myPrefab);
    }

    //build the gameobject and serialize it 
    public int AddProteinIngredient(int cid, JSONNode ingredientDictionary, string prefix, bool surface)
    {
        string iname = ingredientDictionary["name"].Value;
        string img_name = iname;
        if (iname.Contains("DNA")) {
            iname = "Draw DNA";//_"+cid.ToString();
            img_name = "Draw DNA";
        }
        if (iname.Contains("hu"))
        {
            iname = "HU";
            img_name = "HU";
        }
        if (ingredientDictionary.HasKey("sprite"))
        {
            if (ingredientDictionary["sprite"]["image"].Value!="") img_name = ingredientDictionary["sprite"]["image"].Value;
        }
        //JSONClass tmp = new JSONClass();
        ingredientDictionary.Add("surface", (surface)?"surface":"interior");
        Debug.Log("add to dictionary");
        Debug.Log(ingredientDictionary["surface"]);
        Debug.Log(ingredientDictionary["surface"].Value);
        ;
        /*
         * if (iname.StartsWith("HIV")) { 
            if (iname.Contains("_NC"))
                iname = "HIV_" + iname.Split('_')[1] + "_" + iname.Split('_')[2];
            else if (iname.Contains("P6_VPR"))
                iname = "HIV_" + iname.Split('_')[1] + "_" + iname.Split('_')[2];
            else
                iname = "HIV_" + iname.Split('_')[1];
            }
        */
        //dna and rna are special case....or not ?

        /*if (iname.Contains("Membrane")) {
            iname = "Membrane";
        }*/
        //var name = prefix + "_" + iname;
        var biomt = (bool)ingredientDictionary["source"]["biomt"].AsBool;
        var center = (bool)ingredientDictionary["source"]["transform"]["center"].AsBool;
        var pdbName = ingredientDictionary["source"]["pdb"].Value.Replace(".pdb", "");
        Debug.Log("iname is " + iname);
        var name = CompartmentsIDS[cid]+"."+prefix+"."+iname;
        int n = random_uid.Next();//int n = Manager.Instance.ingredients_names.Count;//could be a uniq number
        Manager.Instance.ingredients_names.Add(name,n);
        Manager.Instance.ingredients_ids.Add(n,name);
        Manager.Instance.sprites_names.Add(n,img_name);
        Manager.Instance.ingredient_node.Add(name, ingredientDictionary);
        //first build the sprite if doesnt exist
        if (!Manager.Instance.prefab_materials.ContainsKey(name))
        {
            Material mat = Manager.Instance.createNewSpriteMaterial(name);
            //parse the color that is in the dictionary
            if (ingredientDictionary.HasKey("color")) {
                Debug.Log("color is for "+ name);
                mat.color = new Color(  ingredientDictionary["color"].AsArray[0].AsFloat,
                                        ingredientDictionary["color"].AsArray[1].AsFloat,
                                        ingredientDictionary["color"].AsArray[2].AsFloat);
                Debug.Log(mat.color);
            }
            Manager.Instance.prefab_materials.Add(name, mat);
        }
        return n;
    }


    public void loadNextCompartments() {
        current_cid++;
        if (current_cid >= Compartments.Count)
            current_cid = 0;
        //current_compartment_label.text = Compartments[current_cid];
        current_cname = CompartmentsIDS[Compartments[current_cid]];
        current_compartment_label.text = CompartmentsIDS[Compartments[current_cid]];
        var cid = Compartments[current_cid];
        Debug.Log("populateHexGridFromCenterSpiral "+cid.ToString());
        Debug.Log("populateHexGridFromCenterSpiral "+CompartmentsIDS[cid]);
        Debug.Log("populateHexGridFromCenterSpiral "+CompartmentsIngredients_ids[cid].Count.ToString());
        if (use_coroutine) StartCoroutine(populateHexGridFromCenterSpiral());
        else populateGrid();
        //populateHexGridFromCenterSpiral();
    }

    public void loadPreviousCompartments() {
        current_cid--;
        if (current_cid < 0 )
            current_cid = Compartments.Count-1;
        current_cname = CompartmentsIDS[Compartments[current_cid]];
        current_compartment_label.text = CompartmentsIDS[Compartments[current_cid]];
        if (use_coroutine) StartCoroutine(populateHexGridFromCenterSpiral());
        else populateGrid();
        //populateHexGridFromCenterSpiral();
    }

    public void Loadcompartment(string cname){
        current_cname = cname;
        current_compartment_label.text = cname;
        var cid = CompartmentsNames[cname];
        current_cid = Compartments.IndexOf(cid);
        if (use_coroutine) StartCoroutine(populateHexGridFromCenterSpiral());
        else populateGrid();
    }

    public void Loadcompartment(int cid){
        current_cname = CompartmentsIDS[cid];
        current_compartment_label.text = CompartmentsIDS[cid];
        current_cid = Compartments.IndexOf(cid);
        if (use_coroutine) StartCoroutine(populateHexGridFromCenterSpiral());
        else populateGrid();
    }


    public void filterHighlightHexGrid(string filter)
    {
        //highlight the one filtered
     
        foreach (var item in hex_instance)
        {
            item.SetActive(false);
        }
      
        foreach (var item in hex_list_instance)
        {
            item.SetActive(false);
        }
        
    }

    public void displayTileCounts()
    { 
        foreach (Transform child in parent.transform)
        {
            if (!(child.childCount == 0))
            {
                var ui_handler = child.GetComponent<toggleLabelButtons>();
                string prefab_name = ui_handler.prefab_name;
                string iname = prefab_name.Split('.')[2];
                GameObject label = ui_handler.label;
                if (prefab_name == null) return;
                if (label == null) return;
                Text label_txt = ui_handler.label_txt;
                if ((Manager.Instance.proteins_count.ContainsKey(prefab_name)))
                {
                    var colors = child.GetComponent<Toggle>().colors;
                    colors.normalColor = Color.yellow;
                    child.GetComponent<Toggle>().colors = colors;
                    label_txt.text = iname + ": " + "(" + Manager.Instance.proteins_count[prefab_name].ToString() + ")";
                    //Debug.Log("The labeltext is: " + label_txt);
                    label.SetActive(true);
                }
                else
                {
                    if (is_gridView) label.SetActive(false);
                    label_txt.text = iname;
                    var colors = child.GetComponent<Toggle>().colors;
                    colors.normalColor = Color.white;
                    child.GetComponent<Toggle>().colors = colors;
                }
            }
        }
    }

    public void filterSelectHexGrid(string filter)
    {
        //only display the one filtered
    }

    private IEnumerator Appear(GameObject instance, float aTime)
    {
        instance.SetActive(true);
        float a = -90.0f;// -Mathf.PI / 2.0f;
        instance.transform.rotation = Quaternion.Euler(0, a, 0);
        //instance.transform.localEulerAngles = new Vector3(0,a , 0);
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            float b = Mathf.Lerp(a, 0.0f, t);
            instance.transform.rotation = Quaternion.Euler(0,b, 0);
            yield return null;
        }
        instance.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void oneHexInstanceStartCount(int count, Point p, int start_id) {
        oneHexInstance(count, p,start_id+count);
    }

    private void oneHexInstance(int count, Point p, int ig_id) {
        GameObject instance;
        toggleLabelButtons instance_props;
        if(is_gridView)
        {
            if (count >= hex_instance.Count)
            {
                instance = GameObject.Instantiate(item_prefab);
                hex_instance.Add(instance);
            }
            else 
            {
                instance = hex_instance[count];
            }
        }
        else
        {
            if (count >= hex_list_instance.Count)
            {
                instance = GameObject.Instantiate(item_list_prefab);
                hex_list_instance.Add(instance);
            }
            else 
            {
                instance = hex_list_instance[count];
            }
        }
            //instance = GameObject.Instantiate(item_prefab);//, getPosOnCircle(currentR), Quaternion.AngleAxis(currentR, Vector3.right)) as GameObject;
            
        instance.GetComponent<Toggle>().group = GetComponent<ToggleGroup>();
        instance.SetActive(false);

        if (!Manager.Instance.ingredients_ids.ContainsKey(ig_id))//  ig_id >= Manager.Instance.ingredients_names.Count)
        {
            //raise exception ?
            Debug.Log(ig_id.ToString()+" is out of bound ");
            return;
        }
        //Debug.Log(ig_id.ToString()+" is out of bound ? ");
        string iname = Manager.Instance.ingredients_ids[ig_id];
        string img_name = Manager.Instance.sprites_names[ig_id];
        if (Manager.Instance.bucketMode)
        {
            instance.GetComponent<Toggle>().group = null;
            if (Manager.Instance.selected_prefab.Contains(iname))
                instance.GetComponent<Toggle>().isOn = true;
            else
                instance.GetComponent<Toggle>().isOn = false;
        }
        else {
            instance.GetComponent<Toggle>().group = GetComponent<ToggleGroup>();
        }
        //setparent?
        instance.transform.SetParent(parent.transform,false);
        //instance.transform.parent = parent.transform;
        instance.transform.localScale = Vector3.one;
        //instance.transform.localPosition = new Vector3((float)p.x, (float)p.y, 0);

        instance_props = instance.GetComponent<toggleLabelButtons>();

        if (is_gridView) {instance_props.label.SetActive(false);}
        else {instance_props.label.SetActive(true);}

        Debug.Log("iname is hexinstance " + iname+" "+ img_name);
        if (!Manager.Instance.prefab_materials.ContainsKey(iname))
        {
            Manager.Instance.prefab_materials.Add(iname, Manager.Instance.createNewSpriteMaterial(iname));
        }
        Material amat = Manager.Instance.prefab_materials[iname];
        //bool change_color = true;

        instance_props.label.GetComponent<Text>().text = iname.Split('.')[2].Replace("_"," ");
        instance_props.prefab_name = iname;
        if (Manager.Instance.all_prefab.ContainsKey(iname) && Manager.Instance.all_prefab[iname].GetComponent<PrefabProperties>().is_surface){
            instance_props.mb_sprite.gameObject.SetActive(true);
        }else if (Manager.Instance.ingredient_node.ContainsKey(iname) && 
                    Manager.Instance.ingredient_node[iname]!= null && 
                    Manager.Instance.ingredient_node[iname].HasKey("surface") && 
                    Manager.Instance.ingredient_node[iname]["surface"].Value == "surface") 
        {
            instance_props.mb_sprite.gameObject.SetActive(true);
        } 
        else {
            instance_props.mb_sprite.gameObject.SetActive(false);
        }

        if (iname == "Membrane") {
            iname = "Membrane";
            //change_color = false;
        }
        if (iname.Contains("DNA") && iname.Contains("Draw")) { 
            iname = "DNA_full_Turn";
            img_name = "DNA_full_Turn";
            //change_color = false;
        }
        //var prefabProps = myPrefab.GetComponent<PrefabProperties>();
        //Debug.Log(myPrefab + " is item prefab");
        //string imagePath = Application.dataPath + "/Textures/Recipie/" + iname + ".png";
        //D/ebug.Log(imagePath);
        //Sprite sprite = Helper.LoadNewSprite(imagePath);

        //Texture2D SpriteTexture = Resources.Load<Texture2D>("Recipie/" + name);
        //Sprite sprite = new Sprite();
        //if (SpriteTexture)
        //    sprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0),  100.0f);
        //var sprite = Resources.Load<Sprite>("Recipie/" + Path.GetFileNameWithoutExtension(img_name));//remove extension. if doesnt exist here check in Data folder
        var sprite = Manager.Instance.GetSprite(img_name);
        if (sprite)
        {
            //Debug.Log(iname);
            instance_props.protein_sprite.sprite = sprite;
            float ratio = (float)instance_props.protein_sprite.sprite.texture.width / (float)instance_props.protein_sprite.sprite.texture.height;
            float s;
            if (is_gridView)
            {
                s = 80.0f;
            }
            else
            {
                s = 30.0f;
            }
            //float s = 80;
            float h = (s / ratio);
            if (h > s) instance_props.protein_sprite.rectTransform.sizeDelta = new Vector2((int)(s / ratio), s);
            else  instance_props.protein_sprite.rectTransform.sizeDelta = new Vector2(s, (int)(s / ratio));
            instance_props.protein_sprite.material = amat;//;
        }
        else {
            //build one ?
            //take a random one ?
            //Debug.Log("cant load sprite " + imagePath+" for "+iname.ToString() );
            //check if in the data folder else build it.
        }
        StartCoroutine(Appear(instance, 0.25f));
    }

    public void populateHexGrid() {
        var cid = Compartments[current_cid];
        //clean the prefab or just change the image
        hex_grid.Clear();
        
        foreach (var item in hex_instance)  
        {
            item.SetActive(false);
        }
        
        foreach (var item in hex_list_instance)  
        {
            item.SetActive(false);
        }
        
        Layout flat = new Layout(Layout.flat, new Point(55, 55), new Point(0, 0));
        int count = 0;
        //int start_id = (int)CompartmentsIngredients[current_compartments].x;
        //int total_counts = (int)CompartmentsIngredients[current_compartments].y;
        int total_counts = CompartmentsIngredients_ids[cid].Count;
        //Debug.Log("populate " + start_id.ToString() + " " + total_counts.ToString());
       
        //this layout start on the left-bottom and go up and on the right.
        //try to fin a layout that start from the middle and go around.
        for (int q = -map_radius; q <= map_radius; q++)
        {
            int r1 = Mathf.Max(-map_radius, -q - map_radius);
            int r2 = Mathf.Min(map_radius, -q + map_radius);
            for (int r = r1; r <= r2; r++)
            {
                Hex h = new Hex(q, r, -q - r);
                hex_grid.Add(h);
                Point p = Layout.HexToPixel(flat, h);
                oneHexInstance(count,  p, CompartmentsIngredients_ids[cid][count]);
                count++;
                if (count >= total_counts) return;
            }
        }
    }

    void populateGrid()
    {
        var cid = Compartments[current_cid];
        Debug.Log("populateHexGridFromCenterSpiral "+cid.ToString());
        Debug.Log("populateHexGridFromCenterSpiral "+CompartmentsIDS[cid]);
        Debug.Log("populateHexGridFromCenterSpiral "+CompartmentsIngredients_ids[cid].Count.ToString());
        //clean the prefab or just change the image
        hex_grid.Clear();
      
        foreach (var item in hex_instance)
        {
            item.SetActive(false);
            if (item.GetComponent<Toggle>())
            {
                var colors = item.GetComponent<Toggle>().colors;
                colors.normalColor = Color.white;
                item.GetComponent<Toggle>().colors = colors;
            }
            if (item.GetComponent<Text>()) item.SetActive(false);
        }
        
        foreach (var item in hex_list_instance)
        {
            item.SetActive(false);
            if (item.GetComponent<Toggle>())
            {
                var colors = item.GetComponent<Toggle>().colors;
                colors.normalColor = Color.white;
                item.GetComponent<Toggle>().colors = colors;
            }
            if (item.GetComponent<Text>()) item.SetActive(false);
        }
        
        //layout is the orientation of the hex
        Layout flat = new Layout(Layout.flat, new Point(55, 55), new Point(0, 0));
        
        //int start_id = (int)CompartmentsIngredients[current_compartments].x;
        //int total_counts = (int)CompartmentsIngredients[current_compartments].y; //total number of sprite to display
        int total_counts = CompartmentsIngredients_ids[cid].Count;
        //Debug.Log("populate " + start_id.ToString() + " " + total_counts.ToString());
        //get the Hex in spiral
        Hex center = new Hex(0, 0, 0);
        hex_grid = Hex.Spiral(center, map_radius);
        //this layout start on the left-bottom and go up and on the right.
        //try to fin a layout that start from the middle and go around.
        //map radius is the radius of the grid
        //use cubic coordinates
        for (int i = 0; i < total_counts; i++) {
            Point p = Layout.HexToPixel(flat, hex_grid[i]);
            oneHexInstance(i, p, CompartmentsIngredients_ids[cid][i]);
            //wait for animation ?
            //yield return new WaitForSeconds(0.05f);
        }
        displayTileCounts();
    }

    IEnumerator populateHexGridFromCenterSpiral()
    {
        var cid = Compartments[current_cid];
        //Debug.Log("populateHexGridFromCenterSpiral "+cid.ToString());
        //Debug.Log("populateHexGridFromCenterSpiral "+CompartmentsIDS[cid]);
        //Debug.Log("populateHexGridFromCenterSpiral "+CompartmentsIngredients_ids[cid].Count.ToString());
        //clean the prefab or just change the image
        hex_grid.Clear();

        foreach (var item in hex_instance)
        {
            item.SetActive(false);
            if (item.GetComponent<Toggle>())
            {
                var colors = item.GetComponent<Toggle>().colors;
                colors.normalColor = Color.white;
                item.GetComponent<Toggle>().colors = colors;
            }
            if (item.GetComponent<Text>()) item.SetActive(false);
        }
        
        foreach (var item in hex_list_instance)
        {
            item.SetActive(false);
            if (item.GetComponent<Toggle>())
            {
                var colors = item.GetComponent<Toggle>().colors;
                colors.normalColor = Color.white;
                item.GetComponent<Toggle>().colors = colors;
            }
            if (item.GetComponent<Text>()) item.SetActive(true);
        }
        
        //layout is the orientation of the hex
        Layout flat = new Layout(Layout.flat, new Point(55, 55), new Point(0, 0));
        
        //int start_id = (int)CompartmentsIngredients[current_compartments].x;
        //int total_counts = (int)CompartmentsIngredients[current_compartments].y; //total number of sprite to display
        int total_counts = CompartmentsIngredients_ids[cid].Count;
         //get the Hex in spiral
        Hex center = new Hex(0, 0, 0);
        hex_grid = Hex.Spiral(center, map_radius);
        //this layout start on the left-bottom and go up and on the right.
        //try to fin a layout that start from the middle and go around.
        //map radius is the radius of the grid
        //use cubic coordinates
        for (int i = 0; i < total_counts; i++) {
            Point p = Layout.HexToPixel(flat, hex_grid[i]);
            oneHexInstance(i, p, CompartmentsIngredients_ids[cid][i]);
            //wait for animation ?
            yield return new WaitForSeconds(0.05f);
        }
        displayTileCounts();
    }


    //this where we handle the search bar
    //case insensitive
    private void PruneItemsLinq(string currText)
    {
        currText = currText.ToLower();
        //dont know how to get the index instead of the value
        var Selection = Manager.Instance.ingredients_ids.Where(x => x.Value.ToLower().Contains(currText)).ToArray();
    }
    //Updated to not use Linq
    private void PruneItemsArray(string currText)
    {
        string _currText = currText.ToLower();
        selection.Clear();
        //search for compartment then search for ingredient ?
        //for (int i = Manager.Instance.ingredients_names.Count - 1; i >= 0; i--)
        foreach(var KeyValue in Manager.Instance.ingredients_names)
        {
            string _item = KeyValue.Key.ToLower();//Manager.Instance.ingredients_names[i].ToLower();
            if (_item.Contains(_currText))
            {
                selection.Add(KeyValue.Value);
            }
        }
        if (use_coroutine) StartCoroutine(populateHexGridFromCenterSpiralPrune(selection));
        else populateGridPrune(selection);
    }
    
    IEnumerator populateHexGridFromCenterSpiralPrune(List<int> aselection)
    {
        //clean the prefab or just change the image
        //if (hex_grid == null) return;
        if (hex_grid !=null) {hex_grid.Clear();}
        //if (item_add_prefab_instance != null) { item_add_prefab_instance.SetActive(false); }

        foreach (var item in hex_instance)
        {
            item.SetActive(false);
            if (item.GetComponent<Toggle>())
            {
                var colors = item.GetComponent<Toggle>().colors;
                colors.normalColor = Color.white;
                item.GetComponent<Toggle>().colors = colors;
            }
            if (item.GetComponent<Text>()) item.SetActive(false);
        }
        
        foreach (var item in hex_list_instance)
        {
            item.SetActive(false);
            if (item.GetComponent<Toggle>())
            {
                var colors = item.GetComponent<Toggle>().colors;
                colors.normalColor = Color.white;
                item.GetComponent<Toggle>().colors = colors;
            }
            if (item.GetComponent<Text>()) item.SetActive(false);      
        }
        //layout is the orientation of the hex
        Layout flat = new Layout(Layout.flat, new Point(55, 55), new Point(0, 0));

        int start_id = 0;// (int)CompartmentsIngredients[current_compartments].x;
        //int total_counts = (int)CompartmentsIngredients[current_compartments].y; //total number of sprite to display
        int total_counts = aselection.Count;
        //get the Hex in spiral
        Hex center = new Hex(0, 0, 0);
        hex_grid = Hex.Spiral(center, map_radius);
        //this layout start on the left-bottom and go up and on the right.
        //try to fin a layout that start from the middle and go around.
        //map radius is the radius of the grid
        //use cubic coordinates
        for (int i = 0; i < total_counts; i++)
        {
            Point p = Layout.HexToPixel(flat, hex_grid[i]);
            start_id = aselection[i];
            oneHexInstance(i, p, start_id);// i, p, start_id);
            //wait for animation ?
            yield return new WaitForSeconds(0.05f);
        }
        displayTileCounts();
    }

    void populateGridPrune(List<int> aselection)
    {
        var cid = Compartments[current_cid];
        //clean the prefab or just change the image
        if (hex_grid !=null) {hex_grid.Clear();}
      
        foreach (var item in hex_instance)
        {
            item.SetActive(false);
            if (item.GetComponent<Toggle>())
            {
                var colors = item.GetComponent<Toggle>().colors;
                colors.normalColor = Color.white;
                item.GetComponent<Toggle>().colors = colors;
            }
            if (item.GetComponent<Text>()) item.SetActive(false);
        }
        
        foreach (var item in hex_list_instance)
        {
            item.SetActive(false);
            if (item.GetComponent<Toggle>())
            {
                var colors = item.GetComponent<Toggle>().colors;
                colors.normalColor = Color.white;
                item.GetComponent<Toggle>().colors = colors;
            }
            if (item.GetComponent<Text>()) item.SetActive(false);
        }
        
        //layout is the orientation of the hex
        Layout flat = new Layout(Layout.flat, new Point(55, 55), new Point(0, 0));
        
        int start_id = 0;
        int total_counts  = aselection.Count;
        //Debug.Log("populate " + start_id.ToString() + " " + total_counts.ToString());
        //get the Hex in spiral
        Hex center = new Hex(0, 0, 0);
        hex_grid = Hex.Spiral(center, map_radius);
        //this layout start on the left-bottom and go up and on the right.
        //try to fin a layout that start from the middle and go around.
        //map radius is the radius of the grid
        //use cubic coordinates
        for (int i = 0; i < total_counts; i++) {
            Point p = Layout.HexToPixel(flat, hex_grid[i]);
            start_id = aselection[i];
            oneHexInstance(i, p, start_id);
        }
        displayTileCounts();
    }

    //the callback
    public void OnSearchValidated(string input_query) {
        //filter and change the palette showing only the current selection
        current_selection = input_query;
        if (input_query!="") PruneItemsArray(input_query);
        else {
            if (use_coroutine) StartCoroutine(populateHexGridFromCenterSpiral());
            else populateGrid();
        }
    }

    public void ToggleMergeUponLoading(bool value){
        merge_upon_loading = value;
    }

    public string createIngredientStringJson(string img_name, bool is_surface, bool is_fiber, float scale2d,float yoffset){
        var astring =@"{
                'Type': 'MultiSphere',
                'surface' : 'false',
                'sprite': {
                    'image': 'TF_Cg.png',
                    'offsety': 0,
                    'scale2d': 2.85
                }
            }";
        astring = astring.Replace("false", is_surface.ToString());
        astring = astring.Replace("0", yoffset.ToString());
        astring = astring.Replace("2.85", scale2d.ToString());
        astring = astring.Replace("TF_Cg.png", img_name);
        if (is_fiber) {
            astring.Replace("MultiSphere", "Grow");
        }
        astring = "{\"Type\":\"" + ((is_fiber)?"Grow":"MultiSphere")+"\",";
        astring += "\"surface\":\""+ ((is_surface)?"true":"false")+"\",";
        astring += "\"sprite\":{";
        astring += "\"image\":\""+img_name+"\",";
        astring += "\"offsety\":\""+yoffset.ToString()+"\",";
        astring += "\"scale2d\":\""+scale2d.ToString()+"\"";
        astring += "}}";
        return JSONNode.Parse(astring);
    }

    public void AddOneIngredient(string iname, string img_name, float ascale2d,float yoffset,float fiber_length, 
                                 bool is_surface=false, bool is_fiber = false, string compname ="") {
        Manager.Instance.update_texture = true;
        var name = iname;
        if (compname == "") {
            Debug.Log(current_cid);
            Debug.Log(Compartments[current_cid]);
            Debug.Log(CompartmentsIDS[Compartments[current_cid]]);
            compname = CompartmentsIDS[Compartments[current_cid]];//first
        }
        int comp = 0;
        if (CompartmentsNames.ContainsKey(compname)){
            comp = CompartmentsNames[compname];
        }
        else {
            Debug.Log(compname+" not in the list of compartments");
            foreach(var KeyValue in CompartmentsNames ){
                Debug.Log(KeyValue.Key+" "+KeyValue.Value.ToString());
            }
            compname = CompartmentsIDS[Compartments[current_cid]];//first
            comp = CompartmentsNames[compname];
        }
        string prefix = (is_surface)? "surface" : "interior";
        name = compname+"."+prefix+"."+name;
        Debug.Log("Add one ingredient "+iname+" "+img_name+" "+compname);
        /*
        var myPrefab = Manager.Instance.GetaPrefab(name, PDBid);
        Build the sprites from PDB and Illustrates
        save the sprites in images with name iname
        */
        if (Manager.Instance.all_prefab.ContainsKey(name)) {
            //change name
            name = name+"_user";
        }
        //string compname = comp;
        

        //compname = CompartmentsIDS[Compartments[current_cid]];//Compartments[comp];
        int n = random_uid.Next();//int n = Manager.Instance.ingredients_names.Count;
        CompartmentsIngredients_ids[comp].Add(n); 
        Manager.Instance.ingredients_names.Add(name,n);
        Manager.Instance.ingredients_ids.Add(n,name);
        Manager.Instance.sprites_names.Add(n,img_name);
        Manager.Instance.additional_ingredients_names.Add(name);
        /*create a JSON NODE */
        //JSONNode node = createIngredientStringJson(img_name, is_surface, is_fiber, ascale2d, yoffset);
        //Manager.Instance.ingredient_node.Add(name, node);
        var myPrefab = Manager.Instance.Build(name,img_name);
        myPrefab.SetActive(true);
        Manager.Instance.all_prefab.Add(name, myPrefab);

        if (myPrefab)
        {
            PrefabProperties p = myPrefab.GetComponent<PrefabProperties>();
            p.compartment = compname;
            p.setuped = false;
            p.SetupFromValues(is_surface, is_fiber, ascale2d, yoffset, fiber_length, true);
            if (p.is_fiber)
            {
                p.persistence_length = 3;
            }            
            Debug.Log("SetupedFromValues "+p.scale2d.ToString()+" "+p.y_offset.ToString());
            //need to setup the collider without the node information
        }
        //oneHexInstance(hex_instance.Count + 1, new Point(), Manager.Instance.ingredients_names.Count - 1);
        //loadNextCompartments();
        if (use_coroutine) StartCoroutine(populateHexGridFromCenterSpiral());
        else populateGrid();
    }

    public void AddOneGroup(string iname = null, int comp = -1, string compname = "Custom")
    {
        Manager.Instance.update_texture = true;
        string name = iname;
        if (comp == -1) {
            comp = Compartments[current_cid];
        }
        //add one tile
        //comp = customCompIndice;
        
        int n = random_uid.Next();//int n = Manager.Instance.ingredients_names.Count;
        CompartmentsIngredients_ids[comp].Add(n); 
        Manager.Instance.ingredients_names.Add(name,n);
        Manager.Instance.ingredients_ids.Add(n,name);
        Manager.Instance.sprites_names.Add(n,"grapes");
        if (use_coroutine) StartCoroutine(populateHexGridFromCenterSpiral());
        else populateGrid();
    }

    public void AddOneCompartment(string cname, bool update_ui=true) {
        var name = cname+".surface."+cname+"_membrane";
        if (CompartmentsIDS.ContainsValue(cname)&&CompartmentsNames.ContainsKey(cname)) {
            Debug.Log(cname+" already exist ?");
            Debug.Log(CompartmentsNames[cname]);
            //doest he membrane ingrdient exist ?
            if (!Manager.Instance.all_prefab.ContainsKey(name)){
                AddRecipeIngredientMembrane(CompartmentsNames[cname],name);
            }
            return;
        }
        int nCompartemnts = random_uid.Next();// Compartments.Count;
        if (Compartments.Contains(nCompartemnts)) nCompartemnts++;
        Compartments.Add(nCompartemnts);
        CompartmentsIDS.Add(nCompartemnts,cname);
        CompartmentsNames.Add(cname,nCompartemnts);
        CompartmentsIngredients_ids.Add(nCompartemnts, new List<int>());
        Manager.Instance.additional_compartments_names.Add(cname);
        Manager.Instance.update_texture = true;
        Debug.Log(cname+" has been added, "+name);
        AddRecipeIngredientMembrane(nCompartemnts,name);//"Cell_Membrane"
        if (update_ui) {
            Loadcompartment(nCompartemnts);
            //loadNextCompartments();
            //activate the membrane
            Manager.Instance.SwitchPrefabFromName(name);
            var prefab = Manager.Instance.all_prefab[name];
            Manager.Instance.changeDescription(prefab, prefab.GetComponent<SpriteRenderer>());
            if(is_gridView)
            {
                hex_instance[0].GetComponent<Toggle>().isOn = true; 
            }
            else
            {
                hex_list_instance[0].GetComponent<Toggle>().isOn = true;
            }           
        }
    }

    public void switchToGridView()
    {
        if (is_gridView) return;
        //Switch layout group to grid and set new settings.

        //First Destroy the current layout group
        is_gridView = true;
        VerticalLayoutGroup vertLayout = parent.transform.GetComponent<VerticalLayoutGroup>();
        DestroyImmediate(vertLayout);

        GridLayoutGroup gridLayout = parent.transform.gameObject.AddComponent<GridLayoutGroup>() as GridLayoutGroup;

        gridLayout.cellSize = new Vector2 (95, 95);
        gridLayout.spacing = new Vector2 (0,0);

        //Now get children and swap them for the grid prefab.
        var cid = Compartments[current_cid];
        int ingredientCount = CompartmentsIngredients_ids[cid].Count();

        Layout flat = new Layout(Layout.flat, new Point(55, 55), new Point(0, 0));

        Transform[] items_Container = parent.transform.GetComponentsInChildren<Transform>();

        /*foreach (Transform child in items_Container)
        {

            child.gameObject.SetActive(false);
        }*/

        foreach (var item in hex_list_instance)
        {
            item.SetActive(false);
            if (item.GetComponent<Toggle>())
            {
                var colors = item.GetComponent<Toggle>().colors;
                colors.normalColor = Color.white;
                item.GetComponent<Toggle>().colors = colors;
            }
            if (item.GetComponent<Text>()) item.SetActive(false);      
        }
        if (current_selection != "" && selection.Count != 0){
            ingredientCount = selection.Count;
        }
        for (int i =0; i <= ingredientCount-1;i++)
        {
            var igid = CompartmentsIngredients_ids[cid][i];
            if (current_selection != "" && selection.Count != 0){
                igid = selection[i];
            }
            Point p = Layout.HexToPixel(flat, hex_grid[i]);
            oneHexInstance(i, p, igid);
        }
    }

    public void switchToListView()
    {
        if (!is_gridView) return;
        //Switch layout group to vertical and set new settings.
        is_gridView = false;
        GridLayoutGroup gridLayout = parent.transform.GetComponent<GridLayoutGroup>();
        DestroyImmediate(gridLayout);

        VerticalLayoutGroup vertLayout = parent.transform.gameObject.AddComponent<VerticalLayoutGroup>() as VerticalLayoutGroup;
        vertLayout.spacing = 40.0f;
        vertLayout.padding = new RectOffset (5,5,25,40);
        vertLayout.childControlHeight  = true;
        vertLayout.childControlWidth = false;
        vertLayout.childForceExpandHeight = false;
        vertLayout.childForceExpandWidth = false;

        //Now get children and swap them for the list prefab.
        var cid = Compartments[current_cid];
        int ingredientCount = CompartmentsIngredients_ids[cid].Count();

        //Parse over all children objects and check for inactive item prefabs
        Layout flat = new Layout(Layout.flat, new Point(55, 55), new Point(0, 0));

        Transform[] items_Container = parent.transform.GetComponentsInChildren<Transform>();

        /*foreach (Transform child in items_Container)
        {
            child.gameObject.SetActive(false);
        }*/
        foreach (var item in hex_instance)
        {
            item.SetActive(false);
            if (item.GetComponent<Toggle>())
            {
                var colors = item.GetComponent<Toggle>().colors;
                colors.normalColor = Color.white;
                item.GetComponent<Toggle>().colors = colors;
            }
            if (item.GetComponent<Text>()) item.SetActive(false);      
        }
        
        if (current_selection != "" && selection.Count != 0){
            ingredientCount = selection.Count;
        }
        for (int i =0; i <= ingredientCount-1;i++)
        {
            var igid = CompartmentsIngredients_ids[cid][i];
            if (current_selection != "" && selection.Count != 0){
                igid = selection[i];
            }
            Point p = Layout.HexToPixel(flat, hex_grid[i]);
            oneHexInstance(i, p, igid);
        }
    }



    public void RemoveIngredient(string ing_name,int cid=-1, bool update_ui=true){
        Debug.Log("RemoveIngredient "+ing_name);
        if (cid == -1) {
            //find the compartment
            for (var i =0; i < Compartments.Count;i++){
                var ncid = Compartments[i];
                foreach(var id in CompartmentsIngredients_ids[ncid]) {
                    if (Manager.Instance.ingredients_ids[id] == ing_name) {
                        cid = ncid;
                        break;
                    }
                }
            }
        }
        //delete all instance
        //need to deal with ghost and group
        GhostManager.Get.RemoveIngredientFromGhosts(ing_name);
        if (!GroupManager.Get.groupnames.Contains(ing_name))
            GroupManager.Get.RemoveIngredientFromGroups(ing_name);
        Manager.Instance.DestroyHierarchyPrefabFamily(ing_name);
        int ing_id = Manager.Instance.ingredients_names[ing_name];
        //only remove from UI?
        CompartmentsIngredients_ids[cid].Remove(ing_id);
        if (Manager.Instance.additional_ingredients_names.Contains(ing_name)){
            Manager.Instance.additional_ingredients_names.Remove(ing_name);
        }
        Manager.Instance.ingredients_names.Remove(ing_name);
        Manager.Instance.ingredients_ids.Remove(ing_id);
        if (Manager.Instance.all_prefab.ContainsKey(ing_name))
        {
            if (Manager.Instance.all_prefab[ing_name]!=null)
            {
                var prefab = Manager.Instance.all_prefab[ing_name];
                GameObject.Destroy(prefab);
            }
            Manager.Instance.all_prefab.Remove(ing_name);
        }
        Manager.Instance.ingredient_node.Remove(ing_name);
        Manager.Instance.sprites_names.Remove(ing_id);
        if (GroupManager.Get.groupnames.Contains(ing_name))
        {
            GroupManager.Get.groupnames.Remove(ing_name);
        }
        if (Manager.Instance.myPrefab!=null && Manager.Instance.myPrefab.name.Replace("(Clone)","").StartsWith(ing_name))
        {
            GameObject.Destroy(Manager.Instance.myPrefab);
            Manager.Instance.myPrefab = null;
        }     
        if (Manager.Instance.current_prefab!=null && Manager.Instance.current_prefab.name.Replace("(Clone)","").StartsWith(ing_name))
        {
            GameObject.Destroy(Manager.Instance.current_prefab);
            Manager.Instance.current_prefab = null;
        }     
        if (update_ui) {
            //update the canvas
            Loadcompartment(current_cname);
            //current_cid = current_cid-1;
            //loadNextCompartments();  
        }
    }

    public void RemoveGroup(string gname,int cid=-1){
        RemoveIngredient(gname,cid);
    }   

    public void RemoveCompartment(string cname){
        Debug.Log("removecompartment "+cname);
        int cid = CompartmentsNames[cname];
        Debug.Log("removecompartment "+cid.ToString());
        //remove all ingredient from current compartment
        List<int> toremove = new List<int>(CompartmentsIngredients_ids[cid]);
        Debug.Log("toremove "+toremove.Count.ToString());
        foreach(var id in toremove) {
           //remove id 
           RemoveIngredient(Manager.Instance.ingredients_ids[id],cid,false);
        }
        toremove.Clear();
        CompartmentsIngredients_ids[cid].Clear();
        Compartments.Remove(cid);
        CompartmentsIDS.Remove(cid);
        CompartmentsNames.Remove(cname);
        CompartmentsIngredients_ids.Remove(cid);
        if (Manager.Instance.additional_compartments_names.Contains(cname)){
            Manager.Instance.additional_compartments_names.Remove(cname);
        }
        current_cid = -1;
        loadNextCompartments(); 
    }

    public string GetCurrentCname(){
        return CompartmentsIDS[Compartments[current_cid]];
    }
    public void DeleteCurrentComp(){
        //delete all instance
        //Manager.Instance.DestroyHierarchyFamily(prefab_name);
        //delete in recipe
        RemoveCompartment(CompartmentsIDS[Compartments[current_cid]]);
        delete_panel.SetActive(false);
        loadNextCompartments(); 
    }

    public void CancelDeleteCurrentComp(){
        delete_panel.SetActive(false);
    }

    public void RemoveCompartment_cb (BaseEventData eventData) {
        Debug.Log("OnPointerClick called.");
        var cname = CompartmentsIDS[Compartments[current_cid]];
        if (!Manager.Instance.additional_compartments_names.Contains(cname))
            return;
        PointerEventData ev = eventData as PointerEventData;
        if (ev.button == PointerEventData.InputButton.Right){
            //pop delete/cancel button window
            delete_panel.transform.GetChild(0).GetComponent<Text>().text = "Delete the Compartment named : "+cname+" ?";
            delete_panel.SetActive(true);
        }
     }
}
