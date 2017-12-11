using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vimeo.Controls {
    public class PlayerGUIControls : MonoBehaviour {

        public VimeoPlayer vimeoPlayer;
        public GameObject playButtonText;
        public Slider seekBar;

        private bool seeking = false;

    	void Start () {
            vimeoPlayer.OnPlay  += VideoPlay;
            vimeoPlayer.OnPause += VideoPause;

            // TODO: Implement seek controls
            // Hook into the Unity videoplayer object 
            // https://docs.unity3d.com/ScriptReference/Video.VideoPlayer.html
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