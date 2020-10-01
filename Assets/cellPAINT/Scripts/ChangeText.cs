using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DeleteSelf(){
        var path = GetComponent<Text>().text;
        if (PdbLoader.DataDirectories.Contains(path)) {
            PdbLoader.DataDirectories.Remove(path);
            if (PlayerPrefs.HasKey("UserDirectories")) {
                var current_paths = PlayerPrefs.GetString("UserDirectories");
                current_paths.Replace(path+";","");
                PlayerPrefs.SetString("UserDirectories",path);
                PlayerPrefs.Save();
            }
        }
        UI_manager.Get.UpdatePanelUserDirectory();
    }
}
