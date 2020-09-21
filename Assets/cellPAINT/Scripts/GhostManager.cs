using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostManager : MonoBehaviour
{

    //private List<List<int>> locked_items = new List<List<int>>();
    public List<Ghost> ghosts = new List<Ghost>();
    
    public float cluster_radius = 8.0f;
    private int _counter = 0;
    private List<List<GameObject>> ghosts_selections= new List<List<GameObject>>();
    private List<List<string>> ghosts_selections_group= new List<List<string>>();
    private List<int> ghosts_ids = new List<int>();
    private static GhostManager _instance = null;
    public static GhostManager Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<GhostManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("GhostManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("GhostManager");
                _instance = go.AddComponent<GhostManager>();
                //_instance.hideFlags = HideFlags.HideInInspector;
            }
            return _instance;
        }
    }

    public Ghost CreateGhost(int id = -1){
        GameObject ghost_object = new GameObject("ghost_"+ghosts.Count.ToString());
        Ghost ghost = ghost_object.AddComponent<Ghost>();
        if (id == -1) ghost.gid = _counter;
        else ghost.gid = id;
        ghost.cluster_radius = cluster_radius;
        ghost.transform.parent = transform; 
        ghosts.Add(ghost); 
        ghosts_ids.Add(id);
        _counter++;  
        return ghost;
    }

    public void AddGhost(List<GameObject> selection, int id = -1){
        //create an empty and attach ghost component
        var ghost = CreateGhost(id);
        ghost.SetupFromSelection(selection);
        ghost.SetupGhostArea();
    }

    public void RemoveGhost(GameObject ghost_object){
        //destroy or deactivate
        Ghost ghost = ghost_object.GetComponent<Ghost>();
        if (!ghost) return;
        ghost.unGhost();
        ghosts.Remove(ghost);
        Destroy(ghost_object);
        //reset the ghost ID ?
    }

    public void UpdateFromObject(GameObject newObject, int ghost_id, string group_name, int group_id){
        if (ghost_id!=-1){
            var index = ghosts_ids.IndexOf(ghost_id);
            if (index!=-1)
            {
                if (group_name != "n") {
                    var n = group_name+"_"+group_id;
                    if (!ghosts_selections_group[index].Contains(n)){
                        ghosts_selections_group[index].Add(n);
                    }                    
                }
                else {
                    if (!ghosts_selections[index].Contains(newObject)){
                        ghosts_selections[index].Add(newObject);
                    }
                }
            }
            else {
                var ghost = CreateGhost(ghost_id);
                if (group_name != "n") {
                    string n = group_name+"_"+group_id;
                    ghosts_selections_group.Add(new List<string>(){n});                 
                }
                else { 
                    ghosts_selections_group.Add(new List<string>(){});
                }
                ghosts_selections.Add(new List<GameObject>(){newObject});
            }
        }        
    }

    public void RestoreGhost(){
        //add the group in the selection
        for (var i = 0; i < ghosts_selections_group.Count;i++) 
        {
            for (var j = 0; j < ghosts_selections_group[i].Count;j++ )
            {
                string n = ghosts_selections_group[i][j];
                //find the gameObject
                var o = GameObject.Find(n);
                if ((o)&&(!ghosts_selections[i].Contains(o)))
                {
                    ghosts_selections[i].Add(o);
                }
            }
        }
        for (var i = 0; i < ghosts_selections.Count;i++) 
        {
            var ghost = ghosts[i];
            ghost.SetupFromSelection(ghosts_selections[i]);
            ghost.SetupGhostArea();     
            ghosts_selections[i].Clear();       
        }
        ghosts_selections.Clear();
        ghosts_selections_group.Clear();
    }

    public void Clear(){
        foreach (var o in ghosts)
        {
            //GameObject.Destroy(o.gameObject);
            Destroy(o.gameObject);
        }
        ghosts_selections_group.Clear();
        ghosts_selections.Clear();
        ghosts.Clear();
        ghosts_ids.Clear();
        _counter = 0;
    }
}
