using UnityEngine;
//using UnityEditor;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using HexGrid;


//[ExecuteInEditMode]
public class RecipeUI : MonoBehaviour {
    public TextAsset recipe;
    public TextAsset ingredients_description;
    public GameObject item_prefab;
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
    private GameObject item;

    private float last_radius;
    private int last_nbitems;
    private List<Hex> hex_grid;
    private List<GameObject> hex_instance;
    public List<string> Compartments;
    public bool use_coroutine=false;
    public int current_compartments = 0;
    private Dictionary<int, Vector2> CompartmentsIngredients;
    private Dictionary<int, List<int>> CompartmentsIngredients_ids;//migrate to keep track of ingredients id to support merge
    private bool isGrouped = true; ////3/17/17
    public List<int> selection = new List<int>();
    public bool merge_upon_loading = false;

    void OnEnable() {
        Manager.Instance.recipeUI = this;
        Manager.Instance.ingredient_node = new Dictionary<string, JSONNode>();
        Compartments = new List<string>();
        CompartmentsIngredients = new Dictionary<int, Vector2>();
        CompartmentsIngredients_ids = new Dictionary<int, List<int>>();
        hex_instance = new List<GameObject>();
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
        int total_counts = CompartmentsIngredients_ids[current_compartments].Count;
        for (int count = 0; count < total_counts; count++)
        {
            var ig_id = CompartmentsIngredients_ids[current_compartments][count];
            if (ig_id < Manager.Instance.ingredients_names.Count)
            {
                string iname = Manager.Instance.ingredients_names[ig_id];

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
        CompartmentsIngredients.Clear();
        CompartmentsIngredients_ids.Clear();
        //Manager.Instance.ingredients_prefab.Clear();
        Manager.Instance.all_prefab.Clear();
        Manager.Instance.ingredients_names.Clear();
        Manager.Instance.additional_ingredients_names.Clear();
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
        CompartmentsIngredients.Clear();
        CompartmentsIngredients_ids.Clear();
        //Manager.Instance.ingredients_prefab.Clear();
        //Destroy all prefab ?
        foreach (var KeyVal in Manager.Instance.all_prefab)
        {
            GameObject.Destroy(KeyVal.Value);
        }
        Manager.Instance.all_prefab.Clear();
        Manager.Instance.ingredients_names.Clear();
        Manager.Instance.additional_ingredients_names.Clear();
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
            PdbLoader.DataDirectories.Add(Path.GetDirectoryName(filename));
            Manager.Instance.AddUserDirectory(Path.GetDirectoryName(filename));
            resultData = JSONNode.Parse(File.ReadAllText(filename)); //JSONNode.LoadFromFile(filename);
        }
        LoadRecipe_cb(resultData);
    }

    public void LoadRecipe_cb(JSONNode resultData) {
        
        int nCompartemnts = 0;
        int nIngredients = 0;
        int nC = 0;
        if (resultData["cytoplasme"] != null)
        {
            Compartments.Add("Blood Plasma");
            nIngredients += resultData["cytoplasme"]["ingredients"].Count;
            //CompartmentsIngredients.Add(nC, new Vector2(0, nIngredients));
            CompartmentsIngredients_ids.Add(nC, new List<int>());
            Debug.Log("found 0 " + nIngredients.ToString());
            nCompartemnts += 1;
            nC++;
        }
        //for each compartment should add a Cell_Membrane
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            Compartments.Add(resultData["compartments"].GetKey(i));
            int total = resultData["compartments"][i]["interior"]["ingredients"].Count +
                resultData["compartments"][i]["surface"]["ingredients"].Count + 1;
            //CompartmentsIngredients.Add(nC, new Vector2(nIngredients, total));
            CompartmentsIngredients_ids.Add(nC, new List<int>());
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
            AddRecipeIngredients(nC, resultData["cytoplasme"]["ingredients"], "cytoplasme",false);
            nC++;
        }

        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            //add the membrane
            //SceneManager.Instance.ingredients_names.Add("DrawMembrane");
            AddRecipeIngredientMembrane(nC, resultData["compartments"][i]);//"Cell_Membrane"
            AddRecipeIngredients(nC, resultData["compartments"][i]["surface"]["ingredients"], "surface" + i.ToString(),true);
            AddRecipeIngredients(nC, resultData["compartments"][i]["interior"]["ingredients"], "interior" + i.ToString(),false);
            nC++;
        }
        //this is comment as I manually change a lot of name of the recipe to a better reading for the NSF competition. 
        //buildHierarchy (resultData);
        current_compartments = 0;
        loadNextCompartments();
        //load first ingredient of first compartments
        int ingid = CompartmentsIngredients_ids[current_compartments][0];
        string iname = Manager.Instance.ingredients_names[ingid];
        Manager.Instance.SwitchPrefabFromName(iname);
        var prefab = Manager.Instance.all_prefab[iname];
        Manager.Instance.changeDescription(prefab, prefab.GetComponent<SpriteRenderer>());
        hex_instance[0].GetComponent<Toggle>().isOn = true;
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
            PdbLoader.DataDirectories.Add(Path.GetDirectoryName(filename));
            Manager.Instance.AddUserDirectory(Path.GetDirectoryName(filename));
            resultData = JSONNode.Parse(File.ReadAllText(filename)); //JSONNode.LoadFromFile(filename);
        }
        MergeRecipe_cb(resultData);
    }

    public void MergeRecipe_cb(JSONNode resultData)
    {
        int nCompartemnts = Compartments.Count;
        int nIngredients = Manager.Instance.ingredients_names.Count;
        int nC = nCompartemnts;
        //for each compartment should add a Cell_Membrane
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            if (Compartments.Contains(resultData["compartments"].GetKey(i))) continue;
            Compartments.Add(resultData["compartments"].GetKey(i));
            //CompartmentsIngredients.Add(nC, new Vector2(nIngredients, total));
            CompartmentsIngredients_ids.Add(nC, new List<int>());
            nC++;
        }
        DateTime start = DateTime.Now;
        if (resultData["cytoplasme"] != null)
        {
            nC =  Compartments.IndexOf("Blood Plasma");
            AddRecipeIngredients(nC,resultData["cytoplasme"]["ingredients"], "cytoplasme",false);
        }
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            //add the membrane
            //SceneManager.Instance.ingredients_names.Add("DrawMembrane");
            nC =  Compartments.IndexOf(resultData["compartments"].GetKey(i));
            if (!Manager.Instance.ingredients_names.Contains(resultData["compartments"][i]["name"].Value)) 
            {
                AddRecipeIngredientMembrane(nC,resultData["compartments"][i]);//"Cell_Membrane"
            }
            AddRecipeIngredients(nC,resultData["compartments"][i]["surface"]["ingredients"], "surface" + i.ToString(),true);
            AddRecipeIngredients(nC,resultData["compartments"][i]["interior"]["ingredients"], "interior" + i.ToString(),false);
        }
        //this is comment as I manually change a lot of name of the recipe to a better reading for the NSF competition. 
        //buildHierarchy (resultData);
        current_compartments = 0;
        loadNextCompartments();
        //load first ingredient of first compartments
        int ingid = CompartmentsIngredients_ids[current_compartments][0];
        string iname = Manager.Instance.ingredients_names[ingid];
        Manager.Instance.SwitchPrefabFromName(iname);
        var prefab = Manager.Instance.all_prefab[iname];
        Manager.Instance.changeDescription(prefab, prefab.GetComponent<SpriteRenderer>());
        hex_instance[0].GetComponent<Toggle>().isOn = true;
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
            if (Manager.Instance.ingredients_names.Contains( iname )) continue;
            AddProteinIngredient(cid, recipeDictionary[j], prefix,surface);
            var ig_id = Manager.Instance.ingredients_names.IndexOf( iname );
            if (ig_id == -1) {
                Debug.Log("didnt found "+iname);
            }
            CompartmentsIngredients_ids[cid].Add(ig_id);
        }
    }

    public void AddRecipeIngredientMembrane(int cid, JSONNode compDictionary)
    {
        
        string iname = compDictionary["name"].Value;
        string prefab_name = "Cell_Membrane";
        Debug.Log("AddRecipeIngredientMembrane "+iname);
        CompartmentsIngredients_ids[cid].Add(Manager.Instance.ingredients_names.Count);
        Manager.Instance.ingredient_node.Add(iname, compDictionary);
        Manager.Instance.ingredients_names.Add(iname);
        Manager.Instance.sprites_names.Add(prefab_name);
        if (!Manager.Instance.prefab_materials.ContainsKey(iname))
        {
            Material mat = Manager.Instance.createNewSpriteMaterial(iname);
            //parse the color that is in the dictionary
            //if (compDictionary.HasKey("color")) Debug.Log(compDictionary["color"].Value);
            if (compDictionary.HasKey("color")) {
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
    public static void AddProteinIngredient(int cid, JSONNode ingredientDictionary, string prefix, bool surface)
    {
        string iname = ingredientDictionary["name"].Value;
        string img_name = iname;
        if (iname.Contains("DNA")) {
            iname = "Draw DNA_"+cid.ToString();
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
        JSONClass tmp = new JSONClass();
        ingredientDictionary.Add("surface", (surface)?"surface":"interior");
        Debug.Log("add to dictionary");
        Debug.Log(ingredientDictionary["surface"]);
        Debug.Log(ingredientDictionary["surface"].Value);
        Manager.Instance.ingredient_node.Add(iname, ingredientDictionary);
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
        var name = prefix + "_" + iname;
        var biomt = (bool)ingredientDictionary["source"]["biomt"].AsBool;
        var center = (bool)ingredientDictionary["source"]["transform"]["center"].AsBool;
        var pdbName = ingredientDictionary["source"]["pdb"].Value.Replace(".pdb", "");
        Debug.Log("iname is " + iname);
        Manager.Instance.ingredients_names.Add(iname);
        Manager.Instance.sprites_names.Add(img_name);
        //first build the sprite if doesnt exist
        if (!Manager.Instance.prefab_materials.ContainsKey(iname))
        {
            Material mat = Manager.Instance.createNewSpriteMaterial(iname);
            //parse the color that is in the dictionary
            if (ingredientDictionary.HasKey("color")) {
                Debug.Log("color is for "+ iname);
                mat.color = new Color(  ingredientDictionary["color"].AsArray[0].AsFloat,
                                        ingredientDictionary["color"].AsArray[1].AsFloat,
                                        ingredientDictionary["color"].AsArray[2].AsFloat);
                Debug.Log(mat.color);
            }
            Manager.Instance.prefab_materials.Add(iname, mat);
        }
        /*
        GameObject myPrefab;// = Resources.Load("Prefabs/" + iname) as GameObject;
        //Load the prefab
        if (!Manager.Instance.all_prefab.ContainsKey(iname))
        {
            myPrefab = Resources.Load("Prefabs/" + iname) as GameObject;
            Manager.Instance.all_prefab.Add(iname, myPrefab);
        }
        else
            myPrefab = Manager.Instance.all_prefab[iname];
        */
    }


    public void loadNextCompartments() {
        current_compartments++;
        if (current_compartments >= Compartments.Count)
            current_compartments = 0;
        current_compartment_label.text = Compartments[current_compartments];
        if (use_coroutine) StartCoroutine(populateHexGridFromCenterSpiral());
        else populateGrid();
        //populateHexGridFromCenterSpiral();
    }

    public void loadPreviousCompartments() {
        current_compartments--;
        if (current_compartments < 0 )
            current_compartments = Compartments.Count-1;
        current_compartment_label.text = Compartments[current_compartments];
        if (use_coroutine) StartCoroutine(populateHexGridFromCenterSpiral());
        else populateGrid();
        //populateHexGridFromCenterSpiral();
    }

    public void filterHighlightHexGrid(string filter)
    {
        //highlight the one filtered
        foreach (var item in hex_instance)
        {
            item.SetActive(false);
        }
    }

    public void displayTileCounts()
    {
        
        foreach (Transform child in parent.transform)
        {
            //Debug.Log("In the displayTileCounts foreach loop.");
            //Debug.Log("The child count of the child is:" + child.childCount);
            if (!(child.childCount == 0))
            {
                string prefab_name = child.GetComponent<toggleLabelButtons>().prefab_name;
                GameObject label = child.GetChild(1).gameObject;

                if (prefab_name == null) return;
                if (label == null) return;

                
                Text label_txt = child.GetChild(1).GetComponent<Text>();

                if ((Manager.Instance.proteins_count.ContainsKey(prefab_name)))
                {
                    var colors = child.GetComponent<Toggle>().colors;
                    colors.normalColor = Color.yellow;
                    child.GetComponent<Toggle>().colors = colors;
                    label_txt.text = prefab_name + ": " + "(" + Manager.Instance.proteins_count[prefab_name].ToString() + ")";
                    //Debug.Log("The labeltext is: " + label_txt);
                    label.SetActive(true);
                }
                else
                {
                    label_txt.text = prefab_name;
                    label.SetActive(false);
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

        if (count >= hex_instance.Count)
        {
            instance = GameObject.Instantiate(item_prefab);//, getPosOnCircle(currentR), Quaternion.AngleAxis(currentR, Vector3.right)) as GameObject;
            hex_instance.Add(instance);
            instance.GetComponent<Toggle>().group = GetComponent<ToggleGroup>();
            instance.SetActive(false);
        }
        else {
            instance = hex_instance[count];
        }

        if (ig_id >= Manager.Instance.ingredients_names.Count)
        {
            Debug.Log(ig_id.ToString()+" is out of bound ");
            return;
        }
        Debug.Log(ig_id.ToString()+" is out of bound ? "+Manager.Instance.ingredients_names.Count.ToString());
        string iname = Manager.Instance.ingredients_names[ig_id];
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
        instance.transform.parent = parent.transform;
        instance.transform.localScale = Vector3.one;
        //instance.transform.localPosition = new Vector3((float)p.x, (float)p.y, 0);

        instance_props = instance.GetComponent<toggleLabelButtons>();

        
        Debug.Log("iname is hexinstance " + iname+" "+ img_name);
        if (!Manager.Instance.prefab_materials.ContainsKey(iname))
        {
            Manager.Instance.prefab_materials.Add(iname, Manager.Instance.createNewSpriteMaterial(iname));
        }
        Material amat = Manager.Instance.prefab_materials[iname];
        bool change_color = true;

        instance_props.label.GetComponent<Text>().text = iname.Replace("_"," ");
        instance_props.prefab_name = iname;

        if (iname == "Membrane") {
            iname = "Membrane";
            change_color = false;
        }
        if (iname.Contains("DNA") && iname.Contains("Draw")) { 
            iname = "DNA_full_Turn";
            img_name = "DNA_full_Turn";
            change_color = false;
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
        //load directly from ressources
        Debug.Log("Recipie/" + img_name);
        Debug.Log(sprite);
        //size depends on the area of the objects.

        if (sprite)
        {
            //Debug.Log(iname);
            instance_props.protein_sprite.sprite = sprite;
            float ratio = (float)instance_props.protein_sprite.sprite.texture.width / (float)instance_props.protein_sprite.sprite.texture.height;
            float s = 80;
            float h = (s / ratio);
            if (h > s) instance_props.protein_sprite.rectTransform.sizeDelta = new Vector2((int)(s / ratio), s);
            else  instance_props.protein_sprite.rectTransform.sizeDelta = new Vector2(s, (int)(s / ratio));
            instance_props.protein_sprite.material = amat;//;
            //instance_props.protein_sprite.rectTransform.localScale
            //instance_props.protein_sprite.sprite.
           /* if (change_color)
            {
                GameObject o = Resources.Load("Prefabs/" + iname) as GameObject;
                PrefabProperties props = o.GetComponent<PrefabProperties>();
                props.Setup();
                Color instance_color = props.Default_Sprite_Color;// (Resources.Load("Prefabs/" + iname) as GameObject).GetComponent<PrefabProperties>().Default_Sprite_Color;
                SpriteRenderer sr = o.GetComponent<SpriteRenderer>();
                Material instance_material = sr.sharedMaterial;
                
                if (instance_color != new Color(0,0,0,0) && instance_material == null)
                {
                    //instance_props.protein_sprite.color = instance_color;
                    Shader shader = Shader.Find("Sprites/Contour");
                    Material amat = SceneManager.Instance.prefab_materials[iname];// new Material(shader);
                    amat.name = iname;
                    if (instance_color != new Color(0, 0, 0, 0))
                    {
                        amat.color = instance_color;
                    }
                    else
                    {
                        amat.color = new Color(1, 1, 1, 1);
                    }
                    instance_props.protein_sprite.material = amat;
                }
            }*/
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
        //clean the prefab or just change the image
        hex_grid.Clear();
        foreach (var item in hex_instance) {
            item.SetActive(false);
        }
        Layout flat = new Layout(Layout.flat, new Point(55, 55), new Point(0, 0));
        int count = 0;
        //int start_id = (int)CompartmentsIngredients[current_compartments].x;
        //int total_counts = (int)CompartmentsIngredients[current_compartments].y;
        int total_counts = CompartmentsIngredients_ids[current_compartments].Count;
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
                oneHexInstance(count,  p, CompartmentsIngredients_ids[current_compartments][count]);
                count++;
                if (count >= total_counts) return;
            }
        }
    }

    void populateGrid()
    {
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
        //layout is the orientation of the hex
        Layout flat = new Layout(Layout.flat, new Point(55, 55), new Point(0, 0));
        
        //int start_id = (int)CompartmentsIngredients[current_compartments].x;
        //int total_counts = (int)CompartmentsIngredients[current_compartments].y; //total number of sprite to display
        int total_counts = CompartmentsIngredients_ids[current_compartments].Count;
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
            oneHexInstance(i, p, CompartmentsIngredients_ids[current_compartments][i]);
            //wait for animation ?
            //yield return new WaitForSeconds(0.05f);
        }
        displayTileCounts();
    }

    IEnumerator populateHexGridFromCenterSpiral()
    {
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
        //layout is the orientation of the hex
        Layout flat = new Layout(Layout.flat, new Point(55, 55), new Point(0, 0));
        
        //int start_id = (int)CompartmentsIngredients[current_compartments].x;
        //int total_counts = (int)CompartmentsIngredients[current_compartments].y; //total number of sprite to display
        int total_counts = CompartmentsIngredients_ids[current_compartments].Count;
         //get the Hex in spiral
        Hex center = new Hex(0, 0, 0);
        hex_grid = Hex.Spiral(center, map_radius);
        //this layout start on the left-bottom and go up and on the right.
        //try to fin a layout that start from the middle and go around.
        //map radius is the radius of the grid
        //use cubic coordinates
        for (int i = 0; i < total_counts; i++) {
            Point p = Layout.HexToPixel(flat, hex_grid[i]);
            oneHexInstance(i, p, CompartmentsIngredients_ids[current_compartments][i]);
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
        var Selection = Manager.Instance.ingredients_names.Where(x => x.ToLower().Contains(currText)).ToArray();
    }
    //Updated to not use Linq
    private void PruneItemsArray(string currText)
    {
        string _currText = currText.ToLower();
        selection.Clear();
        //search for compartment then search for ingredient ?
        for (int i = Manager.Instance.ingredients_names.Count - 1; i >= 0; i--)
        {
            string _item = Manager.Instance.ingredients_names[i].ToLower();
            if (_item.Contains(_currText))
            {
                selection.Add(i);
            }
        }
        StartCoroutine(populateHexGridFromCenterSpiralPrune(selection));
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

    //the callback
    public void OnSearchValidated(string input_query) {
        //filter and change the palette showing only the current selection
        if (input_query!="") PruneItemsArray(input_query);
        else StartCoroutine(populateHexGridFromCenterSpiral());
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

    public void AddOneIngredient(string iname, string img_name, float ascale2d,float yoffset, bool is_surface=false, bool is_fiber = false, int comp= -1) {
        var name = iname;
        /*
        var myPrefab = Manager.Instance.GetaPrefab(name, PDBid);
        Build the sprites from PDB and Illustrates
        save the sprites in images with name iname
        */
        if (Manager.Instance.all_prefab.ContainsKey(name)) {
            //change name
            name = name+"_user";
        }
        string compname = "root";
        if (comp == -1) comp = current_compartments;//first
        compname = Compartments[comp];

        CompartmentsIngredients_ids[comp].Add(Manager.Instance.ingredients_names.Count); 
        Manager.Instance.ingredients_names.Add(name);
        Manager.Instance.sprites_names.Add(img_name);
        Manager.Instance.additional_ingredients_names.Add(name);
        /*create a JSON NODE */
        //JSONNode node = createIngredientStringJson(img_name, is_surface, is_fiber, ascale2d, yoffset);
        //Manager.Instance.ingredient_node.Add(name, node);

        GameObject myPrefab = Resources.Load("Prefabs/" + name) as GameObject;
        if (myPrefab==null)
            myPrefab = Manager.Instance.Build(name,img_name);
        myPrefab.SetActive(true);
        Manager.Instance.all_prefab.Add(name, myPrefab);

        if (myPrefab)
        {
            PrefabProperties p = myPrefab.GetComponent<PrefabProperties>();
            p.compartment = compname;
            p.SetupFromValues(is_surface, is_fiber, ascale2d, yoffset);
            if (p.is_fiber)
            {
                p.persistence_length = 3;
            }            
            //need to setup the collider without the node information
        }
        //oneHexInstance(hex_instance.Count + 1, new Point(), Manager.Instance.ingredients_names.Count - 1);
        //loadNextCompartments();
        StartCoroutine(populateHexGridFromCenterSpiral());
    }

    public void AddOneGroup(string iname = null, int comp = -1, string compname = "Custom")
    {
        string name = iname;
        if (comp == -1) {
            comp = current_compartments;
        }
        //add one tile
        //comp = customCompIndice;
        CompartmentsIngredients_ids[comp].Add(Manager.Instance.ingredients_names.Count); 
        Manager.Instance.ingredients_names.Add(name);
        Manager.Instance.sprites_names.Add("grapes");//group icon
        StartCoroutine(populateHexGridFromCenterSpiral());
    }
}
