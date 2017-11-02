using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Vimeo.Controls {
    public class CanvasControls : MonoBehaviour {

        public VimeoPlayer vimeoPlayer;
        public GameObject playButtonText;
        public Slider seekBar;

        private bool seeking;

    	void Start () {
            vimeoPlayer.OnPlay  += VideoPlay;
            vimeoPlayer.OnPause += VideoPause;

            // TODO: Implement seek controls
    	}

        private void VideoPlay()
        {
            Text txt = playButtonText.GetComponent<Text> ();
            txt.text = "Pause";
        }

        private void VideoPause()
        {
            Text txt = playButtonText.GetComponent<Text> ();
            txt.text = "Play";
        }

        void Update()
        {
            if (vimeoPlayer != null && !seeking) {
                
                seekBar.normalizedValue = vimeoPlayer.GetProgress();
            }
        }
    }
}