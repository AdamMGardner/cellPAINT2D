using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIWidgets;
using UIWidgetsSamples;
using UnityEngine.EventSystems;

namespace UnityStandardAssets.Utility
{
    public class DragRigidbody2D : MonoBehaviour
    {
        public float frequency;
        public float damping;
        public bool dragMode = false;
        public bool pinMode = false;

        const float k_Spring = 50.0f;
        const float k_Damper = 5.0f;
        const float k_Drag = 10.0f;
        const float k_AngularDrag = 5.0f;
        const float k_Distance = 0.2f;
        const bool k_AttachToCenterOfMass = false;

        private SpringJoint2D m_SpringJoint;
        private bool isKin;
        private GameObject below;
        private List<PrefabProperties> pinned_object;

        void OnEnable() {
            pinned_object = new List<PrefabProperties>();
        }

        private void Clean()
        {
            if (below)
            {
                if (m_SpringJoint && m_SpringJoint.connectedBody != null)
                    return;
                below.GetComponent<PrefabProperties>().UpdateOutline(false);
                below = null;
            }
        }

        public void togglePinOutline(bool toggle) {
            foreach (PrefabProperties properties in pinned_object) {
                properties.UpdateOutlinePin(toggle);
            }
        }

        private void Update()
        {
            if (!dragMode && !pinMode)
            {
                return;
            }
            
            var mainCamera = FindCamera();
            // We need to actually hit an object
            LayerMask layerMask = ~(1 << LayerMask.NameToLayer("CameraCollider") | 1 << LayerMask.NameToLayer("FiberPushAway")); // ignore both layerX and layerY
            RaycastHit2D hit = new RaycastHit2D();
            hit = Physics2D.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition).origin,
                                 mainCamera.ScreenPointToRay(Input.mousePosition).direction, 100,
                                 layerMask);

            if (!hit)
            {
                Clean();
                return;
            }

            // We need to hit a rigidbody that is not kinematic
            if (!hit.collider.gameObject.GetComponent<Rigidbody2D>())
            {
                Clean();
                return;
            }

            PrefabProperties p;
            if (!Input.GetMouseButton(0))
            {
                Clean();
                below = hit.collider.gameObject;
                
                p = below.GetComponent<PrefabProperties>();
                if (p)
                {
                    below.GetComponent<PrefabProperties>().outline_width = mainCamera.orthographicSize;
                    below.GetComponent<PrefabProperties>().UpdateOutline(true);
                }
                else {
                    below = null;
                }  
            }
            if (!Input.GetMouseButtonDown(0)) return;

            if (pinMode)
            {
                Debug.Log(below.name);
                if (below == gameObject) return;
                p = below.GetComponent<PrefabProperties>();
                below.GetComponent<Rigidbody2D>().isKinematic = !below.GetComponent<Rigidbody2D>().isKinematic;
                p.ispined = !p.ispined;
                p.UpdateOutlinePin(p.ispined);
                if (p.ispined)
                    pinned_object.Add(p);
                else
                    pinned_object.Remove(p);
                Debug.Log("Pinned?");
            }
            else if (dragMode)
            {
                if (!m_SpringJoint)
                {
                    var go = new GameObject("Rigidbody dragger");
                    Rigidbody2D body = go.AddComponent<Rigidbody2D>();
                    m_SpringJoint = go.AddComponent<SpringJoint2D>();
                    body.isKinematic = true;
                }
                m_SpringJoint.transform.position = hit.point;
                m_SpringJoint.anchor = Vector3.zero;
                m_SpringJoint.autoConfigureDistance = false;
                m_SpringJoint.connectedBody = hit.collider.gameObject.GetComponent<Rigidbody2D>();
                isKin = m_SpringJoint.connectedBody.isKinematic;
                m_SpringJoint.connectedBody.isKinematic = false;
                m_SpringJoint.distance = 0.0f;
                StartCoroutine("DragObject");
            }
        }


        private IEnumerator DragObject()
        {
            var oldDrag = m_SpringJoint.connectedBody.drag;
            var oldAngularDrag = m_SpringJoint.connectedBody.angularDrag;
            m_SpringJoint.connectedBody.drag = k_Drag;
            m_SpringJoint.connectedBody.angularDrag = k_AngularDrag;

            m_SpringJoint.dampingRatio = damping;
            m_SpringJoint.frequency = frequency;

            var mainCamera = FindCamera();
            while (Input.GetMouseButton(0))
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10.0f;       // we want the ingredient offset from the camera position

                m_SpringJoint.transform.position = mainCamera.ScreenToWorldPoint(mousePos);
                yield return null;
            }
            if (m_SpringJoint.connectedBody)
            {
                m_SpringJoint.connectedBody.drag = oldDrag;
                m_SpringJoint.connectedBody.angularDrag = oldAngularDrag;
                m_SpringJoint.connectedBody.isKinematic = isKin;
                m_SpringJoint.connectedBody = null;
            }
        }

        private void ToggleFreezePrefab(GameObject name)
        {
            Rigidbody2D rb = name.GetComponent<Rigidbody2D>();
            if (rb.constraints == RigidbodyConstraints2D.None)
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            else
                rb.constraints = RigidbodyConstraints2D.None;
        }


        private Camera FindCamera()
        {
            if (GetComponent<Camera>())
            {
                return GetComponent<Camera>();
            }

            return Camera.main;
        }
    }
}
