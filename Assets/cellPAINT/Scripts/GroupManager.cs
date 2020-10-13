using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group
{
    public string name;
    public Dictionary<int,List<GameObject>> childs ;
    public Group(string aname){
        name = aname;
        childs = new Dictionary<int,List<GameObject>>();
    }
}


public class GroupManager : MonoBehaviour
{
    //help to create and manag group
    // Declare the scene manager as a singleton
    public GameObject prefabGroup;
    public List<string> groupnames;
    public Dictionary<string,int> groupinstances;
   
    public Dictionary<string,Group> groups;
    public List<GameObject> current_selections;


    private static GroupManager _instance = null;
    public static GroupManager Get
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<GroupManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("_GroupManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_GroupManager");
                _instance = go.AddComponent<GroupManager>();
                //_instance.hideFlags = HideFlags.HideInInspector;
            }
            return _instance;
        }
    }
    public int nGroup = 0;
    // Start is called before the first frame update
    void Setup()
    {
        if (groupnames==null) groupnames = new List<string>();
        if (groupinstances==null) groupinstances = new Dictionary<string, int>();
        if (current_selections==null) current_selections = new List<GameObject>();
        if (groups==null) groups = new Dictionary<string,Group>();
    }

    void Start()
    {
        Setup();
    }

    public void Reset()
    {
        Setup();
        current_selections.Clear();
        nGroup = 0;
        groupnames.Clear();
        groupinstances.Clear();
        groups.Clear();
    }

    public void Clear(){
        Setup();
        current_selections.Clear();
        if (groupinstances.Count!=0){
            foreach(var name in groupnames){
                if (groupinstances.ContainsKey(name)) groupinstances[name] = 0;
            }
        }
    }

    public void UpdateGroupFromObject(GameObject newObject, string group_name, int group_id){
        if (group_name!="n"){
            if (groups.ContainsKey(group_name)){
                if (groups[group_name].childs.ContainsKey(group_id)){
                    groups[group_name].childs[group_id].Add(newObject);
                }
                else {
                    groups[group_name].childs.Add(group_id,new List<GameObject>());
                    groups[group_name].childs[group_id].Add(newObject);
                }
            }
            else {
                groups.Add(group_name,new Group(group_name));
                groups[group_name].childs.Add(group_id,new List<GameObject>());
                groups[group_name].childs[group_id].Add(newObject);
            }
        }        
    }

    public Bounds getBound() {
        Vector3 center = Vector3.zero;
        float r = 0.0f;
        int ntotal = 0;
        foreach (var o in current_selections)
        {
            if (Manager.Instance.fiber_parents.Contains(o)) {
                for (int i = 0; i < o.transform.childCount; i++)
                {
                    PrefabProperties p = o.transform.GetChild(i).gameObject.GetComponent<PrefabProperties>();
                    if (p) r += p.circle_radius;
                    center += o.transform.GetChild(i).position;
                    //global_bounds.Encapsulate(o.transform.GetChild(i).position);
                    ntotal++;
                }
            }
            else
            {
                PrefabProperties p = o.GetComponent<PrefabProperties>();
                if (p) r += p.circle_radius;
                center += o.transform.position;
                //global_bounds.Encapsulate(o.transform.position);
                ntotal++;
            }
        }
        center /= (float)ntotal;
        Bounds global_bounds = new Bounds(center,Vector3.one);
        foreach (var o in current_selections)
        {
            if (Manager.Instance.fiber_parents.Contains(o)) {
                for (int i = 0; i < o.transform.childCount; i++)
                {
                    //PrefabProperties p = o.transform.GetChild(i).gameObject.GetComponent<PrefabProperties>();
                    //if (p) r += p.circle_radius;
                    //center += o.transform.GetChild(i).position;
                    global_bounds.Encapsulate(o.transform.GetChild(i).position);
                    //ntotal++;
                }
            }
            else
            {
                //PrefabProperties p = o.GetComponent<PrefabProperties>();
                //if (p) r += p.circle_radius;
                //center += o.transform.position;
                global_bounds.Encapsulate(o.transform.position);
                //ntotal++;
            }
        }
        //r/= (float)ntotal;
        //global_bounds.center = center;
        //r=global_bounds.size.x;
        //global_bounds = new Bounds(center, global_bounds.size);//new Vector3(r,r,r));
        return global_bounds;
    }

    public void CreateInstanceGroup(GameObject group, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        float Zangle = UnityEngine.Random.value * Mathf.PI * Mathf.Rad2Deg;
        Quaternion quat = Quaternion.AngleAxis(Zangle, Vector3.forward);
        //scale ?
        var newGroup = Instantiate(group, new Vector3(position.x,position.y,0.0f), rotation);
        //if (Manager.Instance.random_rotation) 
        //newGroup.transform.rotation = UnityEngine.Random.rotation;
        newGroup.layer = 22;//midle layer == group
        newGroup.gameObject.SetActive(true);
        PrefabGroup p = newGroup.GetComponent<PrefabGroup>();
        //p.name = "group_" + nGroup.ToString();
        //do we fixthem? kinematics ?
        //Manager.Instance.PinHierarchy(newGroup);//this is a toggle
        groupinstances[p.name]++;   
        p.instance_id = groupinstances[p.name];    
        newGroup.gameObject.name = p.name+"_"+p.instance_id.ToString();
        for (int i = 0; i < newGroup.transform.childCount; i++)
        {
            var newObj = newGroup.transform.GetChild(i).gameObject; 
            var props = newObj.GetComponent<PrefabProperties>();
            Rigidbody2D rb = newObj.GetComponent<Rigidbody2D>();
            //we actually need to create a new draw_instance
            if (newObj.name.Contains("_chain_"))
            {
                //check if attached. rename ?
                Manager.Instance.fiber_parents.Add(newObj);
                GameObject[] ch = new GameObject[newObj.transform.childCount];
                for (int j = 0; j < newObj.transform.childCount; j++)
                {
                    var current_f = newObj.transform.GetChild(j).gameObject;
                    int id = current_f.transform.GetSiblingIndex();//int.Parse ( current_f.name.Split(new string[] { "ob" }, StringSplitOptions.None)[1] );
                    ch[id] = current_f;//use j ? or the name ?
                }
                Manager.Instance.fibers_instances.Add(ch.ToList());
                Manager.Instance.fiber_count++;
                //reset connectivity
            }
            else
            {
                if (props.is_surface)
                {
                    Manager.Instance.surface_objects.Add(newObj);
                }
                else
                {
                    Manager.Instance.everything.Add(rb);
                }
                Manager.Instance.rbCount++;
                props.UpdateOutline(false);
                //Manager.Instance.UpdateCountAndLabel(p.name);
            }
            //what about the binding spring?
            //use PrefabGroup p.attachements
        }
        foreach (SpringJoint2D sjt in p.attachements)
        {
            var attach1 = sjt.gameObject;
            var attach2 = sjt.connectedBody.gameObject;
            int id = attach2.transform.GetSiblingIndex();
            /*if (Manager.Instance.fiber_parents.Contains(attach2.transform.parent.gameObject)) {
                id = attach2.transform.parent.GetSiblingIndex();
                int chid = attach2.transform.GetSiblingIndex();
                attach2 = newGroup.transform.GetChild(id).GetChild(chid).gameObject;
            }
            else {
                attach2 = newGroup.transform.GetChild(id).gameObject;
            }
            sjt.connectedBody = attach2.GetComponent<Rigidbody>();
            */
            Manager.Instance.attached.Add(attach1);
            Manager.Instance.attached.Add(attach2);
            Manager.Instance.attachments.Add(sjt);//HighlightManager.Instance.pinned_to_bonds.Add(sjt);
        }
        //parent?
        if (parent != null)
            newGroup.transform.parent = parent;
    }

    public bool IsAttachedToSelection(GameObject attached){
        foreach (var o in current_selections){
            var pg = o.GetComponent<PrefabGroup>();
            if (pg){
                foreach (Transform m in o.transform){
                    if (Manager.Instance.fiber_parents.Contains(m.gameObject)){
                        foreach (Transform k in m.transform){
                            if (k.gameObject == attached) return true;
                        }
                    }
                    else {
                        if (m.gameObject == attached) return true;
                    }
                }
            }
            else {
                if (Manager.Instance.fiber_parents.Contains(o)){
                    foreach (Transform k in o.transform){
                        if (k.gameObject == attached) return true;
                    }
                }
                else {
                    if (o == attached) return true;
                }
            }
        }
        return false;
    }


    public void CheckAttached(GameObject toTest, PrefabGroup g) {
        var foundJT = new List<SpringJoint2D>();
        var foundIndexes = new List<int>();
        for (int i = 0; i < Manager.Instance.attached.Count; i++)
        {
            if (Manager.Instance.attached[i] == toTest) {
                if ((i%2)==0) {
                    foundJT.Add(Manager.Instance.attachments[i/2]);
                    foundIndexes.Add(i);
                }
                else {
                    foundJT.Add(Manager.Instance.attachments[(i-1)/2]);
                    foundIndexes.Add(i-1);
                }
            }
        }
        for (int i = 0; i < foundIndexes.Count; i++){
            var spr = foundJT[i];
            if (IsAttachedToSelection(spr.connectedBody.gameObject)){
                //the spring need to only relate to object in the selection
                if (g.attachements.Contains(spr)) continue;
                g.attachements.Add(spr);
            }
            else {
                if (g.attachements_toskip.Contains(spr)) continue;
                g.attachements_toskip.Add(spr);
            }            
        }
    }

    public void AddSelection(GameObject selection, GameObject group){
        var g = group.GetComponent<PrefabGroup>();
        selection.transform.parent = group.transform;
        //take care of the attachments
        if (Manager.Instance.fiber_parents.Contains(selection)) {
            for (int i = 0; i < selection.transform.childCount; i++) {
                var current_o = selection.transform.GetChild(i).gameObject;
                CheckAttached(current_o,g);
                /*var allspring = current_o.GetComponents<SpringJoint2D>();
                for (int s = 0; s < allspring.Length; s++)
                {
                    var spr = allspring[s];
                    if (IsAttachedToSelection(spr.connectedBody.gameObject)){
                        //the spring need to only relate to object in the selection
                        if (g.attachements.Contains(spr)) continue;
                        g.attachements.Add(spr);
                    }
                    else {
                        if (g.attachements_toskip.Contains(spr)) continue;
                        g.attachements_toskip.Add(spr);
                    }
                }*/
            }
        }
        else
        {
            CheckAttached(selection,g);
            /*var allspring = selection.GetComponents<SpringJoint2D>();
            foreach (var sjt in allspring)
            {
                if (IsAttachedToSelection(sjt.connectedBody.gameObject)){
                    if (g.attachements.Contains(sjt)) continue;
                    g.attachements.Add(sjt);
                }
                else {
                    if (g.attachements_toskip.Contains(sjt)) continue;
                    g.attachements_toskip.Add(sjt);
                }
            }*/
        }
    }

    public void AddSelectionHierarchy(GameObject selection, GameObject group){
        var pg = selection.GetComponent<PrefabGroup>();
        Debug.Log("AddSelectionHierarchy "+selection.name+" pg "+(pg!=null).ToString()+" " +selection.transform.childCount.ToString());
        if (pg) {
            //for (int i = 0; i < selection.transform.childCount; i++) 
            List<GameObject> childObjects = new List<GameObject>();
            foreach(Transform current_o in selection.transform)
            { 
                childObjects.Add(current_o.gameObject);
            }
            foreach(GameObject current_o in childObjects)
            {
                //var current_o = selection.transform.GetChild(i).gameObject;
                Debug.Log("current_o "+current_o.name);
                AddSelection(current_o, group);
            }
        }//compartment ?
        else 
        {
            AddSelection(selection, group);
        }
    }
    
    public string CreateGroup(string gname = "") {
        //add to ui
        //need to reassing the spring connectedbody!
        if (current_selections.Count == 0) return "";
        var bb = getBound();
        Debug.Log("group bb "+bb.ToString());
        var emptyGroup = Instantiate(prefabGroup, Vector3.zero, Quaternion.identity);
        PrefabGroup g = emptyGroup.GetComponent<PrefabGroup>();
        //name the group
        while (groupnames.Contains("group_" + nGroup.ToString())){
            nGroup++;
        }
        g.name = "group_" + nGroup.ToString();
        if (gname != "") {
            g.name = gname;
        }
        var cname = Manager.Instance.recipeUI.GetCurrentCname();
        g.name = cname+".interior."+g.name;
        g.instance_id = 0;
        emptyGroup.name = g.name;//"group_" + nGroup.ToString()+"_0";
        emptyGroup.layer = 22;//midle layer == group
        emptyGroup.transform.position = new Vector3(bb.center.x,bb.center.y,0.0f);
        emptyGroup.transform.parent = Manager.Instance.root.transform;//current_selections[0].transform.parent;//root or compartment
        var compartment = "root";
        var pro = current_selections[0].GetComponent<PrefabProperties>();
        if (pro == null) pro = current_selections[0].transform.GetChild(0).GetComponent<PrefabProperties>();
        if (pro!=null) compartment = pro.compartment;
        //if the parent was already a group use his parent
        /*if (current_selections[0].transform.parent.GetComponent<PrefabGroup>()!= null)
        {
            emptyGroup.transform.parent = current_selections[0].transform.parent.parent;
        }
        if (current_selections[0].GetComponent<PrefabGroup>()!= null)
        {
            pro = current_selections[0].transform.GetChild(0).GetComponent<PrefabProperties>();
            if (pro == null) {
                if (current_selections[0].transform.GetChild(0).childCount > 0)
                    pro = current_selections[0].transform.GetChild(0).GetChild(0).GetComponent<PrefabProperties>();
            }
            if (pro!=null) compartment = pro.compartment;
            if (compartment == "outside") compartment = "root";
        }*/
        //use compartment of first object selected.
        var pp= emptyGroup.GetComponent<PrefabProperties>();
        pp.is_Group = true;
        pp.compartment = Manager.Instance.recipeUI.GetCurrentCname();
        pp.name = g.name;
        Debug.Log("group compartment is "+compartment);

        List<Transform> parents = new List<Transform>();
        foreach (var o in current_selections)
        {
            AddSelectionHierarchy(o, emptyGroup);
        }
        Manager.Instance.highLightHierarchy(emptyGroup.transform, false);
        Debug.Log("group reparented to "+emptyGroup.ToString());
        //Create the prefab
        var newGroup = Instantiate(emptyGroup, Vector3.zero, Quaternion.identity);
        PrefabGroup p = newGroup.GetComponent<PrefabGroup>();
        foreach (var sjt in p.attachements_toskip){
            DestroyImmediate(sjt);
        }
        newGroup.name = g.name;
        p.attachements_toskip.Clear();
        p.name = g.name;
        Debug.Log("group name added "+p.name);
        groupnames.Add(p.name);
        Debug.Log("group instance 0 added "+p.name);
        groupinstances.Add(p.name,0);
        Debug.Log("group prefab added "+p.name);
        Manager.Instance.all_prefab.Add(p.name, newGroup);
        //material ?

        p.bound = getBound();//world bb?
        Debug.Log("group world bound "+ p.bound.ToString());
        //do we fixthem? kinematics ?
        var new_list = new List<GameObject>();
        //for (int o = 0; o < current_selections.Count; o++) {
        for (int o = 0; o < newGroup.transform.childCount; o++) {
            var oc = newGroup.transform.GetChild(o);
            if (oc != null) {
                var selo = emptyGroup.transform.GetChild(o).gameObject;
                Vector3 newpos = selo.transform.position - new Vector3(p.bound.center.x,p.bound.center.y,0.0f);
                oc.position = new Vector3(newpos.x,newpos.y,oc.position.z);//current_selections[o].transform.position - p.bound.center;
            }
        }
        Manager.Instance.highLightHierarchy(newGroup.transform, false);
        newGroup.transform.position = new Vector3(1000, 0, 0);//put it out of the camera view
        newGroup.gameObject.SetActive(false);
        //the prefab is a an empty object parenting copy of all the selected object.
        //forwhich we need a bounding box so we can center it
        Manager.Instance.recipeUI.AddOneGroup(p.name,-1,compartment);
        current_selections.Clear();
        //HighlightManager.Instance.UpdatePinToBondPositions_cb();
        nGroup++;
        return p.name;
    }

    public void Update() {
        if (!Manager.Instance.groupMode && !Manager.Instance.ghostMode)
        {
            if (current_selections.Count!=0) current_selections.Clear();
        }
        foreach (var o in current_selections)
        {
            //Instantiate all child
            var props = o.GetComponent<PrefabProperties>();
            //test for attachements
            if (Manager.Instance.fiber_parents.Contains(o))
            {
                Manager.Instance.highLightHierarchy(o.transform, true);
            }
            else
            {
                if (props) {
                    if (props.is_Group) Manager.Instance.highLightHierarchy(o.transform, true);
                    else {
                        props.outline_width = Manager.Instance.current_camera.orthographicSize;
                        props.UpdateOutline(true);
                    }
                }
            }
        }
    }

    public void RemoveIngredientFromGroups(string ing_name){
        foreach(var gname in groupnames) {
            var prefab = Manager.Instance.all_prefab[gname];
            PrefabGroup pg = prefab.GetComponent<PrefabGroup>();
            var family =
                       from o in prefab.transform.GetComponentsInChildren<Transform>()
                       where (o!=null && o.name.StartsWith(ing_name))
                       select o;  
            foreach (var o in family)
            {
                if (o.gameObject.CompareTag("MembraneChain")) {
                    foreach (Transform ch in o)
                    {
                        pg.RemoveIngredient(ch.gameObject);
                    }
                    Manager.Instance.DestroyHierarchy(o);
                    //GameObject.Destroy(o.gameObject);
                }
                else {
                    pg.RemoveIngredient(o.gameObject);
                    GameObject.DestroyImmediate(o.gameObject);
                }
            }
        }
        /*same for the group instance*/
        var familyg = Manager.Instance.root.transform.GetComponentsInChildren<PrefabGroup>();
        //        from o in Manager.Instance.root.transform.GetComponentsInChildren<PrefabGroup>()
        //        where o.name == name
        //        select o.gameObject;
        foreach (var o in familyg)
        {
            PrefabGroup pg = o;//.GetComponent<PrefabGroup>();
            var childfamily =
                       from oc in o.transform.GetComponentsInChildren<Transform>()
                       where (oc!=null && oc.name.StartsWith(ing_name))
                       select oc;  
            foreach (var oc in childfamily)
            {
                if (oc.gameObject.CompareTag("MembraneChain")) {
                    foreach (Transform ch in oc)
                    {
                        pg.RemoveIngredient(ch.gameObject);
                    }
                    Manager.Instance.DestroyHierarchy(oc);
                }
                else {
                    pg.RemoveIngredient(oc.gameObject);
                    Manager.Instance.DestroyInstance(oc.gameObject);
                }
            }
        }               
        foreach (var o in familyg)
        {
            Debug.Log("RemoveIngredientFromGroups "+o.gameObject.name+" "+o.transform.childCount.ToString());
            if (o.transform.childCount == 0 ) {
                //remove the group
                GameObject.DestroyImmediate(o.gameObject);
            }
        }
        for(int i=groupnames.Count - 1; i > -1; i--)
        {
            var agname=groupnames[i];
            Debug.Log(i.ToString()+" "+agname);
            if (Manager.Instance.all_prefab[agname] == null) {
                Debug.Log(i.ToString()+" RemoveIngredientFromGroups prefab "+agname+" already deleted ?");
                Manager.Instance.all_prefab.Remove(agname);
                groupnames.RemoveAt(i);
            }
            else {
                var prefab = Manager.Instance.all_prefab[agname];
                Debug.Log(i.ToString()+" RemoveIngredientFromGroups prefab "+agname+" "+prefab.transform.childCount.ToString());
                if (prefab.transform.childCount == 0 ) {
                    Manager.Instance.recipeUI.RemoveIngredient(agname,-1,false);
                    Debug.Log(i.ToString()+" "+agname+" "+groupnames.Count.ToString());
                    if (groupnames.Contains(agname)) groupnames.RemoveAt(i);
                }
            }
        }
    }

    public void RemoveIngredientFromGroups(int ing_id){
        string ing_name = Manager.Instance.ingredients_ids[ing_id];
        RemoveIngredientFromGroups(ing_name);
    }

    public void RestoreOneGroup(string group_name, List<GameObject> selection){
        current_selections = selection;
        var bb = getBound();
        var emptyGroup = Instantiate(prefabGroup, Vector3.zero, Quaternion.identity);
        PrefabGroup g = emptyGroup.GetComponent<PrefabGroup>();
        g.name = group_name;
        g.instance_id = 0;
        emptyGroup.layer = 9;//midle layer == group
        emptyGroup.name = group_name+"_0";
        emptyGroup.transform.position = bb.center;
        emptyGroup.transform.parent = current_selections[0].transform.parent;//root or compartment
        //if the parent was already a group use his parent
        //if (current_selections[0].transform.parent.GetComponent<PrefabGroup>()!= null)
        //{
        //    emptyGroup.transform.parent = current_selections[0].transform.parent.parent;
        //}
        List<Transform> parents = new List<Transform>();
        foreach (var o in current_selections)
        {
            parents.Add(o.transform.parent);
            o.transform.parent = emptyGroup.transform;
            //take care of the attachments
            if (Manager.Instance.fiber_parents.Contains(o)) {
                for (int i = 0; i < o.transform.childCount; i++) {
                    var current_o = o.transform.GetChild(i).gameObject;
                    CheckAttached(current_o,g);
                    /*var allspring = current_o.GetComponents<SpringJoint2D>();
                    for (int s = 0; s < allspring.Length; s++)
                    {
                        var spr = allspring[s];
                        if (IsAttachedToSelection(spr.connectedBody.gameObject)){
                            if (g.attachements.Contains(spr)) continue;
                            g.attachements.Add(spr);
                        }
                        else {
                            if (g.attachements_toskip.Contains(spr)) continue;
                            g.attachements_toskip.Add(spr);
                        }
                    }*/
                }
            }
            else
            {
                CheckAttached(o,g);
                /*var allspring = o.GetComponents<SpringJoint2D>();
                foreach (var spr in allspring)
                {
                    if (IsAttachedToSelection(spr.connectedBody.gameObject)){
                        if (g.attachements.Contains(spr)) continue;
                        g.attachements.Add(spr);
                    }
                    else {
                        if (g.attachements_toskip.Contains(spr)) continue;
                        g.attachements_toskip.Add(spr);
                    }
                }*/
            }
        }
        var compartment = "root";
        PrefabProperties childprop = current_selections[0].GetComponent<PrefabProperties>();
        if (childprop==null){
            //get the first child that should be a fiber
            childprop=current_selections[0].transform.GetChild(0).GetComponent<PrefabProperties>();
        }
        if (childprop!=null) compartment = childprop.compartment;
        emptyGroup.GetComponent<PrefabProperties>().compartment = compartment;
        //Create the prefab
        var newGroup = Instantiate(emptyGroup, Vector3.zero, Quaternion.identity);
        PrefabGroup p = newGroup.GetComponent<PrefabGroup>();
        p.name = group_name;
        foreach (var sjt in p.attachements_toskip){
            DestroyImmediate(sjt);
        }
        p.attachements_toskip.Clear();
        groupnames.Add(p.name);
        groupinstances.Add(p.name,0);
        Manager.Instance.all_prefab.Add(p.name, newGroup);
        p.bound = bb;//world bb?
        //do we fixthem? kinematics ?
        var new_list = new List<GameObject>();
        for (int o = 0; o < current_selections.Count; o++) {
            newGroup.transform.GetChild(o).position = current_selections[o].transform.position  - new Vector3(p.bound.center.x,p.bound.center.y,0.0f);
            if (Manager.Instance.fiber_parents.Contains(current_selections[o])) {
                for (int i = 0; i < current_selections[o].transform.childCount; i++) {
                    var current_o = current_selections[o].transform.GetChild(i).gameObject;
                    PrefabProperties props1 = current_o.GetComponent<PrefabProperties>();
                    props1.UpdateOutline(false);
                }
            }
            else
            {
                var newObj = newGroup.transform.GetChild(o).gameObject;
                PrefabProperties props = newObj.GetComponent<PrefabProperties>();
                props.UpdateOutline(false);
            }
        }
        newGroup.transform.position = new Vector3(1000, 0, 0);//put it out of the camera view
        newGroup.gameObject.SetActive(false);
        //the prefab is a an empty object parenting copy of all the selected object.
        //forwhich we need a bounding box so we can center it
        Manager.Instance.recipeUI.AddOneGroup(group_name,-1,compartment);
        current_selections.Clear();
        nGroup++;
    }
    
    public void RestoreOneInstance(string group_name, int group_id, List<GameObject> selection){
        current_selections = selection;
        var bb = getBound();
        var emptyGroup = Instantiate(prefabGroup, Vector3.zero, Quaternion.identity);
        emptyGroup.gameObject.SetActive(true);
        PrefabGroup gr = emptyGroup.GetComponent<PrefabGroup>();
        gr.name = group_name;
        gr.instance_id = group_id;
        gr.bound = bb;
        emptyGroup.name = group_name+"_"+group_id;
        emptyGroup.layer = 22;//midle layer == group
        emptyGroup.transform.position = bb.center;
        emptyGroup.transform.parent = current_selections[0].transform.parent;//root or compartment
        foreach (var o in current_selections)
        {
            o.transform.parent = emptyGroup.transform;
        }        
    }

    public void RestoreGroups(){
        //loop through the dictionary and then clean it
        foreach(var KeyValue in groups){
            string gname = KeyValue.Key; //gname
            Group group = KeyValue.Value; //selection
            foreach(var kv in group.childs){
                int group_id = kv.Key;
                List<GameObject> selections = kv.Value;
                if (!groupnames.Contains(gname)){
                    //restore group
                    RestoreOneGroup(gname, selections);
                }
                else {
                    //restore instance
                    RestoreOneInstance(gname, group_id, selections);
                }
            }
        }
        groups.Clear();
    }
}
