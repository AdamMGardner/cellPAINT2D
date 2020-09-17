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
}
