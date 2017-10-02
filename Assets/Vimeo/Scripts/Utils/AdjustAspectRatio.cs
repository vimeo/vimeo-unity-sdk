using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vimeo;


namespace Vimeo.Fun {
	public class AdjustAspectRatio : MonoBehaviour {
		
		private VimeoPlayer vimeoPlayer;
		private bool isLoaded = false;
		private float targetHeight;

		void Start () {
			vimeoPlayer = GetComponent<VimeoPlayer>();	
			vimeoPlayer.OnVideoStart += OnVideoStart;
		}

		void OnDisable()
		{
			vimeoPlayer.OnVideoStart -= OnVideoStart;
		}

		private void OnVideoStart()
		{
			if (vimeoPlayer.GetWidth () > vimeoPlayer.GetHeight ()) {
				targetHeight = ((float)vimeoPlayer.GetHeight () / vimeoPlayer.GetWidth ()) * transform.localScale.x;
			} else {
				targetHeight = ((float)vimeoPlayer.GetWidth () / vimeoPlayer.GetHeight ()) * transform.localScale.x;
			}

			isLoaded = true;
		}

		void Update () {
			if (targetHeight > 0 && isLoaded) {
				transform.localScale = new Vector3(
					transform.localScale.x,
					Mathf.Lerp(transform.localScale.y, targetHeight, Time.deltaTime * 8f),
					transform.localScale.z
				);
			}
		}
	}
}