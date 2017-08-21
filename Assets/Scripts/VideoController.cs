using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEngine.Video;namespace Vimeo{    public class VideoController : MonoBehaviour {

        public delegate void PlaybackAction(VideoController controller);        public event PlaybackAction OnVideoStart;
        public event PlaybackAction OnPause;
        public event PlaybackAction OnPlay;

        public GameObject videoScreenObject;
        public int width;        public int height;

        public  VideoPlayer videoPlayer;        public  AudioSource audioSource;

        private void Setup()
        {
            if (videoPlayer == null) {
                videoPlayer = videoScreenObject.AddComponent<VideoPlayer>();

                if (audioSource == null) {
                    audioSource = videoScreenObject.AddComponent<AudioSource>();
                }

                audioSource.volume = 1;

                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;                videoPlayer.source = VideoSource.Url;                videoPlayer.SetTargetAudioSource(0, audioSource);                videoPlayer.renderMode = VideoRenderMode.MaterialOverride;                videoPlayer.prepareCompleted += VideoPlayerStarted;
                videoPlayer.isLooping = true;
            }
            else {
                Pause();
                videoPlayer.Stop();
            }
        }
        public void PlayVideoByUrl(string file_url) 		{
            Setup();
            videoPlayer.url = file_url;            videoPlayer.Play();
            //StartCoroutine("PlayVideo");
        }		public void SeekBackward(float amount)		{		}		public void SeekForward(float amount)		{			videoPlayer.frame = (long) (videoPlayer.frame + amount);		}		public void Seek(float seek)		{			videoPlayer.frame = (long) (Mathf.Clamp01(seek) * videoPlayer.frameCount);		}

        IEnumerator PlayVideo()
        {
            videoPlayer.Play();
            yield return null;
        }

        public void TogglePlayback()
        {
            if (videoPlayer.isPlaying){
                Pause();
            }
            else {
                Play();
            }
        }

        public void Pause()
        {
            videoPlayer.Pause();
            if (OnPause != null) OnPause(this);
        }

        public void Play()
        {
            videoPlayer.Play();
            if (OnPlay != null) OnPlay(this);
        }

        private void VideoPlayerStarted(VideoPlayer source)
        {
            if (OnVideoStart != null) {
                this.width  = videoPlayer.texture.width;
                this.height = videoPlayer.texture.height;				StartCoroutine("WaitForRenderTexture");
            }
        }
		IEnumerator WaitForRenderTexture() {			yield return new WaitUntil (() => videoPlayer.texture != null);			OnVideoStart(this);		}
        private void OnDisable()
        {
            videoPlayer.prepareCompleted -= VideoPlayerStarted;
        }

        //private void EndReached(VideoPlayer vp)
        //{
        //    vp.playbackSpeed = vp.playbackSpeed / 10.0F;
        //}    }}