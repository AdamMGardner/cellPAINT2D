using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UIWidgets;

public class toggleLabelButtons : MonoBehaviour , IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler,IPointerExitHandler {
 

    public bool only_ui = false;
    public bool is_ListView = false;
    private bool count_updated = false;
    public GameObject label;
    public string prefab_name="";
    public Image protein_sprite;
    public Image mb_sprite;
    public GameObject delete_panel;
    private Toggle uitoggle;
    //private GameObject uiHolder;
    public Text label_txt;
    private GameObject prefab;

    /*public static void AddEventTriggerListener(EventTrigger trigger,
                                           EventTriggerType eventType,
                                           System.Action<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(callback));
        trigger.triggers.Add(entry);
    }
    */
    void OnEnable() {

        if (!only_ui)
        {
            //label = transform.GetChild(1).gameObject;
            label_txt = label.GetComponent<Text>();
        }
        //if (!uiHolder)
        //{
        //    uiHolder = Manager.Instance.uiHolder;
        //}

    }

    // Use this for initialization
    void Start () {
        if (!only_ui) uitoggle = GetComponent<Toggle>();
        /*
        EventTrigger evtrigger = gameObject.AddComponent<EventTrigger>();
        AddEventTriggerListener(evtrigger,   
                                    EventTriggerType.PointerEnter,
                                    Over);
        AddEventTriggerListener(evtrigger,
                                    EventTriggerType.PointerExit,
                                    Exit);
        //if you want mouse click
        //EventTriggerType.PointerClick
        if (!only_ui)
            AddEventTriggerListener(evtrigger,
                            EventTriggerType.PointerClick,
                            togglePrefabManager);
        else {
            if (GetComponent<Button>()!=null) AddEventTriggerListener(evtrigger,
                            EventTriggerType.PointerClick,
                            Exit);
        }*/
        //if (!uiHolder)
        //{
        //    uiHolder = GameObject.Find("uiHolder");
        //}
        if (prefab_name == null) return;
        if (label == null) return;
        
    }
	
	// Update is called once per frame
	void Update ()
    {

    }

    public void DeleteSelf(){
        //delete in recipe
        Manager.Instance.recipeUI.RemoveIngredient(prefab_name);
        delete_panel.SetActive(false);
    }

    public void CancelDeleteSelf(){
        delete_panel.SetActive(false);
    }

    public void OnPointerClick(PointerEventData data)
    {
        //Debug.Log("OnPointerClick called.");
        if (data.button == PointerEventData.InputButton.Right){
            //pop delete/cancel button window
            if (!Manager.Instance.additional_ingredients_names.Contains(prefab_name))
                delete_panel.SetActive(true);
        }
        if ((data.button == PointerEventData.InputButton.Left)&&(!only_ui)) {
            togglePrefabManager(data);
        }
    }

     public void OnPointerDown (PointerEventData eventData) {
        //Debug.Log("OnPointerClick called.");
        if (eventData.button == PointerEventData.InputButton.Right){
            //pop delete/cancel button window
            delete_panel.SetActive(true);
        }
        if ((eventData.button == PointerEventData.InputButton.Left)&&(!only_ui)) {
            togglePrefabManager(eventData);
        }
     }
 
     public void OnPointerUp (PointerEventData eventData) {
        //Debug.Log("OnPointerUp called.");
        if (gameObject.GetComponent<Button>()){
            Manager.Instance.mask_ui = false;
        }
     }

     public  void OnPointerEnter(PointerEventData data)
    {
        //Debug.Log("OnPointerEnter called.");
        Cursor.visible = true;
        Over(data);
    }

    public void OnPointerExit(PointerEventData data)
    {
        //Debug.Log("OnPointerExit called.");
        Cursor.visible = false;
        Exit(data);
    }
    public void OnSelect(BaseEventData data)
    {
        //Debug.Log("OnSelect called.");
    }

    public void updateTileCount ()
    {
        if (prefab_name == null) return;
        if (label == null) return;
        var iname = prefab_name.Split('.')[2];
        //var PC = SceneManager.Instance.proteins_count;

        if ((Manager.Instance.proteins_count.ContainsKey(prefab_name)))
        {
            label_txt.text =  iname + ": " + "(" + Manager.Instance.proteins_count[prefab_name].ToString() + ")";
            count_updated = true;
            if (Manager.Instance.DEBUG)
            {
                Debug.Log("The Label text is: " + label_txt.text);
                Debug.Log("The protein count is: " + Manager.Instance.proteins_count[prefab_name].ToString());
            }
        }
        else {
            label_txt.text = iname;
        }
    }

