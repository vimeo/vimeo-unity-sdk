using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vimeo.Player;

namespace Vimeo.Utils
{
    [RequireComponent(typeof(VimeoPlayer))]
    public class AutoResize : MonoBehaviour
    {

        private VimeoPlayer vimeoPlayer;
        private bool isLoaded = false;
        private float targetHeight;

        public Vector3 heightAxis = new Vector3(0f, 1f, 0f);
        private float aspectRatio;

        void Start()
        {
            vimeoPlayer = GetComponent<VimeoPlayer>();
            vimeoPlayer.OnVideoStart += OnVideoStart;
        }

        void OnDisable()
        {
            vimeoPlayer.OnVideoStart -= OnVideoStart;
        }

        private void OnVideoStart()
        {
            if (vimeoPlayer.GetWidth() > vimeoPlayer.GetHeight()) {
                aspectRatio = (float)vimeoPlayer.GetHeight() / vimeoPlayer.GetWidth();
            } else {
                aspectRatio = (float)vimeoPlayer.GetWidth() / vimeoPlayer.GetHeight();
            }

            targetHeight = aspectRatio * vimeoPlayer.videoScreen.transform.localScale.x;

            isLoaded = true;
        }

        void Update()
        {
            if (targetHeight > 0 && isLoaded) {
                if (vimeoPlayer.videoScreen.GetComponent<RawImage>() == null) {
                    Vector3 scale = vimeoPlayer.videoScreen.transform.localScale;

                    vimeoPlayer.videoScreen.transform.localScale = new Vector3(
                        heightAxis.x == 1 ? targetHeight : scale.x,
                        heightAxis.y == 1 ? targetHeight : scale.y,
                        heightAxis.z == 1 ? targetHeight : scale.z
                    );
                } else {
                    RawImage img = vimeoPlayer.videoScreen.GetComponent<RawImage>();
                    float height = Screen.width * aspectRatio;
                    float offset = (Screen.height - height) / 2;

                    if (height > Screen.height) {
                        float width = Screen.height / aspectRatio;
                        offset = (Screen.width - width) / 2;

                        img.rectTransform.offsetMin = new Vector2(offset, 0);
                        img.rectTransform.offsetMax = new Vector2(-offset, 0);
                    } else {
                        img.rectTransform.offsetMin = new Vector2(0, offset);
                        img.rectTransform.offsetMax = new Vector2(0, -offset);
                    }
                }
            }
        }
    }
}