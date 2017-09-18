using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Vimeo.Misc
{
	public class RotateObject : MonoBehaviour {

		private float animationSpeed = 5f;

		void Start () {
		}

		void Update () {
			transform.Rotate(new Vector3(5f, 3f, 2f));
		}
	}
}