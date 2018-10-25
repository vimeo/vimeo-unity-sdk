using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Text.RegularExpressions;
using Vimeo;

namespace Vimeo.Player
{
    [AddComponentMenu("Video/Vimeo Player")]
    [HelpURL("https://github.com/vimeo/vimeo-unity-sdk")]
    public class VimeoPlayer : PlayerSettings 
    {
        public delegate void VimeoEvent();
        public event VimeoEvent OnStart;
        public event VimeoEvent OnVideoMetadataLoad;
        public event VimeoEvent OnVideoStart;
        public event VimeoEvent OnPause;
        public event VimeoEvent OnPlay;
        public event VimeoEvent OnFrameReady;
        public event VimeoEvent OnLoadError;

        public GameObject videoScreen;
        public AudioSource audioSource;

        public string videoName;
        public string videoThumbnailUrl;
        public string authorThumbnailUrl;
        public bool is3D;
        public string videoProjection;
        public string videoStereoFormat;

        private VimeoApi api;
        public VideoController controller;
        public VimeoVideo vimeoVideo;

        public bool loadingVideoMetadata = false;
        private bool playVideoAfterLoad = false;
        private bool videoControllerReady = false;

        public void Start()
        {
            Application.runInBackground = true; 

            if (api == null) {
                api = gameObject.AddComponent<VimeoApi>();
                api.token = GetVimeoToken();
                api.OnError  += ApiError;
            }

            SetupVideoController();

            if (autoPlay == true) {
                LoadAndPlayVideo();
            }

            if (OnStart != null) {
                OnStart();
            }
        }

        public override void SignIn(string _token) 
        {
            base.SignIn(_token);

            if (api != null) {
                api.token = GetVimeoToken();
            }
        }

        public void LoadVideo(string vimeo_url)
        {
            if (!String.IsNullOrEmpty(vimeo_url)) {
                vimeoVideo = null;
                Match match = Regex.Match(vimeo_url, "(vimeo.com)?(/channels/[^/]+)?/?([0-9]+)"); 

                if (match.Success) {
                    vimeoVideoId = match.Groups[3].Value;
                    LoadVideo(int.Parse(vimeoVideoId));
                }
                else {
                    Debug.LogError("[Vimeo] Invalid Vimeo URL");
                }
            }
        }

        public void LoadVideo(int vimeo_id)
        {
            loadingVideoMetadata = true;
            videoControllerReady = false;

            SetupVideoController();

            if (videoScreen == null && videoPlayerType == VideoPlayerType.UnityPlayer) {
                Debug.LogWarning("[Vimeo] No video screen was specified.");
            }

            api.OnRequestComplete += VideoMetadataLoad;
            api.GetVideoFileUrlByVimeoId(vimeo_id);
        }

        private void SetupVideoController()
        {
            // TODO abstract this out into a VideoPlayerManager (like EncoderManager.cs)
            if (videoPlayerType == VideoPlayerType.UnityPlayer) {
                if (controller == null) {
                    controller = gameObject.AddComponent<VideoController>();
                    controller.playerSettings = this;
                    controller.videoScreenObject = videoScreen;

                    controller.OnVideoStart += VideoStarted;
                    controller.OnPlay       += VideoPlay;
                    controller.OnPause      += VideoPaused;
                    controller.OnFrameReady += VideoFrameReady;
                            
                    if (audioSource && audioSource is AudioSource) {
                        if (audioSource != null) {
                            controller.audioSource = audioSource;
                        }
                        else {
                            videoScreen.gameObject.AddComponent<AudioSource>();
                        }
                    }
                }
                else if (videoPlayerType == VideoPlayerType.UnityPlayer) {
                    controller.videoScreenObject = videoScreen;
                    controller.Setup();
                }
            }
#if VIMEO_AVPRO_VIDEO_SUPPORT  
            else if (videoPlayerType == VideoPlayerType.AVProVideo && mediaPlayer == null) {
                Debug.LogWarning("[Vimeo] AVPro MediaPlayer has not been assigned.");
            }
#endif
        }
        
        public void LoadVideo()
        {
            if (!vimeoSignIn) {
                Debug.LogError("[Vimeo] You are not signed in.");
            }
            else if (String.IsNullOrEmpty(vimeoVideoId)) {
                Debug.LogError("[Vimeo] Can't load video. No video was specificed.");
            }
            else {
                LoadVideo(vimeoVideoId);
            }
        }


        public bool IsPlaying()
        {
            if (IsPlayerSetup()) { 
                return controller.videoPlayer.isPlaying;
            }
            else {
                return false;
            }
        }

        public bool IsVideoMetadataLoaded()
        {
            return vimeoVideo != null && vimeoVideo.uri != null && vimeoVideo.uri != "";
        }

        public bool IsPlayerSetup()
        {
            return controller != null && controller.videoPlayer != null;
        }

        public void LoadAndPlayVideo()
        {
            playVideoAfterLoad = true;

            if (!IsVideoMetadataLoaded()) {
                LoadVideo();
            }
        }

        public void PlayVideo(string _vimeoUrl) 
        {
            vimeoVideoId = _vimeoUrl;
            LoadAndPlayVideo();
        }

