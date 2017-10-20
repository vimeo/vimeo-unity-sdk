using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Vimeo.Misc
{
	public class RotateObject : MonoBehaviour {

        public Vector3 rotate;

        public bool randomStartX;
        public bool randomStartY;
        public bool randomStartZ;

		void Start () {
            if (randomStartX) {
                transform.Rotate(new Vector3(Random.Range(0, 360), 0, 0));
            }

            if (randomStartZ) {
                transform.Rotate(new Vector3(0, 0, Random.Range(0, 360)));
            }

            if (randomStartY) {
                transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
            }
		}

		void Update () {
            transform.Rotate(rotate);
		}
	}
}