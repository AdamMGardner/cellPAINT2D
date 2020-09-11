using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using Vexe.FastSave;
//using Vexe.Runtime.Types;
//using FSExamples;
using System;
using System.IO;
//using BX20Serializer;


public class UndoStack: MonoBehaviour// : BaseBehaviour
{
    /*
    public Button undoBtn;
    public Button redoBtn;
    public int stack_size = 10;
    public int current_stack_size;
    public int current_redo_size;
    public GameObject target;
    public bool CSrun = false;
    public bool coroutine = false;
    private Stack<string> stacks_undo;
    private Stack<string> stacks_redo;
    private int current_states = 0;
    private int size_states = 0;
    private MemoryStream stream;
    //use serialisation + script to reattach all springs

    // Use this for initialization
    void Start () {
        stacks_undo = new Stack<string>(stack_size);
        stacks_redo = new Stack<string>(stack_size);
        target = Manager.Instance.root;
        stream = new MemoryStream();
        stream.SetLength(0);
        stream.Position = 0;
    }
    
    public void HierarchyToMemory(GameObject root)
    {
        var ms = new MemoryStream();
        //stream.SetLength(0);
        //stream.Position = 0;
        HierarchyToStream(ms, root);
        
       
    }

    public void HierarchyToStream(MemoryStream stream, GameObject root)
    {
        var children = root.GetComponentsInChildren<Transform>();
        stream.WriteInt(children.Length);
        Save.GameObjectToStream(stream, root);
        StartCoroutine(RecurseHierarchy(stream, root, children, 0));
    }

    IEnumerator RecurseHierarchy(MemoryStream stream, GameObject root, Transform[] children, int rootDepth)
    {
        CSrun = true;
        var rootTransform = root.transform;
        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];
            if (child.parent == rootTransform)
            {
                var go = child.gameObject;
                var currentDepth = rootDepth + 1;
                Log(go + ": " + currentDepth);
                stream.WriteInt(currentDepth);
                Save.GameObjectToStream(stream, go);
                Save.RecurseHierarchy(stream, go, go.GetComponentsInChildren<Transform>(), currentDepth);
            }
            yield return null;
        }
        CSrun = false;
        stacks_undo.Push(stream.ToArray().ToString());
    }


    // Update is called once per frame
    void Update () {
        current_stack_size = stacks_undo.Count;
        current_redo_size = stacks_redo.Count;
        if (Input.GetMouseButtonDown(0) && !Manager.Instance.mask_ui) {
            //add states into stack
            //SetFSReferences();//need to corresponds
            //Save.HierarchyToMemory(root);
            if (coroutine)
            {
                if (!CSrun) HierarchyToMemory(target);
                else Debug.Log("already writing");
            }
            else {
                string output = target.SaveHierarchyToMemory().GetString();
                stacks_undo.Push(output);
            }
        }
        undoBtn.interactable = stacks_undo.Count != 0;
        redoBtn.interactable = stacks_redo.Count != 0;
        current_stack_size = stacks_undo.Count;
    }

    void enableButton() {
        undoBtn.interactable = true;
        redoBtn.interactable = true;
    }

    void disableButton() {
        undoBtn.interactable = false;
        redoBtn.interactable = false;
    }

    //FSReference for chains
    public void SetFSReferences()
    {
        foreach (Transform child in Manager.Instance.root.transform)
        {
            if ((child.GetComponent<DrawMeshContour>() != null) || (child.name.Contains("_chain_")))
            {
                //persistence
                if (child.transform.childCount == 0) continue;
                PrefabProperties props = child.transform.GetChild(0).GetComponent<PrefabProperties>();
                for (int i = 0; i < child.transform.childCount - 1; i++)// (Transform ch_elem in child.transform)
                {
                    FSReference fsr = child.transform.GetChild(i).GetComponent<FSReference>();
                    if (fsr == null) fsr = child.transform.GetChild(i).gameObject.AddComponent<FSReference>();
                }
            }
        }
    }

       //ienumerate ? 
    public void restoreChainSprings() {
        //foreach (Transform child in Manager.Instance.root.transform)
        for (int c = 0; c < Manager.Instance.root.transform.childCount - 1;c++)
        {
            var child = Manager.Instance.root.transform.GetChild(c);
            if ((child.GetComponent<DrawMeshContour>() != null) || (child.name.Contains("_chain_"))) {
                //persistence
                PrefabProperties props = child.transform.GetChild(0).GetComponent<PrefabProperties>();
                for (int i=0;i < child.transform.childCount-1;i++)// (Transform ch_elem in child.transform)
                {
                    var ch_elem = child.transform.GetChild(i);
                    HingeJoint2D hjt = ch_elem.GetComponent<HingeJoint2D>();
                    var pos = ch_elem.position;
                    var rotation = ch_elem.rotation;
                    hjt.connectedBody = child.transform.GetChild(i+1).GetComponent<Rigidbody2D>();
                    SpringJoint2D[] sjts = ch_elem.GetComponents<SpringJoint2D>();//only one ?
                    foreach (var aj in sjts) Destroy(aj);
                    //persitence length with spring
                    int nchild = child.childCount;
                    for (int j = 0; j < props.persistence_length; i++)
                    {
                        continue;
                        //we go backward
                        if (nchild < j) continue;
                        if (nchild - (j + 1) < 0) continue;
                        var ch = child.transform.GetChild(nchild - (j + 1));
                        SpringJoint2D spring = ch.gameObject.AddComponent<SpringJoint2D>();
                        spring.connectedBody = ch_elem.GetComponent<Rigidbody2D>();
                        spring.enableCollision = props.enableCollision;
                        spring.autoConfigureDistance = false;
                        // CircleCollider2D[] allc = ch_elem.GetComponents<CircleCollider2D>();
                        spring.distance = props.fiber_length * (j + 1);// + UnityEngine.Random.Range(0.0f, fiber_length / 10.0f);
                        spring.anchor = Vector2.zero;// allc[1].offset;
                        spring.connectedAnchor = Vector2.zero;//allc[0].offset;
                        spring.frequency = 10.0f / ((j + 1) / 2.0f);
                        spring.dampingRatio = 0.5f;
                    }
                    ch_elem.position = pos;
                    ch_elem.rotation = rotation;
                }
                if (child.name.Contains("_Closed")) {
                    Transform first = child.transform.GetChild(0);
                    Transform last = child.transform.GetChild(child.transform.childCount - 1);
                    HingeJoint2D hinge = last.gameObject.AddComponent<HingeJoint2D>();
                    hinge.connectedBody = first.gameObject.GetComponent<Rigidbody2D>();
                    int st = 0;
                    int end = child.transform.childCount - 1;
                    for (int l = 0; l < props.persistence_length; l++)
                    {
                        continue;
                        int i = 0;//i<l+1
                        for (int k = l; k >= 0; k--)
                        {
                            //if (i < i + 1) continue;
                            //Debug.Log((st + i).ToString()+" attached to "+ (end - k).ToString());
                            var ch1 = child.transform.GetChild(st + i);
                            var ch2 = child.transform.GetChild(end - k);
                            SpringJoint2D spring = ch2.gameObject.AddComponent<SpringJoint2D>();
                            spring.connectedBody = ch1.gameObject.GetComponent<Rigidbody2D>();
                            ch1.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                            ch2.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                            spring.enableCollision = props.enableCollision;
                            spring.autoConfigureDistance = false;
                            //CircleCollider2D[] allc = ch1.gameObject.GetComponents<CircleCollider2D>();
                            spring.distance = props.fiber_length * (l + 1);// + UnityEngine.Random.Range(-fiber_length / 10.0f, fiber_length / 10.0f);
                            spring.anchor = Vector2.zero;// allc[1].offset;
                            spring.connectedAnchor = Vector2.zero;//allc[0].offset;
                            spring.frequency = 10.0f / ((l + 1) / 2.0f);
                            spring.dampingRatio = 0.5f;
                            i++;
                        }
                    }
                }
            }
        }
    }

    IEnumerator serialize() {
        string output = target.SaveHierarchyToMemory().GetString();
        stacks_undo.Push(output);
        yield return null;
    }

    public void UndoStates() {
        Manager.Instance.Clear();
        string ouput1 = stacks_undo.Pop();
        Load.HierarchyFromMemory(ouput1.GetBytes(), target);
        //restoreChainSprings();
        stacks_redo.Push(ouput1);
    }

    public void RedoStates() {
        Manager.Instance.Clear();
        string ouput1 = stacks_redo.Pop();
        Load.HierarchyFromMemory(ouput1.GetBytes(), target);
        //restoreChainSprings();
        stacks_undo.Push(ouput1);
    }

    */
}