        public void PlayVideo(int _vimeoId)
        {
            vimeoVideoId = _vimeoId.ToString();
            LoadAndPlayVideo();
        }

        public void Play()
        {
            if (!IsVideoMetadataLoaded()) {
                LoadAndPlayVideo();
            }
            else if (!videoControllerReady) {
                VideoControllerPlayVideo();
            }
            else {
                controller.Play();
            }
        }

        private void VideoControllerPlayVideo()
        {
            videoControllerReady = true;

            if (videoPlayerType == VideoPlayerType.UnityPlayer) {
                controller.PlayVideo(vimeoVideo, selectedResolution);
            }
            else {
#if VIMEO_AVPRO_VIDEO_SUPPORT                
                string file_url = null;

                if (this.selectedResolution == StreamingResolution.Adaptive) {
                    file_url = vimeoVideo.GetAdaptiveVideoFileURL();
                }
                else {
                    file_url = vimeoVideo.GetVideoFileUrlByResolution(selectedResolution);
                }
                
                if (videoPlayerType == VideoPlayerType.AVProVideo && mediaPlayer != null) {
                    mediaPlayer.OpenVideoFromFile(RenderHeads.Media.AVProVideo.MediaPlayer.FileLocation.AbsolutePathOrURL, file_url, autoPlay || playVideoAfterLoad);
                }
#if VIMEO_DEPTHKIT_SUPPORT
                // TODO
                else if (videoPlayerType == VideoPlayerType.DepthKit && depthKitClip != null) {
                    // depthKitClip._moviePath = file_url;
                    // depthKitClip._metaDataText = vimeoVideo.description;
                    // depthKitClip.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().OpenVideoFromFile(RenderHeads.Media.AVProVideo.MediaPlayer.FileLocation.AbsolutePathOrURL, file_url, autoPlay);
                    // depthKitClip.RefreshRenderer();
                }
    #endif  
#endif
            }
        }

        public void Pause()
        {
            controller.Pause();
        }

        public void Seek(float seek)
        {
            controller.Seek(seek);
        }

        public void SeekBySeconds(int seconds)
        {
            controller.SeekBySeconds(seconds);
        }

        public void SeekBackward(float seek)
        {
            controller.SeekBackward(seek);
        }

        public void SeekForward(float seek)
        {
            controller.SeekForward(seek);
        }

        public void ToggleVideoPlayback()
        {
            controller.TogglePlayback();
        }

        public int GetWidth()
        {
            return controller.width;
        }

        public int GetHeight()
        {
            return controller.height;
        }

        public float GetProgress()
        {
            if (controller != null && controller.videoPlayer != null) {
                return (float)controller.GetCurrentFrame() / (float)controller.GetTotalFrames();
            }
            return 0;
        }

        public string GetTimecode()
        {
            if (controller != null) {
                float sec = Mathf.Floor ((float)controller.videoPlayer.time % 60);
                float min = Mathf.Floor ((float)controller.videoPlayer.time / 60f);

                string secZeroPad = sec > 9 ? "" : "0";
                string minZeroPad = min > 9 ? "" : "0";

                return minZeroPad + min + ":" + secZeroPad + sec;
            }

            return null;
        }

        // Events below
        private void VideoStarted(VideoController controller) {
            if (startTime > 0) {
                controller.SeekBySeconds(startTime);
            }

            if (OnVideoStart != null) {
                OnVideoStart();
            }
        }

        private void VideoPlay(VideoController controller)
        {
            if (OnPlay != null) {
                OnPlay();
            }
        }

        private void VideoPaused(VideoController controller)
        {
            if (OnPause != null) {
                OnPause();
            }
        }

        private void VideoFrameReady(VideoController controller)
        {
            if (OnFrameReady != null) {
                OnFrameReady();
            }
        }

        private void VideoMetadataLoad(string response)
        {
            loadingVideoMetadata = false;
            
            JSONNode json = JSON.Parse(response);
            api.OnRequestComplete -= VideoMetadataLoad;
            
            if (json["error"] == null) {
                if (json["user"] != null && json["user"]["account"].Value == "basic") {
                    Debug.LogError("[VimeoPlayer] You do not have permission to stream videos. You must be a Vimeo Pro or Business customer. https://vimeo.com/upgrade");
                }

                if ((json["play"] == null || json["play"]["progressive"] == null) && json["files"] == null) {
                    Debug.LogError("[VimeoPlayer] You do not have permission to access to this video. You must be a Vimeo Pro or Business customer and use videos from your own account. https://vimeo.com/upgrade");
                }
                
                vimeoVideo = new VimeoVideo(json);

                if (autoPlay || playVideoAfterLoad) {
                    Play();
                    playVideoAfterLoad = false;
                }

                if (OnVideoMetadataLoad != null) {
                    OnVideoMetadataLoad();
                }
            } 
            else {
                Debug.LogError("Video could not be found");
            }
        }
        
        private void ApiError(string response)
        {
            if (OnLoadError != null) {
                OnLoadError();
            }
        }
    }
}
