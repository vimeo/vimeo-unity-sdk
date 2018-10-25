using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Vimeo.Player
{
    public class VideoController : MonoBehaviour {

        public delegate void PlaybackAction(VideoController controller);
        public event PlaybackAction OnVideoStart;
        public event PlaybackAction OnPause;
        public event PlaybackAction OnPlay;
        public event PlaybackAction OnFrameReady;

        public GameObject videoScreenObject;
        public int width;
        public int height;

        public VimeoPlayer playerSettings;
        public VideoPlayer videoPlayer;
        public AudioSource audioSource;
        private RenderTexture videoRT;

        private bool is3D;
        private string stereoFormat;
        private MaterialPropertyBlock block;

        private VimeoVideo vimeoVideo; 

        public bool isSeeking = false;
        public long seekFrame = 0;
        private long prevFrameIndex = 0;
        private bool frameStepping = false;
        private void Start() 
        {
            this.hideFlags = HideFlags.HideInInspector;
        }
        public void Setup()
        {  
            if (videoScreenObject != null) {
                if (videoPlayer == null) {
                    videoPlayer = videoScreenObject.AddComponent<VideoPlayer>();
                    
                    if (audioSource == null) {
                        audioSource = videoScreenObject.AddComponent<AudioSource>();
                        audioSource.volume = playerSettings.muteAudio ? 0 : 1;
                    }

                    videoPlayer.playOnAwake = false;
                    videoPlayer.source = VideoSource.Url;
                    videoPlayer.skipOnDrop = true;

                    videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                    videoPlayer.SetTargetAudioSource(0, audioSource);
                    videoPlayer.controlledAudioTrackCount = 1;

                    if (videoScreenObject.GetComponent<RawImage>() != null) {
                        SetupRenderTexture();
                    }
                    else {
                        SetupMaterialOverride();
                    }
                                    
                    videoPlayer.errorReceived    += VideoPlayerError;
                    videoPlayer.prepareCompleted += VideoPlayerStarted;
                    videoPlayer.seekCompleted    += VideoSeekCompleted;
                    videoPlayer.frameReady       += VideoFrameReady;

                    videoPlayer.isLooping = true;

                    block = new MaterialPropertyBlock();
                }
                else {
                    Pause();
                    videoPlayer.Stop();
                }
            }
        }

        public void SendFrameReadyEvents()
        {
            videoPlayer.sendFrameReadyEvents = true;
        }

        public void EnableFrameStepping()
        {
            frameStepping = true;
        }

        private void SetupRenderTexture()
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        }

        private void SetupMaterialOverride()
        {
            videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            if (videoScreenObject.GetComponent<Renderer>().sharedMaterial.name.StartsWith("360VideoScreen")) {
                videoPlayer.targetMaterialProperty = "_Tex";
            } 
            else {
                videoPlayer.targetMaterialProperty = "_MainTex";
            }
        }

        public void PlayVideo(VimeoVideo _vimeoVideo, StreamingResolution resolution) 
        {
            vimeoVideo = _vimeoVideo;
            PlayVideoByUrl(vimeoVideo.GetVideoFileUrlByResolution(resolution), vimeoVideo.is3D, vimeoVideo.stereoFormat);
        }

        public void PlayVideoByUrl(string file_url, bool is3D, string stereoFormat) 
        {
            Setup();
            this.stereoFormat = stereoFormat;
            this.is3D = is3D;

            videoPlayer.url = file_url;
            videoPlayer.Prepare();
        }

        public void SeekBackward(float amount)
        {
            videoPlayer.frame = (long) (videoPlayer.frame - amount);
        }

        public void SeekForward(float amount)
        {
            videoPlayer.frame = (long) (videoPlayer.frame + amount);
        }

        public void Seek(float seek)
        {
            isSeeking = true;
            seekFrame = videoPlayer.frame = (long) (Mathf.Clamp01(seek) * videoPlayer.frameCount);
        }

        public void SeekBySeconds(int seconds)
        {
            Seek((float)seconds / (float)GetDuration());
        }

        public int GetDuration()
        {
            return (int)(videoPlayer.frameCount / videoPlayer.frameRate);   
        }

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

        public long GetCurrentFrame()
        {
            if (isSeeking) {
                return seekFrame;
            }
            
            return videoPlayer.frame; 
        }

        public ulong GetTotalFrames()
        {
            return videoPlayer.frameCount; 
        }

        private void VideoPlayerError(VideoPlayer source, string message)
        {
            Debug.Log("VideoPlayerError: " + message);
        }

        void Update()
        {
            if (videoPlayer != null && videoPlayer.sendFrameReadyEvents) {
                if (videoPlayer.canStep && frameStepping && (videoPlayer.frame == prevFrameIndex+1 || videoPlayer.frame == prevFrameIndex)) {
                    videoPlayer.StepForward();
                }
            }
        }

        private void VideoFrameReady(VideoPlayer source, long frameIdx)
        {
            prevFrameIndex = frameIdx;

            if (OnFrameReady != null) { 
                OnFrameReady(this);
            }
        }

        private void VideoPlayerStarted(VideoPlayer source)
        {
            if (frameStepping) {
                source.Pause();
            }
            else {
                source.Play();
            }

            width  = videoPlayer.texture.width;
            height = videoPlayer.texture.height;

            if (videoPlayer.renderMode == VideoRenderMode.RenderTexture) {
                videoRT = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
                videoRT.Create();
                videoPlayer.targetTexture = videoRT;
                RawImage img = videoScreenObject.GetComponent<RawImage>();
                img.texture = videoRT;
            }

            StartCoroutine("WaitForRenderTexture");
        }

        private void VideoSeekCompleted(VideoPlayer source)
        {
            isSeeking = false;
        }

        IEnumerator WaitForRenderTexture() 
        {
            yield return new WaitUntil (() => videoPlayer.isPrepared);

            var rend = videoScreenObject.GetComponent<MeshRenderer>();

            if (rend != null) { // If we are rendering onto a mesh
                if (is3D && stereoFormat == "mono") {
                    block.SetFloat("_Layout", 0f);
                    rend.SetPropertyBlock(block);
                }
                else if (is3D && stereoFormat == "top-bottom") {
                    block.SetFloat("_Layout", 2f);
                    rend.SetPropertyBlock(block);
                }
                else if (is3D && stereoFormat == "left-right") {
                    block.SetFloat("_Layout", 2f);
                    rend.SetPropertyBlock(block);
                }
            }

            if (OnVideoStart != null) OnVideoStart(this);
        }

        private void OnDisable()
        {
            if (videoPlayer != null) {
                videoPlayer.Stop();
                videoPlayer.prepareCompleted -= VideoPlayerStarted;
                videoPlayer.errorReceived    -= VideoPlayerError;
                videoPlayer.seekCompleted    -= VideoSeekCompleted;

                if (videoRT != null) {
                    videoRT.Release();
                }

                Destroy(videoPlayer);
                Destroy(audioSource);
            }
        }
    }

}