    public void togglePrefabManager(BaseEventData eventData) {

        if (Manager.Instance.bucketMode)
        {
            if (prefab_name != "" && Manager.Instance.all_prefab.ContainsKey(prefab_name))
            {
                //check selected prefab properties, only add to selected_prefab list only if not surface, fiber, and bound items
                GameObject prefab = Manager.Instance.all_prefab[prefab_name];// Resources.Load("Prefabs/" + prefab_name) as GameObject;
                PrefabProperties props = null;
                if (prefab != null)
                    props = prefab.GetComponent<PrefabProperties>();

                if (props != null && !props.is_surface && !props.is_fiber && !props.is_bound)
                {
                    if (Manager.Instance.selected_prefab.Contains(prefab_name))
                        Manager.Instance.selected_prefab.Remove(prefab_name);
                    else
                        Manager.Instance.selected_prefab.Add(prefab_name);
                }
            }
        }
        else
        {
            //clear selected_prefab and only keep current_prefab in the selected_prefab list when switching to brush mode
            Manager.Instance.selected_prefab.Clear();

            //use manager to switch prefab to new one.
            if (prefab_name != "")
            {
                Manager.Instance.SwitchPrefabFromName(prefab_name);

                //add to selected_prefab if in draw mode and not surface, fiber, and bound mode
                if (Manager.Instance.drawMode && !Manager.Instance.surfaceMode && !Manager.Instance.fiberMode && !Manager.Instance.boundMode)
                    Manager.Instance.selected_prefab.Add(prefab_name);
            }
            else
            {
                if (Manager.Instance.myPrefab && !Manager.Instance.surfaceMode && !Manager.Instance.fiberMode&& !Manager.Instance.boundMode)
                    Manager.Instance.selected_prefab.Add(Manager.Instance.myPrefab.name);
            }

            //hide the menu
            //if (!uiHolder)
            //{
            //    uiHolder = Manager.Instance.uiHolder;
            //}
            //uiHolder.SetActive(false);
            //Manager.Instance.Description_Holder.SetActive(false);
            Manager.Instance.mask_ui = false;
            Manager.Instance.last_active_current_name_below = prefab_name;
            Manager.Instance.current_objectparent_below = null;
            //GameObject.Find("ToggleRecipesPrefabs").GetComponent<Toggle>().isOn = false;
            //toggle on the brush

        }
        Manager.Instance.recipeUI.updateHexInstance();
    }

    void Over(BaseEventData eventData) {
        //show the descritpion
        if (!only_ui && prefab_name!="" && prefab_name != null)
        {
            updateTileCount();
            label.SetActive(true);
            //if (prefab == null) prefab = Resources.Load("Prefabs/" + prefab_name) as GameObject;
            //if (prefab != null)
            //{
            Debug.Log("over " + prefab_name);
            Debug.Log(prefab_name +" inside all prefab "+ Manager.Instance.all_prefab.ContainsKey(prefab_name).ToString());
           // Debug.Log(!Manager.Instance.all_prefab.ContainsKey(prefab_name));
           // List<string> keyList = new List<string>(Manager.Instance.all_prefab.Keys);
           // Debug.Log("keys " + keyList.Count.ToString());
           // foreach (var k in keyList) { Debug.Log("keys is "+k); }
            if (!Manager.Instance.all_prefab.ContainsKey(prefab_name))
            {
                List<string> keyList = new List<string>(Manager.Instance.all_prefab.Keys);
                Debug.Log("keys " + keyList.Count.ToString());
                foreach (var k in keyList) { Debug.Log("keys is "+k); }
                //do nothing 
                Debug.Log("Over "+ prefab_name + " not found in all_prefab");
                prefab = Resources.Load("Prefabs/" + prefab_name.Split('.')[2]) as GameObject;
                if (prefab == null)
                    prefab = Manager.Instance.Build(prefab_name);
                if (prefab != null){
                    //prefab = Manager.Instance.Build(prefab_name);
                    prefab.SetActive(true);
                    Manager.Instance.all_prefab.Add(prefab_name, prefab);
                }
            }
            else
                prefab = Manager.Instance.all_prefab[prefab_name];
            //prefab = Resources.Load("Prefabs/" + prefab_name) as GameObject;
            
            if (prefab != null)
            {
                var sr = prefab.GetComponent<SpriteRenderer>();
                Debug.Log("actual name " + prefab.GetComponent<PrefabProperties>().common_name + " "+ prefab.GetComponent<PrefabProperties>().name);
                //Debug.Log(Manager.Instance.prefab_materials[prefab_name]);
                if ( Manager.Instance.prefab_materials.ContainsKey(prefab_name)) {
                    if (sr) sr.sharedMaterial = Manager.Instance.prefab_materials[prefab_name];
                }
                //Manager.Instance.Description_Holder.transform.GetChild(0).GetComponent<ColorPicker>().active = false;
                //Manager.Instance.Description_Holder_HSV.SetActive(false);
                Manager.Instance.current_name_below = prefab_name;
                Manager.Instance.current_objectparent_below = null;
                Manager.Instance.changeDescription(prefab, sr);
                //dont use HSV just show the description
                if (Manager.Instance.last_active_current_name_below == null) Manager.Instance.last_active_current_name_below = prefab_name;
            }
            else {
                Debug.Log("didnt load " + prefab_name);
            }
        }
        Manager.Instance.mask_ui = true;
    }

    void Exit(BaseEventData eventData) {
        if (!only_ui) {
            Debug.Log("exit " + prefab_name);
            if (!count_updated & !is_ListView) label.SetActive(false);
            if (Manager.Instance.all_prefab.ContainsKey(Manager.Instance.last_active_current_name_below)){
                Manager.Instance.current_name_below = Manager.Instance.last_active_current_name_below;
                Manager.Instance.current_objectparent_below = null;
                Manager.Instance.changeDescription(Manager.Instance.all_prefab[Manager.Instance.current_name_below], Manager.Instance.all_prefab[Manager.Instance.current_name_below ].GetComponent<SpriteRenderer>());
            }
        }
        Manager.Instance.mask_ui = false;
    }
}
