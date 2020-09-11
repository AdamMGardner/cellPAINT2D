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
    
    public GameObject parent;
    private GameObject item;

    private float last_radius;
    private int last_nbitems;
    private List<Hex> hex_grid;
    private List<GameObject> hex_instance;
    public List<string> Compartments;
    public bool use_coroutine=false;
    private int current_compartments = 0;
    private Dictionary<int, Vector2> CompartmentsIngredients;

    private bool isGrouped = true; ////3/17/17

    void OnEnable() {
        Manager.Instance.recipeUI = this;
        Manager.Instance.ingredient_node = new Dictionary<string, JSONNode>();
        Compartments = new List<string>();
        CompartmentsIngredients = new Dictionary<int, Vector2>();
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
        int start_id = (int)CompartmentsIngredients[current_compartments].x;
        int total_counts = (int)CompartmentsIngredients[current_compartments].y; //total number of sprite to display

        for (int count = 0; count < total_counts; count++)
        {
            if (count + start_id < Manager.Instance.ingredients_names.Count)
            {
                string iname = Manager.Instance.ingredients_names[count + start_id];

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
        Manager.Instance.Clear();
        Manager.Instance.ingredient_node.Clear();
        Compartments.Clear();
        CompartmentsIngredients.Clear();
        //Manager.Instance.ingredients_prefab.Clear();
        Manager.Instance.all_prefab.Clear();
        Manager.Instance.ingredients_names.Clear();
        Manager.Instance.sprites_names.Clear();
        Manager.Instance.prefab_materials = new Dictionary<string, Material>();        
        Debug.Log("try loading ressource "+id.ToString());
        JSONNode resultData = JSONNode.Parse(recipes[id].text);
        LoadRecipe_cb(resultData);
    }
    //need to add the membrane
    
    public void LoadRecipe(string filename=null) {
        //Debug.Log("*****");
        //Debug.Log("Loading scene: " + recipePath);
        Manager.Instance.ingredient_node.Clear();
        Compartments.Clear();
        CompartmentsIngredients.Clear();
        //Manager.Instance.ingredients_prefab.Clear();
        //Destroy all prefab ?
        foreach (var KeyVal in Manager.Instance.all_prefab)
        {
            GameObject.Destroy(KeyVal.Value);
        }
        Manager.Instance.all_prefab.Clear();
        Manager.Instance.ingredients_names.Clear();
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
            CompartmentsIngredients.Add(nC, new Vector2(0, nIngredients));
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
            CompartmentsIngredients.Add(nC, new Vector2(nIngredients, total));
            Debug.Log("found " + nIngredients.ToString() + " " + total.ToString()+" "+i.ToString());
            nIngredients += total;
            nCompartemnts += 2;
            nC++;
        }
        if (nCompartemnts < 2)
            nCompartemnts = 2;
        DateTime start = DateTime.Now;
        if (resultData["cytoplasme"] != null)
        {
            AddRecipeIngredients(resultData["cytoplasme"]["ingredients"], "cytoplasme",false);
        }

        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            //add the membrane
            //SceneManager.Instance.ingredients_names.Add("DrawMembrane");
            AddRecipeIngredientMembrane(resultData["compartments"][i]);//"Cell_Membrane"
            AddRecipeIngredients(resultData["compartments"][i]["surface"]["ingredients"], "surface" + i.ToString(),true);
            AddRecipeIngredients(resultData["compartments"][i]["interior"]["ingredients"], "interior" + i.ToString(),false);
        }
        //this is comment as I manually change a lot of name of the recipe to a better reading for the NSF competition. 
        //buildHierarchy (resultData);
        current_compartments = 0;
        loadNextCompartments();
        //load first ingredient of first compartments
        int ingid = (int)CompartmentsIngredients[current_compartments].x;
        string iname = Manager.Instance.ingredients_names[ingid];
        Manager.Instance.SwitchPrefabFromName(iname);
        var prefab = Manager.Instance.all_prefab[iname];
        Manager.Instance.changeDescription(prefab, prefab.GetComponent<SpriteRenderer>());
        hex_instance[0].GetComponent<Toggle>().isOn = true;
    }
    
    public static void AddRecipeIngredients(JSONNode recipeDictionary, string prefix,bool surface)
    {
        for (int j = 0; j < recipeDictionary.Count; j++)
        {
            AddProteinIngredient(recipeDictionary[j], prefix,surface);
        }
    }

    public static void AddRecipeIngredientMembrane(JSONNode compDictionary)
    {
        
        string iname = compDictionary["name"].Value;
        string prefab_name = "Cell_Membrane";
        Debug.Log("AddRecipeIngredientMembrane "+iname);
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
    public static void AddProteinIngredient(JSONNode ingredientDictionary, string prefix, bool surface)
    {
        string iname = ingredientDictionary["name"].Value;
        string img_name = iname;
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
        if (iname.Contains("DNA")) {
            iname = "Draw DNA";
        }
        if (iname.Contains("hu"))
        {
            iname = "HU";
        }
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

    private void oneHexInstance(int count, Point p, int start_id) {
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

        if ((start_id + count) >= Manager.Instance.ingredients_names.Count)
        {
            Debug.Log(start_id + count);
            return;
        }
        string iname = Manager.Instance.ingredients_names[start_id + count];
        string img_name = Manager.Instance.sprites_names[start_id + count];
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
        int start_id = (int)CompartmentsIngredients[current_compartments].x;
        int total_counts = (int)CompartmentsIngredients[current_compartments].y;

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
                oneHexInstance(count,  p, start_id);
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
        
        int start_id = (int)CompartmentsIngredients[current_compartments].x;
        int total_counts = (int)CompartmentsIngredients[current_compartments].y; //total number of sprite to display

        Debug.Log("populate " + start_id.ToString() + " " + total_counts.ToString());
        //get the Hex in spiral
        Hex center = new Hex(0, 0, 0);
        hex_grid = Hex.Spiral(center, map_radius);
        //this layout start on the left-bottom and go up and on the right.
        //try to fin a layout that start from the middle and go around.
        //map radius is the radius of the grid
        //use cubic coordinates
        for (int i = 0; i < total_counts; i++) {
            Point p = Layout.HexToPixel(flat, hex_grid[i]);
            oneHexInstance(i, p, start_id);
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
        
        int start_id = (int)CompartmentsIngredients[current_compartments].x;
        int total_counts = (int)CompartmentsIngredients[current_compartments].y; //total number of sprite to display

         Debug.Log("populate " + start_id.ToString() + " " + total_counts.ToString());
        //get the Hex in spiral
        Hex center = new Hex(0, 0, 0);
        hex_grid = Hex.Spiral(center, map_radius);
        //this layout start on the left-bottom and go up and on the right.
        //try to fin a layout that start from the middle and go around.
        //map radius is the radius of the grid
        //use cubic coordinates
        for (int i = 0; i < total_counts; i++) {
            Point p = Layout.HexToPixel(flat, hex_grid[i]);
            oneHexInstance(i, p, start_id);
            //wait for animation ?
            yield return new WaitForSeconds(0.05f);
        }
        displayTileCounts();
    }
}
