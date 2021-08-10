using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabGroup : MonoBehaviour
{
    public string name;
    public int instance_id;
    public Bounds bound;
    public List<SpringJoint2D> attachements = new List<SpringJoint2D>();
    public List<SpringJoint2D> attachements_toskip = new List<SpringJoint2D>();
    public float getRadius(){
        return Mathf.Max(Mathf.Max(bound.extents.x,bound.extents.y),bound.extents.z)*2.0f;
    }

    public void RemoveIngredient(GameObject ingredient) {
        attachements.RemoveAll(elem => elem.gameObject == ingredient || elem.connectedBody.gameObject == ingredient );
        for(int i=attachements.Count - 1; i > -1; i--)
        {
            var jt = attachements[i];
            var attach1 = attachements[i].gameObject;
            var attach2 = attachements[i].connectedBody.gameObject;
            if (attach1 == ingredient || attach2 == ingredient) {
                Destroy(jt);
            }
        }
    }
}
