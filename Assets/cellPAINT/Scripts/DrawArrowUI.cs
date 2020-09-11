using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawArrowUI : MonoBehaviour
{
    [Tooltip("The percent of the line that is consumed by the arrowhead")]
    [Range(0, 1)]
    public float PercentHead = 0.4f;
    public RectTransform target;
    public RectTransform source;
    public Vector3 ArrowOrigin;
    public Vector3 ArrowTarget;
    public LineRenderer arrow;
    public int mode;//0 up,1 down,
    // Start is called before the first frame update
    void setupLine(){
        if (source == null){
            source = gameObject.GetComponent<RectTransform>();
        }
        if (arrow==null){
            arrow = gameObject.AddComponent<LineRenderer>();
            arrow.sharedMaterial = Manager.Instance.lineMat;
            arrow.sortingOrder = 2;
            arrow.widthMultiplier = 0.3f;
            arrow.numCapVertices = 5;       
        }        
    }

    void UpateCorners(){
        //Each corner provides its world space value. The returned array of 4 vertices is clockwise. 
        //It starts bottom left and rotates to top left, then top right, and finally bottom right. 
        //Note that bottom left, for example, is an (x, y, z) vector with x being left and y being bottom.
        Vector3[] v1 = new Vector3[4];
        source.GetWorldCorners(v1);
        Vector3[] v2 = new Vector3[4];
        target.GetWorldCorners(v2);
        if (mode == 0){
            ArrowOrigin = (v1[0]+v1[1])/2.0f;
            ArrowTarget = (v2[2]+v2[3])/2.0f;
        }
        else if (mode == 1){
            ArrowOrigin = v1[0];
            ArrowTarget = (v2[1]+v2[2])/2.0f;
        }
        ArrowOrigin = Camera.main.ScreenToWorldPoint(ArrowOrigin);
        ArrowTarget = Camera.main.ScreenToWorldPoint(ArrowTarget);
    }
    
    [ContextMenu("UpdateArrow")]
    void UpdateArrow()
    {
        UpateCorners();
        float AdaptiveSize = (float)(PercentHead / Vector3.Distance(ArrowOrigin, ArrowTarget));
        arrow.widthCurve = new AnimationCurve(
            new Keyframe(0, 0.4f)
            , new Keyframe(0.999f - AdaptiveSize, 0.4f)  // neck of arrow
            , new Keyframe(1 - AdaptiveSize, 1f)  // max width of arrow head
            , new Keyframe(1, 0f));  // tip of arrow
        arrow.SetPositions(new Vector3[] {
            ArrowOrigin
            , Vector3.Lerp(ArrowOrigin, ArrowTarget, 0.999f - AdaptiveSize)
            , Vector3.Lerp(ArrowOrigin, ArrowTarget, 1 - AdaptiveSize)
            , ArrowTarget });
    }

    void OnEnable(){
        setupLine();
    }
    void Start()
    {
        setupLine();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
