using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vimeo;

public class PlayButton : MonoBehaviour {

	private VimeoPlayer vimeoPlayer;
		
	public void Init (VimeoPlayer player) {
		vimeoPlayer = player;
	}

	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit)) {
				if (hit.transform.name == name) {
					vimeoPlayer.ToggleVideoPlayback ();
				}
			}
		}
	}
}
