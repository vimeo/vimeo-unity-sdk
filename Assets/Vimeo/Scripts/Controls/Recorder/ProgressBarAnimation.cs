using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Vimeo.Controls
{
    public class ProgressBarAnimation : MonoBehaviour
    {
        private float offset = 0f;

        void Update()
        {
            GetComponent<RawImage> ().uvRect = new Rect (offset, 0, 3, 1);
            offset += 0.01f;
        }
    }
}

