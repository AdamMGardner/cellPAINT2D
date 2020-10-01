using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_manager : MonoBehaviour
{
    public Dropdown layer_drawing;
    public Slider ninstance_click;
    public InputField ninstance_click_field;
    public Slider ninstance_radius;
    public InputField ninstance_radius_field;
    public Toggle ToggleSettings;
    public Slider distance_attachment_slider;
    public InputField distance_attachment_field;
    public Slider strength_attachment_slider;
    public InputField strength_attachment_field;
    public GameObject panel_dir;
    public GameObject prefab_directory;
    public Image IngredientSpriteMb;
    public Image IngredientSpriteFiberLeft;
    public Image IngredientSpriteFiberRight;
    public InputField group_name;
    
    private static UI_manager _instance = null;
    public static UI_manager Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<UI_manager>();
            if (_instance == null)
            {
                var go = GameObject.Find("_UIManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_UIManager");
                _instance = go.AddComponent<UI_manager>();
                //_instance.hideFlags = HideFlags.HideInInspector;
            }
            return _instance;
        }
    }


    public void Start(){
        UpdatePanelUserDirectory();
    }

    public void SetNinstance(float number) {
        Manager.Instance.nbInstancePerClick = (int) number;
        ninstance_click_field.text = number.ToString();
    }   
    public void SetNinstance(string number) {
        Manager.Instance.nbInstancePerClick = int.Parse(number);
        ninstance_click.value = int.Parse(number);
    }   

    public void SetRadiusNinstance(float number) {
        Manager.Instance.radiusPerClick = number;
        ninstance_radius_field.text = number.ToString();
    }   
    public void SetRadiusNinstance(string number) {
        Manager.Instance.radiusPerClick = float.Parse(number);
        ninstance_radius.value = int.Parse(number);
    }   

    public void SetCurrentLayer(int layer) {
        Manager.Instance.layer_number_options = layer;
    }

    public void SetStrengthAttach(float number) {
        Manager.Instance.frequency_attach = number;
        strength_attachment_field.text = number.ToString();
    }   
    public void SetStrengthAttach(string number) {
        Manager.Instance.frequency_attach = float.Parse(number);
        strength_attachment_slider.value = float.Parse(number);
    }   

    public void SetDistanceAttach(float number) {
        Manager.Instance.distance_attach = number;
        distance_attachment_field.text = number.ToString();
    }   
    public void SetDistanceAttach(string number) {
        Manager.Instance.distance_attach = float.Parse(number);
        distance_attachment_slider.value = float.Parse(number);
    }   
    public void SetCollisionAttach(bool value) {
        Manager.Instance.collision_attach = value;
    }   
    public void SetTypeAttach(int type) {
        Manager.Instance.type_attach = type;
    }   

    public void UpdatePanelUserDirectory(){
        foreach(Transform child in panel_dir.transform)
        {
            Destroy(child.gameObject);
        }
        string default_path = Path.Combine(PdbLoader.DefaultDataDirectory,"images");
        var adir = GameObject.Instantiate(prefab_directory);
        adir.GetComponent<Text>().text = default_path;
        adir.transform.parent = panel_dir.transform;
        adir.transform.GetChild(0).gameObject.SetActive(false);
        foreach (var d in PdbLoader.DataDirectories) {
            Debug.Log(d);
            adir = GameObject.Instantiate(prefab_directory);
            adir.transform.parent = panel_dir.transform;
            adir.GetComponent<Text>().text = d;
        }
    }

    public void ClearCachDi_cb(){
        Manager.Instance.ClearCacheDirectory();
        UpdatePanelUserDirectory();
    }
}
