using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vimeo;

public class VimoeVR_CanvasControls : MonoBehaviour {

	private VimeoPlayer vimeoPlayer;

	public void Init(VimeoPlayer player) {
		Camera cam = GameObject.FindObjectOfType<Camera> ();
		transform.position = cam.transform.position + cam.transform.forward * 1f;
		transform.Translate(-Vector3.up * 0.1f);

		vimeoPlayer = player;
		//GetComponentInChildren<PlayButton> ().Init(vimeoPlayer);
	}
}
