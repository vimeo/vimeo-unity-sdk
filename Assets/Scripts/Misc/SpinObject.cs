using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Vimeo.Misc
{
    public class SpinObject : MonoBehaviour {

        private float targetWidth = 10f;
        private float targetHeight;
        private float targetDepth = 0.5f;
        private float animationSpeed = 5f;

        public GameObject playerObj;
        private VimeoPlayer player;
        private bool isPlaying = true;

        void Start () {
            player = playerObj.GetComponent<VimeoPlayer>();
            player.OnVideoStart  += OnVideoStart;
            player.OnPlay  += OnPlay;
            player.OnPause += OnPause;

            transform.localScale = new Vector3(0f, 0f, 0f);
        }

        private void OnVideoStart()
        {
            float ratio = (float)player.GetHeight() / player.GetWidth();
            targetHeight = ratio * targetWidth;
            isPlaying = true;
        }

        private void OnPlay()
        {
            isPlaying = true;
        }

        private void OnPause()
        {
            isPlaying = false;
        }

        void Update () {
            if (targetHeight > 0 && isPlaying) {
                transform.localScale = new Vector3(
                    Mathf.Lerp(transform.localScale.x, targetWidth,  Time.deltaTime * animationSpeed),
                    Mathf.Lerp(transform.localScale.y, targetHeight, Time.deltaTime * animationSpeed),
                    Mathf.Lerp(transform.localScale.z, targetDepth,  Time.deltaTime * animationSpeed)
                );

                float rotX = Mathf.Cos(Time.time) * 5f;
                float rotY = Mathf.Sin(Time.time) * 5f + 180;

                transform.eulerAngles = new Vector3(
                    Mathf.LerpAngle(transform.rotation.eulerAngles.x, rotX, Time.deltaTime * animationSpeed),
                    Mathf.LerpAngle(transform.rotation.eulerAngles.y, rotY, Time.deltaTime * animationSpeed),
                    0
                );
            }
            else
            {
                transform.localScale = new Vector3(
                    Mathf.Lerp(transform.localScale.x, 1, Time.deltaTime * animationSpeed),
                    Mathf.Lerp(transform.localScale.y, 1, Time.deltaTime * animationSpeed),
                    Mathf.Lerp(transform.localScale.z, 1, Time.deltaTime * animationSpeed)
                );

                transform.Rotate(new Vector3(5f, 3f, 2f));
            }

        }
    }
}