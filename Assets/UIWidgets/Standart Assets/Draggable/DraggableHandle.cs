using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace UIWidgets {
	/// <summary>
	/// Draggable handle.
	/// </summary>
	public class DraggableHandle : MonoBehaviour, IDragHandler, IInitializePotentialDragHandler {
		RectTransform drag;
		Canvas canvas;
		RectTransform canvasRect;

		/// <summary>
		/// Set the specified draggable object.
		/// </summary>
		/// <param name="newDrag">New drag.</param>
		public void Drag(RectTransform newDrag)
		{
			drag = newDrag;
		}
		
		/// <summary>
		/// Raises the initialize potential drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			canvasRect = Utilites.FindCanvas(transform) as RectTransform;
			canvas = canvasRect.GetComponent<Canvas>();
		}
		
		/// <summary>
		/// Raises the drag event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnDrag(PointerEventData eventData)
		{
			if (canvas==null)
			{
				throw new MissingComponentException(gameObject.name + " not in Canvas hierarchy.");
			}
			Vector3 cur_pos;
			Vector3 prev_pos;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(drag, eventData.position, eventData.pressEventCamera, out cur_pos);
			RectTransformUtility.ScreenPointToWorldPointInRectangle(drag, eventData.position - eventData.delta, eventData.pressEventCamera, out prev_pos);
			var delta = cur_pos - prev_pos; 
			var new_pos = new Vector3(
				drag.position.x + (cur_pos.x - prev_pos.x),
				drag.position.y + (cur_pos.y - prev_pos.y),
				drag.position.z);
			//parentCanvasOfImageToMove.transform.TransformPoint(pos) 
			drag.position = new_pos;
			//drag.position = drag.transform.parent.transform.TransformPoint(new_pos);
			Manager.Instance.mask_ui = true;
		}

		public void OnBeginDrag(PointerEventData data)
		{
			Debug.Log("OnBeginDrag called.");
			Manager.Instance.mask_ui = true;
		}

		public void OnEndDrag(PointerEventData data)
		{
			Debug.Log("OnEndDrag called.");
			Manager.Instance.mask_ui = false;
		}
	}
}