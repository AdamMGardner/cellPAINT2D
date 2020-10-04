using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasureManager : MonoBehaviour
{
    public LineRenderer measure_line;
    public GameObject measure_line_holder;
    public TextMesh measure_label;
    public Vector3 start_position;
    public Vector3 end_position;
    public Dictionary<string,LineRenderer> stored_measure_lines = new Dictionary<string, LineRenderer>();
    public GameObject line_holder;
    public bool keep_alive = true;
    private System.Random random_uid = new System.Random();
    private static MeasureManager _instance = null;
    public static MeasureManager Get
    {
        get
        {
            if (_instance != null) return _instance;
            //currently in LoadImage_Panel
            _instance = FindObjectOfType<MeasureManager>();
            if (_instance == null)
            {
                var go = GameObject.Find("MeasureManager");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("MeasureManager");
                _instance = go.AddComponent<MeasureManager>();
                //_instance.hideFlags = HideFlags.HideInInspector;
            }
            return _instance;
        }
    }

    public void ToggleLineAndLAbel(bool value){
        //should create/delete the lineRendere and see if help with the webGL
        Debug.Log("IN ToggleLineAndLAbel");
        //measure_line.enabled = value;
        measure_line_holder.gameObject.SetActive(value);
        if (value){
            measure_line = measure_line_holder.AddComponent<LineRenderer>();
            measure_line.positionCount = 2;
            measure_line.sharedMaterial = Manager.Instance.lineMat;
            measure_line.sortingOrder = 5;
            measure_line.widthMultiplier = 0.6f;
            measure_line.numCapVertices = 2;
            measure_line.SetPosition(0, new Vector3(10000,0,0));
            measure_line.SetPosition(1, new Vector3(10000,0,0));            
        }
        else {
            Destroy(measure_line);
        }
        measure_label.gameObject.SetActive(value);
        //measure_line.SetPosition(0, new Vector3(10000,0,0));
        //measure_line.SetPosition(1, new Vector3(10000,0,0));
        measure_label.text = "";
        Debug.Log("KEEP ALIVe ?");
        if (keep_alive) line_holder.SetActive(true);
    }

    public void ToggleAllLine(bool value) {
        line_holder.SetActive(value);
        keep_alive = value;
    }
    
    public void ClearLines(){
        foreach(var KeyValue in stored_measure_lines) {
            Destroy(KeyValue.Value.gameObject);
        }
        stored_measure_lines.Clear();
    }

    public void ClearOneLine(string lname){
        var g = stored_measure_lines[lname].gameObject;
        stored_measure_lines.Remove(lname);
        Destroy(g);
    }

    public void StoreCurrentLine(){
        var csize = Manager.Instance.current_camera.orthographicSize;
        var cid = random_uid.Next();
        var name = "lines_"+cid.ToString();
        var g = new GameObject(name);
        g.transform.parent = line_holder.transform;
        var line = g.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.sharedMaterial = Manager.Instance.lineMat;
        line.sortingOrder = 5;
        line.widthMultiplier = 0.6f;
        line.numCapVertices = 2;
        line.SetPosition(0, start_position);
        line.SetPosition(1, end_position);
        var textmesh = g.AddComponent<TextMesh>();
        float d = Vector3.Distance(start_position, end_position) * Manager.Instance.unit_scale;
        textmesh.text = d.ToString("####0.00")+"nm";
        textmesh.anchor = measure_label.anchor;
        textmesh.alignment = measure_label.alignment;
        if (d > 10000.0f) measure_label.text = (d*0.001f).ToString("####0.00")+"microns";
        var mid_point = (end_position - start_position)/2.0f;
        var offset = Vector3.Normalize( Vector3.Cross(mid_point,Vector3.forward) );
        var bounds = measure_label.GetComponent<MeshRenderer>().bounds;
        var off = new Vector3(offset.x*bounds.extents.x*1.25f,offset.y*bounds.extents.y*2.0f,offset.z);
        g.transform.position = start_position+mid_point+off;
        textmesh.fontSize = Mathf.CeilToInt ( csize/2.0f );   
        stored_measure_lines.Add(name,line);
    }

    public void DoMeasure(){
        Vector3 current_pos = Manager.Instance.transform.position;
        var csize = Manager.Instance.current_camera.orthographicSize;
        if (Input.GetMouseButtonDown(0)){
            //ToggleLineAndLAbel(true);
            start_position = current_pos;
            measure_line.SetPosition(0, start_position);
            measure_line.SetPosition(1, start_position);
        }
        else if (Input.GetMouseButton(0)){
            measure_line.SetPosition(1, current_pos);
            float d = Vector3.Distance(start_position,current_pos) * Manager.Instance.unit_scale;
            measure_label.text = d.ToString("####0.00")+"nm";
            if (d > 10000.0f) measure_label.text = (d*0.001f).ToString("####0.00")+"microns";
            var mid_point = (current_pos - start_position)/2.0f;
            var offset = Vector3.Normalize( Vector3.Cross(mid_point,Vector3.forward) );
            var bounds = measure_label.GetComponent<MeshRenderer>().bounds;
            var off = new Vector3(offset.x*bounds.extents.x*1.25f,offset.y*bounds.extents.y*2.0f,offset.z);
            measure_label.transform.position =start_position+mid_point+off;
            measure_label.fontSize = Mathf.CeilToInt ( csize/2.0f );
        }
        else if (Input.GetMouseButtonUp(0)){
            end_position = current_pos;
            measure_line.SetPosition(1, end_position);
        }
        if (Input.GetKeyDown(KeyCode.Return)){
            StoreCurrentLine();
        }
    }
}
