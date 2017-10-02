using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Vimeo.Misc
{
	public class RotateObject : MonoBehaviour {

		void Start () {
		}

		void Update () {
			transform.Rotate(new Vector3(1f, 0, 0));
		}
	}
}