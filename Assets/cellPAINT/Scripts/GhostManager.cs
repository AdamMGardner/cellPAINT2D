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

    public void UpdateFromObject(GameObject newObject, int ghost_id){
        if (ghost_id!=-1){
            var index = ghosts_ids.IndexOf(ghost_id);
            if (index!=-1)
            {
                if (!ghosts_selections[index].Contains(newObject)){
                    ghosts_selections[index].Add(newObject);
                }
            }
            else {
                var ghost = CreateGhost(ghost_id);
                ghosts_selections.Add(new List<GameObject>(){newObject});
            }
        }        
    }

    public void RestoreGhost(){
        for (var i = 0; i < ghosts_selections.Count;i++) 
        {
            var ghost = ghosts[i];
            ghost.SetupFromSelection(ghosts_selections[i]);
            ghost.SetupGhostArea();     
            ghosts_selections[i].Clear();       
        }
        ghosts_selections.Clear();
    }

    public void Clear(){
        foreach (var o in ghosts)
        {
            //GameObject.Destroy(o.gameObject);
            Destroy(o.gameObject);
        }
        ghosts_selections.Clear();
        ghosts.Clear();
        ghosts_ids.Clear();
        _counter = 0;
    }
}
