using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vimeo {
	
	public class TriggerVRControls : MonoBehaviour {

		public GameObject vimeoPlayer;

		void Update () {
			if (Input.GetMouseButtonDown (0)) {
				Camera camera = GameObject.FindObjectOfType<Camera>();

				RaycastHit hit;
				Ray ray = camera.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(ray, out hit)) {
					if (hit.transform.GetInstanceID() == GetInstanceID()) {
						Debug.Log ("video clicked!");
					}
				}
			}
		}
	}

}