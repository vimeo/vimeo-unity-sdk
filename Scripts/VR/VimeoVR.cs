using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VimeoVR : MonoBehaviour {

	// Use this for initialization
	private bool initControllers = false;

	void Start () {
		
	}
		
	void Update () {
		
	}

	void LateUpdate() {
		InitVRComponents();
	}

	private void InitVRComponents()
	{
		if (!initControllers) {
			Debug.Log ("VimeoVR init");
			var controllers = Resources.FindObjectsOfTypeAll<SteamVR_TrackedObject> ();

			foreach (SteamVR_TrackedObject controller in controllers) {
				if (controller.gameObject.GetComponent<VimeoVR_Controller> () == null && !controller.gameObject.name.Contains("head")) {
					controller.gameObject.AddComponent<VimeoVR_Controller> ();
				}

				initControllers = true;
			}
		}
	}
}
