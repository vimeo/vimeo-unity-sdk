using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vimeo;

namespace Vimeo.VR {
	public class VimeoVR_PlayerControls : MonoBehaviour {

		private VimeoPlayer vimeoPlayer;

		void Update () {
		}

		public void Init(VimeoPlayer player) {
			Camera cam = GameObject.FindObjectOfType<Camera> ();
			transform.position = cam.transform.position + cam.transform.forward * 1f;
			transform.Translate(-Vector3.up * 0.1f);

			vimeoPlayer = player;
			//GetComponentInChildren<PlayButton> ().Init(vimeoPlayer);
		}
	}
}